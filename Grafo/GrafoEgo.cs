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
        public int AlturaMaxima { get; set; }
        public int[][] MatrizEgo { get; set; }
        private int TamanhoMatriz;

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

            AlturaMaxima = 0;

            this.Conteudo.Add(Nucleo);

            if (UsuarioInicial.TipoUsuario == 1) BuscaAlturaEstudante(Nucleo, 1);
            else BuscaAlturaProfessor(Nucleo, 1);

            foreach(var vertice in Conteudo) 
            {
                if (AlturaMaxima < vertice.Altura) AlturaMaxima = vertice.Altura;
            }

            DefinirMatriz();
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


        private void DefinirMatriz() 
        {
            //A MATRIZ TEM QUE ESTAR NESSE FORMATO: int[3 + 2*tamanhoAdicional + 2*AlturaMaxima][]

            //FAZER UM CHEQUE PROS QUE ESTÃO VAZIOS

            int tamanhoAdicional = 0;
            int[] alturas = new int[AlturaMaxima];
            int quantidadeLigacoes;

            if(Conteudo.Count() > 1) 
            {
                alturas[1] = Conteudo.Count(v => v.Altura == 1);
                while (alturas[1] / (((5 + 2 * tamanhoAdicional) ^ 2) - 10) > 1)
                {
                    tamanhoAdicional++;
                }

                foreach (var vertice in Conteudo.Skip(1))
                {
                    quantidadeLigacoes = vertice.Ligacoes.Count();
                    while (quantidadeLigacoes / (((5 + 2 * tamanhoAdicional) * (5 + 2 * tamanhoAdicional) / 2) - 6) > 1)
                    {
                        tamanhoAdicional++;
                    }
                }
            }

            this.TamanhoMatriz = 3 + 2 * tamanhoAdicional + 4 * AlturaMaxima;
            this.MatrizEgo = new int[TamanhoMatriz][];

            PreencherMatriz();
        }

        private void PreencherMatriz() 
        {
            int contX = 0, contY = 0, tamanhoAdicional = 0;
            int anguloX = 1, anguloY = 1;
            int delimitacaoX = 5, delimitacaoY = 3;

            Conteudo.ElementAt(0).X = ((TamanhoMatriz - 1) / 2);
            Conteudo.ElementAt(0).Y = ((TamanhoMatriz - 1) / 2);
            Conteudo.ElementAt(0).IsPrinted = true;
            
            //POSIÇÃO PARA O NUCLEO E LIGACOES
            foreach (var ligacao in Conteudo.ElementAt(0).Ligacoes)
            {
                ligacao.Saida.X = ligacao.Entrada.X - 2 - 2*tamanhoAdicional + contX;
                ligacao.Saida.Y = ligacao.Entrada.Y - 2 - 2*tamanhoAdicional + contY;

                if(contY == 0 || contY == 4) 
                {
                    contX++;
                    if (contX == 5) 
                    {
                        contX = 0;
                        contY++;
                    }
                }
                else 
                {
                    if (contX == 0) contX = 4;
                    else 
                    {
                        contX = 0;
                        contY++;
                    }
                }

                if (contX == 5 + 2 * tamanhoAdicional && contY == 5 + 2 * tamanhoAdicional) tamanhoAdicional++;

                ligacao.Saida.IsPrinted = true;
            }

            //POSICAO PARA QUALQUER OUTRO CASO
            foreach (var vertice in Conteudo.Skip(1)) 
            {
                //DEFINE AS VARIAVEIS PRO SEUS VALORES PADRÕES
                anguloX = 1;
                anguloY = 1;
                contX = 0;
                contY = 0;
                tamanhoAdicional = 0;

                //COMECA A PASSAR POR TODAS AS LIGACOES DO VERTICE
                foreach (var ligacao in vertice.Ligacoes) 
                {
                    //SO ENTRA SE ELA NAO FOI DEFINIDA AINDA
                    if (!ligacao.Saida.IsPrinted) 
                    {
                        //SE O ANGULO PASSAR DA METADE DA MATRIZ O ANGULO DE Y É NEGATIVO    
                        if (ligacao.Entrada.X > (TamanhoMatriz - 1) / 2)
                        {
                            anguloX = -1;
                            delimitacaoY = 5;
                            delimitacaoX = 3;
                        }
                        //SE FOR MENOR QUE A METADAE O ANGULO DE Y É POSITIVO
                        else if (ligacao.Entrada.X < (TamanhoMatriz - 1) / 2)
                        {
                            anguloX = 1;
                            delimitacaoY = 5;
                            delimitacaoX = 3;
                        }
                        //SE X FOR NO CENTRO O ARCO IRA MUDAR PRA APONTAR PARA CIMA OU PARA BAIXO
                        else
                        {
                            delimitacaoY = 3;
                            delimitacaoX = 5;

                            //SE PASSAR DA METADE O ANGULO É NEGATIVO
                            if (ligacao.Entrada.Y >= (TamanhoMatriz - 1) / 2)
                            {
                                anguloY = -1;
                            }
                        }

                        ligacao.Saida.X = ligacao.Entrada.X - (2 * anguloX) - 2 * tamanhoAdicional + contX * anguloX;
                        ligacao.Saida.Y = ligacao.Entrada.Y - (2 * anguloY) - 2 * tamanhoAdicional + contY * anguloY;

                        if (contY == 0 || contY == delimitacaoY - 1)
                        {
                            contX++;
                            if (contX == delimitacaoX)
                            {
                                contX = 0;
                                contY++;
                            }
                        }
                        else
                        {
                            contX = 0;
                            contY++;
                        }

                        if (contX == 5 + 2 * tamanhoAdicional && contY == 5 + 2 * tamanhoAdicional) tamanhoAdicional++;
                        ligacao.Saida.IsPrinted = true;

                        foreach(var v in Conteudo) 
                        {
                            if(v != ligacao.Saida && v.X == ligacao.Saida.X && v.Y == ligacao.Saida.Y) 
                            {
                                AbrirEspaco(ligacao.Saida.X + 1, ligacao.Saida.Y + 1);
                                ligacao.Saida.X += 1;
                                ligacao.Saida.Y += 1;
                            }
                        }
                    }
                }
            }


        }
        
        private void AbrirEspaco(double referenciaX, double referenciaY) 
        {
            foreach(var vertice in Conteudo) 
            {
                if (vertice.X >= referenciaX) vertice.X += 1;

                if (vertice.Y >= referenciaY) vertice.Y += 1;

            }
        }
    }

    

}
