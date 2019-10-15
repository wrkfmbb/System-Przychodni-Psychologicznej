using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using SystemPrzychodniPsychologicznej.Models;
using auth.Models;
using System.Configuration;
using System.Net.Mail;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SystemPrzychodniPsychologicznej.Controllers
{
    public class ReservationController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Reservation
        public ActionResult Index()
        {
            var reservations = db.Reservations.Include(r => r.Appointment);
            return View(reservations.ToList());
        }
        [Authorize]
        public ActionResult Reservations()
        {

            var userId = User.Identity.GetUserId();
            var user = db.Users.Where(u => u.Id == userId).SingleOrDefault();
            var reservations = user.Reservations.OrderBy(r => r.Appointment.Date);

            return View(reservations.ToList());

        }

        [Authorize]
        public ActionResult Cancel(int? id)
        {
            var reservation = db.Reservations.Where(r => r.ReservationId == id).SingleOrDefault();

            if (reservation != null)
            {
                reservation.ReservationState = ReservationState.Cancelled;
                reservation.Appointment.IsUnavailable = false;
                reservation.Appointment.IsReservating = false;
            }

            db.SaveChanges();

            return RedirectToAction("Reservations", "Reservation");

        }

        [Authorize(Roles = "Admin")]
        public ActionResult Visits()
        {
            var doctors = db.Doctors;

            return View(doctors.ToList());
        }

        [Authorize(Roles = "Admin")]
        public ActionResult DoctorsVisits(int? id)
        {

            var reservations = db.Reservations
                                 .Where(r => r.Appointment.DoctorId == id)
                                 .OrderBy(r => r.Appointment.Date);

            return View(reservations.ToList());
        }

        private void CreateAndSendEmail(int? id, ReservationState state)
        {
            var reservation = db.Reservations.Find(id);

            Email mail = new Email();
            mail.To = reservation.Email;
            mail.From = ConfigurationManager.AppSettings["ClinicsEmail"];
            mail.Password = ConfigurationManager.AppSettings["EmailPassword"];

            switch (state)
            {
                case ReservationState.Pending:

                    mail.DisplayNameFrom = "Oczekująca na potwierdzenie wizyta w MindClinic";
                    mail.Subject = "Twoja wizyta została zarezerwowana";
                    mail.Body = "Witaj, " + reservation.Name + "!" + Environment.NewLine +
                                " Twoja wizyta z dnia " + reservation.Appointment.Date.ToString("f") + " u " +
                                 reservation.Appointment.Doctor.Name + " " + reservation.Appointment.Doctor.Surname +
                                " została zarezerwowana i oczekuje na potwierdzenie. Oczekuj wkrótce maila z potwierdzeniem! " +
                                "Możesz umówić się na następną wizytę już dziś poprzez naszą stronę! " +
                                 Environment.NewLine + " Pozdrawiamy! " +
                                 Environment.NewLine + "Zespól MindClinic";
                    break;

                case ReservationState.Confirmed:

                    mail.DisplayNameFrom = "Potwierdzona wizyta w MindClinic";
                    mail.Subject = "Twoja wizyta została potwierdzona";
                    mail.Body = "Witaj, " + reservation.Name + "!" + Environment.NewLine +
                                 "Twoja wizyta z dnia " + reservation.Appointment.Date.ToString("f") + " u " +
                                 reservation.Appointment.Doctor.Name + " " + reservation.Appointment.Doctor.Surname +
                                " została potwierdzona. Zapraszamy. " +
                                "Możesz umówić się na następną wizytę już dziś poprzez naszą stronę! " +
                                 Environment.NewLine + " Pozdrawiamy! " +
                                 Environment.NewLine + "Zespól MindClinic";

                    break;

                case ReservationState.Cancelled:

                    mail.DisplayNameFrom = "Odwołana wizyta w MindClinic";
                    mail.Subject = "Twoja wizyta została odwołana";
                    mail.Body = "Witaj, " + reservation.Name + "!" + Environment.NewLine +
                                "Twoja wizyta z dnia " + reservation.Appointment.Date.ToString("f") + " u " +
                                 reservation.Appointment.Doctor.Name + " " + reservation.Appointment.Doctor.Surname +
                                " została odwołana. Przeraszamy za utrudnienia. " +
                                "Możesz umówić się na kolejną już dziś poprzez naszą stronę! " +
                                 Environment.NewLine + " Pozdrawiamy! " +
                                 Environment.NewLine + "Zespól MindClinic";

                    break;
            }

            SendMail(mail);
        }

        private void SendMail(Email email)
        {
            MailMessage mailMessage = new MailMessage();

            mailMessage.To.Add(email.To);
            mailMessage.From = new MailAddress(email.From, email.DisplayNameFrom);
            mailMessage.Subject = email.Subject;
            mailMessage.Body = email.Body;
            mailMessage.IsBodyHtml = false;

            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;
            smtp.EnableSsl = true;

            NetworkCredential nc = new NetworkCredential(email.From, email.Password);
            smtp.UseDefaultCredentials = true;
            smtp.Credentials = nc;
            smtp.Send(mailMessage);

        }


        [Authorize(Roles = "Admin")]
        public ActionResult Confirm(int? id)
        {
            var reservation = db.Reservations.Where(r => r.ReservationId == id).Single();

            if (reservation != null)
            {
                reservation.ReservationState = ReservationState.Confirmed;
            }

            // tu powinno być wysyłanie maila o potwierdzeniu 
            db.SaveChanges();

            CreateAndSendEmail(id, ReservationState.Confirmed);

            var doctorId = reservation.Appointment.DoctorId;

            return RedirectToAction("DoctorsVisits", "Reservation", new { id = doctorId });
        }

        [Authorize(Roles = "Admin")] //id rezerwacji 
        public ActionResult DoctorsCancellation(int? id)
        {
            Reservation reservation = db.Reservations.Where(r => r.ReservationId == id &&
                                                                 r.Appointment.IsUnavailable == true &&
                                                                 r.ReservationState != ReservationState.Cancelled)
                                                     .Single();

            if (reservation != null)
            {
                reservation.ReservationState = ReservationState.Cancelled;
                reservation.Appointment.IsUnavailable = false;
                reservation.Appointment.IsReservating = false;

                db.SaveChanges();
            }

            var reservationId = reservation.ReservationId;
            var doctorId = reservation.Appointment.DoctorId;

            CreateAndSendEmail(reservationId, ReservationState.Cancelled);

            return RedirectToAction("DoctorsVisits", "Reservation", new { id = doctorId });

        }

        [Authorize]
        public ActionResult Assign(int? id)
        {

            var userId = User.Identity.GetUserId();
            var user = db.Users.Where(u => u.Id == userId).Single();

            var isAppointment = db.Appointments.Where(a => a.AppointmentId == id && a.IsReservating == false).Any();
            Appointment appointment = db.Appointments.Find(id);
            int doctorId = appointment.DoctorId;

            if (isAppointment)
            {

                if (appointment != null)
                {
                    var reservation = new Reservation()
                    {
                        ApplicationUser = user,
                        Appointment = appointment,
                        ReservationState = ReservationState.Pending,
                        Email = user.Email,
                        AdditionDate = DateTime.Now.Date,
                        Name = user.Name,
                        Surname = user.Surname
                    };
                    appointment.IsReservating = true;
                    user.Reservations.Add(reservation);
                    reservation.Appointment.IsUnavailable = true;

                    db.Reservations.Add(reservation);
                    db.SaveChanges();

                    var reservationId = reservation.ReservationId;

                    CreateAndSendEmail(reservationId, ReservationState.Pending);

                }

                return RedirectToAction("Reservations", "Reservation");
            }
            ViewBag.Reserved = "Niestety, tę wizyte ktoś właśnie zarezerwował...";

            return RedirectToAction("Index", "Appointment", new { id = doctorId });
        }

        ////GET : Reservation/AssignWithoutLogin/
        public ActionResult AssignWithoutLogin(int? id)
        {
            var isAppointment = db.Appointments.Where(a => a.AppointmentId == id && a.IsReservating == false).Any();
            Appointment appointment = db.Appointments.Find(id);
            int doctorId = appointment.DoctorId;

         
            //jeśli nie ma spotkania to wtedy nie tworzy rezerwacji 
            if (isAppointment)
            {
                Reservation reservation = new Reservation()
                {
                    Appointment = appointment,
                    ReservationState = ReservationState.Pending,
                    AdditionDate = DateTime.Now.Date,
                    Name = "Name",
                    Surname = "Surname",
                    Email = "mindclinicsdoctors@gmail.com",
                };

                if (reservation != null)
                {
                    appointment.IsReservating = true;
                    reservation.Appointment.IsUnavailable = true;
                    db.Reservations.Add(reservation);
                    db.SaveChanges();
                }

                return View(reservation);
            }
            else if(!isAppointment)
            {

                ViewBag.Reserved = "Niestety, tę wizyte ktoś właśnie zarezerwował...";

                return RedirectToAction("Index", "Appointment", new { id = doctorId });
            }
            else
            {
                var reservToDel = db.Reservations.Where(r => r.AppointmentId == appointment.AppointmentId).Single();
                db.Reservations.Remove(reservToDel);
                return RedirectToAction("Index", "Appointment", new { id = doctorId });

            }
        }

        [HttpPost, ActionName("AssignWithoutLogin")]
        [ValidateAntiForgeryToken]
        public ActionResult AssignWithoutLoginPost(int? id)
        {
            //to reCaptcha 
            var response = Request["g-recaptcha-response"];
            string secretKey = "6Lf7uL0UAAAAABmz78SwbUvMWjCzupmC9ar0aQvW";
            var client = new WebClient();
            var result = client.DownloadString(string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", secretKey, response));
            var obj = JObject.Parse(result);
            var status = (bool)obj.SelectToken("success");

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Reservation reservationToUpdate = db.Reservations
                                                   .Where(r => r.AppointmentId == id &&
                                                               r.ReservationState == ReservationState.Pending &&
                                                               r.Appointment.IsUnavailable == true).Single();

            int doctorId = reservationToUpdate.Appointment.DoctorId;
            if (status)
            {
                if (TryUpdateModel(reservationToUpdate, "", new string[] { "Name", "Surname", "Email" }))
                {

                    db.SaveChanges();
                    CreateAndSendEmail(reservationToUpdate.ReservationId, ReservationState.Pending);
                    TempData["reserved"] = "Wizyta została zarezerwowana";
                    
                    if (User.IsInRole("Admin")) return RedirectToAction("Visits", "Reservation");
                                       
                    else return RedirectToAction("Index", "Home"); // tu powinna być strona informująca o pomyślnej rezerwacji
                }
            }
            ViewBag.Captcha = "Nie zaznaczono pola ReCaptchy"; 
            
            return View(reservationToUpdate);



        }
        public ActionResult Quit(int? id)
        {
            Reservation reservationToDel = db.Reservations.Find(id);
            reservationToDel.Appointment.IsReservating = false;
            reservationToDel.Appointment.IsUnavailable = false;
            int doctorId = reservationToDel.Appointment.DoctorId; 

            db.Reservations.Remove(reservationToDel);
            db.SaveChanges(); 
            return RedirectToAction("Index", "Appointment", new { id = doctorId });
        }

        // GET: Reservation/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reservation reservation = db.Reservations.Find(id);
            if (reservation == null)
            {
                return HttpNotFound();
            }
            return View(reservation);
        }

        // GET: Reservation/Create
        public ActionResult Create()
        {
            ViewBag.AppointmentId = new SelectList(db.Appointments, "AppointmentId", "AppointmentId");
            return View();
        }



        //POST: Reservation/Create
        //To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ReservationId,AppointmentId,AdditionDate,Name,Surname,BirthDate,Email,ReservationState")] Reservation reservation)
        {
            if (ModelState.IsValid)
            {
                db.Reservations.Add(reservation);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AppointmentId = new SelectList(db.Appointments, "AppointmentId", "AppointmentId", reservation.AppointmentId);
            return View(reservation);
        }


        // GET: Reservation/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reservation reservation = db.Reservations.Find(id);
            if (reservation == null)
            {
                return HttpNotFound();
            }
            ViewBag.AppointmentId = new SelectList(db.Appointments, "AppointmentId", "AppointmentId", reservation.AppointmentId);
            return View(reservation);
        }

        // POST: Reservation/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ReservationId,AppointmentId,AdditionDate,Name,Surname,BirthDate,Email,ReservationState")] Reservation reservation)
        {
            if (ModelState.IsValid)
            {
                db.Entry(reservation).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AppointmentId = new SelectList(db.Appointments, "AppointmentId", "AppointmentId", reservation.AppointmentId);
            return View(reservation);
        }

        // GET: Reservation/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reservation reservation = db.Reservations.Find(id);
            if (reservation == null)
            {
                return HttpNotFound();
            }
            return View(reservation);
        }

        // POST: Reservation/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Reservation reservation = db.Reservations.Find(id);
            db.Reservations.Remove(reservation);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}