using auth.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using SystemPrzychodniPsychologicznej.Infrastructure;
using SystemPrzychodniPsychologicznej.Models;

namespace SystemPrzychodniPsychologicznej.Controllers

{
    public class EmailController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> SendEmail()
        {
            var tomorrow = DateTime.Now;
            var tomorrowtomorrow = DateTime.Now;

            tomorrow = tomorrow.AddDays(1);
            tomorrowtomorrow = tomorrowtomorrow.AddDays(2);

            List<Reservation> reservations = db.Reservations.Where(r => r.ReservationState == ReservationState.Confirmed &&
            r.Appointment.Date > tomorrow && r.Appointment.Date < tomorrowtomorrow).ToList();

            if (reservations != null)
            {
                foreach (Reservation reservation in reservations)
                {
                    var mail = new Email();

                    mail.To = reservation.Email;
                    mail.From = ConfigurationManager.AppSettings["ClinicsEmail"];
                    mail.Password = ConfigurationManager.AppSettings["EmailPassword"];
                    mail.DisplayNameFrom = "Przypomnienie o wizycie w MindClinic";
                    mail.Subject = "Przypomnienie o nadchodzącej wizycie";
                    mail.Body = "Witaj, " + reservation.Name + "!\n\n" +
                        "Przypominamy, że Twoja wizyta jest jutro!\n" +
                        "Jeśli nie możesz się pojawić," +
                        " możesz w szybki sposób ją odwołać poprzez wejście w zakładkę na naszej stronie" +
                        " i kliknieciu przycisku 'Odwołaj'.\n\n" +
                        "Szczegóły twojej wizyty: \n" +
                        "Termin: " + reservation.Appointment.Date.ToString("f") + Environment.NewLine +
                        "Osoba prowadząca: " + reservation.Appointment.Doctor.Name + " " +
                        reservation.Appointment.Doctor.Surname + Environment.NewLine +
                        "Specjalność: " + reservation.Appointment.Doctor.Specialisation + Environment.NewLine +
                        "Email: " + reservation.Appointment.Doctor.Email + Environment.NewLine +
                        "Dział: " + reservation.Appointment.Doctor.Department.Name + Environment.NewLine + Environment.NewLine +
                        "Serdecznie zapraszamy!" + Environment.NewLine +
                        "Zespół MindClinic" + Environment.NewLine;

                    await SendEmailAsync(mail);

                }

            }
            return Json(0, JsonRequestBehavior.AllowGet);
        }

        public async static Task SendEmailAsync(Email email)
        {
            try
            {
                MailMessage mm = new MailMessage();
                mm.To.Add(email.To);
                mm.From = new MailAddress(email.From, email.DisplayNameFrom);
                mm.Subject = email.Subject;
                mm.Body = email.Body;
                mm.IsBodyHtml = false;

                using (SmtpClient smtp = new SmtpClient())
                {
                    smtp.EnableSsl = true;
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(email.From, email.Password);
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.SendCompleted += (s, e) => { smtp.Dispose(); };
                    await smtp.SendMailAsync(mm);

                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}