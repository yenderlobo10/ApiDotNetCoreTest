using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ApiAngularTest.Models
{
    public class ToDo
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo [{0}] es requerido.")]
        public string Title { get; set; }

        [StringLength(140, ErrorMessage = "El campo [{0}] acepta máximo {1} caracteres.")]
        public string Description { get; set; }

        public bool Done { get; set; }

    }
}
