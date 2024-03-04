using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DataService.Utility;
using OMT.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
                    Template template = _oMTDataContext.Template.Where(x => x.IsActive && x.TemplateName.ToLower().Trim() == createTemplateDTO.TemplateName.ToLower().Trim()).FirstOrDefault();
                    if (template != null)
                    {
                        resultDTO.IsSuccess = false;
                        resultDTO.Message = "Template Name already exists.";
                    }
                    else
                    {
                        if (createTemplateDTO.TemplateColumns.Any())
                        {
                            Template newtemplate = new Template()
                            {
                                TemplateName = createTemplateDTO.TemplateName,
                                TemplateAliasName = createTemplateDTO.TemplateName.Replace(" ", "_"),
                                CreatedDate = DateTime.Now,
                                IsActive = true
                            };
                            _oMTDataContext.Template.Add(newtemplate);
                            _oMTDataContext.SaveChanges();
                            int TemplateId = 0;
                            TemplateId = newtemplate.TemplateId;

                            foreach (TemplateColumnDTO templateColumns in createTemplateDTO.TemplateColumns)
                            {
                                TemplateColumns newtemplateColumns = new TemplateColumns()
                                {
                                    TemplateId = TemplateId,
                                    ColumnAliasName = templateColumns.ColumnName.Replace(" ", "_"),
                                    ColumnName = templateColumns.ColumnName,
                                    ColumnDataType = templateColumns.ColumnDataType,
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
                        command.Parameters.AddWithValue("@TemplateName", createTemplateDTO.TemplateName);
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
    }
}
