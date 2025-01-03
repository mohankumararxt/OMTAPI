using OMT.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataService.Interface
{
    public interface ITaskService
    {
        Task<ResultDTO> AddTaskAsync(TaskDTO taskDTO);
        Task<ResultDTO> UpdateTaskAsync(int taskId, UpdateTaskDTO updateTaskDTO);
        Task<ResultDTO> GetTaskListAsync(TaskFilterDTO filters);
        Task<ResultDTO> BatchUpdateTasksAsync(BatchUpdateTaskDTO batchUpdateTaskDTO);
        Task<ResultDTO> UndoTaskChangeAsync();
        Task<ResultDTO> ExportReportsAsync(ExportFilterDTO filters, string format);

        Task<ResultDTO> GetAllTaskStatus();
        Task<ResultDTO> GetAllTaskPriorities();
    }
}
