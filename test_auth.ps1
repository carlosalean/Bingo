# Script para probar autenticación y creación de sala
$baseUrl = "http://localhost:5086/api"

# Usar credenciales diferentes para evitar conflictos
$timestamp = Get-Date -Format "yyyyMMddHHmmss"
$username = "testuser$timestamp"
$email = "test$timestamp@example.com"
$password = "password123"

# Datos de registro
$registerData = @{
    username = $username
    email = $email
    password = $password
} | ConvertTo-Json

# Datos de login
$loginData = @{
    usernameOrEmail = $username
    password = $password
} | ConvertTo-Json

# Datos para crear sala
$roomData = @{
    name = "Test Room $timestamp"
    bingoType = "SeventyFive"
    maxPlayers = 10
    isPrivate = $false
} | ConvertTo-Json

Write-Host "=== Probando registro con usuario: $username ==="
try {
    $registerResponse = Invoke-WebRequest -Uri "$baseUrl/auth/register" -Method POST -Body $registerData -ContentType "application/json" -UseBasicParsing
    Write-Host "Registro exitoso: $($registerResponse.StatusCode)"
    $registerResult = $registerResponse.Content | ConvertFrom-Json
    Write-Host "Usuario creado: $($registerResult.username) (ID: $($registerResult.id))"
} catch {
    Write-Host "Error en registro: $($_.Exception.Message)"
    if ($_.Exception.Response) {
        $errorStream = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($errorStream)
        $errorContent = $reader.ReadToEnd()
        Write-Host "Error content: $errorContent"
    }
    exit 1
}

Write-Host "\n=== Probando login ==="
try {
    $loginResponse = Invoke-WebRequest -Uri "$baseUrl/auth/login" -Method POST -Body $loginData -ContentType "application/json" -UseBasicParsing
    $loginResult = $loginResponse.Content | ConvertFrom-Json
    $token = $loginResult.token
    Write-Host "Login exitoso. Token: $($token.Substring(0, 20))..."
    Write-Host "Usuario logueado: $($loginResult.user.username) (ID: $($loginResult.user.id))"
} catch {
    Write-Host "Error en login: $($_.Exception.Message)"
    if ($_.Exception.Response) {
        $errorStream = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($errorStream)
        $errorContent = $reader.ReadToEnd()
        Write-Host "Error content: $errorContent"
    }
    exit 1
}

Write-Host "\n=== Creando nueva sala ==="
try {
    $headers = @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/json"
    }
    
    $roomResponse = Invoke-WebRequest -Uri "$baseUrl/room" -Method POST -Body $roomData -Headers $headers -UseBasicParsing
    $roomResult = $roomResponse.Content | ConvertFrom-Json
    $roomId = $roomResult.id
    Write-Host "Sala creada exitosamente. ID: $roomId"
    Write-Host "Host ID: $($roomResult.hostId)"
    Write-Host "Nombre: $($roomResult.name)"
} catch {
    Write-Host "Error creando sala: $($_.Exception.Message)"
    if ($_.Exception.Response) {
        $errorStream = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($errorStream)
        $errorContent = $reader.ReadToEnd()
        Write-Host "Error content: $errorContent"
    }
    exit 1
}

Write-Host "\n=== Probando inicio de juego ==="
try {
    $startGameResponse = Invoke-WebRequest -Uri "$baseUrl/game/start/$roomId" -Method POST -Headers $headers -UseBasicParsing
    Write-Host "¡Juego iniciado exitosamente! Status: $($startGameResponse.StatusCode)"
    Write-Host "Response: $($startGameResponse.Content)"
} catch {
    Write-Host "Error iniciando juego: $($_.Exception.Message)"
    if ($_.Exception.Response) {
        $errorStream = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($errorStream)
        $errorContent = $reader.ReadToEnd()
        Write-Host "Error content: $errorContent"
    }
}

Write-Host "\n=== Prueba completada ==="
Write-Host "Sala ID para pruebas futuras: $roomId"