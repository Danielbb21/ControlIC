﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ControlIC.Data;
using ControlIC.Models;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography.X509Certificates;
using System.IO;

namespace ControlIC.Controllers
{
    public class AtividadesController : Controller
    {
        private readonly ControlICContext _context;

        public AtividadesController(ControlICContext context)
        {
            _context = context;
        }
        
        /// <summary>
        /// ViewModel de atividade
        /// </summary>
        public class AtividadeModel
        {
            public List<AtividadeResponsavelModel> AtividadeResponsaveisModel { get; set; }
            public Atividade Atividade { get; set; }

            /// <summary>
            /// Metodo construtor da classe
            /// </summary>
            public AtividadeModel()
            {
                this.Atividade = new Atividade();
                this.AtividadeResponsaveisModel = new List<AtividadeResponsavelModel>();
            }
        }

        /// <summary>
        /// ViewModel de atividade responsável
        /// </summary>
        public class AtividadeResponsavelModel
        {
            [Display(Name = "#", Prompt = "#")]
            public int UsuarioID { get; set; }
            [Display(Name = "Imagem", Prompt = "Imagem")]
            public string ImgUsuario { get; set; }
            [Display(Name = "Estudante", Prompt = "Estudante")]
            public string NomeUsuario { get; set; }
            public bool Selecionado { get; set; }
        }

        /// <summary>
        /// [GET: Details] Página de detalhes de projeto
        /// </summary>
        [Authorize]
        public async Task<IActionResult> Details(int? id, int? idProjetoUrl)
        {
            //Verifica se os parametros são válidos
            int? idAtividade = id;
            if (idAtividade == null || idAtividade <= 0 || idProjetoUrl == null || idProjetoUrl <= 0)
            {
                return NotFound();
            }

            //Verifica se o usuário é professor
            int tipoUser = Int32.Parse(HttpContext.User.Claims.ToList()[1].Value);
            if (tipoUser != 2)
            {
                return NotFound();
            }

            //Busca a atividade de acordo com o idAtivide
            var atividade = await _context.Atividades
                  .Include(a => a.Projeto)
                  .Include(a => a.Participantes)
                  .ThenInclude(A => A.Usuario)
                  .FirstOrDefaultAsync(m => m.ID == idAtividade);

            //Verifica se a atividade existe
            if (atividade == null)
            {
                return NotFound();
            }

            //Verifica se o idProjeto em ativide é o mesmo do idProjetoUrl do parametro
            if (atividade.Projeto.ID != idProjetoUrl)
            {
                return NotFound();
            }

            // Verifica se o usuário passado tem relação com o projeto para acessar esta tela
            int idUsuarioLogado = Int32.Parse(HttpContext.User.Claims.ToList()[3].Value);
            var responsavelOrientador = _context.Projetos.Where(p => p.UsuarioID == idUsuarioLogado && p.ID == idProjetoUrl).FirstOrDefault();
            var responsavelCoorientador = _context.ProjetoCoorientadores.Where(p => p.UsuarioID == idUsuarioLogado && p.ProjetoID == idProjetoUrl).FirstOrDefault();
            if (responsavelOrientador == null && responsavelCoorientador == null)
            {
                return NotFound();
            }

            return View(atividade);
        }

