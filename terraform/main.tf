# =============================================================================
# RoKey MANAGEMENT - main.tf
# Stack prod GCP: Artifact Registry + Cloud SQL (Postgres 16) + Cloud Run v2
# =============================================================================
# Diseño simple para MVP:
#   - Sin VPC privada compleja (Cloud SQL con IP publica + authorized_networks opcional).
#     Para IP privada ver bloque comentado "VPC / private IP".
#   - Backend expone 8080 + /health, Cloud Run escala 0..10.
#   - Frontends son nginx:8080 estaticos (admin con proxy /api -> backend en compose,
#     en Cloud Run cada servicio tiene su URL propia, usar VITE_API_URL si hace falta).
# =============================================================================

terraform {
  required_version = ">= 1.5.0"
  required_providers {
    google = {
      source  = "hashicorp/google"
      version = "~> 5.0"
    }
  }
}

provider "google" {
  project = var.gcp_project_id
  region  = var.gcp_region
}

# ---------------------------------------------------------------------------
# Artifact Registry (Docker)
# ---------------------------------------------------------------------------
resource "google_artifact_registry_repository" "rokey" {
  location      = var.gcp_region
  repository_id = var.gcp_ar_repo
  description   = "RoKey MANAGEMENT - imagenes docker (backend, admin-panel, client-portal)"
  format        = "DOCKER"

  # Limpieza: opcional, no borrar en prod sin backup
  cleanup_policy_dry_run = false
}

# ---------------------------------------------------------------------------
# Cloud SQL - PostgreSQL 16
# ---------------------------------------------------------------------------
resource "google_sql_database_instance" "main" {
  name             = "rokey-db-${var.gcp_region}"
  database_version = "POSTGRES_16"
  region           = var.gcp_region

  # En dev deletion_protection = false para permitir terraform destroy
  deletion_protection = false

  settings {
    tier              = var.db_tier
    availability_type = "ZONAL"
    disk_autoresize   = true
    disk_size         = 20
    disk_type         = "PD_SSD"

    backup_configuration {
      enabled                        = true
      point_in_time_recovery_enabled = true
      backup_retention_settings {
        retained_backups = 7
        retention_unit   = "COUNT"
      }
    }

    ip_configuration {
      # MVP: IP publica habilitada. En prod restringir via authorized_networks o
      # migrar a IP privada con VPC peering (ver bloque comentado abajo).
      ipv4_enabled = true

      # Ejemplo para restringir (descomentar y completar):
      # authorized_networks {
      #   name  = "office"
      #   value = "203.0.113.0/24"
      # }
    }

    # Insights opcional
    insights_config {
      query_insights_enabled  = true
      query_string_length     = 1024
      record_application_tags = true
      record_client_address   = true
    }
  }
}

# Para IP privada (recomendado en prod) se necesita VPC + peering:
# resource "google_compute_network" "rokey_vpc" {
#   name                    = "rokey-vpc"
#   auto_create_subnetworks = false
# }
# resource "google_compute_global_address" "private_ip" {
#   name          = "rokey-db-private-ip"
#   purpose       = "VPC_PEERING"
#   address_type  = "INTERNAL"
#   prefix_length = 16
#   network       = google_compute_network.rokey_vpc.id
# }
# resource "google_service_networking_connection" "private_vpc" {
#   network                 = google_compute_network.rokey_vpc.id
#   service                 = "servicenetworking.googleapis.com"
#   reserved_peering_ranges = [google_compute_global_address.private_ip.name]
# }
# Luego en google_sql_database_instance.settings.ip_configuration:
#   private_network = google_compute_network.rokey_vpc.id
#   ipv4_enabled    = false
# Y en Cloud Run: vpc_access { connector = ... } para llegar a la IP privada.

resource "google_sql_database" "rokey" {
  name     = var.db_name
  instance = google_sql_database_instance.main.name
}

resource "google_sql_user" "rokey" {
  name     = var.db_user
  instance = google_sql_database_instance.main.name
  # Si var.db_password es null, Terraform pedira valor; en MVP se puede usar var directo.
  # Alternativa prod: crear password en Secret Manager y referenciarlo aqui.
  password = var.db_password
}

