using Front_End.DTOs;
using Front_End.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Front_End.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly TodoRepository _todoRepository;


        public HomeController(ILogger<HomeController> logger, TodoRepository todoRepository)
        {
            _logger = logger;
            _todoRepository = todoRepository;
        }

        // Rota 
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int limit = 10)
        {
            // Recupera o token que esta quardado no cookie do navegador
            var token = Request.Cookies["JwtToken"];

            // Se não houver token, manda de volta para a tela de login
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Index", "Login");
            }

            // Chama o repositório passando o token e os dados da tela
            var pagedResponse = await _todoRepository.GetTodosAsync(token, page, limit);
                        
            if (pagedResponse == null)
            {
                ViewBag.ErrorMessage = "Não foi possível carregar as tarefas. Verifique sua conexão ou login.";
                return View("Error");
            }
                     
            return View(pagedResponse);
        }


        // Rota opcional para deslogar
        [HttpGet]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("JwtToken");
            return RedirectToAction("Index", "Login");
        }


        [HttpPost]
        public async Task<IActionResult> Index([FromBody]  CadastroDtos.CadastroRequest cadastroDtos)
        {
            if(cadastroDtos == null)
            {
                return BadRequest(new { message = "Dados inválidos." });
            }

            // Recupera o token que esta quardado no cookie do navegador
            var token = Request.Cookies["JwtToken"];

            // Se não houver token, manda de volta para a tela de login
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Index", "Login");
            }

            var pagedResponse = await _todoRepository.PostTodosAsync(token, cadastroDtos);

            if (pagedResponse == null) 
            {
                return StatusCode(500, new { message = "Não foi possível criar a tarefa no servidor externo." });              
            }

            // Redireciona para a página inicial com a lista atualizada de tarefas
            return Ok(pagedResponse);
        }

        [HttpPut]
        public async Task<IActionResult> Index([FromBody] EditarDtos.EditarRequest editarDtos, int id)
        {
            if (editarDtos == null)
            {
                return BadRequest(new { message = "Dados inválidos." });
            }
            // Recupera o token que esta quardado no cookie do navegador
            var token = Request.Cookies["JwtToken"];
            // Se não houver token, manda de volta para a tela de login
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Index", "Login");
            }
            var result = await _todoRepository.UpdateTodoAsync(token, id, editarDtos);
            if (result == null)
            {
                return StatusCode(500, new { 
                    message = "Não foi possível atualizar a tarefa no servidor externo." 
                });
            }
            // Redireciona para a página inicial com a lista atualizada de tarefas
            return Ok(result);
        }

        [HttpDelete]
        public async Task<IActionResult> Index(int id)
        {
            // Recupera o token que esta quardado no cookie do navegador
            var token = Request.Cookies["JwtToken"];

            // Se não houver token, manda de volta para a tela de login
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Index", "Login");
            }

            var result = await _todoRepository.DeleteTodoAsync(token, id);

            if (!result)
            {
                return StatusCode(500, new { 
                    message = "Não foi possível deletar a tarefa no servidor externo." 
                });
            }

            // Redireciona para a página inicial com a lista atualizada de tarefas
            return Ok(new { 
                message = "Tarefa deletada com sucesso." 
            });
        }
        
        [HttpPatch]
        public async Task<IActionResult> AtualizarStatus(int id)
        {
            // Recupera o token que esta quardado no cookie do navegador
            var token = Request.Cookies["JwtToken"];

            // Se não houver token, manda de volta para a tela de login
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Index", "Login");
            }

            var result = await _todoRepository.UpdateCompleteTodoAsync(token, id);

            if (!result)
            {
                return StatusCode(500, new { 
                    message = "Não foi possível atualizar a tarefa no servidor externo." 
                });
            }

            // Redireciona para a página inicial com a lista atualizada de tarefas
            return Ok(new { 
                message = "Tarefa atualizada com sucesso." 
            });
        }




    }
}
