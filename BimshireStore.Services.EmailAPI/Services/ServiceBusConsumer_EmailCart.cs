
using System.Text.Json;
using AppLib.ServiceBus.Services.IService;
using BimshireStore.Services.EmailAPI.Models.Dto;
using BimshireStore.Services.EmailAPI.Services.IService;
using static BimshireStore.Services.ShoppingCartAPI.Utility.SD;

namespace BimshireStore.Services.EmailAPI.Services
{
    public class ServiceBusConsumer_EmailCart : IHostedLifecycleService
    {
        private readonly IServiceBusConsumer _sbc;
        private readonly IConfiguration _config;
        private readonly IServiceProvider _serviceProvider;
        private readonly string _queueName;

        public ServiceBusConsumer_EmailCart(IConfiguration config, IServiceProvider serviceProvider, IServiceBusConsumer sbc)
        {
            _sbc = sbc;
            _config = config;
            _serviceProvider = serviceProvider;
            _queueName = _config.GetValue<string>("MessageBus:TopicAndQueueNames:EmailCartQueue") ?? throw new InvalidOperationException("Invalid MessageBus Queue");
        }

        public Task StartingAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _sbc.InitQueue(ProcessMessage, _queueName);
            return Task.CompletedTask;
        }

        public Task StartedAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }

        public Task StoppingAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StoppedAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task ProcessMessage(string content)
        {
            if (content is not null)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                    if (emailService is not null)
                    {
                        var cart = JsonSerializer.Deserialize<CartDto>(content, JsonSerializerConfig.DefaultOptions);
                        if (cart is not null)
                        {
                            var result = await emailService.CartEmailAndLog(cart);
                            if (result != string.Empty) throw new Exception(result);
                        }
                    }
                }
            }
        }
    }
}