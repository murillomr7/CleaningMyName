using CleaningMyName.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CleaningMyName.Infrastructure.Persistence.ValueConverters;

public class EmailValueConverter : ValueConverter<Email, string>
{
    public EmailValueConverter() 
        : base(
            v => v.Value,
            v => Email.Create(v))
    {
    }
}
