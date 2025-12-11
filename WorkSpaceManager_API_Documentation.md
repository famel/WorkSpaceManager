# WorkSpaceManager: API Documentation & Integration Guide

This document provides detailed API endpoint specifications and integration examples for all microservices in the WorkSpaceManager system.

---

## Table of Contents

1. [Authentication & Authorization](#authentication--authorization)
2. [Identity Service API](#identity-service-api)
3. [Resource Service API](#resource-service-api)
4. [Booking Service API](#booking-service-api)
5. [Rules Service API](#rules-service-api)
6. [Notification Service API](#notification-service-api)
7. [Integration Examples](#integration-examples)
8. [Error Handling](#error-handling)

---

## Authentication & Authorization

All API endpoints require JWT authentication obtained from Keycloak.

### Obtaining Access Token

**Endpoint:** `POST http://localhost:8080/realms/{realm}/protocol/openid-connect/token`

**Request Body (x-www-form-urlencoded):**
```
grant_type=password
client_id=workspace-manager-web
username=user@example.com
password=userpassword
```

**Response:**
```json
{
  "access_token": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expires_in": 300,
  "refresh_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "token_type": "Bearer"
}
```

### Using the Token

Include the access token in the `Authorization` header for all API requests:

```
Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## Identity Service API

**Base URL:** `http://localhost:5000/api/identity` (via API Gateway)

### 1. Get Current User Profile

**Endpoint:** `GET /users/me`

**Headers:**
```
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "keycloakUserId": "keycloak-user-001",
  "employeeId": "EMP001",
  "email": "john.doe@alphabank.gr",
  "firstName": "John",
  "lastName": "Doe",
  "department": "IT",
  "jobTitle": "Senior Developer",
  "managerId": null,
  "preferredLanguage": "el",
  "noShowCount": 0,
  "lastLoginAt": "2025-12-09T10:30:00Z"
}
```

### 2. Update User Preferences

**Endpoint:** `PATCH /users/me/preferences`

**Request Body:**
```json
{
  "preferredLanguage": "en"
}
```

**Response (200 OK):**
```json
{
  "message": "Preferences updated successfully"
}
```

### 3. List Users (Admin Only)

**Endpoint:** `GET /users?department=IT&page=1&pageSize=20`

**Response (200 OK):**
```json
{
  "data": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "employeeId": "EMP001",
      "email": "john.doe@alphabank.gr",
      "fullName": "John Doe",
      "department": "IT",
      "jobTitle": "Senior Developer",
      "isActive": true
    }
  ],
  "totalCount": 45,
  "page": 1,
  "pageSize": 20
}
```

---

## Resource Service API

**Base URL:** `http://localhost:5000/api/resources` (via API Gateway)

### 1. List Buildings

**Endpoint:** `GET /buildings`

**Response (200 OK):**
```json
{
  "data": [
    {
      "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
      "name": "Headquarters Athens",
      "code": "HQ-ATH",
      "address": {
        "street": "Panepistimiou 10",
        "city": "Athens",
        "postalCode": "10671",
        "country": "Greece"
      },
      "coordinates": {
        "latitude": 37.9838,
        "longitude": 23.7275
      },
      "timeZone": "Europe/Athens",
      "floorCount": 3
    }
  ]
}
```

### 2. Get Floor Details with Floor Plan

**Endpoint:** `GET /floors/{floorId}`

**Response (200 OK):**
```json
{
  "id": "8d7e5679-8536-51ef-855c-f18gd2g01bf8",
  "buildingId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "floorNumber": 2,
  "name": "First Floor",
  "totalArea": 1200.50,
  "floorPlan": {
    "imageUrl": "/uploads/floorplans/floor2.png",
    "imageWidth": 1920,
    "imageHeight": 1080,
    "mapping": {
      "desks": [
        {
          "id": "D201",
          "x": 150,
          "y": 250
        }
      ],
      "rooms": [
        {
          "id": "R201",
          "x": 850,
          "y": 450
        }
      ]
    }
  },
  "zones": [
    {
      "id": "9e8f6780-9647-62fg-966d-g29he3h12cg9",
      "name": "IT Zone",
      "code": "IT-Z1",
      "department": "IT",
      "color": "#3B82F6"
    }
  ]
}
```

### 3. Search Available Desks

**Endpoint:** `GET /desks/available?floorId={floorId}&date=2025-12-10&startTime=09:00&endTime=17:00`

**Response (200 OK):**
```json
{
  "data": [
    {
      "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
      "deskNumber": "D201",
      "floorId": "8d7e5679-8536-51ef-855c-f18gd2g01bf8",
      "floorName": "First Floor",
      "zoneId": "9e8f6780-9647-62fg-966d-g29he3h12cg9",
      "zoneName": "IT Zone",
      "type": "Standing",
      "coordinates": {
        "x": 150,
        "y": 250
      },
      "amenities": ["Monitor", "Keyboard", "Mouse", "Standing Desk"],
      "isAvailable": true
    }
  ],
  "totalCount": 12
}
```

### 4. Get Meeting Room Availability

**Endpoint:** `GET /meeting-rooms/{roomId}/availability?date=2025-12-10`

**Response (200 OK):**
```json
{
  "roomId": "b2c3d4e5-f6g7-8901-bcde-fg2345678901",
  "roomNumber": "R201",
  "name": "Meeting Room B",
  "capacity": 6,
  "date": "2025-12-10",
  "availableSlots": [
    {
      "startTime": "09:00",
      "endTime": "10:00"
    },
    {
      "startTime": "10:00",
      "endTime": "11:00"
    },
    {
      "startTime": "14:00",
      "endTime": "15:00"
    }
  ],
  "bookedSlots": [
    {
      "startTime": "11:00",
      "endTime": "12:30",
      "bookingId": "c3d4e5f6-g7h8-9012-cdef-gh3456789012",
      "organizer": "John Doe"
    }
  ]
}
```

---

## Booking Service API

**Base URL:** `http://localhost:5000/api/bookings` (via API Gateway)

### 1. Create Desk Booking

**Endpoint:** `POST /bookings`

**Request Body:**
```json
{
  "resourceId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "resourceType": "Desk",
  "startTime": "2025-12-10T09:00:00Z",
  "endTime": "2025-12-10T17:00:00Z",
  "notes": "Working on project X",
  "isRecurring": false
}
```

**Response (201 Created):**
```json
{
  "id": "d4e5f6g7-h8i9-0123-defg-hi4567890123",
  "resourceId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "resourceType": "Desk",
  "resourceName": "D201",
  "startTime": "2025-12-10T09:00:00Z",
  "endTime": "2025-12-10T17:00:00Z",
  "status": "Confirmed",
  "qrCode": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAA...",
  "createdAt": "2025-12-09T12:00:00Z"
}
```

### 2. Create Recurring Booking

**Endpoint:** `POST /bookings`

**Request Body:**
```json
{
  "resourceId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "resourceType": "Desk",
  "startTime": "2025-12-10T09:00:00Z",
  "endTime": "2025-12-10T17:00:00Z",
  "isRecurring": true,
  "recurrencePattern": "Weekly",
  "recurrenceDays": [1, 2, 3],
  "recurrenceEndDate": "2026-01-31T23:59:59Z",
  "notes": "Regular office days"
}
```

**Response (201 Created):**
```json
{
  "parentBookingId": "e5f6g7h8-i9j0-1234-efgh-ij5678901234",
  "instancesCreated": 12,
  "instances": [
    {
      "id": "f6g7h8i9-j0k1-2345-fghi-jk6789012345",
      "date": "2025-12-10",
      "startTime": "09:00",
      "endTime": "17:00"
    }
  ]
}
```

### 3. Create Meeting Room Booking with Participants

**Endpoint:** `POST /bookings`

**Request Body:**
```json
{
  "resourceId": "b2c3d4e5-f6g7-8901-bcde-fg2345678901",
  "resourceType": "MeetingRoom",
  "startTime": "2025-12-10T14:00:00Z",
  "endTime": "2025-12-10T15:30:00Z",
  "notes": "Sprint planning meeting",
  "participants": [
    {
      "userId": "g7h8i9j0-k1l2-3456-ghij-kl7890123456",
      "isOrganizer": false
    },
    {
      "userId": "h8i9j0k1-l2m3-4567-hijk-lm8901234567",
      "isOrganizer": false
    }
  ],
  "syncToOutlook": true
}
```

**Response (201 Created):**
```json
{
  "id": "i9j0k1l2-m3n4-5678-ijkl-mn9012345678",
  "resourceId": "b2c3d4e5-f6g7-8901-bcde-fg2345678901",
  "resourceType": "MeetingRoom",
  "resourceName": "Meeting Room B",
  "startTime": "2025-12-10T14:00:00Z",
  "endTime": "2025-12-10T15:30:00Z",
  "status": "Confirmed",
  "participants": [
    {
      "userId": "g7h8i9j0-k1l2-3456-ghij-kl7890123456",
      "fullName": "Jane Smith",
      "responseStatus": "Pending"
    }
  ],
  "outlookEventId": "AAMkAGI2TG93AAA=",
  "createdAt": "2025-12-09T12:00:00Z"
}
```

### 4. Get My Bookings

**Endpoint:** `GET /bookings/my?status=Confirmed&from=2025-12-01&to=2025-12-31`

**Response (200 OK):**
```json
{
  "data": [
    {
      "id": "d4e5f6g7-h8i9-0123-defg-hi4567890123",
      "resourceType": "Desk",
      "resourceName": "D201",
      "floorName": "First Floor",
      "buildingName": "Headquarters Athens",
      "startTime": "2025-12-10T09:00:00Z",
      "endTime": "2025-12-10T17:00:00Z",
      "status": "Confirmed",
      "checkInStatus": "Pending",
      "qrCode": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAA..."
    }
  ],
  "totalCount": 8
}
```

### 5. Check-In to Booking

**Endpoint:** `POST /bookings/{bookingId}/check-in`

**Request Body:**
```json
{
  "method": "QR",
  "deviceId": "mobile-device-123",
  "location": "Building HQ-ATH, Floor 2"
}
```

**Response (200 OK):**
```json
{
  "message": "Check-in successful",
  "checkInTime": "2025-12-10T09:05:00Z"
}
```

### 6. Cancel Booking

**Endpoint:** `DELETE /bookings/{bookingId}`

**Request Body:**
```json
{
  "reason": "Meeting rescheduled"
}
```

**Response (200 OK):**
```json
{
  "message": "Booking cancelled successfully",
  "refundEligible": true
}
```

---

## Rules Service API

**Base URL:** `http://localhost:5000/api/rules` (via API Gateway)

### 1. Validate Booking Request

**Endpoint:** `POST /validate`

**Request Body:**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "resourceId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "resourceType": "Desk",
  "startTime": "2025-12-10T09:00:00Z",
  "endTime": "2025-12-10T17:00:00Z",
  "isRecurring": false
}
```

**Response (200 OK - Valid):**
```json
{
  "isValid": true,
  "violations": [],
  "warnings": []
}
```

**Response (200 OK - Invalid):**
```json
{
  "isValid": false,
  "violations": [
    {
      "ruleId": "max-days-per-week",
      "ruleName": "Maximum Days Per Week",
      "message": "You have already booked 3 days this week. Maximum allowed is 3.",
      "severity": "Error"
    }
  ],
  "warnings": [
    {
      "ruleId": "no-show-warning",
      "ruleName": "No-Show Warning",
      "message": "You have 2 no-shows. One more will result in a 7-day booking suspension.",
      "severity": "Warning"
    }
  ]
}
```

### 2. Get Policy Configuration

**Endpoint:** `GET /policies/active`

**Response (200 OK):**
```json
{
  "id": "j0k1l2m3-n4o5-6789-jklm-no0123456789",
  "name": "Standard Booking Policy",
  "maxDaysPerWeek": 3,
  "maxDaysPerMonth": 12,
  "maxAdvanceBookingDays": 14,
  "minAdvanceBookingHours": 2,
  "restrictToDepartment": false,
  "allowRecurringBookings": true,
  "maxRecurringWeeks": 8,
  "noShowThreshold": 3,
  "requireCheckIn": true,
  "checkInWindowMinutes": 30
}
```

---

## Notification Service API

**Base URL:** `http://localhost:5000/api/notifications` (via API Gateway)

### 1. Get My Notifications

**Endpoint:** `GET /notifications/my?unreadOnly=true&page=1&pageSize=20`

**Response (200 OK):**
```json
{
  "data": [
    {
      "id": "k1l2m3n4-o5p6-7890-klmn-op1234567890",
      "type": "BookingReminder",
      "title": "Booking Reminder",
      "message": "Your booking for D201 starts in 1 hour.",
      "isRead": false,
      "createdAt": "2025-12-10T08:00:00Z"
    }
  ],
  "unreadCount": 3,
  "totalCount": 15
}
```

### 2. Mark Notification as Read

**Endpoint:** `PATCH /notifications/{notificationId}/read`

**Response (200 OK):**
```json
{
  "message": "Notification marked as read"
}
```

### 3. Register Push Device

**Endpoint:** `POST /devices`

**Request Body:**
```json
{
  "deviceToken": "ExponentPushToken[xxxxxxxxxxxxxxxxxxxxxx]",
  "platform": "ios",
  "deviceId": "iphone-12-pro-max"
}
```

**Response (201 Created):**
```json
{
  "message": "Device registered successfully"
}
```

---

## Integration Examples

### Example 1: Complete Booking Flow (React Web Client)

```typescript
// 1. Search for available desks
const searchDesks = async (floorId: string, date: string) => {
  const response = await fetch(
    `${API_BASE_URL}/api/resources/desks/available?` +
    `floorId=${floorId}&date=${date}&startTime=09:00&endTime=17:00`,
    {
      headers: {
        'Authorization': `Bearer ${accessToken}`,
        'Content-Type': 'application/json'
      }
    }
  );
  return await response.json();
};

// 2. Validate booking before creation
const validateBooking = async (bookingData: BookingRequest) => {
  const response = await fetch(`${API_BASE_URL}/api/rules/validate`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${accessToken}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(bookingData)
  });
  return await response.json();
};

// 3. Create booking if valid
const createBooking = async (bookingData: BookingRequest) => {
  // First validate
  const validation = await validateBooking(bookingData);
  
  if (!validation.isValid) {
    throw new Error(validation.violations[0].message);
  }
  
  // Then create
  const response = await fetch(`${API_BASE_URL}/api/bookings`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${accessToken}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(bookingData)
  });
  
  if (!response.ok) {
    throw new Error('Failed to create booking');
  }
  
  return await response.json();
};

// Usage
try {
  const booking = await createBooking({
    resourceId: 'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
    resourceType: 'Desk',
    startTime: '2025-12-10T09:00:00Z',
    endTime: '2025-12-10T17:00:00Z',
    notes: 'Working on project X'
  });
  
  console.log('Booking created:', booking.id);
} catch (error) {
  console.error('Booking failed:', error.message);
}
```

### Example 2: QR Code Check-In (React Native Mobile App)

```typescript
import { BarCodeScanner } from 'expo-barcode-scanner';

const CheckInScreen = () => {
  const handleBarCodeScanned = async ({ data }: { data: string }) => {
    // data contains booking ID from QR code
    try {
      const response = await fetch(
        `${API_BASE_URL}/api/bookings/${data}/check-in`,
        {
          method: 'POST',
          headers: {
            'Authorization': `Bearer ${accessToken}`,
            'Content-Type': 'application/json'
          },
          body: JSON.stringify({
            method: 'QR',
            deviceId: await getDeviceId(),
            location: await getCurrentLocation()
          })
        }
      );
      
      if (response.ok) {
        Alert.alert('Success', 'Check-in successful!');
      } else {
        const error = await response.json();
        Alert.alert('Error', error.message);
      }
    } catch (error) {
      Alert.alert('Error', 'Failed to check in');
    }
  };
  
  return (
    <BarCodeScanner
      onBarCodeScanned={handleBarCodeScanned}
      style={StyleSheet.absoluteFillObject}
    />
  );
};
```

### Example 3: Real-Time Notifications (SignalR)

```typescript
import * as signalR from '@microsoft/signalr';

const connection = new signalR.HubConnectionBuilder()
  .withUrl(`${API_BASE_URL}/hubs/notifications`, {
    accessTokenFactory: () => accessToken
  })
  .withAutomaticReconnect()
  .build();

connection.on('BookingConfirmed', (booking) => {
  console.log('New booking confirmed:', booking);
  showNotification('Booking Confirmed', booking.resourceName);
});

connection.on('BookingReminder', (booking) => {
  console.log('Booking reminder:', booking);
  showNotification('Reminder', `Your booking starts in 1 hour`);
});

await connection.start();
```

---

## Error Handling

### Standard Error Response Format

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "ResourceId": [
      "The ResourceId field is required."
    ]
  },
  "traceId": "00-3fa85f6457174562b3fc2c963f66afa6-b3fc2c963f66afa6-00"
}
```

### HTTP Status Codes

| Code | Meaning | Usage |
|------|---------|-------|
| 200 | OK | Successful GET, PATCH, DELETE |
| 201 | Created | Successful POST |
| 400 | Bad Request | Validation errors |
| 401 | Unauthorized | Missing or invalid token |
| 403 | Forbidden | Insufficient permissions |
| 404 | Not Found | Resource not found |
| 409 | Conflict | Resource already exists or booking conflict |
| 422 | Unprocessable Entity | Business rule violation |
| 500 | Internal Server Error | Server error |

### Common Error Scenarios

**Booking Conflict:**
```json
{
  "status": 409,
  "title": "Booking Conflict",
  "detail": "The selected desk is already booked for the requested time slot.",
  "conflictingBookingId": "l2m3n4o5-p6q7-8901-lmno-pq2345678901"
}
```

**Rule Violation:**
```json
{
  "status": 422,
  "title": "Policy Violation",
  "detail": "You have exceeded the maximum number of bookings allowed per week.",
  "violations": [
    {
      "ruleId": "max-days-per-week",
      "message": "Maximum 3 days per week allowed"
    }
  ]
}
```

---

**Document Version:** 1.0  
**Last Updated:** December 2025  
**Author:** Manus AI
