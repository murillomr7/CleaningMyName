using CleaningMyName.Application.Common.Interfaces;

namespace CleaningMyName.Infrastructure.Services;

public class DateTimeService : IDateTimeService
{
    public DateTime Now => DateTime.Now;
    public DateTime UtcNow => DateTime.UtcNow;
}