        /// <summary>
        /// [GET: CreateAtividade] Página de criar atividade
        /// </summary>
        [Authorize]
        public IActionResult CreateAtividade(int? id)
        {
            int? idProjetoUrl = id;
            //Verifica se o ID é nulo
            if (idProjetoUrl == null || idProjetoUrl <= 0)
            {
                return NotFound();
            }

            //Verifica se o usuário é um professor
            if(Int32.Parse(HttpContext.User.Claims.ToList()[1].Value) != 2)
            {
                return NotFound();
            }

            // Verifica se o usuário é relacionado ao projeto
            int idUsuarioLogado = Int32.Parse(HttpContext.User.Claims.ToList()[3].Value);
            var responsavelOrientador = _context.Projetos.Where(p => p.UsuarioID == idUsuarioLogado && p.ID == idProjetoUrl).FirstOrDefault();
            var responsavelCoorientador = _context.ProjetoCoorientadores.Where(p => p.UsuarioID == idUsuarioLogado && p.ProjetoID == idProjetoUrl).FirstOrDefault();
            if (responsavelOrientador == null && responsavelCoorientador == null)
            {
                return NotFound();
            }

            //Buscar estudantes disponíveis para a atividade
            var estudantes = _context.ProjetoEstudantes.Where(p => p.ProjetoID == idProjetoUrl && p.Aprovado == true).Include(p => p.Usuario);

            //Cria e preenche a viewmodel para exibir na tela
            AtividadeModel AtividadeModel = new AtividadeModel();
            if (estudantes != null)
            {
                AtividadeModel.Atividade.ProjetoID = (int) idProjetoUrl;
                foreach (var est in estudantes)
                {
                    string imreBase64Data = Convert.ToBase64String(est.Usuario.ImgUsuario);
                    string imgDataURL = string.Format("data:image/png;base64,{0}", imreBase64Data);

                    AtividadeModel.AtividadeResponsaveisModel.Add(
                        new AtividadeResponsavelModel
                        {
                            NomeUsuario = est.Usuario.Nome,
                            UsuarioID = est.UsuarioID,
                            Selecionado = false,
                            ImgUsuario = imgDataURL
                            
                        }
                    );
                }
            }

            //Passar o ID do projeto para a tela
            ViewData["ProjetoID"] = idProjetoUrl;
            return View(AtividadeModel);
        }

        /// <summary>
        /// [POST: CreateAtividade] Página de criar atividade
        /// </summary>
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAtividade(AtividadeModel atividadeModel)
        {
            int idProjetoUrl = 0;
            //Criar a lista de erros especificos
            List<string> Erros = new List<string>();

            ModelState.Remove("Atividade.DataPrevista");

            //Verificações de erros em atividade
            if (atividadeModel.Atividade != null)
            {
                idProjetoUrl = atividadeModel.Atividade.ProjetoID;
                if (atividadeModel.Atividade.Titulo == null || atividadeModel.Atividade.Titulo.Trim().Length <= 0) ModelState.AddModelError("Atividade.Titulo", "Preencha este campo");
                else if (atividadeModel.Atividade.Titulo.Length > 50) ModelState.AddModelError("Atividade.Titulo", "Limite de 50 caracteres");
                if (atividadeModel.Atividade.Texto == null || atividadeModel.Atividade.Texto.Trim().Length <= 0) ModelState.AddModelError("Atividade.Texto", "Preencha este campo");
                else if (atividadeModel.Atividade.Texto.Length > 200) ModelState.AddModelError("Atividade.Texto", "Limite de 200 caracteres");
                
                if (DateTime.Compare(atividadeModel.Atividade.DataPrevista, DateTime.Now) < 0) ModelState.AddModelError("Atividade.DataPrevista", "Selecione um dia de entrega válido");
            }
            else
            {
                Erros.Add("Um erro inesperado ocorreu, tente novamente");
            }

            //Verificações de erros em atividadeResponsavel
            var listResponsaveisSelecionados = atividadeModel.AtividadeResponsaveisModel.Where(p => p.Selecionado == true);
            if (listResponsaveisSelecionados == null || listResponsaveisSelecionados.Count() == 0)
            {
                Erros.Add("Selecione um responsável pela atividade");
            }

            //Guarda o id do projeto para a tela
            ViewData["ProjetoID"] = idProjetoUrl;
            ViewBag.Erros = null;

            //Verifica se ocorreu algum erro no modelState
            if (ModelState.IsValid && Erros.Count == 0)
            {
                //Criando atividade e enviando para o banco
                atividadeModel.Atividade.ProjetoID = idProjetoUrl;
                _context.Atividades.Add(atividadeModel.Atividade);
                await _context.SaveChangesAsync();

                //Criando atividadeResponsável
                foreach (var i in listResponsaveisSelecionados)
                {
                    _context.AtividadeResponsaveis.Add
                    (
                        new AtividadeResponsavel
                        {
                            AtividadeID = atividadeModel.Atividade.ID,
                            Entregue = false,
                            UsuarioID = i.UsuarioID,

                        }
                    );
                }

                //Enviar dados para o banco
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { id = idProjetoUrl });
            }
            ViewBag.Erros = Erros;
            return View(atividadeModel);
        }

