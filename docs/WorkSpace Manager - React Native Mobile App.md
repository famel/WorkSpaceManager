# WorkSpace Manager - React Native Mobile App

Production-ready mobile application for iOS and Android built with React Native and Expo.

## 📱 Features

### Authentication
- ✅ OAuth 2.0 login with Keycloak
- ✅ Biometric authentication (Face ID / Touch ID / Fingerprint)
- ✅ Secure token storage
- ✅ Automatic token refresh

### Booking Management
- ✅ View all bookings
- ✅ Create new bookings (desks and meeting rooms)
- ✅ Check-in to bookings
- ✅ Check-out from bookings
- ✅ Cancel bookings
- ✅ Real-time booking status

### Dashboard
- ✅ Statistics overview
- ✅ Upcoming bookings
- ✅ Quick actions
- ✅ Pull-to-refresh

### Profile
- ✅ User information
- ✅ Role management
- ✅ Biometric settings
- ✅ App information
- ✅ Logout

## 🏗️ Tech Stack

- **Framework:** React Native 0.72 + Expo 49
- **Language:** TypeScript
- **Navigation:** React Navigation 6
- **HTTP Client:** Axios
- **Authentication:** Expo Auth Session
- **Secure Storage:** Expo Secure Store
- **Biometrics:** Expo Local Authentication

## 📋 Prerequisites

- Node.js 18+
- npm or yarn
- Expo CLI (`npm install -g expo-cli`)
- iOS Simulator (Mac only) or Android Emulator
- Expo Go app (for physical device testing)

## 🚀 Quick Start

### 1. Install Dependencies

```bash
cd src/MobileApp
npm install
```

### 2. Configure Environment

Edit `src/config/env.ts`:

```typescript
export const ENV = {
  API_URL: 'http://YOUR_API_GATEWAY_URL:5000',
  KEYCLOAK_URL: 'http://YOUR_KEYCLOAK_URL:8080',
  KEYCLOAK_REALM: 'alpha-bank-realm',
  KEYCLOAK_CLIENT_ID: 'workspace-manager-mobile',
  KEYCLOAK_REDIRECT_URI: 'workspacemanager://oauth',
  APP_NAME: 'WorkSpace Manager',
  APP_VERSION: '1.0.0',
};
```

**Important:** 
- For iOS Simulator: Use `http://localhost:5000`
- For Android Emulator: Use `http://10.0.2.2:5000`
- For Physical Device: Use your computer's local IP (e.g., `http://192.168.1.100:5000`)

### 3. Start Development Server

```bash
npm start
```

This will start the Expo development server and show a QR code.

### 4. Run on Device/Simulator

**iOS Simulator (Mac only):**
```bash
npm run ios
```

**Android Emulator:**
```bash
npm run android
```

**Physical Device:**
1. Install Expo Go app from App Store / Play Store
2. Scan the QR code from the terminal
3. App will load on your device

## 📁 Project Structure

```
MobileApp/
├── App.tsx                      # Main app entry point
├── app.json                     # Expo configuration
├── package.json                 # Dependencies
├── tsconfig.json                # TypeScript config
└── src/
    ├── config/
    │   └── env.ts              # Environment configuration
    ├── navigation/
    │   └── AppNavigator.tsx    # Navigation setup
    ├── screens/
    │   ├── LoginScreen.tsx     # Login screen
    │   ├── DashboardScreen.tsx # Dashboard
    │   ├── BookingsScreen.tsx  # Bookings list
    │   ├── CreateBookingScreen.tsx # Create booking
    │   └── ProfileScreen.tsx   # User profile
    └── services/
        ├── authService.ts      # Authentication
        ├── apiClient.ts        # API client
        ├── bookingService.ts   # Booking API
        └── spaceService.ts     # Space management API
```

## 🔐 Authentication Flow

1. User taps "Sign In with Keycloak"
2. App opens Keycloak login page in browser
3. User enters credentials
4. Keycloak redirects back to app with authorization code
5. App exchanges code for access token
6. Token is stored securely
7. User is authenticated

### Biometric Authentication

After first login, users can enable biometric authentication:

1. Go to Profile tab
2. Enable "Biometric Login" toggle
3. Authenticate with biometric
4. On next app launch, use biometric to login

## 📱 Screens

### Login Screen
- OAuth 2.0 login with Keycloak
- Biometric login option (if enabled)
- Beautiful gradient design

### Dashboard
- Welcome message
- Statistics cards (upcoming, confirmed, types)
- Quick actions (Book Space, My Bookings)
- Upcoming bookings preview
- Pull-to-refresh

### Bookings Screen
- List of all user bookings
- Status badges (Pending, Confirmed, CheckedIn, etc.)
- Check-in button (30 min before - 1 hour after start)
- Check-out button (when checked in)
- Cancel button (for pending/confirmed bookings)
- Floating action button to create booking

### Create Booking Screen
- Resource type selector (Desk / Meeting Room)
- Resource ID input
- Start date & time
- End date & time
- Purpose (optional)
- Notes (optional)
- Form validation

