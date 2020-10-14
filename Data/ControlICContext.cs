using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ControlIC.Models;

namespace ControlIC.Data
{
    public class ControlICContext : DbContext
    {

        public ControlICContext (DbContextOptions<ControlICContext> options)
            : base(options)
        {
            this.Database.EnsureCreated();
        }

        public DbSet<Atividade> Atividades { get; set; }
        public DbSet<AtividadeResponsavel> AtividadeResponsaveis { get; set; }
        public DbSet<CampoPesquisa> CampoPesquisas { get; set; }
        public DbSet<Curso> Cursos { get; set; }
        public DbSet<Postagem> Postagens { get; set; }
        public DbSet<Projeto> Projetos { get; set; }
        public DbSet<ProjetoCoorientador> ProjetoCoorientadores { get; set; }
        public DbSet<ProjetoEstudante> ProjetoEstudantes { get; set; }
        public DbSet<Recrutamento> Recrutamentos { get; set; }
        public DbSet<Titulacao> Titulacoes { get; set; }
        public DbSet<TitulacaoUsuario> TitulacaoUsuarios { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }




    }
}
