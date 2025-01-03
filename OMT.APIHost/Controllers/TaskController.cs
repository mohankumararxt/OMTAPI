using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OMT.Authorization;
using OMT.DataService.Interface;
using OMT.DTO;


namespace OMT.APIHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        // Endpoint to Add Task
        [HttpPost("add")]
        public async Task<ResultDTO> AddTask([FromBody] TaskDTO taskDTO)
        {
            var result = await _taskService.AddTaskAsync(taskDTO);
            return result;
        }

        // Endpoint to Update Task
        [HttpPut("update/{taskId}")]
        public async Task<ResultDTO> UpdateTask(int taskId, [FromBody] UpdateTaskDTO updateTaskDTO)
        {
            var result = await _taskService.UpdateTaskAsync(taskId, updateTaskDTO);
            return result;
        }

        // Endpoint to Get Task List with filters
        [HttpPost("list")]
        public async Task<ResultDTO> GetTaskList([FromBody] TaskFilterDTO filters)
        {
            var result = await _taskService.GetTaskListAsync(filters);
            return result;
        }

        // Endpoint to Batch Update Tasks
        [HttpPost("batch-update")]
        public async Task<ResultDTO> BatchUpdateTasks([FromBody] BatchUpdateTaskDTO batchUpdateTaskDTO)
        {
            var result = await _taskService.BatchUpdateTasksAsync(batchUpdateTaskDTO);
            return result;
        }

        // Endpoint to Undo Task Change
        [HttpPost("undo-change")]
        public async Task<ResultDTO> UndoTaskChange()
        {
            var result = await _taskService.UndoTaskChangeAsync();
            return result;
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportReports([FromQuery] ExportFilterDTO filters, [FromQuery] string format = "csv")
        {
            try
            {
                var result = await _taskService.ExportReportsAsync(filters, format);

                if (!result.IsSuccess)
                {
                    return BadRequest(new { result.Message, result.StatusCode });
                }

                var fileContent = (byte[])result.Data;
                var fileName = $"TasksReport_{DateTime.UtcNow:yyyyMMddHHmmss}.{format}";

                return File(fileContent, "application/octet-stream", fileName);
            }
            catch (NotSupportedException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("getalltaskstatuses")]
        public async Task<ResultDTO> GetAllTaskStatus()
        {
            return await _taskService.GetAllTaskStatus();
        }

        [HttpGet("getalltaskpriorities")]
        public async Task<ResultDTO> GetAllTaskPriorities()
        {
            return await _taskService.GetAllTaskPriorities();
        }
    }
}
