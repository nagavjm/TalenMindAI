using TalentMindAI.Domain.Entities;

namespace TalentMindAI.Application.Interfaces;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAt) GenerateToken(User user);
}
