
using System.Text.Json;
using AppLib.ServiceBus.Services.IService;
using BimshireStore.Services.EmailAPI.Models.Dto;
using BimshireStore.Services.EmailAPI.Services.IService;
using BimshireStore.Services.ShoppingCartAPI.Utility;

namespace BimshireStore.Services.EmailAPI.Services
{
    public class ServiceBusConsumer_OrderApprovedExchange : IHostedLifecycleService
    {
        private readonly IServiceBusConsumer _sbc;
        private readonly IConfiguration _config;
        private readonly IServiceProvider _serviceProvider;
        private readonly string _exchangeName;

        public ServiceBusConsumer_OrderApprovedExchange(IConfiguration config, IServiceProvider serviceProvider, IServiceBusConsumer sbc)
        {
            _sbc = sbc;
            _config = config;
            _serviceProvider = serviceProvider;
            _exchangeName = _config.GetValue<string>("MessageBus:TopicAndQueueNames:OrderApprovedExchange") ?? throw new InvalidOperationException("Invalid MessageBus Queue");
        }

        public Task StartingAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _sbc.InitExchange(ProcessMessage, _exchangeName);
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
                        var reward = JsonSerializer.Deserialize<RewardDto>(content, SD.JsonSerializerConfig.DefaultOptions);
                        if (reward is not null) await emailService.OrderPlacedEmailAndLog(reward);
                    }
                }
            }
        }
    }
}