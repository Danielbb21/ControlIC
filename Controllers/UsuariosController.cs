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
using ControlIC.Grafo;
using Microsoft.AspNetCore.Http.Extensions;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Identity;

namespace ControlIC.Controllers {
    public class UsuariosController : Controller 
    {
        //---------------------------------------------------------------------------------------------------//

            // Conteudo envolvendo o contexto do controller.

            /// <summary>
            /// Meio para adiquirir o banco de dados de um objeto em especifico.
            /// </summary>
            private readonly ControlICContext _context;

            /// <summary>
            /// Inicializar o contexto do banco de dados.
            /// </summary>
            public UsuariosController(ControlICContext context)
            {
                _context = context;
            }

        //---------------------------------------------------------------------------------------------------//

            // Modelos temporários que receberam os inputs dos usuários para evitar o conflito de um modelo usuário não valido.

            /// <summary>
            /// Input que define como necessário apenas os campos de cadastro geral.
            /// </summary>
            public class InputModel {
                [Required(ErrorMessage = "O e-mail deve ser inserido.")]
                [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "E-mail inválido.")]
                public string Email { get; set; }

                [Required(ErrorMessage = "A senha deve ser inserida.")]
                [DataType(DataType.Password)]
                public string Senha { get; set; }
            }

            /// <summary>
            /// Input que define como necessário apenas o email na formatação correta.
            /// </summary>
            public class InputModelEmail 
            {
                [Required(ErrorMessage = "O E-mail deve ser inserido.")]
                [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "E-mail inválido.")]
                public string Email { get; set; }
            }

            /// <summary>
            /// Input que define como necessário o conteudo de um usuário estudante.
            /// </summary>
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

            /// <summary>
            /// Input que define como necessário o conteudo de um usuário professor.
            /// </summary>
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

            /// <summary>
            /// Input que define como necessário apenas o campo senha e a confirmação de senha, que precisa ser igual a primeira.
            /// </summary>
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

        //---------------------------------------------------------------------------------------------------//

            // Conteudo envolvendo a view de cadastro de um usuário professor.

            /// <summary>
            /// Tratamento quando a pagina CadastroProfessor é chamada.
            /// </summary>
            public IActionResult CadastroProfessor() 
            {
                return View();
            }

            /// <summary>
            /// Tratamento quando uma ação é chamada em CadastroProfessor. 
            /// Checa se os inputs foram válidos, insere a nova titulação no banco de dados e envia o email de confirmação para o usuário.
            /// </summary>
            [HttpPost]
            public async Task<IActionResult> CadastroProfessor(InputModelProfessor professor) 
            {
                if (ModelState.IsValid) 
                {
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

        //---------------------------------------------------------------------------------------------------//
        
            // Conteudo envolvendo a view de cadastro de um usuário aluno.
        
            /// <summary>
            /// Tratamento quando a pagina CadastroEstudante é chamada. Retorna a view com uma lista de anos entre 2000 até o ano atual.
            /// </summary>
            public IActionResult CadastroEstudante()
            {
                var listanos = new Dictionary<int, string>();
                for (int i = 2000; i <= DateTime.Now.Year; i++)
                {
                    listanos.Add(i, i.ToString());
                }

                ViewBag.AnoIngresso = new SelectList(listanos, "Key", "Value");
                ViewData["CursoID"] = new SelectList(_context.Cursos, "ID", "Nome");
                return View();
            }

            /// <summary>
            /// Tratamento quando uma ação é chamada em CadastroEstudante. 
            /// Checa se os inputs foram válidos e envia o email de confirmação para o usuário.
            /// </summary>
            [HttpPost]
            public IActionResult CadastroEstudante(InputModelEstudante estudante) 
            {
                if (ModelState.IsValid) 
                {
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

        //---------------------------------------------------------------------------------------------------//

            //Conteudo para a view do cadastro geral.

            /// <summary>
            /// Tratamento quando a view Cadastro é chamada.
            /// </summary>
            public IActionResult Cadastro()
            {
                return View();
            }

            /// <summary>
            /// Tratamento quando uma ação for chamada na view Cadastro. 
            /// Ela valida se os inputs foram válidos, se o email está disponivel e levará o usuario pra proxima pagina de cadastro dependendo do seu tipo. 
            /// </summary>
            [HttpPost]
            [ValidateAntiForgeryToken]
            public IActionResult Cadastro(Usuario usuario)
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        var list = _context.Usuarios.ToList();
                        var usuarioCadastrado = list.Where(a => a.Email.Equals(usuario.Email)).FirstOrDefault();

                        if (usuarioCadastrado == null)
                        {
                            usuario.Senha = Encriptar(usuario.Senha);
                            usuario.ConfirmarSenha = Encriptar(usuario.ConfirmarSenha);

                            var u = JsonConvert.DeserializeObject<Usuario>(TempData["usuarios"].ToString());
                            usuario.TipoUsuario = u.TipoUsuario;

                            TempData["usuarios"] = JsonConvert.SerializeObject(usuario);

                            if (usuario.TipoUsuario == 1)
                            {
                                return RedirectToAction(nameof(CadastroEstudante));
                            }
                            else
                            {
                                return RedirectToAction(nameof(CadastroProfessor));
                            }
                        }
                        else
                        {
                            ViewBag.Erro = "Email já utilizado";
                        }   
                    }
                }
                catch (Exception)
                {
                    ViewBag.Erro = "Ocorreu algum erro ao tentar se cadastrar, tente novamente!";
                }

                return View();
            }

        //---------------------------------------------------------------------------------------------------//

            // Conteudo envolvendo a tela de EscolhaTipo o qual define que tipo de usário está sendo cadastrado.

            /// <summary>
            /// Tratamento para quando a view EscolhaTipo for chamada.
            /// </summary>
            public IActionResult EscolhaTipo()
            {
                if (User.Identity.IsAuthenticated)
                {
                    return RedirectToAction("UserPage");
                }

                return View();
            }
        
            /// <summary>
            /// Tratamento para quando uma ação ocorrer na view EscolhaTipo. 
            /// Apenas recebe o valor do input e segue para a próxima página de cadastro.
            /// </summary>
            [HttpPost]
            public IActionResult EscolhaTipo(Usuario usuario)
            {
                TempData["usuarios"] = JsonConvert.SerializeObject(usuario);

                return RedirectToAction("Cadastro");
            }

        //---------------------------------------------------------------------------------------------------//

            // Conteudo envolvendo a view Profile, o qual apenas definirá o perfil do usuário.

            /// <summary>
            /// Tratamento para quando a view Profile for chamada.
            /// Ela irá validar se o usuário realmente chegou a essa pagina pelo seu email através do id e mandará para outra view caso contrário.
            /// </summary>
            public IActionResult Profile(String id)
            {
                //para praticidade dos testes mude o && para ||
                if (TempData["token"] != null && id != TempData["token"] as string)
                {
                    return View();
                }

                return RedirectToAction(nameof(Index));
            }

            /// <summary>
            /// Tratamento para quando alguma ação ocorrer na view Profile.
            /// Converte o arquivo inserido para um array de bytes caso o usuário tenha escolhido um perfil e finaliza o cadastro inserido ele no banco de dados.
            /// </summary>
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

        //---------------------------------------------------------------------------------------------------//

            // Conteudo envolvendo a tela de IniciarRecuperarSenha, o qual mandará a requisição de mudar senha para o email do usuário.

            /// <summary>
            /// Tratamento para quando a tela IniciarRecuperarSenha for chamada.
            /// </summary>
            public IActionResult IniciarRecuperarSenha()
                {
                    return View();
                }

            /// <summary>
            /// Tratamento para quando alguma ação ocorrer na view de IniciarRecuperarSenha.
            /// Ela checa se o input é valido, se o email está cadastrado e manda um email de requisição de mudança de senha para o usuário.
            /// </summary>
            [HttpPost]
            public IActionResult IniciarRecuperarSenha(InputModelEmail usuario)
            {
                if (ModelState.IsValid)
                {
                    var u = _context.Usuarios.Where(m => m.Email == usuario.Email).FirstOrDefault();

                    if (u == null)
                    {
                        ViewBag.Erro = "Email não está inserido no site.";
                        return View();
                    }

                    TempData["EmailRequisitado"] = usuario.Email;
                    EnviarEmail(usuario.Email, "Recuperar senha", "Houve uma requisição para mudar sua senha. Siga o link para prosseguir nesse processo:", "https://localhost:44346/Usuarios/RecuperarSenha?id=");
                    return RedirectToAction(nameof(ConfirmarEmail));
                }

                return View();
            }

        //---------------------------------------------------------------------------------------------------//
        
            // Conteudo para a tela de RecuperarSenha, o qual é mandada em um link para o usuário e muda sua senha com base em seu input.

            /// <summary>
            /// Tratamento para quando a pagina RecuperarSenha for chamada.
            /// Ela irá validar se o usuário realmente chegou a essa pagina pelo seu email através do token e mandará para outra view caso contrário.
            /// </summary>
            public IActionResult RecuperarSenha(string id)
            {
                if (TempData["token"] != null || id != TempData["token"] as string)
                {
                    return View();
                }

                return RedirectToAction(nameof(Index));
            }

            /// <summary>
            /// Tratamento para quando uma ação em RecuperarSenha for chamada.
            /// Ela checa se os inputs inseridos foram válidos e edita a senha do usuário no banco de dados.
            /// </summary>
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

        //---------------------------------------------------------------------------------------------------//

            // Conteudo para a view de LoginPage, o qual fará o login para o usuário.

            /// <summary>
            /// Tratamento para quando a view LoginPage for chamada.
            /// Ela checa se o usuário não está logado já e manda para outra view caso contrário.
            /// </summary>
            public IActionResult LoginPage()
            {
                if (User.Identity.IsAuthenticated)
                {
                    return RedirectToAction("UserPage");
                }

                return View();
            }

            /// <summary>
            /// Tratamento para quando alguma ação for chamada em LoginPage.
            /// Ela checa se os inputs foram válidos e chama a função Login para logar o usuário no site.
            /// </summary>
            [HttpPost]
            public IActionResult LoginPage(InputModel usuario)
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        usuario.Senha = Encriptar(usuario.Senha);

                        var list = _context.Usuarios.ToList();
                        var usuarioLogado = list.Where(a => a.Email.Equals(usuario.Email) && a.Senha.Equals(usuario.Senha)).FirstOrDefault();

                        if (usuarioLogado != null)
                        {
                            Login(usuarioLogado);
                            return RedirectToAction("UserPage");
                        }
                        else
                        {
                            ViewBag.Erro = "Usuário e / ou senha incorretos!";
                        }
                    }
                }
                catch (Exception)
                {
                    ViewBag.Erro = "Ocorreu algum erro ao tentar se logar, tente novamente!";
                }
                return View();
            }

        //---------------------------------------------------------------------------------------------------//
            
            // Conteudo para a view de UserPage, o qual apenas mostra as informações do usuário.

            /// <summary>
            /// Tratamento para quando a pagina UserPage for chamada.
            /// Ela pega o usuário no banco a partir do ID depositado nos claims de User, checa se ele existe no banco, 
            /// converte seu perfil para uma url de imagem e passa os campos para serem mostrados dependendo do tipo de usuário.
            /// </summary>
            [Authorize]
            public async Task<IActionResult> UserPage()
            {
                string identificador = HttpContext.User.Claims.Where(a => a.Type.Contains("nameidentifier")).FirstOrDefault().Value;
                int? id = int.Parse(identificador);

                if (id == null)
                {
                    return NotFound();
                }

                var usuario = await _context.Usuarios.Include(u => u.Curso)
                                    .Include(t => t.Titulacao)
                                    .Include(p => p.ProjetoEstudantes)
                                    .ThenInclude(p => p.Projeto)
                                    .ThenInclude(p => p.Usuario)
                                    .Include(p => p.projetoCoorientadores)
                                    .ThenInclude(p => p.Projeto)
                                    .ThenInclude(p => p.Usuario)
                                    .FirstOrDefaultAsync(m => m.ID == id);
                

                if(usuario.TipoUsuario == 3) return RedirectToAction("AdmPage");
                
                if (usuario == null)
                {
                    return NotFound();
                }

                if (usuario.ImgUsuario == null)
                {
                    ViewBag.ImageData = "/Imagens/Placeholder_Perfil.png";
                    HttpContext.Session.SetString("UserPerfil", "/Imagens/Placeholder_Perfil.png");
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
                else if(usuario.TipoUsuario == 2)
                {
                    ViewBag.Especifico = "Titulação";
                    ViewBag.ValorEspecifico = usuario.Titulacao.NomeTitulacao;
                }
                

                return View(usuario);
            }

        //---------------------------------------------------------------------------------------------------//

            // Conteudo para o endereço Logout, o qual apenas deslogará o usuário.

            /// <summary>
            /// Tratamento para quando o endereço Logout for chamado.
            /// Apenas tira o usuário como logado.
            /// </summary>
            public async Task<IActionResult> Logout()
            {
                await HttpContext.SignOutAsync();
                return RedirectToAction("LoginPage");
            }

        //---------------------------------------------------------------------------------------------------//

            // Conteudo envolvendo a view EditUser.

            /// <summary>
            /// Tratamento para quando a view EditUser é chamada.
            /// Ela pega o usuário no banco a partir do ID depositado nos claims de User, checa se ele existe no banco, 
            /// converte seu perfil para uma url de imagem e passa os campos para serem mostrados dependendo do tipo de usuário.
            /// </summary>
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

            /// <summary>
            /// Tratamento para quando alguma ação ocorrer em EditUser.
            /// Ela checa se os inputs foram válidos, converte o perfil para um array de bytes caso ele não for nulo e edita ele no banco.
            /// </summary>
            [HttpPost]
            public async Task<IActionResult> EditUser(Usuario usuario)
            {
                ModelState.Remove("ConfirmarSenha");

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

        //---------------------------------------------------------------------------------------------------//

            // Conteudo para a pagina ConfirmarEmail.

            /// <summary>
            /// Tratamento para quando a pagina ConfirmarEmail for chamada.
            /// </summary>
            public IActionResult ConfirmarEmail(string email)
            {
                ViewBag.Email = email;
                return View();
            }

        //---------------------------------------------------------------------------------------------------//

            // Funções complementares.

            /// <summary>
            /// Metodo para enviar email.
            /// </summary>
            public void EnviarEmail(string email, string titulo, string mensagem, string link)
            {
                try 
                {
                    string token = Guid.NewGuid().ToString();

                    MailMessage m = new MailMessage(new MailAddress("controlICsenai@hotmail.com", titulo), new MailAddress(email));
                    m.Subject = "Confirmação de Email";
                    m.Body = string.Format(@"Querido usuário,
                                            <br/> 
                                            {0}
                                            <br/>
                                            <br/> 
                                            <a href=""{1}{2}"" title=User Email Confirm>Link</a>",
                                            mensagem, link, token);

                    TempData["token"] = token;

                    m.IsBodyHtml = true;
                    SmtpClient smtp = new SmtpClient("smtp-mail.outlook.com", 587);
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential("controlICsenai@hotmail.com", "controlic4");
                    smtp.EnableSsl = true;
                    smtp.Send(m);
                }
                catch(Exception ex)
                {
                    TempData["Erro"] = "Um erro aconteceu. Tente novamente eu imploro.";
                }
            }

            /// <summary>
            /// Encripta uma string com base em hash.
            /// </summary>
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

            /// <summary>
            /// Realiza o login para o usuário e define os valores pra ser armazenados nos seus claims.
            /// </summary>
            private async void Login(Usuario usuario)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.Nome),
                    new Claim(ClaimTypes.Role, usuario.TipoUsuario.ToString()),
                    new Claim(ClaimTypes.Email, usuario.Email),
                    new Claim(ClaimTypes.NameIdentifier, usuario.ID.ToString())
                };

                var identidadeDeUsuario = new ClaimsIdentity(claims, "Login");
                ClaimsPrincipal claimPrincipal = new ClaimsPrincipal(identidadeDeUsuario);

                var propriedadesDeAutenticacao = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    ExpiresUtc = DateTime.Now.ToLocalTime().AddHours(2),
                    IsPersistent = true
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimPrincipal, propriedadesDeAutenticacao);
            }

        //---------------------------------------------------------------------------------------------------//
        
        public async Task<IActionResult> AceitarConvite(int id) 
        {
            int tipo = int.Parse(User.Claims.ElementAt(1).Value);
            int idUsuario = int.Parse(User.Claims.ElementAt(3).Value);
            int idProjeto = 0;

            if (tipo == 1) 
            {
                var projetoEstudante = _context.ProjetoEstudantes.Where(pe => pe.ID == id && pe.UsuarioID == idUsuario).FirstOrDefault();
                if (projetoEstudante == null) return NotFound();
                else 
                {
                    idProjeto = projetoEstudante.ProjetoID;

                    projetoEstudante.Aprovado = true;
                    _context.Update(projetoEstudante);
                    await _context.SaveChangesAsync();
                }
            }
            else 
            {
                var projetoCoorientador = _context.ProjetoCoorientadores.Where(pe => pe.ID == id && pe.UsuarioID == idUsuario).FirstOrDefault();
                if (projetoCoorientador == null) return NotFound();
                else 
                {
                    idProjeto = projetoCoorientador.ProjetoID;

                    projetoCoorientador.Aprovado = true;
                    _context.Update(projetoCoorientador);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("Details", "Projetos", new { id = idProjeto});
        }

        public async Task<IActionResult> RecusarConvite(int id) 
        {
            int tipo = int.Parse(User.Claims.ElementAt(1).Value);
            int idUsuario = int.Parse(User.Claims.ElementAt(3).Value);

            if (tipo == 1)
            {
                var projetoEstudante = _context.ProjetoEstudantes.Where(pe => pe.ID == id && pe.UsuarioID == idUsuario).FirstOrDefault();
                if (projetoEstudante == null) return NotFound();
                else
                {
                    _context.Remove(projetoEstudante);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                var projetoCoorientador = _context.ProjetoCoorientadores.Where(pe => pe.ID == id && pe.UsuarioID == idUsuario).FirstOrDefault();
                if (projetoCoorientador == null) return NotFound();
                else
                {
                    _context.Update(projetoCoorientador);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction(nameof(UserPage));
        }

        //---------------------------------------------------------------------------------------------------//
            // Conteudo do crud.

            // GET: Usuarios
            public async Task<IActionResult> Index() {
                var controlICContext = _context.Usuarios.Include(u => u.Curso);
                return View(await controlICContext.ToListAsync());
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

            // GET: Usuarios/Edit/5
            public async Task<IActionResult> Edit(int? id) {
                if (id == null) {
                    return NotFound();
                }

                if (!ValidarUsuario()) return NotFound();

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

                if (!ValidarUsuario()) return NotFound();

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

        public IActionResult AdmPage() 
        {
            if (!ValidarUsuario()) return NotFound();

            return View();
        }

        private bool ValidarUsuario()
        {
            if (int.Parse(User.Claims.ElementAt(1).Value) == 3) return true;

            return false;
        }

        public IActionResult RedeEgo() 
        {
            var listUser = _context.Usuarios.ToList();
            return View(listUser);
        }

        public IActionResult RedeEgoNucleo(int id) 
        {
            var usuario = _context.Usuarios.Where(u => u.ID == id)
                    .Include(u => u.ProjetoEstudantes)
                    .ThenInclude(u => u.Usuario)
                    .Include(u => u.projetoCoorientadores)
                    .ThenInclude(u => u.Usuario)
                    .FirstOrDefault();

            if (usuario == null) return NotFound();
            else if (User.Claims.ElementAt(1).Value != "3") 
            {
                if (int.Parse(User.Claims.ElementAt(3).Value) != usuario.ID) return NotFound();
            }

            Rede rede = new Rede(_context);

            rede.ConstruirGrafo(usuario);

            return View(rede);
        }

    }
}
