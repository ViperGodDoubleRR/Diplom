# GoatBridge backend

## Local build

```bash
dotnet restore back.sln
dotnet build back.sln --no-restore
```

## Docker Compose

Copy the example environment file if you want to override default local values:

```bash
cp ../.env.example ../.env
docker compose up --build
```

Gateway URL: `http://localhost:5107`

Main routes:

- `GET /health`
- `/auth/*`
- `/reg/*`
- `/user/*`
- `/post/*`
- `/comment/*`
- `/chat/*`

If SMTP settings are empty, confirmation codes are written to the service console. Set `SMTP_HOST`, `SMTP_LOGIN`, `SMTP_PASSWORD`, and `SMTP_FROM` in `.env` to send real emails.
