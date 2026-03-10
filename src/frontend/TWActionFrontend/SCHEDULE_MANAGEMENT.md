# Schedule Management System Documentation

## Overview

The system allows authenticated users to:

- View their schedules
- Create new schedules
- Edit existing schedules
- Delete schedules
- Logout from the system

## Project Structure

### TypeScript Types

**`types/schedule.ts`** - Type definitions for schedules:

- `WorldType` - enum of worlds (pl218-pl223)
- `ScheduleType` - enum of schedule types (Fake, Reconnaissance, Main)
- `Schedule` - schedule interface
- `CreateScheduleRequest` - request for creating
- `UpdateScheduleRequest` - request for updating

**`types/user.ts`** - Type definitions for user

### API Services

**`services/scheduleService.ts`** - Handles all CRUD operations on schedules:

- `getSchedules()` - fetch current user's schedules
- `getScheduleById(scheduleId)` - fetch single schedule
- `createSchedule(request)` - create new schedule
- `updateSchedule(scheduleId, request)` - update schedule
- `deleteSchedule(scheduleId)` - delete schedule

**`services/authService.ts`** - Handles authentication:

- `getMe()` - fetch logged-in user data
- `logout()` - logout
- `redirectToGoogleLogin()` - redirect to Google login

### Custom Hooks

**`hooks/useAuth.ts`** - Hook for managing user session:

- Automatically checks session state on mount
- Provides login/logout methods
- Manages loading and error states
- Returns authentication status

**`hooks/useActiveSchedule.ts`** - Hook for managing active schedule:

- Stores active schedule ID in localStorage
- Provides methods to set/clear active schedule
- Persists selection across sessions

### Components

**`components/ScheduleForm.tsx`** - Form for creating/editing schedule:

- Name validation (required)
- World selection from select
- Schedule type selection
- Loading state handling
- Error display

**`components/ScheduleList.tsx`** - Schedule list:

- Display all user's schedules
- Creation date formatting
- Edit and delete buttons
- Confirmation before deletion
- Empty list state
- Visual indication of active schedule
- Button to set schedule as active

### Pages

**`pages/HomePage.tsx`** - Main application page:

- Login screen for unauthenticated users
- Schedule panel for authenticated users
- Handles all CRUD operations
- Form state management (create/edit)
- Active schedule management

## Application Flow

### 1. Login

```
Unauthenticated user → "Sign in with Google" button
→ Redirect to auth/google → After login return to app
→ useAuth hook automatically fetches user data
```

### 2. Displaying Schedules

```
HomePage mount → useAuth fetches user
→ useEffect detects user.id → loadSchedules()
→ scheduleService.getSchedules() → Update schedules state
→ ScheduleList renders list with active schedule highlighted
```

### 3. Creating Schedule

```
"New schedule" button → setShowForm(true)
→ Render ScheduleForm → Fill form
→ Submit → handleCreateSchedule() → scheduleService.createSchedule()
→ Add to state → Close form
```

### 4. Editing Schedule

```
"Edit" button → setEditingSchedule() + setShowForm(true)
→ ScheduleForm with schedule data → Modification
→ Submit → handleUpdateSchedule() → scheduleService.updateSchedule()
→ Update in state → Close form
```

### 5. Deleting Schedule

```
"Delete" button → Confirmation (confirm)
→ handleDeleteSchedule() → scheduleService.deleteSchedule()
→ Remove from state
```

### 6. Setting Active Schedule

```
"Set as active" button → onSetActive(scheduleId)
→ useActiveSchedule stores ID in localStorage
→ Visual update: blue border, gradient background, "ACTIVE" badge
```

### 7. Logout

```
"Logout" button → logout() from useAuth
→ authService.logout() → Clear user state
→ Redirect to login screen
```

## Best Practices Applied

### React Best Practices

1. **Functional Components & Hooks** - All components as functions with hooks
2. **Custom Hooks** - `useAuth` encapsulates authentication logic, `useActiveSchedule` for active schedule
3. **Controlled Components** - Forms use controlled inputs
4. **Proper State Management** - Local state for UI, services for API, localStorage for persistence
5. **Error Handling** - Try-catch in all async operations
6. **Loading States** - Loading indicators for better UX
7. **TypeScript** - Full typing for type safety
8. **CSS Modules** - Isolated styles for each component

### Session Management

1. **Automatic Session Check** - useAuth checks session on mount
2. **Credentials Handling** - `withCredentials: true` in axios for cookies
3. **Error Recovery** - Graceful handling of authentication errors
4. **Conditional Rendering** - Different views for authenticated/unauthenticated

### Code Structure

1. **Separation of Concerns** - Services, hooks, components separated
2. **Reusable Components** - ScheduleForm for both create and edit
3. **Type Safety** - Type-only imports where required
4. **Clean Architecture** - Business logic separated from UI

### UI/UX Design

1. **Dark Theme** - Modern dark color scheme with CSS variables
2. **Consistent Design** - Unified styling across all components
3. **Visual Feedback** - Hover effects, transitions, loading states
4. **Accessibility** - Focus states, keyboard navigation
5. **Semantic Colors** - Blue for primary actions, green for success, red for danger

## API Endpoints (Backend)

```
GET    /schedules                       - Get current user's schedules
GET    /schedules/{scheduleId}          - Get single schedule
GET    /schedules/admin/{userId}        - Get schedules for user (admin only)
POST   /schedules                       - Create new schedule
PUT    /schedules/{scheduleId}          - Update schedule
DELETE /schedules/{scheduleId}          - Delete schedule
```

## Backend Configuration

**Enum Serialization** - Backend configured to serialize enums as strings (not numbers):

```csharp
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
```

This ensures that `WorldType` and `ScheduleType` are sent as strings like `"pl218"` and `"Fake"` instead of numeric values.

## Testing

1. **Check login** - Redirect to Google auth
2. **Check empty list** - "No schedules" message
3. **Create schedule** - Form, validation, save
4. **Edit schedule** - Load data, modification
5. **Delete schedule** - Confirmation, deletion
6. **Set active schedule** - Visual indication, persistence across reload
7. **Logout** - Return to login screen

## Potential Extensions

- Pagination for large number of schedules
- Filtering and sorting
- Schedule search
- Schedule details (separate page)
- Schedule sharing
- Import/Export schedules
- Schedule templates
- Bulk operations
- Schedule history/versioning
