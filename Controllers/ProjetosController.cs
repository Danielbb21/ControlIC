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

namespace ControlIC.Controllers
{
    public class ProjetosController : Controller
    {
        private readonly ControlICContext _context;

        public ProjetosController(ControlICContext context)
        {
            _context = context;
        }


        //[OK]GET: Projetos
        public async Task<IActionResult> Index()
        {
            ViewBag.MsgGeral = null;
             
            int idUser = 0;
            try
            {
                idUser = Int32.Parse(HttpContext.User.Claims.ToList()[3].Value);
            }
            catch (Exception e)
            {
                ViewBag.ErroGeral = "Ocorreu um erro tente novamente";
            }

            var projetosProfessorLogado = _context.Projetos.Where(p => p.UsuarioID == idUser).Include(p => p.CampoPesquisa).Include(p => p.Usuario);
            return View(await projetosProfessorLogado.ToListAsync());
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

        //[OK] GET: Projetos/Create
        public IActionResult Create()
        {
            //Tem que pegar qual o usuario logado no momento(A FAZER)
            //int idUsuarioLogado = Int32.Parse(HttpContext.User.Claims.ToList()[3].Value);
            //var user = _context.Usuarios.FindAsync(idUsuarioLogado);

            //ViewData["UsuarioID"] = user.Result.ID;

            ViewData["CampoPesquisaID"] = new SelectList(_context.CampoPesquisas, "ID", "Nome");
            return View();
        }

        //[OK] POST: Projetos/Create
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
                        catch(Exception e)
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
            //ViewData["UsuarioID"] = new SelectList(_context.Usuarios, "ID", "Email", projeto.UsuarioID);
            return View(projeto);
        }

        // POST: Projetos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Projeto projeto)
        {
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
            //ViewData["UsuarioID"] = new SelectList(_context.Usuarios, "ID", "Email", projeto.UsuarioID);
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
