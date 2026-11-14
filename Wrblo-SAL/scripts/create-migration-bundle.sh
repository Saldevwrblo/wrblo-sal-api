#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd -- "${SCRIPT_DIR}/../.." && pwd)"
cd "${REPO_ROOT}"

API_PROJECT="Wrblo-SAL/Wrblo.SAL.Api.csproj"
OUTPUT_PATH="artifacts/efbundle"

mkdir -p "$(dirname -- "${OUTPUT_PATH}")"
dotnet tool restore

dotnet ef migrations bundle \
    --project "${API_PROJECT}" \
    --startup-project "${API_PROJECT}" \
    --configuration Release \
    --self-contained \
    --runtime linux-x64 \
    --output "${OUTPUT_PATH}"

echo "Migration bundle generated at ${OUTPUT_PATH}."