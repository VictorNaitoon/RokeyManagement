# CI/CD — RoKey MANAGEMENT

## Qué hace `ci.yml`
Build .NET 9 + tests, buildx de 3 imágenes y push a `ghcr.io` (verde sin GCP).

## Cómo habilitar deploy a GCP (Cloud Run)
1. Terraform: crear Artifact Registry, Cloud Run y Cloud SQL.
2. En GitHub: `Settings > Variables` → `GCP_PROJECT_ID`, `GCP_REGION`, `GCP_AR_REPO`.
3. `Settings > Secrets` → `GCP_WORKLOAD_IDENTITY_PROVIDER`, `GCP_SERVICE_ACCOUNT` (WIF).
4. Descomentar bloques `Artifact Registry` y `deploy` en `ci.yml`.
5. Push a `main` → deploy automático. El job `deploy` solo corre en `push main`.

## Notas
- GHCR usa `GITHUB_TOKEN` (no requiere secrets).
- Tags: `:latest` (solo en main) y `:sha` (siempre).
- Env vars esperadas por la API: `ConnectionStrings__WebApiDatabase`, `JWT__Key/Issuer/Audience` (ver `.env.example` y `Program.cs`).
