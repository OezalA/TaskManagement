# Quick Start Guide - Task Management Frontend

Get the Angular frontend running in 5 minutes!

## Prerequisites

- Node.js 18+ ([Download](https://nodejs.org/))
- npm 9+
- Backend API running on `http://localhost:5253`
- Valid JWT token with `access_api` scope

## 1. Install Dependencies

```bash
cd TaskManagement.Frontend
npm install
```

This may take 2-3 minutes on first run.

## 2. Start Development Server

```bash
npm start
```

Output:
```
✔ Compiled successfully.

Application bundle generated successfully.
Watch mode enabled. To disable watch mode, pass --watch=false.
- Local:        http://localhost:4200/
```

## 3. Open in Browser

Navigate to: **http://localhost:4200**

## 4. Login with JWT Token

### Get a JWT Token

#### Option A: Using Azure CLI
```powershell
# Login to Azure
az login

# Get token
$token = (az account get-access-token --resource "api://55739de4-48cd-4dc1-9067-06867aa3c9b3" --query accessToken -o tsv)
Write-Host $token
```

#### Option B: Using Portal
1. Go to Azure Portal
2. Navigate to your App Registration
3. Create a client secret
4. Use it to request a token from the token endpoint

#### Option C: Development Token
For testing, you can create a simple JWT token at [jwt.io](https://jwt.io):

```json
{
  "sub": "user-id",
  "email": "user@example.com",
  "email_verified": true,
  "name": "John Doe",
  "scope": "access_api",
  "aud": "api://55739de4-48cd-4dc1-9067-06867aa3c9b3"
}
```

### Login Steps

1. Click the **Sign In** button or go to http://localhost:4200/login
2. Paste your JWT token in the textarea
3. Click **Sign In**
4. You'll be redirected to the dashboard

## 5. Explore the App

### Dashboard
- Overview of your tasks, projects, and users
- Quick action buttons
- This week's tasks

### Tasks
- View all tasks
- Click on a task to see details
- Create new tasks
- Start/stop work on tasks

### NLP Query Interface
- Ask natural language questions
- Examples:
  - "What did I do this week?"
  - "Show my work logs"
  - "How much time on ProjectX?"

### Other Features
- Projects
- Users
- Time Tracking
- Teams

## Available Commands

```bash
npm start          # Start dev server (http://localhost:4200)
npm test           # Run unit tests
npm run build      # Build for production
npm run lint       # Run linter
```

## Troubleshooting

### "Cannot GET /login"
- Make sure `npm start` is running
- Check browser console (F12) for errors

### "401 Unauthorized"
- Token is expired or invalid
- Get a new token and login again

### "Cannot read property 'id' of undefined"
- Check that API is running on http://localhost:5253
- Check API logs for errors

### "CORS policy blocked"
- Make sure API has CORS enabled
- Check that API is accessible from http://localhost:4200

### API Connection Issues

Check API status:
```bash
curl http://localhost:5253/api/health
```

Should return HTTP 200.

## File Structure Quick Reference

```
src/
├── app/
│   ├── core/
│   │   ├── services/           # API services
│   │   ├── interceptors/       # HTTP interceptors
│   │   └── guards/             # Route guards
│   ├── shared/
│   │   └── components/         # Shared components
│   ├── features/
│   │   ├── auth/               # Login
│   │   ├── dashboard/          # Overview
│   │   ├── tasks/              # Tasks
│   │   ├── nlp-query/          # NLP
│   │   └── ...                 # Other features
│   └── app.routes.ts           # Routes
├── environments/
│   └── environment.ts          # Config
└── main.ts                     # Bootstrap
```

## Next Steps

After login, try:

1. **Check Dashboard** - See your work overview
2. **View Tasks** - See all tasks
3. **Try NLP Query** - Ask a question
4. **Start Work** - Click a task and start tracking time
5. **View Work Logs** - See your time entries

## Development Tips

### Change API URL

Edit `src/environments/environment.ts`:

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://your-api:5253'
};
```

### Enable Debug Mode

```typescript
// In main.ts
enableDebugTools(moduleRef.components[0]);
```

Then in browser console:
```javascript
ng.getComponent($0);
```

### View Network Requests

1. Open DevTools (F12)
2. Go to Network tab
3. Filter by "api"
4. Watch requests as you use the app

## Common Issues

| Issue | Solution |
|-------|----------|
| Port 4200 already in use | `ng serve --port 4201` |
| Module not found errors | `npm install` |
| Blank page | Check browser console for errors |
| API 404 errors | Check API is running on port 5253 |
| Token errors | Get a new JWT token |

## Getting Help

1. Check browser console (F12 → Console tab)
2. Check API logs
3. Review `README.md` for full documentation
4. Check `DEPLOYMENT.md` for production setup

## Next: Build for Production

When ready to deploy:

```bash
npm run build
# Then deploy dist/task-management-frontend/ to Azure
```

See `DEPLOYMENT.md` for detailed instructions.

---

**Happy Coding!** 🚀
