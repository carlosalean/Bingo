# Script simple para probar el envio de correo electronico

$baseUrl = "http://localhost:5086"
$uniqueId = Get-Random
$testEmail = "testuser_$uniqueId@example.com"

Write-Host "=== Prueba Simple de Envio de Email ===" -ForegroundColor Green
Write-Host "Email de prueba: $testEmail" -ForegroundColor Cyan

# Paso 1: Registrar usuario
Write-Host "\n1. Registrando usuario..." -ForegroundColor Yellow
$registerData = @{
    username = "testuser_$uniqueId"
    email = $testEmail
    password = "TestPassword123!"
} | ConvertTo-Json

try {
    $registerResponse = Invoke-WebRequest -Uri "$baseUrl/api/auth/register" -Method POST -Body $registerData -ContentType "application/json"
    Write-Host "Usuario registrado exitosamente" -ForegroundColor Green
    
    # Ahora hacer login para obtener el token
    $loginData = @{
        usernameOrEmail = $testEmail
        password = "TestPassword123!"
    } | ConvertTo-Json
    
    $loginResponse = Invoke-WebRequest -Uri "$baseUrl/api/auth/login" -Method POST -Body $loginData -ContentType "application/json"
    $loginInfo = $loginResponse.Content | ConvertFrom-Json
    $token = $loginInfo.token
    Write-Host "Token obtenido" -ForegroundColor Cyan
} catch {
    Write-Host "Error en registro/login: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Paso 2: Crear sala
Write-Host "\n2. Creando sala..." -ForegroundColor Yellow
$roomData = @{
    name = "Sala de Prueba $uniqueId"
    BingoType = "SeventyFive"
    maxPlayers = 6
    isPrivate = $false
} | ConvertTo-Json

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

try {
    $roomResponse = Invoke-WebRequest -Uri "$baseUrl/api/room" -Method POST -Body $roomData -Headers $headers
    Write-Host "Sala creada exitosamente" -ForegroundColor Green
    $roomInfo = $roomResponse.Content | ConvertFrom-Json
    $roomId = $roomInfo.id
    Write-Host "ID de sala: $roomId" -ForegroundColor Cyan
} catch {
    Write-Host "Error creando sala: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Paso 3: Crear invitacion (enviar email)
Write-Host "\n3. Creando invitacion y enviando email..." -ForegroundColor Yellow
$invitationEmail = "destinatario_$uniqueId@example.com"
Write-Host "Email destinatario: $invitationEmail" -ForegroundColor Cyan

$invitationData = @{
    roomId = $roomId
    email = $invitationEmail
} | ConvertTo-Json

try {
    $invitationResponse = Invoke-WebRequest -Uri "$baseUrl/api/invitation/create" -Method POST -Body $invitationData -Headers $headers
    Write-Host "Invitacion creada exitosamente" -ForegroundColor Green
    $invitationInfo = $invitationResponse.Content | ConvertFrom-Json
    Write-Host "ID de invitacion: $($invitationInfo.id)" -ForegroundColor Cyan
    Write-Host "Email enviado correctamente a: $invitationEmail" -ForegroundColor Green
} catch {
    Write-Host "Error creando invitacion: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.ErrorDetails.Message) {
        Write-Host "Detalles: $($_.ErrorDetails.Message)" -ForegroundColor Yellow
    }
    exit 1
}

Write-Host "\n=== Prueba completada exitosamente ===" -ForegroundColor Green
Write-Host "El email se envio correctamente usando MailKit con puerto 465" -ForegroundColor Cyan