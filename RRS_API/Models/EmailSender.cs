using System;
using System.Net.Mail;

namespace RRS_API.Models
{
    public class EmailSender
    {
        public void sendEmail(int numOfReceiptsUploaded, string familyID)
        {
            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress("maimonmaor@gmail.com");
                mail.To.Add("maimonmaor@gmail.com");
                mail.Subject = numOfReceiptsUploaded + " Receipts uploaded from " + familyID;
                using (SmtpClient smtp = new SmtpClient())
                {
                    smtp.Send(mail);
                }
            }
        }
    }
}