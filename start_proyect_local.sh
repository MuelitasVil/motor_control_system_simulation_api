#!/usr/bin/env bash

set -euo pipefail

PROJECT_ROOT="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
BACKEND_DIR="$PROJECT_ROOT/back_end_control_system_simulation"
BACKEND_PROJECT="$BACKEND_DIR/motor_control_system_simulation_api/motor_control_system_simulation_api.csproj"
FRONTEND_DIR="$PROJECT_ROOT/front_end/motor_control_system"

if [[ ! -f "$BACKEND_PROJECT" ]]; then
  echo "Backend project was not found: $BACKEND_PROJECT" >&2
  exit 1
fi

if [[ ! -f "$FRONTEND_DIR/package.json" ]]; then
  echo "Frontend project was not found: $FRONTEND_DIR" >&2
  exit 1
fi

if ! command -v dotnet >/dev/null 2>&1; then
  echo "The .NET SDK is required but dotnet was not found in PATH." >&2
  exit 1
fi

if ! command -v npm >/dev/null 2>&1; then
  echo "Node.js and npm are required but npm was not found in PATH." >&2
  exit 1
fi

if [[ ! -d "$FRONTEND_DIR/node_modules" ]]; then
  echo "Installing frontend dependencies..."
  (cd "$FRONTEND_DIR" && npm install)
fi

cleanup() {
  echo "Stopping API and frontend..."
  kill "$BACKEND_PID" "$FRONTEND_PID" 2>/dev/null || true
  wait "$BACKEND_PID" "$FRONTEND_PID" 2>/dev/null || true
}
trap cleanup EXIT INT TERM

echo "Starting API at http://localhost:5168..."
dotnet run --project "$BACKEND_PROJECT" &
BACKEND_PID=$!

echo "Starting frontend at http://localhost:5173..."
(cd "$FRONTEND_DIR" && npm run dev -- --host 0.0.0.0) &
FRONTEND_PID=$!

echo "Both services are running. Press Ctrl+C to stop them."
wait
