# CV Website - Projekt Status

**Letztes Update:** 7. Mai 2026  
**Ziel:** Website mit passwortgeschütztem CV (PDF) + Admin-Bereich für Upload  
**Deployment:** Railway

---

## ✅ BACKEND (C# ASP.NET Core) - KOMPLETT

### Implementiert:
- [x] Authentication Service (BCrypt Passwort-Validierung)
- [x] Admin Password Validation (separates Admin-Passwort)
- [x] PDF Service (laden, speichern, löschen mit File-Fallback)
- [x] AuthController (POST /api/auth/login)
- [x] AuthController (POST /api/auth/admin-login) ✨
- [x] PdfController (GET /api/pdf/download)
- [x] PdfController (POST /api/pdf/upload) ✨
- [x] PdfController (DELETE /api/pdf/delete) ✨
- [x] Session Management (Admin-Flag in Session)
- [x] CORS konfiguriert (localhost:3000)
- [x] Rate Limiting (100 req/min allgemein, 5 req/min Login)
- [x] Honeypot-Feld im Login gegen einfache Bot-Form-Submits
- [x] Security Headers Middleware
- [x] Input Validation & Logging mit Emoji-Indikatoren
- ✅ **BUILD: Erfolgreich** (0 Errors)

---

## ✅ FRONTEND (Next.js 14) - KOMPLETT

