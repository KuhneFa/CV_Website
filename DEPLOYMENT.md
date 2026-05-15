# Deployment Checklist

This project is prepared for a split deployment:

- Backend: ASP.NET Core API, for example on Railway
- Frontend: Next.js app, for example on Vercel, Netlify, or Railway

For Railway-only deployment, create two services from the same GitHub repository:

- `CV_Website` backend service: root directory `/`
- Frontend service: root directory `/cv_frontend`

Do not make `cv_frontend` a separate Git repository. It is a normal folder in the root repository.

## Backend Environment Variables

Set these in the backend hosting platform:

```text
ASPNETCORE_ENVIRONMENT=Production
PORT=8080
Auth__PasswordHash=YOUR_USER_BCRYPT_HASH
Auth__AdminPasswordHash=YOUR_ADMIN_BCRYPT_HASH
Cors__AllowedOrigins=https://kathercv.de
Pdf__StoragePath=/data/cv.pdf
Pdf__ContentBase64=
```

Use a persistent volume mounted at `/data` if uploads should survive deploys and restarts.

## Frontend Environment Variables

Set this in the frontend hosting platform:

```text
NEXT_PUBLIC_API_BASE=https://YOUR_BACKEND_RAILWAY_DOMAIN/api
```

If the backend gets the custom domain `api.kathercv.de`, use:

```text
NEXT_PUBLIC_API_BASE=https://api.kathercv.de/api
```

If the backend only uses its Railway domain, use:

```text
NEXT_PUBLIC_API_BASE=https://YOUR_BACKEND.up.railway.app/api
```

Do not point `NEXT_PUBLIC_API_BASE` to the frontend service itself.

## Custom Domains

Recommended mapping:

```text
kathercv.de       -> frontend service
www.kathercv.de   -> frontend service, optional
api.kathercv.de   -> backend service, optional but clean
```

If you use both `kathercv.de` and `www.kathercv.de`, set backend CORS to both:

```text
Cors__AllowedOrigins=https://kathercv.de,https://www.kathercv.de
```

If you only use `kathercv.de`, this is enough:

```text
Cors__AllowedOrigins=https://kathercv.de
```

## Production Notes

- Use HTTPS for both frontend and backend.
- Keep `appsettings.json`, `.env.local`, and PDF files out of git.
- Store only BCrypt hashes in environment variables, never plain passwords.
- Configure CORS to the exact production frontend domain.
- Mount persistent storage for `/data` before relying on admin uploads.
- Run the GitHub Actions CI before deploying.

## Smoke Test After Deployment

1. Open the frontend production URL.
2. Login with the default password and verify the PDF viewer.
3. Login with the admin password.
4. Upload a dummy or real PDF.
5. Login as default user again and verify the new PDF appears.
