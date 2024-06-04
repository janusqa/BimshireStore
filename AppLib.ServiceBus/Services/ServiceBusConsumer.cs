using System.Text;
using AppLib.ServiceBus.Services.IService;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AppLib.ServiceBus.Services
{
    public class ServiceBusConsumer : IServiceBusConsumer, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly Dictionary<string, EventingBasicConsumer> _consumers;
        private readonly Dictionary<string, EventHandler<BasicDeliverEventArgs>> _handlers;
        private (string DLX, string DLQ) _deadLetter;
        private readonly Dictionary<string, object> _queueArgs;
        private bool _disposed = false;
        private readonly object _lock;

        public ServiceBusConsumer(string hostname, string username, string password, string exchangeDeadLetter, string queueDeadLetter)
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
            _consumers = [];
            _handlers = [];
            _lock = new();

            // Dead letter exchange to collect failed messages for later manual examination
            // we could set up a consumer for this if we want. Only enable the below on consumers
            // ***
            _channel.ExchangeDeclare(exchange: _deadLetter.DLX, type: ExchangeType.Fanout, durable: false);
            _channel.QueueDeclare(queue: _deadLetter.DLQ, durable: false, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueBind(queue: _deadLetter.DLQ, exchange: _deadLetter.DLX, routingKey: "");
        }

        public void InitQueue(Func<string, Task> ProcessMessage, string queueName)
        {
            lock (_lock)
            {
                if (_consumers.ContainsKey(queueName)) throw new InvalidOperationException($"Queue '{queueName}' already exist.");

                _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: _queueArgs);

                var consumer = new EventingBasicConsumer(_channel);
                var handler = CreateEventHandler(ProcessMessage);

                consumer.Received += handler;

                _consumers[queueName] = consumer;
                _handlers[queueName] = handler;

                _channel.BasicConsume(queue: queueName, autoAck: false, consumer: _consumers[queueName]);
            }
        }

        public void InitExchange(Func<string, Task> ProcessMessage, string exchangeName)
        {
            lock (_lock)
            {
                if (_consumers.ContainsKey(exchangeName)) throw new InvalidOperationException($"Queue '{exchangeName}' already exist.");

                _channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Fanout, durable: false);

                var queueName = Guid.NewGuid().ToString();
                _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: _queueArgs);
                _channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: "");

                var consumer = new EventingBasicConsumer(_channel);
                var handler = CreateEventHandler(ProcessMessage);

                consumer.Received += handler;

                _consumers[exchangeName] = consumer;
                _handlers[exchangeName] = handler;

                _channel.BasicConsume(queue: queueName, autoAck: false, consumer: _consumers[exchangeName]);
            }
        }

        private EventHandler<BasicDeliverEventArgs> CreateEventHandler(Func<string, Task> ProcessMessage)
        {
            return async (ch, ea) =>
            {
                try
                {
                    var content = Encoding.UTF8.GetString(ea.Body.ToArray());
                    await ProcessMessage(content);
                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing ServiceBus message: {ex.Message}");
                    // We can re-queue or better yet send to a dead letter exchange and log
                    _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                }
            };
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
                    lock (_lock)
                    {
                        foreach (var (queueName, consumer) in _consumers)
                        {
                            if (_handlers.TryGetValue(queueName, out var handler) && consumer is not null)
                            {
                                consumer.Received -= handler;
                            }
                        }

                        _channel?.Dispose();
                        _connection?.Dispose();
                    }
                }
                _disposed = true;
            }
        }

        ~ServiceBusConsumer()
        {
            Dispose(disposing: false);
        }
    }
}