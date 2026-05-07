# Deployment Checklist

This project is prepared for a split deployment:

- Backend: ASP.NET Core API, for example on Railway
- Frontend: Next.js app, for example on Vercel, Netlify, or Railway

## Backend Environment Variables

Set these in the backend hosting platform:

```text
ASPNETCORE_ENVIRONMENT=Production
PORT=8080
Auth__PasswordHash=YOUR_USER_BCRYPT_HASH
Auth__AdminPasswordHash=YOUR_ADMIN_BCRYPT_HASH
Cors__AllowedOrigins=https://YOUR_FRONTEND_DOMAIN
Pdf__StoragePath=/data/cv.pdf
Pdf__ContentBase64=
```

Use a persistent volume mounted at `/data` if uploads should survive deploys and restarts.

## Frontend Environment Variables

Set this in the frontend hosting platform:

```text
NEXT_PUBLIC_API_BASE=https://YOUR_BACKEND_DOMAIN/api
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
