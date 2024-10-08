﻿using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DTO;
using System.Data;

namespace OMT.DataService.Service
{
    public class SkillSetService : ISkillSetService
    {
        private readonly OMTDataContext _oMTDataContext;
        public SkillSetService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }
        public ResultDTO CreateSkillSet( SkillSetCreateDTO skillSetCreateDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                string existingSkillSetName = _oMTDataContext.SkillSet.Where(x => x.SkillSetName == skillSetCreateDTO.SkillSetName && x.SystemofRecordId == skillSetCreateDTO.SystemofRecordId && x.IsActive).Select(_ => _.SkillSetName).FirstOrDefault();
                if (existingSkillSetName != null)
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "The SkillSet already exists. Please try to add different SkillSet.";
                }
                else
                {
                    SkillSet skillSet = new SkillSet()
                    {
                        SystemofRecordId = skillSetCreateDTO.SystemofRecordId,
                        SkillSetName = skillSetCreateDTO.SkillSetName,
                        Threshold = skillSetCreateDTO.Threshold,
                        IsActive = true
                    };
                    _oMTDataContext.SkillSet.Add(skillSet);
                    _oMTDataContext.SaveChanges();

                    // add harstatenames for skillset

                    //var skillsetid = _oMTDataContext.SkillSet.Where(x => x.SkillSetName == skillSetCreateDTO.SkillSetName && x.IsActive).Select(_ => _.SkillSetId).FirstOrDefault();

                    //foreach (var item in skillSetCreateDTO.HardstateNames)
                    //{
                    //    SkillSetHardStates skillSetHardStates = new SkillSetHardStates()
                    //    {
                    //        SkillSetId = skillsetid,
                    //        StateName = item,
                    //        IsActive = true,
                    //        CreatedDate = DateTime.Now,
                    //    };
                    //    _oMTDataContext.SkillSetHardStates.Add(skillSetHardStates);
                    //    _oMTDataContext.SaveChanges() ;
                    //}

                    resultDTO.Message = "SkillSet created successfully";
                    resultDTO.IsSuccess = true;
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
        public ResultDTO DeleteSkillSet(int skillsetId)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                SkillSet? skillSet = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == skillsetId && x.IsActive).FirstOrDefault();
                if(skillSet == null)
                {
                    resultDTO.StatusCode = "404";
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Skill Set is not found";
                }
                else
                {
                    skillSet.IsActive = false;
                    _oMTDataContext.SkillSet.Update(skillSet);
                    _oMTDataContext.SaveChanges();

                    // delete userskillsets associated with this skillset

                    var us = _oMTDataContext.UserSkillSet.Where(x => x.SkillSetId == skillsetId && x.IsActive).ToList();

                    foreach (var u in us)
                    {
                        u.IsActive = false;
                        _oMTDataContext.UserSkillSet.Update(u);
                    }

                    _oMTDataContext.SaveChanges();

                    //delete template

                    List<TemplateColumns> columns = _oMTDataContext.TemplateColumns.Where(x => x.SkillSetId == skillSet.SkillSetId).ToList();

                    _oMTDataContext.TemplateColumns.RemoveRange(columns);
                    _oMTDataContext.SaveChanges();

                    string? connectionstring = _oMTDataContext.Database.GetConnectionString();

                    using SqlConnection connection = new(connectionstring);
                    using SqlCommand command = new()
                    {
                        Connection = connection,
                        CommandType = CommandType.Text,
                        CommandText = @"
                                       IF OBJECT_ID(@TableName, 'U') IS NOT NULL
                                       BEGIN
                                           DROP TABLE [" + skillSet.SkillSetName + @"]
                                       END"
                    };
                    command.Parameters.AddWithValue("@TableName", skillSet.SkillSetName);

                    connection.Open();
                    command.ExecuteNonQuery();

                    // delete timeline associated with the skillset

                    var tl = _oMTDataContext.Timeline.Where(x => x.SkillSetId == skillSet.SkillSetId && x.IsActive).ToList();

                    foreach ( var t in tl)
                    {
                        t.IsActive = false;
                        _oMTDataContext.Timeline.Update(t);
                    }

                    _oMTDataContext.SaveChanges();

                    // delete SkillSetHardStates of this skillset

                    var sshs = _oMTDataContext.SkillSetHardStates.Where(x => x.IsActive && x.SkillSetId == skillSet.SkillSetId).ToList();

                    foreach ( var ss in sshs)
                    {
                        ss.IsActive = false;
                        _oMTDataContext.SkillSetHardStates.Update(ss);
                    }
                    _oMTDataContext.SaveChanges();

                    // delete details from joint tables

                    var ijr = _oMTDataContext.InvoiceJointResware.Where(x => x.SkillSetId == skillSet.SkillSetId).FirstOrDefault();

                    if (ijr != null)
                    {
                        _oMTDataContext.InvoiceJointResware.Remove(ijr);
                        _oMTDataContext.SaveChanges();
                    }

                    var ijs = _oMTDataContext.InvoiceJointSci.Where(x => x.SkillSetId == skillSet.SkillSetId).FirstOrDefault();

                    if (ijs != null)
                    {
                        _oMTDataContext.InvoiceJointSci.Remove(ijs);
                        _oMTDataContext.SaveChanges();
                    }

                    resultDTO.Message = "Skill Set has been deleted successfully";
                    resultDTO.IsSuccess = true;
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
        public ResultDTO GetSkillSetList()
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                // get list of skillsets
               List<SkillSetResponseDTO> ListofSkillSets = (from sor in _oMTDataContext.SystemofRecord
                                                             join ss in _oMTDataContext.SkillSet on sor.SystemofRecordId equals ss.SystemofRecordId
                                                             where ss.IsActive == true
                                                             orderby sor.SystemofRecordName,ss.SkillSetName
                                                             select new SkillSetResponseDTO
                                                             {
                                                                 SkillSetName = ss.SkillSetName,
                                                                 SkillSetId = ss.SkillSetId,
                                                                 Threshold = ss.Threshold,
                                                                 SystemofRecordName = sor.SystemofRecordName,
                                                                 SystemofRecordId = ss.SystemofRecordId,
                                                             }).ToList();
                resultDTO.IsSuccess = true;
                resultDTO.Message = "List of SkillSets";
                resultDTO.Data = ListofSkillSets;
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }

