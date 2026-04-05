# Testing Guide - Auth Frontend (Fase 2)

This document provides step-by-step manual testing instructions for the authentication system implemented in Phases 1-4.

## Prerequisites

- Backend API running at `https://localhost:7096` (or configured VITE_API_URL)
- Frontend running at `http://localhost:5173` (or configured port)
- Browser DevTools console open for debugging
- Test user credentials available (ask backend developer)

## Test Environment Setup

### Starting the Application

```bash
# Terminal 1 - Backend (from Management/ folder)
dotnet run --project src/API/API.csproj

# Terminal 2 - Frontend (from Management/apps/admin-panel/ folder)
npm run dev
```

### Clear Browser State

Before testing, clear localStorage to ensure clean state:

1. Open DevTools → Application tab → Local Storage
2. Right-click → Clear all
3. Refresh the page

---

## Test Cases

### 5.1 Login with Valid Credentials → Redirects to Dashboard

**Objective:** Verify successful login redirects to dashboard

**Steps:**
1. Navigate to `http://localhost:5173/login`
2. Enter a valid email (e.g., `admin@rokeystore.com`)
3. Enter the correct password
4. Click "Iniciar Sesión"
5. Observe the redirect

**Expected Results:**
- Button shows "Ingresando..." with spinner during submission
- After successful login, page redirects to `/dashboard`
- DashboardLayout shows the navigation sidebar with user name
- Console shows no errors

**Verification Points:**
- [ ] Spinner appears on button during login
- [ ] Button is disabled during submission
- [ ] Redirects to `/dashboard` after success
- [ ] User name appears in header (e.g., "Juan Pérez")

---

### 5.2 Login with Invalid Email → Shows "Email inválido"

**Objective:** Verify Zod validation catches invalid email format

**Steps:**
1. Navigate to `http://localhost:5173/login`
2. Enter an invalid email (e.g., `not-an-email`, `test@`, `test`)
3. Enter any password (6+ characters)
4. Click "Iniciar Sesión"
5. Observe validation message

**Expected Results:**
- Form does NOT submit (no API call made)
- Error message appears below email field: "Email inválido"
- Form fields remain enabled
- No toast notification appears

**Verification Points:**
- [ ] Error message "Email inválido" appears
- [ ] Message is red/below the email field
- [ ] No API call made (check Network tab)
- [ ] Form remains interactive

---

### 5.3 Login with Short Password → Shows Password Validation Error

**Objective:** Verify Zod validation catches short passwords

**Steps:**
1. Navigate to `http://localhost:5173/login`
2. Enter a valid email (e.g., `admin@rokeystore.com`)
3. Enter a short password (e.g., "12345" - less than 6 characters)
4. Click "Iniciar Sesión"
5. Observe validation message

**Expected Results:**
- Form does NOT submit (no API call made)
- Error message appears below password field: "La contraseña debe tener al menos 6 caracteres"
- Form fields remain enabled

**Verification Points:**
- [ ] Error message appears below password field
- [ ] Message shows the 6-character minimum requirement
- [ ] No API call made
- [ ] Form remains interactive

---

### 5.4 Login with Wrong Credentials → Shows "Credenciales inválidas" Toast

**Objective:** Verify API error 401 shows proper toast notification

**Steps:**
1. Navigate to `http://localhost:5173/login`
2. Enter a valid email format but wrong credentials
   - Email: `admin@rokeystore.com`
   - Password: `wrongpassword123`
3. Click "Iniciar Sesión"
4. Observe the toast notification

**Expected Results:**
- Button shows spinner during API call
- After 401 response, Sonner toast appears in top-right
- Toast message: "Credenciales inválidas"
- Toast type: error (red)
- Form remains on login page

**Verification Points:**
- [ ] Spinner appears on button during submission
- [ ] "Credenciales inválidas" toast appears
- [ ] Toast is error-style (red)
- [ ] User stays on /login page
- [ ] No local error message (toast replaces local error display)

---

### 5.5 Login Shows Spinner and Disables Form During Submission

**Objective:** Verify loading state prevents double-submission

**Steps:**
1. Navigate to `http://localhost:5173/login`
2. Enter valid credentials
3. Click "Iniciar Sesión"
4. Immediately try to click again or change form fields

**Expected Results:**
- Button changes to "Ingresando..." with spinner icon
- Both input fields become disabled
- Clicking the button again does nothing
- Changing input values is prevented (fields are read-only during submission)

**Verification Points:**
- [ ] Button shows "Ingresando..." text
- [ ] Loader2 spinner icon appears in button
- [ ] Email input is disabled
- [ ] Password input is disabled
- [ ] Button is disabled (cursor: not-allowed)
- [ ] Cannot double-click to submit again

---

### 5.6 Click Logout → Calls API, Clears State, Redirects to /login

**Objective:** Verify logout flow works end-to-end

**Steps:**
1. Login with valid credentials (must be authenticated first)
2. Verify you're on the dashboard
3. Click the logout button in the top-right header
4. Observe the behavior

**Expected Results:**
- Logout button shows spinner briefly
- API call to `/api/v1/auth/logout` is made
- Local state (token, user) is cleared
- Page redirects to `/login`
- Attempting to access protected route redirects to login

