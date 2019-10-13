using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace auth.Models
{
    public class Doctor
    {
        public int DoctorId { get; set; }
        public int DepartmentId { get; set; }
        [Required(ErrorMessage = "Wprowadz Imię!")]
        [StringLength(50)]
        public string Name { get; set; }
        [Required(ErrorMessage = "Wprowadz Nazwisko!")]
        [StringLength(50)]
        public string Surname { get; set; }
        [Required(ErrorMessage = "Wprowadz swoj Email!")]
        [StringLength(100)]
        public string Email { get; set; }
        [StringLength(150)]
        public string Specialisation { get; set; }
        public string FullName
        {
            get
            {
                return Name + " " + Surname;
            }
        }

        public virtual Department Department { get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; }

    }
}