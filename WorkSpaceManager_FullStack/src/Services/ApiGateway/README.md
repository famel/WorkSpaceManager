# WorkSpaceManager API Gateway

Ocelot-based API Gateway for routing requests to microservices.

## Overview

The API Gateway provides a single entry point for all client applications to access the WorkSpaceManager microservices. It handles:

- **Request Routing** - Routes requests to appropriate microservices
- **Authentication** - JWT token validation with Keycloak
- **Rate Limiting** - Protects services from excessive requests
- **CORS** - Cross-origin resource sharing configuration
- **Health Checks** - Monitors downstream service health

## Architecture

```
Client Applications
        ↓
   API Gateway (Port 5000)
        ↓
    ┌───┴───┐
    ↓       ↓
Booking   Space Management
Service   Service
(5001)    (5002)
```

## Routes

### Booking Service Routes

| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/bookings` | Create booking |
| GET | `/api/bookings/{id}` | Get booking |
| GET | `/api/bookings/my-bookings` | Get user's bookings |
| GET | `/api/bookings/upcoming` | Get upcoming bookings |
| POST | `/api/bookings/search` | Search bookings |
| PUT | `/api/bookings/{id}` | Update booking |
| POST | `/api/bookings/{id}/cancel` | Cancel booking |
| POST | `/api/bookings/{id}/checkin` | Check in |
| POST | `/api/bookings/{id}/checkout` | Check out |
| POST | `/api/bookings/check-availability` | Check availability |

### Space Management Service Routes

**Buildings:**
| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/buildings` | Create building |
| GET | `/api/buildings` | List buildings |
| GET | `/api/buildings/{id}` | Get building |
| PUT | `/api/buildings/{id}` | Update building |
| DELETE | `/api/buildings/{id}` | Delete building |

**Floors:**
| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/floors` | Create floor |
| GET | `/api/floors` | List floors |
| GET | `/api/floors/{id}` | Get floor |
| PUT | `/api/floors/{id}` | Update floor |
| DELETE | `/api/floors/{id}` | Delete floor |

**Desks:**
| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/desks` | Create desk |
| GET | `/api/desks/{id}` | Get desk |
| POST | `/api/desks/search` | Search desks |
| PUT | `/api/desks/{id}` | Update desk |
| DELETE | `/api/desks/{id}` | Delete desk |

**Meeting Rooms:**
| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/meetingrooms` | Create meeting room |
| GET | `/api/meetingrooms/{id}` | Get meeting room |
| POST | `/api/meetingrooms/search` | Search meeting rooms |
| PUT | `/api/meetingrooms/{id}` | Update meeting room |
| DELETE | `/api/meetingrooms/{id}` | Delete meeting room |

### Health Check Routes

| Method | Path | Description |
|--------|------|-------------|
| GET | `/health` | Gateway health |
| GET | `/health/booking` | Booking service health |
| GET | `/health/space` | Space service health |

## Configuration

### Environment-Specific Configuration

**Development (localhost):**
- Uses `ocelot.Development.json`
- Booking Service: `localhost:5001`
- Space Service: `localhost:5002`

**Production (Docker):**
- Uses `ocelot.json`
- Booking Service: `booking-service:80`
- Space Service: `space-service:80`

### Authentication

All routes (except health checks) require JWT Bearer token authentication.

**Token Requirements:**
- Valid JWT token from Keycloak
- Must include `sub` (user ID) claim
- Must include `tenant_id` claim
- Must not be expired

**Example Request:**
```bash
curl -X GET http://localhost:5000/api/bookings/my-bookings \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### Rate Limiting

Rate limiting is enabled on the `/api/bookings` POST endpoint:
- **Limit:** 10 requests per second
- **Response:** HTTP 429 (Too Many Requests) when exceeded

## Running the Gateway

### Local Development

```bash
cd src/Services/ApiGateway

# Restore packages
dotnet restore

# Run the gateway
dotnet run
```

**Access:** http://localhost:5000

### With Docker

```bash
docker-compose up api-gateway
```

## Testing

### Test Gateway Health

```bash
curl http://localhost:5000/health
```

**Expected Response:**
```json
{
  "status": "Healthy",
  "service": "API Gateway",
  "timestamp": "2025-12-10T10:00:00Z"
}
```

### Test Service Health

```bash
# Booking service
curl http://localhost:5000/health/booking

# Space service
curl http://localhost:5000/health/space
```

### Test Routing

```bash
# Get buildings (requires auth token)
curl -X GET http://localhost:5000/api/buildings \
  -H "Authorization: Bearer YOUR_TOKEN"

# Get bookings (requires auth token)
curl -X GET http://localhost:5000/api/bookings/my-bookings \
  -H "Authorization: Bearer YOUR_TOKEN"
```

## Troubleshooting

### Common Issues

**1. 401 Unauthorized**
- Verify JWT token is valid
- Check token expiration
- Ensure Keycloak is running and accessible

**2. 404 Not Found**
- Verify the route exists in `ocelot.json`
- Check downstream service is running
- Verify path template matches

**3. 503 Service Unavailable**
- Check downstream service health
- Verify service host and port configuration
- Check network connectivity

**4. CORS Errors**
- Verify origin is in allowed origins list
- Check CORS policy configuration
- Ensure credentials are allowed if needed

### Logging

The gateway logs all requests and responses:

```
[API Gateway] GET /api/bookings/my-bookings
[API Gateway] Token validated - User: abc123, Tenant: xyz789
[API Gateway] GET /api/bookings/my-bookings - Status: 200
```

## Configuration Files

- `ocelot.json` - Production routing configuration
- `ocelot.Development.json` - Development routing configuration
- `appsettings.json` - Application settings
- `appsettings.Development.json` - Development settings
- `Program.cs` - Application startup and middleware
- `ApiGateway.csproj` - Project file

## Security Features

✅ JWT Bearer authentication
✅ Token validation with Keycloak
✅ Rate limiting
✅ CORS protection
✅ Request logging
✅ Health monitoring

## Performance

- **Throughput:** Handles thousands of requests per second
- **Latency:** Adds minimal overhead (~5-10ms)
- **Scalability:** Can be horizontally scaled
- **Reliability:** Automatic retry on failure (configured in Ocelot)

## Next Steps

1. Configure SSL/TLS for production
2. Add request/response caching
3. Implement circuit breaker pattern
4. Add distributed tracing
5. Configure load balancing for multiple service instances
