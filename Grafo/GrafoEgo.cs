using ControlIC.Data;
using ControlIC.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ControlIC.Grafo
{
    //INICIAR O CONTEXT
    //RETIRAR TODOS OS USUARIOS
    //USAR ID DE USUARIO PARA PEGAR OS PROJETOS NO NOME DELE
    //PEGA O ID DOS PROJETOS PARA CHECAR NAS TABELAS DE LIGAÇÕES
    //CHECAR AS LIGAÇÕES EXISTENTES

    //???

    //SUCESSO

    public class Aresta 
    {
        public Vertice Entrada { get; set; }
        public Vertice Saida { get; set; }

    }
    public class Vertice 
    {
        public Usuario Usuario { get; set; }
        public List<Aresta> Ligacoes { get; set; }
        public int Altura { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public bool IsPrinted { get; set; }

        public Vertice() 
        {
            Ligacoes = new List<Aresta>();
            IsPrinted = false;
        }
    }
    public class Rede
    {
        private readonly ControlICContext _context;

        public Rede(ControlICContext context)
        {
            this.Conteudo = new List<Vertice>();
            _context = context;
        }

        public List<Vertice> Conteudo { get; set; }

        public void ConstruirGrafo(Usuario UsuarioInicial)
        {
            Vertice Nucleo = new Vertice();
            Nucleo.Altura = 0;
            Nucleo.Usuario = UsuarioInicial;

            this.Conteudo.Add(Nucleo);

            if (UsuarioInicial.TipoUsuario == 1) BuscaAlturaEstudante(Nucleo, 1);
            else BuscaAlturaProfessor(Nucleo, 1);
        }
        private void BuscaAlturaEstudante(Vertice Predecessor, int altura)
        {
            Usuario usuario = Predecessor.Usuario;

            var ProjetosEnvolvidos = ProjetosEnvolvidosEstudante(usuario);

            foreach (var projeto in ProjetosEnvolvidos)
            {
                Vertice VerticeProfessor = new Vertice();
                VerticeProfessor.Altura = altura;
                VerticeProfessor.Usuario = _context.Usuarios.Where(u => u.ID == projeto.Projeto.UsuarioID).FirstOrDefault();
                if (VerticeProfessor.Usuario != Predecessor.Usuario)
                {
                    var vertice = this.Conteudo.Where(u => u.Usuario.ID == VerticeProfessor.Usuario.ID).FirstOrDefault();
                    if (vertice == null)
                    {
                        this.Conteudo.Add(VerticeProfessor);
                        BuscaAlturaProfessor(VerticeProfessor, altura + 1);
                    }
                    else
                    {
                        VerticeProfessor = vertice;
                    }

                    if (Predecessor.Ligacoes.Where(p => p.Entrada.Usuario.ID == Predecessor.Usuario.ID && p.Saida.Usuario.ID == VerticeProfessor.Usuario.ID).FirstOrDefault() == null)
                    {
                        if (Predecessor.Altura > VerticeProfessor.Altura + 1) Predecessor.Altura = VerticeProfessor.Altura + 1;

                        Aresta Ligacao = new Aresta();
                        Ligacao.Entrada = Predecessor;
                        Ligacao.Saida = VerticeProfessor;

                        Predecessor.Ligacoes.Add(Ligacao);
                    }
                }

                foreach (var coorientadores in projeto.Projeto.projetoCoorientadores)
                {
                    Vertice VerticeCoorientador = new Vertice();
                    VerticeCoorientador.Altura = altura;
                    VerticeCoorientador.Usuario = _context.Usuarios.Where(u => u.ID == coorientadores.UsuarioID).FirstOrDefault();
                    if (VerticeCoorientador.Usuario != Predecessor.Usuario)
                    {
                        var vertice = this.Conteudo.Where(u => u.Usuario.ID == VerticeCoorientador.Usuario.ID).FirstOrDefault();
                        if (vertice == null)
                        {
                            this.Conteudo.Add(VerticeCoorientador);
                            BuscaAlturaProfessor(VerticeCoorientador, altura + 1);
                        }
                        else
                        {
                            VerticeCoorientador = vertice;
                        }

                        if (Predecessor.Ligacoes.Where(p => p.Entrada.Usuario.ID == Predecessor.Usuario.ID && p.Saida.Usuario.ID == VerticeCoorientador.Usuario.ID).FirstOrDefault() == null)
                        {
                            if (Predecessor.Altura > VerticeCoorientador.Altura + 1) Predecessor.Altura = VerticeCoorientador.Altura + 1;

                            Aresta Ligacao = new Aresta();
                            Ligacao.Entrada = Predecessor;
                            Ligacao.Saida = VerticeCoorientador;

                            Predecessor.Ligacoes.Add(Ligacao);
                        }
                    }
                }

                foreach (var estudantes in projeto.Projeto.ProjetoEstudantes)
                {
                    Vertice VerticeEstudante = new Vertice();
                    VerticeEstudante.Altura = altura;
                    VerticeEstudante.Usuario = _context.Usuarios.Where(u => u.ID == estudantes.UsuarioID).FirstOrDefault();

                    if (VerticeEstudante.Usuario != Predecessor.Usuario)
                    {
                        var vertice = this.Conteudo.Where(u => u.Usuario.ID == VerticeEstudante.Usuario.ID).FirstOrDefault();
                        if (vertice == null)
                        {
                            this.Conteudo.Add(VerticeEstudante);
                            BuscaAlturaEstudante(VerticeEstudante, altura + 1);
                        }
                        else
                        {
                            VerticeEstudante = vertice;
                        }

                        if (Predecessor.Ligacoes.Where(p => p.Entrada.Usuario.ID == Predecessor.Usuario.ID && p.Saida.Usuario.ID == VerticeEstudante.Usuario.ID).FirstOrDefault() == null)
                        {
                            if (Predecessor.Altura > VerticeEstudante.Altura + 1) Predecessor.Altura = VerticeEstudante.Altura + 1;

                            Aresta Ligacao = new Aresta();
                            Ligacao.Entrada = Predecessor;
                            Ligacao.Saida = VerticeEstudante;

                            Predecessor.Ligacoes.Add(Ligacao);
                        }
                    }
                }
            }
        }
        private void BuscaAlturaProfessor(Vertice Predecessor, int altura)
        {
            Usuario usuario = Predecessor.Usuario;

            var ProjetosEnvolvidos = ProjetosEnvolvidosCoorientador(usuario);
            var ProjetosCriados = ProjetosEnvolvidosOrientador(usuario);

            foreach (var projeto in ProjetosEnvolvidos)
            {
                Vertice VerticeProfessor = new Vertice();
                VerticeProfessor.Altura = altura;
                VerticeProfessor.Usuario = _context.Usuarios.Where(u => u.ID == projeto.Projeto.UsuarioID).FirstOrDefault();
                if (VerticeProfessor.Usuario != Predecessor.Usuario)
                {
                    var vertice = this.Conteudo.Where(u => u.Usuario.ID == VerticeProfessor.Usuario.ID).FirstOrDefault();
                    if (vertice == null)
                    {
                        this.Conteudo.Add(VerticeProfessor);
                        BuscaAlturaProfessor(VerticeProfessor, altura + 1);
                    }
                    else
                    {
                        VerticeProfessor = vertice;
                    }

                    if (Predecessor.Ligacoes.Where(p => p.Entrada.Usuario.ID == Predecessor.Usuario.ID && p.Saida.Usuario.ID == VerticeProfessor.Usuario.ID).FirstOrDefault() == null)
                    {
                        if (Predecessor.Altura > VerticeProfessor.Altura + 1) Predecessor.Altura = VerticeProfessor.Altura + 1;

                        Aresta Ligacao = new Aresta();
                        Ligacao.Entrada = Predecessor;
                        Ligacao.Saida = VerticeProfessor;

                        Predecessor.Ligacoes.Add(Ligacao);
                    }
                }

                foreach (var coorientadores in projeto.Projeto.projetoCoorientadores)
                {
                    Vertice VerticeCoorientador = new Vertice();
                    VerticeCoorientador.Altura = altura;
                    VerticeCoorientador.Usuario = _context.Usuarios.Where(u => u.ID == coorientadores.UsuarioID).FirstOrDefault();
                    if (VerticeCoorientador.Usuario != Predecessor.Usuario)
                    {
                        var vertice = this.Conteudo.Where(u => u.Usuario.ID == VerticeCoorientador.Usuario.ID).FirstOrDefault();
                        if (vertice == null)
                        {
                            this.Conteudo.Add(VerticeCoorientador);
                            BuscaAlturaProfessor(VerticeCoorientador, altura + 1);
                        }
                        else
                        {
                            VerticeCoorientador = vertice;
                        }

                        if (Predecessor.Ligacoes.Where(p => p.Entrada.Usuario.ID == Predecessor.Usuario.ID && p.Saida.Usuario.ID == VerticeCoorientador.Usuario.ID).FirstOrDefault() == null)
                        {
                            if (Predecessor.Altura > VerticeCoorientador.Altura + 1) Predecessor.Altura = VerticeCoorientador.Altura + 1;

                            Aresta Ligacao = new Aresta();
                            Ligacao.Entrada = Predecessor;
                            Ligacao.Saida = VerticeCoorientador;

                            Predecessor.Ligacoes.Add(Ligacao);
                        }
                    }
                }

                foreach (var estudantes in projeto.Projeto.ProjetoEstudantes)
                {
                    Vertice VerticeEstudante = new Vertice();
                    VerticeEstudante.Altura = altura;
                    VerticeEstudante.Usuario = _context.Usuarios.Where(u => u.ID == estudantes.UsuarioID).FirstOrDefault();

                    if (VerticeEstudante.Usuario != Predecessor.Usuario)
                    {
                        var vertice = this.Conteudo.Where(u => u.Usuario.ID == VerticeEstudante.Usuario.ID).FirstOrDefault();
                        if (vertice == null)
                        {
                            this.Conteudo.Add(VerticeEstudante);
                            BuscaAlturaEstudante(VerticeEstudante, altura + 1);
                        }
                        else
                        {
                            VerticeEstudante = vertice;
                        }

                        if (Predecessor.Ligacoes.Where(p => p.Entrada.Usuario.ID == Predecessor.Usuario.ID && p.Saida.Usuario.ID == VerticeEstudante.Usuario.ID).FirstOrDefault() == null)
                        {
                            if (Predecessor.Altura > VerticeEstudante.Altura + 1) Predecessor.Altura = VerticeEstudante.Altura + 1;

                            Aresta Ligacao = new Aresta();
                            Ligacao.Entrada = Predecessor;
                            Ligacao.Saida = VerticeEstudante;

                            Predecessor.Ligacoes.Add(Ligacao);
                        }
                    }
                }
            }
            foreach (var projeto in ProjetosCriados)
            {
                foreach (var coorientadores in projeto.projetoCoorientadores)
                {
                    Vertice VerticeCoorientador = new Vertice();
                    VerticeCoorientador.Altura = altura;
                    VerticeCoorientador.Usuario = _context.Usuarios.Where(u => u.ID == coorientadores.UsuarioID).FirstOrDefault();
                    if (VerticeCoorientador.Usuario != Predecessor.Usuario)
                    {
                        var vertice = this.Conteudo.Where(u => u.Usuario.ID == VerticeCoorientador.Usuario.ID).FirstOrDefault();
                        if (vertice == null)
                        {
                            this.Conteudo.Add(VerticeCoorientador);
                            BuscaAlturaProfessor(VerticeCoorientador, altura + 1);
                        }
                        else
                        {
                            VerticeCoorientador = vertice;
                        }

                        if (Predecessor.Ligacoes.Where(p => p.Entrada.Usuario.ID == Predecessor.Usuario.ID && p.Saida.Usuario.ID == VerticeCoorientador.Usuario.ID).FirstOrDefault() == null)
                        {
                            if (Predecessor.Altura > VerticeCoorientador.Altura + 1) Predecessor.Altura = VerticeCoorientador.Altura + 1;

                            Aresta Ligacao = new Aresta();
                            Ligacao.Entrada = Predecessor;
                            Ligacao.Saida = VerticeCoorientador;

                            Predecessor.Ligacoes.Add(Ligacao);
                        }
                    }
                }

                foreach (var estudantes in projeto.ProjetoEstudantes)
                {
                    Vertice VerticeEstudante = new Vertice();
                    VerticeEstudante.Altura = altura;
                    VerticeEstudante.Usuario = _context.Usuarios.Where(u => u.ID == estudantes.UsuarioID).FirstOrDefault();

                    if (VerticeEstudante.Usuario != Predecessor.Usuario)
                    {
                        var vertice = this.Conteudo.Where(u => u.Usuario.ID == VerticeEstudante.Usuario.ID).FirstOrDefault();
                        if (vertice == null)
                        {
                            this.Conteudo.Add(VerticeEstudante);
                            BuscaAlturaEstudante(VerticeEstudante, altura + 1);
                        }
                        else
                        {
                            VerticeEstudante = vertice;
                        }

                        if (Predecessor.Ligacoes.Where(p => p.Entrada.Usuario.ID == Predecessor.Usuario.ID && p.Saida.Usuario.ID == VerticeEstudante.Usuario.ID).FirstOrDefault() == null)
                        {
                            if (Predecessor.Altura > VerticeEstudante.Altura + 1) Predecessor.Altura = VerticeEstudante.Altura + 1;

                            Aresta Ligacao = new Aresta();
                            Ligacao.Entrada = Predecessor;
                            Ligacao.Saida = VerticeEstudante;

                            Predecessor.Ligacoes.Add(Ligacao);
                        }
                    }
                }
            }
        }

        public void CalcularPosicoes() 
        {
            int AlturaMaxima = 0;

            foreach(var vertice in Conteudo) 
            {
                if (vertice.Altura > AlturaMaxima) AlturaMaxima = vertice.Altura;
            }
        }

        private List<ProjetoEstudante> ProjetosEnvolvidosEstudante(Usuario usuario) 
        {
            var ProjetosEnvolvidos = _context.ProjetoEstudantes.Where(p => p.UsuarioID == usuario.ID)
                                                .Include(p => p.Projeto)
                                                .ThenInclude(p => p.projetoCoorientadores)
                                                .Include(p => p.Projeto)
                                                .ThenInclude(p => p.ProjetoEstudantes)
                                                .ToList();

            return ProjetosEnvolvidos;
        }
        private List<ProjetoCoorientador> ProjetosEnvolvidosCoorientador(Usuario usuario)
        {
            var ProjetosEnvolvidos = _context.ProjetoCoorientadores.Where(p => p.UsuarioID == usuario.ID)
                                                .Include(p => p.Projeto)
                                                .ThenInclude(p => p.projetoCoorientadores)
                                                .Include(p => p.Projeto)
                                                .ThenInclude(p => p.ProjetoEstudantes)
                                                .ToList();

            return ProjetosEnvolvidos;
        }
        private List<Projeto> ProjetosEnvolvidosOrientador(Usuario usuario)
        {
            var ProjetosEnvolvidos = _context.Projetos.Where(p => p.UsuarioID == usuario.ID)
                                                      .Include(p => p.projetoCoorientadores)
                                                      .Include(p => p.ProjetoEstudantes).ToList();

            return ProjetosEnvolvidos;
        }
    }

    

}
