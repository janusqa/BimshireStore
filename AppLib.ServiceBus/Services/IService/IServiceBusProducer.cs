namespace AppLib.ServiceBus.Services.IService
{
    public interface IServiceBusProducer
    {
        void SendMessageToQueue(object message, string queueName);
        void SendMessageToExchange(object message, string exchangeName);
    }
}