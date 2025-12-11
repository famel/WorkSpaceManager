-- =============================================
-- WorkSpaceManager: Unified Database Schema
-- SQL Server 2022
-- Single Database with Service-Prefixed Tables
-- Version: 2.0
-- Date: December 2025
-- =============================================

-- =============================================
-- SECTION 1: CREATE DATABASE
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'WorkSpaceManager')
BEGIN
    CREATE DATABASE WorkSpaceManager;
END
GO

USE WorkSpaceManager;
GO

-- =============================================
-- SECTION 2: IDENTITY SERVICE TABLES
-- =============================================

-- Identity_Tenants table (for multitenancy)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Identity_Tenants')
BEGIN
    CREATE TABLE Identity_Tenants (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Name NVARCHAR(200) NOT NULL,
        Code NVARCHAR(50) NOT NULL UNIQUE,
        KeycloakRealmId NVARCHAR(100) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        SubscriptionTier NVARCHAR(50) NOT NULL DEFAULT 'Standard',
        MaxUsers INT NOT NULL DEFAULT 100,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        INDEX IX_Identity_Tenants_Code (Code),
        INDEX IX_Identity_Tenants_IsActive (IsActive)
    );
END
GO

-- Identity_UserMetadata (supplementary to Keycloak)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Identity_UserMetadata')
BEGIN
    CREATE TABLE Identity_UserMetadata (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        TenantId UNIQUEIDENTIFIER NOT NULL,
        KeycloakUserId NVARCHAR(100) NOT NULL UNIQUE,
        EmployeeId NVARCHAR(50),
        Department NVARCHAR(100),
        JobTitle NVARCHAR(200),
        ManagerId UNIQUEIDENTIFIER NULL,
        PreferredLanguage NVARCHAR(10) NOT NULL DEFAULT 'el',
        NoShowCount INT NOT NULL DEFAULT 0,
        LastLoginAt DATETIME2,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        FOREIGN KEY (TenantId) REFERENCES Identity_Tenants(Id),
        FOREIGN KEY (ManagerId) REFERENCES Identity_UserMetadata(Id),
        INDEX IX_Identity_UserMetadata_TenantId (TenantId),
        INDEX IX_Identity_UserMetadata_KeycloakUserId (KeycloakUserId),
        INDEX IX_Identity_UserMetadata_Department (Department)
    );
END
GO

-- Identity_AuditLog
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Identity_AuditLog')
BEGIN
    CREATE TABLE Identity_AuditLog (
        Id BIGINT IDENTITY(1,1) PRIMARY KEY,
        TenantId UNIQUEIDENTIFIER NOT NULL,
        UserId NVARCHAR(100),
        Action NVARCHAR(100) NOT NULL,
        EntityType NVARCHAR(100),
        EntityId NVARCHAR(100),
        OldValues NVARCHAR(MAX),
        NewValues NVARCHAR(MAX),
        IpAddress NVARCHAR(50),
        UserAgent NVARCHAR(500),
        Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        INDEX IX_Identity_AuditLog_TenantId_Timestamp (TenantId, Timestamp),
        INDEX IX_Identity_AuditLog_UserId (UserId)
    );
END
GO

-- =============================================
-- SECTION 3: RESOURCE SERVICE TABLES
-- =============================================

-- Resource_Buildings
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Resource_Buildings')
BEGIN
    CREATE TABLE Resource_Buildings (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        TenantId UNIQUEIDENTIFIER NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        Code NVARCHAR(50) NOT NULL,
        Street NVARCHAR(200),
        City NVARCHAR(100),
        PostalCode NVARCHAR(20),
        Country NVARCHAR(100),
        Latitude DECIMAL(10, 8),
        Longitude DECIMAL(11, 8),
        TimeZone NVARCHAR(50) NOT NULL DEFAULT 'Europe/Athens',
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        FOREIGN KEY (TenantId) REFERENCES Identity_Tenants(Id),
        INDEX IX_Resource_Buildings_TenantId (TenantId),
        INDEX IX_Resource_Buildings_TenantId_IsActive (TenantId, IsActive),
        UNIQUE (TenantId, Code)
    );
END
GO

-- Resource_Floors
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Resource_Floors')
BEGIN
    CREATE TABLE Resource_Floors (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        TenantId UNIQUEIDENTIFIER NOT NULL,
        BuildingId UNIQUEIDENTIFIER NOT NULL,
        FloorNumber INT NOT NULL,
        Name NVARCHAR(100) NOT NULL,
        TotalArea DECIMAL(10, 2),
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        FOREIGN KEY (TenantId) REFERENCES Identity_Tenants(Id),
        FOREIGN KEY (BuildingId) REFERENCES Resource_Buildings(Id) ON DELETE CASCADE,
        INDEX IX_Resource_Floors_BuildingId (BuildingId),
        INDEX IX_Resource_Floors_TenantId (TenantId),
        UNIQUE (BuildingId, FloorNumber)
    );
END
GO

