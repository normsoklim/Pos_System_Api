using PosSystem.Domain.Entities;

namespace PosSystem.Application.Interfaces.Services
{
    public interface ISecurityAuditService
    {
        Task LogLoginAttempt(string email, bool success, string ipAddress, string userAgent);
        Task LogSuccessfulLogin(User user, string ipAddress, string userAgent);
        Task LogLogout(int userId, string ipAddress);
        Task LogFailedAuthentication(string email, string ipAddress, string reason);
        Task LogPasswordChange(int userId, string ipAddress);
        Task LogUnauthorizedAccess(string endpoint, int? userId, string ipAddress);
    }
}