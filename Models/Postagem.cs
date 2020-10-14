using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ControlIC.Models
{
    public class Postagem
    {
        public int ID { get; set; }
        public string Texto { get; set; }
        public DateTime DataPostagem { get; set; }

        public int UsuarioID { get; set; }
        public virtual Usuario Usuario{ get; set; }

        public int ProjetoID { get; set; }
        public virtual Projeto Projeto { get; set; }
    }
}
