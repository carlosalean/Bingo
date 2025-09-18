using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace BingoGameApi.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly bool _enableSsl;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            
            // Cargar configuración SMTP
            _smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            _smtpUsername = _configuration["Email:SmtpUsername"] ?? "";
            _smtpPassword = _configuration["Email:SmtpPassword"] ?? "";
            _fromEmail = _configuration["Email:FromEmail"] ?? _smtpUsername;
            _fromName = _configuration["Email:FromName"] ?? "Bingo Game";
            _enableSsl = bool.Parse(_configuration["Email:EnableSsl"] ?? "true");
        }

        public async Task<bool> SendInvitationEmailAsync(string toEmail, string inviteCode, string roomName, string hostName, string invitationLink)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Bingo Game", _smtpUsername));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = $"Invitación a la sala de Bingo: {roomName}";

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = $@"
                    <h2>¡Has sido invitado a jugar Bingo!</h2>
                    <p><strong>Sala:</strong> {roomName}</p>
                    <p><strong>Anfitrión:</strong> {hostName}</p>
                    <p><strong>Código de invitación:</strong> {inviteCode}</p>
                    <p>Haz clic en el siguiente enlace para unirte:</p>
                    <a href='{invitationLink}' style='background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Unirse a la sala</a>
                    <p>O copia y pega este enlace en tu navegador: {invitationLink}</p>
                ";

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new MailKit.Net.Smtp.SmtpClient();
                
                // Configurar SSL según el puerto
                if (_smtpPort == 465)
                {
                    // SSL implícito para puerto 465
                    await client.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.SslOnConnect);
                }
                else
                {
                    // STARTTLS para puerto 587
                    await client.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
                }
                
                await client.AuthenticateAsync(_smtpUsername, _smtpPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email enviado exitosamente a {Email} con código de invitación: {InviteCode}", toEmail, inviteCode);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando email: {Message}", ex.Message);
                return false;
            }
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                // Validar configuración
                if (string.IsNullOrEmpty(_smtpUsername) || string.IsNullOrEmpty(_smtpPassword))
                {
                    _logger.LogWarning("SMTP credentials not configured. Email not sent to {Email}", toEmail);
                    return false;
                }

                // Crear mensaje usando MimeKit
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_fromName, _fromEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = body
                };
                message.Body = bodyBuilder.ToMessageBody();

                // Usar MailKit SmtpClient que maneja SSL implícito correctamente
                using var client = new MailKit.Net.Smtp.SmtpClient();
                
                // Configurar opciones de seguridad según el puerto
                SecureSocketOptions secureSocketOptions;
                if (_smtpPort == 465)
                {
                    // Puerto 465 usa SSL implícito
                    secureSocketOptions = SecureSocketOptions.SslOnConnect;
                    _logger.LogInformation("Usando SSL implícito para puerto 465");
                }
                else if (_smtpPort == 587)
                {
                    // Puerto 587 usa STARTTLS
                    secureSocketOptions = SecureSocketOptions.StartTls;
                }
                else
                {
                    // Para otros puertos, usar auto-detección
                    secureSocketOptions = SecureSocketOptions.Auto;
                }

                // Conectar al servidor SMTP
                await client.ConnectAsync(_smtpHost, _smtpPort, secureSocketOptions);
                
                // Autenticar
                await client.AuthenticateAsync(_smtpUsername, _smtpPassword);
                
                // Enviar mensaje
                await client.SendAsync(message);
                
                // Desconectar
                await client.DisconnectAsync(true);
                
                _logger.LogInformation("Email sent successfully to {Email} using MailKit", toEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Email}: {Message}", toEmail, ex.Message);
                return false;
            }
        }

        private string GenerateInvitationEmailBody(string invitationId, string roomName, string hostName, string invitationLink)
        {
            return $@"
<!DOCTYPE html>
<html lang=""es"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Invitación a Bingo</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f4f4f4;
        }}
        .container {{
            background: white;
            padding: 30px;
            border-radius: 10px;
            box-shadow: 0 0 20px rgba(0,0,0,0.1);
        }}
        .header {{
            text-align: center;
            margin-bottom: 30px;
        }}
        .logo {{
            font-size: 2.5em;
            color: #4CAF50;
            margin-bottom: 10px;
        }}
        .title {{
            color: #2c3e50;
            margin-bottom: 20px;
        }}
        .invitation-details {{
            background: #f8f9fa;
            padding: 20px;
            border-radius: 8px;
            margin: 20px 0;
            border-left: 4px solid #4CAF50;
        }}
        .join-button {{
            display: inline-block;
            background: #4CAF50;
            color: white;
            padding: 15px 30px;
            text-decoration: none;
            border-radius: 5px;
            font-weight: bold;
            margin: 20px 0;
            text-align: center;
        }}
        .join-button:hover {{
            background: #45a049;
        }}
        .footer {{
            margin-top: 30px;
            padding-top: 20px;
            border-top: 1px solid #eee;
            text-align: center;
            color: #666;
            font-size: 0.9em;
        }}
        .warning {{
            background: #fff3cd;
            border: 1px solid #ffeaa7;
            color: #856404;
            padding: 10px;
            border-radius: 5px;
            margin: 15px 0;
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <div class=""logo"">🎯</div>
            <h1 class=""title"">¡Estás invitado a jugar Bingo!</h1>
        </div>
        
        <p>¡Hola!</p>
        
        <p><strong>{hostName}</strong> te ha invitado a unirte a una emocionante partida de Bingo en línea.</p>
        
        <div class=""invitation-details"">
            <h3>📋 Detalles de la invitación:</h3>
            <p><strong>🏠 Sala:</strong> {roomName}</p>
            <p><strong>👤 Anfitrión:</strong> {hostName}</p>
            <p><strong>🎫 ID de invitación:</strong> {invitationId}</p>
        </div>
        
        <div style=""text-align: center;"">
            <a href=""{invitationLink}"" class=""join-button"">🎮 ¡Unirse al juego!</a>
        </div>
        
        <div class=""warning"">
            <strong>⚠️ Importante:</strong> Esta invitación es válida por tiempo limitado. ¡No esperes demasiado para unirte!
        </div>
        
        <p>Si no puedes hacer clic en el botón, copia y pega este enlace en tu navegador:</p>
        <p style=""word-break: break-all; background: #f8f9fa; padding: 10px; border-radius: 5px;"">
            <a href=""{invitationLink}"">{invitationLink}</a>
        </p>
        
        <div class=""footer"">
            <p>Este email fue enviado automáticamente por el sistema de Bingo Game.</p>
            <p>Si no esperabas esta invitación, puedes ignorar este mensaje.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}