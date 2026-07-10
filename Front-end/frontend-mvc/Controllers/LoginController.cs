using Front_End.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Front_End.Controllers
{
    public class LoginController : Controller
    {
        private readonly AuthRepository _authRepository;

        public LoginController(AuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        // GET: /Auth/Login
        [HttpGet]
        public IActionResult Index()
        {
            var token = Request.Cookies["JwtToken"];

            // Se não houver token, manda de volta para a tela de login
            if (string.IsNullOrEmpty(token))
            {
                return View();
            }

            // vou criar uma rota para verificar se o token é válido

            // tiver token, redireciona para Home
            return RedirectToAction("Index", "Home");    
        }



     
        [HttpPost]
        public async Task<IActionResult> Index(string email, string password)
        {
            try
            {
                var authResult = await _authRepository.LoginAsync(email, password);

                // Se o login for bem-sucedido, grava o token no cookie
                if (authResult != null && !string.IsNullOrEmpty(authResult.Token))
                {
                    // Configurações do Cookie para torná-lo seguro
                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true, // Impede que scripts maliciosos (JS) roubem o token
                        Secure = false,  // Usar quando tiver HTTPS isso serve para segurança
                        Expires = System.DateTimeOffset.UtcNow.AddHours(2) // Tempo de expiração do token
                    };

                    // Grava o token no navegador com a chave "JwtToken"
                    Response.Cookies.Append("JwtToken", authResult.Token, cookieOptions);

                    // Redireciona para a controller das Tarefas que criamos antes
                    return RedirectToAction("Index", "Home");
                }

                // Se der erro, exibe mensagem na tela
                ViewBag.ErrorMessage = "E-mail ou senha inválidos.";
                return View("Index");
            }
            catch (HttpRequestException ex) 
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("Index");
            }

        }

    }
}
