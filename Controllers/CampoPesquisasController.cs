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
    public class CampoPesquisasController : Controller
    {
        private readonly ControlICContext _context;

        public CampoPesquisasController(ControlICContext context)
        {
            _context = context;
        }

        // GET: CampoPesquisas
        public async Task<IActionResult> Index()
        {
            return View(await _context.CampoPesquisas.ToListAsync());
        }

        // GET: CampoPesquisas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var campoPesquisa = await _context.CampoPesquisas
                .FirstOrDefaultAsync(m => m.ID == id);
            if (campoPesquisa == null)
            {
                return NotFound();
            }

            return View(campoPesquisa);
        }

        // GET: CampoPesquisas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CampoPesquisas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Nome")] CampoPesquisa campoPesquisa)
        {
            if (ModelState.IsValid)
            {
                _context.Add(campoPesquisa);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(campoPesquisa);
        }

        // GET: CampoPesquisas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var campoPesquisa = await _context.CampoPesquisas.FindAsync(id);
            if (campoPesquisa == null)
            {
                return NotFound();
            }
            return View(campoPesquisa);
        }

        // POST: CampoPesquisas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Nome")] CampoPesquisa campoPesquisa)
        {
            if (id != campoPesquisa.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(campoPesquisa);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CampoPesquisaExists(campoPesquisa.ID))
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
            return View(campoPesquisa);
        }

        // GET: CampoPesquisas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var campoPesquisa = await _context.CampoPesquisas
                .FirstOrDefaultAsync(m => m.ID == id);
            if (campoPesquisa == null)
            {
                return NotFound();
            }

            return View(campoPesquisa);
        }

        // POST: CampoPesquisas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var campoPesquisa = await _context.CampoPesquisas.FindAsync(id);
            _context.CampoPesquisas.Remove(campoPesquisa);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CampoPesquisaExists(int id)
        {
            return _context.CampoPesquisas.Any(e => e.ID == id);
        }
    }
}