        public ResultDTO GetSkillSetListBySORId(int sorid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                List<SkillSetResponseDTO> ListofSkillSets = (from sor in _oMTDataContext.SystemofRecord
                                                             join ss in _oMTDataContext.SkillSet on sor.SystemofRecordId equals ss.SystemofRecordId
                                                             where ss.IsActive == true && sor.SystemofRecordId == sorid && sor.IsActive
                                                             orderby ss.SkillSetName
                                                             select new SkillSetResponseDTO
                                                             {
                                                                 SkillSetName = ss.SkillSetName,
                                                                 SkillSetId = ss.SkillSetId,
                                                                 Threshold = ss.Threshold,
                                                                 SystemofRecordName = sor.SystemofRecordName,
                                                                 SystemofRecordId = ss.SystemofRecordId,
                                                             }).ToList();

                if(ListofSkillSets.Count > 0 )
                {
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "List of SkillSets";
                    resultDTO.Data = ListofSkillSets;
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Skill sets not found for the given SystemofRecordId";
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

        public ResultDTO GetStatenameList(int skillsetid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                List<HardStatenameDTO> hardStatename = (from shs in _oMTDataContext.SkillSetHardStates
                                                        join ss in _oMTDataContext.SkillSet on shs.SkillSetId equals ss.SkillSetId
                                                        where shs.SkillSetId == skillsetid && shs.IsActive && ss.IsActive
                                                        select new HardStatenameDTO
                                                        {
                                                            //SkillSetId = ss.SkillSetId,
                                                            //SkillSetName = ss.SkillSetName,
                                                            HardstateName = shs.StateName,
                                                        }).ToList();

                if(hardStatename.Count > 0 )
                {
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "List of Statenames";
                    resultDTO.Data = hardStatename;
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Hardstate Names not found for the given SystemofRecordId";
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

        public ResultDTO UpdateSkillSet(SkillSetResponseDTO skillSetResponseDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "201" };
            try
            {
                SkillSet? skillset = _oMTDataContext.SkillSet.Find(skillSetResponseDTO.SkillSetId);
                if (skillset != null)
                {
                    skillset.SystemofRecordId = skillSetResponseDTO.SystemofRecordId;
                    skillset.SkillSetName = skillSetResponseDTO.SkillSetName;
                    skillset.Threshold = skillSetResponseDTO.Threshold;
                    _oMTDataContext.SkillSet.Update(skillset);
                    _oMTDataContext.SaveChanges();
                    resultDTO.IsSuccess= true;
                    resultDTO.Message = "Skill Set updated successfully";
                    resultDTO.Data = skillset;
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Skill set not found";
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


    }
}
