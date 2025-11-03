using Microsoft.AspNetCore.Mvc;
using MvcWebApp.Models;
using System.Threading.Tasks;

namespace MvcWebApp.Controllers
{
    public class TodoController : Controller
    {
        private readonly TodoApiClient _todoApi;

        public TodoController(TodoApiClient todoApi)
        {
            _todoApi = todoApi;
        }
        public async Task<IActionResult> Index()
        {
            var todos = await _todoApi.GetTodosAsync();
            return View(todos);
        }

        [HttpGet]
        public async Task<IActionResult> GetTodos()
           => Json(await _todoApi.GetTodosAsync());

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] TodoDto todo)
        {
            if (todo == null || string.IsNullOrWhiteSpace(todo.Title))
                return BadRequest("Invalid todo");

            var created = await _todoApi.AddTodoAsync(todo);
            if (created == null)
                return BadRequest("Could not create todo");

            return Json(created);
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] TodoDto todo)
        {
            if (todo == null)
                return BadRequest();

            var ok = await _todoApi.UpdateTodoAsync(todo.Id, todo);
            return ok ? Ok() : NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _todoApi.DeleteTodoAsync(id);
            return ok ? Ok() : NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> MoveUp(int id)
        {
            var ok = await _todoApi.MoveTaskUpAsync(id);
            return ok ? Ok() : NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> MoveDown(int id)
        {
            var ok = await _todoApi.MoveTaskDownAsync(id);
            return ok ? Ok() : NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Swap([FromBody] int[] ids)
        {
            if (ids == null || ids.Length != 2)
                return BadRequest("Provide two ids to swap");

            var ok = await _todoApi.SwapTodoPositionAsync(ids[0], ids[1]);
            return ok ? Ok() : NotFound();
        }
    }
}
