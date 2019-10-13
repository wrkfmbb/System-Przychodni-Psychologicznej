using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace auth.Models
{
    public class Appointment
    {
        public int AppointmentId { get; set; }
        public int DoctorId { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
        public bool IsUnavailable { get; set; }
        public bool IsReservating { get; set; }

        public virtual Doctor Doctor { get; set;  }
    }
}