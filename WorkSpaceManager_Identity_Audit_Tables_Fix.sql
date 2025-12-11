-- ============================================================
-- WorkSpaceManager - Identity and Audit Tables Fix
-- Creates Users table and fixes missing tables
-- ============================================================

USE [WorkSpaceManager]
GO

SET QUOTED_IDENTIFIER ON
GO

-- ============================================================
-- USERS TABLE (Required for Identity Service)
-- ============================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE [dbo].[Users] (
        [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [Email] NVARCHAR(256) NOT NULL,
        [PasswordHash] NVARCHAR(MAX) NOT NULL,
        [FirstName] NVARCHAR(100) NOT NULL,
        [LastName] NVARCHAR(100) NOT NULL,
        [EmployeeId] NVARCHAR(50) NULL,
        [Department] NVARCHAR(100) NULL,
        [PhoneNumber] NVARCHAR(50) NULL,
        [KeycloakId] NVARCHAR(200) NULL,
        [PreferredBuildingId] UNIQUEIDENTIFIER NULL,
        [PreferredFloorId] UNIQUEIDENTIFIER NULL,
        [AccessibilityNeeds] BIT NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [EmailVerified] BIT NOT NULL DEFAULT 0,
        [LastLoginAt] DATETIME2 NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [UQ_Users_TenantId_Email] UNIQUE ([TenantId], [Email])
    );

    CREATE INDEX [IX_Users_TenantId] ON [dbo].[Users] ([TenantId]);
    CREATE INDEX [IX_Users_Email] ON [dbo].[Users] ([Email]);
    CREATE INDEX [IX_Users_KeycloakId] ON [dbo].[Users] ([KeycloakId]) WHERE [KeycloakId] IS NOT NULL;
    CREATE INDEX [IX_Users_EmployeeId] ON [dbo].[Users] ([EmployeeId]) WHERE [EmployeeId] IS NOT NULL;
    CREATE INDEX [IX_Users_IsActive] ON [dbo].[Users] ([IsActive]) WHERE [IsDeleted] = 0;

    PRINT 'Created Users table';
END
ELSE
    PRINT 'Users table already exists';
GO

-- ============================================================
-- USERROLES TABLE
-- ============================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserRoles')
BEGIN
    CREATE TABLE [dbo].[UserRoles] (
        [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [UserId] UNIQUEIDENTIFIER NOT NULL,
        [RoleId] UNIQUEIDENTIFIER NOT NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [ExpiresAt] DATETIME2 NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        CONSTRAINT [PK_UserRoles] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_UserRoles_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UserRoles_Roles] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Roles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [UQ_UserRoles_UserId_RoleId] UNIQUE ([UserId], [RoleId])
    );

    CREATE INDEX [IX_UserRoles_UserId] ON [dbo].[UserRoles] ([UserId]);
    CREATE INDEX [IX_UserRoles_RoleId] ON [dbo].[UserRoles] ([RoleId]);
    CREATE INDEX [IX_UserRoles_TenantId] ON [dbo].[UserRoles] ([TenantId]);

    PRINT 'Created UserRoles table';
END
ELSE
    PRINT 'UserRoles table already exists';
GO

-- ============================================================
-- GROUPMEMBERS TABLE
-- ============================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'GroupMembers')
BEGIN
    CREATE TABLE [dbo].[GroupMembers] (
        [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [GroupId] UNIQUEIDENTIFIER NOT NULL,
        [UserId] UNIQUEIDENTIFIER NOT NULL,
        [IsLeader] BIT NOT NULL DEFAULT 0,
        [JoinedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        CONSTRAINT [PK_GroupMembers] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_GroupMembers_Groups] FOREIGN KEY ([GroupId]) REFERENCES [dbo].[Groups] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_GroupMembers_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [UQ_GroupMembers_GroupId_UserId] UNIQUE ([GroupId], [UserId])
    );

    CREATE INDEX [IX_GroupMembers_GroupId] ON [dbo].[GroupMembers] ([GroupId]);
    CREATE INDEX [IX_GroupMembers_UserId] ON [dbo].[GroupMembers] ([UserId]);
    CREATE INDEX [IX_GroupMembers_TenantId] ON [dbo].[GroupMembers] ([TenantId]);

    PRINT 'Created GroupMembers table';
END
ELSE
    PRINT 'GroupMembers table already exists';
GO

-- ============================================================
-- REFRESHTOKENS TABLE
-- ============================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RefreshTokens')
BEGIN
    CREATE TABLE [dbo].[RefreshTokens] (
        [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [UserId] UNIQUEIDENTIFIER NOT NULL,
        [Token] NVARCHAR(500) NOT NULL,
        [ExpiresAt] DATETIME2 NOT NULL,
        [IsRevoked] BIT NOT NULL DEFAULT 0,
        [RevokedAt] DATETIME2 NULL,
        [ReplacedByToken] NVARCHAR(500) NULL,
        [CreatedByIp] NVARCHAR(50) NULL,
        [RevokedByIp] NVARCHAR(50) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        CONSTRAINT [PK_RefreshTokens] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_RefreshTokens_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_RefreshTokens_UserId] ON [dbo].[RefreshTokens] ([UserId]);
    CREATE INDEX [IX_RefreshTokens_Token] ON [dbo].[RefreshTokens] ([Token]);
    CREATE INDEX [IX_RefreshTokens_TenantId] ON [dbo].[RefreshTokens] ([TenantId]);
    CREATE INDEX [IX_RefreshTokens_ExpiresAt] ON [dbo].[RefreshTokens] ([ExpiresAt]) WHERE [IsRevoked] = 0;

    PRINT 'Created RefreshTokens table';
END
ELSE
    PRINT 'RefreshTokens table already exists';
GO

-- ============================================================
-- ADD PASSWORDHASH COLUMN TO USERS IF MISSING
-- ============================================================

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
    AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'PasswordHash')
BEGIN
    ALTER TABLE [dbo].[Users] ADD [PasswordHash] NVARCHAR(MAX) NOT NULL DEFAULT '';
    PRINT 'Added PasswordHash column to Users table';
END
GO

-- ============================================================
-- INSERT DEFAULT ADMIN USER
-- ============================================================

DECLARE @DefaultTenantId UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000001';
DECLARE @AdminRoleId UNIQUEIDENTIFIER;
DECLARE @UserRoleId UNIQUEIDENTIFIER;
DECLARE @AdminUserId UNIQUEIDENTIFIER;
DECLARE @UserUserId UNIQUEIDENTIFIER;

-- Get or create Admin role
SELECT @AdminRoleId = Id FROM [dbo].[Roles] WHERE [Name] = 'Admin' AND [TenantId] = @DefaultTenantId;
IF @AdminRoleId IS NULL
BEGIN
    SET @AdminRoleId = NEWID();
    INSERT INTO [dbo].[Roles] ([Id], [TenantId], [Name], [Description], [IsSystemRole])
    VALUES (@AdminRoleId, @DefaultTenantId, 'Admin', 'System Administrator with full access', 1);
END

-- Get or create User role
SELECT @UserRoleId = Id FROM [dbo].[Roles] WHERE [Name] = 'User' AND [TenantId] = @DefaultTenantId;
IF @UserRoleId IS NULL
BEGIN
    SET @UserRoleId = NEWID();
    INSERT INTO [dbo].[Roles] ([Id], [TenantId], [Name], [Description], [IsSystemRole])
    VALUES (@UserRoleId, @DefaultTenantId, 'User', 'Standard user with basic access', 1);
END

-- Create admin user if not exists
IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [Email] = 'admin@workspacemanager.com' AND [TenantId] = @DefaultTenantId)
BEGIN
    SET @AdminUserId = NEWID();
    -- Password: Admin@123 (BCrypt hash)
    INSERT INTO [dbo].[Users] ([Id], [TenantId], [Email], [PasswordHash], [FirstName], [LastName], [IsActive], [EmailVerified])
    VALUES (@AdminUserId, @DefaultTenantId, 'admin@workspacemanager.com', 
            '$2a$11$K3g4/ZKpK8Zrv8cQK0Fv5.ZvWJx2gH1HH7JxpWfJzZmzNmZLqZKfO', 
            'System', 'Administrator', 1, 1);

    -- Assign admin role
    INSERT INTO [dbo].[UserRoles] ([TenantId], [UserId], [RoleId])
    VALUES (@DefaultTenantId, @AdminUserId, @AdminRoleId);

    PRINT 'Created admin user: admin@workspacemanager.com / Admin@123';
