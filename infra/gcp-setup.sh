#!/usr/bin/env bash
# =============================================================================
# FPL Tool — GCP Infrastructure Setup
# Run this once from Google Cloud Shell or locally with gcloud authenticated.
#
# Prerequisites:
#   gcloud auth login
#   gcloud config set project YOUR_PROJECT_ID
#
# Usage:
#   1. Fill in the three variables below
#   2. chmod +x infra/gcp-setup.sh
#   3. ./infra/gcp-setup.sh
# =============================================================================

set -euo pipefail

# ── CONFIGURE THESE ──────────────────────────────────────────────────────────
PROJECT_ID="fpl-tool-490409"       # e.g. "fpltool-prod"
REGION="europe-west2"                   # Cloud Run + Cloud SQL region
GITHUB_REPO="tszymanski-devdoc/fpl-tool"        # e.g. "jsmith/fpl-tool"
# ─────────────────────────────────────────────────────────────────────────────

# Derived values
SA_NAME="fpltool-api"
SA_EMAIL="${SA_NAME}@${PROJECT_ID}.iam.gserviceaccount.com"
DB_INSTANCE="fpltool-mysql"
DB_NAME="fpltool"
DB_USER="fpltool"
ARTIFACT_REPO="fpltool"
WIF_POOL="github-actions-pool"
WIF_PROVIDER="github-actions-provider"

# Auto-generated secrets (saved to .secrets.local — DO NOT COMMIT)
DB_ROOT_PASSWORD="$(openssl rand -base64 24 | tr -d '/+=')"
DB_APP_PASSWORD="$(openssl rand -base64 24 | tr -d '/+=')"
JWT_SIGNING_KEY="$(openssl rand -base64 48 | tr -d '/+=')"

SECRETS_FILE="$(dirname "$0")/.secrets.local"

echo "=== FPL Tool GCP Setup ==="
echo "Project:  $PROJECT_ID"
echo "Region:   $REGION"
echo "GitHub:   $GITHUB_REPO"
echo ""

# =============================================================================
# 1. Set project and enable APIs
# =============================================================================
echo "--- [1/7] Setting project and enabling APIs ---"

gcloud config set project "$PROJECT_ID"

gcloud services enable \
  sqladmin.googleapis.com \
  secretmanager.googleapis.com \
  run.googleapis.com \
  artifactregistry.googleapis.com \
  iam.googleapis.com \
  cloudresourcemanager.googleapis.com \
  iamcredentials.googleapis.com \
  --project="$PROJECT_ID"

echo "APIs enabled."

# =============================================================================
# 2. Cloud SQL — MySQL 8
# =============================================================================
echo ""
echo "--- [2/7] Creating Cloud SQL instance (this takes ~5 minutes) ---"

if gcloud sql instances describe "$DB_INSTANCE" --project="$PROJECT_ID" &>/dev/null; then
  echo "Instance $DB_INSTANCE already exists, skipping creation."
else
  gcloud sql instances create "$DB_INSTANCE" \
    --database-version=MYSQL_8_0 \
    --tier=db-f1-micro \
    --region="$REGION" \
    --root-password="$DB_ROOT_PASSWORD" \
    --project="$PROJECT_ID"
  echo "Cloud SQL instance created."
fi

echo "Creating database '$DB_NAME'..."
gcloud sql databases create "$DB_NAME" \
  --instance="$DB_INSTANCE" \
  --project="$PROJECT_ID" 2>/dev/null || echo "Database already exists."

echo "Creating database user '$DB_USER'..."
gcloud sql users create "$DB_USER" \
  --instance="$DB_INSTANCE" \
  --password="$DB_APP_PASSWORD" \
  --project="$PROJECT_ID" 2>/dev/null || \
  gcloud sql users set-password "$DB_USER" \
    --instance="$DB_INSTANCE" \
    --password="$DB_APP_PASSWORD" \
    --project="$PROJECT_ID"

