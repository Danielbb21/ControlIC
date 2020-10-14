using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ControlIC.Models
{
    public class ProjetoCoorientador
    {
        public int ID { get; set; }
        public bool Aprovado { get; set; }

        public int ProjetoID { get; set; }
        public virtual Projeto Projeto { get; set; }

        public int UsuarioID { get; set; }
        public virtual Usuario Usuario { get; set; }
    }
}
