using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Data;
using ProjectManagement.DTOs.Team;
using ProjectManagement.Models.Entities;
using ProjectManagement.Models.Enums;
using ProjectManagement.Models.Identity;

namespace ProjectManagement.Controllers
{
    [ApiController]
    [Route("api/v1/teams")]
    [Authorize]
    public class TeamController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TeamController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTeam(CreateTeamDto request)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized(new
                {
                    code = "USER_NOT_FOUND",
                    message = "User not found"
                });
            }

            var existedTeam = _context.Teams.Any(t => t.Name == request.Name);

            if (existedTeam)
            {
                return BadRequest(new
                {
                    code = "TEAM_ALREADY_EXISTS",
                    message = "Team name already exists"
                });
            }

            var team = new Team
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                LeaderId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            var teamMember = new TeamMember
            {
                Id = Guid.NewGuid(),
                TeamId = team.Id,
                UserId = user.Id,
                Role = RoleInTeam.Leader
            };

            _context.Teams.Add(team);
            _context.TeamMembers.Add(teamMember);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Team created successfully",
                data = new
                {
                    team.Id,
                    team.Name,
                    team.LeaderId,
                    team.CreatedAt
                }
            });
        }

        [HttpPost("{teamId:guid}/invite")]
        public async Task<IActionResult> InviteMember(Guid teamId, InviteMemberDto request)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Unauthorized(new
                {
                    code = "USER_NOT_FOUND",
                    message = "User not found"
                });
            }

            var team = await _context.Teams.FindAsync(teamId);

            if (team == null)
            {
                return NotFound(new
                {
                    code = "TEAM_NOT_FOUND",
                    message = "Team not found"
                });
            }

            var isLeader = await _context.TeamMembers.AnyAsync(x =>
                x.TeamId == teamId &&
                x.UserId == currentUser.Id &&
                x.Role == RoleInTeam.Leader);

            if (!isLeader)
            {
                return Forbid();
            }

            var userToInvite = await _userManager.FindByEmailAsync(request.Email);

            if (userToInvite == null)
            {
                return NotFound(new
                {
                    code = "TARGET_USER_NOT_FOUND",
                    message = "User does not exist"
                });
            }

            var alreadyInTeam = await _context.TeamMembers.AnyAsync(x =>
                x.TeamId == teamId &&
                x.UserId == userToInvite.Id);

            if (alreadyInTeam)
            {
                return BadRequest(new
                {
                    code = "USER_ALREADY_IN_TEAM",
                    message = "User is already in this team"
                });
            }

            var existingPendingInvitation = await _context.TeamInvitations
                .AnyAsync(x =>
                    x.TeamId == teamId &&
                    x.InvitedUserId == userToInvite.Id &&
                    x.Status == InvitationStatus.Pending);

            if (existingPendingInvitation)
            {
                return BadRequest(new
                {
                    code = "INVITATION_ALREADY_EXISTS",
                    message = "User already has a pending invitation"
                });
            }

            // BR-09 & BR-10
            var totalMembers = await _context.TeamMembers
                .CountAsync(x => x.TeamId == teamId);

            var hasPM = await _context.TeamMembers
                .AnyAsync(x =>
                    x.TeamId == teamId &&
                    x.Role == RoleInTeam.PM);

            if (!hasPM && totalMembers >= 7)
            {
                return BadRequest(new
                {
                    code = "PM_REQUIRED",
                    message = "Team must assign at least one PM before inviting the 8th member"
                });
            }

            var invitation = new TeamInvitation
            {
                Id = Guid.NewGuid(),
                TeamId = teamId,
                InvitedUserId = userToInvite.Id,
                InvitedByUserId = currentUser.Id,
                Status = InvitationStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.TeamInvitations.Add(invitation);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Invitation sent successfully",
                data = new
                {
                    invitation.Id,
                    invitation.TeamId,
                    invitedUser = userToInvite.Email,
                    invitation.Status,
                    invitation.CreatedAt
                }
            });
        }
        [HttpPost("invitations/{invitationId:guid}/accept")]
        public async Task<IActionResult> AcceptInvitation(Guid invitationId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Unauthorized(new
                {
                    code = "USER_NOT_FOUND",
                    message = "User not found"
                });
            }

            var invitation = await _context.TeamInvitations
                .FirstOrDefaultAsync(x => x.Id == invitationId);

            if (invitation == null)
            {
                return NotFound(new
                {
                    code = "INVITATION_NOT_FOUND",
                    message = "Invitation not found"
                });
            }

            if (invitation.InvitedUserId != currentUser.Id)
            {
                return Forbid();
            }

            if (invitation.Status != InvitationStatus.Pending)
            {
                return BadRequest(new
                {
                    code = "INVITATION_ALREADY_PROCESSED",
                    message = "Invitation has already been processed"
                });
            }

            var alreadyInTeam = await _context.TeamMembers.AnyAsync(x =>
                x.TeamId == invitation.TeamId &&
                x.UserId == currentUser.Id);

            if (alreadyInTeam)
            {
                return BadRequest(new
                {
                    code = "USER_ALREADY_IN_TEAM",
                    message = "User is already in this team"
                });
            }

            var teamMember = new TeamMember
            {
                Id = Guid.NewGuid(),
                TeamId = invitation.TeamId,
                UserId = currentUser.Id,
                Role = RoleInTeam.Member
            };

            invitation.Status = InvitationStatus.Accepted;

            _context.TeamMembers.Add(teamMember);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Invitation accepted successfully",
                data = new
                {
                    invitationId,
                    invitation.TeamId,
                    currentUser.Email,
                    role = RoleInTeam.Member.ToString()
                }
            });
        }
        [HttpPost("invitations/{invitationId:guid}/reject")]
        public async Task<IActionResult> RejectInvitation(Guid invitationId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Unauthorized(new
                {
                    code = "USER_NOT_FOUND",
                    message = "User not found"
                });
            }

            var invitation = await _context.TeamInvitations
                .FirstOrDefaultAsync(x => x.Id == invitationId);

            if (invitation == null)
            {
                return NotFound(new
                {
                    code = "INVITATION_NOT_FOUND",
                    message = "Invitation not found"
                });
            }

            if (invitation.InvitedUserId != currentUser.Id)
            {
                return Forbid();
            }

            if (invitation.Status != InvitationStatus.Pending)
            {
                return BadRequest(new
                {
                    code = "INVITATION_ALREADY_PROCESSED",
                    message = "Invitation has already been processed"
                });
            }

            invitation.Status = InvitationStatus.Rejected;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Invitation rejected successfully",
                data = new
                {
                    invitationId,
                    invitation.TeamId,
                    invitation.Status
                }
            });
        }
        [HttpPut("{teamId:guid}/assign-pm")]
        public async Task<IActionResult> AssignProjectManager(Guid teamId, AssignPmDto request)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Unauthorized(new
                {
                    code = "USER_NOT_FOUND",
                    message = "User not found"
                });
            }

            var team = await _context.Teams.FindAsync(teamId);

            if (team == null)
            {
                return NotFound(new
                {
                    code = "TEAM_NOT_FOUND",
                    message = "Team not found"
                });
            }

            var isLeader = await _context.TeamMembers.AnyAsync(x =>
                x.TeamId == teamId &&
                x.UserId == currentUser.Id &&
                x.Role == RoleInTeam.Leader);

            if (!isLeader)
            {
                return Forbid();
            }

            var targetMember = await _context.TeamMembers
                .FirstOrDefaultAsync(x => x.TeamId == teamId && x.UserId == request.UserId);

            if (targetMember == null)
            {
                return NotFound(new
                {
                    code = "TEAM_MEMBER_NOT_FOUND",
                    message = "Target user is not in this team"
                });
            }

            if (targetMember.Role == RoleInTeam.Leader)
            {
                return BadRequest(new
                {
                    code = "INVALID_ROLE_CHANGE",
                    message = "Leader cannot be reassigned as PM"
                });
            }

            if (targetMember.Role == RoleInTeam.PM)
            {
                return BadRequest(new
                {
                    code = "ALREADY_PM",
                    message = "User is already a Project Manager"
                });
            }

            targetMember.Role = RoleInTeam.PM;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Project Manager assigned successfully",
                data = new
                {
                    teamId,
                    userId = targetMember.UserId,
                    role = targetMember.Role.ToString()
                }
            });
        }
        [HttpDelete("{teamId:guid}/members/{userId}")]
        public async Task<IActionResult> RemoveMember(Guid teamId, string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Unauthorized(new
                {
                    code = "USER_NOT_FOUND",
                    message = "User not found"
                });
            }

            var team = await _context.Teams.FindAsync(teamId);

            if (team == null)
            {
                return NotFound(new
                {
                    code = "TEAM_NOT_FOUND",
                    message = "Team not found"
                });
            }

            var isLeader = await _context.TeamMembers.AnyAsync(x =>
                x.TeamId == teamId &&
                x.UserId == currentUser.Id &&
                x.Role == RoleInTeam.Leader);

            if (!isLeader)
            {
                return Forbid();
            }

            var targetMember = await _context.TeamMembers
                .FirstOrDefaultAsync(x => x.TeamId == teamId && x.UserId == userId);

            if (targetMember == null)
            {
                return NotFound(new
                {
                    code = "TEAM_MEMBER_NOT_FOUND",
                    message = "Target user is not in this team"
                });
            }

            if (targetMember.Role == RoleInTeam.Leader)
            {
                return BadRequest(new
                {
                    code = "CANNOT_REMOVE_LEADER",
                    message = "Leader cannot be removed from team"
                });
            }

            // BR-11:
            // Block removing the last PM if team has 8 or more members
            var totalMembers = await _context.TeamMembers
                .CountAsync(x => x.TeamId == teamId);

            var totalPMs = await _context.TeamMembers
                .CountAsync(x =>
                    x.TeamId == teamId &&
                    x.Role == RoleInTeam.PM);

            if (targetMember.Role == RoleInTeam.PM &&
                totalMembers >= 8 &&
                totalPMs == 1)
            {
                return BadRequest(new
                {
                    code = "LAST_PM_REQUIRED",
                    message = "Cannot remove the last PM when team has 8 or more members"
                });
            }

            // Only unassign subtasks inside this team's projects
            var subtasks = await _context.Subtasks
                .Include(x => x.MainTask)
                .ThenInclude(x => x.Project)
                .Where(x =>
                    x.AssigneeId == userId &&
                    x.MainTask.Project.TeamId == teamId)
                .ToListAsync();

            foreach (var subtask in subtasks)
            {
                subtask.AssigneeId = null;

                if (subtask.Status == TaskItemStatus.InProgress)
                {
                    subtask.Status = TaskItemStatus.Pending;
                }
            }

            _context.TeamMembers.Remove(targetMember);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Member removed successfully and related tasks unassigned",
                data = new
                {
                    teamId,
                    removedUserId = userId,
                    affectedSubtasks = subtasks.Count
                }
            });
        }
    
    }
}