#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd -- "${SCRIPT_DIR}/../.." && pwd)"
cd "${REPO_ROOT}"

: "${AZURE_RESOURCE_GROUP:?Missing AZURE_RESOURCE_GROUP}"
: "${ACR_NAME:?Missing ACR_NAME}"
: "${CONTAINER_APP_NAME:?Missing CONTAINER_APP_NAME}"

IMAGE_NAME="${IMAGE_NAME:-wrblo-sal-api}"
IMAGE_TAG="${GITHUB_SHA:-latest}"
CONTAINER_PORT="${CONTAINER_PORT:-8080}"
IMAGE="${ACR_NAME}.azurecr.io/${IMAGE_NAME}:${IMAGE_TAG}"

az extension add --name containerapp --upgrade

echo "Building ${IMAGE} from the repository root..."
az acr build \
    --registry "${ACR_NAME}" \
    --image "${IMAGE_NAME}:${IMAGE_TAG}" \
    --file "Wrblo-SAL/Dockerfile" \
    .

echo "Configuring managed-identity image pulls..."
az containerapp registry set \
    --name "${CONTAINER_APP_NAME}" \
    --resource-group "${AZURE_RESOURCE_GROUP}" \
    --server "${ACR_NAME}.azurecr.io" \
    --identity system

echo "Configuring Container Apps ingress on port ${CONTAINER_PORT}..."
az containerapp ingress enable \
    --name "${CONTAINER_APP_NAME}" \
    --resource-group "${AZURE_RESOURCE_GROUP}" \
    --type external \
    --target-port "${CONTAINER_PORT}" \
    --transport auto

echo "Deploying ${IMAGE}..."
az containerapp update \
    --name "${CONTAINER_APP_NAME}" \
    --resource-group "${AZURE_RESOURCE_GROUP}" \
    --image "${IMAGE}"

FQDN="$(az containerapp show \
    --name "${CONTAINER_APP_NAME}" \
    --resource-group "${AZURE_RESOURCE_GROUP}" \
    --query "properties.configuration.ingress.fqdn" \
    --output tsv)"

if [[ -z "${FQDN}" ]]; then
    echo "Container App did not return an ingress FQDN" >&2
    exit 1
fi

echo "Container App URL: https://${FQDN}"
curl --fail --silent --show-error --retry 10 --retry-delay 5 "https://${FQDN}/health"
echo
echo "Azure Container Apps deployment completed."
