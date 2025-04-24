using CleaningMyName.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace CleaningMyName.Domain.ValueObjects;

public record Email
{
    public string Value { get; }
    
    private Email(string value)
    {
        Value = value;
    }
    
    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainValidationException("Email cannot be empty.");
        }
        
        email = email.Trim();
        
        if (email.Length > 320)
        {
            throw new DomainValidationException("Email is too long.");
        }
        
        if (!IsValidEmail(email))
        {
            throw new DomainValidationException("Email is invalid.");
        }
        
        return new Email(email);
    }
    
    private static bool IsValidEmail(string email)
    {
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern);
    }
    
    public static implicit operator string(Email email) => email.Value;
    
    public override string ToString() => Value;
}
