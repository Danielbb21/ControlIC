using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ControlIC.Models
{
    public class Curso
    {
        public int ID { get; set; }
        public string Nome { get; set; }

        public virtual List<Usuario> Usuarios { get; set; }
    }
}
