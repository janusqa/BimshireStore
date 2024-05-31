using System.Text;
using System.Text.Json;
using BimshireStore.Services.AuthAPI.Services.IService;
using RabbitMQ.Client;

namespace BimshireStore.Services.AuthAPI.Services
{
    public class MessageBusSender : IMessageBusSender
    {
        private readonly IConfiguration _config;
        private readonly string _hostName;
        private readonly string _username;
        private readonly string _password;
        private IConnection _connection { get; }

        public MessageBusSender(IConfiguration config)
        {
            _config = config;
            _hostName = _config.GetValue<string>("MessageBus:host") ?? throw new InvalidOperationException("Invalid MessageBus Host");
            _username = _config.GetValue<string>("MessageBus:uid") ?? throw new InvalidOperationException("Invalid MessageBus UID");
            _password = _config.GetValue<string>("MessageBus:pid") ?? throw new InvalidOperationException("Invalid MessageBus PID");

            var factory = new ConnectionFactory
            {
                HostName = _hostName,
                UserName = _username,
                Password = _password
            };

            _connection = factory.CreateConnection();
        }

        public void SendMessage(object message, string queueName)
        {
            using var channel = _connection.CreateModel();
            channel.QueueDeclare(queueName);
            var jsonMessage = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(jsonMessage);
            channel.BasicPublish(exchange: "", routingKey: queueName, body: body);
        }
    }
}