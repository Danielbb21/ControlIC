using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ControlIC.Data;
using ControlIC.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;

namespace ControlIC.Controllers
{
    public class ProjetosController : Controller
    {
        private readonly ControlICContext _context;

        public ProjetosController(ControlICContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Procurar e preencher os projetos de um usuario professor
        /// </summary>
        private async Task ProcurarProjetoProfessoresAsync(List<Projeto> MeusProjetos, int idUser)
        {

            //Pesquisa todos os projetos que o professor foi coorientador 
            var listaCoorientadorProjetos = await _context.ProjetoCoorientadores.Where(c => c.UsuarioID == idUser).Include(p => p.Projeto).Where(p => p.Projeto.Aprovado == true).Where(P => P.Aprovado == true).Include(p => p.Projeto.CampoPesquisa).ToListAsync();
            foreach (var c in listaCoorientadorProjetos)
            {
                MeusProjetos.Add(c.Projeto);
            }

            //Pesquisa todos os projetos que o professor foi orientador
            var listaProjetosProfessorOrientador = await _context.Projetos.Where(p => p.UsuarioID == idUser).Include(p => p.CampoPesquisa).Include(p => p.Usuario).ToListAsync();
            foreach (var p in listaProjetosProfessorOrientador)
            {
                MeusProjetos.Add(p);
            }
        }

        /// <summary>
        /// Procurar e preencher os projetos de um usuario estudante
        /// </summary>
        private async Task ProcurarProjetoEstudante(List<Projeto> MeusProjetos, int idUser)
        {
            //Pesquisa todos os projetos que o estudante foi aluno 
            var listaEstudanteProjetos = await _context.ProjetoEstudantes.Where(c => c.UsuarioID == idUser).Include(p => p.Projeto).Where(p => p.Aprovado == true).Include(p => p.Projeto.CampoPesquisa).ToListAsync();
            foreach (var c in listaEstudanteProjetos)
            {
                MeusProjetos.Add(c.Projeto);
            }
        }

        //GET: Projetos
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index(string nomeProjeto)
        {
            //Captura todos os dados do usuario logado no banco 
            int idUser = Int32.Parse(HttpContext.User.Claims.ToList()[3].Value);
            int tipoUser = Int32.Parse(HttpContext.User.Claims.ToList()[1].Value);
            ViewBag.TipoUsuario = tipoUser;
            ViewBag.IdUsuario = idUser;

            //Cria a lista de projetos que será exibida na tela index de projeto
            List<Projeto> MeusProjetos = new List<Projeto>();

            //Buscar projetos de acordo com o usuário
            if (tipoUser == 2) {
                await ProcurarProjetoProfessoresAsync(MeusProjetos, idUser);
            }
            else
            {
                await ProcurarProjetoEstudante(MeusProjetos, idUser);
            }

            //Pesquisa um conjunto de projetos pelo nome
            if (!String.IsNullOrEmpty(nomeProjeto))
            {
                if (MeusProjetos.Count > 0)
                {
                    var projetosBusca = MeusProjetos.Where(p => p.Nome.ToUpper().Contains(nomeProjeto.ToUpper()));
                    ViewBag.QtdProjetos = projetosBusca.Count();
                    return View(projetosBusca.ToList());
                }
            }
            //Pesquisa todos os projetos do orientador
            ViewBag.QtdProjetos = MeusProjetos.Count();
            return View(MeusProjetos);
        }

        // GET: Projetos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var projeto = await _context.Projetos
                .Include(p => p.CampoPesquisa)
                .Include(p => p.Usuario)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (projeto == null)
            {
                return NotFound();
            }

            return View(projeto);
        }

        //[OK]GET: Projetos/Create
        public IActionResult Create()
        {
            ViewData["CampoPesquisaID"] = new SelectList(_context.CampoPesquisas, "ID", "Nome");
            return View();
        }

