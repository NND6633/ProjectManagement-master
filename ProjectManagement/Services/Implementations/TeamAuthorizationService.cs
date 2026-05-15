using Microsoft.EntityFrameworkCore;
using ProjectManagement.Data;
using ProjectManagement.Models.Enums;
using ProjectManagement.Services.Interfaces;

namespace ProjectManagement.Services.Implementations
{
    public class TeamAuthorizationService : ITeamAuthorizationService
    {
        private readonly AppDbContext _context;

        public TeamAuthorizationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsLeaderAsync(Guid teamId, string userId)
        {
            return await _context.TeamMembers.AnyAsync(x =>
                x.TeamId == teamId &&
                x.UserId == userId &&
                x.Role == RoleInTeam.Leader);
        }

        public async Task<bool> IsPMAsync(Guid teamId, string userId)
        {
            return await _context.TeamMembers.AnyAsync(x =>
                x.TeamId == teamId &&
                x.UserId == userId &&
                x.Role == RoleInTeam.PM);
        }

        public async Task<bool> IsLeaderOrPMAsync(Guid teamId, string userId)
        {
            return await _context.TeamMembers.AnyAsync(x =>
                x.TeamId == teamId &&
                x.UserId == userId &&
                (x.Role == RoleInTeam.Leader || x.Role == RoleInTeam.PM));
        }

        public async Task<bool> IsMemberAsync(Guid teamId, string userId)
        {
            return await _context.TeamMembers.AnyAsync(x =>
                x.TeamId == teamId &&
                x.UserId == userId);
        }
    }
}