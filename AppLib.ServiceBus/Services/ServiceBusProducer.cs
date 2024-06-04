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
        private (string DLX, string DLQ) _deadLetter;
        private readonly Dictionary<string, object> _queueArgs;
        private bool _disposed;

        public ServiceBusProducer(string hostname, string username, string password, string exchangeDeadLetter, string queueDeadLetter)
        {
            var factory = new ConnectionFactory
            {
                HostName = hostname,
                UserName = username,
                Password = password
            };

            _deadLetter = (exchangeDeadLetter, queueDeadLetter);
            _queueArgs = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", _deadLetter.DLX },
                { "x-queue-type", "quorum" },
                { "x-delivery-limit", 3 }
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Dead letter exchange to collect failed messages for later manual examination
            // we could set up a consumer for this if we want.
            // ***
            _channel.ExchangeDeclare(exchange: _deadLetter.DLX, type: ExchangeType.Fanout, durable: false);
            _channel.QueueDeclare(queue: _deadLetter.DLQ, durable: false, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueBind(queue: _deadLetter.DLQ, exchange: _deadLetter.DLX, routingKey: "");
        }

        public void SendMessageToQueue(object message, string queueName)
        {
            try
            {
                _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: _queueArgs);

                var jsonMessage = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(jsonMessage);

                _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message to ServiceBus: {ex.Message}");
            }
        }

        public void SendMessageToExchange(object message, string exchangeName)
        {
            try
            {
                _channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Fanout, durable: false);
                var jsonMessage = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(jsonMessage);
                _channel.BasicPublish(exchange: exchangeName, routingKey: "", basicProperties: null, body: body);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message to ServiceBus: {ex.Message}");
            }
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