
#!/usr/bin/env bash

set -euo pipefail

echo "Starting Railway development deployment..."

if [[ -z "${RAILWAY_TOKEN:-}" ]]; then
    echo "RAILWAY_TOKEN is missing"
    exit 1
fi

npm install --global @railway/cli

echo "Deploying application to Railway..."

railway up \
    --service "${RAILWAY_SERVICE}" \
    --environment "${RAILWAY_ENVIRONMENT}" \
    --ci

echo "Railway deployment completed."