        //[OK]POST: Projetos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Projeto projeto)
        {
            ViewBag.ErroGeral = null;
            int idUser = 0;

            if (projeto == null)
            {
                ViewBag.ErroGeral = "Ocorreu um erro tente novamente";
            }
            else
            {
                if (projeto.Nome == null || projeto.Nome.Trim().Length <= 0) ModelState.AddModelError("Nome", "Preencha este campo");

                if (projeto.Descricao == null || projeto.Descricao.Trim().Length <= 0) ModelState.AddModelError("Descricao", "Preencha este campo");

                if (projeto.OutrasInformacoes == null || projeto.OutrasInformacoes.Trim().Length <= 0) ModelState.AddModelError("OutrasInformacoes", "Preencha este campo");

                if (projeto.CampoPesquisaID <= 0) ModelState.AddModelError("CampoPesquisaID", "Selecione uma opção");

                if (projeto.ImgProjetoFormato == null) 
                { 
                    ModelState.AddModelError("ImgProjetoFormato", "Selecione um arquivo"); 
                }

                //Capturando a imagem
                else
                {
                    IFormFile imagemEnviada = projeto.ImgProjetoFormato;
                    if (imagemEnviada != null)
                    {
                        MemoryStream ms = new MemoryStream();
                        try
                        {
                            await imagemEnviada.OpenReadStream().CopyToAsync(ms);
                            projeto.ImgProjeto = ms.ToArray();
                        }
                        catch
                        {
                            ModelState.AddModelError("ImgProjetoFormato", "Selecione um arquivo válido");
                        }
                    }
                }

                //Captura o id do usuario logado
                try
                {
                    idUser = Int32.Parse(HttpContext.User.Claims.ToList()[3].Value);
                }
                catch (Exception e)
                {
                    ViewBag.ErroGeral = "Ocorreu um erro tente novamente";
                }
            }

            //Verificação do model
            if (ModelState.IsValid && ViewBag.ErroGeral == null)
            {
                projeto.UsuarioID = idUser;
                projeto.Status = true;
                projeto.Aprovado = false;

                _context.Add(projeto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CampoPesquisaID"] = new SelectList(_context.CampoPesquisas, "ID", "Nome", projeto.CampoPesquisaID);
            //ViewData["UsuarioID"] = new SelectList(_context.Usuarios, "ID", "Email", projeto.UsuarioID);
            return View(projeto);
        }

        // GET: Projetos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var projeto = await _context.Projetos.FindAsync(id);
            if (projeto == null)
            {
                return NotFound();
            }
            ViewData["CampoPesquisaID"] = new SelectList(_context.CampoPesquisas, "ID", "Nome", projeto.CampoPesquisaID);
            TempData["ProjetoUsuarioID"] = projeto.UsuarioID;
            ViewData["UsuarioID"] = new SelectList(_context.Usuarios, "ID", "Email", projeto.UsuarioID);
            return View(projeto);
        }

        // POST: Projetos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Projeto projeto)
        {
            //id = JsonConvert.DeserializeObject<int>(TempData["ProjetoUsuarioID"].ToString());
            if (id != projeto.ID)
            {
                return NotFound();
            }

            ViewBag.ErroGeral = null;
            if (projeto == null)
            {
                ViewBag.ErroGeral = "Ocorreu um erro tente novamente";
            }
            else
            {
                if (projeto.Nome == null || projeto.Nome.Trim().Length <= 0) ModelState.AddModelError("Nome", "Preencha este campo");

                if (projeto.Descricao == null || projeto.Descricao.Trim().Length <= 0) ModelState.AddModelError("Descricao", "Preencha este campo");

                if (projeto.OutrasInformacoes == null || projeto.OutrasInformacoes.Trim().Length <= 0) ModelState.AddModelError("OutrasInformacoes", "Preencha este campo");

                if (projeto.CampoPesquisaID <= 0) ModelState.AddModelError("CampoPesquisaID", "Selecione uma opção");

                if (projeto.ImgProjeto == null || projeto.ImgProjeto.ToString().Trim().Length <= 0) { ModelState.AddModelError("ImgProjeto", "Selecione uma arquivo valido"); }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(projeto);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjetoExists(projeto.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CampoPesquisaID"] = new SelectList(_context.CampoPesquisas, "ID", "Nome", projeto.CampoPesquisaID);
            ViewData["UsuarioID"] = new SelectList(_context.Usuarios, "ID", "Email", projeto.UsuarioID);
            return View(projeto);
        }

        // GET: Projetos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var projeto = await _context.Projetos
                .Include(p => p.CampoPesquisa)
                .Include(p => p.Usuario)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (projeto == null)
            {
                return NotFound();
            }

            return View(projeto);
        }

        // POST: Projetos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var projeto = await _context.Projetos.FindAsync(id);
            _context.Projetos.Remove(projeto);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjetoExists(int id)
        {
            return _context.Projetos.Any(e => e.ID == id);
        }
    }
}
