using System.Text;
using System.Text.Json;
using AppLib.ServiceBus.Services.IService;
using RabbitMQ.Client;

namespace AppLib.ServiceBus.Services
{
    public class ServiceBusProducer : IServiceBusProducer
    {
        // private readonly IConfiguration _config;
        // private readonly string _hostName;
        // private readonly string _username;
        // private readonly string _password; 
        private IConnection _connection { get; }

        public ServiceBusProducer(string hostname, string username, string password)
        {
            // _config = config;
            // _hostName = _config.GetValue<string>("MessageBus:host") ?? throw new InvalidOperationException("Invalid MessageBus Host");
            // _username = _config.GetValue<string>("MessageBus:uid") ?? throw new InvalidOperationException("Invalid MessageBus UID");
            // _password = _config.GetValue<string>("MessageBus:pid") ?? throw new InvalidOperationException("Invalid MessageBus PID");

            var factory = new ConnectionFactory
            {
                HostName = hostname,
                UserName = username,
                Password = password
            };

            _connection = factory.CreateConnection();
        }

        public void SendMessage(object message, string queueName)
        {
            using var channel = _connection.CreateModel();
            channel.QueueDeclare(queueName, false, false, false, null);
            var jsonMessage = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(jsonMessage);
            channel.BasicPublish(exchange: "", routingKey: queueName, null, body: body);
        }
    }
}