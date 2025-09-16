# Configuración del Servicio de Email

Este documento explica cómo configurar el servicio de email para el envío automático de invitaciones.

## Configuración Requerida

Para que el sistema pueda enviar emails automáticamente, necesitas configurar las credenciales SMTP en el archivo `appsettings.json` o `appsettings.Development.json`.

### 1. Configuración para Gmail

Si usas Gmail, necesitas:

1. **Habilitar la verificación en 2 pasos** en tu cuenta de Google
2. **Generar una contraseña de aplicación**:
   - Ve a [Configuración de Google](https://myaccount.google.com/security)
   - Busca "Contraseñas de aplicaciones"
   - Genera una nueva contraseña para "Correo"

3. **Actualizar appsettings.json**:
```json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "SmtpUsername": "tu-email@gmail.com",
    "SmtpPassword": "tu-contraseña-de-aplicacion",
    "FromEmail": "tu-email@gmail.com",
    "FromName": "Bingo Game",
    "EnableSsl": "true"
  }
}
```

### 2. Configuración para Outlook/Hotmail

```json
{
  "Email": {
    "SmtpHost": "smtp-mail.outlook.com",
    "SmtpPort": "587",
    "SmtpUsername": "tu-email@outlook.com",
    "SmtpPassword": "tu-contraseña",
    "FromEmail": "tu-email@outlook.com",
    "FromName": "Bingo Game",
    "EnableSsl": "true"
  }
}
```

### 3. Otros Proveedores SMTP

Puedes usar cualquier proveedor SMTP configurando:
- `SmtpHost`: Servidor SMTP del proveedor
- `SmtpPort`: Puerto (generalmente 587 para TLS o 465 para SSL)
- `SmtpUsername`: Tu usuario/email
- `SmtpPassword`: Tu contraseña
- `EnableSsl`: `true` para conexiones seguras

## Funcionamiento

Cuando el servicio está configurado correctamente:

1. **Creación de Invitación**: Al crear una invitación desde el frontend, el sistema automáticamente:
   - Guarda la invitación en la base de datos
   - Genera un enlace único de invitación
   - Envía un email HTML con el enlace al destinatario

2. **Contenido del Email**: El email incluye:
   - Información de la sala y el anfitrión
   - Enlace directo para unirse al juego
   - Diseño responsive y atractivo
   - Instrucciones claras para el invitado

3. **Manejo de Errores**: Si el email no se puede enviar:
   - La invitación se crea exitosamente de todos modos
   - Se registra el error en los logs
   - El usuario puede copiar el enlace manualmente

## Verificación

Para verificar que el servicio funciona:

1. Configura las credenciales SMTP
2. Reinicia la aplicación
3. Crea una nueva invitación desde el frontend
4. Verifica que el email llegue al destinatario
5. Revisa los logs del servidor para cualquier error

## Seguridad

⚠️ **Importante**: 
- Nunca commits las credenciales reales al repositorio
- Usa variables de entorno en producción
- Para Gmail, usa contraseñas de aplicación, no tu contraseña principal
- Considera usar servicios como SendGrid o AWS SES para producción

## Troubleshooting

### Error: "Authentication failed"
- Verifica que las credenciales sean correctas
- Para Gmail, asegúrate de usar una contraseña de aplicación
- Verifica que la verificación en 2 pasos esté habilitada

### Error: "Connection timeout"
- Verifica el host y puerto SMTP
- Asegúrate de que el firewall no bloquee la conexión
- Verifica la configuración SSL/TLS

### Los emails no llegan
- Revisa la carpeta de spam del destinatario
- Verifica que el email "From" sea válido
- Considera usar un dominio verificado