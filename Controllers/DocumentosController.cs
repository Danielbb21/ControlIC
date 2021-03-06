﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ControlIC.Data;
using ControlIC.Models;

namespace ControlIC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentosController : Controller
    {
        private readonly ControlICContext _context;

        public DocumentosController(ControlICContext context)
        {
            _context = context;
        }

        public class ModelDocumentos 
        {
            public string NomeProjeto { get; set; }
            public string LinkDocumento { get; set; }
            public string DescricaoProjeto { get; set; }
        }

        // GET: api/Documentos
        [HttpGet]
        public async Task<ActionResult> GetDocumentos(string NomeProjeto)
        {
            List<AtividadeResponsavel> listDocumentos;

            if (!string.IsNullOrEmpty(NomeProjeto))
            {
                listDocumentos = await _context.AtividadeResponsaveis
                                            .Where(p => p.Atividade.Projeto.Nome.ToLower().Contains(NomeProjeto.ToLower()) && p.Atividade.Restricao)
                                            .Include(p => p.Atividade)
                                            .ThenInclude(p => p.Projeto)
                                            .ToListAsync();
            }
            else
            {
                listDocumentos = await _context.AtividadeResponsaveis
                                            .Where(p => p.Atividade.Restricao)
                                            .Include(p => p.Atividade)
                                            .ThenInclude(p => p.Projeto)
                                            .ToListAsync();
            }

            List<ModelDocumentos> list = new List<ModelDocumentos>() ;

            foreach (var documento in listDocumentos)
            {
                ModelDocumentos doc = new ModelDocumentos();
                doc.NomeProjeto = documento.Atividade.Projeto.Nome;
                doc.LinkDocumento = "https://localhost:44346/Atividades/Download/" + documento.ID.ToString();
                doc.DescricaoProjeto = documento.Atividade.Projeto.Descricao;
                list.Add(doc);
            }

            return View(list);
        }

        // GET: api/Documentos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<ModelDocumentos>>> GetDocumentos(int id)
        {
            var projeto = await _context.Projetos.Where(p => p.ID == id).Include(p => p.Atividades).ThenInclude(a => a.Participantes).FirstOrDefaultAsync();

            if (projeto == null)
            {
                return NotFound();
            }

            List<ModelDocumentos> list = new List<ModelDocumentos>();

            foreach (var item in projeto.Atividades.Where(a => a.Restricao))
            {
                foreach(var documento in item.Participantes) 
                {
                    ModelDocumentos documentos = new ModelDocumentos();
                    documentos.NomeProjeto = projeto.Nome;
                    documentos.LinkDocumento = "https://localhost:44346/Atividades/Download/" + documento.ID.ToString();
                    list.Add(documentos);
                }
            }

            return list;
        }

        // PUT: api/Documentos/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAtividadeResponsavel(int id, AtividadeResponsavel atividadeResponsavel)
        {
            if (id != atividadeResponsavel.ID)
            {
                return BadRequest();
            }

            _context.Entry(atividadeResponsavel).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AtividadeResponsavelExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Documentos
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<AtividadeResponsavel>> PostAtividadeResponsavel(AtividadeResponsavel atividadeResponsavel)
        {
            _context.AtividadeResponsaveis.Add(atividadeResponsavel);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAtividadeResponsavel", new { id = atividadeResponsavel.ID }, atividadeResponsavel);
        }

        // DELETE: api/Documentos/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<AtividadeResponsavel>> DeleteAtividadeResponsavel(int id)
        {
            var atividadeResponsavel = await _context.AtividadeResponsaveis.FindAsync(id);
            if (atividadeResponsavel == null)
            {
                return NotFound();
            }

            _context.AtividadeResponsaveis.Remove(atividadeResponsavel);
            await _context.SaveChangesAsync();

            return atividadeResponsavel;
        }

        private bool AtividadeResponsavelExists(int id)
        {
            return _context.AtividadeResponsaveis.Any(e => e.ID == id);
        }
    }
}
