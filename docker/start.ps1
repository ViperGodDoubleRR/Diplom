$ErrorActionPreference = "Stop"

function Test-DockerReady {
    docker version --format "{{.Server.Version}}" 2>$null | Out-Null
    return $LASTEXITCODE -eq 0
}

function Start-DockerDesktop {
    $paths = @(
        "${env:ProgramFiles}\Docker\Docker\Docker Desktop.exe",
        "${env:ProgramFiles(x86)}\Docker\Docker\Docker Desktop.exe",
        "$env:LOCALAPPDATA\Programs\Docker\Docker\Docker Desktop.exe"
    )
    foreach ($path in $paths) {
        if (Test-Path $path) {
            Write-Host "Starting Docker Desktop..." -ForegroundColor Yellow
            Start-Process $path
            return $true
        }
    }
    return $false
}

Write-Host "Checking Docker..." -ForegroundColor Cyan

if (-not (Test-DockerReady)) {
    Write-Host ""
    Write-Host "Docker is not running (dockerDesktopLinuxEngine pipe missing)." -ForegroundColor Red

    if (Start-DockerDesktop) {
        Write-Host "Waiting for Docker engine (up to 2 minutes)..." -ForegroundColor Yellow
        $deadline = (Get-Date).AddMinutes(2)
        while ((Get-Date) -lt $deadline) {
            Start-Sleep -Seconds 3
            if (Test-DockerReady) {
                Write-Host "Docker is ready." -ForegroundColor Green
                break
            }
        }
    }
}

if (-not (Test-DockerReady)) {
    Write-Host ""
    Write-Host "Could not connect to Docker." -ForegroundColor Red
    Write-Host ""
    Write-Host "Try:" -ForegroundColor Yellow
    Write-Host "  1. Open Docker Desktop and wait for 'Engine running'"
    Write-Host "  2. Restart Docker Desktop if it is stuck"
    Write-Host "  3. Run as Admin: Start-Service com.docker.service"
    Write-Host ""
    Write-Host "Check: docker version  (both Client and Server must appear)" -ForegroundColor White
    Write-Host "Then run again: .\docker\start.ps1" -ForegroundColor White
    exit 1
}

$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

if (-not (Test-Path ".env")) {
    Copy-Item ".env.example" ".env"
    Write-Host "Created .env from .env.example" -ForegroundColor Green
}

$build = $args -contains "--build"

if ($build) {
    Write-Host "Building images..." -ForegroundColor Cyan
    docker compose build
}

Write-Host "Starting Neura in background..." -ForegroundColor Cyan
Write-Host "App:      http://localhost:8080" -ForegroundColor Gray
Write-Host "API:      http://localhost:5107" -ForegroundColor Gray
Write-Host "Media:    http://localhost:9000 (MinIO, photos/videos)" -ForegroundColor Gray
docker compose up -d --remove-orphans

Write-Host ""
Write-Host "Stack is starting. First boot can take 1-2 min (SQL migrations)." -ForegroundColor Yellow
Write-Host "Logs: docker compose logs -f" -ForegroundColor Gray
Write-Host "Rebuild: .\docker\start.ps1 --build" -ForegroundColor Gray
