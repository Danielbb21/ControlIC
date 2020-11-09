using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ControlIC.Models
{
    public class Recrutamento
    {
        public int ID { get; set; }
        public string Descricao { get; set; }
        [RegularExpression(@"(https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|www\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9]+\.[^\s]{2,}|www\.[a-zA-Z0-9]+\.[^\s]{2,})", ErrorMessage = "Link inválido")]
        public string LinkExterno { get; set; }
        [Column(TypeName = "BLOB")]
        public byte[] Arquivo { get; set; }
        public DateTime DataPostagem { get; set; }
        public bool status { get; set; }
        public int ProjetoID { get; set; }
        public virtual Projeto Projeto { get; set; }

        [NotMapped]
        public IFormFile ArquivoFormato { get; set; }
    }
}
