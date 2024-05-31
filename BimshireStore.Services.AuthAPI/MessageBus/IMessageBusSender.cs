namespace BimshireStore.Services.AuthAPI.MessageBus
{
    public interface IMessageBusSender
    {
        void SendMessage(object message, string queueName);
    }
}