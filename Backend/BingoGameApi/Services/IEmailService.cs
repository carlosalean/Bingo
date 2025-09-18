using System.Threading.Tasks;

namespace BingoGameApi.Services
{
    public interface IEmailService
    {
        /// <summary>
        /// Envía un email de invitación a un usuario
        /// </summary>
        /// <param name="toEmail">Email del destinatario</param>
        /// <param name="inviteCode">Código de invitación de la sala</param>
        /// <param name="roomName">Nombre de la sala</param>
        /// <param name="hostName">Nombre del host</param>
        /// <param name="invitationLink">Enlace de la invitación</param>
        /// <returns>True si el email se envió correctamente</returns>
        Task<bool> SendInvitationEmailAsync(string toEmail, string inviteCode, string roomName, string hostName, string invitationLink);
        
        /// <summary>
        /// Envía un email genérico
        /// </summary>
        /// <param name="toEmail">Email del destinatario</param>
        /// <param name="subject">Asunto del email</param>
        /// <param name="body">Cuerpo del email (HTML)</param>
        /// <returns>True si el email se envió correctamente</returns>
        Task<bool> SendEmailAsync(string toEmail, string subject, string body);
    }
}