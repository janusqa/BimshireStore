namespace AppLib.ServiceBus.Services.IService
{
    public interface IServiceBusConsumer
    {
        public void InitQueue(Func<string, Task> processMessage, string queueName);
        public void InitExchange(Func<string, Task> processMessage, string exchangeName);
    }
}