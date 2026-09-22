# Deploy — RoKey MANAGEMENT

## Requisitos
- Docker Desktop 4.x + Compose v2, Git, `gcloud` CLI, Terraform >= 1.5.

## Local
```bash
cp .env.example .env          # completar POSTGRES_* / JWT_* si hace falta
docker compose up --build     # crea postgres+backend+admin+client en rokey-net
```
Puertos: API `8080`, admin `3000→8080`, client `3001→8080` (nginx interno 8080).
Health: `curl http://localhost:8080/health` → `Healthy` (200).

## CI (`.github/workflows/ci.yml`)
Triggers: `push main`, `pull_request → main`, `workflow_dispatch`.
Imágenes: `ghcr.io/<owner>/rokeymanagement/{backend,admin-panel,client-portal}:sha + :latest` (cache `gha`, `GITHUB_TOKEN`).
Ver builds: GitHub → Actions → workflow **CI** → jobs `build-and-test-backend` / `build-images`.

## Prod GCP (Terraform)
```bash
cp terraform/terraform.tfvars.example terraform/terraform.tfvars  # editar obligatorias abajo
terraform -chdir=terraform init
terraform -chdir=terraform plan -var-file=terraform.tfvars
terraform -chdir=terraform apply -var-file=terraform.tfvars
```
Vars obligatorias: `gcp_project_id`, `gcp_region`, `gcp_ar_repo`, `db_password`, `jwt_key`, `backend_image`/`admin_image`/`client_image`.
Outputs: `backend_url`, `admin_url`, `client_url`, `cloud_sql_connection_name`.

### WIF (Workload Identity Federation) — 4 comandos
```bash
gcloud iam workload-identity-pools create github --location=global --project=PROJECT
gcloud iam workload-identity-pools providers create-oidc github --pool=github --issuer-uri=https://token.actions.githubusercontent.com --project=PROJECT --attribute-mapping="google.subject=assertion.sub,attribute.repository=assertion.repository"
gcloud iam service-accounts create deploy --project=PROJECT
gcloud iam service-accounts add-iam-policy-binding deploy@PROJECT.iam.gserviceaccount.com --role=roles/iam.workloadIdentityUser --member="principalSet://iam.googleapis.com/projects/PROJECT_NUMBER/locations/global/workloadIdentityPools/github/attribute.repository/OWNER/REPO"
```
GitHub → Settings → Variables: `GCP_PROJECT_ID`, `GCP_REGION`, `GCP_AR_REPO`; Secrets: `GCP_WORKLOAD_IDENTITY_PROVIDER`, `GCP_SERVICE_ACCOUNT`.
Activar deploy: descomentar en `ci.yml` bloques `Auth to Google Cloud (WIF)` + `Build and push (Artifact Registry)` + job `deploy-cloudrun` (3x `deploy-cloudrun@v2`).

## Secrets
```bash
openssl rand -base64 48   # JWT_KEY  (poner en .env y terraform.tfvars → jwt_key)
openssl rand -base64 32   # DB_PASSWORD
```
Opcional prod: Secret Manager (`google_secret_manager_secret`) y en `main.tf` usar `value_source.secret_key_ref` en vez de `value = var.jwt_key`.

## Rollback
```bash
terraform -chdir=terraform state list; terraform -chdir=terraform apply -var-file=terraform.tfvars  # revert infra
docker compose down; docker compose up --build -d                                     # local
gcloud run services update-traffic rokey-backend --to-revisions=PREV=100 --region=REGION  # Cloud Run
git revert <sha> && git push origin main   # + re-deploy (CI) o terraform apply
```

## Troubleshooting
- `UseHttpsRedirection` tras proxy: en prod Cloud Run envía `X-Forwarded-Proto`; si falla, condicionar `UseHttpsRedirection` a `!IsDevelopment`.
- DB no arranca: `docker compose logs postgres`; healthcheck `pg_isready -U $POSTGRES_USER -d $POSTGRES_DB` (interval 10s).
- API no healthy: `curl -v http://localhost:8080/health`; revisar `ConnectionStrings__WebApiDatabase` y `JWT__Key`.
- Nginx 8080: frontends escuchan `8080` interno, mapeados a `3000/3001` host; verificar `docker compose ps` y `nginx.conf` `try_files`/`proxy /api`.
