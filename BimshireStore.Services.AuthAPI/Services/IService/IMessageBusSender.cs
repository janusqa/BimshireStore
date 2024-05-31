namespace BimshireStore.Services.AuthAPI.Services.IService
{
    public interface IMessageBusSender
    {
        void SendMessage(object message, string queueName);
    }
}