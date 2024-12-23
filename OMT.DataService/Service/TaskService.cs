using Microsoft.EntityFrameworkCore;
using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OMT.DataService.Service
{
    public class TaskService : ITaskService
    {
        private readonly OMTDataContext _dbContext;

        public TaskService(OMTDataContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ResultDTO> AddTaskAsync(TaskDTO taskDTO)
        {
            var resultDTO = new ResultDTO { IsSuccess = true, StatusCode = "200" };

            try
            {
                var task = new Tasks
                {
                    TopicProposal = taskDTO.TopicProposal,
                    TaskDescription = taskDTO.Description,
                    StartDate = taskDTO.StartDate,
                    TargetClosureDate = taskDTO.TargetClosureDate,
                    TaskStatus = taskDTO.Status,
                    PrimaryContact = taskDTO.PrimaryContact,
                    RequestedBy = taskDTO.RequestedBy,
                    ApprovalFrom = taskDTO.ApprovalFrom,
                    Remarks = taskDTO.Remarks,
                    TaskPriority = taskDTO.Priority,
                    CreatedBy = taskDTO.CreatedBy,
                    CreatedAt = DateTime.UtcNow
                };

                await _dbContext.Tasks.AddAsync(task);
                await _dbContext.SaveChangesAsync();

                resultDTO.Message = "Task added successfully.";
                resultDTO.Data = task.Id;
                resultDTO.StatusCode = "201";
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = $"An error occurred: {ex.Message}";
            }

            return resultDTO;
        }

        public async Task<ResultDTO> UpdateTaskAsync(int taskId, UpdateTaskDTO updateTaskDTO)
        {
            var resultDTO = new ResultDTO { IsSuccess = true, StatusCode = "200" };

            try
            {
                var task = await _dbContext.Tasks.FindAsync(taskId);
                if (task == null)
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Task not found.";
                    resultDTO.StatusCode = "404";
                    return resultDTO;
                }

                var oldValue = GetTaskFieldValue(task, updateTaskDTO.FieldName);
                SetTaskFieldValue(task, updateTaskDTO.FieldName, updateTaskDTO.NewValue);

                var taskHistory = new TaskHistory
                {
                    TaskId = taskId,
                    FieldName = updateTaskDTO.FieldName,
                    OldValue = oldValue,
                    NewValue = updateTaskDTO.NewValue,
                    UpdatedBy = updateTaskDTO.UpdatedBy,
                    UpdatedAt = DateTime.UtcNow
                };

                await _dbContext.TaskHistories.AddAsync(taskHistory);
                await _dbContext.SaveChangesAsync();

                resultDTO.Message = "Task updated successfully.";
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = $"An error occurred: {ex.Message}";
            }

            return resultDTO;
        }

        public async Task<ResultDTO> GetTaskListAsync(TaskFilterDTO filters)
        {
            var resultDTO = new ResultDTO { IsSuccess = true, StatusCode = "200" };

            try
            {
                var query = _dbContext.Tasks.AsQueryable();

                // Applying filters
                if (filters.Status.HasValue)
                    query = query.Where(t => t.TaskStatus == filters.Status);

                if (filters.PrimaryContact.HasValue)
                    query = query.Where(t => t.PrimaryContact == filters.PrimaryContact);

                if (filters.RequestedBy.HasValue)
                    query = query.Where(t => t.RequestedBy == filters.RequestedBy);

                if (filters.StartDate.HasValue)
                    query = query.Where(t => t.StartDate >= filters.StartDate.Value);

                if (filters.Priority.HasValue)
                    query = query.Where(t => t.TaskPriority == filters.Priority);

                if (filters.StartDate.HasValue && filters.TargetClosureDate.HasValue)
                    query = query.Where(t => t.StartDate >= filters.StartDate.Value && t.TargetClosureDate <= filters.TargetClosureDate.Value);

                // Fetching tasks
                var tasks = await query.Select(t => new TaskWithHistoryDTO
                {
                    Id = t.Id,
                    TopicProposal = t.TopicProposal,
                    Description = t.TaskDescription,
                    Status = t.TaskStatus,
                    PrimaryContact = t.PrimaryContact,
                    RequestedBy = t.RequestedBy,
                    TargetClosureDate = t.TargetClosureDate,
                    Priority = t.TaskPriority,
                    Remarks = t.Remarks,
                    History = new List<TaskHistoryDTO>()  // Initialize empty history list
                }).ToListAsync();

                // Fetching task history after retrieving tasks
                foreach (var task in tasks)
                {
                    task.History = await _dbContext.TaskHistories
                        .Where(h => h.TaskId == task.Id)
                        .Select(h => new TaskHistoryDTO
                        {
                            Action = $"{h.FieldName} changed from {h.OldValue} to {h.NewValue}",
                            UpdatedBy = h.UpdatedBy,
                            UpdatedAt = h.UpdatedAt
                        }).ToListAsync();
                }

                resultDTO.Message = tasks.Any() ? "Tasks fetched successfully." : "No tasks found.";
                resultDTO.Data = tasks;
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = $"An error occurred: {ex.Message}";
            }

            return resultDTO;
        }




        

        public async Task<ResultDTO> BatchUpdateTasksAsync(BatchUpdateTaskDTO batchUpdateTaskDTO)
        {
            var resultDTO = new ResultDTO { IsSuccess = true, StatusCode = "200" };

            try
            {
                var tasks = await _dbContext.Tasks
                    .Where(t => batchUpdateTaskDTO.TaskIDs.Contains(t.Id))
                    .ToListAsync();

                foreach (var task in tasks)
                {
                    var oldValue = GetTaskFieldValue(task, batchUpdateTaskDTO.FieldName);
                    SetTaskFieldValue(task, batchUpdateTaskDTO.FieldName, batchUpdateTaskDTO.NewValue);

                    var taskHistory = new TaskHistory
                    {
                        TaskId = task.Id,
                        FieldName = batchUpdateTaskDTO.FieldName,
                        OldValue = oldValue,
                        NewValue = batchUpdateTaskDTO.NewValue,
                        UpdatedBy = batchUpdateTaskDTO.UpdatedBy,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _dbContext.Tasks.AddAsync(task);
                    await _dbContext.TaskHistories.AddAsync(taskHistory);
                }

                await _dbContext.SaveChangesAsync();

                resultDTO.Message = "Tasks updated successfully.";
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = $"An error occurred: {ex.Message}";
            }

            return resultDTO;
        }

        public async Task<ResultDTO> UndoTaskChangeAsync()
        {
            var resultDTO = new ResultDTO { IsSuccess = true, StatusCode = "200" };

            try
            {
                var lastHistory = await _dbContext.TaskHistories
                    .OrderByDescending(h => h.UpdatedAt)
                    .FirstOrDefaultAsync();

                if (lastHistory == null || (DateTime.UtcNow - lastHistory.UpdatedAt).TotalMinutes > 5)
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "No changes to undo or the last change exceeds the allowed time frame.";
                    resultDTO.StatusCode = "400";
                    return resultDTO;
                }

                var task = await _dbContext.Tasks.FindAsync(lastHistory.Task.Id);
                if (task == null)
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Task not found.";
                    resultDTO.StatusCode = "404";
                    return resultDTO;
                }

                SetTaskFieldValue(task, lastHistory.FieldName, lastHistory.OldValue);

                _dbContext.TaskHistories.Remove(lastHistory);
                await _dbContext.SaveChangesAsync();

                resultDTO.Message = $"The last change to TaskID {task.Id} has been undone.";
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = $"An error occurred: {ex.Message}";
            }

            return resultDTO;
        }

        // Helper method to get the value of a task field dynamically
        private string GetTaskFieldValue(Tasks task, string fieldName)
        {
            return fieldName switch
            {
                "TopicProposal" => task.TopicProposal,
                "TaskDescription" => task.TaskDescription,
                "StartDate" => task.StartDate.ToString(),
                "TargetClosureDate" => task.TargetClosureDate.ToString(),
                "TaskStatus" => task.TaskStatus.ToString(),
                "PrimaryContact" => task.PrimaryContact.ToString(),
                "RequestedBy" => task.RequestedBy.ToString(),
                "ApprovalFrom" => task.ApprovalFrom.ToString(),
                "Remarks" => task.Remarks,
                "TaskPriority" => task.TaskPriority.ToString(),
                _ => string.Empty
            };
        }

        // Helper method to set the value of a task field dynamically
        private void SetTaskFieldValue(Tasks task, string fieldName, string newValue)
        {
            switch (fieldName)
            {
                case "TopicProposal":
                    task.TopicProposal = newValue;
                    break;
                case "TaskDescription":
                    task.TaskDescription = newValue;
                    break;
                case "StartDate":
                    task.StartDate = DateTime.Parse(newValue);
                    break;
                case "TargetClosureDate":
                    task.TargetClosureDate = DateTime.Parse(newValue);
                    break;
                case "TaskStatus":
                    task.TaskStatus = int.Parse(newValue);
                    break;
                case "PrimaryContact":
                    task.PrimaryContact = int.Parse(newValue);
                    break;
                case "RequestedBy":
                    task.RequestedBy = int.Parse(newValue);
                    break;
                case "ApprovalFrom":
                    task.ApprovalFrom = int.Parse(newValue);
                    break;
                case "Remarks":
                    task.Remarks = newValue;
                    break;
                case "TaskPriority":
                    task.TaskPriority = int.Parse(newValue);
                    break;
                default:
                    break;
            }
        }
    }
}
