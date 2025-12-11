-- =============================================
-- WorkSpaceManager: Seed Data Script
-- Purpose: Populate database with realistic test data
-- Database: WorkSpaceManager (Unified)
-- =============================================

USE WorkSpaceManager;
GO

PRINT 'Starting seed data insertion...';
GO

-- =============================================
-- 1. IDENTITY SERVICE: Tenants
-- =============================================

PRINT 'Inserting Tenants...';

DECLARE @Tenant1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Tenant2Id UNIQUEIDENTIFIER = NEWID();

INSERT INTO Identity_Tenants (Id, Name, Code, KeycloakRealmId, IsActive, SubscriptionTier, MaxUsers, CreatedAt, UpdatedAt)
VALUES 
    (@Tenant1Id, 'Alpha Bank', 'ALPHA', 'alpha-bank-realm', 1, 'Enterprise', 500, GETUTCDATE(), GETUTCDATE()),
    (@Tenant2Id, 'Beta Corporation', 'BETA', 'beta-corp-realm', 1, 'Standard', 100, GETUTCDATE(), GETUTCDATE());

PRINT 'Tenants inserted: 2';
GO

-- =============================================
-- 2. IDENTITY SERVICE: User Metadata
-- =============================================

PRINT 'Inserting User Metadata...';

DECLARE @Tenant1Id UNIQUEIDENTIFIER = (SELECT Id FROM Identity_Tenants WHERE Code = 'ALPHA');

DECLARE @User1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @User2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @User3Id UNIQUEIDENTIFIER = NEWID();
DECLARE @User4Id UNIQUEIDENTIFIER = NEWID();
DECLARE @User5Id UNIQUEIDENTIFIER = NEWID();

INSERT INTO Identity_UserMetadata (Id, TenantId, KeycloakUserId, EmployeeId, Department, JobTitle, ManagerId, PreferredLanguage, NoShowCount, LastLoginAt, CreatedAt, UpdatedAt)
VALUES 
    (@User1Id, @Tenant1Id, 'keycloak-user-001', 'EMP001', 'IT', 'Senior Developer', NULL, 'el', 0, GETUTCDATE(), GETUTCDATE(), GETUTCDATE()),
    (@User2Id, @Tenant1Id, 'keycloak-user-002', 'EMP002', 'IT', 'Junior Developer', @User1Id, 'en', 0, GETUTCDATE(), GETUTCDATE(), GETUTCDATE()),
    (@User3Id, @Tenant1Id, 'keycloak-user-003', 'EMP003', 'HR', 'HR Manager', NULL, 'el', 1, GETUTCDATE(), GETUTCDATE(), GETUTCDATE()),
    (@User4Id, @Tenant1Id, 'keycloak-user-004', 'EMP004', 'Finance', 'Financial Analyst', NULL, 'en', 0, GETUTCDATE(), GETUTCDATE(), GETUTCDATE()),
    (@User5Id, @Tenant1Id, 'keycloak-user-005', 'EMP005', 'IT', 'DevOps Engineer', @User1Id, 'el', 0, GETUTCDATE(), GETUTCDATE(), GETUTCDATE());

PRINT 'User Metadata inserted: 5';
GO

-- =============================================
-- 3. RESOURCE SERVICE: Buildings
-- =============================================

PRINT 'Inserting Buildings...';

DECLARE @Tenant1Id UNIQUEIDENTIFIER = (SELECT Id FROM Identity_Tenants WHERE Code = 'ALPHA');

DECLARE @Building1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Building2Id UNIQUEIDENTIFIER = NEWID();

INSERT INTO Resource_Buildings (Id, TenantId, Name, Code, Street, City, PostalCode, Country, Latitude, Longitude, TimeZone, IsActive, CreatedAt, UpdatedAt)
VALUES 
    (@Building1Id, @Tenant1Id, 'Headquarters Athens', 'HQ-ATH', 'Panepistimiou 10', 'Athens', '10671', 'Greece', 37.9838, 23.7275, 'Europe/Athens', 1, GETUTCDATE(), GETUTCDATE()),
    (@Building2Id, @Tenant1Id, 'Branch Office Thessaloniki', 'BR-THK', 'Tsimiski 45', 'Thessaloniki', '54623', 'Greece', 40.6401, 22.9444, 'Europe/Athens', 1, GETUTCDATE(), GETUTCDATE());

