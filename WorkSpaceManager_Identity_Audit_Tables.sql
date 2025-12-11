-- ============================================================
-- WorkSpaceManager - Identity and Audit Tables
-- Execute this script against the WorkSpaceManager database
-- ============================================================

USE [WorkSpaceManager]
GO

-- ============================================================
-- IDENTITY TABLES
-- ============================================================

-- Roles Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Roles')
BEGIN
    CREATE TABLE [dbo].[Roles] (
        [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsSystemRole] BIT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Roles] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [UQ_Roles_TenantId_Name] UNIQUE ([TenantId], [Name])
    );

    CREATE INDEX [IX_Roles_TenantId] ON [dbo].[Roles] ([TenantId]);
    CREATE INDEX [IX_Roles_IsActive] ON [dbo].[Roles] ([IsActive]) WHERE [IsDeleted] = 0;

    PRINT 'Created Roles table';
END
ELSE
    PRINT 'Roles table already exists';
GO

-- UserRoles Table
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

-- Permissions Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Permissions')
BEGIN
    CREATE TABLE [dbo].[Permissions] (
        [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [Resource] NVARCHAR(100) NOT NULL,
        [Action] NVARCHAR(50) NOT NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Permissions] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [UQ_Permissions_TenantId_Name] UNIQUE ([TenantId], [Name])
    );

    CREATE INDEX [IX_Permissions_TenantId] ON [dbo].[Permissions] ([TenantId]);
    CREATE INDEX [IX_Permissions_Resource] ON [dbo].[Permissions] ([Resource]);

    PRINT 'Created Permissions table';
END
ELSE
    PRINT 'Permissions table already exists';
GO

-- RolePermissions Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RolePermissions')
BEGIN
    CREATE TABLE [dbo].[RolePermissions] (
        [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [RoleId] UNIQUEIDENTIFIER NOT NULL,
        [PermissionId] UNIQUEIDENTIFIER NOT NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        CONSTRAINT [PK_RolePermissions] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_RolePermissions_Roles] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Roles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_RolePermissions_Permissions] FOREIGN KEY ([PermissionId]) REFERENCES [dbo].[Permissions] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [UQ_RolePermissions_RoleId_PermissionId] UNIQUE ([RoleId], [PermissionId])
    );

    CREATE INDEX [IX_RolePermissions_RoleId] ON [dbo].[RolePermissions] ([RoleId]);
    CREATE INDEX [IX_RolePermissions_PermissionId] ON [dbo].[RolePermissions] ([PermissionId]);

    PRINT 'Created RolePermissions table';
END
ELSE
    PRINT 'RolePermissions table already exists';
GO

-- Groups Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Groups')
BEGIN
    CREATE TABLE [dbo].[Groups] (
        [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [Name] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [ParentGroupId] UNIQUEIDENTIFIER NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Groups] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_Groups_ParentGroup] FOREIGN KEY ([ParentGroupId]) REFERENCES [dbo].[Groups] ([Id]),
        CONSTRAINT [UQ_Groups_TenantId_Name] UNIQUE ([TenantId], [Name])
    );

    CREATE INDEX [IX_Groups_TenantId] ON [dbo].[Groups] ([TenantId]);
    CREATE INDEX [IX_Groups_ParentGroupId] ON [dbo].[Groups] ([ParentGroupId]);

    PRINT 'Created Groups table';
END
ELSE
    PRINT 'Groups table already exists';
GO

