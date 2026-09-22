# =============================================================================
# RoKey MANAGEMENT - variables.tf
# =============================================================================
# Todas las variables necesarias para desplegar en GCP (Cloud Run + Cloud SQL + AR).
# Valores sensibles (passwords, JWT) deben venir de tfvars o Secret Manager.
# =============================================================================

variable "gcp_project_id" {
  description = "GCP project ID (ej: rokey-prod-123456)"
  type        = string
}

variable "gcp_region" {
  description = "GCP region para todos los recursos"
  type        = string
  default     = "us-central1"
}

variable "gcp_ar_repo" {
  description = "Nombre del Artifact Registry repo (docker)"
  type        = string
  default     = "rokey-repo"
}

# ---------------------------------------------------------------------------
# Cloud SQL
# ---------------------------------------------------------------------------
variable "db_name" {
  description = "Nombre de la base de datos"
  type        = string
  default     = "rokey"
}

variable "db_user" {
  description = "Usuario de la base de datos"
  type        = string
  default     = "rokey"
}

variable "db_password" {
  description = "Password de la base de datos (usar Secret Manager en prod)"
  type        = string
  sensitive   = true
  default     = null
}

variable "db_tier" {
  description = "Tier de Cloud SQL (db-f1-micro para dev, db-custom-1-3840 para prod)"
  type        = string
  default     = "db-f1-micro"
}

# ---------------------------------------------------------------------------
# JWT / App config
# ---------------------------------------------------------------------------
variable "jwt_key" {
  description = "JWT signing key (min 32 chars, generar con openssl rand -base64 48)"
  type        = string
  sensitive   = true
  default     = null
}

variable "jwt_issuer" {
  description = "JWT issuer"
  type        = string
  default     = "RoKeyAPI"
}

variable "jwt_audience" {
  description = "JWT audience"
  type        = string
  default     = "RoKeyApp"
}

variable "jwt_expiry_minutes" {
  description = "JWT expiry en minutos"
  type        = string
  default     = "480"
}

# ---------------------------------------------------------------------------
# Cloud Run images
# ---------------------------------------------------------------------------
variable "backend_image" {
  description = "Imagen del backend (ej: us-central1-docker.pkg.dev/PROJECT/rokey-repo/backend:latest)"
  type        = string
  default     = null
}

variable "admin_image" {
  description = "Imagen del admin-panel"
  type        = string
  default     = null
}

variable "client_image" {
  description = "Imagen del client-portal"
  type        = string
  default     = null
}
