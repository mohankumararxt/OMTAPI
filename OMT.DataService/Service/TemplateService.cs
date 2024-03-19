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

        public ResultDTO ValidateOrders(UploadTemplateDTO uploadTemplateDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                SkillSet? skillSet = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == uploadTemplateDTO.SkillsetId && x.IsActive).FirstOrDefault();
                if (skillSet != null)
                {
                    List<TemplateColumns> template = _oMTDataContext.TemplateColumns.Where(x => x.SkillSetId == uploadTemplateDTO.SkillsetId).ToList();
                    if (template.Count > 0)
                    {
                        string tablename = skillSet.SkillSetName;

                        List<string> listofColumns = template.Where(x => x.IsDuplicateCheck).Select(_ => _.ColumnName).ToList();

                        //parse json data
                        JObject jsondata = JObject.Parse(uploadTemplateDTO.JsonData);
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

        //public ResultDTO GetOrders(int userid)
        //{
        //    ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
        //    try
        //    {
        //        UserSkillSet? userSkillSet = _oMTDataContext.UserSkillSet.Where(x => x.UserId == userid && x.IsActive).
        //            OrderByDescending(x => x.IsPrimary).ThenByDescending(x => x.Percentage).FirstOrDefault();
        //        if (userSkillSet != null)
        //        {
        //            SkillSet skillset = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == userSkillSet.SkillSetId).FirstOrDefault();

        //            int? teamleadid = _oMTDataContext.TeamAssociation.Where(x => x.UserId == userid).Select(_ => _.TeamId).FirstOrDefault();

        //            List<TemplateColumns> template = _oMTDataContext.TemplateColumns.Where(x => x.SkillSetId == skillset.SkillSetId).ToList();
        //            if (template.Count > 0)
        //            {
        //                List<string> listofColumns = template.Select(_ => _.ColumnName).ToList();
        //                string Columns = string.Join(",", listofColumns);
        //                int Id = 0;

        //                string sql = $"SELECT TOP 1 * FROM {skillset.SkillSetName} WHERE UserId IS NULL ORDER BY Id";

        //                string? connectionstring = _oMTDataContext.Database.GetConnectionString();

        //                using SqlConnection connection = new(connectionstring);

        //                connection.Open();

        //                using (SqlCommand checkorders = connection.CreateCommand())
        //                {
        //                    checkorders.CommandText = sql;

        //                    using (SqlDataReader reader = checkorders.ExecuteReader())
        //                    {
        //                        if (reader.Read())
        //                        {
        //                            //get the id of the fetched order from the table
        //                            Id = reader.GetInt32(reader.GetOrdinal("Id"));
        //                        }
        //                    }
        //                }
        //                if (Id != 0)
        //                {
        //                    using (SqlCommand updateorder = connection.CreateCommand())
        //                    {
        //                        updateorder.CommandText = $"UPDATE {skillset.SkillSetName} SET UserId = @UserId, Status = @Status, TeamLeadId = @TeamLeadId, SystemofRecordId = @SystemofRecordId, SkillSetId = @SkillSetId WHERE Id = {Id}";
        //                        updateorder.Parameters.AddWithValue("@UserId", userid);
        //                        updateorder.Parameters.AddWithValue("@Status", 1);
        //                        updateorder.Parameters.AddWithValue("@TeamLeadId", teamleadid);
        //                        updateorder.Parameters.AddWithValue("@SystemofRecordId", skillset.SystemofRecordId);
        //                        updateorder.Parameters.AddWithValue("@SkillSetId", skillset.SkillSetId);
        //                        updateorder.ExecuteNonQuery();
        //                    }
        //                }
        //                if (Id == 0)
        //                {
        //                    resultDTO.IsSuccess = false;
        //                    resultDTO.Message = "Orders not available";
        //                    resultDTO.StatusCode = "404";
        //                }

        //                using (SqlCommand getupdatedorder = connection.CreateCommand())
        //                {
        //                    getupdatedorder.CommandText = $"SELECT Id, {Columns} FROM {skillset.SkillSetName} WHERE Id = {Id}";
        //                    using (SqlDataReader sqlDataReader = getupdatedorder.ExecuteReader())
        //                    {

        //                        dynamic order = new ExpandoObject();
        //                        while (sqlDataReader.Read())
        //                        {
        //                            for (int i = 0; i < sqlDataReader.FieldCount; i++)
        //                            {
        //                                string colname = sqlDataReader.GetName(i);
        //                                object colvalue = sqlDataReader.GetValue(i);
        //                                ((IDictionary<string, object>)order)[colname] = colvalue;
        //                            }
        //                        }

        //                        if (order != null)
        //                        {
        //                            object updorder = order;
        //                            resultDTO.IsSuccess = true;
        //                            resultDTO.Data = updorder;
        //                            resultDTO.Message = "Your order";
        //                        }
        //                    }
        //                }

        //            }
        //            else
        //            {
        //                resultDTO.IsSuccess = false;
        //                resultDTO.StatusCode = "404";
        //                resultDTO.Message = "Template doesnt exist";
        //            }
        //        }
        //        else
        //        {
        //            resultDTO.IsSuccess = false;
        //            resultDTO.StatusCode = "404";
        //            resultDTO.Message = "something went wrong.";
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        resultDTO.IsSuccess = false;
        //        resultDTO.StatusCode = "500";
        //        resultDTO.Message = ex.Message;
        //    }
        //    return resultDTO;
        //}

        public ResultDTO GetOrders(int userid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                UserSkillSet? userSkillSet = _oMTDataContext.UserSkillSet.Where(x => x.UserId == userid && x.IsActive).OrderByDescending(x => x.IsPrimary).ThenByDescending(x => x.Percentage).FirstOrDefault();
                if(userSkillSet != null)
                {
                    SkillSet skillset = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == userSkillSet.SkillSetId).FirstOrDefault();

                    List<TemplateColumns> template = _oMTDataContext.TemplateColumns.Where(x => x.SkillSetId == skillset.SkillSetId).ToList();
                    if(template.Count > 0)
                    {
                        string updatedOrder = "";

                        string? connectionstring = _oMTDataContext.Database.GetConnectionString();

                        using SqlConnection connection = new(connectionstring);
                        using SqlCommand command = new()
                        {
                            Connection = connection,
                            CommandType = CommandType.StoredProcedure,
                            CommandText = "GetOrders"
                        };
                        command.Parameters.AddWithValue("@userid",userid);

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

                        connection.Open();
                        command.ExecuteNonQuery();

                        int returnCode = (int)command.Parameters["@RETURN_VALUE"].Value;

                        if (returnCode != 1)
                        {
                            throw new InvalidOperationException("Stored Procedure call failed.");
                        }

                        updatedOrder = command.Parameters["@updatedrecord"].Value.ToString();
                        if (updatedOrder != null)
                        {
                            resultDTO.Data = updatedOrder;
                            resultDTO.IsSuccess = true;
                            resultDTO.Message = "Order assigned successfully";
                        }

                    }
                    else
                    {
                        resultDTO.IsSuccess = false;
                        resultDTO.StatusCode = "404";
                        resultDTO.Message = "Template doesnt exist";
                    }
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.StatusCode = "404";
                    resultDTO.Message = "You dont have any skillsets in your profile.";
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
