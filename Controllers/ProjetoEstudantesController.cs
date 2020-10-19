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
    public class ProjetoEstudantesController : Controller
    {
        private readonly ControlICContext _context;

        public ProjetoEstudantesController(ControlICContext context)
        {
            _context = context;
        }

        // GET: ProjetoEstudantes
        public async Task<IActionResult> Index()
        {
            var controlICContext = _context.ProjetoEstudantes.Include(p => p.Projeto).Include(p => p.Usuario);
            return View(await controlICContext.ToListAsync());
        }

        // GET: ProjetoEstudantes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var projetoEstudante = await _context.ProjetoEstudantes
                .Include(p => p.Projeto)
                .Include(p => p.Usuario)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (projetoEstudante == null)
            {
                return NotFound();
            }

            return View(projetoEstudante);
        }

        // GET: ProjetoEstudantes/Create
        public IActionResult Create()
        {
            ViewData["ProjetoID"] = new SelectList(_context.Projetos, "ID", "Nome");
            ViewData["UsuarioID"] = new SelectList(_context.Usuarios, "ID", "Email");
            return View();
        }

        // POST: ProjetoEstudantes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Aprovado,ProjetoID,UsuarioID")] ProjetoEstudante projetoEstudante)
        {
            if (ModelState.IsValid)
            {
                _context.Add(projetoEstudante);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProjetoID"] = new SelectList(_context.Projetos, "ID", "Nome", projetoEstudante.ProjetoID);
            ViewData["UsuarioID"] = new SelectList(_context.Usuarios, "ID", "Email", projetoEstudante.UsuarioID);
            return View(projetoEstudante);
        }

        // GET: ProjetoEstudantes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var projetoEstudante = await _context.ProjetoEstudantes.FindAsync(id);
            if (projetoEstudante == null)
            {
                return NotFound();
            }
            ViewData["ProjetoID"] = new SelectList(_context.Projetos, "ID", "Nome", projetoEstudante.ProjetoID);
            ViewData["UsuarioID"] = new SelectList(_context.Usuarios, "ID", "Email", projetoEstudante.UsuarioID);
            return View(projetoEstudante);
        }

        // POST: ProjetoEstudantes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Aprovado,ProjetoID,UsuarioID")] ProjetoEstudante projetoEstudante)
        {
            if (id != projetoEstudante.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(projetoEstudante);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjetoEstudanteExists(projetoEstudante.ID))
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
            ViewData["ProjetoID"] = new SelectList(_context.Projetos, "ID", "Nome", projetoEstudante.ProjetoID);
            ViewData["UsuarioID"] = new SelectList(_context.Usuarios, "ID", "Email", projetoEstudante.UsuarioID);
            return View(projetoEstudante);
        }

        // GET: ProjetoEstudantes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var projetoEstudante = await _context.ProjetoEstudantes
                .Include(p => p.Projeto)
                .Include(p => p.Usuario)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (projetoEstudante == null)
            {
                return NotFound();
            }

            return View(projetoEstudante);
        }

        // POST: ProjetoEstudantes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var projetoEstudante = await _context.ProjetoEstudantes.FindAsync(id);
            _context.ProjetoEstudantes.Remove(projetoEstudante);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjetoEstudanteExists(int id)
        {
            return _context.ProjetoEstudantes.Any(e => e.ID == id);
        }
    }
}
