# syntax=docker/dockerfile:1
# Build the Vite SPA and serve its static files through Nginx.

FROM node:24-bookworm-slim AS build

WORKDIR /app

# TypeScript and Vite are development dependencies needed for the build.
# npm ci requires package.json and package-lock.json to be synchronized.
COPY package.json package-lock.json ./
RUN npm ci --include=dev

# Copy project source and the Docker-specific Vite environment.
COPY index.html vite.config.ts .oxlintrc.json tsconfig*.json ./
COPY .env.docker .env.production
COPY src/ ./src/
COPY public/ ./public/

# Run the existing build script: tsc -b && vite build.
RUN npm run build

FROM nginx:stable-alpine AS runtime

# Only compiled assets are copied to the runtime image.
RUN rm -rf /usr/share/nginx/html/*
COPY --from=build /app/dist/ /usr/share/nginx/html/

# Configuracion incluida para no depender de un nginx.conf adicional.
# API and SignalR calls are reverse-proxied to the backend service.
# SPA routes fall back to index.html when the browser is refreshed.
COPY <<'NGINX' /etc/nginx/conf.d/default.conf
server {
    listen 80;
    server_name _;

    root /usr/share/nginx/html;
    index index.html;
    server_tokens off;

    location = /index.html {
        add_header Cache-Control "no-cache" always;
    }

    location /assets/ {
        try_files $uri =404;
    }

    location /api/ {
        proxy_pass http://motor-control-api:5168;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    location /hubs/ {
        proxy_pass http://motor-control-api:5168;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }

    location / {
        try_files $uri $uri/ /index.html;
    }
}
NGINX

RUN nginx -t

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
