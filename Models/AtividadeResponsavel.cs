using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ControlIC.Models
{
    public class AtividadeResponsavel
    {
        public int ID { get; set; }
        public DateTime? DataEntrega { get; set; }
        [Column(TypeName = "BLOB")]
        public byte[] Arquivo { get; set; }

        [NotMapped]
        public IFormFile ArquivoFormato { get; set; }
        public bool Entregue { get; set; }
        public string TipoArquivo { get; set; }
        public string NomeArquivo { get; set; }
        public int UsuarioID { get; set; }
        public virtual Usuario Usuario { get; set; }
        public int AtividadeID { get; set; }
        public virtual Atividade Atividade { get; set; }
    }
}
