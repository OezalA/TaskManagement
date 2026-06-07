# --- Stage 1: build the Angular app ---
FROM node:20-alpine AS build
WORKDIR /app

# Install dependencies (deterministic via package-lock.json)
COPY package*.json ./
RUN npm ci

# Build the production bundle
COPY . .
RUN npm run build

# --- Stage 2: serve the static files with nginx ---
FROM nginx:alpine
RUN rm -rf /usr/share/nginx/html/*
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/task-management-frontend /usr/share/nginx/html
EXPOSE 80
