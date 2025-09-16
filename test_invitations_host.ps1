# Script para probar invitaciones con el usuario host
$baseUrl = "http://localhost:5086/api"
$roomId = "870f2488-4149-4ed0-9e4e-e97e6b421986"
$hostId = "03193a7a-bfd9-436e-aeb0-29f8cc7aa484"

Write-Host "=== PRUEBA DE INVITACIONES CON USUARIO HOST ===" -ForegroundColor Green
Write-Host "Room ID: $roomId"
Write-Host "Host ID: $hostId"
Write-Host ""

# Función para hacer peticiones HTTP
function Invoke-ApiRequest {
    param(
        [string]$Url,
        [string]$Method = "GET",
        [object]$Body = $null,
        [hashtable]$Headers = @{}
    )
    
    try {
        $params = @{
            Uri = $Url
            Method = $Method
            Headers = $Headers
            ContentType = "application/json"
        }
        
        if ($Body) {
            $params.Body = $Body | ConvertTo-Json
        }
        
        $response = Invoke-RestMethod @params
        return @{ Success = $true; Data = $response }
    }
    catch {
        return @{ Success = $false; Error = $_.Exception.Message; StatusCode = $_.Exception.Response.StatusCode }
    }
}

# 1. Intentar login con credenciales del host (necesitamos encontrar las credenciales)
Write-Host "1. Buscando usuario host en la base de datos..." -ForegroundColor Yellow

# Como no tenemos las credenciales del host, vamos a crear un nuevo usuario host
Write-Host "2. Creando nuevo usuario host..." -ForegroundColor Yellow
$registerData = @{
    username = "hostuser_$(Get-Random)"
    email = "host_$(Get-Random)@test.com"
    password = "TestPass123"
}

$registerResult = Invoke-ApiRequest -Url "$baseUrl/auth/register" -Method "POST" -Body $registerData
if (-not $registerResult.Success) {
    Write-Host "Error en registro: $($registerResult.Error)" -ForegroundColor Red
    exit 1
}
Write-Host "Registro exitoso" -ForegroundColor Green

# 3. Login
Write-Host "3. Iniciando sesion..." -ForegroundColor Yellow
$loginData = @{
    usernameOrEmail = $registerData.username
    password = $registerData.password
}

$loginResult = Invoke-ApiRequest -Url "$baseUrl/auth/login" -Method "POST" -Body $loginData
if (-not $loginResult.Success) {
    Write-Host "Error en login: $($loginResult.Error)" -ForegroundColor Red
    exit 1
}
Write-Host "Login exitoso" -ForegroundColor Green
$token = $loginResult.Data.token
$newUserId = $loginResult.Data.user.id
$userRole = $loginResult.Data.user.role
Write-Host "User ID: $newUserId"
Write-Host "Role: $userRole"

# 4. Crear una nueva sala con este usuario como host
Write-Host "4. Creando nueva sala..." -ForegroundColor Yellow
$headers = @{ "Authorization" = "Bearer $token" }
$roomData = @{
    name = "Sala de Prueba Host"
    bingoType = "SeventyFive"
    maxPlayers = 10
    isPrivate = $false
}

$roomResult = Invoke-ApiRequest -Url "$baseUrl/room" -Method "POST" -Body $roomData -Headers $headers
if (-not $roomResult.Success) {
    Write-Host "Error creando sala: $($roomResult.Error)" -ForegroundColor Red
    exit 1
}
Write-Host "Sala creada exitosamente" -ForegroundColor Green
$newRoomId = $roomResult.Data.id
Write-Host "Nueva Room ID: $newRoomId"

# 5. Crear una invitación
Write-Host "5. Creando invitación..." -ForegroundColor Yellow
$invitationData = @{
    email = "invitado@test.com"
    roomId = $newRoomId
}

$invitationResult = Invoke-ApiRequest -Url "$baseUrl/invitation" -Method "POST" -Body $invitationData -Headers $headers
if (-not $invitationResult.Success) {
    Write-Host "Error creando invitación: $($invitationResult.Error)" -ForegroundColor Red
    Write-Host "Status Code: $($invitationResult.StatusCode)" -ForegroundColor Red
} else {
    Write-Host "Invitación creada exitosamente" -ForegroundColor Green
    Write-Host "Invitation ID: $($invitationResult.Data.id)"
}

# 6. Obtener invitaciones de la sala
Write-Host "6. Obteniendo invitaciones de la sala..." -ForegroundColor Yellow
$getInvitationsResult = Invoke-ApiRequest -Url "$baseUrl/invitation/room/$newRoomId" -Method "GET" -Headers $headers
if (-not $getInvitationsResult.Success) {
    Write-Host "Error obteniendo invitaciones: $($getInvitationsResult.Error)" -ForegroundColor Red
    Write-Host "Status Code: $($getInvitationsResult.StatusCode)" -ForegroundColor Red
} else {
    Write-Host "Invitaciones obtenidas exitosamente" -ForegroundColor Green
    $invitations = $getInvitationsResult.Data
    Write-Host "Número de invitaciones: $($invitations.Count)" -ForegroundColor Cyan
    
    if ($invitations.Count -gt 0) {
        Write-Host "Detalles de las invitaciones:" -ForegroundColor Cyan
        foreach ($inv in $invitations) {
            Write-Host "  - Email: $($inv.email), Creada: $($inv.createdAt), Usada: $($inv.isUsed)" -ForegroundColor White
        }
    } else {
        Write-Host "No hay invitaciones en la sala" -ForegroundColor Yellow
    }
}

Write-Host "" 
Write-Host "=== FIN DE LA PRUEBA ===" -ForegroundColor Green