-- GroupMembers Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'GroupMembers')
BEGIN
    CREATE TABLE [dbo].[GroupMembers] (
        [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [GroupId] UNIQUEIDENTIFIER NOT NULL,
        [UserId] UNIQUEIDENTIFIER NOT NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [ExpiresAt] DATETIME2 NULL,
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

-- RefreshTokens Table
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

    CREATE UNIQUE INDEX [IX_RefreshTokens_Token] ON [dbo].[RefreshTokens] ([Token]);
    CREATE INDEX [IX_RefreshTokens_UserId] ON [dbo].[RefreshTokens] ([UserId]);
    CREATE INDEX [IX_RefreshTokens_TenantId] ON [dbo].[RefreshTokens] ([TenantId]);

    PRINT 'Created RefreshTokens table';
END
ELSE
    PRINT 'RefreshTokens table already exists';
GO

-- ============================================================
-- AUDIT TABLES
-- ============================================================

-- AuditLogs Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditLogs')
BEGIN
    CREATE TABLE [dbo].[AuditLogs] (
        [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [UserId] UNIQUEIDENTIFIER NULL,
        [UserName] NVARCHAR(255) NULL,
        [Action] NVARCHAR(100) NOT NULL,
        [Resource] NVARCHAR(200) NOT NULL,
        [ResourceId] NVARCHAR(100) NULL,
        [Success] BIT NOT NULL DEFAULT 1,
        [Details] NVARCHAR(500) NULL,
        [ErrorMessage] NVARCHAR(500) NULL,
        [IpAddress] NVARCHAR(50) NULL,
        [UserAgent] NVARCHAR(500) NULL,
        [DurationMs] BIGINT NULL,
        [Metadata] NVARCHAR(MAX) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY CLUSTERED ([Id])
    );

    CREATE INDEX [IX_AuditLogs_TenantId] ON [dbo].[AuditLogs] ([TenantId]);
    CREATE INDEX [IX_AuditLogs_UserId] ON [dbo].[AuditLogs] ([UserId]);
    CREATE INDEX [IX_AuditLogs_Action] ON [dbo].[AuditLogs] ([Action]);
    CREATE INDEX [IX_AuditLogs_Resource] ON [dbo].[AuditLogs] ([Resource]);
    CREATE INDEX [IX_AuditLogs_CreatedAt] ON [dbo].[AuditLogs] ([CreatedAt] DESC);
    CREATE INDEX [IX_AuditLogs_TenantId_CreatedAt] ON [dbo].[AuditLogs] ([TenantId], [CreatedAt] DESC);

    PRINT 'Created AuditLogs table';
END
ELSE
    PRINT 'AuditLogs table already exists';
GO

-- ============================================================
-- ADD NEW COLUMNS TO USERS TABLE (if not exist)
-- ============================================================

-- Add PasswordHash column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'PasswordHash')
BEGIN
    ALTER TABLE [dbo].[Users] ADD [PasswordHash] NVARCHAR(500) NULL;
    PRINT 'Added PasswordHash column to Users table';
END
GO

-- Add LastLoginDate column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'LastLoginDate')
BEGIN
    ALTER TABLE [dbo].[Users] ADD [LastLoginDate] DATETIME2 NULL;
    PRINT 'Added LastLoginDate column to Users table';
END
GO

-- Add FailedLoginAttempts column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'FailedLoginAttempts')
BEGIN
    ALTER TABLE [dbo].[Users] ADD [FailedLoginAttempts] INT NOT NULL DEFAULT 0;
    PRINT 'Added FailedLoginAttempts column to Users table';
END
GO

-- Add LockoutEndDate column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'LockoutEndDate')
BEGIN
    ALTER TABLE [dbo].[Users] ADD [LockoutEndDate] DATETIME2 NULL;
    PRINT 'Added LockoutEndDate column to Users table';
END
GO

-- Add PhoneNumber column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'PhoneNumber')
BEGIN
    ALTER TABLE [dbo].[Users] ADD [PhoneNumber] NVARCHAR(50) NULL;
    PRINT 'Added PhoneNumber column to Users table';
END
GO

-- Make KeycloakUserId nullable (if it's currently NOT NULL)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'KeycloakUserId' AND is_nullable = 0)
BEGIN
    ALTER TABLE [dbo].[Users] ALTER COLUMN [KeycloakUserId] NVARCHAR(100) NULL;
    PRINT 'Made KeycloakUserId nullable in Users table';
END
GO

-- ============================================================
-- SEED DEFAULT DATA
-- ============================================================

DECLARE @TenantId UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000001';

-- Insert default roles if they don't exist
IF NOT EXISTS (SELECT 1 FROM [dbo].[Roles] WHERE [Name] = 'Admin' AND [TenantId] = @TenantId)
BEGIN
    INSERT INTO [dbo].[Roles] ([Id], [TenantId], [Name], [Description], [IsActive], [IsSystemRole])
    VALUES 
        (NEWID(), @TenantId, 'Admin', 'System Administrator with full access', 1, 1),
        (NEWID(), @TenantId, 'FacilityManager', 'Facility Manager - can manage spaces and view reports', 1, 1),
        (NEWID(), @TenantId, 'User', 'Regular user - can book desks and meeting rooms', 1, 1);
    
    PRINT 'Inserted default roles';
END
GO

-- Insert default permissions if they don't exist
DECLARE @TenantId UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000001';

IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [Name] = 'users.read' AND [TenantId] = @TenantId)
BEGIN
    INSERT INTO [dbo].[Permissions] ([Id], [TenantId], [Name], [Description], [Resource], [Action])
    VALUES 
        -- User permissions
        (NEWID(), @TenantId, 'users.read', 'View users', 'users', 'read'),
        (NEWID(), @TenantId, 'users.create', 'Create users', 'users', 'create'),
        (NEWID(), @TenantId, 'users.update', 'Update users', 'users', 'update'),
        (NEWID(), @TenantId, 'users.delete', 'Delete users', 'users', 'delete'),
        -- Booking permissions
        (NEWID(), @TenantId, 'bookings.read', 'View bookings', 'bookings', 'read'),
        (NEWID(), @TenantId, 'bookings.create', 'Create bookings', 'bookings', 'create'),
        (NEWID(), @TenantId, 'bookings.update', 'Update bookings', 'bookings', 'update'),
        (NEWID(), @TenantId, 'bookings.delete', 'Delete bookings', 'bookings', 'delete'),
        -- Space permissions
        (NEWID(), @TenantId, 'spaces.read', 'View spaces', 'spaces', 'read'),
        (NEWID(), @TenantId, 'spaces.create', 'Create spaces', 'spaces', 'create'),
        (NEWID(), @TenantId, 'spaces.update', 'Update spaces', 'spaces', 'update'),
        (NEWID(), @TenantId, 'spaces.delete', 'Delete spaces', 'spaces', 'delete'),
        -- Audit permissions
        (NEWID(), @TenantId, 'audit.read', 'View audit logs', 'audit', 'read'),
        (NEWID(), @TenantId, 'audit.delete', 'Delete audit logs', 'audit', 'delete'),
        -- Report permissions
        (NEWID(), @TenantId, 'reports.read', 'View reports', 'reports', 'read');
    
    PRINT 'Inserted default permissions';
