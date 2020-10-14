using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ControlIC.Models
{
    public class Usuario
    {
        public int ID { get; set; }
        [Required]
        public string Nome { get; set; }
        public char Sexo { get; set; }
        public DateTime DataNascimento { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Senha { get; set; }
        public string Linkedin { get; set; }
        public int TipoUsuario { get; set; }
        public DateTime AnoIngresso { get; set; }
        [Column(TypeName = "BLOB")]
        public byte[] ImgUsuario { get; set; }

        public virtual List<TitulacaoUsuario> TitulacaoUsuarios { get; set; }
        public virtual List<ProjetoCoorientador> projetoCoorientadores { get; set; }
        public virtual List<ProjetoEstudante> ProjetoEstudantes { get; set; }

        public int CursoID { get; set; }
        public virtual Curso Curso {get; set;}
    }
}
