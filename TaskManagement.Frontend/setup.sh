#!/bin/bash

# Task Management Frontend - Development Setup Script
# This script sets up and runs the Angular frontend

set -e

echo "========================================="
echo "Task Management Frontend Setup"
echo "========================================="

# Check if Node.js is installed
if ! command -v node &> /dev/null; then
    echo "❌ Node.js is not installed. Please install it first."
    exit 1
fi

echo "✓ Node.js version: $(node --version)"
echo "✓ npm version: $(npm --version)"

# Navigate to frontend directory
cd "$(dirname "$0")"

# Check if node_modules exists
if [ ! -d "node_modules" ]; then
    echo ""
    echo "📦 Installing dependencies..."
    npm install
else
    echo "✓ Dependencies already installed"
fi

# Check if Angular CLI is installed
if ! npm list -g @angular/cli &> /dev/null 2>&1; then
    echo ""
    echo "⚠️  Angular CLI is not installed globally"
    echo "You can use: npm start (uses local CLI)"
fi

echo ""
echo "========================================="
echo "Setup Complete!"
echo "========================================="
echo ""
echo "Available commands:"
echo "  npm start       - Start development server (http://localhost:4200)"
echo "  npm test        - Run unit tests"
echo "  npm run build   - Build for production"
echo ""
echo "Next steps:"
echo "  1. Make sure the API is running (http://localhost:5253)"
echo "  2. Run: npm start"
echo "  3. Open browser to: http://localhost:4200"
echo "  4. Go to /login and paste your JWT token"
echo ""
