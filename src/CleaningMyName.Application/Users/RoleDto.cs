using AutoMapper;
using CleaningMyName.Application.Common.Mappings;
using CleaningMyName.Domain.Entities;

namespace CleaningMyName.Application.Users;

public class RoleDto : IMapFrom<Role>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Role, RoleDto>();
    }
}
