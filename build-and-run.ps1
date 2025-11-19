Write-Host "Stopping any running containers..." -ForegroundColor Green
docker-compose down

Write-Host "Building and starting containers..." -ForegroundColor Green
docker-compose build --no-cache
docker-compose up -d

Write-Host "Containers are starting..." -ForegroundColor Green
Write-Host "Check status with: docker-compose ps" -ForegroundColor Yellow
Write-Host "View logs with: docker-compose logs [service-name]" -ForegroundColor Yellow

# Проверка статуса контейнеров
Start-Sleep -Seconds 10
docker-compose ps