# Frontend Deployment Guide

## Overview

This guide covers deploying the Task Management Frontend to Azure Static Web Apps, a recommended production hosting solution for Angular SPAs.

## Prerequisites

- Azure subscription
- Azure CLI installed (`az login` configured)
- Frontend built and tested locally
- API deployed and accessible

## Deployment Options

### Option 1: Azure Static Web Apps (Recommended)

Azure Static Web Apps is ideal for hosting Single Page Applications (SPAs) like Angular with automatic CI/CD.

#### Step 1: Create Static Web App Resource

```bash
az staticwebapp create \
  --name "task-mgmt-frontend" \
  --resource-group "task-management-rg" \
  --source "https://github.com/YOUR_USERNAME/TaskManagementMCP" \
  --branch "dev" \
  --location "westeurope" \
  --build-folder "TaskManagement.Frontend/dist"
```

#### Step 2: Configure Build Settings

Create `.github/workflows/azure-static-web-apps-*.yml` (auto-generated):

```yaml
name: Azure Static Web Apps CI/CD

on:
  push:
    branches:
      - dev
    paths:
      - 'TaskManagement.Frontend/**'
  pull_request:
    branches:
      - dev

jobs:
  build_and_deploy_job:
    runs-on: ubuntu-latest
    name: Build and Deploy
    steps:
      - uses: actions/checkout@v2
        with:
          submodules: true

      - name: Setup Node
        uses: actions/setup-node@v2
        with:
          node-version: '18'

      - name: Install dependencies
        run: npm install
        working-directory: TaskManagement.Frontend

      - name: Build
        run: npm run build
        working-directory: TaskManagement.Frontend

      - name: Deploy
        uses: Azure/static-web-apps-deploy@v1
        with:
          azure_static_web_apps_api_token: ${{ secrets.AZURE_STATIC_WEB_APPS_API_TOKEN }}
          repo_token: ${{ secrets.GITHUB_TOKEN }}
          action: "upload"
          app_location: "TaskManagement.Frontend"
          app_artifact_location: "dist/task-management-frontend"
          skip_app_build: true
```

#### Step 3: Configure API URL

Update `src/environments/environment.prod.ts`:

```typescript
export const environment = {
  production: true,
  apiUrl: 'https://your-api.azurewebsites.net'  // Update with your API URL
};
```

#### Step 4: Enable SPA Routing

Create `staticwebapp.config.json` in root:

```json
{
  "navigationFallback": {
    "rewrite": "/index.html",
    "exclude": ["/images/*.{svg,png,jpg,jpeg,gif}", "/css/*"]
  },
  "mimeTypes": {
    ".wasm": "application/wasm"
  },
  "routes": [
    {
      "route": "/api/*",
      "allowedRoles": ["authenticated"]
    }
  ],
  "auth": {
    "identityProviders": {
      "azureActiveDirectory": {
        "enabled": true,
        "registration": {
          "clientIdSettingName": "AZURE_CLIENT_ID",
          "clientSecretSettingName": "AZURE_CLIENT_SECRET",
          "openIdIssuer": "https://login.microsoftonline.com/{tenantId}/v2.0"
        }
      }
    }
  }
}
```

### Option 2: Azure App Service

For more control over the hosting environment:

#### Step 1: Create App Service

```bash
az appservice plan create \
  --name "task-mgmt-plan" \
  --resource-group "task-management-rg" \
  --sku B1 \
  --is-linux

az webapp create \
  --name "task-mgmt-frontend" \
  --resource-group "task-management-rg" \
  --plan "task-mgmt-plan" \
  --runtime "node|18"
```

#### Step 2: Build and Deploy

```bash
# Build the Angular app
npm run build

# Deploy using Azure CLI
az webapp deployment source config-zip \
  --resource-group "task-management-rg" \
  --name "task-mgmt-frontend" \
  --src dist/task-management-frontend.zip
```

Or use VS Code Azure Tools extension.

### Option 3: Docker Container

Containerize the frontend for maximum flexibility:

#### Create Dockerfile

```dockerfile
# Build stage
FROM node:18-alpine AS builder
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

# Serve stage
FROM nginx:alpine
COPY --from=builder /app/dist/task-management-frontend /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

#### Build and Push

```bash
docker build -t task-mgmt-frontend:latest .
docker tag task-mgmt-frontend:latest YOUR_REGISTRY.azurecr.io/task-mgmt-frontend:latest
docker push YOUR_REGISTRY.azurecr.io/task-mgmt-frontend:latest
```

#### Deploy to Container Instances or AKS

```bash
az container create \
  --resource-group "task-management-rg" \
  --name "task-mgmt-frontend" \
  --image "YOUR_REGISTRY.azurecr.io/task-mgmt-frontend:latest" \
  --dns-name-label "task-mgmt-frontend" \
  --ports 80 \
  --environment-variables \
    API_URL="https://your-api.azurewebsites.net"
