using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
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

        [Required(ErrorMessage = "O E-mail deve ser inserido.")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "E-mail inválido.")]
        public string Email { get; set; }
        [Required(ErrorMessage = "A senha deve ser inserida.")]
        public string Senha { get; set; }
        
        [Required(ErrorMessage = "O nome deve ser inserido.")]
        public string Nome { get; set; }

        public char Sexo { get; set; }
        public string LinkedIn { get; set; }

        [DataType(DataType.Date)]
        public DateTime DataNascimento { get; set; }
        
        [Column(TypeName = "BLOB")]
        public byte[] ImgUsuario { get; set; }
        public int TipoUsuario { get; set; }

        [DataType(DataType.Date)]
        public DateTime? AnoIngresso { get; set; }
        public int? CursoID { get; set; }
        public virtual Curso Curso {get; set;}

        public int? TitulacaoID { get; set; }
        public virtual Titulacao Titulacao { get; set; }

        public virtual List<ProjetoCoorientador> projetoCoorientadores { get; set; }
        public virtual List<ProjetoEstudante> ProjetoEstudantes { get; set; }

        [NotMapped]
        public string ConfirmarSenha { get; set; }
        [NotMapped]
        public IList<IFormFile> Perfil { get; set; }

    }
}
