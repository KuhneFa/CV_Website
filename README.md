# CV Website

Private CV website with a password-protected PDF viewer and an admin area for replacing the current CV PDF.

## What The App Does

- Public entry page with one password field.
- Default user password opens the PDF viewer.
- Admin password opens the admin upload page.
- Admin can upload one PDF; a new upload replaces the old PDF.
- Authentication uses BCrypt password hashes and ASP.NET Core sessions.
- Frontend is a Next.js app with a black grid-style UI.

## Project Structure

```text
.
├── Controllers/
│   ├── AuthController.cs      # Login, admin login, logout, dev hash generator
│   └── PdfController.cs       # Protected PDF download, admin upload/delete
├── Services/
│   ├── AuthService.cs         # BCrypt password validation
│   └── PdfService.cs          # PDF file storage and access checks
├── Models/
│   ├── LoginRequest.cs        # Login payload, including honeypot field
│   └── LoginResponse.cs       # Login response shape
├── Middleware/
│   └── SecurityHeadersMiddleware.cs
├── Program.cs                 # ASP.NET Core service and middleware setup
├── data/
│   └── cv.pdf                 # Current uploaded PDF, ignored by git
├── cv_frontend/               # Next.js frontend
└── PROJECT_STATUS.md          # Current status and next tasks
```

## Architecture

The application is split into two local services:

- Backend: ASP.NET Core API on `http://localhost:8080`
- Frontend: Next.js on `http://localhost:3000`

The frontend talks to the backend through `cv_frontend/lib/api.ts` using `fetch(..., { credentials: "include" })`. Credentials are important because the backend stores login state in a session cookie.

Login flow:

1. User enters a password on `/`.
2. Frontend first tries `POST /api/auth/admin-login`.
3. If that succeeds, the backend stores `IsAuthenticated=true` and `IsAdmin=true` in the session and the frontend opens `/admin`.
4. If admin login fails, the frontend tries `POST /api/auth/login`.
5. If default login succeeds, the backend stores `IsAuthenticated=true` and the frontend opens `/viewer`.

PDF flow:

1. `/viewer` requests `GET /api/pdf/download`.
2. Backend only returns the PDF when `IsAuthenticated=true`.
3. `/admin` uploads via `POST /api/pdf/upload`.
4. Backend only accepts uploads when `IsAdmin=true`.
5. Uploaded PDFs are written to `Pdf:StoragePath`; each new upload replaces the old file.

Security pieces currently in place:

- BCrypt password hashes.
- Separate user and admin password hashes.
- Session cookie with `HttpOnly`, strict same-site behavior, and secure cookies in production.
- Rate limiting for general requests, user login, and admin login.
- Honeypot field in login payloads for simple bot submissions.
- CSRF token validation for login, logout, upload, and delete requests.
- Basic security headers middleware.
- PDF upload checks for content type, file size, and PDF file signature.

## Requirements

- .NET SDK 10
- Node.js and npm

## Configuration

Create or update `appsettings.json` locally. Do not commit it.

Use this shape:

```json
{
  "Auth": {
    "PasswordHash": "YOUR_USER_BCRYPT_HASH",
    "AdminPasswordHash": "YOUR_ADMIN_BCRYPT_HASH"
  },
  "Pdf": {
    "ContentBase64": "",
    "StoragePath": "data/cv.pdf"
  },
  "Cors": {
    "AllowedOrigins": "http://localhost:3000"
  }
}
```

The frontend local API URL is configured in:

```text
cv_frontend/.env.local
```

Expected local value:

```text
NEXT_PUBLIC_API_BASE=http://localhost:8080/api
```

## Generate Password Hashes

Start the backend first, then call:

```bash
curl "http://localhost:8080/api/auth/generate-hash?password=your-password"
```

Copy the returned `hash` into either:

- `Auth:PasswordHash` for default CV viewer access
- `Auth:AdminPasswordHash` for admin upload access

Important: type the plain password in the UI, not the BCrypt hash.

## CSRF Protection

The backend exposes:

```text
GET /api/auth/csrf
```

The frontend calls this endpoint before mutating requests and sends the returned token as:

```text
X-CSRF-TOKEN: ...
```

This protects cookie-authenticated API requests from cross-site request forgery. If you call protected mutating endpoints manually with `curl`, fetch a CSRF token first or use the browser UI.

## Start Locally

Start backend:

```bash
cd "/Users/fabiankassner/Documents/4 Semester/Website/cv_website"
dotnet run
```

Start frontend in a second terminal:

```bash
cd "/Users/fabiankassner/Documents/4 Semester/Website/cv_website/cv_frontend"
npm run dev
```

Open:

```text
http://localhost:3000
```

## Restart Locally

Stop each running server with `Ctrl+C`, then run the same start commands again.

If a port is stuck:

```bash
lsof -ti tcp:8080 | xargs kill
lsof -ti tcp:3000 | xargs kill
```

## Build Checks

Backend:

```bash
dotnet build
```

Frontend:

```bash
cd cv_frontend
npm run build
```

## GitHub Actions

The repository includes a CI workflow at:

```text
.github/workflows/ci.yml
```

It runs on pushes to `main`/`master` and on pull requests. The workflow:

- builds the ASP.NET Core backend
- installs and builds the Next.js frontend
- starts the backend with CI-only test password hashes
- verifies default user login
- verifies admin login
- uploads a tiny dummy PDF through the admin API

The CI passwords are only test values:

- `ci-user-password`
- `ci-admin-password`

Do not reuse them for local or production access.

## Useful API Tests

Default user login:

```bash
curl -c csrf_cookies.txt http://localhost:8080/api/auth/csrf
```

Then copy the returned `token` into:

```bash
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -H "X-CSRF-TOKEN: YOUR_CSRF_TOKEN" \
  -d '{"password":"YOUR_USER_PASSWORD","website":""}' \
  -b csrf_cookies.txt \
  -c cookies.txt
```

Admin login:

```bash
curl -X POST http://localhost:8080/api/auth/admin-login \
  -H "Content-Type: application/json" \
  -H "X-CSRF-TOKEN: YOUR_CSRF_TOKEN" \
  -d '{"password":"YOUR_ADMIN_PASSWORD","website":""}' \
  -b csrf_cookies.txt \
  -c admin_cookies.txt
```

Upload PDF after admin login:

```bash
curl -X POST http://localhost:8080/api/pdf/upload \
  -H "X-CSRF-TOKEN: YOUR_CSRF_TOKEN" \
  -F "file=@./cv.pdf" \
  -b admin_cookies.txt
```

Download PDF after user or admin login:

```bash
curl http://localhost:8080/api/pdf/download \
  -b cookies.txt \
  -o cv.pdf
```

## Deployment Notes

See [DEPLOYMENT.md](./DEPLOYMENT.md) for the full checklist.

For Railway or another production host:

- Set `Auth__PasswordHash` and `Auth__AdminPasswordHash` as environment variables.
- Set `Cors__AllowedOrigins` to the production frontend domain.
- Set `Pdf__StoragePath` to a persistent volume path, for example `/data/cv.pdf`.
- Use HTTPS so secure session cookies work as intended.
- Mount a persistent volume at `/data` if uploaded PDFs should survive deploys or restarts.
- Do not commit `appsettings.json`, `.env.local`, or PDF files.
