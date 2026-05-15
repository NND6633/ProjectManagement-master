namespace ProjectManagement.Services.Interfaces
{
    public interface ITeamAuthorizationService
    {
        Task<bool> IsLeaderAsync(Guid teamId, string userId);
        Task<bool> IsPMAsync(Guid teamId, string userId);
        Task<bool> IsLeaderOrPMAsync(Guid teamId, string userId);
        Task<bool> IsMemberAsync(Guid teamId, string userId);
    }
}