END
GO

-- Insert an admin user with password 'Admin@123'
DECLARE @TenantId UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000001';
DECLARE @AdminUserId UNIQUEIDENTIFIER;

IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [Email] = 'admin@workspacemanager.com')
BEGIN
    SET @AdminUserId = NEWID();
    
    -- Password hash for 'Admin@123' using BCrypt
    INSERT INTO [dbo].[Users] ([Id], [TenantId], [EmployeeId], [FirstName], [LastName], [Email], [PasswordHash], [Department], [JobTitle], [IsActive])
    VALUES (@AdminUserId, @TenantId, 'EMP001', 'System', 'Administrator', 'admin@workspacemanager.com', 
            '$2a$11$rQbPFZxJBmHG/aVQPqKnKeZJEWp3.k/6HnL0zGgJkXG5qGPKMKhKi', -- BCrypt hash for 'Admin@123'
            'IT', 'System Administrator', 1);
    
    -- Assign Admin role to the admin user
    DECLARE @AdminRoleId UNIQUEIDENTIFIER;
    SELECT @AdminRoleId = [Id] FROM [dbo].[Roles] WHERE [Name] = 'Admin' AND [TenantId] = @TenantId;
    
    IF @AdminRoleId IS NOT NULL
    BEGIN
        INSERT INTO [dbo].[UserRoles] ([Id], [TenantId], [UserId], [RoleId], [IsActive])
        VALUES (NEWID(), @TenantId, @AdminUserId, @AdminRoleId, 1);
    END
    
    PRINT 'Inserted admin user (admin@workspacemanager.com / Admin@123)';
END
GO

-- Insert a test user with password 'User@123'
DECLARE @TenantId UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000001';
DECLARE @TestUserId UNIQUEIDENTIFIER;

IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [Email] = 'user@workspacemanager.com')
BEGIN
    SET @TestUserId = NEWID();
    
    INSERT INTO [dbo].[Users] ([Id], [TenantId], [EmployeeId], [FirstName], [LastName], [Email], [PasswordHash], [Department], [JobTitle], [IsActive])
    VALUES (@TestUserId, @TenantId, 'EMP002', 'Test', 'User', 'user@workspacemanager.com', 
            '$2a$11$BjJ.4Y.xL6KGxPOgZf6yOuhMzJ9.8LQ5MV7EqB0HyKKOPJRGNvI2C', -- BCrypt hash for 'User@123'
            'Operations', 'Employee', 1);
    
    -- Assign User role to the test user
    DECLARE @UserRoleId UNIQUEIDENTIFIER;
    SELECT @UserRoleId = [Id] FROM [dbo].[Roles] WHERE [Name] = 'User' AND [TenantId] = @TenantId;
    
    IF @UserRoleId IS NOT NULL
    BEGIN
        INSERT INTO [dbo].[UserRoles] ([Id], [TenantId], [UserId], [RoleId], [IsActive])
        VALUES (NEWID(), @TenantId, @TestUserId, @UserRoleId, 1);
    END
    
    PRINT 'Inserted test user (user@workspacemanager.com / User@123)';
END
GO

PRINT '';
PRINT '============================================================';
PRINT 'Identity and Audit tables setup completed successfully!';
PRINT '============================================================';
PRINT '';
PRINT 'Default users created:';
PRINT '  - admin@workspacemanager.com / Admin@123 (Admin role)';
PRINT '  - user@workspacemanager.com / User@123 (User role)';
PRINT '';
GO