### Setup:
- [x] Next.js 14 mit TypeScript
- [x] Tailwind CSS 4
- [x] Modernes Color-Theme (Schwarz/Dunkelblau #0a0e27 + Silber/Weiß #e5e7eb)
- [x] Custom color extensions in tailwind.config.ts
- [x] Globals CSS mit Dark-Theme Styling
- ✅ **BUILD: Erfolgreich** (0 Errors)

### Komponenten:
- [x] PasswordInput - Sichere Passwort-Eingabe
- [x] Button - Mit 3 Varianten (primary, secondary, danger) + Loading State
- [x] Card - Moderne Card mit Dark-Theme Borders
- [x] API Client (lib/api.ts) - Vollständige API-Integration
- [x] Types (lib/types.ts) - TypeScript Interfaces
- [x] useAuth Hook - Session-Management im Frontend

### Pages:
- [x] Landing Page (/) - Schöne 2-spaltige Übersicht
- [x] Login Page (/auth/login) - Passwort-Login für CV-Zugang
- [x] Viewer Page (/viewer) - PDF-Anzeige in iframe
- [x] Admin Page (/admin) - Admin-Login, Upload, Delete, Preview

### Features:
- [x] Responsive Design (Mobile & Desktop)
- [x] Session-basierte Authentifizierung
- [x] Error Handling & Validation
- [x] Loading States mit Spinner
- [x] PDF Drag-and-Drop vorbereitet
- [x] Logout Funktionalität
- [x] Moderne Übergänge & Hover-Effects

---

## 🚀 LAUFEND TESTEN

### Frontend:
- **URL:** http://localhost:3000
- **Status:** ✅ Läuft im Dev-Mode (Turbopack)
- **API Base:** http://localhost:8080/api

### Backend:
- **URL:** http://localhost:8080
- **Status:** ✅ Läuft im Development-Mode
- **Ports:** HTTP 5256, HTTP 8080

---

## 📋 SICHERHEIT - Implementiert

- [x] Passwort-Hashing (BCrypt mit workFactor 11)
- [x] Admin-Passwort getrennt vom User-Passwort
- [x] Sichere Session-Cookies (HttpOnly, Secure, SameSite=Strict)
- [x] Rate Limiting gegen Brute-Force
- [x] CORS eingeschränkt auf Whitelist
- [x] Security Headers (Middleware)
- [x] Input Validation (Längen-Prüfung, ContentType-Prüfung)
- [x] Logging für Sicherheitsereignisse (mit Emojis für Visualisierung)
- [ ] CSRF Protection (für Production)
- [ ] Content Security Policy Headers (optional)
- [ ] Rate-Limits für Admin-Login separat prüfen/verschärfen (`/api/auth/admin-login`)
- [ ] Optional: temporäre IP-/Session-Sperre nach wiederholten Fehlversuchen
- [ ] Optional: serverseitige Audit-Logs für Login-/Upload-Versuche ohne sensible Daten
- [ ] Optional: produktionsreife Persistenz/Backups für `data/cv.pdf` auf Railway prüfen

Hinweis: Das Honeypot-Feld ist nur ein leichter Bot-Filter. Wichtiger bleiben serverseitiges Rate Limiting, Logging, sichere Cookies, starke Passwörter und später CSRF-Schutz.

---

## 🎨 UI - NÄCHSTE SCHRITTE

- [ ] PDF-Anzeige für Default User ästhetischer und mittiger darstellen
- [ ] PDF-Vorschau im Adminbereich ästhetischer und mittiger darstellen
- [ ] Viewer/Admin PDF-Fläche an das schwarze Grid-Design anpassen
- [ ] Mobile Darstellung der PDF-Ansicht prüfen

---

## 🎯 TODO - DEPLOYMENT ZU RAILWAY

### 1. Vorbereitung:
- [ ] Git Repo aktualisieren (git push)
- [ ] .gitignore überprüfen (keine Secrets!)
- [ ] appsettings.example.json erstellen (ohne echte Werte)

### 2. Railway Setup:
- [ ] Railway Account & Projekt erstellen
- [ ] Environment Variables auf Railway einstellen:
  - `Auth:PasswordHash` (BCrypt Hash)
  - `Auth:AdminPasswordHash` (BCrypt Hash)
  - `Cors:AllowedOrigins` (Production Domain)

### 3. Frontend Deployment:
- [ ] next.config.ts für Production überprüfen
- [ ] .env.production erstellen (mit Production API URL)
- [ ] Build testen: `npm run build`

### 4. Testen:
- [ ] User-Login testen
- [ ] Admin-Login testen
- [ ] PDF Upload/Delete testen
- [ ] Responsive Design prüfen

---

## 🔧 TECHNISCHE DETAILS

### Backend:
- **Framework:** ASP.NET Core mit C#
- **Authentication:** Session-basiert (Cookies)
- **Security:** BCrypt + Rate Limiting + CORS
- **PDF Speichern:** Datei-basiert mit In-Memory Fallback

### Frontend:
- **Framework:** Next.js 14 (App Router)
- **UI:** Tailwind CSS mit Custom Theme
- **State:** Client-side mit Hooks (useAuth, useState)
- **API:** Fetch mit credentials: 'include'

---

## 📝 TESTFALL-BEISPIELE

### 1. User-Login:
```bash
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"password": "YOUR_USER_PASSWORD"}' \
  -c cookies.txt
```

### 2. PDF Download (mit Session Cookie):
```bash
curl -X GET http://localhost:8080/api/pdf/download \
  -b cookies.txt \
  -o lebenslauf.pdf
```

### 3. Admin-Login:
```bash
curl -X POST http://localhost:8080/api/auth/admin-login \
  -H "Content-Type: application/json" \
  -d '{"password": "YOUR_ADMIN_PASSWORD"}' \
  -c admin_cookies.txt
```

### 4. PDF Upload (mit Admin-Session):
```bash
curl -X POST http://localhost:8080/api/pdf/upload \
  -H "Content-Type: multipart/form-data" \
  -F "file=@./cv.pdf" \
  -b admin_cookies.txt
```

---

## ✨ STATUS ZUSAMMENFASSUNG

| Komponente | Status | Notes |
|-----------|--------|-------|
| Backend API | ✅ Ready | Alle Endpoints implementiert |
| Frontend UI | ✅ Ready | Modernes Design, responsiv |
| Build | ✅ Success | Keine Fehler |
| Local Testing | 🔄 Ready | Bereit zum Testen |
| Production | 📋 TODO | Railway Deployment |

---

**Nächste Aktion:** Testen im Browser oder mit Postman, dann Railway Deployment vorbereiten!
