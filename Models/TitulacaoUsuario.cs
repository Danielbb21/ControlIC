using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ControlIC.Models
{
    public class TitulacaoUsuario
    {
        public int ID { get; set; }
        [Required]
        public string NomeInstituicao { get; set; }
        public DateTime DataTitulacao { get; set; }

        public int TitulacaoID { get; set; }
        public virtual Titulacao Titulacao { get; set; }

        public int UsuarioID { get; set; }
        public virtual Usuario Usuario { get; set; }
    }
}
