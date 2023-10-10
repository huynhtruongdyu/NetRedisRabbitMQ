using EventBus.Bus;
using EventBus.Events;
using EventBus.Subscriptions;

using EventBusRabbitMQ.Connections;

using Microsoft.Extensions.Logging;

using Polly;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

using System;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace EventBusRabbitMQ.Bus
{
    /// <summary>
    /// References:
    /// https://github.com/dotnet-architecture/eShopOnContainers
    /// https://github.com/evgomes/net-core-event-bus
    /// </summary>
    public class RabbitMQEventBus : IEventBus
    {
        #region fields

        private readonly ILogger<RabbitMQEventBus> _logger;
        private readonly IPersistentConnection _persistentConnection;
        private readonly IEventSubscriptionManager _subscriptionsManager;
        private readonly IServiceProvider _serviceProvider;

        private IModel _consumerChannel;
        private readonly int _publishRetryCount = 5;
        private readonly string _queueName;
        private readonly string _exchangeName;
        private readonly TimeSpan _subscribeRetryTime = TimeSpan.FromSeconds(5);

        #endregion fields

        #region ctor

        public RabbitMQEventBus(
            ILogger<RabbitMQEventBus> logger,
            IPersistentConnection persistentConnection,
            IEventSubscriptionManager subscriptionsManager,
            IServiceProvider serviceProvider,
            string brokerName,
            string queueName)
        {
            _logger = logger;
            _persistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
            _subscriptionsManager = subscriptionsManager ?? throw new ArgumentNullException(nameof(subscriptionsManager));
            _serviceProvider = serviceProvider;
            _exchangeName = brokerName ?? throw new ArgumentNullException(nameof(brokerName));
            _queueName = queueName ?? throw new ArgumentNullException(nameof(queueName));

            ConfigureMessageBroker();
        }

        #endregion ctor

        #region public methods

        public void Publish<TEvent>(TEvent @event)
            where TEvent : IntegrationEvent
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            var policy = Policy
                .Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(_publishRetryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (exception, timeSpan) =>
                {
                    _logger.LogWarning(exception, "Could not publish event #{EventId} after {Timeout} seconds: {ExceptionMessage}.", @event.Id, $"{timeSpan.TotalSeconds:n1}", exception.Message);
                });

            var eventName = @event.GetType().Name;

            _logger.LogTrace("Creating RabbitMQ channel to publish event #{EventId} ({EventName})...", @event.Id, eventName);

            using (var channel = _persistentConnection.CreateModel())
            {
                _logger.LogTrace("Declaring RabbitMQ exchange to publish event #{EventId}...", @event.Id);

                channel.ExchangeDeclare(exchange: _exchangeName, type: "direct");

                // Set the prefetch count
                //channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                var message = JsonSerializer.Serialize<TEvent>(@event);
                var body = Encoding.UTF8.GetBytes(message);

                policy.Execute(() =>
                {
                    var properties = channel.CreateBasicProperties();
                    properties.DeliveryMode = (byte)DeliveryMode.Persistent;

                    _logger.LogTrace("Publishing event to RabbitMQ with ID #{EventId}...", @event.Id);

                    channel.BasicPublish(
                        exchange: _exchangeName,
                        routingKey: eventName,
                        mandatory: true,
                        basicProperties: properties,
                        body: body);

                    _logger.LogTrace("Published event with ID #{EventId}.", @event.Id);
                });
            }
        }

        public void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = _subscriptionsManager.GetEventIdentifier<T>();
            var eventHandlerName = typeof(TH).Name;

            AddQueueBindForEventSubscription(eventName);

            _logger.LogInformation("Subscribing to event {EventName} with {EventHandler}...", eventName, eventHandlerName);

            _subscriptionsManager.AddSubscription<T, TH>();
            StartBasicConsume();

            _logger.LogInformation("Subscribed to event {EventName} with {EvenHandler}.", eventName, eventHandlerName);
        }

        public void Unsubscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = _subscriptionsManager.GetEventIdentifier<T>();

            _logger.LogInformation("Unsubscribing from event {EventName}...", eventName);

            _subscriptionsManager.RemoveSubscription<T, TH>();

            _logger.LogInformation("Unsubscribed from event {EventName}.", eventName);
        }

        #endregion public methods

        #region private methods

        private void ConfigureMessageBroker()
        {
            _consumerChannel = CreateConsumerChannel();
            _subscriptionsManager.OnEventRemoved += SubscriptionManager_OnEventRemoved;
            _persistentConnection.OnReconnectedAfterConnectionFailure += PersistentConnection_OnReconnectedAfterConnectionFailure;
        }

        private IModel CreateConsumerChannel()
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            _logger.LogTrace("Creating RabbitMQ consumer channel...");

            var channel = _persistentConnection.CreateModel();

            channel.ExchangeDeclare(exchange: _exchangeName, type: "direct");
            channel.QueueDeclare
            (
                queue: _queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            channel.CallbackException += (sender, ea) =>
            {
                _logger.LogWarning(ea.Exception, "Recreating RabbitMQ consumer channel...");
                DoCreateConsumerChannel();
            };

            _logger.LogTrace("Created RabbitMQ consumer channel.");

            return channel;
        }

        private void DoCreateConsumerChannel()
        {
            _consumerChannel.Dispose();
            _consumerChannel = CreateConsumerChannel();
            StartBasicConsume();
        }

        private void StartBasicConsume()
        {
            _logger.LogTrace("Starting RabbitMQ basic consume...");

            if (_consumerChannel == null)
            {
                _logger.LogError("Could not start basic consume because consumer channel is null.");
                return;
            }

            var consumer = new AsyncEventingBasicConsumer(_consumerChannel);
            consumer.Received += Consumer_Received;

            _consumerChannel.BasicConsume
            (
                queue: _queueName,
                autoAck: false,
                consumer: consumer
            );

            _logger.LogTrace("Started RabbitMQ basic consume.");
        }

        private async Task ProcessEvent(string eventName, string message)
        {
            _logger.LogTrace("Processing RabbitMQ event: {EventName}...", eventName);

            if (!_subscriptionsManager.HasSubscriptionsForEvent(eventName))
            {
                _logger.LogTrace("There are no subscriptions for this event.");
                return;
            }

            var subscriptions = _subscriptionsManager.GetHandlersForEvent(eventName);
            foreach (var subscription in subscriptions)
            {
                var handler = _serviceProvider.GetService(subscription.HandlerType);
                if (handler == null)
                {
                    _logger.LogWarning("There are no handlers for the following event: {EventName}", eventName);
                    continue;
                }

                var eventType = _subscriptionsManager.GetEventTypeByName(eventName);

                var @event = JsonSerializer.Deserialize(message, eventType);
                var eventHandlerType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                await Task.Yield();
                await (Task)eventHandlerType.GetMethod(nameof(IIntegrationEventHandler<IntegrationEvent>.HandleAsync)).Invoke(handler, new object[] { @event });
            }

            _logger.LogTrace("Processed event {EventName}.", eventName);
        }

        private async Task TryEnqueueMessageAgainAsync(BasicDeliverEventArgs eventArgs)
        {
            try
            {
                _logger.LogWarning("Adding message to queue again with {Time} seconds delay...", $"{_subscribeRetryTime.TotalSeconds:n1}");

                await Task.Delay(_subscribeRetryTime);
                _consumerChannel.BasicNack(eventArgs.DeliveryTag, false, true);

                _logger.LogTrace("Message added to queue again.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Could not enqueue message again: {Error}.", ex.Message);
            }
        }

        private void RecreateSubscriptions()
        {
            var subscriptions = _subscriptionsManager.GetAllSubscriptions();
            _subscriptionsManager.Clear();

            Type eventBusType = this.GetType();
            MethodInfo genericSubscribe;

            foreach (var entry in subscriptions)
            {
                foreach (var subscription in entry.Value)
                {
                    genericSubscribe = eventBusType.GetMethod("Subscribe").MakeGenericMethod(subscription.EventType, subscription.HandlerType);
                    genericSubscribe.Invoke(this, null);
                }
            }
        }

        private void AddQueueBindForEventSubscription(string eventName)
        {
            var containsKey = _subscriptionsManager.HasSubscriptionsForEvent(eventName);
            if (containsKey)
            {
                return;
            }

            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            using (var channel = _persistentConnection.CreateModel())
            {
                channel.QueueBind(queue: _queueName, exchange: _exchangeName, routingKey: eventName);
            }
        }

        #endregion private methods

        #region event handlers

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs eventArgs)
        {
            var eventName = eventArgs.RoutingKey;
            var message = Encoding.UTF8.GetString(eventArgs.Body.Span);

            bool isAcknowledged = false;

            try
            {
                await ProcessEvent(eventName, message);

                _consumerChannel.BasicAck(eventArgs.DeliveryTag, multiple: false);
                isAcknowledged = true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error processing the following message: {Message}.", message);
            }
            finally
            {
                if (!isAcknowledged)
                {
                    await TryEnqueueMessageAgainAsync(eventArgs);
                }
            }
        }

        private void SubscriptionManager_OnEventRemoved(object sender, string eventName)
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            using (var channel = _persistentConnection.CreateModel())
            {
                channel.QueueUnbind(queue: _queueName, exchange: _exchangeName, routingKey: eventName);

                if (_subscriptionsManager.IsEmpty)
                {
                    _consumerChannel.Close();
                }
            }
        }

        private void PersistentConnection_OnReconnectedAfterConnectionFailure(object sender, EventArgs e)
        {
            DoCreateConsumerChannel();
            RecreateSubscriptions();
        }

        #endregion event handlers
    }
}