using System.Collections.Generic;

namespace CleaningMyName.Application.Common.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(string userId, string username, List<string> roles);
    }
}
