using AutoMapper;
using DotNet.Web.Api.Template.DTOs.User;
using DotNet.Web.Api.Template.Models.Auth;

namespace DotNet.Web.Api.Template.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User Mapping
            CreateMap<ApplicationUser, UserDTO>();

        }
    }
}