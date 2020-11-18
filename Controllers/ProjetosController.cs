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
using System.Net.Mail;
using System.Net;

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

        public IActionResult ExplorarProjetos(string Nome) 
        {
            var listProjetos = _context.Projetos.Include(p => p.CampoPesquisa).ToList();

            if (!String.IsNullOrEmpty(Nome))
            {
                if (listProjetos.Count > 0)
                {
                    var projetosBusca = listProjetos.Where(p => p.Nome.ToUpper().Contains(Nome.ToUpper()));
                    return View(projetosBusca.ToList());
                }
            }

            return View(listProjetos);
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
                .Include(p => p.ProjetoEstudantes)
                .ThenInclude(p => p.Usuario)
                .Include(p => p.projetoCoorientadores)
                .ThenInclude(p => p.Usuario)
                .Include(p => p.Recrutamentos)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (projeto == null)
            {
                return NotFound();
            }

            if (projeto.ImgProjeto == null)
            {
                ViewBag.ImageData = "/Imagens/Placeholder_Perfil.png";
            }
            else
            {
                string imreBase64Data = Convert.ToBase64String(projeto.ImgProjeto);
                string imgDataURL = string.Format("data:image/png;base64,{0}", imreBase64Data);
                //Passing image data in viewbag to view  
                ViewBag.ImageData = imgDataURL;
            }

            if (projeto.Usuario.ImgUsuario == null)
            {
                projeto.Usuario.ImgUrl = "/Imagens/Placeholder_Perfil.png";
            }
            else
            {
                string imreBase64Data = Convert.ToBase64String(projeto.Usuario.ImgUsuario);
                string imgDataURL = string.Format("data:image/png;base64,{0}", imreBase64Data);
                //Passing image data in viewbag to view  
                projeto.Usuario.ImgUrl = imgDataURL;
            }

            foreach (var a in projeto.ProjetoEstudantes) 
            {
                if (a.Usuario.ImgUsuario == null)
                {
                    ViewBag.ImageData = "/Imagens/Placeholder_Perfil.png";
                }
                else
                {
                    string imreBase64Data = Convert.ToBase64String(a.Usuario.ImgUsuario);
                    string imgDataURL = string.Format("data:image/png;base64,{0}", imreBase64Data);
                    //Passing image data in viewbag to view  
                    a.Usuario.ImgUrl = imgDataURL;
                }
            }
            foreach (var a in projeto.projetoCoorientadores)
            {
                if (a.Usuario.ImgUsuario == null)
                {
                    ViewBag.ImageData = "/Imagens/Placeholder_Perfil.png";
                }
                else
                {
                    string imreBase64Data = Convert.ToBase64String(a.Usuario.ImgUsuario);
                    string imgDataURL = string.Format("data:image/png;base64,{0}", imreBase64Data);
                    //Passing image data in viewbag to view  
                    a.Usuario.ImgUrl = imgDataURL;
                }
            }

            return View(projeto);
        }

        [HttpPost]
        public async Task<IActionResult> Details(Projeto p) 
        {
            string email = p.EmailConvite;
            var usuario = _context.Usuarios.Where(u => u.Email == email).FirstOrDefault();

            int ID = 0;
            string Token;

            if (!p.Aprovado) 
            {
                TempData["Aviso"] = "Precisa estar aprovado para enviar convites.";
                return RedirectToAction(nameof(Details), p.ID);
            }

            if(usuario != null) 
            {
                if (usuario.ID == Int32.Parse(HttpContext.User.Claims.ToList()[3].Value))
                {
                    TempData["Aviso"] = "Email inválido.";
                    return RedirectToAction(nameof(Details), p.ID);
                }

                if (usuario.TipoUsuario == 1) 
                {
                    var UsuarioProjeto = _context.ProjetoEstudantes.Where(up => up.UsuarioID == usuario.ID && up.ProjetoID == p.ID).FirstOrDefault();

                    if(UsuarioProjeto != null) 
                    {
                        TempData["Aviso"] = "Email já enviado para esse usuario.";
                        return RedirectToAction(nameof(Details), p.ID);
                    }

                    ProjetoEstudante projetoEstudante = new ProjetoEstudante();
                    projetoEstudante.Aprovado = false;
                    projetoEstudante.ProjetoID = p.ID;
                    projetoEstudante.UsuarioID = usuario.ID;
                    projetoEstudante.Usuario = usuario;
                    projetoEstudante.Token = Guid.NewGuid().ToString();

                    _context.ProjetoEstudantes.Add(projetoEstudante);
                    await _context.SaveChangesAsync();

                    ID = projetoEstudante.ID;
                    Token = projetoEstudante.Token;
                }
                else 
                {
                    var UsuarioProjeto = _context.ProjetoCoorientadores.Where(up => up.UsuarioID == usuario.ID && up.ProjetoID == p.ID).FirstOrDefault();

                    if (UsuarioProjeto != null)
                    {
                        TempData["Aviso"] = "Email já enviado para esse usuário.";
                        return RedirectToAction(nameof(Details), p.ID);
                    }

                    ProjetoCoorientador projetoCoorientador = new ProjetoCoorientador();
                    projetoCoorientador.Aprovado = false;
                    projetoCoorientador.ProjetoID = p.ID;
                    projetoCoorientador.UsuarioID = usuario.ID;
                    projetoCoorientador.Usuario = usuario;
                    projetoCoorientador.Token = Guid.NewGuid().ToString();

                    _context.ProjetoCoorientadores.Add(projetoCoorientador);
                    await _context.SaveChangesAsync();

                    ID = projetoCoorientador.ID;
                    Token = projetoCoorientador.Token;
                }


                TempData["Aviso"] = "Enviado com sucesso.";
            }
            else 
            {
                TempData["Aviso"] = "Email não existe";
            }
            
            return RedirectToAction(nameof(Details), p.ID);
        }

        [Authorize]
        public async Task<IActionResult> ConviteAceito(int id, string token) 
        {
            int userID = Int32.Parse(HttpContext.User.Claims.ToList()[3].Value);
            var usuario = _context.Usuarios.Where(u => u.ID == userID).FirstOrDefault();

            if (Int32.Parse(HttpContext.User.Claims.ToList()[1].Value) == 1) 
            {
                var UsuarioProjeto = _context.ProjetoEstudantes.Where(up => up.ID == id).FirstOrDefault();

                if (UsuarioProjeto != null && UsuarioProjeto.Token == token && userID == UsuarioProjeto.UsuarioID) 
                {
                    UsuarioProjeto.Aprovado = true;

                    usuario.ProjetoEstudantes.Add(UsuarioProjeto);
                    _context.Update(UsuarioProjeto);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
            }
            else 
            {
                var UsuarioProjeto = _context.ProjetoCoorientadores.Where(up => up.ID == id).FirstOrDefault();

                if (UsuarioProjeto != null && UsuarioProjeto.Token == token && userID == UsuarioProjeto.UsuarioID)
                {
                    UsuarioProjeto.Aprovado = true;

                    usuario.projetoCoorientadores.Add(UsuarioProjeto);
                    _context.Update(UsuarioProjeto);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
            }

            return RedirectToAction(nameof(UsuariosController.UserPage));
        }

        public void EnviarEmail(string email, string titulo, string mensagem, string link)
        {
            //string token = Guid.NewGuid().ToString();

            try 
            {
                MailMessage m = new MailMessage(new MailAddress("controlICsenai@hotmail.com", titulo), new MailAddress(email));
                m.Subject = "Confirmação de Email";
                m.Body = string.Format(@"Querido usuário,
                                    <br/> 
                                    {0}
                                    <br/>
                                    <br/> 
                                    <a href=""{1}"" title=User Email Confirm>Link</a>",
                                        mensagem, link);

                m.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient("smtp-mail.outlook.com", 587);
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential("controlICsenai@hotmail.com", "controlic4");
                smtp.EnableSsl = true;
                smtp.Send(m);
            }
            catch 
            {
                TempData["Aviso"] = "Erro aconteceu.";
            }
        }

        public async Task<IActionResult> ConsultarProjeto(int? id) 
        {
            if (id == null)
            {
                return NotFound();
            }

            var projeto = await _context.Projetos
                .Include(p => p.Postagens)
                .ThenInclude(p => p.Usuario)
                .Include(p => p.ProjetoEstudantes)
                .Include(p => p.projetoCoorientadores)
                .FirstOrDefaultAsync(m => m.ID == id);

            int userId = int.Parse(User.Claims.ElementAt(3).Value);

            if (projeto.UsuarioID != userId)
            {
                if (int.Parse(User.Claims.ElementAt(1).Value) == 1)
                {
                    var usuario = projeto.ProjetoEstudantes.Where(u => u.UsuarioID == userId).FirstOrDefault();
                    if (usuario == null) return NotFound();
                }
                else if (int.Parse(User.Claims.ElementAt(1).Value) == 2)
                {
                    var usuario = projeto.projetoCoorientadores.Where(u => u.UsuarioID == userId).FirstOrDefault();
                    if (usuario == null) return NotFound();
                }
            }

            projeto.Postagens = projeto.Postagens.OrderByDescending(p => p.DataPostagem.TimeOfDay).ToList();

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

                if (projeto.CampoPesquisa.Nome != null)
                {
                    var cp = _context.CampoPesquisas.Where(cp => cp.Nome == projeto.CampoPesquisa.Nome).FirstOrDefault();

                    if (cp == null)
                    {
                        _context.CampoPesquisas.Add(projeto.CampoPesquisa);
                        await _context.SaveChangesAsync();
                        projeto.CampoPesquisaID = projeto.CampoPesquisa.ID;
                    }
                    else
                    {
                        return View(projeto.ID);
                    }
                }
                else
                {
                    if (projeto.CampoPesquisaID <= 0) ModelState.AddModelError("CampoPesquisaID", "Selecione uma opção");
                    else projeto.CampoPesquisa = _context.CampoPesquisas.Where(cp => cp.ID == projeto.CampoPesquisaID).FirstOrDefault();
                }

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
                catch
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
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception e)
                {
                    ViewBag.ErroGeral = "Ocorreu um erro tente novamente. Professor necessita ser doutor para criar um projeto.";
                } 
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

            if(int.Parse(User.Claims.ElementAt(3).Value) != projeto.UsuarioID) 
            {
                return NotFound();
            }

            if (projeto.ImgProjeto == null)
            {
                ViewBag.ImageData = "/Imagens/Placeholder_Perfil.png";
            }
            else
            {
                string imreBase64Data = Convert.ToBase64String(projeto.ImgProjeto);
                string imgDataURL = string.Format("data:image/png;base64,{0}", imreBase64Data);
                ViewBag.ImageData = imgDataURL;
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
                if (projeto.CampoPesquisa.Nome != null)
                {
                    var cp = _context.CampoPesquisas.Where(cp => cp.Nome == projeto.CampoPesquisa.Nome).FirstOrDefault();

                    if(cp == null) 
                    {
                        _context.CampoPesquisas.Add(projeto.CampoPesquisa);
                        await _context.SaveChangesAsync();
                        projeto.CampoPesquisaID = projeto.CampoPesquisa.ID;
                    }
                    else 
                    {
                        return View(projeto.ID);
                    }
                }
                else 
                {
                    projeto.CampoPesquisa = _context.CampoPesquisas.Where(cp => cp.ID == projeto.CampoPesquisaID).FirstOrDefault();
                }

                IFormFile imagemEnviada = projeto.ImgProjetoFormato;
                if (imagemEnviada != null)
                {
                    MemoryStream ms = new MemoryStream();
                    await imagemEnviada.OpenReadStream().CopyToAsync(ms);
                    projeto.ImgProjeto = ms.ToArray();
                }


                try
                {

                    //////////////////////////////////////////////

                    projeto.Aprovado = true;

                    //////////////////////////////////////////////
                    

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

            if (!ValidarUsuario()) return NotFound();

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

        public IActionResult IndexADM(string Nome) 
        {
            var listProjetos = _context.Projetos.Include(p => p.CampoPesquisa).ToList();

            if (!String.IsNullOrEmpty(Nome))
            {
                if (listProjetos.Count > 0)
                {
                    var projetosBusca = listProjetos.Where(p => p.Nome.ToUpper().Contains(Nome.ToUpper()));
                    return View(projetosBusca.ToList());
                }
            }

            return View(listProjetos);
        }

        public async Task<IActionResult> EditStatus(int id)
        {
            if (!ValidarUsuario()) return NotFound();

            var projeto = _context.Projetos.Where(i => i.ID == id).FirstOrDefault();

            if (projeto.Aprovado) projeto.Aprovado = false;
            else projeto.Aprovado = true;

            _context.Projetos.Update(projeto);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(IndexADM));
        }

        private bool ValidarUsuario()
        {
            if (int.Parse(User.Claims.ElementAt(1).Value) == 3) return true;

            return false;
        }
    }
}
