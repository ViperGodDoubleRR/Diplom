#!/bin/sh
set -eu

wait_tcp() {
  host="$1"
  port="$2"
  name="$3"
  attempt=1
  max_attempts=90

  echo "Waiting for ${name} (${host}:${port})..."

  while [ "$attempt" -le "$max_attempts" ]; do
    if nc -z "$host" "$port" 2>/dev/null; then
      echo "${name} is reachable."
      return 0
    fi

    attempt=$((attempt + 1))
    sleep 2
  done

  echo "Timeout waiting for ${name}."
  exit 1
}

wait_tcp "${SQL_HOST:-sqlserver}" "${SQL_PORT:-1433}" "SQL Server"
wait_tcp "${RABBITMQ_HOST:-rabbitmq}" "${RABBITMQ_PORT:-5672}" "RabbitMQ"

if [ "${WAIT_MINIO:-false}" = "true" ]; then
  wait_tcp "${MINIO_HOST:-minio}" "${MINIO_PORT:-9000}" "MinIO"
fi

exec "$@"