# ---------------------------------------------------------------------------
# Secret Manager (opcional - recomendado para prod)
# Para MVP se puede usar var.db_password / var.jwt_key directo (menos seguro).
# Descomentar si se quiere gestionar secretos via GCP en vez de tfvars.
# ---------------------------------------------------------------------------
# resource "google_secret_manager_secret" "db_password" {
#   secret_id = "rokey-db-password"
#   replication {
#     auto {}
#   }
# }
# resource "google_secret_manager_secret_version" "db_password" {
#   secret      = google_secret_manager_secret.db_password.id
#   secret_data = var.db_password
# }
# resource "google_secret_manager_secret" "jwt_key" {
#   secret_id = "rokey-jwt-key"
#   replication {
#     auto {}
#   }
# }
# resource "google_secret_manager_secret_version" "jwt_key" {
#   secret      = google_secret_manager_secret.jwt_key.id
#   secret_data = var.jwt_key
# }
# Nota: Cloud Run puede montar secretos como env vars con value_source.secret_key_ref
# en vez de var directa (ver ejemplo comentado en google_cloud_run_v2_service.backend).

# ---------------------------------------------------------------------------
# Cloud Run v2 - Backend (.NET 9 API)
# ---------------------------------------------------------------------------
resource "google_cloud_run_v2_service" "backend" {
  name     = "rokey-backend"
  location = var.gcp_region

  template {
    scaling {
      min_instance_count = 0
      max_instance_count = 10
    }

    containers {
      # Si var.backend_image es null, usar placeholder para que validate pase;
      # en apply real debe venir de tfvars o CI (GHCR / Artifact Registry).
      image = coalesce(var.backend_image, "us-docker.pkg.dev/cloudrun/container/hello")

      ports {
        container_port = 8080
      }

      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = "Production"
      }
      env {
        name  = "ASPNETCORE_URLS"
        value = "http://+:8080"
      }
      env {
        name  = "DOTNET_RUNNING_IN_CONTAINER"
        value = "true"
      }

      # Connection string ARMADA desde Cloud SQL.
      # MVP usa IP publica del instance (host = primera IP). Para socket unix:
      # Host=/cloudsql/PROJECT:REGION:INSTANCE
      # Ver https://cloud.google.com/sql/docs/postgres/connect-run
      env {
        name  = "ConnectionStrings__WebApiDatabase"
        value = "Host=${google_sql_database_instance.main.public_ip_address};Port=5432;Database=${var.db_name};Username=${var.db_user};Password=${coalesce(var.db_password, "changeme")}"
      }
      env {
        name  = "ConnectionStrings__DefaultConnection"
        value = "Host=${google_sql_database_instance.main.public_ip_address};Port=5432;Database=${var.db_name};Username=${var.db_user};Password=${coalesce(var.db_password, "changeme")}"
      }

      env {
        name  = "JWT__Key"
        value = coalesce(var.jwt_key, "RoKeySuperSecretKey2026!@#$$%^&*()_changeme")
        # Alternativa con Secret Manager (descomentar si se crean los secrets arriba):
        # value_source {
        #   secret_key_ref {
        #     secret  = google_secret_manager_secret.jwt_key.secret_id
        #     version = "latest"
        #   }
        # }
      }
      env {
        name  = "JWT__Issuer"
        value = var.jwt_issuer
      }
      env {
        name  = "JWT__Audience"
        value = var.jwt_audience
      }
      env {
        name  = "JWT__ExpiryMinutes"
        value = var.jwt_expiry_minutes
      }

      resources {
        limits = {
          cpu    = "1000m"
          memory = "512Mi"
        }
        cpu_idle = true
      }

      startup_probe {
        http_get {
          path = "/health"
          port = 8080
        }
        initial_delay_seconds = 10
        period_seconds        = 10
        failure_threshold     = 3
        timeout_seconds       = 5
      }

      liveness_probe {
        http_get {
          path = "/health"
          port = 8080
        }
        period_seconds    = 30
        failure_threshold = 3
        timeout_seconds   = 5
      }
    }

    # Si se usa Cloud SQL con IP privada + VPC, descomentar:
    # vpc_access {
    #   connector = google_vpc_access_connector.rokey.id
    #   egress    = "PRIVATE_RANGES_ONLY"
    # }

    timeout = "300s"
  }

  traffic {
    percent = 100
    type    = "TRAFFIC_TARGET_ALLOCATION_TYPE_LATEST"
  }
}

