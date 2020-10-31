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
using System.Reflection.Emit;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using System.Security.Policy;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Http.Extensions;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Identity;

namespace ControlIC.Controllers {
    public class UsuariosController : Controller {
        private readonly ControlICContext _context;
        public const string SessionKeyNome = "Nome";
        public const string SessionKeyID = "ID";

        public class InputModel {
            [Required(ErrorMessage = "O e-mail deve ser inserido.")]
            [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "E-mail inválido.")]
            public string Email { get; set; }

            [Required(ErrorMessage = "A senha deve ser inserida.")]
            [DataType(DataType.Password)]
            public string Senha { get; set; }
        }

        public class InputModelEmail 
        {
            [Required(ErrorMessage = "O E-mail deve ser inserido.")]
            [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "E-mail inválido.")]
            public string Email { get; set; }
        }

        public class InputModelEstudante {
            [Required]
            [DataType(DataType.Date)]
            public DateTime DataNascimento { get; set; }

            [Required]
            public int DataIngresso { get; set; }

            [Required]
            public char Genero { get; set; }

            [Required]
            public int CursoID { get; set; }

            [RegularExpression(@"^(https?:\/\/)?([\w\-])+\.{1}linkedin.com([\/\w-]*)*\/?in\??\/?[^@\s/]*\/?", ErrorMessage = "Forneça o link para seu linkedin")]
            public string LinkedIn { get; set; }
        }

        public class InputModelProfessor {
            [Required(ErrorMessage ="Data invalida")]
            [DataType(DataType.Date)]
            public DateTime DataNascimento { get; set; }

            [Required]
            public char Genero { get; set; }

            [Required]
            public Titulacao Titulacao { get; set; }

            [RegularExpression(@"^(https?:\/\/)?([\w\-])+\.{1}linkedin.com([\/\w-]*)*\/?in\??\/?[^@\s/]*\/?", ErrorMessage ="Forneça o link para seu linkedin")]
            public string LinkedIn { get; set; }
        }

        public class InputModelSenha 
        {
            [Required]
            [DataType(DataType.Password)]
            public string Senha { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Compare("Senha", ErrorMessage = "As senhas não estão iguais.")]
            public string ConfirmarSenha { get; set; }
        }

        public UsuariosController(ControlICContext context) {
            _context = context;
        }

        public IActionResult CadastroProfessor(Usuario usuario) {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CadastroProfessor(InputModelProfessor professor) {
            if (ModelState.IsValid) {
                if(professor.DataNascimento < DateTime.Now && professor.DataNascimento.Year > 1900) 
                {
                    var u = JsonConvert.DeserializeObject<Usuario>(TempData["usuarios"].ToString());

                    u.DataNascimento = new DateTime();
                    u.DataNascimento = professor.DataNascimento;
                    u.LinkedIn = professor.LinkedIn;
                    u.Sexo = professor.Genero;

                    _context.Titulacoes.Add(professor.Titulacao);
                    await _context.SaveChangesAsync();

                    u.TitulacaoID = professor.Titulacao.ID;

                    TempData["usuarios"] = JsonConvert.SerializeObject(u);

                    EnviarEmail(u.Email, "Cadastro", "Obrigado por se registrar. Siga o link para finalizar o cadastro:", "https://localhost:44346/Usuarios/Profile?id=");

                    return RedirectToAction("ConfirmarEmail");
                }
            }
            return RedirectToAction("CadastroProfessor");
        }

        [HttpPost]
        public IActionResult CadastroEstudante(InputModelEstudante estudante) {
            if (ModelState.IsValid) {
                if(estudante.DataNascimento < DateTime.Now && estudante.DataNascimento.Year > 1900) 
                {
                    var u = JsonConvert.DeserializeObject<Usuario>(TempData["usuarios"].ToString());

                    u.DataNascimento = estudante.DataNascimento;
                    u.AnoIngresso = estudante.DataIngresso;
                    u.LinkedIn = estudante.LinkedIn;
                    u.Sexo = estudante.Genero;
                    u.CursoID = estudante.CursoID;

                    TempData["usuarios"] = JsonConvert.SerializeObject(u);

                    EnviarEmail(u.Email, "Cadastro" ,"Obrigado por se registrar. Siga o link para finalizar o cadastro:", "https://localhost:44346/Usuarios/Profile?id=");

                    return RedirectToAction("ConfirmarEmail");
                }
            }
            return RedirectToAction("CadastroEstudante");
        }

        public IActionResult CadastroEstudante() {
            var listanos = new Dictionary<int, string>();
            for(int i = 2000; i <= DateTime.Now.Year; i++) 
            {
                listanos.Add(i, i.ToString());
            }

            ViewBag.AnoIngresso = new SelectList(listanos, "Key", "Value") ;
            ViewData["CursoID"] = new SelectList(_context.Cursos, "ID", "Nome");
            return View();
        }

        // GET: Usuarios
        public async Task<IActionResult> Index() {
            var controlICContext = _context.Usuarios.Include(u => u.Curso);
            return View(await controlICContext.ToListAsync());
        }

        public IActionResult Cadastro() {
            return View();
        }

        public IActionResult IniciarRecuperarSenha() 
        {
            return View();
        }

        [HttpPost]
        public IActionResult IniciarRecuperarSenha(InputModelEmail usuario) 
        {
            if(ModelState.IsValid) 
            {
                var u = _context.Usuarios.Where(m => m.Email == usuario.Email).FirstOrDefault();

                if(u == null) 
                {
                    ViewBag.Erro = "Email não está inserido no site.";
                    return View();
                }

                TempData["EmailRequisitado"] = usuario.Email;
                EnviarEmail(usuario.Email, "Recuperar senha" ,"Houve uma requisição para mudar sua senha. Siga o link para prosseguir nesse processo:", "https://localhost:44346/Usuarios/RecuperarSenha?id=");
                return RedirectToAction(nameof(ConfirmarEmail));
            }

            return View();
        }

        public IActionResult RecuperarSenha(string id)
        {
            if (TempData["token"] != null || id != TempData["token"] as string)
            {
                return View();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> RecuperarSenha(InputModelSenha senhaRequisitada) 
        {
            if (ModelState.IsValid) 
            {
                senhaRequisitada.Senha = Encriptar(senhaRequisitada.Senha);
                senhaRequisitada.ConfirmarSenha = Encriptar(senhaRequisitada.ConfirmarSenha);

                string email = TempData["EmailRequisitado"].ToString();

                var usuario = await _context.Usuarios.Include(u => u.Curso).Include(t => t.Titulacao).FirstOrDefaultAsync(m => m.Email == email);
                usuario.Senha = senhaRequisitada.Senha;

                _context.Update(usuario);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(LoginPage));
            }
            return View();
        }

        public IActionResult Profile(String id) 
        {
            if (TempData["token"] != null || id != TempData["token"] as string)
            {
                return View();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Profile(Usuario usuario) 
        {
            Usuario u = JsonConvert.DeserializeObject<Usuario>(TempData["usuarios"].ToString());
            IFormFile imagemEnviada = usuario.Perfil;
            if (imagemEnviada != null)
            {
                MemoryStream ms = new MemoryStream();
                await imagemEnviada.OpenReadStream().CopyToAsync(ms);
                u.ImgUsuario = ms.ToArray();
            }

            _context.Add(u);
            await _context.SaveChangesAsync();

            Login(u);

            TempData["token"] = null;

            return RedirectToAction("UserPage");
        }

        public string Encriptar(string str) 
        {
            MD5 md5Hash = MD5.Create();
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(str));

            StringBuilder sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cadastro(Usuario usuario) {
            try {
                if (ModelState.IsValid) {
                    if (usuario.Senha.Equals(usuario.ConfirmarSenha)) {
                        var list = _context.Usuarios.ToList();
                        var usuarioCadastrado = list.Where(a => a.Email.Equals(usuario.Email)).FirstOrDefault();

                        if (usuarioCadastrado == null) {

                            usuario.Senha = Encriptar(usuario.Senha);
                            usuario.ConfirmarSenha = Encriptar(usuario.ConfirmarSenha);

                            var u = JsonConvert.DeserializeObject<Usuario>(TempData["usuarios"].ToString());
                            usuario.TipoUsuario = u.TipoUsuario;

                            TempData["usuarios"] = JsonConvert.SerializeObject(usuario);
                            if (usuario.TipoUsuario == 1) {
                                return RedirectToAction(nameof(CadastroEstudante));
                            }
                            else {
                                return RedirectToAction(nameof(CadastroProfessor));
                            }
                        }
                        else {
                            ViewBag.Erro = "Email já utilizado";
                        }
                    }
                }
            }
            catch (Exception) {
                ViewBag.Erro = "Ocorreu algum erro ao tentar se cadastrar, tente novamente!";
            }
            return View();
        }

        public IActionResult EscolhaTipo() {
            if (User.Identity.IsAuthenticated) {
                return RedirectToAction("UserPage");
            }

            return View();
        }

        [HttpPost]
        public IActionResult EscolhaTipo(Usuario usuario) {
            TempData["usuarios"] = JsonConvert.SerializeObject(usuario);

            return RedirectToAction("Cadastro");
        }

        // GET: Usuarios/Details/5
        public async Task<IActionResult> Details(int? id) {
            if (id == null) {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Curso)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (usuario == null) {
                return NotFound();
            }

            string imreBase64Data = Convert.ToBase64String(usuario.ImgUsuario);
            string imgDataURL = string.Format("data:image/png;base64,{0}", imreBase64Data);
            //Passing image data in viewbag to view  
            ViewBag.ImageData = imgDataURL;
            return View(usuario);
        }

        private async void Login(Usuario usuario) {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(ClaimTypes.Role, usuario.TipoUsuario.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.NameIdentifier, usuario.ID.ToString())
            };

            var identidadeDeUsuario = new ClaimsIdentity(claims, "Login");
            ClaimsPrincipal claimPrincipal = new ClaimsPrincipal(identidadeDeUsuario);

            var propriedadesDeAutenticacao = new AuthenticationProperties {
                AllowRefresh = true,
                ExpiresUtc = DateTime.Now.ToLocalTime().AddHours(2),
                IsPersistent = true
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimPrincipal, propriedadesDeAutenticacao);
        }

        public async Task<IActionResult> Logout() {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index");
        }

        [Authorize]
        public async Task<IActionResult> UserPage() {
            string identificador = HttpContext.User.Claims.Where(a => a.Type.Contains("nameidentifier")).FirstOrDefault().Value;
            int? id = int.Parse(identificador);

            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios.Include(u => u.Curso).Include(t => t.Titulacao).FirstOrDefaultAsync(m => m.ID == id);

            if (usuario == null)
            {
                return NotFound();
            }

            if(usuario.ImgUsuario == null) 
            {
                ViewBag.ImageData = "/Imagens/Placeholder_Perfil.png";
            }
            else 
            {
                string imreBase64Data = Convert.ToBase64String(usuario.ImgUsuario);
                string imgDataURL = string.Format("data:image/png;base64,{0}", imreBase64Data);
                //Passing image data in viewbag to view  
                ViewBag.ImageData = imgDataURL;

                HttpContext.Session.SetString("UserPerfil", imgDataURL);
            }
            
            if (usuario.TipoUsuario == 1) 
            {
                ViewBag.Especifico = "Curso";
                ViewBag.ValorEspecifico = usuario.Curso.Nome;
            }
            else 
            {
                ViewBag.Especifico = "Titulação";
                ViewBag.ValorEspecifico = usuario.Titulacao.NomeTitulacao;
            }

            return View(usuario);
        }

        public IActionResult LoginPage() {
            if (User.Identity.IsAuthenticated) {
                return RedirectToAction("UserPage");
            }

            return View();
        }

        //GET: Usuarios/Login
        [HttpPost]
        public IActionResult LoginPage(InputModel usuario) {
            try {
                if (ModelState.IsValid) {
                    usuario.Senha = Encriptar(usuario.Senha);

                    var list = _context.Usuarios.ToList();
                    var usuarioLogado = list.Where(a => a.Email.Equals(usuario.Email) && a.Senha.Equals(usuario.Senha)).FirstOrDefault();

                    if (usuarioLogado != null) {
                        Login(usuarioLogado);
                        return RedirectToAction("UserPage");
                    }
                    else {
                        ViewBag.Erro = "Usuário e / ou senha incorretos!";
                    }
                }
            }
            catch (Exception) {
                ViewBag.Erro = "Ocorreu algum erro ao tentar se logar, tente novamente!";
            }
            return View();
        }

        public IActionResult ConfirmarEmail(string email) 
        {
            ViewBag.Email = email;
            return View();
        }

        // GET: Usuarios/Create
        public IActionResult Create() {
            ViewData["CursoID"] = new SelectList(_context.Cursos, "ID", "Nome");
            return View();
        }

        public void EnviarEmail(string email, string titulo, string mensagem, string link) 
        {
            string token = Guid.NewGuid().ToString();

            MailMessage m = new MailMessage(new MailAddress("Piuser3012@hotmail.com", titulo), new MailAddress(email));
            m.Subject = "Confirmação de Email";
            m.Body = string.Format(@"Querido usuário,
                                    <br/> 
                                    {0}
                                    <br/>
                                    <br/> 
                                    <a href=""{1}{2}"" title=User Email Confirm>Link</a>",
                                    mensagem, link ,token);

            TempData["token"] = token;

            m.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient("smtp-mail.outlook.com", 587);
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential("Piuser3012@hotmail.com", "Opioinanimus123");
            smtp.EnableSsl = true;
            smtp.Send(m);
        }

        // POST: Usuarios/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Nome,Sexo,DataNascimento,Email,Senha,LinkedIn,TipoUsuario,AnoIngresso,ImgUsuario,CursoID")] Usuario usuario) {
            if (ModelState.IsValid) {
                _context.Add(usuario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CursoID"] = new SelectList(_context.Cursos, "ID", "Nome", usuario.CursoID);
            return View(usuario);
        }

        public async Task<IActionResult> EditUser()
        {
            string identificador = HttpContext.User.Claims.Where(a => a.Type.Contains("nameidentifier")).FirstOrDefault().Value;
            int? id = int.Parse(identificador);

            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios.Include(u => u.Curso).Include(t => t.Titulacao).FirstOrDefaultAsync(m => m.ID == id);

            if (usuario == null)
            {
                return NotFound();
            }

            if (usuario.ImgUsuario == null)
            {
                ViewBag.ImageData = "/Imagens/Placeholder_Perfil.png";
            }
            else
            {
                string imreBase64Data = Convert.ToBase64String(usuario.ImgUsuario);
                string imgDataURL = string.Format("data:image/png;base64,{0}", imreBase64Data);
                //Passing image data in viewbag to view  
                ViewBag.ImageData = imgDataURL;
            }

            if (usuario.TipoUsuario == 1)
            {
                ViewBag.Especifico = "Curso";
                ViewBag.ValorEspecifico = usuario.Curso.Nome;
                ViewBag.CursoID = new SelectList(_context.Cursos, "ID", "Nome", usuario.CursoID);
            }
            else
            {
                ViewBag.Especifico = "Titulação";
                ViewBag.ValorEspecifico = usuario.Titulacao.NomeTitulacao;
                ViewBag.TitulacaoID = new SelectList(_context.Titulacoes, "ID", "NomeTitulacao");
            }

            return View(usuario);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(Usuario usuario) 
        {
            if (ModelState.IsValid)
            {
                if (usuario.DataNascimento < DateTime.Now && usuario.DataNascimento.Year > 1900)
                {
                    IFormFile imagemEnviada = usuario.Perfil;
                    if (imagemEnviada != null)
                    {
                        MemoryStream ms = new MemoryStream();
                        await imagemEnviada.OpenReadStream().CopyToAsync(ms);
                        usuario.ImgUsuario = ms.ToArray();
                    }

                    try
                    {
                        _context.Update(usuario);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!UsuarioExists(usuario.ID))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }

                    await HttpContext.SignOutAsync();

                    Login(usuario);

                    return RedirectToAction(nameof(UserPage));
                }
            }
            return RedirectToAction(nameof(EditUser));
        }

        // GET: Usuarios/Edit/5
        public async Task<IActionResult> Edit(int? id) {
            if (id == null) {
                return NotFound();
            }

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) {
                return NotFound();
            }
            ViewData["CursoID"] = new SelectList(_context.Cursos, "ID", "Nome", usuario.CursoID);
            return View(usuario);
        }

        // POST: Usuarios/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Nome,Sexo,DataNascimento,Email,Senha,LinkedIn,TipoUsuario,AnoIngresso,ImgUsuario,CursoID")] Usuario usuario) {
            if (id != usuario.ID) {
                return NotFound();
            }

            if (ModelState.IsValid) {
                try {
                    _context.Update(usuario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException) {
                    if (!UsuarioExists(usuario.ID)) {
                        return NotFound();
                    }
                    else {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CursoID"] = new SelectList(_context.Cursos, "ID", "Nome", usuario.CursoID);
            return View(usuario);
        }

        // GET: Usuarios/Delete/5
        public async Task<IActionResult> Delete(int? id) {
            if (id == null) {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Curso)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (usuario == null) {
                return NotFound();
            }

            return View(usuario);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) {
            var usuario = await _context.Usuarios.FindAsync(id);
            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UsuarioExists(int id) {
            return _context.Usuarios.Any(e => e.ID == id);
        }

    }
}
