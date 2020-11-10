using System;
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

namespace ControlIC.Controllers
{
    public class AtividadesController : Controller
    {
        private readonly ControlICContext _context;

        public AtividadesController(ControlICContext context)
        {
            _context = context;
        }
   
        public class AtividadeModel
        {
            public List<AtividadeResponsavelModel> AtividadeResponsaveisModel {get; set; }
            public Atividade Atividade { get; set; }
            
            public AtividadeModel()
            {
                this.Atividade = new Atividade();
                this.AtividadeResponsaveisModel = new List<AtividadeResponsavelModel>();
            }
        }
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


        //GET Cadastro atividade
        [Authorize]
        public IActionResult CreateAtividade(int? id) 
        {
            //Verifica se o ID é nulo
            int? idProjetoUrl = id;  
            if (idProjetoUrl == null)
            {
                return NotFound();
            }

            //Busca o projeto pelo ID passado
            var projeto = _context.Projetos.Find(idProjetoUrl);
            if (projeto == null)
            {
                return NotFound();
            }

            // Verifica é orientador ou coorientador do projeto passado pela URL
            int idUsuarioLogado = Int32.Parse(HttpContext.User.Claims.ToList()[3].Value);
            var responsavelOrientador = _context.Projetos.Where(p => p.UsuarioID == idUsuarioLogado && p.ID == idProjetoUrl).FirstOrDefault();
            var responsavelCoorientador = _context.ProjetoCoorientadores.Where(p => p.UsuarioID == idUsuarioLogado && p.ProjetoID == idProjetoUrl).FirstOrDefault();

            //Verifica se há um orientador ou coorientador logado
            if (responsavelOrientador == null && responsavelCoorientador == null)
            {
                return NotFound();
            }

            //Passar o ID do projeto para a tela
            ViewData["ProjetoID"] = idProjetoUrl;

            //Buscar estudantes para exibir na tela
            var estudantes = _context.ProjetoEstudantes.Where(p => p.ProjetoID == idProjetoUrl && p.Aprovado == true).Include(p => p.Usuario);

            //Verificar e preencher os dados dos estudantes para a viewmodel
            AtividadeModel AtividadeModel = new AtividadeModel();
            if (estudantes != null)
            {
                foreach (var e in estudantes)
                {
                    string imreBase64Data = Convert.ToBase64String(e.Usuario.ImgUsuario);
                    string imgDataURL = string.Format("data:image/png;base64,{0}", imreBase64Data);

                    AtividadeModel.AtividadeResponsaveisModel.Add(
                        new AtividadeResponsavelModel
                        {
                            NomeUsuario = e.Usuario.Nome,
                            UsuarioID = e.UsuarioID,
                            Selecionado = false,
                            ImgUsuario = imgDataURL
                        }
                    );
                }
            }

            return View(AtividadeModel);
        }

        //POST CreateAtividade
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAtividade(int id, AtividadeModel atividadeModel)
        {
            //Captura o id do projeto e criar a lista de erros especificos
            int idProjetoUrl = id;
            List<string> Erros = new List<string>();

            //Verificações de erros em atividade
            if(atividadeModel.Atividade != null)
            {
                if (atividadeModel.Atividade.Titulo == null || atividadeModel.Atividade.Titulo.Trim().Length <= 0) ModelState.AddModelError("Atividade.Titulo", "Preencha este campo");
                else if (atividadeModel.Atividade.Titulo.Length > 50) ModelState.AddModelError("Atividade.Titulo", "Limite de 50 caracteres");
                if (atividadeModel.Atividade.Texto == null || atividadeModel.Atividade.Texto.Trim().Length <= 0) ModelState.AddModelError("Atividade.Texto", "Preencha este campo");
                else if (atividadeModel.Atividade.Texto.Length > 200) ModelState.AddModelError("Atividade.Texto", "Limite de 200 caracteres");
                if (atividadeModel.Atividade.DataPrevista == null) ModelState.AddModelError("Atividade.DataPrevista", "Selecione uma data");
                else if (DateTime.Compare(atividadeModel.Atividade.DataPrevista, DateTime.Now) < 0) ModelState.AddModelError("Atividade.DataPrevista", "Selecione um dia de entrega válido");
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

        // GET: Atividades criadas pelo professor
        [Authorize]
        public async Task<IActionResult> Index(int? id)
        {
            int? idProjetoUrl = id;

            if(idProjetoUrl == null)
            {
                return NotFound();
            }
            
            if(_context.Atividades.Where(p => p.ProjetoID == idProjetoUrl) == null) return NotFound();
            
            var controlICContext = _context.Atividades.Include(a => a.Projeto).Where(a => a.ProjetoID == id);
            ViewData["ProjetoID"] = idProjetoUrl;
            ViewBag.NomeProjeto = controlICContext.First().Projeto.Nome;
            return View(await controlICContext.ToListAsync());
        }

        // GET: Atividades/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var atividade = await _context.Atividades
               .Include(a => a.Projeto)
               .Include(a => a.Participantes)
               .ThenInclude(A => A.Usuario)
               .FirstOrDefaultAsync(m => m.ID == id);
            if (atividade == null)
            {
                return NotFound();
            }
            
            return View(atividade);
        }

        // GET: Atividades/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var atividade = await _context.Atividades.FindAsync(id);
            if (atividade == null)
            {
                return NotFound();
            }

            return View(atividade);
        }

