
using BimshireStore.Services.EmailAPI.Models.Dto;

namespace BimshireStore.Services.EmailAPI.Models.Extensions
{
    public static class DtoMapper
    {
        // NB this is an "extension method" for model
        // the "this" keyword allows this to appear as a member method
        // of the model. It allows us to call it like myModel.ToDto
        // which looks much better than DomainExtension.ToDto(myModel).
        // aka it is syntactic sugar over the static method.

        public static EmailLogDto ToDto(this EmailLog emailLog)
        {
            return new EmailLogDto
            {
                Id = emailLog.Id,
                Email = emailLog.Email,
                Message = emailLog.Message,
                EmailSent = emailLog.EmailSent
            };
        }
    }
}