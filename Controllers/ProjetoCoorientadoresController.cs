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
    public class ProjetoCoorientadoresController : Controller
    {
        private readonly ControlICContext _context;

        public ProjetoCoorientadoresController(ControlICContext context)
        {
            _context = context;
        }

        // GET: ProjetoCoorientadores
        public async Task<IActionResult> Index()
        {
            var controlICContext = _context.ProjetoCoorientadores.Include(p => p.Projeto).Include(p => p.Usuario);
            return View(await controlICContext.ToListAsync());
        }

        // GET: ProjetoCoorientadores/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var projetoCoorientador = await _context.ProjetoCoorientadores
                .Include(p => p.Projeto)
                .Include(p => p.Usuario)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (projetoCoorientador == null)
            {
                return NotFound();
            }

            return View(projetoCoorientador);
        }

        // GET: ProjetoCoorientadores/Create
        public IActionResult Create()
        {
            ViewData["ProjetoID"] = new SelectList(_context.Projetos, "ID", "Nome");
            ViewData["UsuarioID"] = new SelectList(_context.Usuarios, "ID", "Email");
            return View();
        }

        // POST: ProjetoCoorientadores/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Aprovado,ProjetoID,UsuarioID")] ProjetoCoorientador projetoCoorientador)
        {
            if (ModelState.IsValid)
            {
                _context.Add(projetoCoorientador);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProjetoID"] = new SelectList(_context.Projetos, "ID", "Nome", projetoCoorientador.ProjetoID);
            ViewData["UsuarioID"] = new SelectList(_context.Usuarios, "ID", "Email", projetoCoorientador.UsuarioID);
            return View(projetoCoorientador);
        }

        // GET: ProjetoCoorientadores/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var projetoCoorientador = await _context.ProjetoCoorientadores.FindAsync(id);
            if (projetoCoorientador == null)
            {
                return NotFound();
            }
            ViewData["ProjetoID"] = new SelectList(_context.Projetos, "ID", "Nome", projetoCoorientador.ProjetoID);
            ViewData["UsuarioID"] = new SelectList(_context.Usuarios, "ID", "Email", projetoCoorientador.UsuarioID);
            return View(projetoCoorientador);
        }

        // POST: ProjetoCoorientadores/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Aprovado,ProjetoID,UsuarioID")] ProjetoCoorientador projetoCoorientador)
        {
            if (id != projetoCoorientador.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(projetoCoorientador);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjetoCoorientadorExists(projetoCoorientador.ID))
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
            ViewData["ProjetoID"] = new SelectList(_context.Projetos, "ID", "Nome", projetoCoorientador.ProjetoID);
            ViewData["UsuarioID"] = new SelectList(_context.Usuarios, "ID", "Email", projetoCoorientador.UsuarioID);
            return View(projetoCoorientador);
        }

        // GET: ProjetoCoorientadores/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var projetoCoorientador = await _context.ProjetoCoorientadores
                .Include(p => p.Projeto)
                .Include(p => p.Usuario)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (projetoCoorientador == null)
            {
                return NotFound();
            }

            return View(projetoCoorientador);
        }

        // POST: ProjetoCoorientadores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var projetoCoorientador = await _context.ProjetoCoorientadores.FindAsync(id);
            _context.ProjetoCoorientadores.Remove(projetoCoorientador);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjetoCoorientadorExists(int id)
        {
            return _context.ProjetoCoorientadores.Any(e => e.ID == id);
        }
    }
}
