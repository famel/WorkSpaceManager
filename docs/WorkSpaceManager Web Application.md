# WorkSpaceManager Web Application

Modern React web application for managing workspace bookings and resources.

## Features

### User Features
- ✅ **Authentication** - Secure login with Keycloak
- ✅ **Dashboard** - Overview of bookings and quick actions
- ✅ **My Bookings** - View and manage personal bookings
- ✅ **Book a Space** - Reserve desks and meeting rooms
- ✅ **Check-in/Check-out** - Manage booking lifecycle
- ✅ **Browse Spaces** - Explore available workspaces

### Admin Features (admin, facility_manager roles)
- ✅ **Building Management** - CRUD operations for buildings
- ✅ **Desk Management** - Manage desk inventory
- ✅ **Meeting Room Management** - Manage meeting rooms

## Tech Stack

- **React 18** - UI framework
- **TypeScript** - Type safety
- **React Router 6** - Client-side routing
- **Axios** - HTTP client
- **JWT Decode** - Token handling
- **CSS3** - Modern styling

## Project Structure

```
src/
├── components/
│   ├── Auth/
│   │   ├── Login.tsx               # Login page
│   │   ├── Login.css
│   │   └── ProtectedRoute.tsx      # Route protection
│   ├── Layout/
│   │   ├── DashboardLayout.tsx     # Main layout with sidebar
│   │   └── DashboardLayout.css
│   ├── Dashboard/
│   │   ├── Dashboard.tsx           # Dashboard page
│   │   └── Dashboard.css
│   └── Bookings/
│       ├── MyBookings.tsx          # User bookings list
│       ├── MyBookings.css
│       ├── BookSpace.tsx           # Booking creation form
│       └── BookSpace.css
├── services/
│   ├── api.ts                      # API client configuration
│   ├── authService.ts              # Authentication service
│   ├── bookingService.ts           # Booking API client
│   └── spaceService.ts             # Space management API client
├── App.tsx                         # Main app component with routes
├── App.css                         # Global styles
└── index.tsx                       # App entry point
```

## Getting Started

### Prerequisites

- Node.js 18+ and npm
- API Gateway running on port 5000
- Keycloak running on port 8080

### Installation

```bash
cd src/WebApp

# Install dependencies
npm install

# Copy environment variables
cp .env.example .env

# Update .env with your configuration
```

### Environment Variables

Create a `.env` file:

```env
REACT_APP_API_URL=http://localhost:5000
REACT_APP_KEYCLOAK_URL=http://localhost:8080
REACT_APP_KEYCLOAK_REALM=alpha-bank-realm
REACT_APP_KEYCLOAK_CLIENT_ID=workspace-manager-web
```

### Development

```bash
# Start development server
npm start
```

**Access:** http://localhost:3000

### Build for Production

```bash
# Create production build
npm run build

# Build output in build/ directory
```

## API Integration

### API Services

**Authentication Service** (`authService.ts`)
- Login with username/password
- Token management and refresh
- User info extraction
- Role checking

**Booking Service** (`bookingService.ts`)
- Create, update, cancel bookings
- Get user bookings
- Check-in/check-out
- Search bookings
- Check availability

**Space Service** (`spaceService.ts`)
- Buildings CRUD
- Floors CRUD
- Desks CRUD + search
- Meeting Rooms CRUD + search

### API Client Configuration

The app uses Axios with interceptors for:
- Automatic JWT token attachment
- Token refresh on 401
- Error handling
- Request/response logging

## Authentication Flow

1. User enters credentials on login page
2. App requests token from Keycloak
3. Token stored in localStorage
4. Token attached to all API requests
5. Automatic refresh before expiration
6. Redirect to login on token expiration

## Routing

| Path | Component | Protection | Role |
|------|-----------|------------|------|
| `/login` | Login | Public | - |
| `/dashboard` | Dashboard | Protected | Any |
| `/bookings` | MyBookings | Protected | Any |
| `/book-space` | BookSpace | Protected | Any |
| `/spaces` | BrowseSpaces | Protected | Any |
| `/admin/buildings` | Buildings | Protected | admin, facility_manager |
| `/admin/desks` | Desks | Protected | admin, facility_manager |
| `/admin/meeting-rooms` | MeetingRooms | Protected | admin, facility_manager |

## Demo Credentials

**Admin User:**
- Username: `admin@alphabank.gr`
- Password: `Admin@123`

**Regular User:**
- Username: `user1@alphabank.gr`
- Password: `User@123`

## Components

### Login
- Username/password form
- Error handling
- Demo credentials display
- Gradient background design

### Dashboard
- Welcome message
- Statistics cards (upcoming bookings, confirmed, resource types)
- Quick action cards
- Upcoming bookings list

### My Bookings
- Grid layout of booking cards
- Status badges (Pending, Confirmed, CheckedIn, etc.)
- Check-in/check-out buttons
- Cancel booking functionality

### Book Space
- Resource type selector (Desk/Meeting Room)
- Resource dropdown with details
- Date/time pickers
- Purpose and notes fields
- Form validation

### Dashboard Layout
- Sidebar navigation
- User info display
- Role-based menu items
- Logout button
- Responsive design

## Styling

### Design System

**Colors:**
- Primary: `#667eea` (Purple-blue gradient)
- Success: `#48bb78` (Green)
- Danger: `#f56565` (Red)
- Background: `#f7fafc` (Light gray)

**Typography:**
- Headings: System font stack
- Body: 16px base size
- Weights: 400 (normal), 600 (semibold), 700 (bold)

**Spacing:**
- Base unit: 8px
- Common: 16px, 24px, 32px, 48px

**Shadows:**
- Cards: `0 1px 3px rgba(0, 0, 0, 0.1)`
- Hover: `0 4px 12px rgba(0, 0, 0, 0.15)`

## Responsive Design

- Desktop: Full sidebar + content
- Tablet: Collapsible sidebar
- Mobile: Stacked layout, full-width cards

## Error Handling

- API errors displayed in error messages
- Network errors caught and shown
- 401 errors trigger logout
- Form validation errors

## Performance

- Code splitting with React.lazy
- Memoization for expensive computations
- Debounced search inputs
- Optimized re-renders

## Security

- JWT tokens in localStorage
- Automatic token refresh
- Protected routes
- Role-based access control
- CORS configuration
- XSS protection

## Browser Support

- Chrome (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)

## Troubleshooting

### Cannot connect to API
- Verify API Gateway is running on port 5000
- Check REACT_APP_API_URL in .env
- Check CORS configuration

### Login fails
- Verify Keycloak is running on port 8080
- Check realm and client ID configuration
- Verify user credentials

### Token expired errors
- Check token refresh logic
- Verify Keycloak token lifespan settings
- Clear localStorage and login again

## Future Enhancements

- Real-time updates with SignalR
- Floor plan visualization
- QR code scanning
- Push notifications
- Dark mode
- Internationalization (i18n)
- Advanced search filters
- Calendar view for bookings
- Analytics dashboard

## Contributing

1. Follow the existing code structure
2. Use TypeScript for type safety
3. Add CSS files for component styles
4. Test with different user roles
5. Ensure responsive design

## License

Proprietary - WorkSpaceManager Project