-- Resource_FloorPlans
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Resource_FloorPlans')
BEGIN
    CREATE TABLE Resource_FloorPlans (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        TenantId UNIQUEIDENTIFIER NOT NULL,
        FloorId UNIQUEIDENTIFIER NOT NULL,
        ImageUrl NVARCHAR(500) NOT NULL,
        ImageWidth INT NOT NULL,
        ImageHeight INT NOT NULL,
        MappingJson NVARCHAR(MAX) NOT NULL, -- JSON with coordinates mapping
        Version INT NOT NULL DEFAULT 1,
        UploadedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UploadedBy NVARCHAR(100) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        
        FOREIGN KEY (TenantId) REFERENCES Identity_Tenants(Id),
        FOREIGN KEY (FloorId) REFERENCES Resource_Floors(Id) ON DELETE CASCADE,
        INDEX IX_Resource_FloorPlans_FloorId (FloorId),
        INDEX IX_Resource_FloorPlans_TenantId (TenantId)
    );
END
GO

-- Resource_Zones
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Resource_Zones')
BEGIN
    CREATE TABLE Resource_Zones (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        TenantId UNIQUEIDENTIFIER NOT NULL,
        FloorId UNIQUEIDENTIFIER NOT NULL,
        Name NVARCHAR(100) NOT NULL,
        Code NVARCHAR(50) NOT NULL,
        Department NVARCHAR(100),
        Color NVARCHAR(7), -- Hex color code for UI
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        FOREIGN KEY (TenantId) REFERENCES Identity_Tenants(Id),
        FOREIGN KEY (FloorId) REFERENCES Resource_Floors(Id) ON DELETE CASCADE,
        INDEX IX_Resource_Zones_FloorId (FloorId),
        INDEX IX_Resource_Zones_TenantId (TenantId),
        UNIQUE (FloorId, Code)
    );
END
GO

-- Resource_Desks
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Resource_Desks')
BEGIN
    CREATE TABLE Resource_Desks (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        TenantId UNIQUEIDENTIFIER NOT NULL,
        FloorId UNIQUEIDENTIFIER NOT NULL,
        ZoneId UNIQUEIDENTIFIER NULL,
        DeskNumber NVARCHAR(50) NOT NULL,
        CoordinateX FLOAT NOT NULL DEFAULT 0,
        CoordinateY FLOAT NOT NULL DEFAULT 0,
        Type NVARCHAR(50) NOT NULL DEFAULT 'Standard', -- Standard, Standing, Ergonomic, Quiet
        IsAvailable BIT NOT NULL DEFAULT 1,
        IsBookable BIT NOT NULL DEFAULT 1,
        Amenities NVARCHAR(500), -- Comma-separated: Monitor,Keyboard,Mouse
        Notes NVARCHAR(500),
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        FOREIGN KEY (TenantId) REFERENCES Identity_Tenants(Id),
        FOREIGN KEY (FloorId) REFERENCES Resource_Floors(Id) ON DELETE CASCADE,
        FOREIGN KEY (ZoneId) REFERENCES Resource_Zones(Id),
        INDEX IX_Resource_Desks_FloorId (FloorId),
        INDEX IX_Resource_Desks_TenantId (TenantId),
        INDEX IX_Resource_Desks_TenantId_IsAvailable_IsBookable (TenantId, IsAvailable, IsBookable),
        INDEX IX_Resource_Desks_ZoneId (ZoneId),
        UNIQUE (FloorId, DeskNumber)
    );
END
GO

-- Resource_MeetingRooms
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Resource_MeetingRooms')
BEGIN
    CREATE TABLE Resource_MeetingRooms (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        TenantId UNIQUEIDENTIFIER NOT NULL,
        FloorId UNIQUEIDENTIFIER NOT NULL,
        ZoneId UNIQUEIDENTIFIER NULL,
        RoomNumber NVARCHAR(50) NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        Capacity INT NOT NULL,
        CoordinateX FLOAT NOT NULL DEFAULT 0,
        CoordinateY FLOAT NOT NULL DEFAULT 0,
        Equipment NVARCHAR(500), -- Comma-separated: Projector,Whiteboard,VideoConference
        IsAvailable BIT NOT NULL DEFAULT 1,
        IsBookable BIT NOT NULL DEFAULT 1,
        RequiresApproval BIT NOT NULL DEFAULT 0,
        Notes NVARCHAR(500),
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        FOREIGN KEY (TenantId) REFERENCES Identity_Tenants(Id),
        FOREIGN KEY (FloorId) REFERENCES Resource_Floors(Id) ON DELETE CASCADE,
        FOREIGN KEY (ZoneId) REFERENCES Resource_Zones(Id),
        INDEX IX_Resource_MeetingRooms_FloorId (FloorId),
        INDEX IX_Resource_MeetingRooms_TenantId (TenantId),
        INDEX IX_Resource_MeetingRooms_TenantId_IsAvailable_IsBookable (TenantId, IsAvailable, IsBookable),
        INDEX IX_Resource_MeetingRooms_Capacity (Capacity),
        UNIQUE (FloorId, RoomNumber)
    );
END
GO