        /// <summary>
        /// [GET: Edit] Página de edição da atividade
        /// </summary>
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            //Verifica se os parametros são válidos
            int? idAtividade = id;
            if (idAtividade == null || idAtividade <= 0)
            {
                return NotFound();
            }

            //Verifica se o usuário é professor
            int tipoUser = Int32.Parse(HttpContext.User.Claims.ToList()[1].Value);
            int idUsuarioLogado = Int32.Parse(HttpContext.User.Claims.ToList()[3].Value);
            if (tipoUser != 2)
            {
                return NotFound();
            }

            //Busca a atividade de acordo com o idAtivide
            var atividade = await _context.Atividades
                  .Include(a => a.Projeto)
                  .ThenInclude(a => a.projetoCoorientadores)
                  .Include(a => a.Participantes)
                  .ThenInclude(A => A.Usuario)
                  .FirstOrDefaultAsync(m => m.ID == idAtividade);

            //Verifica se a atividade existe
            if (atividade == null)
            {
                return NotFound();
            }

            //Verifica so usuario esta ligado a atividade
            var atvCoorientador = atividade.Projeto.projetoCoorientadores.Find(p => p.UsuarioID == idUsuarioLogado && p.ProjetoID == atividade.ProjetoID);
            if (atvCoorientador == null && atividade.Projeto.UsuarioID != idUsuarioLogado)
            {
                return NotFound();
            }