```

## Post-Deployment Configuration

### 1. Update API URL

After API is deployed, update the API base URL:

```bash
# Get API URL
API_URL=$(az webapp show \
  --resource-group "task-management-rg" \
  --name "task-mgmt-api" \
  --query "defaultHostName" --output tsv)

# Update Static Web App app settings
az staticwebapp appsettings set \
  --name "task-mgmt-frontend" \
  --setting-names API_URL="https://${API_URL}"
```

Update `environment.prod.ts`:

```typescript
const apiUrl = localStorage.getItem('API_URL') || 'https://your-api.azurewebsites.net';
export const environment = {
  production: true,
  apiUrl: apiUrl
};
```

### 2. Configure CORS in API

Update the .NET API's `Program.cs`:

```csharp
var allowedOrigins = new[]
{
    "https://task-mgmt-frontend.azurestaticapps.net",
    "http://localhost:4200"  // For local testing
};

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

app.UseCors("AllowFrontend");
```

### 3. Set Environment Variables

For Static Web Apps, set app configuration:

```bash
az staticwebapp appsettings set \
  --name "task-mgmt-frontend" \
  --setting-names \
    API_URL="https://your-api.azurewebsites.net" \
    ENVIRONMENT="production"
```

### 4. Configure Custom Domain

```bash
az staticwebapp custom-domain create \
  --name "task-mgmt-frontend" \
  --domain-name "app.yourdomain.com" \
  --validation-method cname
```

Follow the CNAME validation steps provided by Azure.

## Monitoring and Logging

### View Application Insights

```bash
# Get instrumentation key
az monitor app-insights component show \
  --app "task-mgmt-frontend-insights" \
  --resource-group "task-management-rg" \
  --query "instrumentationKey"
```

Add to `main.ts`:

```typescript
import { enableDebugTools } from '@angular/platform-browser';
import { NgZone } from '@angular/core';

// Application Insights
import { ApplicationInsightsService } from './app/core/services/application-insights.service';

bootstrapApplication(AppComponent, {
  providers: [
    ApplicationInsightsService
  ]
}).then(moduleRef => {
  const appRef = moduleRef.injector.get(ApplicationRef);
  enableDebugTools(appRef.components[0]);
}).catch(err => console.error(err));
```

### View Logs

```bash
# Stream logs from Static Web Apps
az staticwebapp logs stream \
  --name "task-mgmt-frontend"

# View app logs
az webapp log tail \
  --resource-group "task-management-rg" \
  --name "task-mgmt-frontend"
```

## Troubleshooting

### 404 on Page Refresh

Ensure SPA routing is configured in `staticwebapp.config.json`:

```json
{
  "navigationFallback": {
    "rewrite": "/index.html"
  }
}
```

### CORS Errors

Check API CORS settings in `Program.cs` and API deployment logs:

```bash
az webapp log tail \
  --resource-group "task-management-rg" \
  --name "task-mgmt-api"
```

### Authentication Issues

Verify JWT token claims include required scope:

```bash
# Decode JWT token (use jwt.io or jwtdecode library)
# Check for: "scope": "access_api"
```

### Build Failures

Check GitHub Actions logs:

```bash
# View workflow runs
gh run list --repo YOUR_USERNAME/TaskManagementMCP
gh run view <run-id> --log
```

## Performance Optimization

### Enable CDN

```bash
az staticwebapp update \
  --name "task-mgmt-frontend" \
  --resource-group "task-management-rg" \
  --enable-cdn true
```

### Configure Caching

Update `nginx.conf` (for Docker) or `staticwebapp.config.json`:

```json
{
  "routes": [
    {
      "route": "/assets/*",
      "headers": {
        "cache-control": "public, max-age=31536000"
      }
    },
    {
      "route": "/index.html",
      "headers": {
        "cache-control": "no-cache, no-store, must-revalidate"
      }
    }
  ]
}
```

## Rollback

### Revert to Previous Version

```bash
# For Static Web Apps, use GitHub Actions to trigger redeploy:
git revert HEAD
git push origin dev

# Or manually deploy previous build:
az staticwebapp deployment create \
  --name "task-mgmt-frontend" \
  --source-url "path/to/dist/build" \
  --provider "github"
```

## Cleanup

```bash
# Remove all resources
az group delete \
  --name "task-management-rg" \
  --yes --no-wait
```

## Next Steps

1. ✅ Deploy backend API
2. ✅ Deploy frontend
3. ⏳ Configure CI/CD for automated deployments
4. ⏳ Set up monitoring and alerting
5. ⏳ Configure custom domain
6. ⏳ Enable HTTPS (automatic with Azure)

## Support

For deployment issues:
- Check Azure Static Web Apps documentation
- Review GitHub Actions logs
- Check Application Insights
- Review Azure CLI error messages
