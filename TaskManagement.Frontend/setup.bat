@echo off
REM Task Management Frontend - Development Setup Script
REM This script sets up and runs the Angular frontend on Windows

echo =========================================
echo Task Management Frontend Setup
echo =========================================
echo.

REM Check if Node.js is installed
where node >nul 2>nul
if %errorlevel% neq 0 (
    echo ❌ Node.js is not installed. Please install it first.
    echo    Download from: https://nodejs.org/
    pause
    exit /b 1
)

for /f "tokens=*" %%i in ('node --version') do set NODE_VERSION=%%i
for /f "tokens=*" %%i in ('npm --version') do set NPM_VERSION=%%i

echo ✓ Node.js version: %NODE_VERSION%
echo ✓ npm version: %NPM_VERSION%
echo.

REM Navigate to frontend directory
cd /d "%~dp0"

REM Check if node_modules exists
if not exist "node_modules\" (
    echo 📦 Installing dependencies...
    echo    This may take a few minutes...
    echo.
    call npm install
    if %errorlevel% neq 0 (
        echo ❌ npm install failed
        pause
        exit /b 1
    )
) else (
    echo ✓ Dependencies already installed
)

echo.
echo =========================================
echo Setup Complete!
echo =========================================
echo.
echo Available commands:
echo   npm start       - Start development server (http://localhost:4200)
echo   npm test        - Run unit tests
echo   npm run build   - Build for production
echo.
echo Next steps:
echo   1. Make sure the API is running (http://localhost:5253)
echo   2. Run: npm start
echo   3. Open browser to: http://localhost:4200
echo   4. Go to /login and paste your JWT token
echo.
pause