            return View(atividade);
        }
        
        /// <summary>
        /// [POST: Edit] Página de edição da atividade
        /// </summary>
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Atividade atividade)
        {
            //Verifica se os parametros são válidos
            int? idAtividade = id;
            if (idAtividade <= 0 )
            {
                return NotFound();
            }

            //Verifica se o usuário é professor
            int tipoUser = Int32.Parse(HttpContext.User.Claims.ToList()[1].Value);
            if (tipoUser != 2)
            {
                return NotFound();
            }

            //Verifica se a atividade existe
            if (!AtividadeExists(atividade.ID))
            {
                return NotFound();
            }
            
            //Verifica os atributos do objeto atividade são válidos
            if (atividade.Titulo == null || atividade.Titulo.Trim().Length <= 0) ModelState.AddModelError("Titulo", "Preencha este campo");
            else if (atividade.Titulo.Length > 50) ModelState.AddModelError("Titulo", "Limite de 50 caracteres");
            if (atividade.Texto == null || atividade.Texto.Trim().Length <= 0) ModelState.AddModelError("Texto", "Preencha este campo");
            else if (atividade.Texto.Length > 200) ModelState.AddModelError("Texto", "Limite de 200 caracteres");
            if (atividade.DataPrevista == null) ModelState.AddModelError("DataPrevista", "Selecione uma data");
            else if (DateTime.Compare(atividade.DataPrevista, DateTime.Now) < 0) ModelState.AddModelError("DataPrevista", "Selecione um dia de entrega válido");

            //Verificações do modelState
            ViewBag.Erro = null;
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(atividade);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index), new { id = atividade.ProjetoID });
                }
                catch (DbUpdateConcurrencyException)
                {
                    ViewBag.Erro = "Um erro inesperado ocorreu tente novamete";
                }
            }
            return View(atividade);
        }

        /// <summary>
        /// [GET: Entregar] Página de entregar atividade
        /// </summary>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Entregar(int? id)
        {
            //Verifica os parametros
            if (id == null)
            {
                return NotFound();
            }
            int idUsuarioLogado = Int32.Parse(HttpContext.User.Claims.ToList()[3].Value);
            int tipoUser = Int32.Parse(HttpContext.User.Claims.ToList()[1].Value);

            //Verifica se o usuario é aluno
            if (tipoUser != 1) return NotFound();

            //Busca a atividade de acordo com o id da atividade da URL
            var atividadeEntrega = await _context.AtividadeResponsaveis
                  .Include(a => a.Atividade)
                  .ThenInclude(a => a.Participantes)
                  .ThenInclude(A => A.Usuario)
                  .FirstOrDefaultAsync(m => m.ID == id);

            //Verifica se a atividade existe e se o usuario é responsavel
            if (atividadeEntrega == null || atividadeEntrega.UsuarioID != idUsuarioLogado)
            {
                return NotFound();
            }

            //converter array de bytes em IFormFile

            ViewData["idProjeto"] = atividadeEntrega.Atividade.ProjetoID;
            return View(atividadeEntrega);
        }

        /// <summary>
        /// [POST: Entregar] Página de entregar atividade 
        /// </summary>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Entregar(AtividadeResponsavel atividadeResponsavel)
        {
            IFormFile imagemEnviada = atividadeResponsavel.ArquivoFormato;
            

            if (imagemEnviada != null)
            {
                MemoryStream ms = new MemoryStream();
                try
                {
                    await imagemEnviada.OpenReadStream().CopyToAsync(ms);
                    atividadeResponsavel.Arquivo = ms.ToArray();
                }
                catch
                {
                    ModelState.AddModelError("ArquivoFormato", "Insira um arquivo válido");
                }
            }
            else ModelState.AddModelError("ArquivoFormato", "Insira um arquivo para enviar");

            ViewBag.Erro = null;
            ViewData["idProjeto"] = atividadeResponsavel.Atividade.ProjetoID;
            Atividade atv = atividadeResponsavel.Atividade;
            if (ModelState.IsValid)
            {
                IFormFile arqEnviado = atividadeResponsavel.ArquivoFormato;
                try
                {
                    atividadeResponsavel.TipoArquivo = arqEnviado.ContentType;
                    atividadeResponsavel.NomeArquivo = arqEnviado.FileName;
                    atividadeResponsavel.DataEntrega = DateTime.Now;
                    atividadeResponsavel.Entregue = true;
                    atividadeResponsavel.Atividade = null;
                    _context.AtividadeResponsaveis.Update(atividadeResponsavel);
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    LimparCampos(atividadeResponsavel, atv);
                    ViewBag.Erro = "Um erro inesperado ocorreu tente novamente.";
                    return View(atividadeResponsavel);
                }
                catch(Exception e)
                {
                    LimparCampos(atividadeResponsavel, atv);
                    ViewBag.Erro = "Formato de arquivo incompatível.";
                    return View(atividadeResponsavel);
                }
                return RedirectToAction(nameof(Index), new { id = atv.ProjetoID });
            }
            return View(atividadeResponsavel);
        }

        /// <summary>
        /// Limpar os campos mocados em entregar atividade
        /// </summary>
        private void LimparCampos(AtividadeResponsavel atividadeResponsavel, Atividade atv)
        {
            atividadeResponsavel.TipoArquivo = null;
            atividadeResponsavel.NomeArquivo = null;
            atividadeResponsavel.DataEntrega = null;
            atividadeResponsavel.Entregue = false;
            atividadeResponsavel.Atividade = atv;
        }

        /// <summary>
        /// [GET: Delete] Pagina de exclusão de atividade
        /// </summary>
        /// <param name="id">id de atividade</param>
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var atividade = await _context.Atividades
                .Include(a => a.Projeto)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (atividade == null)
            {
                return NotFound();
            }

            //Verifica se atividade é do orientador do projeto
            int idUsuarioLogado = Int32.Parse(HttpContext.User.Claims.ToList()[3].Value);
            if (atividade.Projeto.UsuarioID != idUsuarioLogado) return NotFound();

            return View(atividade);
        }

        /// <summary>
        /// [POST: Delete] Pagina de exclusão de atividade
        /// </summary>
        /// <param name="id">id de atividade</param>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var atividade = await _context.Atividades.FindAsync(id);
            int projetoID = atividade.ProjetoID;
            _context.Atividades.Remove(atividade);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { id = projetoID});
        }

        [Authorize]
        public async Task<IActionResult> Index(int? id)
        {
            int? idProjetoUrl = id;

            if (idProjetoUrl == null)
            {
                return NotFound();
            }

            //Busca o usuario
            int idUsuarioLogado = int.Parse(User.Claims.ElementAt(3).Value);
            int tipoUser = Int32.Parse(HttpContext.User.Claims.ToList()[1].Value);
            ViewData["Orientador"] = null;

            //Verifica se o usuario que esta solicitando o documento é orientador/coorientador ou o dono da atividade
            if (tipoUser == 2)
            {
                var responsavelOrientador = _context.Projetos.Where(p => p.UsuarioID == idUsuarioLogado && p.ID == idProjetoUrl).FirstOrDefault();
                var responsavelCoorientador = _context.ProjetoCoorientadores.Where(p => p.UsuarioID == idUsuarioLogado && p.ProjetoID == idProjetoUrl).FirstOrDefault();
                if (responsavelOrientador == null && responsavelCoorientador == null)
                {
                    return NotFound();
                }
                ViewData["Orientador"] = responsavelOrientador;
            }
            else if(tipoUser == 1)
            {
                var estudante = _context.ProjetoEstudantes.Where(p => p.UsuarioID == idUsuarioLogado && p.ProjetoID == idProjetoUrl);
                if (estudante == null) return NotFound();
            }
            else
            {
                return NotFound();
            }
            var controlICContext = _context.Atividades.Include(a => a.Participantes).Include(a => a.Projeto).ThenInclude(a => a.Usuario).Where(a => a.ProjetoID == idProjetoUrl);
            ViewData["ProjetoID"] = idProjetoUrl;
            ViewData["idUsuario"] = idUsuarioLogado;
            ViewData["tipoUser"] = tipoUser;

            return View(await controlICContext.ToListAsync());
        }

        /// <summary>
        /// Verifica se uma atividade existe no banco
        /// </summary>
        private bool AtividadeExists(int id)
        {
            return _context.Atividades.Any(e => e.ID == id);
        }

        /// <summary>
        /// [GET: Download] Página de download de arquivo acessada em Details de atividade
        /// </summary>
        public IActionResult Download(int id)
        {
            int atvResponsavelID = id;
            var Resposta = _context.AtividadeResponsaveis.Where(r => r.ID == atvResponsavelID).Include(a => a.Usuario).Include(a => a.Atividade).ThenInclude(a => a.Projeto).ThenInclude(a => a.projetoCoorientadores).FirstOrDefault();

            if (Resposta == null || Resposta.Arquivo == null) return NotFound();
           
            //Verifica se a atividade NÃO é publica 
            if (!Resposta.Atividade.Restricao)
            {
                //Verifica se tem usuário logado
                if (User.Claims == null) return NotFound();
                
                //Busca o usuario
                int idUser = int.Parse(User.Claims.ElementAt(3).Value);
                int tipoUser = Int32.Parse(HttpContext.User.Claims.ToList()[1].Value);
                
                //Verifica se o usuario que esta solicitando o documento é orientador/coorientador ou o dono da atividade
                if (tipoUser == 2)
                {
                    //Verifica se o usuario NÃO é orientador ou coorientador do projeto
                    if ((Resposta.Atividade.Projeto.UsuarioID != idUser) &&
                       (Resposta.Atividade.Projeto.projetoCoorientadores.Where(p => p.UsuarioID == idUser) == null || Resposta.Atividade.Projeto.projetoCoorientadores.Where(p => p.UsuarioID == idUser).Count() <= 0)) return NotFound();
                }
                else
                {
                    //Verifica se o usuario NÃO é dono da atividade
                    if (Resposta.UsuarioID != idUser) return NotFound();
                }

            }

            FileContentResult ArquivoPdf;
            ArquivoPdf = new FileContentResult(Resposta.Arquivo, Resposta.TipoArquivo);
            string extensão;
            
            //Verifica se é pdf
            if (Resposta.TipoArquivo.Equals("application/pdf")) extensão = ".pdf";
            
            //Verifica se é word
            else if (Resposta.TipoArquivo.Equals("application/vnd.openxmlformats-officedocument.wordprocessingml.document")) extensão = ".docx";
            
            //Verifica se é excel
            else if (Resposta.TipoArquivo.Equals("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")) extensão = ".xlsx";
            
            //Verifica se é powepoint
            else if (Resposta.TipoArquivo.Equals("application/vnd.openxmlformats-officedocument.presentationml.presentation")) extensão = ".pptx";
       
            //Verifica se é .rar
            else extensão = ".zip";

            ArquivoPdf.FileDownloadName = Resposta.Atividade.Titulo + "_" + Resposta.Usuario.Nome + extensão;
            return ArquivoPdf;
        }
    }
}
