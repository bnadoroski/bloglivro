using BlogLivro.Configuracoes;
using BlogLivro.Models.Usuarios;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Threading.Tasks;

namespace BlogLivro.Models.Emails
{
    public class Email
    {
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string MyProperty { get; set; }
        public Guid? GuidConfirmarEmail { get; set; }
        public Guid? GuidRecuperarSenha { get; set; }
        public List<IFormFile> Attachments { get; set; }
        public AlternateView AlternateView { get; set; }

        public static async Task SendEmailAsync(Email mailRequest, ConfiguracaoEmail configuracaoEmail)
        {
                MailMessage message = new MailMessage();
                SmtpClient smtp = new SmtpClient();

                message.From = new MailAddress(configuracaoEmail.Mail, configuracaoEmail.DisplayName);
                message.To.Add(new MailAddress(mailRequest.ToEmail));
                message.Subject = mailRequest.Subject;
                message.IsBodyHtml = true;

                if (mailRequest.Attachments != null)
                {
                    foreach (var file in mailRequest.Attachments)
                    {
                        if (file.Length > 0)
                        {
                            using (var ms = new MemoryStream())
                            {
                                file.CopyTo(ms);
                                var fileBytes = ms.ToArray();
                                Attachment att = new Attachment(new MemoryStream(fileBytes), file.FileName);
                                message.Attachments.Add(att);
                            }
                        }
                    }
                }

                message.Body = mailRequest.Body;
                smtp.Port = configuracaoEmail.Port;
                smtp.Host = configuracaoEmail.Host;
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(configuracaoEmail.Mail, configuracaoEmail.Password);
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                message.AlternateViews.Add(mailRequest.AlternateView);

                await smtp.SendMailAsync(message);
        }

        public static Email ConfirmarEmail(Usuario usuario)
        {
            string confirmarEmail = File.ReadAllText(Path.Combine(Links.CAMINHO, Links.TEMPLATECONFIRMAREMAIL));
            Email email = new Email { ToEmail = usuario.Email, GuidConfirmarEmail = usuario.GuidConfirmarEmail };
            email.Subject = "Confirmar Email | Readers Club - Clube para Leitores";
            email.Body = confirmarEmail.Replace("{{linkIndex}}", Links.LINKSITE)
                    .Replace("{{linkAtivar}}", string.Concat(Links.LINKSITE, "confirmarEmail.html?guid=", usuario.GuidConfirmarEmail))
                    .Replace("{{imagem}}", "photo");
            AlternateView htmlview = default(AlternateView);
            htmlview = AlternateView.CreateAlternateViewFromString(email.Body, null, "text/html");
            LinkedResource imageResourceEs = new LinkedResource(Path.Combine(Links.CAMINHO, Links.IMAGEMLOGO));
            imageResourceEs.ContentId = "photo";
            imageResourceEs.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlview.LinkedResources.Add(imageResourceEs);
            email.AlternateView = htmlview;

            return email;
        }

        public static Email RecuperarSenha(Usuario usuario)
        {
            string confirmarEmail = File.ReadAllText(Path.Combine(Links.CAMINHO, Links.TEMPLATERECUPERARSENHA));
            Email email = new Email { ToEmail = usuario.Email, GuidRecuperarSenha = usuario.GuidRecuperarSenha };
            email.Subject = "Redefinir Senha | Readers Club - Clube para Leitores";
            email.Body = confirmarEmail.Replace("{{linkIndex}}", Links.LINKSITE)
                    .Replace("{{linkAtivar}}", string.Concat(Links.LINKSITE, "redefinirSenha.html?guid=", usuario.GuidRecuperarSenha))
                    .Replace("{{imagem}}", "photo");
            AlternateView htmlview = default(AlternateView);
            htmlview = AlternateView.CreateAlternateViewFromString(email.Body, null, "text/html");
            LinkedResource imageResourceEs = new LinkedResource(Path.Combine(Links.CAMINHO, Links.IMAGEMLOGO));
            imageResourceEs.ContentId = "photo";
            imageResourceEs.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            htmlview.LinkedResources.Add(imageResourceEs);
            email.AlternateView = htmlview;

            return email;
        }
    }
}
