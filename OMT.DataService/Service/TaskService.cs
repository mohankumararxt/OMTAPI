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
        private async Task<bool> ValidateEntityUserAsync<T>(DbSet<T> dbSet, int id) where T : class
        {
            return await dbSet.AnyAsync(entity => EF.Property<int>(entity, "UserId") == id);
        }

        private async Task<string> ValidateTaskDataAsync(TaskDTO taskDTO)
        {
            // Validate Task Status
            if (!await ValidateEntityAsync(_dbContext.TasksStatus, taskDTO.Status))
                return "Invalid Task Status.";

            // Validate Task Priority
            if (!await ValidateEntityAsync(_dbContext.TaskPriority, taskDTO.Priority))
                return "Invalid Task Priority.";

            // Validate Topic Proposal uniqueness
            if (await _dbContext.Tasks.AnyAsync(t => t.TopicProposal == taskDTO.TopicProposal))
                return "Topic Proposal already exists.";

            // Validate Task Description uniqueness
            if (await _dbContext.Tasks.AnyAsync(t => t.TaskDescription == taskDTO.Description))
                return "Task Description already exists.";

            // Validate if Topic Proposal is empty
            if (string.IsNullOrEmpty(taskDTO.TopicProposal))
                return "Topic Proposal cannot be empty.";

            // Validate if Task Description is empty
            if (string.IsNullOrEmpty(taskDTO.Description))
                return "Task Description cannot be empty.";

            // Validate RequestedBy, ApprovalFrom, PrimaryContact, and CreatedBy
            if (!await ValidateEntityUserAsync(_dbContext.UserProfile, taskDTO.RequestedBy))
                return "Task Requested By is invalid.";
            if (!await ValidateEntityUserAsync(_dbContext.UserProfile, taskDTO.ApprovalFrom))
                return "Task Approval is invalid.";
            if (!await ValidateEntityUserAsync(_dbContext.UserProfile, taskDTO.PrimaryContact))
                return "Task Primary Contact is invalid.";
            if (!await ValidateEntityUserAsync(_dbContext.UserProfile, taskDTO.CreatedBy))
                return "Task Created By is invalid.";

            // Validate Start Date (for DateTime objects)
            if (taskDTO.StartDate == default(DateTime))
                return "Task Start Date is invalid.";

            // Validate Target Closure Date (for DateTime objects)
            if (taskDTO.TargetClosureDate == default(DateTime))
                return "Task Target Closure Date is invalid.";

            // Ensure Start Date is earlier than Target Closure Date
            if (taskDTO.StartDate > taskDTO.TargetClosureDate)
                return "Task Start Date cannot be later than Target Closure Date.";

            if (taskDTO.Status != 1)
            {
                return "The Task Status must be set to 'Not Started' to proceed. Please update the status accordingly.";
            }

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
                    if (validationError == "Topic Proposal already exists." || validationError == "Task Description already exists.")
                    {
                        resultDTO.IsSuccess = false;
                        resultDTO.Message = validationError;
                        resultDTO.StatusCode = "409";
                        return resultDTO;
                    }
                    else
                    {
                        return new ResultDTO
                        {
                            IsSuccess = false,
                            StatusCode = "400",
                            Message = validationError
                        };
                    }
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

                resultDTO.IsSuccess = true;
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
                // Apply filters
                if (filters.Status.HasValue)
                {
                    if (!await ValidateEntityAsync(_dbContext.TasksStatus, filters.Status ?? 0))
                    {
                        return new ResultDTO
                        {
                            IsSuccess = false,
                            StatusCode = "400",
                            Message = "Invalid Task Status."
                        };
                    }
                }

                if (filters.PrimaryContact.HasValue)
                {
                    if (!await ValidateEntityUserAsync(_dbContext.UserProfile, filters.PrimaryContact ?? 0))
                    {
                        return new ResultDTO
                        {
                            IsSuccess = false,
                            StatusCode = "400",
                            Message = "Invalid Task Primary Contact."
                        };
                    }
                }

                if (filters.RequestedBy.HasValue)
                {
                    if (!await ValidateEntityUserAsync(_dbContext.UserProfile, filters.RequestedBy ?? 0))
                    {
                        return new ResultDTO
                        {
                            IsSuccess = false,
                            StatusCode = "400",
                            Message = "Invalid Task Requested By."
                        };
                    }
                }

                if (filters.Priority.HasValue)
                {
                    if (!await ValidateEntityAsync(_dbContext.TaskPriority, filters.Priority ?? 0))
                    {
                        return new ResultDTO
                        {
                            IsSuccess = false,
                            StatusCode = "400",
                            Message = "Invalid Task Priority."
                        };
                    }
                }

                // Validate Start Date
                if (filters.StartDate.HasValue)
                {
                    if (!DateTime.TryParse(filters.StartDate.ToString(), out _))
                    {
                        return new ResultDTO
                        {
                            IsSuccess = false,
                            StatusCode = "400",
                            Message = "Invalid Task Start Date."
                        };
                    }
                }

                // Validate Target Closure Date
                if (filters.TargetClosureDate.HasValue)
                {
                    if (!DateTime.TryParse(filters.TargetClosureDate.ToString(), out _))
                    {
                        return new ResultDTO
                        {
                            IsSuccess = false,
                            StatusCode = "400",
                            Message = "Invalid Task Target Closure Date."
                        };
                    }
                }
                if (filters.StartDate.HasValue && filters.TargetClosureDate.HasValue && filters.StartDate >= filters.TargetClosureDate)
                {
                    return new ResultDTO
                    {
                        IsSuccess = false,
                        StatusCode = "400",
                        Message = "Start Date cannot be later than Target Closure Date."
                    };
                }
                   
                // Build query to apply filters
                var query = _dbContext.Tasks.AsQueryable();

                query = filters.Status.HasValue ? query.Where(t => t.TaskStatus == filters.Status.Value) : query;
                query = filters.PrimaryContact.HasValue ? query.Where(t => t.PrimaryContact == filters.PrimaryContact.Value) : query;
                query = filters.RequestedBy.HasValue ? query.Where(t => t.RequestedBy == filters.RequestedBy.Value) : query;
                query = filters.StartDate.HasValue ? query.Where(t => t.StartDate >= filters.StartDate.Value) : query;
                query = filters.Priority.HasValue ? query.Where(t => t.TaskPriority == filters.Priority.Value) : query;
                query = filters.StartDate.HasValue && filters.TargetClosureDate.HasValue
                    ? query.Where(t => t.StartDate >= filters.StartDate.Value && t.TargetClosureDate <= filters.TargetClosureDate.Value)
                    : query;

                // Pagination logic
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

                // Fetching task history for each task
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

                // Field-specific validation
                if (updateTaskDTO.FieldName == "TaskStatus")
                {
                    if (!await ValidateEntityAsync(_dbContext.TasksStatus, int.Parse(updateTaskDTO.NewValue)))
                    {
                        return new ResultDTO
                        {
                            IsSuccess = false,
                            StatusCode = "400",
                            Message = "Invalid Task Status."
                        };
                    }
                }
                else if (updateTaskDTO.FieldName == "TaskPriority")
                {
                    if (!await ValidateEntityAsync(_dbContext.TaskPriority, int.Parse(updateTaskDTO.NewValue)))
                    {
                        return new ResultDTO
                        {
                            IsSuccess = false,
                            StatusCode = "400",
                            Message = "Invalid Task Priority."
                        };
                    }
                }
                else if (updateTaskDTO.FieldName == "TopicProposal" || updateTaskDTO.FieldName == "TaskDescription")
                {
                    if (string.IsNullOrEmpty(updateTaskDTO.NewValue))
                    {
                        return new ResultDTO
                        {
                            IsSuccess = false,
                            StatusCode = "400",
                            Message = $"{updateTaskDTO.FieldName} cannot be empty."
                        };
                    }
                }
                else if (updateTaskDTO.FieldName == "StartDate" || updateTaskDTO.FieldName == "TargetClosureDate")
                {
                    if (!DateTime.TryParse(updateTaskDTO.NewValue, out _))
                    {
                        return new ResultDTO
                        {
                            IsSuccess = false,
                            StatusCode = "400",
                            Message = $"Invalid {updateTaskDTO.FieldName} format."
                        };
                    }

                    if (updateTaskDTO.FieldName == "StartDate" &&
                        DateTime.TryParse(updateTaskDTO.NewValue, out var startDate) &&
                        startDate > task.TargetClosureDate)
                    {
                        return new ResultDTO
                        {
                            IsSuccess = false,
                            StatusCode = "400",
                            Message = "Start Date cannot be later than Target Closure Date."
                        };
                    }

                    if (updateTaskDTO.FieldName == "TargetClosureDate" &&
                        DateTime.TryParse(updateTaskDTO.NewValue, out var targetClosureDate) &&
                        targetClosureDate < task.StartDate)
                    {
                        return new ResultDTO
                        {
                            IsSuccess = false,
                            StatusCode = "400",
                            Message = "Target Closure Date cannot be earlier than Start Date."
                        };
                    }
                }
                else if (updateTaskDTO.FieldName == "PrimaryContact" || updateTaskDTO.FieldName == "RequestedBy" || updateTaskDTO.FieldName == "ApprovalFrom" || updateTaskDTO.FieldName == "CreatedBy")
                {
                    if (!await ValidateEntityUserAsync(_dbContext.UserProfile, int.Parse(updateTaskDTO.NewValue)))
                    {
                        return new ResultDTO
                        {
                            IsSuccess = false,
                            StatusCode = "400",
                            Message = $"Invalid {updateTaskDTO.FieldName}."
                        };
                    }
                }
                else
                {
                    return new ResultDTO
                    {
                        IsSuccess = false,
                        StatusCode = "400",
                        Message = "Invalid Task Field Name."
                    };
                }

                // Get the old value of the field to record the change
                var oldValue = GetTaskFieldValue(task, updateTaskDTO.FieldName);
                if (string.IsNullOrEmpty(oldValue))
                {
                    return new ResultDTO
                    {
                        IsSuccess = false,
                        StatusCode = "400",
                        Message = "Invalid Task Field Name."
                    };
                }

                // Update the field value
                SetTaskFieldValue(task, updateTaskDTO.FieldName, updateTaskDTO.NewValue);

                // Log the task history
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
                    // Validate Task Status field
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

                    // Validate Task Priority field
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

                    // Validate Start Date field
                    if (batchUpdateTaskDTO.FieldName == "StartDate")
                    {
                        if (!DateTime.TryParse(batchUpdateTaskDTO.NewValue, out var startDate))
                        {
                            return new ResultDTO
                            {
                                IsSuccess = false,
                                StatusCode = "400",
                                Message = "Invalid Task Start Date."
                            };
                        }

                        // Ensure StartDate is before TargetClosureDate if both are being updated
                        if ( startDate > task.TargetClosureDate)
                        {
                            return new ResultDTO
                            {
                                IsSuccess = false,
                                StatusCode = "400",
                                Message = "Start Date cannot be later than Target Closure Date."
                            };
                        }
                    }

                    // Validate Target Closure Date field
                    if (batchUpdateTaskDTO.FieldName == "TargetClosureDate")
                    {
                        if (!DateTime.TryParse(batchUpdateTaskDTO.NewValue, out var targetClosureDate))
                        {
                            return new ResultDTO
                            {
                                IsSuccess = false,
                                StatusCode = "400",
                                Message = "Invalid Task Target Closure Date."
                            };
                        }

                        // Ensure TargetClosureDate is after StartDate if both are being updated
                        if ( targetClosureDate < task.StartDate)
                        {
                            return new ResultDTO
                            {
                                IsSuccess = false,
                                StatusCode = "400",
                                Message = "Target Closure Date cannot be earlier than Start Date."
                            };
                        }
                    }

                    // Validate other fields like PrimaryContact, RequestedBy, ApprovalFrom, CreatedBy
                    if (batchUpdateTaskDTO.FieldName == "PrimaryContact" ||
                        batchUpdateTaskDTO.FieldName == "RequestedBy" ||
                        batchUpdateTaskDTO.FieldName == "ApprovalFrom" ||
                        batchUpdateTaskDTO.FieldName == "CreatedBy")
                    {
                        if (!await ValidateEntityUserAsync(_dbContext.UserProfile, int.Parse(batchUpdateTaskDTO.NewValue)))
                        {
                            return new ResultDTO
                            {
                                IsSuccess = false,
                                StatusCode = "400",
                                Message = $"Invalid {batchUpdateTaskDTO.FieldName}."
                            };
                        }
                    }

                    // Get the old value of the field to record the change
                    var oldValue = GetTaskFieldValue(task, batchUpdateTaskDTO.FieldName);
                    SetTaskFieldValue(task, batchUpdateTaskDTO.FieldName, batchUpdateTaskDTO.NewValue);

                    // Log the task history
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
               

                // Validate format
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

                // Validate and apply Status filter
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

                // Validate and apply Priority filter
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

                // Validate and apply StartDate filter
                if (filters.StartDate.HasValue)
                {
                    if (filters.StartDate.Value > DateTime.UtcNow)
                    {
                        return new ResultDTO
                        {
                            IsSuccess = false,
                            StatusCode = "400",
                            Message = "Start Date cannot be in the future."
                        };
                    }
                    query = query.Where(t => t.StartDate >= filters.StartDate.Value);
                }

                // Validate and apply TargetClosureDate filter
                if (filters.TargetClosureDate.HasValue)
                {
                    if (filters.TargetClosureDate.Value > DateTime.UtcNow)
                    {
                        return new ResultDTO
                        {
                            IsSuccess = false,
                            StatusCode = "400",
                            Message = "Target Closure Date cannot be in the future."
                        };
                    }
                    query = query.Where(t => t.TargetClosureDate <= filters.TargetClosureDate.Value);
                }

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