PRINT 'Buildings inserted: 2';
GO

-- =============================================
-- 4. RESOURCE SERVICE: Floors
-- =============================================

PRINT 'Inserting Floors...';

DECLARE @Tenant1Id UNIQUEIDENTIFIER = (SELECT Id FROM Identity_Tenants WHERE Code = 'ALPHA');
DECLARE @Building1Id UNIQUEIDENTIFIER = (SELECT Id FROM Resource_Buildings WHERE Code = 'HQ-ATH');

DECLARE @Floor1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Floor2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Floor3Id UNIQUEIDENTIFIER = NEWID();

INSERT INTO Resource_Floors (Id, TenantId, BuildingId, FloorNumber, Name, TotalArea, IsActive, CreatedAt, UpdatedAt)
VALUES 
    (@Floor1Id, @Tenant1Id, @Building1Id, 1, 'Ground Floor', 1200.50, 1, GETUTCDATE(), GETUTCDATE()),
    (@Floor2Id, @Tenant1Id, @Building1Id, 2, 'First Floor', 1200.50, 1, GETUTCDATE(), GETUTCDATE()),
    (@Floor3Id, @Tenant1Id, @Building1Id, 3, 'Second Floor', 1200.50, 1, GETUTCDATE(), GETUTCDATE());

PRINT 'Floors inserted: 3';
GO

-- =============================================
-- 5. RESOURCE SERVICE: Floor Plans
-- =============================================

PRINT 'Inserting Floor Plans...';

DECLARE @Tenant1Id UNIQUEIDENTIFIER = (SELECT Id FROM Identity_Tenants WHERE Code = 'ALPHA');
DECLARE @Floor1Id UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Resource_Floors WHERE FloorNumber = 1);
DECLARE @Floor2Id UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Resource_Floors WHERE FloorNumber = 2);

INSERT INTO Resource_FloorPlans (Id, TenantId, FloorId, ImageUrl, ImageWidth, ImageHeight, MappingJson, Version, UploadedAt, UploadedBy, IsActive)
VALUES 
    (NEWID(), @Tenant1Id, @Floor1Id, '/uploads/floorplans/floor1.png', 1920, 1080, 
     '{"desks":[{"id":"D101","x":100,"y":200},{"id":"D102","x":300,"y":200}],"rooms":[{"id":"R101","x":800,"y":400}]}', 
     1, GETUTCDATE(), 'admin@alphabank.gr', 1),
    (NEWID(), @Tenant1Id, @Floor2Id, '/uploads/floorplans/floor2.png', 1920, 1080, 
     '{"desks":[{"id":"D201","x":150,"y":250},{"id":"D202","x":350,"y":250}],"rooms":[{"id":"R201","x":850,"y":450}]}', 
     1, GETUTCDATE(), 'admin@alphabank.gr', 1);

PRINT 'Floor Plans inserted: 2';
GO

-- =============================================
-- 6. RESOURCE SERVICE: Zones
-- =============================================

PRINT 'Inserting Zones...';

DECLARE @Tenant1Id UNIQUEIDENTIFIER = (SELECT Id FROM Identity_Tenants WHERE Code = 'ALPHA');
DECLARE @Floor2Id UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Resource_Floors WHERE FloorNumber = 2);

DECLARE @Zone1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Zone2Id UNIQUEIDENTIFIER = NEWID();

INSERT INTO Resource_Zones (Id, TenantId, FloorId, Name, Code, Department, Color, IsActive, CreatedAt)
VALUES 
    (@Zone1Id, @Tenant1Id, @Floor2Id, 'IT Zone', 'IT-Z1', 'IT', '#3B82F6', 1, GETUTCDATE()),
    (@Zone2Id, @Tenant1Id, @Floor2Id, 'Finance Zone', 'FIN-Z1', 'Finance', '#10B981', 1, GETUTCDATE());

PRINT 'Zones inserted: 2';
GO

