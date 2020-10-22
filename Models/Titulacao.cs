using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ControlIC.Models
{
    public class Titulacao
    {
        public int ID { get; set; }
        [Required]
        public string NomeTitulacao { get; set; }
        [Required(ErrorMessage ="Campo precisa ser preenchido")]
        public string NomeInstituicao { get; set; }
        public DateTime DataTitulacao { get; set; }

    }
}
