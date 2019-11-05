using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SystemPrzychodniPsychologicznej.Models;
using auth.Models;
using SystemPrzychodniPsychologicznej.Infrastructure;

namespace SystemPrzychodniPsychologicznej.Controllers
{
    
    public class AppointmentController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Appointment
        public ActionResult Index(int? id)
        {
            var appointments = db.Appointments.Include(a => a.Doctor).OrderBy(a => a.Date);

            if (id != null)
            {
                appointments = db.Appointments
                                 .Where(a => a.DoctorId == id && a.IsUnavailable == false && a.Date > DateTime.Now)
                                 .OrderBy(a => a.Date);
            }

            return View(appointments.ToList());
        }

        // GET: Appointment/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Appointment appointment = db.Appointments.Find(id);
            if (appointment == null)
            {
                return HttpNotFound();
            }
            return View(appointment);
        }


        // GET: Appointment/Create
        public ActionResult Create()
        {
            ViewBag.DoctorId = new SelectList(db.Doctors, "DoctorId", "FullName");
            return View();
        }

        // POST: Appointment/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "AppointmentId,DoctorId,Date,Price,IsUnavailable")] Appointment appointment)
        {
            ViewBag.Error = string.Empty;
            var existingAppointment = db.Appointments.Where(a => a.Date == appointment.Date && a.DoctorId == appointment.DoctorId).Any();


            if (ModelState.IsValid && existingAppointment == false && appointment.Date > DateTime.Now)
            {
                db.Appointments.Add(appointment);
                db.SaveChanges();
                ViewBag.Error = string.Empty;
                TempData["AdditionAppointment"] = "Wizyta została dodana";
                return RedirectToAction("Visits", "Reservation");
            }

            ViewBag.DoctorId = new SelectList(db.Doctors, "DoctorId", "FullName", appointment.DoctorId);
            ViewBag.Error = "Taki termin już istnieje!";
            return View(appointment);
        }

        // GET: Appointment/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Appointment appointment = db.Appointments.Find(id);
            if (appointment == null)
            {
                return HttpNotFound();
            }
            ViewBag.DoctorId = new SelectList(db.Doctors, "DoctorId", "Name", appointment.DoctorId);
            return View(appointment);
        }

        // POST: Appointment/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "AppointmentId,DoctorId,Date,Price,IsUnavailable")] Appointment appointment)
        {
            if (ModelState.IsValid)
            {
                db.Entry(appointment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.DoctorId = new SelectList(db.Doctors, "DoctorId", "Name", appointment.DoctorId);
            return View(appointment);
        }

        // GET: Appointment/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Appointment appointment = db.Appointments.Find(id);
            if (appointment == null)
            {
                return HttpNotFound();
            }
            return View(appointment);
        }

        // POST: Appointment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Appointment appointment = db.Appointments.Find(id);
            db.Appointments.Remove(appointment);
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

