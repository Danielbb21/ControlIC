using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControlIC.Models
{
    public class Projeto
    {
        public int ID { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public string OutrasInformacoes { get; set; }
        public bool Status { get; set; }
        public bool Aprovado { get; set; }
        [Column(TypeName = "BLOB")]
        public byte[] ImgProjeto { get; set; }
        [NotMapped]
        public IFormFile ImgProjetoFormato { get; set; }
        public int CampoPesquisaID { get; set; }
        public int UsuarioID { get; set; }
      
        public virtual Usuario Usuario { get; set; }
        public virtual CampoPesquisa CampoPesquisa { get; set; }
        public virtual List<Recrutamento> Recrutamentos { get; set; }
        public virtual List<Atividade> Atividades { get; set; }
        public virtual List<ProjetoCoorientador> projetoCoorientadores { get; set; }
        public virtual List<ProjetoEstudante> ProjetoEstudantes { get; set; }
        public virtual List<Postagem> Postagens { get; set; }


        [NotMapped]
        public string EmailConvite { get; set; }

    }
}