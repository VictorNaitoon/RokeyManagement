# =============================================================================
# RoKey MANAGEMENT - outputs.tf
# =============================================================================

output "artifact_registry_url" {
  description = "URL del Artifact Registry repo"
  value       = "${var.gcp_region}-docker.pkg.dev/${var.gcp_project_id}/${google_artifact_registry_repository.rokey.repository_id}"
}

output "cloud_sql_connection_name" {
  description = "Connection name de Cloud SQL (para --add-cloudsql-instances o socket)"
  value       = google_sql_database_instance.main.connection_name
}

output "cloud_sql_public_ip" {
  description = "IP publica de Cloud SQL (MVP)"
  value       = google_sql_database_instance.main.public_ip_address
}

output "backend_url" {
  description = "URL publica del backend (Cloud Run)"
  value       = google_cloud_run_v2_service.backend.uri
}

output "admin_url" {
  description = "URL publica del admin-panel (Cloud Run)"
  value       = google_cloud_run_v2_service.admin_panel.uri
}

output "client_url" {
  description = "URL publica del client-portal (Cloud Run)"
  value       = google_cloud_run_v2_service.client_portal.uri
}
