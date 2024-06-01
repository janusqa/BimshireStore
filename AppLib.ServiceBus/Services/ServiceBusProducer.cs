using System.Text;
using System.Text.Json;
using AppLib.ServiceBus.Services.IService;
using RabbitMQ.Client;

namespace AppLib.ServiceBus.Services
{
    public class ServiceBusProducer : IServiceBusProducer, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private bool _disposed;

        public ServiceBusProducer(string hostname, string username, string password)
        {
            var factory = new ConnectionFactory
            {
                HostName = hostname,
                UserName = username,
                Password = password
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public void SendMessage(object message, string queueName)
        {
            _channel.QueueDeclare(queueName, false, false, false, null);
            var jsonMessage = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(jsonMessage);
            _channel.BasicPublish(exchange: "", routingKey: queueName, null, body: body);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _channel?.Dispose();
                    _connection?.Dispose();
                }

                _disposed = true;
            }
        }

        ~ServiceBusProducer()
        {
            Dispose(disposing: false);
        }
    }
}