using BimshireStore.Services.EmailAPI.Data;
using BimshireStore.Services.EmailAPI.Models;
using BimshireStore.Services.EmailAPI.Models.Dto;
using BimshireStore.Services.EmailAPI.Services.IService;
using System.Text;

namespace Mango.Services.EmailAPI.Services
{
    public class EmailService : IEmailService
    {
        private readonly ApplicationDbContext _db;

        public EmailService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task CartEmailAndLog(CartDto cart)
        {
            StringBuilder message = new();

            message.AppendLine("<br/>Cart Email Requested");
            message.AppendLine($"<br/>Total {cart.CartHeader.CartTotal.ToString("c")}");
            message.Append("<br/>");
            if (cart.CartDetail?.Count() > 0)
            {
                message.Append("<ul>");
                foreach (var item in cart.CartDetail)
                {
                    message.Append("<li>");
                    message.Append($"{item.Product?.Name} x {item.Count}");
                    message.Append("</li>");
                }
                message.Append("</ul>");
            }
            else
            {
                message.Append("<p>No items in cart.</p>");
            }

            if (cart.CartHeader.Email is not null) await LogAndEmail(cart.CartHeader.Email, message.ToString());
        }

        public async Task OrderPlacedEmailAndLog(RewardDto reward)
        {
            string message = "New Order Placed. <br/> Order ID : " + reward.OrderId;
            await LogAndEmail("admin1@string.com", message);
        }

        public async Task RegisteredUserEmailAndLog(string email)
        {
            string message = $"User Registeration Successful. <br/> Email : {email}";
            await LogAndEmail("admin1@string.com", message);
        }

        private async Task<string> LogAndEmail(string email, string message)
        {
            try
            {
                var emailLog = new EmailLog
                {
                    Email = email,
                    EmailSent = DateTime.UtcNow,
                    Message = message
                };

                await _db.EmailLogs.AddAsync(emailLog);
                await _db.SaveChangesAsync();

                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}