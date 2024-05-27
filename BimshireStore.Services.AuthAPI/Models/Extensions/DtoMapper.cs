using BimshireStore.Services.AuthAPI.Models.Dto;

namespace BimshireStore.Services.AuthAPI.Models.Extensions
{
    public static class DtoMapper
    {
        // NB this is an "extension method" for model
        // the "this" keyword allows this to appear as a member method
        // of the model. It allows us to call it like myModel.ToDto
        // which looks much better than DomainExtension.ToDto(myModel).
        // aka it is syntactic sugar over the static method.

        public static UserDto ToDto(this ApplicationUser user)
        {
            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber
            };
        }

    }
}