using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WorkSpaceManager.Shared.DTOs;

// ==================== AUTHENTICATION DTOs ====================

public class LoginRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserInfoResponse User { get; set; } = null!;
}

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

public class RegisterRequest
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;
    
    [Required]
    [Compare("Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? Department { get; set; }
    
    [MaxLength(100)]
    public string? JobTitle { get; set; }
    
    [MaxLength(50)]
    public string? PhoneNumber { get; set; }
}

public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;
    
    [Required]
    [MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;
    
    [Required]
    [Compare("NewPassword")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordConfirmRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;
    
    [Required]
    [MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;
    
    [Required]
    [Compare("NewPassword")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}

// ==================== USER DTOs ====================

public class UserInfoResponse
{
    public Guid Id { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<string> Groups { get; set; } = new();
}

public class UserResponse
{
    public Guid Id { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
    public string? PhoneNumber { get; set; }
    public Guid? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    public string PreferredLanguage { get; set; } = "en";
    public bool IsActive { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<RoleResponse> Roles { get; set; } = new();
    public List<GroupResponse> Groups { get; set; } = new();
}

public class CreateUserRequest
{
    [Required]
    [MaxLength(100)]
    public string EmployeeId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [MinLength(8)]
    public string? Password { get; set; }
    
    [MaxLength(100)]
    public string? Department { get; set; }
    
    [MaxLength(100)]
    public string? JobTitle { get; set; }
    
    [MaxLength(50)]
    public string? PhoneNumber { get; set; }
    
    public Guid? ManagerId { get; set; }
    
    public List<Guid>? RoleIds { get; set; }
    
    public List<Guid>? GroupIds { get; set; }
}

public class UpdateUserRequest
{
    [MaxLength(100)]
    public string? FirstName { get; set; }
    
    [MaxLength(100)]
    public string? LastName { get; set; }
    
    [EmailAddress]
    [MaxLength(255)]
    public string? Email { get; set; }
    
    [MaxLength(100)]
    public string? Department { get; set; }
    
    [MaxLength(100)]
    public string? JobTitle { get; set; }
    
    [MaxLength(50)]
    public string? PhoneNumber { get; set; }
    
    public Guid? ManagerId { get; set; }
    
    [MaxLength(10)]
    public string? PreferredLanguage { get; set; }
    
    public bool? IsActive { get; set; }
}

public class UserSearchRequest
{
    public string? SearchTerm { get; set; }
    public string? Department { get; set; }
    public bool? IsActive { get; set; }
    public List<Guid>? RoleIds { get; set; }
    public List<Guid>? GroupIds { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

// ==================== ROLE DTOs ====================

public class RoleResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public bool IsSystemRole { get; set; }
    public int UserCount { get; set; }
    public int PermissionCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<PermissionResponse> Permissions { get; set; } = new();
}

public class CreateRoleRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public List<Guid>? PermissionIds { get; set; }
}

public class UpdateRoleRequest
{
    [MaxLength(100)]
    public string? Name { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public bool? IsActive { get; set; }
}

public class AssignRoleRequest
{
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public Guid RoleId { get; set; }
    
    public DateTime? ExpiresAt { get; set; }
}

public class RevokeRoleRequest
{
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public Guid RoleId { get; set; }
}

// ==================== PERMISSION DTOs ====================

public class PermissionResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Resource { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreatePermissionRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Resource { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Action { get; set; } = string.Empty;
}

// ==================== GROUP DTOs ====================

public class GroupResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentGroupId { get; set; }
    public string? ParentGroupName { get; set; }
    public bool IsActive { get; set; }
    public int MemberCount { get; set; }
    public int ChildGroupCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateGroupRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public Guid? ParentGroupId { get; set; }
}

public class UpdateGroupRequest
{
    [MaxLength(200)]
    public string? Name { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public Guid? ParentGroupId { get; set; }
    
    public bool? IsActive { get; set; }
}

public class AddGroupMemberRequest
{
    [Required]
    public Guid GroupId { get; set; }
    
    [Required]
    public Guid UserId { get; set; }
    
    public DateTime? ExpiresAt { get; set; }
}

public class RemoveGroupMemberRequest
{
    [Required]
    public Guid GroupId { get; set; }
    
    [Required]
    public Guid UserId { get; set; }
}

public class GroupMemberResponse
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
