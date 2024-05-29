using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DTO;
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace OMT.DataService.Service
{
    public class TemplateService : ITemplateService
    {
        private readonly OMTDataContext _oMTDataContext;
        public TemplateService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }
        public ResultDTO CreateTemplate(CreateTemplateDTO createTemplateDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            //using (var dbContextTransaction = _oMTDataContext.Database.BeginTransaction())
            //{
            try
            {
                SkillSet skillSet = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == createTemplateDTO.SkillsetId && x.SystemofRecordId == createTemplateDTO.SystemofRecordId).FirstOrDefault();
                if (skillSet != null)
                {
                    TemplateColumns template = _oMTDataContext.TemplateColumns.Where(x => x.SystemOfRecordId == createTemplateDTO.SystemofRecordId && x.SkillSetId == createTemplateDTO.SkillsetId).FirstOrDefault();
                    if (template != null)
                    {
                        resultDTO.IsSuccess = false;
                        resultDTO.Message = "Template Name already exists.";
                    }
                    else
                    {
                        if (createTemplateDTO.TemplateColumns.Any())
                        {
                            foreach (TemplateColumnDTO templateColumns in createTemplateDTO.TemplateColumns)
                            {
                                TemplateColumns newtemplateColumns = new TemplateColumns()
                                {
                                    SkillSetId = createTemplateDTO.SkillsetId,
                                    SystemOfRecordId = createTemplateDTO.SystemofRecordId,
                                    ColumnAliasName = templateColumns.ColumnName.Replace(" ", "_"),
                                    ColumnName = templateColumns.ColumnName,
                                    ColumnDataType = templateColumns.ColumnDataType,
                                    IsDuplicateCheck = templateColumns.IsDuplicateCheck,
                                    IsGetOrderColumn = templateColumns.IsGetOrderColumn,
                                };
                                _oMTDataContext.TemplateColumns.Add(newtemplateColumns);
                            }
                            _oMTDataContext.SaveChanges();
                        }

                        string? connectionstring = _oMTDataContext.Database.GetConnectionString();

                        using SqlConnection connection = new(connectionstring);
                        using SqlCommand command = new()
                        {
                            Connection = connection,
                            CommandType = CommandType.StoredProcedure,
                            CommandText = "CreateTemplate"
                        };
                        command.Parameters.AddWithValue("@SkillsetId", createTemplateDTO.SkillsetId);
                        command.Parameters.AddWithValue("@SystemofRecordId", createTemplateDTO.SystemofRecordId);
                        SqlParameter returnValue = new()
                        {
                            ParameterName = "@RETURN_VALUE",
                            Direction = ParameterDirection.ReturnValue
                        };
                        command.Parameters.Add(returnValue);
                        connection.Open();
                        command.ExecuteNonQuery();

                        int returnCode = (int)command.Parameters["@RETURN_VALUE"].Value;
                        resultDTO.Message = "Template Added Successfully.";
                        if (returnCode != 1)
                        {
                            throw new InvalidOperationException("Error encountered while creating template,please try again");
                        }
                        // Commit transaction
                        // dbContextTransaction.Commit();

                    }
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.StatusCode = "404";
                    resultDTO.Message = "Skillset not exists.";
                }
            }
            catch (Exception ex)
            {
                // Rollback transaction
                // dbContextTransaction.Rollback();
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            //}
            return resultDTO;
        }
        public ResultDTO DeleteTemplate(int SkillSetId)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                SkillSet skillSet = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == SkillSetId && x.IsActive).FirstOrDefault();
                if (skillSet != null)
                {
                    List<TemplateColumns> columns = _oMTDataContext.TemplateColumns.Where(x => x.SkillSetId == skillSet.SkillSetId).ToList();
                    _oMTDataContext.TemplateColumns.RemoveRange(columns);
                    _oMTDataContext.SaveChanges();

                    string? connectionstring = _oMTDataContext.Database.GetConnectionString();

                    using SqlConnection connection = new(connectionstring);
                    using SqlCommand command = new()
                    {
                        Connection = connection,
                        CommandType = CommandType.Text,
                        CommandText = "Drop table " + skillSet.SkillSetName
                    };
                    connection.Open();
                    command.ExecuteNonQuery();
                    resultDTO.Message = "Template Deleted Successfully";
                }
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }
        public ResultDTO UploadOrders(UploadTemplateDTO uploadTemplateDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                SkillSet skillSet = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == uploadTemplateDTO.SkillsetId && x.IsActive).FirstOrDefault();
                if (skillSet != null)
                {
                    TemplateColumns template = _oMTDataContext.TemplateColumns.Where(x => x.SkillSetId == uploadTemplateDTO.SkillsetId).FirstOrDefault();
                    if (template != null)
                    {
                        string? connectionstring = _oMTDataContext.Database.GetConnectionString();

                        using SqlConnection connection = new(connectionstring);
                        using SqlCommand command = new()
                        {
                            Connection = connection,
                            CommandType = CommandType.StoredProcedure,
                            CommandText = "InsertData"
                        };
                        command.Parameters.AddWithValue("@SkillSetId", uploadTemplateDTO.SkillsetId);
                        command.Parameters.AddWithValue("@jsonData", uploadTemplateDTO.JsonData);

                        SqlParameter returnValue = new()
                        {
                            ParameterName = "@RETURN_VALUE",
                            Direction = ParameterDirection.ReturnValue
                        };
                        command.Parameters.Add(returnValue);

                        connection.Open();
                        command.ExecuteNonQuery();

                        int returnCode = (int)command.Parameters["@RETURN_VALUE"].Value;

                        if (returnCode != 1)
                        {
                            throw new InvalidOperationException("Something went wrong while uploading the orders,please check the order details.");
                        }
                       
                        resultDTO.IsSuccess = true;
                        resultDTO.Message = "Order uploaded successfully";
                    }
                    else
                    {
                        resultDTO.IsSuccess = false;
                        resultDTO.StatusCode = "404";
                        resultDTO.Message = "Template doesnt exist for the given skillsetid,please create one to upload";
                    }
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.StatusCode = "404";
                    resultDTO.Message = "Skillset does not exist.";
                }
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }
        public ResultDTO ValidateOrders(ValidateOrderDTO validateorderDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                SkillSet? skillSet = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == validateorderDTO.SkillsetId && x.IsActive).FirstOrDefault();
                if (skillSet != null)
                {
                    List<TemplateColumns> template = _oMTDataContext.TemplateColumns.Where(x => x.SkillSetId == validateorderDTO.SkillsetId).ToList();
                    if (template.Count > 0)
                    {
                        string tablename = skillSet.SkillSetName;
                        
                        List<string> listofcolumns1 = _oMTDataContext.DefaultTemplateColumns.Where(x => x.SystemOfRecordId == skillSet.SystemofRecordId && x.IsDuplicateCheck).Select(_ => _.DefaultColumnName).ToList();
                        List<string> listofColumns = template.Where(x => x.IsDuplicateCheck).Select(_ => _.ColumnAliasName).ToList();

                        List<string> combinedList = listofcolumns1.Concat(listofColumns).ToList();

                        //parse json data
                        JObject jsondata = JObject.Parse(validateorderDTO.JsonData);
                        JArray recordsarray = jsondata.Value<JArray>("Records");

                        //sql query
                        string isDuplicateColumns1 = listofcolumns1 != null ? string.Join(",", listofcolumns1) : "";
                        string isDuplicateColumns = listofColumns != null ? string.Join(",", listofColumns) : "";

                        // Combine the strings, ensuring that if any of them is null, it's selected without a comma
                        string combinedString = (isDuplicateColumns1 != "" && isDuplicateColumns != "") ? isDuplicateColumns1 + "," + isDuplicateColumns : isDuplicateColumns1 + isDuplicateColumns;

                        string sql = $"SELECT CASE WHEN t.UserId IS NULL THEN '' ELSE CONCAT(up.FirstName, ' ', up.LastName, '(',up.EmployeeId, ')') END AS UserName, {combinedString}, " +
                                     $"ISNULL(ps.Status, '') AS Status, " +
                                     $"ISNULL(CONVERT(VARCHAR(10), t.CompletionDate, 120), '') AS CompletionDate, " +
                                     $"ISNULL(CONVERT(VARCHAR(19), t.StartTime, 120), '') AS StartTime, " +
                                     $"ISNULL(CONVERT(VARCHAR(19), t.EndTime, 120), '') AS EndTime " +
                                     $"FROM {tablename} t " +
                                     $"LEFT JOIN UserProfile up ON t.UserId = up.UserId " +
                                     $"LEFT JOIN ProcessStatus ps ON t.Status = ps.Id AND t.SystemOfRecordId = ps.SystemOfRecordId WHERE ";

                        foreach (JObject records in recordsarray)
                        {
                            string query = "(";
                            foreach (string columnname in combinedList)
                            {
                                string columndata = records.Value<string>(columnname);

                                query += $"[{columnname}] = '{columndata}' AND ";
                            }

                            query = query.Substring(0, query.Length - 5);
                            query += ") OR ";

                            sql += query;
                        }

                        sql = sql.Substring(0, sql.Length - 4);

                       //execute sql query to fetch records from table
                        string? connectionstring = _oMTDataContext.Database.GetConnectionString();

                        using SqlConnection connection = new(connectionstring);
                        using SqlDataAdapter dataAdapter = new SqlDataAdapter(sql, connection);

                        DataSet dataset = new DataSet();
                        connection.Open();

                        dataAdapter.Fill(dataset);

                        DataTable datatable = dataset.Tables[0];

                        //query dt to get records
                        var querydt = datatable.AsEnumerable()
                                      .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                                          column => column.ColumnName,
                                          column => row[column])).ToList();

                        if (querydt.Count > 0)
                        {
                            ValidationResponseDTO validationResponseDTO = new ValidationResponseDTO()
                            {
                                DuplicateCheckColumns = combinedString,
                                DuplicateOrders = querydt
                            };
                            resultDTO.IsSuccess = true;
                            resultDTO.Data = validationResponseDTO;
                            resultDTO.Message = "Duplicate records found,please verify before uploading";
                        }
                        else
                        {
                            resultDTO.IsSuccess = true;
                            resultDTO.Message = "No dulpicate records found, proceed to upload";
                        }
                    }
                    else
                    {
                        resultDTO.IsSuccess = false;
                        resultDTO.StatusCode = "404";
                        resultDTO.Message = "Template doesn't exist for the given skillsetid,please create one to upload";
                    }
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.StatusCode = "404";
                    resultDTO.Message = "Skillset does not exist.";
                }

            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }
        public ResultDTO GetTemplateList()
        {

            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                List<TemplateListDTO> templateList = new List<TemplateListDTO>();

                List<SkillSet> skillSets = (from ss in _oMTDataContext.SkillSet
                                            join tc in _oMTDataContext.TemplateColumns on ss.SkillSetId equals tc.SkillSetId
                                            where ss.IsActive == true
                                            select ss).Distinct().OrderBy(ss => ss.SkillSetName).ToList();

                List<TemplateColumns> templatecolumns = _oMTDataContext.TemplateColumns.ToList();

                if(skillSets.Count > 0 ) { 
                    foreach(SkillSet skillset in skillSets)
                    {
                        TemplateListDTO templateListDTO = new TemplateListDTO();
                        templateListDTO.SkillsetId = skillset.SkillSetId;
                        templateListDTO.SkillSetName = skillset.SkillSetName;
                        templateListDTO.SystemofRecordId = skillset.SystemofRecordId;
                        List<TemplateColumnDTO> TemplateColumns = templatecolumns.Where(x => x.SkillSetId == skillset.SkillSetId).
                                                            Select(_ => new TemplateColumnDTO() { ColumnDataType = _.ColumnDataType, ColumnName = _.ColumnName, IsDuplicateCheck = _.IsDuplicateCheck }).ToList();

                        List<DefaultTemplateColumnlistDTO> defaultTemplateColumns = _oMTDataContext.DefaultTemplateColumns
                                                                                    .Where(x => x.SystemOfRecordId == skillset.SystemofRecordId && x.IsDefOnlyColumn)
                                                                                    .Select(_ => new DefaultTemplateColumnlistDTO()
                                                                                    {
                                                                                        DataType = _.DefaultColumnName == "IsPriority" ? "bit" : _.DataType,
                                                                                        DefaultColumnName = _.DefaultColumnName,
                                                                                        IsDuplicateCheck = _.IsDuplicateCheck
                                                                                    }).ToList();
                       
                        var finalcolumns = TemplateColumns.
                                            Concat(defaultTemplateColumns.
                                            Select(dc => new TemplateColumnDTO
                                            {
                                                ColumnDataType =dc.DataType,
                                                ColumnName = dc.DefaultColumnName,
                                                IsDuplicateCheck = dc.IsDuplicateCheck
                                            })).ToList();

                        templateListDTO.TemplateColumns = finalcolumns;
                        templateList.Add(templateListDTO);
                    }
                    resultDTO.Data = templateList;
                }

            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }

        public ResultDTO GetOrders(int userid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                string? connectionstring = _oMTDataContext.Database.GetConnectionString();

                using SqlConnection connection = new(connectionstring);
                connection.Open();

                //check if user has uncompleted orders in all of his skillsets. if any is there- dont assign orders,say- first complete pending orders
                List<string> tablenames = (from us in _oMTDataContext.UserSkillSet
                                           join ss in _oMTDataContext.SkillSet on us.SkillSetId equals ss.SkillSetId
                                           where us.UserId == userid && us.IsActive
                                           && _oMTDataContext.TemplateColumns.Any(temp => temp.SkillSetId == ss.SkillSetId)
                                           select ss.SkillSetName).ToList();

                List<Dictionary<string, object>> noStatusRecords = new List<Dictionary<string, object>>();

                foreach (string tablename in tablenames)
                {
                    string query = $"select * from {tablename} where UserId = @UserId and (Status IS NULL OR Status = '')";

                    using SqlCommand command = connection.CreateCommand();
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@UserId", userid);

                    using SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                    DataSet dataset = new DataSet();

                    dataAdapter.Fill(dataset);

                    DataTable datatable = dataset.Tables[0];

                    var querydt1 = datatable.AsEnumerable()
                                    .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                                        column => column.ColumnName,
                                        column => row[column])).ToList();

                    noStatusRecords.AddRange(querydt1);

                }

                if (noStatusRecords.Count > 0)
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "You have pending order status update,can't get fresh orders until you have updated the status for all the orders";
                    resultDTO.StatusCode = "404";
                }

                else
                {
                    List<UserSkillSet> userskillsetlist = _oMTDataContext.UserSkillSet.Where(x => x.UserId == userid && x.IsActive).ToList();
                    List<UserSkillSet> hardstateid = userskillsetlist.Where(x => x.IsHardStateUser && x.IsPrimary).ToList();
                    if (hardstateid.Count > 0)
                    {
                        string uporder;
                        using SqlCommand command = new()
                        {
                            Connection = connection,
                            CommandType = CommandType.StoredProcedure,
                            CommandText = "GetOrderByHardstate"
                        };
                        command.Parameters.AddWithValue("@userid", userid);
                        //output param to get the record
                        SqlParameter outputParam = new SqlParameter("@updatedrecord", SqlDbType.NVarChar, -1);
                        outputParam.Direction = ParameterDirection.Output;
                        command.Parameters.Add(outputParam);

                        SqlParameter returnValue = new()
                        {
                            ParameterName = "@RETURN_VALUE",
                            Direction = ParameterDirection.ReturnValue
                        };
                        command.Parameters.Add(returnValue);
                        command.ExecuteNonQuery();

                        int returnCode = (int)command.Parameters["@RETURN_VALUE"].Value;

                        if (returnCode != 1)
                        {
                            throw new InvalidOperationException("Stored Procedure call failed.");
                        }

                        uporder = command.Parameters["@updatedrecord"].Value.ToString();
                        if (!string.IsNullOrWhiteSpace(uporder))
                        {
                            resultDTO.Data = uporder;
                            resultDTO.IsSuccess = true;
                            resultDTO.Message = "Order assigned successfully";
                        }
                        else
                        {
                            callSPbyPrimary(userid, resultDTO, connection);
                        }
                    }
                    else
                    {
                        callSPbyPrimary(userid, resultDTO, connection);

                    }
                }
             
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }

        private static void callSPbyPrimary(int userid, ResultDTO resultDTO, SqlConnection connection)
        {
            string updatedOrder;
            SqlCommand command1 = new()
            {
                Connection = connection,
                CommandType = CommandType.StoredProcedure,
                CommandText = "GetOrderByIsPRorPerc"
            };
            command1.Parameters.AddWithValue("@userid", userid);
            //output param to get the record
            SqlParameter outputParam = new SqlParameter("@updatedrecord", SqlDbType.NVarChar, -1);
            outputParam.Direction = ParameterDirection.Output;
            command1.Parameters.Add(outputParam);

            SqlParameter returnValue = new()
            {
                ParameterName = "@RETURN_VALUE",
                Direction = ParameterDirection.ReturnValue
            };
            command1.Parameters.Add(returnValue);


            command1.ExecuteNonQuery();

            int returnCode = (int)command1.Parameters["@RETURN_VALUE"].Value;

            if (returnCode != 1)
            {
                throw new InvalidOperationException("Stored Procedure call failed.");
            }

            updatedOrder = command1.Parameters["@updatedrecord"].Value.ToString();
            if (string.IsNullOrWhiteSpace(updatedOrder))
            {
                resultDTO.Data = "";
                resultDTO.StatusCode = "404";
                resultDTO.IsSuccess = false;
                resultDTO.Message = "No more oders for now, please come back again";
            }
            else
            {
                resultDTO.Data = updatedOrder;
                resultDTO.IsSuccess = true;
                resultDTO.Message = "Order assigned successfully";
            }

            //return updatedOrder;
        }

        public ResultDTO UpdateOrderStatus(UpdateOrderStatusDTO updateOrderStatusDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "201" };
            try
            {
                SkillSet? skillset = _oMTDataContext.SkillSet.Where(x =>x.SkillSetId == updateOrderStatusDTO.SkillSetId && x.IsActive).FirstOrDefault();
              
                string sql1 = $"UPDATE {skillset.SkillSetName} SET Status = @Status, Remarks = @Remarks, CompletionDate = @CompletionDate, EndTime = @EndTime WHERE Id = @ID";

                 DateTime dateTime = DateTime.Now;
               
                string? connectionstring = _oMTDataContext.Database.GetConnectionString();
                using SqlConnection connection = new(connectionstring);
                connection.Open();

               using (SqlCommand command = connection.CreateCommand())
               { 
                    command.CommandText = sql1;
                    command.Parameters.AddWithValue("@Status", updateOrderStatusDTO.StatusId);
                    command.Parameters.AddWithValue("@Remarks", updateOrderStatusDTO.Remarks);
                    command.Parameters.AddWithValue("@Id", updateOrderStatusDTO.Id);
                    command.Parameters.AddWithValue("@EndTime", dateTime);
                    command.Parameters.AddWithValue("@CompletionDate", dateTime);
                    command.ExecuteNonQuery();
               }
               resultDTO.Message = "Order status has been updated successfully";
               resultDTO.IsSuccess = true;
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }

        public ResultDTO AgentCompletedOrders(AgentCompletedOrdersDTO agentCompletedOrdersDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                string? connectionstring = _oMTDataContext.Database.GetConnectionString();

                using SqlConnection connection = new(connectionstring);
                connection.Open();

                if (agentCompletedOrdersDTO.SkillSetId == null )
                {
                    List<string> tablenames = (from us in _oMTDataContext.UserSkillSet 
                                               join ss in _oMTDataContext.SkillSet on us.SkillSetId equals ss.SkillSetId
                                               where us.UserId == agentCompletedOrdersDTO.UserId && us.IsActive
                                               && _oMTDataContext.TemplateColumns.Any(temp => temp.SkillSetId == ss.SkillSetId)
                                               select ss.SkillSetName).ToList();

                    List<Dictionary<string, object>> allCompletedRecords = new List<Dictionary<string, object>>();
                    foreach(string tablename in tablenames)
                    {
                       
                        string sqlquery = $"SELECT t.OrderId,ss.SkillSetName as skillset, ps.Status as Status,t.Remarks, " +
                                          $"CONVERT(VARCHAR(19), t.StartTime, 120) as StartTime, " +
                                          $"CONVERT(VARCHAR(19), t.EndTime, 120) as EndTime, " +
                                          $"CONVERT(VARCHAR(10), t.CompletionDate, 120) as CompletionDate, " +
                                          $"CONCAT((DATEDIFF(SECOND, t.StartTime, t.EndTime) / 3600), ':', " +
                                          $"((DATEDIFF(SECOND, t.StartTime, t.EndTime) / 60) % 60), ':', " +
                                          $"(DATEDIFF(SECOND, t.StartTime, t.EndTime) % 60)) as TimeTaken " +
                                          $"FROM {tablename} t " +
                                          $"INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId " +
                                          $"INNER JOIN ProcessStatus ps on ps.Id = t.Status " +
                                          $"WHERE UserId = @userid AND t.Status IS NOT NULL AND t.Status <> '' AND CONVERT(DATE, CompletionDate) BETWEEN @FromDate AND @ToDate";

                        using SqlCommand command = connection.CreateCommand();
                        command.CommandText = sqlquery ;
                        command.Parameters.AddWithValue("@userid", agentCompletedOrdersDTO.UserId);
                        command.Parameters.AddWithValue("@FromDate", agentCompletedOrdersDTO.FromDate);
                        command.Parameters.AddWithValue("@ToDate", agentCompletedOrdersDTO.ToDate);

                        using SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                        DataSet dataset = new DataSet();
                        
                        dataAdapter.Fill(dataset);

                        DataTable datatable = dataset.Tables[0];

                        //query dt to get records
                        var querydt1 = datatable.AsEnumerable()
                                      .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                                          column => column.ColumnName,
                                          column => row[column])).ToList();

                        allCompletedRecords.AddRange(querydt1);

                    }
                    if (allCompletedRecords.Count > 0)
                    {
                        resultDTO.Data = allCompletedRecords;
                        resultDTO.Message = "Completed orders fetched successfully";
                        resultDTO.IsSuccess = true;
                    }
                    else
                    {
                        resultDTO.IsSuccess = false;
                        resultDTO.Message = "Completed orders not found";
                        resultDTO.StatusCode = "404";
                    }
                }
                else
                {
                   
                    SkillSet? skillSet = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == agentCompletedOrdersDTO.SkillSetId).FirstOrDefault();

                    string sql = $"SELECT t.OrderId,ss.SkillSetName as skillset, ps.Status as Status,t.Remarks, " +
                                 $"CONVERT(VARCHAR(19), t.StartTime, 120) as StartTime, " +
                                 $"CONVERT(VARCHAR(19), t.EndTime, 120) as EndTime, " +
                                 $"CONVERT(VARCHAR(10), t.CompletionDate, 120) as CompletionDate, " +
                                 $"CONCAT((DATEDIFF(SECOND, t.StartTime, t.EndTime) / 3600), ':', " +
                                 $"((DATEDIFF(SECOND, t.StartTime, t.EndTime) / 60) % 60), ':', " +
                                 $"(DATEDIFF(SECOND, t.StartTime, t.EndTime) % 60)) as TimeTaken " +
                                 $"FROM {skillSet.SkillSetName} t " +
                                 $"INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId " +
                                 $"INNER JOIN ProcessStatus ps on ps.Id = t.Status " +
                                 $"WHERE UserId = @userid AND t.Status IS NOT NULL AND t.Status <> '' AND CONVERT(DATE, CompletionDate) BETWEEN @FromDate AND @ToDate";


                    using SqlCommand sqlCommand = connection.CreateCommand();
                    sqlCommand.CommandText = sql;
                    sqlCommand.Parameters.AddWithValue("@userid",agentCompletedOrdersDTO.UserId);
                    sqlCommand.Parameters.AddWithValue("@FromDate", agentCompletedOrdersDTO.FromDate);
                    sqlCommand.Parameters.AddWithValue("@ToDate", agentCompletedOrdersDTO.ToDate);

                    using SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlCommand);

                    DataSet dataset = new DataSet();
                    
                    dataAdapter.Fill(dataset);

                    DataTable datatable = dataset.Tables[0];

                    //query dt to get records
                    var querydt2 = datatable.AsEnumerable()
                                  .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                                      column => column.ColumnName,
                                      column => row[column])).ToList();
                    if (querydt2.Count > 0)
                    {
                        resultDTO.IsSuccess = true;
                        resultDTO.Data = querydt2;
                        resultDTO.Message = "Completed orders fetched successfully";
                    }
                    else
                    {
                        resultDTO.IsSuccess = false;
                        resultDTO.Message = "Completed orders not found";
                        resultDTO.StatusCode = "404";
                    }

                }
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }

        public ResultDTO TeamCompletedOrders(TeamCompletedOrdersDTO teamCompletedOrdersDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
              
                string? connectionstring = _oMTDataContext.Database.GetConnectionString();

                using SqlConnection connection = new(connectionstring);
                connection.Open();

                if(teamCompletedOrdersDTO.SkillSetId != null)
                {
                    
                    SkillSet? skillSet = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == teamCompletedOrdersDTO.SkillSetId).FirstOrDefault();

                    string sql1 = $"SELECT CONCAT(up.FirstName, ' ', up.LastName) as UserName,t.OrderId,ss.SkillSetName as SkillSet,ps.Status as Status,t.Remarks, " +
                                  $"CONVERT(VARCHAR(19), t.StartTime, 120) as StartTime, " +
                                  $"CONVERT(VARCHAR(19), t.EndTime, 120) as EndTime, " +
                                  $"CONVERT(VARCHAR(10), t.CompletionDate, 120) as CompletionDate, " +
                                  $"CONCAT((DATEDIFF(SECOND, t.StartTime, t.EndTime) / 3600), ':', " +
                                  $"((DATEDIFF(SECOND, t.StartTime, t.EndTime) / 60) % 60), ':', " +
                                  $"(DATEDIFF(SECOND, t.StartTime, t.EndTime) % 60)) as TimeTaken " +
                                  $"FROM {skillSet.SkillSetName} t " +
                                  $"INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId " +
                                  $"INNER JOIN ProcessStatus ps on ps.Id = t.Status " +
                                  $"INNER JOIN UserProfile up on up.UserId = t.UserId " +
                                  $"WHERE TeamLeadId = @Teamid AND t.Status IS NOT NULL AND t.Status <> '' AND CONVERT(DATE, CompletionDate) BETWEEN @FromDate AND @ToDate";


                    using SqlCommand sqlCommand = connection.CreateCommand();
                    sqlCommand.CommandText = sql1;
                    sqlCommand.Parameters.AddWithValue("@Teamid", teamCompletedOrdersDTO.TeamId);
                    sqlCommand.Parameters.AddWithValue("@FromDate", teamCompletedOrdersDTO.FromDate);
                    sqlCommand.Parameters.AddWithValue("@ToDate", teamCompletedOrdersDTO.ToDate);

                    using SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlCommand);

                    DataSet dataset = new DataSet();

                    dataAdapter.Fill(dataset);

                    DataTable datatable = dataset.Tables[0];

                    //query dt to get records
                    var querydt2 = datatable.AsEnumerable()
                                  .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                                      column => column.ColumnName,
                                      column => row[column])).ToList();
                    if (querydt2.Count > 0)
                    {
                        resultDTO.IsSuccess = true;
                        resultDTO.Data = querydt2;
                        resultDTO.Message = "Completed orders of the team has been fetched successfully";
                    }
                    else
                    {
                        resultDTO.IsSuccess = false;
                        resultDTO.Message = "Completed orders not found";
                        resultDTO.StatusCode = "404";
                    }
                }
                else
                {
                    List<string> tablenames = (from ta in _oMTDataContext.TeamAssociation
                                               where ta.TeamId == teamCompletedOrdersDTO.TeamId 
                                               join us in _oMTDataContext.UserSkillSet on ta.UserId equals us.UserId
                                               join ss in _oMTDataContext.SkillSet on us.SkillSetId equals ss.SkillSetId
                                               where us.IsActive && _oMTDataContext.TemplateColumns.Any(temp => temp.SkillSetId == ss.SkillSetId)
                                               select ss.SkillSetName).Distinct().ToList();

                    List<Dictionary<string, object>> allCompletedRecords = new List<Dictionary<string, object>>();
                    foreach (string tablename in tablenames)
                    {

                        string sqlquery = $"SELECT CONCAT(up.FirstName, ' ', up.LastName) as UserName,t.OrderId,ss.SkillSetName as SkillSet,ps.Status as Status,t.Remarks, " +
                                          $"CONVERT(VARCHAR(19), t.StartTime, 120) as StartTime, " +
                                          $"CONVERT(VARCHAR(19), t.EndTime, 120) as EndTime, " +
                                          $"CONVERT(VARCHAR(10), t.CompletionDate, 120) as CompletionDate, " +
                                          $"CONCAT((DATEDIFF(SECOND, t.StartTime, t.EndTime) / 3600), ':', " +
                                          $"((DATEDIFF(SECOND, t.StartTime, t.EndTime) / 60) % 60), ':', " +
                                          $"(DATEDIFF(SECOND, t.StartTime, t.EndTime) % 60)) as TimeTaken " +
                                          $"FROM {tablename} t " +
                                          $"INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId " +
                                          $"INNER JOIN ProcessStatus ps on ps.Id = t.Status " +
                                          $"INNER JOIN UserProfile up on up.UserId = t.UserId " +
                                          $"WHERE TeamLeadId = @Teamid AND t.Status IS NOT NULL AND t.Status <> '' AND CONVERT(DATE, CompletionDate) BETWEEN @FromDate AND @ToDate";


                        using SqlCommand command = connection.CreateCommand();
                        command.CommandText = sqlquery;
                        command.Parameters.AddWithValue("@Teamid", teamCompletedOrdersDTO.TeamId);
                        command.Parameters.AddWithValue("@FromDate", teamCompletedOrdersDTO.FromDate);
                        command.Parameters.AddWithValue("@ToDate", teamCompletedOrdersDTO.ToDate);

                        using SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                        DataSet dataset = new DataSet();

                        dataAdapter.Fill(dataset);

                        DataTable datatable = dataset.Tables[0];

                        //query dt to get records
                        var querydt1 = datatable.AsEnumerable()
                                      .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                                          column => column.ColumnName,
                                          column => row[column])).ToList();

                        allCompletedRecords.AddRange(querydt1);

                    }
                    if (allCompletedRecords.Count > 0)
                    {
                        resultDTO.Data = allCompletedRecords;
                        resultDTO.Message = "Completed orders of the team has been fetched successfully";
                        resultDTO.IsSuccess = true;
                    }
                    else
                    {
                        resultDTO.IsSuccess = false;
                        resultDTO.Message = "Completed orders not found";
                        resultDTO.StatusCode = "404";
                    }

                }
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }

        public ResultDTO GetDefaultColumnNames(int systemofrecordid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                List<DefaultTemplateColumnsDTO> defaultTemplateColumns = _oMTDataContext.DefaultTemplateColumns.Where(x => x.SystemOfRecordId == systemofrecordid && x.IsDefOnlyColumn).Select(_ => new DefaultTemplateColumnsDTO() { DefaultColumnName = _.DefaultColumnName}).ToList();
               
                if (defaultTemplateColumns.Count > 0)
                {
                    resultDTO.Data = defaultTemplateColumns;
                    resultDTO.Message = "Default columns are successfully fetched";
                    resultDTO.IsSuccess = true;
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Default columns doesnt exist for the specified systemofrecordid";
                    resultDTO.StatusCode = "404";
                }
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }

        public ResultDTO GetPendingOrderDetails(int userid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                string? connectionstring = _oMTDataContext.Database.GetConnectionString();

                using SqlConnection connection = new(connectionstring);
                connection.Open();

                //check if user has uncompleted orders in all of his skillsets. if any is there- dont assign orders,say- first complete pending orders
               List<string> tablenames = (from us in _oMTDataContext.UserSkillSet
                                           join ss in _oMTDataContext.SkillSet on us.SkillSetId equals ss.SkillSetId
                                           where us.UserId == userid && us.IsActive 
                                           && _oMTDataContext.TemplateColumns.Any(temp => temp.SkillSetId == ss.SkillSetId)
                                           select ss.SkillSetName).ToList();
                
                List<Dictionary<string, object>> noStatusRecords = new List<Dictionary<string, object>>();

                foreach (string tablename in tablenames)
                {
                    
                    var columns1 = (from ss in _oMTDataContext.SkillSet
                                    join dt in _oMTDataContext.TemplateColumns on ss.SkillSetId equals dt.SkillSetId
                                    where ss.SkillSetName == tablename && dt.IsGetOrderColumn
                                    select dt.ColumnAliasName).ToList();

                    var columns2 = (from ss in _oMTDataContext.SkillSet 
                                   join dt in _oMTDataContext.DefaultTemplateColumns on ss.SystemofRecordId equals dt.SystemOfRecordId
                                   where ss.SkillSetName == tablename && dt.IsGetOrderColumn
                                   select dt.DefaultColumnName).ToList();

                    var columns = (columns1 ?? Enumerable.Empty<string>()).Concat(columns2 ?? Enumerable.Empty<string>());
                    string selectedColumns = string.Join(", ", columns.Select(c => $"t1.{c}"));

                    string query = $@"
                                    SELECT TOP 1 
                                        {selectedColumns},
                                        t2.SkillSetName AS SkillSetName, 
                                        t1.SkillSetId,
                                        t3.SystemOfRecordName AS SystemOfRecordName,
                                        t1.SystemOfRecordId,
                                        t1.Id
                                    FROM 
                                        [{tablename}] AS t1
                                    LEFT JOIN SkillSet AS t2 ON t1.SkillSetId = t2.SkillSetId
                                    LEFT JOIN SystemOfRecord AS t3 ON t1.SystemofRecordId = t3.SystemOfRecordId
                                    WHERE 
                                        UserId = @UserId 
                                        AND (Status IS NULL OR Status = '')";

                    using SqlCommand command = connection.CreateCommand();
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@UserId", userid);

                    using SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                    DataSet dataset = new DataSet();

                    dataAdapter.Fill(dataset);

                    DataTable datatable = dataset.Tables[0];

                    var querydt1 = datatable.AsEnumerable()
                                    .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                                        column => column.ColumnName,
                                        column => row[column])).ToList();

                    noStatusRecords.AddRange(querydt1);

                }

                if (noStatusRecords.Count > 0)
                {
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Please update the status of this order";
                    resultDTO.StatusCode = "200";
                    resultDTO.Data = noStatusRecords;
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.StatusCode = "404";
                    resultDTO.Message = "No more orders to update status";
                }

            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }

        public ResultDTO GetComplexOrdersDetails(ComplexOrdersRequestDTO complexOrdersRequestDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                string? connectionstring = _oMTDataContext.Database.GetConnectionString();

                using SqlConnection connection = new(connectionstring);
                connection.Open();

                List<Dictionary<string, object>> ComplexRecords = new List<Dictionary<string, object>>();

                if (complexOrdersRequestDTO.SkillsetId == null && complexOrdersRequestDTO.UserId == null)
                {
                    List<SkillSet> tablenames = (from ss in _oMTDataContext.SkillSet
                                                 join tc in _oMTDataContext.TemplateColumns on ss.SkillSetId equals tc.SkillSetId
                                                 join sor in _oMTDataContext.SystemofRecord on ss.SystemofRecordId equals sor.SystemofRecordId
                                                 where ss.IsActive == true
                                                 select new SkillSet
                                                 {
                                                     SkillSetName = ss.SkillSetName,
                                                     SystemofRecordId = ss.SystemofRecordId
                                                 }
                                            ).Distinct().ToList();

                    foreach (SkillSet tablename in tablenames)
                    {
                        string sqlquery = "";

                        if (tablename.SystemofRecordId == 1)
                        {
                            sqlquery = $@"SELECT t.Id,t.ProjectId,t.OrderId,CONCAT(up.FirstName, ' ', up.LastName,'(',up.Email,')') as UserName,t.UserId,ss.SkillSetName as skillset,sor.SystemofRecordName as SystemofRecord, ps.Status as Status,t.Remarks,
                                            CONVERT(VARCHAR(10), t.CompletionDate, 120) as CompletionDate
                                            FROM {tablename.SkillSetName} t 
                                            INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId
                                            INNER JOIN ProcessStatus ps on ps.Id = t.Status
                                            INNER JOIN SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                            INNER JOIN UserProfile up on up.UserId = t.UserId
                                            WHERE t.Status = 5";
                        }

                        else if (tablename.SystemofRecordId == 2)
                        {
                            sqlquery = $@"SELECT t.Id,t.OrderId,CONCAT(up.FirstName, ' ', up.LastName,'(',up.Email,')') as UserName,t.UserId,ss.SkillSetName as skillset,sor.SystemofRecordName as SystemofRecord, ps.Status as Status,t.Remarks,
                                            CONVERT(VARCHAR(10), t.CompletionDate, 120) as CompletionDate
                                            FROM {tablename.SkillSetName} t 
                                            INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId
                                            INNER JOIN ProcessStatus ps on ps.Id = t.Status
                                            INNER JOIN SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                            INNER JOIN UserProfile up on up.UserId = t.UserId
                                            WHERE t.Status = 9";

                        }

                            using SqlCommand command = connection.CreateCommand();
                            command.CommandText = sqlquery;

                            using SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                            DataSet dataset = new DataSet();

                            dataAdapter.Fill(dataset);

                            DataTable datatable = dataset.Tables[0];

                            //query dt to get records
                            var querydt1 = datatable.AsEnumerable()
                                          .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                                              column => column.ColumnName,
                                              column => row[column])).ToList();

                            ComplexRecords.AddRange(querydt1);
                        
                    }

                    if (ComplexRecords.Count > 0)
                    {
                        resultDTO.Data = ComplexRecords;
                        resultDTO.Message = "Complex orders fetched successfully";
                        resultDTO.IsSuccess = true;
                    }
                    else
                    {
                        resultDTO.IsSuccess = false;
                        resultDTO.Message = "Complex orders not found";
                        resultDTO.StatusCode = "404";
                    }
                }
                else if(complexOrdersRequestDTO.SkillsetId != null && complexOrdersRequestDTO.UserId == null)
                {
                    SkillSet skillset = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == complexOrdersRequestDTO.SkillsetId && x.IsActive).FirstOrDefault();

                    if (skillset != null)
                    {
                        string sqlquery ="";
                        if (skillset.SystemofRecordId == 1)
                        {
                            sqlquery = $@"SELECT t.Id,t.ProjectId,t.OrderId,CONCAT(up.FirstName, ' ', up.LastName,'(',up.Email,')') as UserName,t.UserId,ss.SkillSetName as skillset,sor.SystemofRecordName as SystemofRecord, ps.Status as Status,t.Remarks,
                                            CONVERT(VARCHAR(10), t.CompletionDate, 120) as CompletionDate
                                            FROM {skillset.SkillSetName} t 
                                            INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId
                                            INNER JOIN ProcessStatus ps on ps.Id = t.Status
                                            INNER JOIN SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                            INNER JOIN UserProfile up on up.UserId = t.UserId
                                            WHERE t.Status = 5";

                        }
                        else if (skillset.SystemofRecordId == 2)
                        {
                            sqlquery = $@"SELECT t.Id,t.OrderId,CONCAT(up.FirstName, ' ', up.LastName,'(',up.Email,')') as UserName,t.UserId,ss.SkillSetName as skillset,sor.SystemofRecordName as SystemofRecord, ps.Status as Status,t.Remarks,
                                            CONVERT(VARCHAR(10), t.CompletionDate, 120) as CompletionDate
                                            FROM {skillset.SkillSetName} t 
                                            INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId
                                            INNER JOIN ProcessStatus ps on ps.Id = t.Status
                                            INNER JOIN SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                            INNER JOIN UserProfile up on up.UserId = t.UserId
                                            WHERE t.Status = 9";
                        }


                            using SqlCommand command = connection.CreateCommand();
                            command.CommandText = sqlquery;

                            using SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                            DataSet dataset = new DataSet();

                            dataAdapter.Fill(dataset);

                            DataTable datatable = dataset.Tables[0];

                            //query dt to get records
                            var querydt1 = datatable.AsEnumerable()
                                          .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                                              column => column.ColumnName,
                                              column => row[column])).ToList();

                            ComplexRecords.AddRange(querydt1);
                        

                        if (ComplexRecords.Count > 0)
                        {
                            resultDTO.Data = ComplexRecords;
                            resultDTO.Message = "Complex orders fetched successfully";
                            resultDTO.IsSuccess = true;
                        }
                        else
                        {
                            resultDTO.IsSuccess = false;
                            resultDTO.Message = "Complex orders not found";
                            resultDTO.StatusCode = "404";
                        }
                    }
                    else
                    {
                        resultDTO.IsSuccess = false;
                        resultDTO.Message = "Skillset not found";
                        resultDTO.StatusCode = "404";
                    }
                }

                else if (complexOrdersRequestDTO.SkillsetId == null && complexOrdersRequestDTO.UserId != null)
                {
                    List<SkillSet> skillset2 = (from us in _oMTDataContext.UserSkillSet
                                                join ss in _oMTDataContext.SkillSet on us.SkillSetId equals ss.SkillSetId
                                                where us.UserId == complexOrdersRequestDTO.UserId && us.IsActive
                                                && _oMTDataContext.TemplateColumns.Any(temp => temp.SkillSetId == ss.SkillSetId)
                                                select new SkillSet
                                                {
                                                    SkillSetName = ss.SkillSetName,
                                                    SystemofRecordId = ss.SystemofRecordId
                                                }).Distinct().ToList();

                    foreach (SkillSet skillset in skillset2)
                    {
                        string sqlquery = "";
                        if (skillset.SystemofRecordId == 1)
                        {
                            sqlquery = $@"SELECT t.Id,t.ProjectId, t.OrderId, CONCAT(up.FirstName, ' ', up.LastName,'(',up.Email,')') as UserName,t.UserId,
                                        ss.SkillSetName as skillset, sor.SystemofRecordName as SystemofRecord, 
                                        ps.Status as Status, t.Remarks, CONVERT(VARCHAR(10), t.CompletionDate, 120) as CompletionDate
                                        FROM {skillset.SkillSetName} t 
                                        INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId
                                        INNER JOIN ProcessStatus ps on ps.Id = t.Status
                                        INNER JOIN SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                        INNER JOIN UserProfile up on up.UserId = t.UserId
                                        WHERE t.Status = 5 and t.UserId = @UserId";
                        }
                        else if (skillset.SystemofRecordId == 2)
                        {
                            sqlquery = $@"SELECT t.Id,t.OrderId, CONCAT(up.FirstName, ' ', up.LastName,'(',up.Email,')') as UserName,t.UserId,
                                        ss.SkillSetName as skillset, sor.SystemofRecordName as SystemofRecord, 
                                        ps.Status as Status, t.Remarks, CONVERT(VARCHAR(10), t.CompletionDate, 120) as CompletionDate
                                        FROM {skillset.SkillSetName} t 
                                        INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId
                                        INNER JOIN ProcessStatus ps on ps.Id = t.Status
                                        INNER JOIN SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                        INNER JOIN UserProfile up on up.UserId = t.UserId
                                        WHERE t.Status = 9 and t.UserId = @UserId";
                        }
                        
                        using SqlCommand command = connection.CreateCommand();
                        command.CommandText = sqlquery;
                        command.Parameters.AddWithValue("@UserId", complexOrdersRequestDTO.UserId);

                        using SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                        DataSet dataset = new DataSet();
                        dataAdapter.Fill(dataset);

                        DataTable datatable = dataset.Tables[0];

                        // Query dt to get records
                        var querydt1 = datatable.AsEnumerable()
                            .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                                column => column.ColumnName,
                                column => row[column]))
                            .ToList();

                        ComplexRecords.AddRange(querydt1);
                    }

                    if (ComplexRecords.Count > 0)
                    {
                        resultDTO.Data = ComplexRecords;
                        resultDTO.Message = "Complex orders fetched successfully";
                        resultDTO.IsSuccess = true;
                    }
                    else
                    {
                        resultDTO.IsSuccess = false;
                        resultDTO.Message = "Complex orders not found";
                        resultDTO.StatusCode = "404";
                    }
                }


                else if (complexOrdersRequestDTO.SkillsetId != null && complexOrdersRequestDTO.UserId != null)
                {
                    SkillSet? skillset = _oMTDataContext.SkillSet
                                        .Where(x => x.SkillSetId == complexOrdersRequestDTO.SkillsetId && x.IsActive)
                                        .FirstOrDefault();

                    if (skillset != null)
                    {
                        string sqlquery = "";

                        if (skillset.SystemofRecordId == 1)
                        {
                            sqlquery = $@"SELECT t.Id,t.ProjectId, t.OrderId, CONCAT(up.FirstName, ' ', up.LastName,'(',up.Email,')') as UserName,t.UserId, ss.SkillSetName as skillset, sor.SystemofRecordName as SystemofRecord, ps.Status as Status, t.Remarks,
                                        CONVERT(VARCHAR(10), t.CompletionDate, 120) as CompletionDate
                                        FROM {skillset.SkillSetName} t 
                                        INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId
                                        INNER JOIN ProcessStatus ps on ps.Id = t.Status
                                        INNER JOIN SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                        INNER JOIN UserProfile up on up.UserId = t.UserId
                                        WHERE t.Status = 5 AND t.UserId = @UserId";
                        }
                        else if (skillset.SystemofRecordId == 2)
                        {
                            sqlquery = $@"SELECT t.Id,t.OrderId, CONCAT(up.FirstName, ' ', up.LastName,'(',up.Email,')') as UserName,t.UserId, ss.SkillSetName as skillset, sor.SystemofRecordName as SystemofRecord, ps.Status as Status, t.Remarks,
                                         CONVERT(VARCHAR(10), t.CompletionDate, 120) as CompletionDate
                                         FROM {skillset.SkillSetName} t 
                                         INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId
                                         INNER JOIN ProcessStatus ps on ps.Id = t.Status
                                         INNER JOIN SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                         INNER JOIN UserProfile up on up.UserId = t.UserId
                                         WHERE t.Status = 9 AND t.UserId = @UserId";
                        }

                        using SqlCommand command = connection.CreateCommand();
                        command.CommandText = sqlquery;
                        command.Parameters.AddWithValue("@UserId", complexOrdersRequestDTO.UserId);

                        using SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                        DataSet dataset = new DataSet();
                        dataAdapter.Fill(dataset);

                        DataTable datatable = dataset.Tables[0];

                        // Query dt to get records
                        var querydt1 = datatable.AsEnumerable()
                            .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                                column => column.ColumnName,
                                column => row[column]))
                            .ToList();

                        ComplexRecords.AddRange(querydt1);

                        if (ComplexRecords.Count > 0)
                        {
                            resultDTO.Data = ComplexRecords;
                            resultDTO.Message = "Complex orders fetched successfully";
                            resultDTO.IsSuccess = true;
                        }
                        else
                        {
                            resultDTO.IsSuccess = false;
                            resultDTO.Message = "Complex orders not found";
                            resultDTO.StatusCode = "404";
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }

        public ResultDTO ReleaseOrder(ReleaseOrderDTO releaseOrderDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "201" };
            try
            {
                string? connectionstring = _oMTDataContext.Database.GetConnectionString();

                using SqlConnection connection = new(connectionstring);
                connection.Open();

                foreach (var order in releaseOrderDTO.Orders)
                {
                    if (order.TryGetValue("skillset", out var skillSetName))
                    {
                        // Fetch the corresponding SkillSet based on SkillSetName
                        SkillSet skillSet = _oMTDataContext.SkillSet.FirstOrDefault(x => x.SkillSetName == skillSetName.ToString());

                        if (skillSet != null)
                        {
                            if (order.TryGetValue("OrderId", out var orderId) && order.TryGetValue("UserId", out var userId) && order.TryGetValue("Remarks",out var remarks))
                            {
                                string sqlquery = $@"
                                UPDATE {skillSet.SkillSetName} 
                                SET UserId = NULL, 
                                Status = NULL, 
                                Remarks = NULL, 
                                CompletionDate = NULL, 
                                StartTime = NULL, 
                                EndTime = NULL, 
                                TeamLeadId = NULL, 
                                SkillSetId = NULL, 
                                SystemOfRecordId = NULL
                                WHERE OrderId = @OrderId and UserId = @UserId and Remarks = @Remarks";

                                using SqlCommand command = connection.CreateCommand();
                                command.CommandText = sqlquery;
                                command.Parameters.AddWithValue("@OrderId", orderId.ToString());
                                command.Parameters.AddWithValue("@UserId", userId.ToString());
                                command.Parameters.AddWithValue("@Remarks", remarks.ToString());

                                command.ExecuteNonQuery();
                                resultDTO.Message = "Order has been released back to queue successfully";
                                resultDTO.IsSuccess = true;
                            }
                            else
                            {
                                resultDTO.IsSuccess = false;
                                resultDTO.Message = "Something went wrong";
                                resultDTO.StatusCode = "404";
                            }
                                
                        }
                        else
                        {
                            resultDTO.IsSuccess = false;
                            resultDTO.Message = "Skillset not found";
                            resultDTO.StatusCode = "404";
                        }

                    }
                    else
                    {
                        throw new Exception("Skillset key not found in order.");
                    }
                }
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }

        public ResultDTO TimeExceededOrders(TimeExceededOrdersDTO timeExceededOrdersDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                string? connectionstring = _oMTDataContext.Database.GetConnectionString();

                using SqlConnection connection = new(connectionstring);
                connection.Open();

                List<Dictionary<string, object>> TExceededRecords = new List<Dictionary<string, object>>();

                if (timeExceededOrdersDTO.SkillsetId == null && timeExceededOrdersDTO.UserId == null)
                {
                    List<SkillSet> tablenames = (from ss in _oMTDataContext.SkillSet
                                                 join tc in _oMTDataContext.TemplateColumns on ss.SkillSetId equals tc.SkillSetId
                                                 join sor in _oMTDataContext.SystemofRecord on ss.SystemofRecordId equals sor.SystemofRecordId
                                                 where ss.IsActive == true
                                                 select new SkillSet
                                                 {
                                                     SkillSetName = ss.SkillSetName,
                                                     SystemofRecordId = ss.SystemofRecordId
                                                 }
                                           ).Distinct().ToList();

                    foreach (SkillSet tablename in tablenames)
                    {
                        string sqlquery = "";

                        if (tablename.SystemofRecordId == 1)
                        {
                            sqlquery = $@"SELECT t.Id,t.ProjectId,t.OrderId,CONCAT(up.FirstName, ' ', up.LastName,'(',up.Email,')') as UserName,t.UserId,ss.SkillSetName as skillset,sor.SystemofRecordName as SystemofRecord
                                            FROM {tablename.SkillSetName} t 
                                            INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId
                                            INNER JOIN SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                            INNER JOIN UserProfile up on up.UserId = t.UserId
                                            WHERE t.Status IS NULL and t.Completiondate IS NULL and DATEDIFF(minute, t.StartTime, GETDATE()) > 3";
                        }

                        else if (tablename.SystemofRecordId == 2)
                        {
                            sqlquery = $@"SELECT t.Id,t.OrderId,CONCAT(up.FirstName, ' ', up.LastName,'(',up.Email,')') as UserName,t.UserId,ss.SkillSetName as skillset,sor.SystemofRecordName as SystemofRecord
                                            FROM {tablename.SkillSetName} t 
                                            INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId
                                            INNER JOIN SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                            INNER JOIN UserProfile up on up.UserId = t.UserId
                                            WHERE t.Status IS NULL and t.Completiondate IS NULL and DATEDIFF(minute, t.StartTime, GETDATE()) > 3";

                        }
                        using SqlCommand command = connection.CreateCommand();
                        command.CommandText = sqlquery;

                        using SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                        DataSet dataset = new DataSet();

                        dataAdapter.Fill(dataset);

                        DataTable datatable = dataset.Tables[0];

                        //query dt to get records
                        var querydt1 = datatable.AsEnumerable()
                                      .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                                          column => column.ColumnName,
                                          column => row[column])).ToList();

                        TExceededRecords.AddRange(querydt1);

                    }

                    if (TExceededRecords.Count > 0)
                    {
                        resultDTO.Data = TExceededRecords;
                        resultDTO.Message = "Time Exceeded orders fetched successfully";
                        resultDTO.IsSuccess = true;
                    }
                    else
                    {
                        resultDTO.IsSuccess = false;
                        resultDTO.Message = "Time Exceeded orders not found";
                        resultDTO.StatusCode = "404";
                    }

                }

                else if (timeExceededOrdersDTO.SkillsetId == null && timeExceededOrdersDTO.UserId != null)
                {
                    List<SkillSet> skillset2 = (from us in _oMTDataContext.UserSkillSet
                                                join ss in _oMTDataContext.SkillSet on us.SkillSetId equals ss.SkillSetId
                                                where us.UserId == timeExceededOrdersDTO.UserId && us.IsActive
                                                && _oMTDataContext.TemplateColumns.Any(temp => temp.SkillSetId == ss.SkillSetId)
                                                select new SkillSet
                                                {
                                                    SkillSetName = ss.SkillSetName,
                                                    SystemofRecordId = ss.SystemofRecordId
                                                }).Distinct().ToList();

                    foreach (SkillSet skillset in skillset2)
                    {
                        string sqlquery = "";
                        if (skillset.SystemofRecordId == 1)
                        {
                            sqlquery = $@"SELECT t.Id,t.ProjectId, t.OrderId, CONCAT(up.FirstName, ' ', up.LastName,'(',up.Email,')') as UserName,t.UserId,
                                        ss.SkillSetName as skillset, sor.SystemofRecordName as SystemofRecord
                                        FROM {skillset.SkillSetName} t 
                                        INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId
                                        INNER JOIN SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                        INNER JOIN UserProfile up on up.UserId = t.UserId
                                        WHERE t.Status IS NULL and t.Completiondate IS NULL and DATEDIFF(minute, t.StartTime, GETDATE()) > 3 and t.UserId = @UserId";
                        }
                        else if (skillset.SystemofRecordId == 2)
                        {
                            sqlquery = $@"SELECT t.Id,t.OrderId, CONCAT(up.FirstName, ' ', up.LastName,'(',up.Email,')') as UserName,t.UserId,
                                        ss.SkillSetName as skillset, sor.SystemofRecordName as SystemofRecord
                                        FROM {skillset.SkillSetName} t 
                                        INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId
                                        INNER JOIN SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                        INNER JOIN UserProfile up on up.UserId = t.UserId
                                        WHERE t.Status IS NULL and t.Completiondate IS NULL and DATEDIFF(minute, t.StartTime, GETDATE()) > 3 and t.UserId = @UserId";
                        }
                        using SqlCommand command = connection.CreateCommand();
                        command.CommandText = sqlquery;
                        command.Parameters.AddWithValue("@UserId", timeExceededOrdersDTO.UserId);

                        using SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                        DataSet dataset = new DataSet();

                        dataAdapter.Fill(dataset);

                        DataTable datatable = dataset.Tables[0];

                        //query dt to get records
                        var querydt1 = datatable.AsEnumerable()
                                      .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                                          column => column.ColumnName,
                                          column => row[column])).ToList();

                        TExceededRecords.AddRange(querydt1);

                    }

                    if (TExceededRecords.Count > 0)
                    {
                        resultDTO.Data = TExceededRecords;
                        resultDTO.Message = "Time Exceeded orders fetched successfully";
                        resultDTO.IsSuccess = true;
                    }
                    else
                    {
                        resultDTO.IsSuccess = false;
                        resultDTO.Message = "Time Exceeded orders not found";
                        resultDTO.StatusCode = "404";
                    }


                }

                else if (timeExceededOrdersDTO.SkillsetId != null && timeExceededOrdersDTO.UserId == null)
                {
                    SkillSet? skillset = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == timeExceededOrdersDTO.SkillsetId && x.IsActive).FirstOrDefault();

                    if (skillset != null)
                    {
                        string sqlquery = "";
                        if (skillset.SystemofRecordId == 1)
                        {
                            sqlquery = $@"SELECT t.Id,t.ProjectId,t.OrderId,CONCAT(up.FirstName, ' ', up.LastName,'(',up.Email,')') as UserName,t.UserId,ss.SkillSetName as skillset,sor.SystemofRecordName as SystemofRecord
                                            FROM {skillset.SkillSetName} t 
                                            INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId
                                            INNER JOIN SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                            INNER JOIN UserProfile up on up.UserId = t.UserId
                                            WHERE t.Status IS NULL and t.Completiondate IS NULL and DATEDIFF(minute, t.StartTime, GETDATE()) > 3";

                        }
                        else if (skillset.SystemofRecordId == 2)
                        {
                            sqlquery = $@"SELECT t.Id,t.OrderId,CONCAT(up.FirstName, ' ', up.LastName,'(',up.Email,')') as UserName,t.UserId,ss.SkillSetName as skillset,sor.SystemofRecordName as SystemofRecord
                                            FROM {skillset.SkillSetName} t 
                                            INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId
                                            INNER JOIN SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                            INNER JOIN UserProfile up on up.UserId = t.UserId
                                            WHERE t.Status IS NULL and t.Completiondate IS NULL and DATEDIFF(minute, t.StartTime, GETDATE()) > 3";
                        }


                        using SqlCommand command = connection.CreateCommand();
                        command.CommandText = sqlquery;

                        using SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                        DataSet dataset = new DataSet();

                        dataAdapter.Fill(dataset);

                        DataTable datatable = dataset.Tables[0];

                        //query dt to get records
                        var querydt1 = datatable.AsEnumerable()
                                      .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                                          column => column.ColumnName,
                                          column => row[column])).ToList();

                        TExceededRecords.AddRange(querydt1);


                        if (TExceededRecords.Count > 0)
                        {
                            resultDTO.Data = TExceededRecords;
                            resultDTO.Message = "Time Exceeded orders fetched successfully";
                            resultDTO.IsSuccess = true;
                        }
                        else
                        {
                            resultDTO.IsSuccess = false;
                            resultDTO.Message = "Time Exceeded orders not found";
                            resultDTO.StatusCode = "404";
                        }
                    }
                }
                else if (timeExceededOrdersDTO.SkillsetId != null && timeExceededOrdersDTO.UserId != null)
                {
                    SkillSet? skillset = _oMTDataContext.SkillSet
                                        .Where(x => x.SkillSetId == timeExceededOrdersDTO.SkillsetId && x.IsActive)
                                        .FirstOrDefault();

                    if (skillset != null)
                    {
                        string sqlquery = "";

                        if (skillset.SystemofRecordId == 1)
                        {
                            sqlquery = $@"SELECT t.Id,t.ProjectId, t.OrderId, CONCAT(up.FirstName, ' ', up.LastName,'(',up.Email,')') as UserName,t.UserId, ss.SkillSetName as skillset, sor.SystemofRecordName as SystemofRecord
                                        FROM {skillset.SkillSetName} t 
                                        INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId
                                        INNER JOIN SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                        INNER JOIN UserProfile up on up.UserId = t.UserId
                                        WHERE t.Status IS NULL and t.Completiondate IS NULL and DATEDIFF(minute, t.StartTime, GETDATE()) > 3 AND t.UserId = @UserId";
                        }
                        else if (skillset.SystemofRecordId == 2)
                        {
                            sqlquery = $@"SELECT t.Id,t.OrderId, CONCAT(up.FirstName, ' ', up.LastName,'(',up.Email,')') as UserName,t.UserId, ss.SkillSetName as skillset, sor.SystemofRecordName as SystemofRecord
                                         FROM {skillset.SkillSetName} t 
                                         INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId
                                         INNER JOIN SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                         INNER JOIN UserProfile up on up.UserId = t.UserId
                                         WHERE t.Status IS NULL and t.Completiondate IS NULL and DATEDIFF(minute, t.StartTime, GETDATE()) > 3 AND t.UserId = @UserId";
                        }

                        using SqlCommand command = connection.CreateCommand();
                        command.CommandText = sqlquery;
                        command.Parameters.AddWithValue("@UserId", timeExceededOrdersDTO.UserId);

                        using SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                        DataSet dataset = new DataSet();
                        dataAdapter.Fill(dataset);

                        DataTable datatable = dataset.Tables[0];

                        // Query dt to get records
                        var querydt1 = datatable.AsEnumerable()
                            .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                                column => column.ColumnName,
                                column => row[column]))
                            .ToList();

                        TExceededRecords.AddRange(querydt1);

                        if (TExceededRecords.Count > 0)
                        {
                            resultDTO.Data = TExceededRecords;
                            resultDTO.Message = "Time Exceeded orders fetched successfully";
                            resultDTO.IsSuccess = true;
                        }
                        else
                        {
                            resultDTO.IsSuccess = false;
                            resultDTO.Message = "Time Exceeded orders not found";
                            resultDTO.StatusCode = "404";
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }

        public ResultDTO ReplaceOrders(ReplaceOrdersDTO replaceOrdersDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                SkillSet? skillSet = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == replaceOrdersDTO.SkillsetId && x.IsActive).FirstOrDefault();
                if (skillSet != null)
                {
                    List<TemplateColumns> template = _oMTDataContext.TemplateColumns.Where(x => x.SkillSetId == replaceOrdersDTO.SkillsetId).ToList();
                    if(template.Count > 0)
                    {
                        string tablename = skillSet.SkillSetName;

                        List<string> listofcolumns1 = _oMTDataContext.DefaultTemplateColumns.Where(x => x.SystemOfRecordId == skillSet.SystemofRecordId && x.IsDuplicateCheck).Select(_ => _.DefaultColumnName).ToList();
                        List<string> listofColumns = template.Where(x => x.IsDuplicateCheck).Select(_ => _.ColumnAliasName).ToList();

                        List<string> combinedList = listofcolumns1.Concat(listofColumns).ToList();

                        //parse json data
                        JObject jsondata = JObject.Parse(replaceOrdersDTO.JsonData);
                        JArray recordsarray = jsondata.Value<JArray>("Records");

                        string isDuplicateColumns1 = listofcolumns1 != null ? string.Join(",", listofcolumns1) : "";
                        string isDuplicateColumns = listofColumns != null ? string.Join(",", listofColumns) : "";

                        // Combine the strings, ensuring that if any of them is null, it's selected without a comma
                        string combinedString = (isDuplicateColumns1 != "" && isDuplicateColumns != "") ? isDuplicateColumns1 + "," + isDuplicateColumns : isDuplicateColumns1 + isDuplicateColumns;

                        string sql = $"SELECT * "+
                                    $"FROM {tablename} t " +
                                    $"WHERE UserID IS NULL AND ";

                        foreach (JObject records in recordsarray)
                        {
                            string query = "(";
                            foreach (string columnname in combinedList)
                            {
                                string columndata = records.Value<string>(columnname);

                                query += $"[{columnname}] = '{columndata}' AND ";
                            }

                            query = query.Substring(0, query.Length - 5);
                            query += ") OR ";

                            sql += query;
                        }

                        sql = sql.Substring(0, sql.Length - 4);

                        //execute sql query to fetch records from table
                        string? connectionstring = _oMTDataContext.Database.GetConnectionString();

                        using SqlConnection connection = new(connectionstring);
                        using SqlDataAdapter dataAdapter = new SqlDataAdapter(sql, connection);

                        DataSet dataset = new DataSet();
                        connection.Open();

                        dataAdapter.Fill(dataset);

                        DataTable datatable = dataset.Tables[0];

                        //query dt to get records
                        var querydt = datatable.AsEnumerable()
                                      .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                                          column => column.ColumnName,
                                          column => row[column])).ToList();

                        List<JObject> recordsToInsert = new List<JObject>();

                        if (querydt.Count > 0 )
                        {
                            string deleteSql = $"DELETE FROM {tablename} WHERE UserId IS NULL AND ";

                            foreach (var record in querydt)
                            {
                                string deleteQuery = "(";
                                foreach (string columnname in combinedList)
                                {
                                    string columndata = record[columnname].ToString();

                                    deleteQuery += $"[{columnname}] = '{columndata}' AND ";
                                }

                                deleteQuery = deleteQuery.Substring(0, deleteQuery.Length - 5);
                                deleteQuery += ") OR ";

                                deleteSql += deleteQuery;
                            }

                            deleteSql = deleteSql.Substring(0, deleteSql.Length - 4);

                            using SqlCommand deleteCommand = new SqlCommand(deleteSql, connection);
                            deleteCommand.ExecuteNonQuery();

                            foreach (JObject records in recordsarray)
                            {
                                bool recordExists = querydt.Any(existingRecord =>
                                    combinedList.All(column => records.Value<string>(column) == existingRecord[column].ToString()));

                                if (recordExists)
                                {
                                    recordsToInsert.Add(records);
                                }
                            }

                            if (recordsToInsert.Count > 0)
                            {
                                // Create the new JSON object with "Records" key
                                JObject recordsObject = new JObject();
                                recordsObject["Records"] = JArray.FromObject(recordsToInsert);

                                // Serialize the records to a JSON string
                                string jsonToInsert = recordsObject.ToString();

                                using SqlCommand cmd = new SqlCommand("InsertData", connection);
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@SkillSetId", replaceOrdersDTO.SkillsetId);
                                cmd.Parameters.AddWithValue("@jsonData", jsonToInsert);

                                SqlParameter returnValue = new()
                                {
                                    ParameterName = "@RETURN_VALUE",
                                    Direction = ParameterDirection.ReturnValue
                                };

                                cmd.Parameters.Add(returnValue);

                                cmd.ExecuteNonQuery();

                                int returnCode = (int)cmd.Parameters["@RETURN_VALUE"].Value;

                                if (returnCode != 1)
                                {
                                    throw new InvalidOperationException("Something went wrong while replacing the orders,please check the order details.");
                                }

                                resultDTO.IsSuccess = true;
                                resultDTO.Message = "Orders replaced successfully";
                            }

                        }
                        
                    }
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.StatusCode = "404";
                    resultDTO.Message = "Skillset does not exist.";
                }
            }
            catch (Exception ex)    
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }
    }
}