-- =============================================
-- 7. RESOURCE SERVICE: Desks
-- =============================================

PRINT 'Inserting Desks...';

DECLARE @Tenant1Id UNIQUEIDENTIFIER = (SELECT Id FROM Identity_Tenants WHERE Code = 'ALPHA');
DECLARE @Floor1Id UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Resource_Floors WHERE FloorNumber = 1);
DECLARE @Floor2Id UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Resource_Floors WHERE FloorNumber = 2);
DECLARE @Zone1Id UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Resource_Zones WHERE Code = 'IT-Z1');

DECLARE @Desk1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Desk2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Desk3Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Desk4Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Desk5Id UNIQUEIDENTIFIER = NEWID();

INSERT INTO Resource_Desks (Id, TenantId, FloorId, ZoneId, DeskNumber, CoordinateX, CoordinateY, Type, IsAvailable, IsBookable, Amenities, Notes, CreatedAt, UpdatedAt)
VALUES 
    (@Desk1Id, @Tenant1Id, @Floor1Id, NULL, 'D101', 100, 200, 'Standard', 1, 1, 'Monitor,Keyboard,Mouse', NULL, GETUTCDATE(), GETUTCDATE()),
    (@Desk2Id, @Tenant1Id, @Floor1Id, NULL, 'D102', 300, 200, 'Standard', 1, 1, 'Monitor,Keyboard,Mouse', NULL, GETUTCDATE(), GETUTCDATE()),
    (@Desk3Id, @Tenant1Id, @Floor2Id, @Zone1Id, 'D201', 150, 250, 'Standing', 1, 1, 'Monitor,Keyboard,Mouse,Standing Desk', 'Height adjustable', GETUTCDATE(), GETUTCDATE()),
    (@Desk4Id, @Tenant1Id, @Floor2Id, @Zone1Id, 'D202', 350, 250, 'Standard', 1, 1, 'Monitor,Keyboard,Mouse', NULL, GETUTCDATE(), GETUTCDATE()),
    (@Desk5Id, @Tenant1Id, @Floor2Id, @Zone1Id, 'D203', 550, 250, 'Executive', 0, 0, 'Dual Monitor,Keyboard,Mouse,Ergonomic Chair', 'Reserved for management', GETUTCDATE(), GETUTCDATE());

PRINT 'Desks inserted: 5';
GO

-- =============================================
-- 8. RESOURCE SERVICE: Meeting Rooms
-- =============================================

PRINT 'Inserting Meeting Rooms...';

DECLARE @Tenant1Id UNIQUEIDENTIFIER = (SELECT Id FROM Identity_Tenants WHERE Code = 'ALPHA');
DECLARE @Floor1Id UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Resource_Floors WHERE FloorNumber = 1);
DECLARE @Floor2Id UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Resource_Floors WHERE FloorNumber = 2);

DECLARE @Room1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Room2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Room3Id UNIQUEIDENTIFIER = NEWID();

INSERT INTO Resource_MeetingRooms (Id, TenantId, FloorId, ZoneId, RoomNumber, Name, Capacity, CoordinateX, CoordinateY, Equipment, IsAvailable, IsBookable, RequiresApproval, Notes, CreatedAt, UpdatedAt)
VALUES 
    (@Room1Id, @Tenant1Id, @Floor1Id, NULL, 'R101', 'Conference Room A', 10, 800, 400, 'Projector,Whiteboard,Video Conference', 1, 1, 0, NULL, GETUTCDATE(), GETUTCDATE()),
    (@Room2Id, @Tenant1Id, @Floor2Id, NULL, 'R201', 'Meeting Room B', 6, 850, 450, 'TV Screen,Whiteboard', 1, 1, 0, NULL, GETUTCDATE(), GETUTCDATE()),
    (@Room3Id, @Tenant1Id, @Floor2Id, NULL, 'R202', 'Executive Boardroom', 20, 1200, 600, 'Projector,Video Conference,Whiteboard,Sound System', 1, 1, 1, 'Requires VP approval', GETUTCDATE(), GETUTCDATE());

PRINT 'Meeting Rooms inserted: 3';
GO

