using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml;
using System.Text;

namespace OMT.DataService.Service
{
    public class TaskService : ITaskService
    {
        private readonly OMTDataContext _dbContext;

        public TaskService(OMTDataContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Centralized validation method
        private async Task<bool> ValidateEntityAsync<T>(DbSet<T> dbSet, int id) where T : class
        {
            return await dbSet.AnyAsync(entity => EF.Property<int>(entity, "Id") == id);
        }

        private async Task<string> ValidateTaskDataAsync(TaskDTO taskDTO)
        {
            if (!await ValidateEntityAsync(_dbContext.TasksStatus, taskDTO.Status))
                return "Invalid Task Status.";

            if (!await ValidateEntityAsync(_dbContext.TaskPriority, taskDTO.Priority))
                return "Invalid Task Priority.";

            if (await _dbContext.Tasks.AnyAsync(t => t.TopicProposal == taskDTO.TopicProposal))
                return "Topic Proposal already exists.";

            if (await _dbContext.Tasks.AnyAsync(t => t.TaskDescription == taskDTO.Description))
                return "Task Description already exists.";

            return string.Empty;
        }

        public async Task<ResultDTO> AddTaskAsync(TaskDTO taskDTO)
        {
            var resultDTO = new ResultDTO { IsSuccess = true, StatusCode = "200" };

            try
            {
                var validationError = await ValidateTaskDataAsync(taskDTO);
                if (!string.IsNullOrEmpty(validationError))
                {
                    return new ResultDTO
                    {
                        IsSuccess = false,
                        StatusCode = "400",
                        Message = validationError
                    };
                }

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

        public async Task<ResultDTO> GetTaskListAsync(TaskFilterDTO filters)
        {
            var resultDTO = new ResultDTO { IsSuccess = true, StatusCode = "200" };

            try
            {
                var query = _dbContext.Tasks.AsQueryable();

                // Apply filters
                query = filters.Status.HasValue ? query.Where(t => t.TaskStatus == filters.Status.Value) : query;
                query = filters.PrimaryContact.HasValue ? query.Where(t => t.PrimaryContact == filters.PrimaryContact.Value) : query;
                query = filters.RequestedBy.HasValue ? query.Where(t => t.RequestedBy == filters.RequestedBy.Value) : query;
                query = filters.StartDate.HasValue ? query.Where(t => t.StartDate >= filters.StartDate.Value) : query;
                query = filters.Priority.HasValue ? query.Where(t => t.TaskPriority == filters.Priority.Value) : query;
                query = filters.StartDate.HasValue && filters.TargetClosureDate.HasValue
                    ? query.Where(t => t.StartDate >= filters.StartDate.Value && t.TargetClosureDate <= filters.TargetClosureDate.Value)
                    : query;

                // Pagination
                var totalRecords = await query.CountAsync();
                var tasks = await query.OrderBy(t => t.Id)
                                       .Skip((filters.PageNumber - 1) * filters.PageSize)
                                       .Take(filters.PageSize)
                                       .Select(t => new TaskWithHistoryDTO
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
                                           History = new List<TaskHistoryDTO>()
                                       })
                                       .ToListAsync();

                // Fetching task history
                foreach (var task in tasks)
                {
                    task.History = await _dbContext.TaskHistory
                        .Where(h => h.TaskId == task.Id)
                        .OrderByDescending(h => h.UpdatedAt)
                        .Select(h => new TaskHistoryDTO
                        {
                            Action = $"{TaskService.GetTaskAliasFieldname(h.FieldName)} changed from {h.OldValue} to {h.NewValue}",
                            UpdatedBy = h.UpdatedBy,
                            UpdatedAt = h.UpdatedAt
                        })
                        .ToListAsync();
                }

                resultDTO.Data = new
                {
                    Tasks = tasks,
                    Pagination = new
                    {
                        CurrentPage = filters.PageNumber,
                        PageSize = filters.PageSize,
                        TotalRecords = totalRecords,
                        TotalPages = (int)Math.Ceiling((double)totalRecords / filters.PageSize)
                    }
                };
                resultDTO.Message = tasks.Any() ? "Tasks fetched successfully." : "No tasks found.";
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
                    return new ResultDTO
                    {
                        IsSuccess = false,
                        StatusCode = "404",
                        Message = "Task not found."
                    };
                }

                if (updateTaskDTO.FieldName == "TaskStatus" &&
                    !await ValidateEntityAsync(_dbContext.TasksStatus, int.Parse(updateTaskDTO.NewValue)))
                {
                    return new ResultDTO
                    {
                        IsSuccess = false,
                        StatusCode = "400",
                        Message = "Invalid Task Status."
                    };
                }

                if (updateTaskDTO.FieldName == "TaskPriority" &&
                    !await ValidateEntityAsync(_dbContext.TaskPriority, int.Parse(updateTaskDTO.NewValue)))
                {
                    return new ResultDTO
                    {
                        IsSuccess = false,
                        StatusCode = "400",
                        Message = "Invalid Task Priority."
                    };
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

                await _dbContext.TaskHistory.AddAsync(taskHistory);
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
                    if (batchUpdateTaskDTO.FieldName == "TaskStatus" &&
                        !await ValidateEntityAsync(_dbContext.TasksStatus, int.Parse(batchUpdateTaskDTO.NewValue)))
                    {
                        return new ResultDTO
                        {
                            IsSuccess = false,
                            StatusCode = "400",
                            Message = "Invalid Task Status."
                        };
                    }

                    if (batchUpdateTaskDTO.FieldName == "TaskPriority" &&
                        !await ValidateEntityAsync(_dbContext.TaskPriority, int.Parse(batchUpdateTaskDTO.NewValue)))
                    {
                        return new ResultDTO
                        {
                            IsSuccess = false,
                            StatusCode = "400",
                            Message = "Invalid Task Priority."
                        };
                    }

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

                    await _dbContext.TaskHistory.AddAsync(taskHistory);
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
                var lastHistory = await _dbContext.TaskHistory
                    .OrderByDescending(h => h.UpdatedAt)
                    .FirstOrDefaultAsync();

                if (lastHistory == null || (DateTime.UtcNow - lastHistory.UpdatedAt).TotalMinutes > 5)
                {
                    return new ResultDTO
                    {
                        IsSuccess = false,
                        StatusCode = "400",
                        Message = "No changes to undo or the last change exceeds the allowed time frame."
                    };
                }

                var task = await _dbContext.Tasks.FindAsync(lastHistory.TaskId);
                if (task == null)
                {
                    return new ResultDTO
                    {
                        IsSuccess = false,
                        StatusCode = "404",
                        Message = "Task not found."
                    };
                }

                SetTaskFieldValue(task, lastHistory.FieldName, lastHistory.OldValue);
                _dbContext.Tasks.Update(task);
                _dbContext.TaskHistory.Remove(lastHistory);
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

        public async Task<ResultDTO> ExportReportsAsync(ExportFilterDTO filters, string format)
        {
            var resultDTO = new ResultDTO { IsSuccess = true, StatusCode = "200" };

            try
            {
                // Validate inputs
                if (filters == null)
                {
                    return new ResultDTO
                    {
                        IsSuccess = false,
                        StatusCode = "400",
                        Message = "Filters cannot be null."
                    };
                }

                if (string.IsNullOrWhiteSpace(format) || !(format.Equals("csv", StringComparison.OrdinalIgnoreCase) ||
                                                          format.Equals("excel", StringComparison.OrdinalIgnoreCase)))
                {
                    return new ResultDTO
                    {
                        IsSuccess = false,
                        StatusCode = "400",
                        Message = "Invalid format. Supported formats are 'csv' and 'excel'."
                    };
                }

                var query = _dbContext.Tasks.AsQueryable();

                // Apply filters
                if (filters.Status.HasValue)
                {
                    if (!await ValidateEntityAsync(_dbContext.TasksStatus, filters.Status.Value))
                    {
                        return new ResultDTO
                        {
                            IsSuccess = false,
                            StatusCode = "400",
                            Message = "Invalid Task Status."
                        };
                    }
                    query = query.Where(t => t.TaskStatus == filters.Status);
                }

                if (filters.Priority.HasValue)
                {
                    if (!await ValidateEntityAsync(_dbContext.TaskPriority, filters.Priority.Value))
                    {
                        return new ResultDTO
                        {
                            IsSuccess = false,
                            StatusCode = "400",
                            Message = "Invalid Task Priority."
                        };
                    }
                    query = query.Where(t => t.TaskPriority == filters.Priority);
                }

                if (filters.StartDate.HasValue)
                    query = query.Where(t => t.StartDate >= filters.StartDate.Value);

                if (filters.TargetClosureDate.HasValue)
                    query = query.Where(t => t.TargetClosureDate <= filters.TargetClosureDate.Value);

                var tasks = await (from t in query
                                   join primaryContact in _dbContext.UserProfile on t.PrimaryContact equals primaryContact.UserId into pcGroup
                                   from primaryContact in pcGroup.DefaultIfEmpty()
                                   join requestedBy in _dbContext.UserProfile on t.RequestedBy equals requestedBy.UserId into rbGroup
                                   from requestedBy in rbGroup.DefaultIfEmpty()
                                   join approvalFrom in _dbContext.UserProfile on t.ApprovalFrom equals approvalFrom.UserId into afGroup
                                   from approvalFrom in afGroup.DefaultIfEmpty()
                                   join createdBy in _dbContext.UserProfile on t.CreatedBy equals createdBy.UserId into cbGroup
                                   from createdBy in cbGroup.DefaultIfEmpty()
                                   join taskStatus in _dbContext.TasksStatus on t.TaskStatus equals taskStatus.Id into tsGroup
                                   from taskStatus in tsGroup.DefaultIfEmpty()
                                   join taskPriority in _dbContext.TaskPriority on t.TaskPriority equals taskPriority.Id into tpGroup
                                   from taskPriority in tpGroup.DefaultIfEmpty()
                                   select new
                                   {
                                       t.Id,
                                       t.TopicProposal,
                                       t.TaskDescription,
                                       Status = taskStatus.Status ?? "N/A",
                                       Priority = taskPriority.Priority ?? "N/A",
                                       t.StartDate,
                                       t.TargetClosureDate,
                                       t.Remarks,
                                       PrimaryContact = primaryContact != null ? $"{primaryContact.FirstName} {primaryContact.LastName}" : "N/A",
                                       RequestedBy = requestedBy != null ? $"{requestedBy.FirstName} {requestedBy.LastName}" : "N/A",
                                       ApprovalFrom = approvalFrom != null ? $"{approvalFrom.FirstName} {approvalFrom.LastName}" : "N/A",
                                       CreatedBy = createdBy != null ? $"{createdBy.FirstName} {createdBy.LastName}" : "N/A"
                                   }).ToListAsync();

                // Generate file based on format
                byte[] fileContent = format.ToLower() switch
                {
                    "csv" => GenerateCSV(tasks),
                    "excel" => GenerateExcel(tasks),
                    _ => throw new NotSupportedException("Unsupported format.")
                };

                resultDTO.Message = "Report generated successfully.";
                resultDTO.Data = fileContent;
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = $"An error occurred: {ex.Message}";
            }

            return resultDTO;
        }


        private byte[] GenerateCSV(IEnumerable<object> data)
        {
            if (data == null || !data.Any())
                throw new ArgumentException("No data provided to generate CSV.");

            var stringBuilder = new StringBuilder();

            // Get the properties of the first object to create the header row
            var properties = data.First().GetType().GetProperties();
            stringBuilder.AppendLine(string.Join(",", properties.Select(p => p.Name)));

            // Iterate over the data to create rows
            foreach (var item in data)
            {
                var values = properties.Select(p => p.GetValue(item)?.ToString() ?? string.Empty);
                stringBuilder.AppendLine(string.Join(",", values.Select(v => $"\"{v}\""))); // Escape values for CSV
            }

            return Encoding.UTF8.GetBytes(stringBuilder.ToString());
        }

        private byte[] GenerateExcel(IEnumerable<object> data)
        {
            if (data == null || !data.Any())
                throw new ArgumentException("No data provided to generate Excel.");

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Sheet1");

            // Get the properties of the first object to create the header row
            var properties = data.First().GetType().GetProperties();

            // Add header row
            for (int i = 0; i < properties.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = properties[i].Name;
            }

            // Add data rows
            int rowIndex = 2;
            foreach (var item in data)
            {
                for (int colIndex = 0; colIndex < properties.Length; colIndex++)
                {
                    worksheet.Cells[rowIndex, colIndex + 1].Value = properties[colIndex].GetValue(item)?.ToString() ?? string.Empty;
                }
                rowIndex++;
            }

            // Auto-fit columns for better readability
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            return package.GetAsByteArray();
        }

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

        // Helper method to get the alias name of a task field dynamically
        private static string GetTaskAliasFieldname(string fieldName)
        {
            return fieldName switch
            {
                "TopicProposal" => "The Topic or Proposal had ",
                "TaskDescription" => "The Task Description had ",
                "StartDate" => "The Start Date had ",
                "TargetClosureDate" => "The Target Closure Date had ",
                "TaskStatus" => "The Task Status had ",
                "PrimaryContact" => "The Task Primary Contact had ",
                "RequestedBy" => "The Task Requested Name had ",
                "ApprovalFrom" => "The Task Approver had ",
                "Remarks" => "The Remarks had ",
                "TaskPriority" => "The Task Priority had ",
                _ => string.Empty
            };
        }

    }
}
