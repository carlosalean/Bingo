# Script para probar el endpoint de invitaciones y diagnosticar el error 403
$ErrorActionPreference = "Continue"

# Configuración
$baseUrl = "http://localhost:5086/api"
$roomId = "870f2488-4149-4ed0-9e4e-e97e6b421986"

# Generar credenciales únicas
$timestamp = Get-Date -Format "yyyyMMddHHmmss"
$username = "testuser_$timestamp"
$email = "test_$timestamp@example.com"
$password = "TestPassword123!"

Write-Host "=== PRUEBA DE INVITACIONES - DIAGNOSTICO 403 ===" -ForegroundColor Yellow
Write-Host "Room ID: $roomId" -ForegroundColor Gray
Write-Host ""

# Función para hacer solicitudes HTTP
function Invoke-ApiRequest {
    param(
        [string]$Method,
        [string]$Uri,
        [hashtable]$Headers = @{},
        [object]$Body = $null
    )
    
    try {
        $params = @{
            Method = $Method
            Uri = $Uri
            Headers = $Headers
            ContentType = "application/json"
        }
        
        if ($Body) {
            $params.Body = ($Body | ConvertTo-Json -Depth 10)
        }
        
        $response = Invoke-RestMethod @params
        return @{ Success = $true; Data = $response; StatusCode = 200 }
    }
    catch {
        $statusCode = if ($_.Exception.Response) { $_.Exception.Response.StatusCode.value__ } else { 0 }
        $errorMessage = $_.Exception.Message
        
        return @{ 
            Success = $false
            StatusCode = $statusCode
            Error = $errorMessage
        }
    }
}

# 1. Registro de usuario
Write-Host "1. Registrando usuario..." -ForegroundColor Cyan
$registerData = @{
    username = $username
    email = $email
    password = $password
}

$registerResult = Invoke-ApiRequest -Method "POST" -Uri "$baseUrl/auth/register" -Body $registerData

if ($registerResult.Success) {
    Write-Host "Registro exitoso" -ForegroundColor Green
} else {
    Write-Host "Error en registro (continuando): $($registerResult.Error)" -ForegroundColor Yellow
}

# 2. Login
Write-Host "2. Iniciando sesion..." -ForegroundColor Cyan
$loginData = @{
    usernameOrEmail = $username
    password = $password
}

$loginResult = Invoke-ApiRequest -Method "POST" -Uri "$baseUrl/auth/login" -Body $loginData

if ($loginResult.Success) {
    Write-Host "Login exitoso" -ForegroundColor Green
    $token = $loginResult.Data.token
    $user = $loginResult.Data.user
    Write-Host "User ID: $($user.id)" -ForegroundColor Gray
    Write-Host "Role: $($user.role)" -ForegroundColor Gray
} else {
    Write-Host "Error en login: $($loginResult.Error)" -ForegroundColor Red
    exit 1
}

# 3. Configurar headers de autenticación
$authHeaders = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# 4. Intentar obtener invitaciones (aquí debería ocurrir el error 403)
Write-Host "3. Intentando obtener invitaciones..." -ForegroundColor Cyan
$invitationsResult = Invoke-ApiRequest -Method "GET" -Uri "$baseUrl/invitation/room/$roomId" -Headers $authHeaders

if ($invitationsResult.Success) {
    Write-Host "Invitaciones obtenidas exitosamente" -ForegroundColor Green
    Write-Host "Numero de invitaciones: $($invitationsResult.Data.Count)" -ForegroundColor Gray
} else {
    Write-Host "Error al obtener invitaciones" -ForegroundColor Red
    Write-Host "Status Code: $($invitationsResult.StatusCode)" -ForegroundColor Red
    Write-Host "Error: $($invitationsResult.Error)" -ForegroundColor Red
}

# 5. Verificar si la sala existe y quien es el host
Write-Host "4. Verificando informacion de la sala..." -ForegroundColor Cyan
$roomResult = Invoke-ApiRequest -Method "GET" -Uri "$baseUrl/room/$roomId" -Headers $authHeaders

if ($roomResult.Success) {
    Write-Host "Sala encontrada" -ForegroundColor Green
    Write-Host "Nombre: $($roomResult.Data.name)" -ForegroundColor Gray
    Write-Host "Host ID: $($roomResult.Data.hostId)" -ForegroundColor Gray
    Write-Host "Usuario actual ID: $($user.id)" -ForegroundColor Gray
    
    if ($roomResult.Data.hostId -eq $user.id) {
        Write-Host "El usuario ES el host de la sala" -ForegroundColor Green
    } else {
        Write-Host "El usuario NO es el host de la sala" -ForegroundColor Red
        Write-Host "CAUSA DEL ERROR 403: Solo el host puede ver las invitaciones" -ForegroundColor Yellow
    }
} else {
    Write-Host "Error al obtener informacion de la sala" -ForegroundColor Red
    Write-Host "Status Code: $($roomResult.StatusCode)" -ForegroundColor Red
    
    if ($roomResult.StatusCode -eq 404) {
        Write-Host "CAUSA DEL ERROR 403: La sala no existe" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "=== FIN DE LA PRUEBA ===" -ForegroundColor Yellow