### Profile Screen
- User avatar and name
- Email address
- User roles
- Biometric settings
- App information
- Logout button

## 🎨 Design System

### Colors
- **Primary:** `#667eea` (Purple-blue)
- **Success:** `#10b981` (Green)
- **Warning:** `#f59e0b` (Orange)
- **Danger:** `#ef4444` (Red)
- **Info:** `#3b82f6` (Blue)
- **Background:** `#f7fafc` (Light gray)
- **Text:** `#1a202c` (Dark gray)

### Typography
- **Heading:** 24-32px, Bold
- **Title:** 18-20px, Semibold
- **Body:** 14-16px, Regular
- **Caption:** 12px, Regular

### Components
- Card-based layouts
- Rounded corners (8-12px)
- Subtle shadows
- Status badges
- Icon buttons
- Pull-to-refresh
- Loading states
- Empty states

## 🔧 Configuration

### Keycloak Setup

Ensure the mobile client is configured in Keycloak:

1. Client ID: `workspace-manager-mobile`
2. Client Type: Public
3. Valid Redirect URIs: `workspacemanager://*`
4. Web Origins: `*`
5. Access Type: Public
6. Standard Flow Enabled: Yes
7. Direct Access Grants Enabled: No

### Deep Linking

The app uses the custom scheme `workspacemanager://` for OAuth redirects.

This is configured in `app.json`:
```json
{
  "expo": {
    "scheme": "workspacemanager"
  }
}
```

## 📦 Build for Production

### iOS

```bash
expo build:ios
```

Requirements:
- Apple Developer account
- iOS distribution certificate
- Provisioning profile

### Android

```bash
expo build:android
```

Requirements:
- Google Play Developer account
- Keystore file

### Using EAS Build (Recommended)

```bash
# Install EAS CLI
npm install -g eas-cli

# Configure project
eas build:configure

# Build for iOS
eas build --platform ios

# Build for Android
eas build --platform android
```

## 🧪 Testing

### Manual Testing

1. **Login Flow:**
   - Test OAuth login
   - Test biometric login
   - Test logout

2. **Booking Flow:**
   - Create booking
   - View bookings
   - Check-in to booking
   - Check-out from booking
   - Cancel booking

3. **Navigation:**
   - Test all tab navigation
   - Test screen transitions
   - Test back navigation

4. **Error Handling:**
   - Test with invalid credentials
   - Test with network errors
   - Test with expired tokens

### Test Credentials

Use the same credentials as the web app:
- **Admin:** admin@alphabank.gr / Admin@123
- **User:** user1@alphabank.gr / User@123

## 🐛 Troubleshooting

### Cannot connect to API

**Problem:** App cannot reach the API Gateway

**Solutions:**
- Check `API_URL` in `src/config/env.ts`
- For Android Emulator, use `http://10.0.2.2:5000`
- For iOS Simulator, use `http://localhost:5000`
- For Physical Device, use your computer's local IP
- Ensure API Gateway is running
- Check firewall settings

### OAuth redirect not working

**Problem:** After login, app doesn't redirect back

**Solutions:**
- Verify `KEYCLOAK_REDIRECT_URI` matches Keycloak client config
- Check Keycloak client "Valid Redirect URIs" includes `workspacemanager://*`
- Restart Expo development server

### Biometric not available

**Problem:** Biometric option not showing

**Solutions:**
- Ensure device has biometric hardware
- Check device settings for enrolled biometrics
- For simulators, biometric may not be available

### Token expired errors

**Problem:** Getting 401 errors frequently

**Solutions:**
- Token refresh is automatic
- Check Keycloak token lifespans
- Ensure refresh token is valid
- Re-login if refresh token expired

## 📱 Platform-Specific Notes

### iOS
- Requires Xcode for building
- Biometric: Face ID / Touch ID
- Minimum iOS version: 13.0

### Android
- Requires Android Studio for building
- Biometric: Fingerprint / Face unlock
- Minimum Android version: 5.0 (API 21)

## 🔒 Security

- Tokens stored in Expo Secure Store (encrypted)
- HTTPS recommended for production
- Biometric authentication for quick access
- Automatic token refresh
- Logout clears all stored data

## 📈 Performance

- Lazy loading of screens
- Optimized images
- Minimal re-renders
- Efficient API calls
- Pull-to-refresh for data updates

## 🚀 Deployment

### App Store (iOS)

1. Build with `expo build:ios` or `eas build --platform ios`
2. Download IPA file
3. Upload to App Store Connect
4. Submit for review

### Google Play (Android)

1. Build with `expo build:android` or `eas build --platform android`
2. Download APK/AAB file
3. Upload to Google Play Console
4. Submit for review

## 📄 License

Proprietary - WorkSpace Manager Project

## 🆘 Support

For issues or questions:
1. Check this README
2. Review the API documentation
3. Check Expo documentation: https://docs.expo.dev
4. Check React Navigation docs: https://reactnavigation.org

---

**Version:** 1.0.0  
**Last Updated:** December 2024  
**Status:** Production Ready ✅
