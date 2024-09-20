using Microsoft.Data.SqlClient;
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
        public ResultDTO CreateSkillSet(SkillSetCreateDTO skillSetCreateDTO)
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
                    bool isHardState = skillSetCreateDTO.HardstateNames != null && skillSetCreateDTO.HardstateNames.Any(); 
                    SkillSet skillSet = new SkillSet()
                    {
                        SystemofRecordId = skillSetCreateDTO.SystemofRecordId,
                        SkillSetName = skillSetCreateDTO.SkillSetName,
                        Threshold = skillSetCreateDTO.Threshold,
                        IsHardState = skillSetCreateDTO.IsHardState, 
                        IsActive = true
                    };
                    _oMTDataContext.SkillSet.Add(skillSet);
                    _oMTDataContext.SaveChanges();

                    // add harstatenames for skillset

                    var skillsetid = _oMTDataContext.SkillSet.Where(x => x.SkillSetName == skillSetCreateDTO.SkillSetName && x.IsActive).Select(_ => _.SkillSetId).FirstOrDefault();

                    if (isHardState) 
                    {
                        foreach (var item in skillSetCreateDTO.HardstateNames)
                        {
                            SkillSetHardStates skillSetHardStates = new SkillSetHardStates()
                            {
                                SkillSetId = skillsetid,
                                StateName = item,
                                IsActive = true,
                                CreatedDate = DateTime.Now,
                            };
                            _oMTDataContext.SkillSetHardStates.Add(skillSetHardStates);
                            _oMTDataContext.SaveChanges();
                        }    
                    }
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
                if (skillSet == null)
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

                    foreach (var t in tl)
                    {
                        t.IsActive = false;
                        _oMTDataContext.Timeline.Update(t);
                    }

                    _oMTDataContext.SaveChanges();

                    // delete SkillSetHardStates of this skillset

                    var sshs = _oMTDataContext.SkillSetHardStates.Where(x => x.IsActive && x.SkillSetId == skillSet.SkillSetId).ToList();

                    foreach (var ss in sshs)
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
                var skillSetGroups = (from ss in _oMTDataContext.SkillSet
                                      join sor in _oMTDataContext.SystemofRecord on ss.SystemofRecordId equals sor.SystemofRecordId
                                      join hs in _oMTDataContext.SkillSetHardStates on ss.SkillSetId equals hs.SkillSetId into hsGroup 
                                      from hs in hsGroup.DefaultIfEmpty()  // Left join to include all skill sets
                                      where sor.IsActive  
                                      orderby ss.SkillSetId,sor.SystemofRecordName, ss.SkillSetName
                                      group new { ss, sor, hs } by new  //Group the data's
                                      {
                                          ss.SkillSetId,
                                          ss.SkillSetName,
                                          ss.Threshold,
                                          sor.SystemofRecordName,
                                          ss.SystemofRecordId,
                                          ss.IsHardState
                                      } into grp  //here grp is the grouping key
                                        select new SkillSetResponseDTO
                                      {
                                          SkillSetId = grp.Key.SkillSetId,
                                          SkillSetName = grp.Key.SkillSetName,
                                          Threshold = grp.Key.Threshold,
                                          SystemofRecordName = grp.Key.SystemofRecordName,
                                          SystemofRecordId = grp.Key.SystemofRecordId,
                                          IsHardState = grp.Any(x => x.hs != null),
                                          StateName = string.Join(", ", grp.Select(x => x.hs.StateName)) 
                                      }).ToList();

                resultDTO.IsSuccess = true;
                resultDTO.Message = "List of SkillSets";
                resultDTO.Data = skillSetGroups;
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
                var skillSetGroups = (from ss in _oMTDataContext.SkillSet
                                      join sor in _oMTDataContext.SystemofRecord on ss.SystemofRecordId equals sor.SystemofRecordId
                                      join hs in _oMTDataContext.SkillSetHardStates on ss.SkillSetId equals hs.SkillSetId into hsGroup 
                                      from hs in hsGroup.DefaultIfEmpty()  
                                      where sor.IsActive && sor.SystemofRecordId == sorid // Filter by the specific SOR ID
                                      orderby sor.SystemofRecordId
                                      group new { ss, sor, hs } by new 
                                      {
                                          ss.SkillSetId,
                                          ss.SkillSetName,
                                          ss.Threshold,
                                          sor.SystemofRecordName,
                                          ss.SystemofRecordId,
                                          ss.IsHardState
                                      } into grp  
                                      select new SkillSetResponseDTO
                                      {
                                          SkillSetId = grp.Key.SkillSetId,
                                          SkillSetName = grp.Key.SkillSetName,
                                          Threshold = grp.Key.Threshold,
                                          SystemofRecordName = grp.Key.SystemofRecordName,
                                          SystemofRecordId = grp.Key.SystemofRecordId,
                                          IsHardState = grp.Any(x => x.hs != null), //null 
                                          StateName = string.Join(", ", grp.Select(x => x.hs.StateName)) 
                                      }).ToList();

                resultDTO.IsSuccess = true;
                resultDTO.Message = "List of SkillSets";
                resultDTO.Data = skillSetGroups;
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

                if (hardStatename.Count > 0)
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
                    resultDTO.IsSuccess = true;
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
