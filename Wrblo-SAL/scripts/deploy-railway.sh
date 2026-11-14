#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd -- "${SCRIPT_DIR}/../.." && pwd)"
cd "${REPO_ROOT}"

required_variables=(RAILWAY_TOKEN RAILWAY_SERVICE RAILWAY_ENVIRONMENT)
for variable in "${required_variables[@]}"; do
    if [[ -z "${!variable:-}" ]]; then
        echo "${variable} is missing" >&2
        exit 1
    fi
done

if ! command -v railway >/dev/null 2>&1; then
    npm install --global @railway/cli
fi

echo "Deploying ${RAILWAY_SERVICE} to Railway environment ${RAILWAY_ENVIRONMENT}..."
railway up \
    --service "${RAILWAY_SERVICE}" \
    --environment "${RAILWAY_ENVIRONMENT}" \
    --ci

echo "Railway deployment completed."
