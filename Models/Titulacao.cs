using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ControlIC.Models
{
    public class Titulacao
    {
        public int ID { get; set; }
        public string NomeTitulacao { get; set; }
        public virtual List<TitulacaoUsuario> TitulacaoUsuarios { get; set; }

    }
}
