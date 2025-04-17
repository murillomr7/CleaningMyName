using AutoMapper;
using CleaningMyName.Domain.Entities;
using CleaningMyName.Application.Common.Mappings;

namespace CleaningMyName.Application.Debts;

public class DebtDto : IMapFrom<Debt>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? PaidOnUtc { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? ModifiedOnUtc { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Debt, DebtDto>()
            .ForMember(d => d.UserName, opt => opt.MapFrom(s => s.User.FullName));
    }
}