-- =============================================
-- 9. RESOURCE SERVICE: Special Spaces
-- =============================================

PRINT 'Inserting Special Spaces...';

DECLARE @Tenant1Id UNIQUEIDENTIFIER = (SELECT Id FROM Identity_Tenants WHERE Code = 'ALPHA');
DECLARE @Building1Id UNIQUEIDENTIFIER = (SELECT Id FROM Resource_Buildings WHERE Code = 'HQ-ATH');

INSERT INTO Resource_SpecialSpaces (Id, TenantId, BuildingId, FloorId, SpaceType, SpaceNumber, Name, IsAvailable, IsBookable, Notes, CreatedAt, UpdatedAt)
VALUES 
    (NEWID(), @Tenant1Id, @Building1Id, NULL, 'Parking', 'P001', 'Parking Spot 1', 1, 1, 'Underground parking', GETUTCDATE(), GETUTCDATE()),
    (NEWID(), @Tenant1Id, @Building1Id, NULL, 'Parking', 'P002', 'Parking Spot 2', 1, 1, 'Underground parking', GETUTCDATE(), GETUTCDATE()),
    (NEWID(), @Tenant1Id, @Building1Id, NULL, 'Locker', 'L001', 'Locker 1', 1, 1, NULL, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), @Tenant1Id, @Building1Id, NULL, 'Locker', 'L002', 'Locker 2', 1, 1, NULL, GETUTCDATE(), GETUTCDATE());

PRINT 'Special Spaces inserted: 4';
GO

-- =============================================
-- 10. RULES SERVICE: Policy Configurations
-- =============================================

PRINT 'Inserting Policy Configurations...';

DECLARE @Tenant1Id UNIQUEIDENTIFIER = (SELECT Id FROM Identity_Tenants WHERE Code = 'ALPHA');

DECLARE @Policy1Id UNIQUEIDENTIFIER = NEWID();

INSERT INTO Rules_PolicyConfigurations (
    Id, TenantId, Name, Description, IsActive,
    MaxDaysPerWeek, MaxDaysPerMonth, MaxAdvanceBookingDays, MinAdvanceBookingHours,
    RestrictToDepartment, RestrictToBuilding, AllowRecurringBookings, MaxRecurringWeeks,
    NoShowThreshold, NoShowPenaltyDays, RequireCheckIn, CheckInWindowMinutes,
    MinCancellationHours, AllowSameDayCancellation,
    CreatedAt, UpdatedAt
)
VALUES (
    @Policy1Id, @Tenant1Id, 'Standard Booking Policy', 'Default policy for all users', 1,
    3, 12, 14, 2,
    0, 0, 1, 8,
    3, 7, 1, 30,
    2, 1,
    GETUTCDATE(), GETUTCDATE()
);

PRINT 'Policy Configurations inserted: 1';
GO

-- =============================================
-- 11. RULES SERVICE: Custom Rules
-- =============================================

PRINT 'Inserting Custom Rules...';

DECLARE @Tenant1Id UNIQUEIDENTIFIER = (SELECT Id FROM Identity_Tenants WHERE Code = 'ALPHA');
DECLARE @Policy1Id UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Rules_PolicyConfigurations);

INSERT INTO Rules_CustomRules (Id, TenantId, PolicyId, RuleName, RuleType, Condition, Message, Severity, IsActive, Priority, CreatedAt)
VALUES 
    (NEWID(), @Tenant1Id, @Policy1Id, 'No Weekend Bookings', 'TimeRestriction', 
     '{"type":"DayOfWeek","values":[0,6]}', 
     'Bookings are not allowed on weekends', 'Error', 1, 1, GETUTCDATE()),
    (NEWID(), @Tenant1Id, @Policy1Id, 'Max 2 Hours Meeting Room', 'DurationLimit', 
     '{"resourceType":"MeetingRoom","maxHours":2}', 
     'Meeting room bookings cannot exceed 2 hours', 'Warning', 1, 2, GETUTCDATE());

PRINT 'Custom Rules inserted: 2';
GO

-- =============================================
-- 12. BOOKING SERVICE: Sample Bookings
-- =============================================

PRINT 'Inserting Sample Bookings...';

