using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using task_manager_api.Data;
using task_manager_api.Models;

namespace task_manager_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly TaskManagerDbContext _context;

        public TasksController(TaskManagerDbContext context)
        {
            _context = context;
        }

        // Parte 2.1: Listado con orden y filtro
        // GET: /api/tasks?isCompleted=true|false
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks([FromQuery] bool? isCompleted)
        {
            IQueryable<TaskItem> query = _context.Tasks;

            if (isCompleted.HasValue)
            {
                query = query.Where(t => t.IsCompleted == isCompleted.Value);
            }

            var result = await query
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return Ok(result);
        }

        // Parte 2.2: Marcar como completada
        // PATCH: /api/tasks/{id}/complete
        [HttpPatch("{id:int}/complete")]
        public async Task<IActionResult> CompleteTask([FromRoute] int id)
        {
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.TaskId == id);

            if (task is null)
            {
                return NotFound(new { message = "Tarea no encontrada" });
            }

            if (task.IsCompleted)
            {
                return Ok(new { message = "La tarea ya estaba completada" });
            }

            task.IsCompleted = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Tarea marcada como completada" });
        }
    }
}
