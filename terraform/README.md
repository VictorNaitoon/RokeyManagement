# Terraform GCP - RoKey MANAGEMENT

1. `cp terraform.tfvars.example terraform.tfvars` y setear `gcp_project_id`, `db_password`, `jwt_key` y `*_image`.
2. `terraform init` (primera vez) descarga provider `hashicorp/google ~> 5.0`.
3. `terraform plan -var-file=terraform.tfvars` previsualiza AR, Cloud SQL y Cloud Run.
4. `terraform apply -var-file=terraform.tfvars` crea todo; `output` muestra URLs y `connection_name`.
5. Vars clave: `gcp_region` (default us-central1), `gcp_ar_repo`, `db_tier` (dev db-f1-micro, prod db-custom-1-3840).
6. CI: definir Vars `GCP_PROJECT_ID`, `GCP_REGION`, `GCP_AR_REPO`; Secrets `GCP_WORKLOAD_IDENTITY_PROVIDER`, `GCP_SERVICE_ACCOUNT`.
7. Descomentar en `.github/workflows/ci.yml` bloques `Auth to Google Cloud (WIF)` y `Build and push (Artifact Registry)` y `deploy-cloudrun`.
8. Secret Manager (opcional MVP): usar `google_secret_manager_secret` para `JWT_KEY` y `DB_PASSWORD` en vez de var directa.
9. En `main.tf` cambiar `value = var.jwt_key` por `value_source.secret_key_ref` si se activan los secrets (ejemplo comentado).
10. WIF: `gcloud iam workload-identity-pools create github --location=global --project=PROJECT`
11. `gcloud iam workload-identity-pools providers create-oidc github --pool=github --issuer-uri=https://token.actions.githubusercontent.com --project=PROJECT`
12. Bindear SA: `gcloud iam service-accounts add-iam-policy-binding SA --role=roles/iam.workloadIdentityUser --member=principalSet://...`
13. Cloud SQL prod: descomentar VPC/peering en `main.tf`, `ipv4_enabled=false`, `private_network`, y `vpc_access` en backend.
