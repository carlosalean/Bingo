using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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

        public async Task<bool> SendInvitationEmailAsync(string toEmail, string invitationId, string roomName, string hostName, string invitationLink)
        {
            try
            {
                var subject = $"¡Invitación para jugar Bingo en la sala '{roomName}'!";
                var body = GenerateInvitationEmailBody(invitationId, roomName, hostName, invitationLink);
                
                return await SendEmailAsync(toEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending invitation email to {Email}", toEmail);
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

                using var client = new SmtpClient(_smtpHost, _smtpPort)
                {
                    EnableSsl = _enableSsl,
                    Credentials = new NetworkCredential(_smtpUsername, _smtpPassword)
                };

                using var message = new MailMessage
                {
                    From = new MailAddress(_fromEmail, _fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                    BodyEncoding = Encoding.UTF8,
                    SubjectEncoding = Encoding.UTF8
                };

                message.To.Add(toEmail);

                await client.SendMailAsync(message);
                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
                return true;
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "SMTP error sending email to {Email}: {Message}", toEmail, ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error sending email to {Email}", toEmail);
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