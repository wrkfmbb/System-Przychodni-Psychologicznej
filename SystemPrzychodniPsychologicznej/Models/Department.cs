using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace auth.Models
{
    public class Department
    {
        public int DepartmentId { get; set; }
        [Required(ErrorMessage = "Wprowadz nazwę wydziału do 150 znakow!")]
        [StringLength(150)]
        public string Name { get; set; }
        [Required(ErrorMessage = "Wprowadz opis do 500 znaków!")]
        [StringLength(500)]
        public string Description { get; set; }
        
        public virtual ICollection<Doctor> Doctors { get; set; }
    }
}