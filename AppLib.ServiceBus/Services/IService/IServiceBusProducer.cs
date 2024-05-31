namespace AppLib.ServiceBus.Services.IService
{
    public interface IServiceBusProducer
    {
        void SendMessage(object message, string queueName);
    }
}