-- Resource_SpecialSpaces
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Resource_SpecialSpaces')
BEGIN
    CREATE TABLE Resource_SpecialSpaces (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        TenantId UNIQUEIDENTIFIER NOT NULL,
        BuildingId UNIQUEIDENTIFIER NOT NULL,
        FloorId UNIQUEIDENTIFIER NULL,
        SpaceType NVARCHAR(50) NOT NULL, -- Parking, Locker, PhoneBooth, RelaxArea
        SpaceNumber NVARCHAR(50) NOT NULL,
        Name NVARCHAR(200),
        IsAvailable BIT NOT NULL DEFAULT 1,
        IsBookable BIT NOT NULL DEFAULT 1,
        Notes NVARCHAR(500),
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        FOREIGN KEY (TenantId) REFERENCES Identity_Tenants(Id),
        FOREIGN KEY (BuildingId) REFERENCES Resource_Buildings(Id) ON DELETE CASCADE,
        FOREIGN KEY (FloorId) REFERENCES Resource_Floors(Id),
        INDEX IX_Resource_SpecialSpaces_BuildingId (BuildingId),
        INDEX IX_Resource_SpecialSpaces_TenantId (TenantId),
        INDEX IX_Resource_SpecialSpaces_SpaceType (SpaceType)
    );
END
GO

-- Resource_AuditLog
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Resource_AuditLog')
BEGIN
    CREATE TABLE Resource_AuditLog (
        Id BIGINT IDENTITY(1,1) PRIMARY KEY,
        TenantId UNIQUEIDENTIFIER NOT NULL,
        ResourceType NVARCHAR(50) NOT NULL, -- Building, Floor, Desk, MeetingRoom
        ResourceId UNIQUEIDENTIFIER NOT NULL,
        Action NVARCHAR(50) NOT NULL, -- Created, Updated, Deleted, Activated, Deactivated
        ChangedBy NVARCHAR(100) NOT NULL,
        OldValues NVARCHAR(MAX),
        NewValues NVARCHAR(MAX),
        Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        FOREIGN KEY (TenantId) REFERENCES Identity_Tenants(Id),
        INDEX IX_Resource_AuditLog_TenantId_Timestamp (TenantId, Timestamp),
        INDEX IX_Resource_AuditLog_ResourceType_ResourceId (ResourceType, ResourceId)
    );
END
GO

-- =============================================
-- SECTION 4: BOOKING SERVICE TABLES
-- =============================================

-- Booking_Bookings
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Booking_Bookings')
BEGIN
    CREATE TABLE Booking_Bookings (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        TenantId UNIQUEIDENTIFIER NOT NULL,
        UserId UNIQUEIDENTIFIER NOT NULL,
        ResourceId UNIQUEIDENTIFIER NOT NULL,
        ResourceType NVARCHAR(50) NOT NULL, -- Desk, MeetingRoom, Parking, Locker
        StartTime DATETIME2 NOT NULL,
        EndTime DATETIME2 NOT NULL,
        Status NVARCHAR(50) NOT NULL DEFAULT 'Confirmed', -- Confirmed, CheckedIn, Cancelled, NoShow, Completed
        IsRecurring BIT NOT NULL DEFAULT 0,
        RecurrencePattern NVARCHAR(100), -- Daily, Weekly, Monthly
        RecurrenceEndDate DATETIME2,
        ParentBookingId UNIQUEIDENTIFIER NULL, -- For recurring bookings
        CancellationReason NVARCHAR(500),
        CheckInTime DATETIME2,
        CheckOutTime DATETIME2,
        Notes NVARCHAR(500),
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        FOREIGN KEY (TenantId) REFERENCES Identity_Tenants(Id),
        FOREIGN KEY (UserId) REFERENCES Identity_UserMetadata(Id),
        FOREIGN KEY (ParentBookingId) REFERENCES Booking_Bookings(Id),
        INDEX IX_Booking_Bookings_TenantId_UserId_StartTime (TenantId, UserId, StartTime),
        INDEX IX_Booking_Bookings_TenantId_ResourceId_StartTime (TenantId, ResourceId, StartTime),
        INDEX IX_Booking_Bookings_Status (Status),
        INDEX IX_Booking_Bookings_StartTime_EndTime (StartTime, EndTime),
        INDEX IX_Booking_Bookings_ResourceType (ResourceType)
    );
END
GO

-- Booking_CheckIns
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Booking_CheckIns')
BEGIN
    CREATE TABLE Booking_CheckIns (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        TenantId UNIQUEIDENTIFIER NOT NULL,
        BookingId UNIQUEIDENTIFIER NOT NULL,
        UserId UNIQUEIDENTIFIER NOT NULL,
        CheckInTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CheckInMethod NVARCHAR(50) NOT NULL, -- Manual, RFID, QRCode, Mobile
        DeviceId NVARCHAR(100),
        Location NVARCHAR(200),
        
        FOREIGN KEY (TenantId) REFERENCES Identity_Tenants(Id),
        FOREIGN KEY (BookingId) REFERENCES Booking_Bookings(Id) ON DELETE CASCADE,
        FOREIGN KEY (UserId) REFERENCES Identity_UserMetadata(Id),
        INDEX IX_Booking_CheckIns_BookingId (BookingId),
        INDEX IX_Booking_CheckIns_TenantId_UserId (TenantId, UserId),
        INDEX IX_Booking_CheckIns_CheckInTime (CheckInTime)
    );
