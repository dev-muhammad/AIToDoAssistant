using Microsoft.AspNetCore.Mvc;
using System.Linq;
using ToDoAssistant.Services;

namespace ToDoAssistant.Controllers
{
    public class TodosController : Controller
    {
        private readonly ToDoService _toDoService;

        public TodosController(ToDoService toDoService)
        {
            _toDoService = toDoService;
        }

        [HttpGet("/todos")]
        public IActionResult Index()
        {
            var todos = _toDoService.GetAll().Select(todo => new ToDoViewModel
            {
                Id = todo.Id,
                Description = todo.Description,
                IsDone = todo.IsDone
            }).ToList();

            return View(todos);
        }
    }

    public class ToDoViewModel
    {
        public int Id { get; set; }
        public required string Description { get; set; }
        public bool IsDone { get; set; }
    }
}
