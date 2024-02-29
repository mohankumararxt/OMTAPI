using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "201" };

            try
            {
                string existingSkillSetName = _oMTDataContext.SkillSet.Where(x => x.SkillSetName == skillSetCreateDTO.SkillSetName).Select(_ => _.SkillSetName).FirstOrDefault();
               
                if (existingSkillSetName != null)
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "The SkillSetName already exists. Please try to add different SkillSetName.";
                }

                else
                {
                    SkillSet skillSet = new SkillSet()
                    {
                        SkillSetName = skillSetCreateDTO.SkillSetName,
                        IsActive = true
                    };

                    _oMTDataContext.SkillSet.Add(skillSet);
                    _oMTDataContext.SaveChanges();
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
                List<SkillSetResponseDTO> ListofSkillSets = _oMTDataContext.SkillSet.Where(x => x.IsActive).Select(_ => new SkillSetResponseDTO() { SkillSetId = _.SkillSetId, SkillSetName = _.SkillSetName }).ToList();
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

        public ResultDTO UpdateSkillSet(SkillSetResponseDTO skillSetResponseDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "201" };

            try
            {
                SkillSet? skillset = _oMTDataContext.SkillSet.Find(skillSetResponseDTO.SkillSetId);

                if (skillset != null)
                {
                    skillset.SkillSetName = skillSetResponseDTO.SkillSetName;
                   // skillset.IsActive = true;

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