        // POST: Atividades/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Atividade atividade)
        {
            if (id != atividade.ID)
            {
                return NotFound();
            }
            //
            int tipoUser = Int32.Parse(HttpContext.User.Claims.ToList()[1].Value);
            if (tipoUser != 2) return NotFound();

            // Verifica é orientador ou coorientador do projeto passado pela URL
            int idUsuarioLogado = Int32.Parse(HttpContext.User.Claims.ToList()[3].Value);
            var responsavelOrientador = _context.Projetos.Where(p => p.UsuarioID == idUsuarioLogado && p.ID == atividade.ProjetoID).FirstOrDefault();
            var responsavelCoorientador = _context.ProjetoCoorientadores.Where(p => p.UsuarioID == idUsuarioLogado && p.ProjetoID == atividade.ProjetoID).FirstOrDefault();

            //Verifica se há um orientador ou coorientador logado
            if (responsavelOrientador == null && responsavelCoorientador == null)
            {
                return NotFound();
            }

            if (atividade.Titulo == null || atividade.Titulo.Trim().Length <= 0) ModelState.AddModelError("Titulo", "Preencha este campo");
            else if (atividade.Titulo.Length > 50) ModelState.AddModelError("Titulo", "Limite de 50 caracteres");
            if (atividade.Texto == null || atividade.Texto.Trim().Length <= 0) ModelState.AddModelError("Texto", "Preencha este campo");
            else if (atividade.Texto.Length > 200) ModelState.AddModelError("Texto", "Limite de 200 caracteres");
            if (atividade.DataPrevista == null) ModelState.AddModelError("DataPrevista", "Selecione uma data");
            else if (DateTime.Compare(atividade.DataPrevista, DateTime.Now) < 0) ModelState.AddModelError("DataPrevista", "Selecione um dia de entrega válido");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(atividade);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AtividadeExists(atividade.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { id= atividade.ProjetoID});
            }

            return View(atividade);
        }

        // GET: Atividades/Delete/5
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

            return View(atividade);
        }

        // POST: Atividades/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var atividade = await _context.Atividades.FindAsync(id);
            int projetoID = atividade.ProjetoID;
            _context.Atividades.Remove(atividade);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { id = projetoID});
        }

        private bool AtividadeExists(int id)
        {
            return _context.Atividades.Any(e => e.ID == id);
        }

        public IActionResult Download(int id)
        {
            int idUser = int.Parse(User.Claims.ElementAt(3).Value);
            int atvResponsavelID = id;
            var Resposta = _context.AtividadeResponsaveis.Where(r => r.ID == atvResponsavelID).Include(a => a.Usuario).Include(a => a.Atividade).FirstOrDefault();

            if (Resposta == null || Resposta.Arquivo == null) return NotFound();
            if (Resposta.Atividade == null) return NotFound();
            else if (!Resposta.Atividade.Restricao && Resposta.UsuarioID != idUser) return NotFound();

            var ArquivoPdf = new FileContentResult(Resposta.Arquivo, "application/pdf");
            ArquivoPdf.FileDownloadName = Resposta.Atividade.Titulo +"_"+ Resposta.Usuario.Nome + ".pdf";
            return ArquivoPdf;
        }
    }
}
