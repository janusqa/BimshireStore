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
        private EventingBasicConsumer? _consumer;
        private EventHandler<BasicDeliverEventArgs>? ConsumerReceivedHandler { get; set; }
        private bool _disposed = false;

        public ServiceBusConsumer(string hostname, string username, string password)
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

        public void Init(Func<string, Task> ProcessMessage, string queueName)
        {
            _channel.QueueDeclare(queueName, false, false, false, null);
            _consumer = new EventingBasicConsumer(_channel);
            ConsumerReceivedHandler = ConsumerReceivedEvent(ProcessMessage);
            _consumer.Received += ConsumerReceivedHandler;
            _channel.BasicConsume(queueName, false, _consumer);
        }

        private EventHandler<BasicDeliverEventArgs> ConsumerReceivedEvent(Func<string, Task> ProcessMessage)
        {
            return async (ch, ea) =>
            {
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());

                await ProcessMessage(content);

                _channel.BasicAck(ea.DeliveryTag, false);
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
                    if (ConsumerReceivedHandler is not null && _consumer is not null)
                    {
                        if (_consumer is not null) _consumer.Received -= ConsumerReceivedHandler;
                        ConsumerReceivedHandler = null;
                    }

                    _channel?.Dispose();
                    _connection?.Dispose();
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