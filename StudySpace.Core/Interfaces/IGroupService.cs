using StudySpace.Core.DTOs.Group;

namespace StudySpace.Core.Interfaces;

public interface IGroupService
{
    Task<GroupDto> CreateAsync(int currentUserId, CreateGroupRequest request);
    Task<GroupDto> UpdateAsync(int currentUserId, int groupId, UpdateGroupRequest request);
    Task DeleteAsync(int currentUserId, int groupId);
    Task<GroupDto> GetByIdAsync(int? currentUserId, int groupId);
    Task<List<GroupDto>> ListAsync(int? currentUserId, string? search = null, bool? joinedOnly = null);
    Task<List<GroupDto>> MyGroupsAsync(int currentUserId);
    Task<GroupMemberDto> JoinAsync(int currentUserId, JoinGroupRequest request);
    Task LeaveAsync(int currentUserId, int groupId);
    Task<List<GroupMemberDto>> ListMembersAsync(int? currentUserId, int groupId);
    Task<GroupMemberDto> UpdateMemberRoleAsync(int currentUserId, int groupId, int targetUserId, UpdateMemberRoleRequest request);
    Task RemoveMemberAsync(int currentUserId, int groupId, int targetUserId);
}
