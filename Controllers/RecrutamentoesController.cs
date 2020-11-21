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
using System.Reflection.Metadata;

namespace ControlIC.Controllers
{
    public class RecrutamentoesController : Controller
    {
        private readonly ControlICContext _context;

        public RecrutamentoesController(ControlICContext context)
        {
            _context = context;
        }

        // GET: Recrutamentoes
        public async Task<IActionResult> Index(int idProjeto)
        {
            int idUsuario = int.Parse(User.Claims.ElementAt(3).Value);
            var projeto = _context.Projetos.Where(p => p.ID == idProjeto && p.UsuarioID == idUsuario).FirstOrDefault();

            if (projeto == null) return NotFound();

            ViewBag.Titulo = projeto.Nome;

            HttpContext.Session.SetInt32("IdProjeto",idProjeto);

            var controlICContext = _context.Recrutamentos.Where(p => p.ProjetoID == idProjeto);
            return View(await controlICContext.ToListAsync());
        }

        // GET: Recrutamentoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            int idUsuario = int.Parse(User.Claims.ElementAt(3).Value);

            if (id == null)
            {
                return NotFound();
            }

            var recrutamento = await _context.Recrutamentos
                .Include(r => r.Projeto)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (recrutamento == null)
            {
                return NotFound();
            }

            if (recrutamento.Projeto.UsuarioID != idUsuario)
            {
                if (!recrutamento.status) return NotFound();
            }

            //var ArquivoPdf = new FileContentResult(recrutamento.Arquivo, "application/pdf");

            ViewBag.Titulo = recrutamento.Projeto.Nome;

            return View(recrutamento);
        }

        public IActionResult Download(int id)
        {
            int idUser = int.Parse(User.Claims.ElementAt(3).Value);

            var recrutamento = _context.Recrutamentos.Where(r => r.ID == id).Include(p => p.Projeto).FirstOrDefault();

            if (recrutamento == null) return NotFound();
            else if (!recrutamento.status && recrutamento.Projeto.UsuarioID != idUser) return NotFound();

            var ArquivoPdf = new FileContentResult(recrutamento.Arquivo, "application/pdf");
            ArquivoPdf.FileDownloadName = "Edital_" + recrutamento.Projeto.Nome + ".pdf";
            return ArquivoPdf;
        }

        // GET: Recrutamentoes/Create
        public IActionResult Create()
        {
            int idUsuario = int.Parse(User.Claims.ElementAt(3).Value);
            int idProjeto = HttpContext.Session.GetInt32("IdProjeto").GetValueOrDefault();

            var projeto = _context.Projetos.Where(p => p.ID == idProjeto && p.UsuarioID == idUsuario).FirstOrDefault();
            if (projeto == null) return NotFound();

            Recrutamento r = new Recrutamento();
            r.DataPostagem = DateTime.Now;
            r.ProjetoID = idProjeto;

            return View(r);
        }

        // POST: Recrutamentoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Descricao,LinkExterno,ArquivoFormato,DataPostagem,status,ProjetoID")] Recrutamento recrutamento)
        {
            if (string.IsNullOrEmpty(recrutamento.Descricao)) ModelState.AddModelError("Descricao", "Campo precisa estar preenchido");
            if(string.IsNullOrEmpty(recrutamento.LinkExterno)) ModelState.AddModelError("LinkExterno", "Campo precisa estar preenchido");
            if(recrutamento.ArquivoFormato == null) ModelState.AddModelError("ArquivoFormato", "Arquivo dever ser submetido.");

            if (ModelState.IsValid)
            {
                IFormFile arquivo = recrutamento.ArquivoFormato;
                if (arquivo != null)
                {
                    MemoryStream ms = new MemoryStream();
                    await arquivo.OpenReadStream().CopyToAsync(ms);
                    recrutamento.Arquivo = ms.ToArray();
                }

                if (!recrutamento.LinkExterno.Contains("https://")) 
                {
                    recrutamento.LinkExterno = "https://" + recrutamento.LinkExterno;
                }

                _context.Add(recrutamento);
                await _context.SaveChangesAsync();

                if (recrutamento.status) 
                {
                    var projeto = _context.Projetos.Where(p => p.ID == recrutamento.ProjetoID).Include(p => p.Recrutamentos).FirstOrDefault();
                    foreach (var item in projeto.Recrutamentos)
                    {
                        if (item.ID != recrutamento.ID && item.status)
                        {
                            item.status = false;
                            _context.Update(item);
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index), new { idProjeto = recrutamento.ProjetoID });
            }

            return View(recrutamento);
        }

        // GET: Recrutamentoes/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recrutamento = _context.Recrutamentos.Where(r => r.ID == id).Include(r => r.Projeto).FirstOrDefault();
            if (recrutamento == null)
            {
                return NotFound();
            }

            if (recrutamento.Projeto.UsuarioID != int.Parse(User.Claims.ElementAt(3).Value)) return NotFound(); 

            return View(recrutamento);
        }

        // POST: Recrutamentoes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Descricao,LinkExterno,Arquivo,ArquivoFormato,DataPostagem,status,ProjetoID")] Recrutamento recrutamento)
        {
            if (id != recrutamento.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (!recrutamento.LinkExterno.Contains("https://"))
                    {
                        recrutamento.LinkExterno = "https://" + recrutamento.LinkExterno;
                    }

                    recrutamento.DataPostagem = DateTime.Now;

                    IFormFile arquivo = recrutamento.ArquivoFormato;
                    if (arquivo != null)
                    {
                        MemoryStream ms = new MemoryStream();
                        await arquivo.OpenReadStream().CopyToAsync(ms);
                        recrutamento.Arquivo = ms.ToArray();
                    }

                    _context.Update(recrutamento);
                    await _context.SaveChangesAsync();

                    var projeto = _context.Projetos.Where(p => p.ID == recrutamento.ProjetoID).Include(p => p.Recrutamentos).FirstOrDefault();
                    foreach(var item in projeto.Recrutamentos) 
                    {
                        if(item.ID != recrutamento.ID && item.status) 
                        {
                            item.status = false;
                            _context.Update(item);
                        }
                    }
                    await _context.SaveChangesAsync();

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RecrutamentoExists(recrutamento.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { idProjeto = recrutamento.ProjetoID });
            }
            
            return View(recrutamento);
        }

        // GET: Recrutamentoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recrutamento = await _context.Recrutamentos
                .Include(r => r.Projeto)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (recrutamento == null)
            {
                return NotFound();
            }

            return View(recrutamento);
        }

        // POST: Recrutamentoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var recrutamento = await _context.Recrutamentos.FindAsync(id);
            _context.Recrutamentos.Remove(recrutamento);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RecrutamentoExists(int id)
        {
            return _context.Recrutamentos.Any(e => e.ID == id);
        }
    }
}
