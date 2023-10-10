using RabbitMQ.Client;

namespace EventBusRabbitMQ.Connections
{
    public interface IPersistentConnection
    {
        event EventHandler OnReconnectedAfterConnectionFailure;

        bool IsConnected { get; }

        bool TryConnect();

        IModel CreateModel();
    }
}