**Verification Points:**
- [ ] Logout button shows spinner while logging out
- [ ] Network tab shows POST /api/v1/auth/logout
- [ ] After logout, URL changes to /login
- [ ] authStore.isAuthenticated is false
- [ ] authStore.user is null
- [ ] Try navigating to /dashboard manually - should redirect to /login

**Testing the API failure scenario:**
- With backend down, click logout
- Should still redirect to /login (graceful degradation)
- Console shows warning: "Logout API call failed, proceeding with local logout"

---

### 5.7 Access /admin/users Without Auth → Redirects to /login

**Objective:** Verify ProtectedRoute guards protected routes

**Steps:**
1. Clear browser state (logout or incognito window)
2. Attempt to access a protected route directly:
   - Navigate to `http://localhost:5173/dashboard`
   - Or navigate to `http://localhost:5173/productos`
   - Or any route wrapped in ProtectedRoute
3. Observe the redirect

**Expected Results:**
- Page redirects to `/login`
- After successful login, user is redirected back to the originally requested route (e.g., /dashboard)

**Verification Points:**
- [ ] Accessing /dashboard without auth redirects to /login
- [ ] Accessing /productos without auth redirects to /login
- [ ] URL shows redirect to /login
- [ ] After login, user goes to the original destination (not always /dashboard)

**Note:** The `state={{ from: location }}` in ProtectedRoute ensures users return to their intended destination after login.

---

### 5.8 Expired Token Triggers Auto-Refresh and Retry

**Objective:** Verify 401 interceptor refreshes token automatically

**This test requires either:**
- **Option A:** Wait for token to expire (8-hour JWT)
- **Option B:** Manually expire the token in browser

**Option B - Manual Token Expiry:**
1. Login successfully
2. Open DevTools → Application → Local Storage
3. Find the token (search for "token" key)
4. Modify or delete the token value
5. Make an API request (e.g., refresh the page or click a menu item)

**Steps for testing:**
1. Login and authenticate
2. Trigger a protected API call (e.g., navigate to /productos)
3. If token is expired/missing, observe the refresh flow

**Expected Results:**
- If refresh token cookie is valid:
  - 401 response triggers refresh call to `/api/v1/auth/refresh`
  - New access token is received
  - Original request is retried automatically
  - User sees no interruption (transparent to user)
  - Check Network tab: you'll see the refresh call followed by the retry

- If refresh token is also expired/invalid:
  - Refresh call returns 401
  - User is logged out automatically
  - Redirected to /login
  - Toast may show "Sesión expirada" or similar

**Verification Points:**
- [ ] Network tab shows 401 on original request
- [ ] Network tab shows POST /api/v1/auth/refresh after 401
- [ ] Original request is retried with new token
- [ ] User doesn't see interruption (if refresh succeeds)
- [ ] User is redirected to /login (if refresh fails)

**Console debugging:**
- Look for: "Axios Error" with status 401
- Then: "POST /api/v1/auth/refresh"
- Then: original request retry

---

## Testing Checklist

Use this checklist to verify all test cases pass:

| # | Test Case | Status | Notes |
|---|-----------|--------|-------|
| 5.1 | Valid login → dashboard redirect | ⬜ | |
| 5.2 | Invalid email → "Email inválido" | ⬜ | |
| 5.3 | Short password → validation error | ⬜ | |
| 5.4 | Wrong credentials → "Credenciales inválidas" | ⬜ | |
| 5.5 | Loading state disables form | ⬜ | |
| 5.6 | Logout → API + redirect | ⬜ | |
| 5.7 | Unauthenticated → /login redirect | ⬜ | |
| 5.8 | Token refresh on 401 | ⬜ | |

---

## Known Limitations & Notes

### Assumptions

1. **Backend availability:** Tests assume the backend API is running and accessible
2. **Test data:** You need valid user credentials to test successful login
3. **Browser features:** Tests assume modern browser with localStorage and cookies enabled
4. **HTTPS only for cookies:** Refresh tokens use HttpOnly cookies, which require HTTPS or localhost

### Potential Issues

1. **CORS errors:** If frontend and backend run on different ports, ensure CORS is configured in backend
2. **Cookie issues:** Refresh token cookie may be blocked by browser privacy settings or third-party cookie restrictions
3. **Token timing:** The 8-hour expiry is long; for testing, you may need to manually expire the token

### Debugging Tips

- **Check Network tab:** See exact API calls and responses
- **Check Console:** Look for axios errors and state changes
- **Check Application tab:** Inspect localStorage for token and user data
- **Check Redux DevTools (if using):** Monitor authStore state changes

---

## File References

| File | Purpose |
|------|---------|
| `src/pages/auth/LoginPage.tsx` | Login form with RHF + Zod validation |
| `src/stores/authStore.ts` | Zustand auth state management |
| `src/lib/api/axios.ts` | Axios with refresh token interceptor |
| `src/components/layout/DashboardLayout.tsx` | Layout with logout button |
| `src/components/layout/ProtectedRoute.tsx` | Route guard component |
| `src/lib/schemas/login.schema.ts` | Zod validation schema |

---

## Next Steps

If all tests pass:
- ✅ Phase 5 complete
- Ready for `sdd-verify` to validate implementation

If tests fail:
- Note the failing test and error messages
- Return to implementation phases to fix issues
- Re-run tests after fixes