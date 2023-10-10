using EventBus.Events;

using System;

namespace EventBus.Subscriptions
{
    public interface IEventSubscriptionManager
    {
        #region Event Handlers

        event EventHandler<string> OnEventRemoved;

        #endregion Event Handlers

        #region Status

        bool IsEmpty { get; }

        bool HasSubscriptionsForEvent(string eventName);

        #endregion Status

        #region Events info

        string GetEventIdentifier<TEvent>();

        Type GetEventTypeByName(string eventName);

        IEnumerable<Subscription> GetHandlersForEvent(string eventName);

        Dictionary<string, List<Subscription>> GetAllSubscriptions();

        #endregion Events info

        #region Subscription management

        void AddSubscription<TEvent, TEventHandler>()
            where TEvent : IntegrationEvent
            where TEventHandler : IIntegrationEventHandler<TEvent>;

        void RemoveSubscription<TEvent, TEventHandler>()
            where TEvent : IntegrationEvent
            where TEventHandler : IIntegrationEventHandler<TEvent>;

        void Clear();

        #endregion Subscription management
    }
}