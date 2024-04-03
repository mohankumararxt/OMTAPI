using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DTO;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;

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
                            throw new InvalidOperationException("Stored Procedure call failed.");
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
                            throw new InvalidOperationException("Stored Procedure call failed.");
                        }
                        //else
                        //{
                        //    if(uploadTemplateDTO.IsPriority)
                        //    {
                        //        SkillSetPriority skillSetPriority = new SkillSetPriority() { CreatedDate = DateTime.Now, IsComplete = false, SkillSetId = uploadTemplateDTO.SkillsetId};
                        //        _oMTDataContext.SkillSetPriority.Add(skillSetPriority);
                        //        _oMTDataContext.SaveChanges();
                        //    }
                        //}
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

                        List<string> listofColumns = template.Where(x => x.IsDuplicateCheck).Select(_ => _.ColumnName).ToList();

                        //parse json data
                        JObject jsondata = JObject.Parse(validateorderDTO.JsonData);
                        JArray recordsarray = jsondata.Value<JArray>("Records");

                        //sql query
                        string isDuplicateColumns = string.Join(",", listofColumns);
                        string defaultColumns = "UserId,Status,CompletionDate,StartTime,EndTime";

                        string sql = $"SELECT {isDuplicateColumns}, {defaultColumns} FROM {tablename} WHERE ";
                        foreach (JObject records in recordsarray)
                        {
                            string query = "(";
                            foreach (string columnname in listofColumns)
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
                            resultDTO.IsSuccess = true;
                            resultDTO.Data = querydt;
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
                                            select ss).Distinct().ToList();
                List<TemplateColumns> templatecolumns = _oMTDataContext.TemplateColumns.ToList();

                if(skillSets.Count > 0 ) { 
                    foreach(SkillSet skillset in skillSets)
                    {
                        TemplateListDTO templateListDTO = new TemplateListDTO();
                        templateListDTO.SkillsetId = skillset.SkillSetId;
                        templateListDTO.SkillSetName = skillset.SkillSetName;
                        templateListDTO.SystemofRecordId = skillset.SystemofRecordId;
                        templateListDTO.TemplateColumns = templatecolumns.Where(x => x.SkillSetId == skillset.SkillSetId).
                                                            Select(_ => new TemplateColumnDTO() { ColumnDataType = _.ColumnDataType, ColumnName = _.ColumnName, IsDuplicateCheck = _.IsDuplicateCheck }).ToList();
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

                    //List<SkillSetPriority> skillSetPriority = _oMTDataContext.SkillSetPriority.Where(x => x.IsComplete == false).ToList();
                    //if (skillSetPriority.Count != 0)
                    //{

                    //    List<int> skillsetids = skillSetPriority.Select(x => x.SkillSetId).ToList();
                    //    List<UserSkillSet> userskillset = userskillsetlist.Where(x => x.UserId == userid && x.IsActive && skillsetids.Contains(x.SkillSetId)).ToList();

                    //    if (userskillset.Count != 0)
                    //    {
                    //        string uporder;
                    //        using SqlCommand command = new()
                    //        {
                    //            Connection = connection,
                    //            CommandType = CommandType.StoredProcedure,
                    //            CommandText = "GetOrderByPriority"
                    //        };
                    //        command.Parameters.AddWithValue("@userid", userid);
                    //        //output param to get the record
                    //        SqlParameter outputParam = new SqlParameter("@updatedrecord", SqlDbType.NVarChar, -1);
                    //        outputParam.Direction = ParameterDirection.Output;
                    //        command.Parameters.Add(outputParam);

                    //        SqlParameter returnValue = new()
                    //        {
                    //            ParameterName = "@RETURN_VALUE",
                    //            Direction = ParameterDirection.ReturnValue
                    //        };
                    //        command.Parameters.Add(returnValue);


                    //        command.ExecuteNonQuery();

                    //        int returnCode = (int)command.Parameters["@RETURN_VALUE"].Value;

                    //        if (returnCode != 1)
                    //        {
                    //            throw new InvalidOperationException("Stored Procedure call failed.");
                    //        }

                    //        uporder = command.Parameters["@updatedrecord"].Value.ToString();
                    //        if (!string.IsNullOrWhiteSpace(uporder))
                    //        {
                    //            resultDTO.Data = uporder;
                    //            resultDTO.IsSuccess = true;
                    //            resultDTO.Message = "Order assigned successfully";
                    //        }
                    //        else
                    //        {
                    //            callSPbyPrimary(userid, resultDTO, connection);
                    //        }
                    //    }
                    //    else
                    //    {
                            callSPbyPrimary(userid, resultDTO, connection);
                    //    }
                   // }
                    //else
                    //{
                    //    UserSkillSet? userSkillSet = _oMTDataContext.UserSkillSet.Where(x => x.UserId == userid && x.IsActive).OrderByDescending(x => x.IsPrimary).ThenByDescending(x => x.Percentage).FirstOrDefault();
                    //    if (userSkillSet != null)
                    //    {
                    //        SkillSet skillset = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == userSkillSet.SkillSetId).FirstOrDefault();

                    //        List<TemplateColumns> template = _oMTDataContext.TemplateColumns.Where(x => x.SkillSetId == skillset.SkillSetId).ToList();
                    //        if (template.Count > 0)
                    //        {
                    //            callSPbyPrimary(userid, resultDTO, connection);
                    //        }

                    //    }

                    //}
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
               
                string? connectionstring = _oMTDataContext.Database.GetConnectionString();
                using SqlConnection connection = new(connectionstring);
                connection.Open();

               using (SqlCommand command = connection.CreateCommand())
               { 
                    command.CommandText = sql1;
                    command.Parameters.AddWithValue("@Status", updateOrderStatusDTO.StatusId);
                    command.Parameters.AddWithValue("@Remarks", updateOrderStatusDTO.Remarks);
                    command.Parameters.AddWithValue("@Id", updateOrderStatusDTO.Id);
                    command.Parameters.AddWithValue("@EndTime",updateOrderStatusDTO.EndTime);
                    command.Parameters.AddWithValue("@CompletionDate", updateOrderStatusDTO.EndTime);
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
                string columns = "Id,SkillSetId,Status,StartTime,EndTime";

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
                        var query1 = (from ss in _oMTDataContext.SkillSet
                                     join ps in _oMTDataContext.ProcessStatus on ss.SystemofRecordId equals ps.SystemOfRecordId
                                     where ss.SkillSetName == tablename && ps.Status == "Completed"
                                     select new
                                     {
                                         Id = ps.Id
                                     }).FirstOrDefault();

                        string sqlquery = $"SELECT {columns} FROM {tablename} WHERE UserId = @userid AND Status = {query1.Id} AND CompletionDate BETWEEN @FromDate AND @ToDate";
                       
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
                   var query = (from ss in _oMTDataContext.SkillSet
                                 join ps in _oMTDataContext.ProcessStatus on ss.SystemofRecordId equals ps.SystemOfRecordId
                                 where ss.SkillSetId == agentCompletedOrdersDTO.SkillSetId && ps.Status == "Completed"
                                 select new
                                 {
                                     SkillSetName = ss.SkillSetName,
                                     SystemofRecordId = ss.SystemofRecordId,
                                     Id = ps.Id
                                 }).FirstOrDefault();

                    string sql = $"SELECT {columns} FROM {query.SkillSetName} WHERE UserId = @userid AND Status = {query.Id} AND CompletionDate BETWEEN @FromDate AND @ToDate";

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
                string columns = "Id,UserId,SkillSetId,Status,StartTime,EndTime";

                string? connectionstring = _oMTDataContext.Database.GetConnectionString();

                using SqlConnection connection = new(connectionstring);
                connection.Open();

                if(teamCompletedOrdersDTO.SkillSetId != null)
                {
                    var query = (from ss in _oMTDataContext.SkillSet
                                 join ps in _oMTDataContext.ProcessStatus on ss.SystemofRecordId equals ps.SystemOfRecordId
                                 where ss.SkillSetId == teamCompletedOrdersDTO.SkillSetId && ps.Status == "Completed"
                                 select new
                                 {
                                     SkillSetName = ss.SkillSetName,
                                     SystemofRecordId = ss.SystemofRecordId,
                                     Id = ps.Id
                                 }).FirstOrDefault();

                    string sql1 = $"SELECT {columns} FROM {query.SkillSetName} WHERE TeamLeadId = @Teamid AND Status = {query.Id} AND CompletionDate BETWEEN @FromDate AND @ToDate";

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
                        var query1 = (from ss in _oMTDataContext.SkillSet
                                      join ps in _oMTDataContext.ProcessStatus on ss.SystemofRecordId equals ps.SystemOfRecordId
                                      where ss.SkillSetName == tablename && ps.Status == "Completed"
                                      select new
                                      {
                                          Id = ps.Id
                                      }).FirstOrDefault();

                        string sqlquery = $"SELECT {columns} FROM {tablename} WHERE TeamLeadId = @Teamid AND Status = {query1.Id} AND CompletionDate BETWEEN @FromDate AND @ToDate";

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
    }
}
