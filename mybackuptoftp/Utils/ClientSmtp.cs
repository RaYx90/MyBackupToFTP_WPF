using AegisImplicitMail;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace mybackuptoftp.Utils
{
    public class ClientSmtp
    {       
        /// <summary>
        /// Funcion para enviar un email al cliente
        /// IMPORTANTE: TENDREMOS QUE CONFIGURAR EL SERVIDOR FTP, CUENTA y PASSWORD ORIGEN DE MAIL 
        /// </summary>
        /// <param name="body"></param>
        /// <param name="mailClient"></param>
        /// <param name="nameClient"></param>
        public static void SendEmail(string body, string mailClient, string nameClient)
        {
            try
            {
                DateTime date = DateTime.Now;
                var mailMessage = new MimeMailMessage();
                mailMessage.Subject = "MyBackupToFTP - Copia de seguridad (" + date.ToShortDateString() + ") - " + nameClient;
                mailMessage.Body = body;
                mailMessage.From = new MimeMailAddress("noreply@DOMINIOAINTRODUCIR");
                mailMessage.To.Add(new MimeMailAddress(mailClient));
                var emailer = new MimeMailer("MAILSERVERAINTRODUCIR", 465);
                emailer.User = "noreply@DOMINIOAINTRODUCIR";
                emailer.Password = "PasswordAIntroducir";
                emailer.SslType = SslMode.Ssl;
                // The authentication types depends on your server, it can be plain, base 64 or none. 
                //if you do not need user name and password means you are using default credentials 
                // In this case, your authentication type is none 
                emailer.AuthenticationMode = AuthenticationType.Base64;
                emailer.SendMailAsync(mailMessage); 
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
