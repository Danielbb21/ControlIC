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
    public class TitulacoesController : Controller
    {
        private readonly ControlICContext _context;

        public TitulacoesController(ControlICContext context)
        {
            _context = context;
        }

        // GET: Titulacoes
        public async Task<IActionResult> Index()
        {
            return View(await _context.Titulacoes.ToListAsync());
        }

        // GET: Titulacoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var titulacao = await _context.Titulacoes
                .FirstOrDefaultAsync(m => m.ID == id);
            if (titulacao == null)
            {
                return NotFound();
            }

            return View(titulacao);
        }

        // GET: Titulacoes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Titulacoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,NomeTitulacao,NomeInstituicao,DataTitulacao")] Titulacao titulacao)
        {
            if (ModelState.IsValid)
            {
                _context.Add(titulacao);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(titulacao);
        }

        // GET: Titulacoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var titulacao = await _context.Titulacoes.FindAsync(id);
            if (titulacao == null)
            {
                return NotFound();
            }
            return View(titulacao);
        }

        // POST: Titulacoes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,NomeTitulacao,NomeInstituicao,DataTitulacao")] Titulacao titulacao)
        {
            if (id != titulacao.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(titulacao);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TitulacaoExists(titulacao.ID))
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
            return View(titulacao);
        }

        // GET: Titulacoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var titulacao = await _context.Titulacoes
                .FirstOrDefaultAsync(m => m.ID == id);
            if (titulacao == null)
            {
                return NotFound();
            }

            return View(titulacao);
        }

        // POST: Titulacoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var titulacao = await _context.Titulacoes.FindAsync(id);
            _context.Titulacoes.Remove(titulacao);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TitulacaoExists(int id)
        {
            return _context.Titulacoes.Any(e => e.ID == id);
        }
    }
}