END
GO

-- Booking_Participants
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Booking_Participants')
BEGIN
    CREATE TABLE Booking_Participants (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        BookingId UNIQUEIDENTIFIER NOT NULL,
        UserId UNIQUEIDENTIFIER NOT NULL,
        IsOrganizer BIT NOT NULL DEFAULT 0,
        ResponseStatus NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, Accepted, Declined
        AddedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        FOREIGN KEY (BookingId) REFERENCES Booking_Bookings(Id) ON DELETE CASCADE,
        FOREIGN KEY (UserId) REFERENCES Identity_UserMetadata(Id),
        INDEX IX_Booking_Participants_BookingId (BookingId),
        INDEX IX_Booking_Participants_UserId (UserId),
        UNIQUE (BookingId, UserId)
    );
END
GO

-- Booking_History
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Booking_History')
BEGIN
    CREATE TABLE Booking_History (
        Id BIGINT IDENTITY(1,1) PRIMARY KEY,
        BookingId UNIQUEIDENTIFIER NOT NULL,
        TenantId UNIQUEIDENTIFIER NOT NULL,
        Action NVARCHAR(50) NOT NULL, -- Created, Modified, Cancelled, CheckedIn, NoShow
        ChangedBy UNIQUEIDENTIFIER NOT NULL,
        OldStatus NVARCHAR(50),
        NewStatus NVARCHAR(50),
        Details NVARCHAR(MAX),
        Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        FOREIGN KEY (TenantId) REFERENCES Identity_Tenants(Id),
        FOREIGN KEY (ChangedBy) REFERENCES Identity_UserMetadata(Id),
        INDEX IX_Booking_History_BookingId (BookingId),
        INDEX IX_Booking_History_TenantId_Timestamp (TenantId, Timestamp)
    );
END
GO

-- Booking_Waitlist
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Booking_Waitlist')
BEGIN
    CREATE TABLE Booking_Waitlist (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        TenantId UNIQUEIDENTIFIER NOT NULL,
        UserId UNIQUEIDENTIFIER NOT NULL,
        ResourceId UNIQUEIDENTIFIER NOT NULL,
        ResourceType NVARCHAR(50) NOT NULL,
        RequestedStartTime DATETIME2 NOT NULL,
        RequestedEndTime DATETIME2 NOT NULL,
        Status NVARCHAR(50) NOT NULL DEFAULT 'Waiting', -- Waiting, Notified, Expired, Cancelled
        Priority INT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        NotifiedAt DATETIME2,
        ExpiresAt DATETIME2,
        
        FOREIGN KEY (TenantId) REFERENCES Identity_Tenants(Id),
        FOREIGN KEY (UserId) REFERENCES Identity_UserMetadata(Id),
        INDEX IX_Booking_Waitlist_TenantId_ResourceId (TenantId, ResourceId),
        INDEX IX_Booking_Waitlist_UserId (UserId),
        INDEX IX_Booking_Waitlist_Status (Status),
        INDEX IX_Booking_Waitlist_RequestedStartTime (RequestedStartTime)
    );
END
GO

-- =============================================
-- SECTION 5: RULES SERVICE TABLES
-- =============================================

-- Rules_PolicyConfigurations
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Rules_PolicyConfigurations')
BEGIN
    CREATE TABLE Rules_PolicyConfigurations (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        TenantId UNIQUEIDENTIFIER NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        Description NVARCHAR(500),
        IsActive BIT NOT NULL DEFAULT 1,
        
        -- Booking limits
        MaxDaysPerWeek INT NOT NULL DEFAULT 5,
        MaxDaysPerMonth INT NOT NULL DEFAULT 20,
        MaxAdvanceBookingDays INT NOT NULL DEFAULT 30,
        MinAdvanceBookingHours INT NOT NULL DEFAULT 1,
        
        -- Restrictions
        RestrictToDepartment BIT NOT NULL DEFAULT 0,
        RestrictToBuilding BIT NOT NULL DEFAULT 0,
        AllowRecurringBookings BIT NOT NULL DEFAULT 1,
        MaxRecurringWeeks INT NOT NULL DEFAULT 12,
        
        -- No-show management
        NoShowThreshold INT NOT NULL DEFAULT 3,
        NoShowPenaltyDays INT NOT NULL DEFAULT 7,
        RequireCheckIn BIT NOT NULL DEFAULT 1,
        CheckInWindowMinutes INT NOT NULL DEFAULT 30,
        
        -- Cancellation
        MinCancellationHours INT NOT NULL DEFAULT 2,
        AllowSameDayCancellation BIT NOT NULL DEFAULT 1,
        
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        FOREIGN KEY (TenantId) REFERENCES Identity_Tenants(Id),
        INDEX IX_Rules_PolicyConfigurations_TenantId (TenantId),
        UNIQUE (TenantId, Name)
    );
