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
            public int TitulacaoID { get; set; }

            [RegularExpression(@"^(https?:\/\/)?([\w\-])+\.{1}linkedin.com([\/\w-]*)*\/?in\??\/?[^@\s/]*\/?", ErrorMessage ="Forneça o link para seu linkedin")]
            public string LinkedIn { get; set; }
        }

        public UsuariosController(ControlICContext context) {
            _context = context;
        }

        public IActionResult CadastroProfessor(Usuario usuario) {
            ViewData["TitulacaoID"] = new SelectList(_context.Titulacoes, "ID", "NomeTitulacao");
            return View();
        }

        [HttpPost]
        public IActionResult CadastroProfessor(InputModelProfessor professor) {
            if (ModelState.IsValid) {
                if(professor.DataNascimento < DateTime.Now && professor.DataNascimento.Year > 1900) 
                {
                    var u = JsonConvert.DeserializeObject<Usuario>(TempData["usuarios"].ToString());

                    u.DataNascimento = new DateTime();
                    u.DataNascimento = professor.DataNascimento;
                    u.LinkedIn = professor.LinkedIn;
                    u.Sexo = professor.Genero;
                    u.TitulacaoID = professor.TitulacaoID;

                    TempData["usuarios"] = JsonConvert.SerializeObject(u);

                    return RedirectToAction("Profile");
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

                    return RedirectToAction("Profile");
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

        public IActionResult Profile() 
        {
            return View();
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
                                return RedirectToAction("CadastroProfessor");
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

            string imreBase64Data = Convert.ToBase64String(usuario.ImgUsuario);
            string imgDataURL = string.Format("data:image/png;base64,{0}", imreBase64Data);
            //Passing image data in viewbag to view  
            ViewBag.ImageData = imgDataURL;

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


        // GET: Usuarios/Create
        public IActionResult Create() {
            ViewData["CursoID"] = new SelectList(_context.Cursos, "ID", "Nome");
            return View();
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
