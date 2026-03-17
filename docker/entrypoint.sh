#!/bin/bash
set -e
# Start PostgreSQL using the official image entrypoint (handles init, POSTGRES_PASSWORD, etc.)
/usr/local/bin/docker-entrypoint.sh postgres &
# Wait for Postgres to be ready
until pg_isready -U "${POSTGRES_USER:-postgres}"; do
  sleep 0.5
done
exec "$@"