END
GO

-- Rules_CustomRules
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Rules_CustomRules')
BEGIN
    CREATE TABLE Rules_CustomRules (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        TenantId UNIQUEIDENTIFIER NOT NULL,
        PolicyId UNIQUEIDENTIFIER NOT NULL,
        RuleName NVARCHAR(100) NOT NULL,
        RuleType NVARCHAR(50) NOT NULL, -- Validation, Warning, Information
        Condition NVARCHAR(MAX) NOT NULL, -- JSON expression
        Message NVARCHAR(500) NOT NULL,
        Severity NVARCHAR(50) NOT NULL DEFAULT 'Error', -- Error, Warning, Info
        IsActive BIT NOT NULL DEFAULT 1,
        Priority INT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        FOREIGN KEY (TenantId) REFERENCES Identity_Tenants(Id),
        FOREIGN KEY (PolicyId) REFERENCES Rules_PolicyConfigurations(Id) ON DELETE CASCADE,
        INDEX IX_Rules_CustomRules_TenantId (TenantId),
        INDEX IX_Rules_CustomRules_PolicyId (PolicyId),
        INDEX IX_Rules_CustomRules_IsActive (IsActive)
    );
END
GO

-- Rules_ExecutionLog
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Rules_ExecutionLog')
BEGIN
    CREATE TABLE Rules_ExecutionLog (
        Id BIGINT IDENTITY(1,1) PRIMARY KEY,
        TenantId UNIQUEIDENTIFIER NOT NULL,
        UserId UNIQUEIDENTIFIER NOT NULL,
        BookingId UNIQUEIDENTIFIER,
        RuleName NVARCHAR(100) NOT NULL,
        Result NVARCHAR(50) NOT NULL, -- Passed, Failed, Warning
        Message NVARCHAR(500),
        ExecutionTimeMs INT,
        Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        FOREIGN KEY (TenantId) REFERENCES Identity_Tenants(Id),
        FOREIGN KEY (UserId) REFERENCES Identity_UserMetadata(Id),
        INDEX IX_Rules_ExecutionLog_TenantId_Timestamp (TenantId, Timestamp),
        INDEX IX_Rules_ExecutionLog_BookingId (BookingId),
        INDEX IX_Rules_ExecutionLog_Result (Result)
    );
END
GO

-- Rules_UserViolations
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Rules_UserViolations')
BEGIN
    CREATE TABLE Rules_UserViolations (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        TenantId UNIQUEIDENTIFIER NOT NULL,
        UserId UNIQUEIDENTIFIER NOT NULL,
        ViolationType NVARCHAR(50) NOT NULL, -- NoShow, LateCancellation, PolicyViolation
        BookingId UNIQUEIDENTIFIER,
        Description NVARCHAR(500),
        PenaltyApplied BIT NOT NULL DEFAULT 0,
        PenaltyEndDate DATETIME2,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ResolvedAt DATETIME2,
        ResolvedBy UNIQUEIDENTIFIER,
        
        FOREIGN KEY (TenantId) REFERENCES Identity_Tenants(Id),
        FOREIGN KEY (UserId) REFERENCES Identity_UserMetadata(Id),
        FOREIGN KEY (ResolvedBy) REFERENCES Identity_UserMetadata(Id),
        INDEX IX_Rules_UserViolations_TenantId_UserId (TenantId, UserId),
        INDEX IX_Rules_UserViolations_ViolationType (ViolationType),
        INDEX IX_Rules_UserViolations_CreatedAt (CreatedAt)
    );
END
GO

-- =============================================
-- SECTION 6: NOTIFICATION SERVICE TABLES
-- =============================================

-- Notification_Templates
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Notification_Templates')
BEGIN
    CREATE TABLE Notification_Templates (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        TenantId UNIQUEIDENTIFIER NOT NULL,
        TemplateType NVARCHAR(50) NOT NULL, -- BookingConfirmation, BookingReminder, Cancellation, etc.
        Channel NVARCHAR(50) NOT NULL, -- Email, Push, SMS
        Language NVARCHAR(10) NOT NULL DEFAULT 'el',
        Subject NVARCHAR(200),
        BodyTemplate NVARCHAR(MAX) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        FOREIGN KEY (TenantId) REFERENCES Identity_Tenants(Id),
        INDEX IX_Notification_Templates_TenantId (TenantId),
        INDEX IX_Notification_Templates_TemplateType (TemplateType),
        UNIQUE (TenantId, TemplateType, Channel, Language)
    );
END
GO