DECLARE @Tenant1Id UNIQUEIDENTIFIER = (SELECT Id FROM Identity_Tenants WHERE Code = 'ALPHA');
DECLARE @User1Id UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Identity_UserMetadata WHERE EmployeeId = 'EMP001');
DECLARE @User2Id UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Identity_UserMetadata WHERE EmployeeId = 'EMP002');
DECLARE @Desk1Id UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Resource_Desks WHERE DeskNumber = 'D101');
DECLARE @Desk2Id UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Resource_Desks WHERE DeskNumber = 'D201');
DECLARE @Room1Id UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Resource_MeetingRooms WHERE RoomNumber = 'R101');

DECLARE @Booking1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Booking2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Booking3Id UNIQUEIDENTIFIER = NEWID();

-- Tomorrow's desk booking
INSERT INTO Booking_Bookings (Id, TenantId, UserId, ResourceId, ResourceType, StartTime, EndTime, Status, IsRecurring, RecurrencePattern, RecurrenceEndDate, ParentBookingId, CancellationReason, CheckInTime, CheckOutTime, Notes, CreatedAt, UpdatedAt)
VALUES 
    (@Booking1Id, @Tenant1Id, @User1Id, @Desk1Id, 'Desk', 
     DATEADD(DAY, 1, CAST(CAST(GETUTCDATE() AS DATE) AS DATETIME) + CAST('08:00:00' AS TIME)), 
     DATEADD(DAY, 1, CAST(CAST(GETUTCDATE() AS DATE) AS DATETIME) + CAST('17:00:00' AS TIME)), 
     'Confirmed', 0, NULL, NULL, NULL, NULL, NULL, NULL, 'Working on project X', GETUTCDATE(), GETUTCDATE());

-- Next week's desk booking
INSERT INTO Booking_Bookings (Id, TenantId, UserId, ResourceId, ResourceType, StartTime, EndTime, Status, IsRecurring, RecurrencePattern, RecurrenceEndDate, ParentBookingId, CancellationReason, CheckInTime, CheckOutTime, Notes, CreatedAt, UpdatedAt)
VALUES 
    (@Booking2Id, @Tenant1Id, @User2Id, @Desk2Id, 'Desk', 
     DATEADD(DAY, 7, CAST(CAST(GETUTCDATE() AS DATE) AS DATETIME) + CAST('09:00:00' AS TIME)), 
     DATEADD(DAY, 7, CAST(CAST(GETUTCDATE() AS DATE) AS DATETIME) + CAST('18:00:00' AS TIME)), 
     'Confirmed', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, GETUTCDATE(), GETUTCDATE());

-- Tomorrow's meeting room booking
INSERT INTO Booking_Bookings (Id, TenantId, UserId, ResourceId, ResourceType, StartTime, EndTime, Status, IsRecurring, RecurrencePattern, RecurrenceEndDate, ParentBookingId, CancellationReason, CheckInTime, CheckOutTime, Notes, CreatedAt, UpdatedAt)
VALUES 
    (@Booking3Id, @Tenant1Id, @User1Id, @Room1Id, 'MeetingRoom', 
     DATEADD(DAY, 1, CAST(CAST(GETUTCDATE() AS DATE) AS DATETIME) + CAST('14:00:00' AS TIME)), 
     DATEADD(DAY, 1, CAST(CAST(GETUTCDATE() AS DATE) AS DATETIME) + CAST('15:30:00' AS TIME)), 
     'Confirmed', 0, NULL, NULL, NULL, NULL, NULL, NULL, 'Sprint planning meeting', GETUTCDATE(), GETUTCDATE());

PRINT 'Bookings inserted: 3';
GO

-- =============================================
-- 13. BOOKING SERVICE: Participants
-- =============================================

PRINT 'Inserting Booking Participants...';

DECLARE @Booking3Id UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Booking_Bookings WHERE ResourceType = 'MeetingRoom');
DECLARE @User1Id UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Identity_UserMetadata WHERE EmployeeId = 'EMP001');
DECLARE @User2Id UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Identity_UserMetadata WHERE EmployeeId = 'EMP002');
DECLARE @User5Id UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Identity_UserMetadata WHERE EmployeeId = 'EMP005');

