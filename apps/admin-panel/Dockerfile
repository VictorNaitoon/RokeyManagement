# =============================================================================
# RoKey Management - admin-panel Dockerfile (multi-stage)
# Build: node:22-alpine + tsc + vite build
# Runtime: nginx:alpine, non-root, puerto 8080, healthcheck en /health
# =============================================================================

# -----------------------------------------------------------------------------
# BUILD STAGE
# -----------------------------------------------------------------------------
FROM node:22-alpine AS build
WORKDIR /app

# Dependencias primero (cache de capas)
COPY package*.json ./
RUN npm ci --prefer-offline --no-audit --progress=false

# Código fuente y build de producción
COPY . .
RUN npm run build

# -----------------------------------------------------------------------------
# RUNTIME STAGE
# -----------------------------------------------------------------------------
FROM nginx:alpine AS runtime

# Config nginx SPA + /health + proxy /api (rutas relativas del axios)
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Assets compilados
COPY --from=build /app/dist /usr/share/nginx/html

# Permisos para usuario no-root
RUN mkdir -p /var/cache/nginx /var/run /var/log/nginx \
    && chown -R nginx:nginx /var/cache/nginx /var/run /var/log/nginx /usr/share/nginx/html

USER nginx

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=5s --retries=3 \
    CMD wget -q --spider http://localhost:8080/health || exit 1

CMD ["nginx", "-g", "daemon off;"]