-- Notification_Queue
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Notification_Queue')
BEGIN
    CREATE TABLE Notification_Queue (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        TenantId UNIQUEIDENTIFIER NOT NULL,
        UserId UNIQUEIDENTIFIER NOT NULL,
        Channel NVARCHAR(50) NOT NULL, -- Email, Push, SMS
        RecipientAddress NVARCHAR(200) NOT NULL, -- Email address, device token, phone number
        Subject NVARCHAR(200),
        Body NVARCHAR(MAX) NOT NULL,
        Status NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, Sent, Failed, Cancelled
        Priority INT NOT NULL DEFAULT 0,
        ScheduledFor DATETIME2,
        SentAt DATETIME2,
        FailureReason NVARCHAR(500),
        RetryCount INT NOT NULL DEFAULT 0,
        MaxRetries INT NOT NULL DEFAULT 3,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        FOREIGN KEY (TenantId) REFERENCES Identity_Tenants(Id),
        FOREIGN KEY (UserId) REFERENCES Identity_UserMetadata(Id),
        INDEX IX_Notification_Queue_TenantId_UserId (TenantId, UserId),
        INDEX IX_Notification_Queue_Status (Status),
        INDEX IX_Notification_Queue_ScheduledFor (ScheduledFor),
        INDEX IX_Notification_Queue_CreatedAt (CreatedAt)
    );
END
GO

-- Notification_PushDevices
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Notification_PushDevices')
BEGIN
    CREATE TABLE Notification_PushDevices (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        TenantId UNIQUEIDENTIFIER NOT NULL,
        UserId UNIQUEIDENTIFIER NOT NULL,
        DeviceToken NVARCHAR(500) NOT NULL,
        Platform NVARCHAR(20) NOT NULL, -- iOS, Android
        DeviceModel NVARCHAR(100),
        AppVersion NVARCHAR(20),
        IsActive BIT NOT NULL DEFAULT 1,
        RegisteredAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        LastUsedAt DATETIME2,
        
        FOREIGN KEY (TenantId) REFERENCES Identity_Tenants(Id),
        FOREIGN KEY (UserId) REFERENCES Identity_UserMetadata(Id),
        INDEX IX_Notification_PushDevices_UserId (UserId),
        INDEX IX_Notification_PushDevices_DeviceToken (DeviceToken),
        UNIQUE (DeviceToken)
    );
END
GO

-- Notification_Preferences
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Notification_Preferences')
BEGIN
    CREATE TABLE Notification_Preferences (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        TenantId UNIQUEIDENTIFIER NOT NULL,
        UserId UNIQUEIDENTIFIER NOT NULL,
        
        -- Email preferences
        EmailEnabled BIT NOT NULL DEFAULT 1,
        BookingConfirmationEmail BIT NOT NULL DEFAULT 1,
        BookingReminderEmail BIT NOT NULL DEFAULT 1,
        CancellationEmail BIT NOT NULL DEFAULT 1,
        
        -- Push preferences
        PushEnabled BIT NOT NULL DEFAULT 1,
        BookingConfirmationPush BIT NOT NULL DEFAULT 1,
        BookingReminderPush BIT NOT NULL DEFAULT 1,
        CancellationPush BIT NOT NULL DEFAULT 1,
        
        -- Timing
        ReminderHoursBefore INT NOT NULL DEFAULT 24,
        
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        FOREIGN KEY (TenantId) REFERENCES Identity_Tenants(Id),
        FOREIGN KEY (UserId) REFERENCES Identity_UserMetadata(Id),
        INDEX IX_Notification_Preferences_UserId (UserId),
        UNIQUE (TenantId, UserId)
    );
END
GO

-- =============================================
-- SECTION 7: VIEWS AND FUNCTIONS
-- =============================================

-- View: Active Bookings Summary
IF EXISTS (SELECT * FROM sys.views WHERE name = 'vw_Booking_ActiveBookings')
    DROP VIEW vw_Booking_ActiveBookings;
GO

CREATE VIEW vw_Booking_ActiveBookings AS
SELECT 
    b.Id,
    b.TenantId,
    b.UserId,
    b.ResourceId,
    b.ResourceType,
    b.StartTime,
    b.EndTime,
    b.Status,
    b.CheckInTime,
    CASE 
        WHEN b.CheckInTime IS NOT NULL THEN 'CheckedIn'
        WHEN GETUTCDATE() BETWEEN b.StartTime AND b.EndTime THEN 'Active'
        WHEN GETUTCDATE() < b.StartTime THEN 'Upcoming'
        ELSE 'Past'
    END AS CurrentStatus
FROM Booking_Bookings b
WHERE b.Status IN ('Confirmed', 'CheckedIn')
    AND b.EndTime >= DATEADD(DAY, -1, GETUTCDATE());
GO

-- Function: Get User Booking Count for Week
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'FN' AND name = 'fn_Booking_GetUserBookingCountForWeek')
    DROP FUNCTION fn_Booking_GetUserBookingCountForWeek;
GO

CREATE FUNCTION fn_Booking_GetUserBookingCountForWeek(
    @TenantId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER,
    @WeekStartDate DATE
)
RETURNS INT
AS
BEGIN
    DECLARE @Count INT;
    
    SELECT @Count = COUNT(DISTINCT CAST(StartTime AS DATE))
    FROM Booking_Bookings
    WHERE TenantId = @TenantId
        AND UserId = @UserId
        AND Status IN ('Confirmed', 'CheckedIn')
        AND StartTime >= @WeekStartDate
        AND StartTime < DATEADD(DAY, 7, @WeekStartDate);
    
    RETURN ISNULL(@Count, 0);
