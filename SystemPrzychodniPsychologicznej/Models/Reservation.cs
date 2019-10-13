using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using SystemPrzychodniPsychologicznej.Models;

namespace auth.Models
{
    public class Reservation
    {
        public int ReservationId { get; set; }

        public int AppointmentId { get; set; }

        [DataType(DataType.Date)]
        public DateTime AdditionDate { get; set; } //data dodania

        [Required(ErrorMessage = "Wprowadz Imię!")]
        [StringLength(50)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Wprowadz Nazwisko!")]
        [StringLength(50)]
        public string Surname { get; set; }

        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        [Required(ErrorMessage = "Wprowadz Email!")]
        public string Email { get; set; }

        public ReservationState ReservationState { get; set; }

        public virtual ApplicationUser ApplicationUser { get; set; }
        public virtual Appointment Appointment { get; set; }
    }

    public enum ReservationState
    {
        Pending,     //oczekujace na potwierdzenie
        Confirmed,   //potwierdzone
        Cancelled    //odwolane
    }
}