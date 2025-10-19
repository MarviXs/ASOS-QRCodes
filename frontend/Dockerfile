# ---------- BUILD ----------
FROM node:22-alpine AS build-stage

ENV VITE_API_URL=QR_BACKEND_API_URL
WORKDIR /app

# Use pnpm via Corepack
RUN corepack enable
# Optional: pin pnpm version if you want
# ARG PNPM_VERSION=10.18.3
# RUN corepack prepare pnpm@${PNPM_VERSION} --activate

# Make pnpm non-interactive in Docker/CI
ENV CI=true

# Warm the store from lockfile for caching
COPY pnpm-lock.yaml ./
RUN --mount=type=cache,id=pnpm-store,target=/root/.local/share/pnpm/store \
    pnpm fetch

# Copy project so prepare/build scripts can see it
COPY . .

# Allow native deps to build during install (or use .npmrc allow-scripts)
ENV PNPM_ALLOW_SCRIPTS=*

# Link deps from store; no network thanks to --offline
RUN --mount=type=cache,id=pnpm-store,target=/root/.local/share/pnpm/store \
    pnpm install --frozen-lockfile --offline

# Build your SPA
RUN pnpm run build

# ---------- PROD ----------
FROM nginx:stable-alpine AS production
ENV API_URL=$API_URL

COPY ./docker/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build-stage /app/dist/spa /usr/share/nginx/html
COPY ./docker/env.sh /docker-entrypoint.d/env.sh
RUN chmod +x /docker-entrypoint.d/env.sh

EXPOSE 80
CMD ["nginx","-g","daemon off;"]
