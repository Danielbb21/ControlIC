using System;
using System.Collections.Generic;
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
        public bool Restricao { get; set; }
        public bool Status { get; set; }

        public int ProjetoID { get; set; }
        public virtual Projeto Projeto { get; set; }

    }
}
