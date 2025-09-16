# Script para probar el endpoint de eliminar invitación
$baseUrl = "http://localhost:5086"

# Función para hacer solicitudes HTTP
function Invoke-ApiRequest {
    param(
        [string]$Url,
        [string]$Method = "GET",
        [hashtable]$Headers = @{},
        [string]$Body = $null
    )
    
    try {
        $params = @{
            Uri = $Url
            Method = $Method
            Headers = $Headers
            ContentType = "application/json"
        }
        
        if ($Body) {
            $params.Body = $Body
        }
        
        $response = Invoke-RestMethod @params
        return $response
    }
    catch {
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "Status Code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
        return $null
    }
}

Write-Host "=== Probando endpoint de eliminar invitación ===" -ForegroundColor Green

# 1. Registrar usuario
Write-Host "\n1. Registrando usuario..." -ForegroundColor Yellow
$timestamp = Get-Date -Format "yyyyMMddHHmmss"
$registerData = @{
    email = "testdelete$timestamp@example.com"
    password = "Test123!"
    confirmPassword = "Test123!"
} | ConvertTo-Json

$registerResponse = Invoke-ApiRequest -Url "$baseUrl/api/auth/register" -Method "POST" -Body $registerData
if ($registerResponse) {
    Write-Host "Usuario registrado exitosamente" -ForegroundColor Green
} else {
    Write-Host "Error al registrar usuario" -ForegroundColor Red
    exit 1
}

# 2. Hacer login
Write-Host "\n2. Haciendo login..." -ForegroundColor Yellow
$loginData = @{
    email = "testdelete$timestamp@example.com"
    password = "Test123!"
} | ConvertTo-Json

$loginResponse = Invoke-ApiRequest -Url "$baseUrl/api/auth/login" -Method "POST" -Body $loginData
if ($loginResponse -and $loginResponse.token) {
    Write-Host "Login exitoso" -ForegroundColor Green
    $token = $loginResponse.token
    Write-Host "Token obtenido: $($token.Substring(0, 20))..." -ForegroundColor Cyan
    Write-Host "Rol del usuario: $($loginResponse.user.role)" -ForegroundColor Cyan
} else {
    Write-Host "Error al hacer login" -ForegroundColor Red
    exit 1
}

# 3. Crear una sala
Write-Host "\n3. Creando sala..." -ForegroundColor Yellow
$roomData = @{
    name = "Sala de Prueba Delete"
    maxPlayers = 10
    isPrivate = $false
} | ConvertTo-Json

$headers = @{ "Authorization" = "Bearer $token" }
$roomResponse = Invoke-ApiRequest -Url "$baseUrl/api/room" -Method "POST" -Headers $headers -Body $roomData
if ($roomResponse -and $roomResponse.id) {
    Write-Host "Sala creada exitosamente" -ForegroundColor Green
    $roomId = $roomResponse.id
    Write-Host "Room ID: $roomId" -ForegroundColor Cyan
} else {
    Write-Host "Error al crear sala" -ForegroundColor Red
    exit 1
}

# 4. Crear una invitación
Write-Host "\n4. Creando invitación..." -ForegroundColor Yellow
$invitationData = @{
    roomId = $roomId
    email = "invitado$timestamp@example.com"
} | ConvertTo-Json

$invitationResponse = Invoke-ApiRequest -Url "$baseUrl/api/invitation/create" -Method "POST" -Headers $headers -Body $invitationData
if ($invitationResponse -and $invitationResponse.id) {
    Write-Host "Invitación creada exitosamente" -ForegroundColor Green
    $invitationId = $invitationResponse.id
    Write-Host "Invitation ID: $invitationId" -ForegroundColor Cyan
} else {
    Write-Host "Error al crear invitación" -ForegroundColor Red
    exit 1
}

# 5. Probar eliminar la invitación
Write-Host "\n5. Probando eliminar invitación..." -ForegroundColor Yellow
$deleteResponse = Invoke-ApiRequest -Url "$baseUrl/api/invitation/$invitationId" -Method "DELETE" -Headers $headers
if ($deleteResponse -ne $null) {
    Write-Host "¡Invitación eliminada exitosamente!" -ForegroundColor Green
} else {
    Write-Host "Error al eliminar invitación" -ForegroundColor Red
}

# 6. Verificar que la invitación fue eliminada
Write-Host "\n6. Verificando que la invitación fue eliminada..." -ForegroundColor Yellow
$getResponse = Invoke-ApiRequest -Url "$baseUrl/api/invitation/$invitationId" -Method "GET"
if ($getResponse -eq $null) {
    Write-Host "Confirmado: La invitación ya no existe" -ForegroundColor Green
} else {
    Write-Host "La invitación aún existe" -ForegroundColor Yellow
}

Write-Host "\n=== Prueba completada ===" -ForegroundColor Green