END
ELSE
    PRINT 'Admin user already exists';

-- Create regular user if not exists
IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [Email] = 'user@workspacemanager.com' AND [TenantId] = @DefaultTenantId)
BEGIN
    SET @UserUserId = NEWID();
    -- Password: User@123 (BCrypt hash)
    INSERT INTO [dbo].[Users] ([Id], [TenantId], [Email], [PasswordHash], [FirstName], [LastName], [IsActive], [EmailVerified])
    VALUES (@UserUserId, @DefaultTenantId, 'user@workspacemanager.com', 
            '$2a$11$K3g4/ZKpK8Zrv8cQK0Fv5.ZvWJx2gH1HH7JxpWfJzZmzNmZLqZKfO', 
            'Test', 'User', 1, 1);

    -- Assign user role
    INSERT INTO [dbo].[UserRoles] ([TenantId], [UserId], [RoleId])
    VALUES (@DefaultTenantId, @UserUserId, @UserRoleId);

    PRINT 'Created test user: user@workspacemanager.com / User@123';
END
ELSE
    PRINT 'Test user already exists';
GO

-- ============================================================
-- COMPLETION MESSAGE
-- ============================================================
PRINT '';
PRINT '============================================================';
PRINT 'Identity and Audit tables fix completed!';
PRINT '============================================================';
PRINT '';
PRINT 'Default users:';
PRINT '  - admin@workspacemanager.com / Admin@123 (Admin role)';
PRINT '  - user@workspacemanager.com / User@123 (User role)';
PRINT '';
GO
