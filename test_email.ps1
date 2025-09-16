# Script para probar el envío de correo electrónico

$baseUrl = "http://localhost:5086"
$testEmail = "test@example.com"

Write-Host "=== Prueba de Envío de Correo Electrónico ===" -ForegroundColor Green

# Paso 1: Registrar usuario
Write-Host "\n1. Registrando usuario de prueba..." -ForegroundColor Yellow
$registerData = @{
    username = "testuser_$(Get-Random)"
    email = $testEmail
    password = "TestPassword123!"
} | ConvertTo-Json

try {
    $registerResponse = Invoke-WebRequest -Uri "$baseUrl/api/auth/register" -Method POST -Body $registerData -ContentType "application/json"
    Write-Host "Usuario registrado exitosamente" -ForegroundColor Green
    $userInfo = $registerResponse.Content | ConvertFrom-Json
    Write-Host "Token: $($userInfo.token.Substring(0,20))..." -ForegroundColor Cyan
} catch {
    Write-Host "Error en registro: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Paso 2: Crear sala
Write-Host "\n2. Creando sala de juego..." -ForegroundColor Yellow
$roomData = @{
    name = "Sala de Prueba Email"
    maxPlayers = 4
    isPrivate = $false
} | ConvertTo-Json

$headers = @{
    "Authorization" = "Bearer $($userInfo.token)"
    "Content-Type" = "application/json"
}

try {
    $roomResponse = Invoke-WebRequest -Uri "$baseUrl/api/room/create" -Method POST -Body $roomData -Headers $headers
    Write-Host "Sala creada exitosamente" -ForegroundColor Green
    $roomInfo = $roomResponse.Content | ConvertFrom-Json
    Write-Host "ID de sala: $($roomInfo.id)" -ForegroundColor Cyan
} catch {
    Write-Host "Error creando sala: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Paso 3: Crear invitación (esto debería enviar el email)
Write-Host "\n3. Creando invitación (enviando email)..." -ForegroundColor Yellow
$invitationData = @{
    roomId = $roomInfo.id
    email = "destinatario@example.com"
} | ConvertTo-Json

try {
    $invitationResponse = Invoke-WebRequest -Uri "$baseUrl/api/invitation/create" -Method POST -Body $invitationData -Headers $headers
    Write-Host "Invitación creada exitosamente" -ForegroundColor Green
    $invitationInfo = $invitationResponse.Content | ConvertFrom-Json
    Write-Host "ID de invitación: $($invitationInfo.id)" -ForegroundColor Cyan
    Write-Host "Email enviado a: destinatario@example.com" -ForegroundColor Green
} catch {
    Write-Host "Error creando invitación: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Detalles del error: $($_.ErrorDetails.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "\n=== Prueba completada exitosamente ===" -ForegroundColor Green
Write-Host "Si no hay errores arriba, el email debería haberse enviado correctamente." -ForegroundColor Cyan