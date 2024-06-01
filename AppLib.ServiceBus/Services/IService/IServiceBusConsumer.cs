namespace AppLib.ServiceBus.Services.IService
{
    public interface IServiceBusConsumer
    {
        public void Init(Func<string, Task> processMessage, string queueName);
    }
}