INSTANCE_CONNECTION_NAME="${PROJECT_ID}:${REGION}:${DB_INSTANCE}"
DB_CONNECTION_STRING="Server=/cloudsql/${INSTANCE_CONNECTION_NAME};Database=${DB_NAME};User Id=${DB_USER};Password=${DB_APP_PASSWORD};AllowPublicKeyRetrieval=True;"

echo "Cloud SQL ready. Instance connection name: $INSTANCE_CONNECTION_NAME"

# =============================================================================
# 3. Secret Manager secrets
# =============================================================================
echo ""
echo "--- [3/7] Creating Secret Manager secrets ---"

create_or_update_secret() {
  local SECRET_ID="$1"
  local SECRET_VALUE="$2"

  if gcloud secrets describe "$SECRET_ID" --project="$PROJECT_ID" &>/dev/null; then
    echo "  Updating secret: $SECRET_ID"
    echo -n "$SECRET_VALUE" | gcloud secrets versions add "$SECRET_ID" \
      --data-file=- \
      --project="$PROJECT_ID"
  else
    echo "  Creating secret: $SECRET_ID"
    echo -n "$SECRET_VALUE" | gcloud secrets create "$SECRET_ID" \
      --data-file=- \
      --replication-policy=automatic \
      --project="$PROJECT_ID"
  fi
}

create_or_update_secret "fpltool-db-connection-string" "$DB_CONNECTION_STRING"
create_or_update_secret "fpltool-jwt-signing-key"       "$JWT_SIGNING_KEY"
create_or_update_secret "fpltool-google-client-id"      "PLACEHOLDER_UPDATE_WITH_YOUR_OAUTH_CLIENT_ID"

echo "Secrets created."

# =============================================================================
# 4. Artifact Registry
# =============================================================================
echo ""
echo "--- [4/7] Creating Artifact Registry repository ---"

if gcloud artifacts repositories describe "$ARTIFACT_REPO" \
  --location="$REGION" \
  --project="$PROJECT_ID" &>/dev/null; then
  echo "Repository $ARTIFACT_REPO already exists."
else
  gcloud artifacts repositories create "$ARTIFACT_REPO" \
    --repository-format=docker \
    --location="$REGION" \
    --description="FPL Tool Docker images" \
    --project="$PROJECT_ID"
  echo "Artifact Registry repository created."
fi

# =============================================================================
# 5. Cloud Run service account
# =============================================================================
echo ""
echo "--- [5/7] Creating Cloud Run service account ---"

if gcloud iam service-accounts describe "$SA_EMAIL" --project="$PROJECT_ID" &>/dev/null; then
  echo "Service account $SA_EMAIL already exists."
else
  gcloud iam service-accounts create "$SA_NAME" \
    --display-name="FPL Tool API Service Account" \
    --project="$PROJECT_ID"
  echo "Service account created."
fi

echo "Granting roles..."

gcloud projects add-iam-policy-binding "$PROJECT_ID" \
  --member="serviceAccount:${SA_EMAIL}" \
  --role="roles/cloudsql.client" \
  --condition=None

gcloud projects add-iam-policy-binding "$PROJECT_ID" \
  --member="serviceAccount:${SA_EMAIL}" \
  --role="roles/secretmanager.secretAccessor" \
  --condition=None

echo "Service account roles granted."

# =============================================================================
# 6. Workload Identity Federation for GitHub Actions
# =============================================================================
echo ""
echo "--- [6/7] Setting up Workload Identity Federation ---"

# Create pool
if gcloud iam workload-identity-pools describe "$WIF_POOL" \
  --location=global \
  --project="$PROJECT_ID" &>/dev/null; then
  echo "WIF pool already exists."
else
  gcloud iam workload-identity-pools create "$WIF_POOL" \
    --location=global \
    --display-name="GitHub Actions Pool" \
    --project="$PROJECT_ID"
  echo "WIF pool created."
fi

