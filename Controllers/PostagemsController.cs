using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ControlIC.Data;
using ControlIC.Models;
using ControlIC.Controllers;

namespace ControlIC.Views
{
    public class PostagemsController : Controller
    {
        private readonly ControlICContext _context;

        public PostagemsController(ControlICContext context)
        {
            _context = context;
        }

        // GET: Postagems
        public async Task<IActionResult> Index()
        {
            var controlICContext = _context.Postagens.Include(p => p.Projeto).Include(p => p.Usuario);
            return View(await controlICContext.ToListAsync());
        }

        // GET: Postagems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var postagem = await _context.Postagens
                .Include(p => p.Projeto)
                .Include(p => p.Usuario)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (postagem == null)
            {
                return NotFound();
            }

            return View(postagem);
        }

        // GET: Postagems/Create
        public IActionResult Create(int id)
        {
            var projeto = _context.Projetos.Where(p => p.ID == id)
                                .Include(p => p.ProjetoEstudantes)
                                .Include(p => p.projetoCoorientadores)
                                .FirstOrDefault();

            int userId = int.Parse(User.Claims.ElementAt(3).Value);

            if (projeto.UsuarioID != userId)
            {
                if (int.Parse(User.Claims.ElementAt(1).Value) == 1)
                {
                    var usuario = projeto.ProjetoEstudantes.Where(u => u.ID == userId).FirstOrDefault();
                    if (usuario == null) return NotFound();
                }
                else if (int.Parse(User.Claims.ElementAt(1).Value) == 2)
                {
                    var usuario = projeto.projetoCoorientadores.Where(u => u.ID == userId).FirstOrDefault();
                    if (usuario == null) return NotFound();
                }
            }

            Postagem p = new Postagem();
            p.ProjetoID = id;
            return View(p);
        }

        // POST: Postagems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Texto,DataPostagem,UsuarioID,ProjetoID")] Postagem postagem)
        {
            postagem.ID = 0;
            postagem.DataPostagem = DateTime.Now;
            postagem.UsuarioID = int.Parse(User.Claims.ElementAt(3).Value);

            if (ModelState.IsValid)
            {
                _context.Add(postagem);
                await _context.SaveChangesAsync();
                return RedirectToAction("ConsultarProjeto", "Projetos", new {id = postagem.ProjetoID });
            }

            return View(postagem);
        }

        // GET: Postagems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var postagem = await _context.Postagens.FindAsync(id);
            if (postagem == null)
            {
                return NotFound();
            }

            int userId = int.Parse(User.Claims.ElementAt(3).Value);

            if (postagem.UsuarioID != userId)
            {
                return NotFound();
            }

            return View(postagem);
        }

        // POST: Postagems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Texto,DataPostagem,UsuarioID,ProjetoID")] Postagem postagem)
        {
            if (id != postagem.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                postagem.DataPostagem = DateTime.Now;
                try
                {
                    _context.Update(postagem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostagemExists(postagem.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("ConsultarProjeto", "Projetos", new { id = postagem.ProjetoID });
            }
            ViewData["ProjetoID"] = new SelectList(_context.Projetos, "ID", "ID", postagem.ProjetoID);
            ViewData["UsuarioID"] = new SelectList(_context.Usuarios, "ID", "Email", postagem.UsuarioID);
            return View(postagem);
        }

        // GET: Postagems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var postagem = await _context.Postagens
                .Include(p => p.Projeto)
                .Include(p => p.Usuario)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (postagem == null)
            {
                return NotFound();
            }

            return View(postagem);
        }

        // POST: Postagems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var postagem = await _context.Postagens.FindAsync(id);
            _context.Postagens.Remove(postagem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostagemExists(int id)
        {
            return _context.Postagens.Any(e => e.ID == id);
        }
    }
}
