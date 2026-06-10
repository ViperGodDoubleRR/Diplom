#!/bin/sh
set -eu

services="authservice regservice userservice postservice commentservice chatservice"
attempt=1
max_attempts=45

echo "Waiting for microservices..."

while [ "$attempt" -le "$max_attempts" ]; do
  all_ready=true

  for service in $services; do
    if ! nc -z "$service" 8080 2>/dev/null; then
      all_ready=false
      break
    fi
  done

  if [ "$all_ready" = "true" ]; then
    echo "All microservices are reachable."
    exec "$@"
  fi

  attempt=$((attempt + 1))
  sleep 2
done

echo "Timeout waiting for microservices."
exit 1