WIF_POOL_NAME="projects/$(gcloud projects describe "$PROJECT_ID" --format='value(projectNumber)')/locations/global/workloadIdentityPools/${WIF_POOL}"

# Create provider
if gcloud iam workload-identity-pools providers describe "$WIF_PROVIDER" \
  --workload-identity-pool="$WIF_POOL" \
  --location=global \
  --project="$PROJECT_ID" &>/dev/null; then
  echo "WIF provider already exists."
else
  gcloud iam workload-identity-pools providers create-oidc "$WIF_PROVIDER" \
    --workload-identity-pool="$WIF_POOL" \
    --location=global \
    --issuer-uri="https://token.actions.githubusercontent.com" \
    --attribute-mapping="google.subject=assertion.sub,attribute.repository=assertion.repository,attribute.actor=assertion.actor" \
    --attribute-condition="assertion.repository=='${GITHUB_REPO}'" \
    --project="$PROJECT_ID"
  echo "WIF provider created."
fi

# Allow GitHub Actions to impersonate the service account
gcloud iam service-accounts add-iam-policy-binding "$SA_EMAIL" \
  --project="$PROJECT_ID" \
  --role="roles/iam.workloadIdentityUser" \
  --member="principalSet://iam.googleapis.com/${WIF_POOL_NAME}/attribute.repository/${GITHUB_REPO}"

WIF_PROVIDER_FULL="${WIF_POOL_NAME}/providers/${WIF_PROVIDER}"

echo "Workload Identity Federation configured."

# =============================================================================
# 7. Save secrets locally and print GitHub Actions secrets
# =============================================================================
echo ""
echo "--- [7/7] Saving local reference and printing next steps ---"

cat > "$SECRETS_FILE" <<EOF
# FPL Tool — GCP Secrets Reference
# Generated: $(date -u)
# DO NOT COMMIT THIS FILE

PROJECT_ID=$PROJECT_ID
REGION=$REGION
DB_INSTANCE=$DB_INSTANCE
DB_ROOT_PASSWORD=$DB_ROOT_PASSWORD
DB_APP_PASSWORD=$DB_APP_PASSWORD
JWT_SIGNING_KEY=$JWT_SIGNING_KEY
INSTANCE_CONNECTION_NAME=$INSTANCE_CONNECTION_NAME
SA_EMAIL=$SA_EMAIL
WIF_PROVIDER_FULL=$WIF_PROVIDER_FULL
EOF

chmod 600 "$SECRETS_FILE"
echo "Secrets saved to $SECRETS_FILE (chmod 600)"

# =============================================================================
# Summary — GitHub Actions secrets to set
# =============================================================================
echo ""
echo "============================================================"
echo "  SETUP COMPLETE — Add these secrets to GitHub Actions:"
echo "  (Settings → Secrets and variables → Actions → New secret)"
echo "============================================================"
echo ""
echo "  GCP_WORKLOAD_IDENTITY_PROVIDER = $WIF_PROVIDER_FULL"
echo "  GCP_SERVICE_ACCOUNT_EMAIL      = $SA_EMAIL"
echo "  GCP_PROJECT_ID                 = $PROJECT_ID"
echo "  GCP_REGION                     = $REGION"
echo "  CLOUD_SQL_INSTANCE_CONNECTION_NAME = $INSTANCE_CONNECTION_NAME"
echo ""
echo "============================================================"
echo "  MANUAL STEP — Set your Google OAuth Client ID:"
echo "============================================================"
echo ""
echo "  1. Go to: https://console.cloud.google.com/apis/credentials"
echo "  2. Create an OAuth 2.0 Client ID (Web application)"
echo "  3. Add your Cloud Run URL to Authorised JavaScript origins"
echo "  4. Run:"
echo "     echo 'YOUR_CLIENT_ID' | gcloud secrets versions add fpltool-google-client-id --data-file=- --project=$PROJECT_ID"
echo ""
echo "Done!"