# ---------------------------------------------------------------------------
# Cloud Run v2 - Admin Panel (nginx 8080, proxy /api en compose, en Cloud Run URL directa)
# ---------------------------------------------------------------------------
resource "google_cloud_run_v2_service" "admin_panel" {
  name     = "rokey-admin"
  location = var.gcp_region

  template {
    scaling {
      min_instance_count = 0
      max_instance_count = 10
    }

    containers {
      image = coalesce(var.admin_image, "us-docker.pkg.dev/cloudrun/container/hello")

      ports {
        container_port = 8080
      }

      # Si el admin necesita saber la URL del backend en prod, inyectar VITE_API_URL
      # al build (ARG) o como runtime env si el nginx lo lee via sub_filter / js config.
      # Para MVP el admin-panel hace proxy /api -> backend:8080 en compose; en Cloud Run
      # conviene setear VITE_API_URL al backend_url.
      env {
        name  = "VITE_API_URL"
        value = google_cloud_run_v2_service.backend.uri
      }

      resources {
        limits = {
          cpu    = "500m"
          memory = "256Mi"
        }
        cpu_idle = true
      }

      startup_probe {
        http_get {
          path = "/health"
          port = 8080
        }
        initial_delay_seconds = 5
        period_seconds        = 10
        failure_threshold     = 3
      }
    }
  }

  traffic {
    percent = 100
    type    = "TRAFFIC_TARGET_ALLOCATION_TYPE_LATEST"
  }
}

# ---------------------------------------------------------------------------
# Cloud Run v2 - Client Portal (SPA nginx 8080)
# ---------------------------------------------------------------------------
resource "google_cloud_run_v2_service" "client_portal" {
  name     = "rokey-client"
  location = var.gcp_region

  template {
    scaling {
      min_instance_count = 0
      max_instance_count = 10
    }

    containers {
      image = coalesce(var.client_image, "us-docker.pkg.dev/cloudrun/container/hello")

      ports {
        container_port = 8080
      }

      env {
        name  = "VITE_API_URL"
        value = google_cloud_run_v2_service.backend.uri
      }

      resources {
        limits = {
          cpu    = "500m"
          memory = "256Mi"
        }
        cpu_idle = true
      }

      startup_probe {
        http_get {
          path = "/"
          port = 8080
        }
        initial_delay_seconds = 5
        period_seconds        = 10
        failure_threshold     = 3
      }
    }
  }

  traffic {
    percent = 100
    type    = "TRAFFIC_TARGET_ALLOCATION_TYPE_LATEST"
  }
}

# ---------------------------------------------------------------------------
# IAM - permitir invocacion publica (unauthenticated)
# ---------------------------------------------------------------------------
resource "google_cloud_run_v2_service_iam_member" "backend_public" {
  location = google_cloud_run_v2_service.backend.location
  project  = var.gcp_project_id
  name     = google_cloud_run_v2_service.backend.name
  role     = "roles/run.invoker"
  member   = "allUsers"
}

resource "google_cloud_run_v2_service_iam_member" "admin_public" {
  location = google_cloud_run_v2_service.admin_panel.location
  project  = var.gcp_project_id
  name     = google_cloud_run_v2_service.admin_panel.name
  role     = "roles/run.invoker"
  member   = "allUsers"
}

resource "google_cloud_run_v2_service_iam_member" "client_public" {
  location = google_cloud_run_v2_service.client_portal.location
  project  = var.gcp_project_id
  name     = google_cloud_run_v2_service.client_portal.name
  role     = "roles/run.invoker"
  member   = "allUsers"
}