END
GO

-- Function: Check Resource Availability
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'FN' AND name = 'fn_Booking_IsResourceAvailable')
    DROP FUNCTION fn_Booking_IsResourceAvailable;
GO

CREATE FUNCTION fn_Booking_IsResourceAvailable(
    @TenantId UNIQUEIDENTIFIER,
    @ResourceId UNIQUEIDENTIFIER,
    @StartTime DATETIME2,
    @EndTime DATETIME2
)
RETURNS BIT
AS
BEGIN
    DECLARE @IsAvailable BIT = 1;
    
    IF EXISTS (
        SELECT 1
        FROM Booking_Bookings
        WHERE TenantId = @TenantId
            AND ResourceId = @ResourceId
            AND Status IN ('Confirmed', 'CheckedIn')
            AND (
                (@StartTime >= StartTime AND @StartTime < EndTime)
                OR (@EndTime > StartTime AND @EndTime <= EndTime)
                OR (@StartTime <= StartTime AND @EndTime >= EndTime)
            )
    )
    BEGIN
        SET @IsAvailable = 0;
    END
    
    RETURN @IsAvailable;
END
GO

-- =============================================
-- SECTION 8: STORED PROCEDURES
-- =============================================

-- Procedure: Create Booking with Validation
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_Booking_CreateBooking')
    DROP PROCEDURE sp_Booking_CreateBooking;
GO

CREATE PROCEDURE sp_Booking_CreateBooking
    @TenantId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER,
    @ResourceId UNIQUEIDENTIFIER,
    @ResourceType NVARCHAR(50),
    @StartTime DATETIME2,
    @EndTime DATETIME2,
    @Notes NVARCHAR(500) = NULL,
    @BookingId UNIQUEIDENTIFIER OUTPUT,
    @ErrorMessage NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Check if resource is available
        IF dbo.fn_Booking_IsResourceAvailable(@TenantId, @ResourceId, @StartTime, @EndTime) = 0
        BEGIN
            SET @ErrorMessage = 'Resource is not available for the selected time slot';
            ROLLBACK TRANSACTION;
            RETURN 1;
        END
        
        -- Create booking
        SET @BookingId = NEWID();
        
        INSERT INTO Booking_Bookings (
            Id, TenantId, UserId, ResourceId, ResourceType,
            StartTime, EndTime, Status, Notes
        )
        VALUES (
            @BookingId, @TenantId, @UserId, @ResourceId, @ResourceType,
            @StartTime, @EndTime, 'Confirmed', @Notes
        );
        
        -- Log history
        INSERT INTO Booking_History (
            BookingId, TenantId, Action, ChangedBy, NewStatus, Timestamp
        )
        VALUES (
            @BookingId, @TenantId, 'Created', @UserId, 'Confirmed', GETUTCDATE()
        );
        
        COMMIT TRANSACTION;
        SET @ErrorMessage = NULL;
        RETURN 0;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        SET @ErrorMessage = ERROR_MESSAGE();
        RETURN 1;
    END CATCH
END
GO

-- Procedure: Cancel Booking
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_Booking_CancelBooking')
    DROP PROCEDURE sp_Booking_CancelBooking;
GO

CREATE PROCEDURE sp_Booking_CancelBooking
    @BookingId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER,
    @Reason NVARCHAR(500),
    @ErrorMessage NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        DECLARE @OldStatus NVARCHAR(50);
        DECLARE @TenantId UNIQUEIDENTIFIER;
        
        SELECT @OldStatus = Status, @TenantId = TenantId
        FROM Booking_Bookings
        WHERE Id = @BookingId;
        
        IF @OldStatus IS NULL
        BEGIN
            SET @ErrorMessage = 'Booking not found';
            ROLLBACK TRANSACTION;
            RETURN 1;
        END
        
        IF @OldStatus = 'Cancelled'
        BEGIN
            SET @ErrorMessage = 'Booking is already cancelled';
            ROLLBACK TRANSACTION;
            RETURN 1;
        END
        
        -- Update booking
        UPDATE Booking_Bookings
        SET Status = 'Cancelled',
            CancellationReason = @Reason,
            UpdatedAt = GETUTCDATE()
        WHERE Id = @BookingId;
        
        -- Log history
        INSERT INTO Booking_History (
            BookingId, TenantId, Action, ChangedBy, OldStatus, NewStatus, Details, Timestamp
        )
        VALUES (
            @BookingId, @TenantId, 'Cancelled', @UserId, @OldStatus, 'Cancelled', @Reason, GETUTCDATE()
        );
        
        COMMIT TRANSACTION;
        SET @ErrorMessage = NULL;
        RETURN 0;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        SET @ErrorMessage = ERROR_MESSAGE();
        RETURN 1;
    END CATCH
END
GO

-- =============================================
-- SECTION 9: PERFORMANCE INDEXES
-- =============================================

-- Index for finding overlapping bookings
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Booking_Bookings_Overlap_Check')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Booking_Bookings_Overlap_Check
    ON Booking_Bookings (TenantId, ResourceId, StartTime, EndTime)
    INCLUDE (Status)
    WHERE Status IN ('Confirmed', 'CheckedIn');