INSERT INTO Booking_Participants (Id, BookingId, UserId, IsOrganizer, ResponseStatus, AddedAt)
VALUES 
    (NEWID(), @Booking3Id, @User1Id, 1, 'Accepted', GETUTCDATE()),
    (NEWID(), @Booking3Id, @User2Id, 0, 'Accepted', GETUTCDATE()),
    (NEWID(), @Booking3Id, @User5Id, 0, 'Pending', GETUTCDATE());

PRINT 'Booking Participants inserted: 3';
GO

-- =============================================
-- 14. NOTIFICATION SERVICE: Templates
-- =============================================

PRINT 'Inserting Notification Templates...';

DECLARE @Tenant1Id UNIQUEIDENTIFIER = (SELECT Id FROM Identity_Tenants WHERE Code = 'ALPHA');

INSERT INTO Notification_Templates (Id, TenantId, TemplateType, Channel, Language, Subject, BodyTemplate, IsActive, CreatedAt, UpdatedAt)
VALUES 
    -- Greek templates
    (NEWID(), @Tenant1Id, 'BookingConfirmation', 'Email', 'el', 
     'Επιβεβαίωση Κράτησης', 
     'Αγαπητέ/ή {{UserName}},<br><br>Η κράτησή σας για {{ResourceType}} {{ResourceName}} επιβεβαιώθηκε.<br>Ημερομηνία: {{Date}}<br>Ώρα: {{StartTime}} - {{EndTime}}<br><br>Ευχαριστούμε!', 
     1, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), @Tenant1Id, 'BookingReminder', 'Push', 'el', 
     'Υπενθύμιση Κράτησης', 
     'Η κράτησή σας για {{ResourceName}} ξεκινά σε 1 ώρα.', 
     1, GETUTCDATE(), GETUTCDATE()),
    -- English templates
    (NEWID(), @Tenant1Id, 'BookingConfirmation', 'Email', 'en', 
     'Booking Confirmation', 
     'Dear {{UserName}},<br><br>Your booking for {{ResourceType}} {{ResourceName}} has been confirmed.<br>Date: {{Date}}<br>Time: {{StartTime}} - {{EndTime}}<br><br>Thank you!', 
     1, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), @Tenant1Id, 'BookingReminder', 'Push', 'en', 
     'Booking Reminder', 
     'Your booking for {{ResourceName}} starts in 1 hour.', 
     1, GETUTCDATE(), GETUTCDATE());

PRINT 'Notification Templates inserted: 4';
GO

-- =============================================
-- 15. VERIFICATION QUERIES
-- =============================================

PRINT '';
PRINT '==============================================';
PRINT 'SEED DATA SUMMARY';
PRINT '==============================================';

SELECT 'Tenants' AS Entity, COUNT(*) AS Count FROM Identity_Tenants
UNION ALL
SELECT 'Users', COUNT(*) FROM Identity_UserMetadata
UNION ALL
SELECT 'Buildings', COUNT(*) FROM Resource_Buildings
UNION ALL
SELECT 'Floors', COUNT(*) FROM Resource_Floors
UNION ALL
SELECT 'Zones', COUNT(*) FROM Resource_Zones
UNION ALL
SELECT 'Desks', COUNT(*) FROM Resource_Desks
UNION ALL
SELECT 'Meeting Rooms', COUNT(*) FROM Resource_MeetingRooms
UNION ALL
SELECT 'Special Spaces', COUNT(*) FROM Resource_SpecialSpaces
UNION ALL
SELECT 'Policy Configurations', COUNT(*) FROM Rules_PolicyConfigurations
UNION ALL
SELECT 'Custom Rules', COUNT(*) FROM Rules_CustomRules
UNION ALL
SELECT 'Bookings', COUNT(*) FROM Booking_Bookings
UNION ALL
SELECT 'Booking Participants', COUNT(*) FROM Booking_Participants
UNION ALL
SELECT 'Notification Templates', COUNT(*) FROM Notification_Templates;

PRINT '';
PRINT 'Seed data insertion completed successfully!';
PRINT '==============================================';
GO
