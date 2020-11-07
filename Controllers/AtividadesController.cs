using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ControlIC.Data;
using ControlIC.Models;

namespace ControlIC.Controllers
{
    public class AtividadesController : Controller
    {
        private readonly ControlICContext _context;

        public AtividadesController(ControlICContext context)
        {
            _context = context;
        }

        public class AtividadeResponsavelModel
        {
            public bool Selecionado { get; set; }
            public Usuario Usuario { get; set; }
        }

        public class AtividadeModel{
            public Atividade Atividade { get; set; }
            public List<AtividadeResponsavelModel> Responsaveis { get; set; }
        }

        // GET: Atividades
        public async Task<IActionResult> Index()
        {

            var controlICContext = _context.Atividades.Include(a => a.Projeto);
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
               .FirstOrDefaultAsync(m => m.ID == id);
            if (atividade == null)
            {
                return NotFound();
            }
            var atvResp = _context.AtividadeResponsaveis.Where(AtividadeResponsavel => AtividadeResponsavel.AtividadeID == atividade.ID);
            foreach (var item in atvResp)
            {
                var Responsavel = _context.Usuarios.Find(item.UsuarioID);
                atividade.Participantes.Add(Responsavel);
            }

            return View(atividade);
        }

        // GET: Atividades/Create
        public  IActionResult Create(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var projeto = _context.Projetos.Where(p => p.ID == id).Include(p => p.ProjetoEstudantes).ThenInclude(p => p.Usuario).FirstOrDefault();

            if (projeto == null)
            {
                return NotFound();
            }
            AtividadeModel atividadeModel = new AtividadeModel();
            
            foreach(var i in projeto.ProjetoEstudantes)
            {
                atividadeModel.Responsaveis.Add( 
                    new AtividadeResponsavelModel {
                        Selecionado = false,
                        Usuario = i.Usuario
                    }
                );
            }
            

            //ViewBag.ProjetoParticipantes = projeto.ProjetoEstudantes;
            ViewData["ProjetoID"] = projeto.ID;
            ViewData["Projeto"] = projeto.Nome;

            return View(atividadeModel);
        }

        // POST: Atividades/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int id ,Atividade atividade)
        {
            var projeto = await _context.Projetos.FindAsync(id);
            if (projeto == null)
            {
                return NotFound();
            }

            atividade.ProjetoID = id;
            if (ModelState.IsValid)
            {
                _context.Add(atividade);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Projeto"] = projeto.Nome;
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
            ViewData["ProjetoID"] = new SelectList(_context.Projetos, "ID", "Nome", atividade.ProjetoID);
            return View(atividade);
        }

        // POST: Atividades/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Titulo,Texto,DataPrevista,Restricao,Status,Cor,ProjetoID")] Atividade atividade)
        {
            if (id != atividade.ID)
            {
                return NotFound();
            }

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
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProjetoID"] = new SelectList(_context.Projetos, "ID", "Nome", atividade.ProjetoID);
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
            _context.Atividades.Remove(atividade);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AtividadeExists(int id)
        {
            return _context.Atividades.Any(e => e.ID == id);
        }
    }
}