END
GO

-- Index for user booking history
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Booking_Bookings_UserHistory')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Booking_Bookings_UserHistory
    ON Booking_Bookings (UserId, StartTime DESC)
    INCLUDE (ResourceId, ResourceType, Status);
END
GO

-- Index for searching available desks
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Resource_Desks_Available_Search')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Resource_Desks_Available_Search
    ON Resource_Desks (TenantId, FloorId, IsAvailable, IsBookable)
    INCLUDE (DeskNumber, Type, ZoneId);
END
GO

-- =============================================
-- SECTION 10: INITIAL DATA SEEDING
-- =============================================

-- Seed default notification templates (Greek)
IF NOT EXISTS (SELECT * FROM Notification_Templates WHERE TemplateType = 'BookingConfirmation' AND Language = 'el' AND TenantId = '00000000-0000-0000-0000-000000000000')
BEGIN
    INSERT INTO Notification_Templates (Id, TenantId, TemplateType, Channel, Language, Subject, BodyTemplate)
    VALUES (
        NEWID(),
        '00000000-0000-0000-0000-000000000000', -- System default
        'BookingConfirmation',
        'Email',
        'el',
        'Επιβεβαίωση Κράτησης',
        '<html><body><h2>Η κράτησή σας επιβεβαιώθηκε</h2><p>Αγαπητέ/ή χρήστη,</p><p>Η κράτησή σας έχει επιβεβαιωθεί με επιτυχία.</p><ul><li><strong>Πόρος:</strong> {{ResourceName}}</li><li><strong>Ημερομηνία:</strong> {{Date}}</li><li><strong>Ώρα:</strong> {{Time}}</li><li><strong>Τοποθεσία:</strong> {{Location}}</li></ul><p>Παρακαλούμε να κάνετε check-in την ημέρα της κράτησής σας.</p></body></html>'
    );
END
GO

-- Seed default notification templates (English)
IF NOT EXISTS (SELECT * FROM Notification_Templates WHERE TemplateType = 'BookingConfirmation' AND Language = 'en' AND TenantId = '00000000-0000-0000-0000-000000000000')
BEGIN
    INSERT INTO Notification_Templates (Id, TenantId, TemplateType, Channel, Language, Subject, BodyTemplate)
    VALUES (
        NEWID(),
        '00000000-0000-0000-0000-000000000000',
        'BookingConfirmation',
        'Email',
        'en',
        'Booking Confirmation',
        '<html><body><h2>Your Booking is Confirmed</h2><p>Dear User,</p><p>Your booking has been successfully confirmed.</p><ul><li><strong>Resource:</strong> {{ResourceName}}</li><li><strong>Date:</strong> {{Date}}</li><li><strong>Time:</strong> {{Time}}</li><li><strong>Location:</strong> {{Location}}</li></ul><p>Please remember to check in on the day of your booking.</p></body></html>'
    );
END
GO

-- =============================================
-- SECTION 11: SECURITY AND PERMISSIONS
-- =============================================

-- Create application user for the unified database
USE master;
GO

IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'WorkSpaceManagerAppUser')
BEGIN
    CREATE LOGIN WorkSpaceManagerAppUser WITH PASSWORD = 'YourStrongPassword123!';
END
GO

USE WorkSpaceManager;
GO

IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'WorkSpaceManagerAppUser')
BEGIN
    CREATE USER WorkSpaceManagerAppUser FOR LOGIN WorkSpaceManagerAppUser;
    ALTER ROLE db_datareader ADD MEMBER WorkSpaceManagerAppUser;
    ALTER ROLE db_datawriter ADD MEMBER WorkSpaceManagerAppUser;
    GRANT EXECUTE ON SCHEMA::dbo TO WorkSpaceManagerAppUser;
END
GO

-- =============================================
-- SCRIPT COMPLETION
-- =============================================

PRINT '========================================';
PRINT 'WorkSpaceManager Unified Database Schema';
PRINT 'Installation Complete';
PRINT '========================================';
PRINT '';
PRINT 'Created Database: WorkSpaceManager';
PRINT '';
PRINT 'Table Prefixes by Service:';
PRINT '  - Identity_*     (Identity Service)';
PRINT '  - Resource_*     (Resource Service)';
PRINT '  - Booking_*      (Booking Service)';
PRINT '  - Rules_*        (Rules Service)';
PRINT '  - Notification_* (Notification Service)';
PRINT '';
PRINT 'Total Tables: 30+';
PRINT 'Views: 1';
PRINT 'Functions: 2';
PRINT 'Stored Procedures: 2';
PRINT '';
PRINT 'Next Steps:';
PRINT '  1. Update connection strings in all microservices';
PRINT '  2. Change default password for WorkSpaceManagerAppUser';
PRINT '  3. Configure Keycloak for Identity management';
PRINT '  4. Seed initial tenant and user data';
PRINT '  5. Test API endpoints with the unified database';
PRINT '';
PRINT '========================================';
GO
