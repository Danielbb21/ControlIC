using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ControlIC.Models
{
    public class Atividade
    {
        public int ID { get; set; }
        public string Titulo { get; set; }
        public string Texto { get; set; }
        public DateTime DataPrevista { get; set; }
        //Se Documentos enviados serão publicos ou privados
        public bool Restricao { get; set; }

        [Column(TypeName = "varchar(13)")]
        public string Cor { get; set; }
        public int ProjetoID { get; set; }
        public virtual Projeto Projeto { get; set; }
        public virtual List<AtividadeResponsavel> Participantes { get; set; }
    }
}
