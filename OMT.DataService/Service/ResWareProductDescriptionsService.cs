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
    public class ResWareProductDescriptionsService : IResWareProductDescriptionsService
    {
        private readonly OMTDataContext _oMTDataContext;

        public ResWareProductDescriptionsService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }

        public ResultDTO CreateOnlyResWareProductDescriptions(ResWareProductDescriptionOnlyDTO resWareProductDescriptionOnlyDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                var res = _oMTDataContext.ResWareProductDescriptions.Where(x => x.IsActive && resWareProductDescriptionOnlyDTO.ResWareProductDescriptionNames.Contains(x.ResWareProductDescriptionName)).Select(x => x.ResWareProductDescriptionName).ToList();

                if (res.Any())
                {
                    resultDTO.Message = "The following resware product descriptions already exists: " + string.Join(", ", res) + ". Please try again.";
                    resultDTO.IsSuccess = false;
                }
                else
                {
                    var rpdnames = resWareProductDescriptionOnlyDTO.ResWareProductDescriptionNames.Select(x => new ResWareProductDescriptions { ResWareProductDescriptionName = x, IsActive = true }).ToList();

                    _oMTDataContext.ResWareProductDescriptions.AddRange(rpdnames);
                    _oMTDataContext.SaveChanges();

                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "ResWare Product Descriptions added successfully.";
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
        public ResultDTO GetResWareProductDescriptions()
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                var resWareProductDescriptions = _oMTDataContext.ResWareProductDescriptions.Where(x => x.IsActive).Select(_ => new { _.ResWareProductDescriptionName, _.ResWareProductDescriptionId }).ToList();

                if (resWareProductDescriptions.Count > 0)
                {
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "List of ResWare Product Descriptions";
                    resultDTO.Data = resWareProductDescriptions;
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "ResWare Product Descriptions not found";
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

        public ResultDTO UpdateResWareProductDescriptions(ResWareProductDescriptionsUpdateDTO res)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "201" };
            try
            {
                ResWareProductDescriptions resWareProductDescriptions = _oMTDataContext.ResWareProductDescriptions.Where(x => x.IsActive && x.ResWareProductDescriptionId == res.ResWareProductDescriptionId).FirstOrDefault();

                if (resWareProductDescriptions != null)
                {
                    resWareProductDescriptions.ResWareProductDescriptionName = res.ResWareProductDescriptionName;
                    _oMTDataContext.ResWareProductDescriptions.Update(resWareProductDescriptions);
                    _oMTDataContext.SaveChanges();

                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "ResWare Product Description updated successfully";
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "ResWare Product Descriptions not found";
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
        public ResultDTO CreateResWareProductDescriptionsMap(ResWareProductDescriptionsDTO resWareProductDescriptionsDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {

               var rpdmapexists = (from rpdm in _oMTDataContext.ResWareProductDescriptionMap
                                    join rpd in _oMTDataContext.ResWareProductDescriptions on rpdm.ResWareProductDescriptionId equals rpd.ResWareProductDescriptionId
                                    join ss in _oMTDataContext.SkillSet on rpdm.SkillSetId equals ss.SkillSetId
                                    where rpdm.SkillSetId == resWareProductDescriptionsDTO.SkillSetId && rpdm.ProductDescriptionId == resWareProductDescriptionsDTO.ProductDescriptionId && resWareProductDescriptionsDTO.ResWareProductDescriptionIds.Contains(rpdm.ResWareProductDescriptionId)
                                    select new 
                                    {
                                        ResWareProductDescriptionName =   rpd.ResWareProductDescriptionName,
                                        SkillSetName =   ss.SkillSetName
                                    }
                                    ).ToList();

                if (rpdmapexists.Any())
                {
                    var skillSetName = rpdmapexists.First().SkillSetName;
                    var descriptionNames = string.Join(", ", rpdmapexists.Select(x => x.ResWareProductDescriptionName));
                    resultDTO.Message = $"The following ResWare product descriptions already exist for {skillSetName}: {descriptionNames}. Please try again.";
                    resultDTO.IsSuccess = false;
                }
                else
                {
                    var rpdnames = resWareProductDescriptionsDTO.ResWareProductDescriptionIds.Select(x => new ResWareProductDescriptionMap { ResWareProductDescriptionId = x, SkillSetId = resWareProductDescriptionsDTO.SkillSetId, ProductDescriptionId = resWareProductDescriptionsDTO.ProductDescriptionId }).ToList();

                    _oMTDataContext.ResWareProductDescriptionMap.AddRange(rpdnames);
                    _oMTDataContext.SaveChanges();

                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "ResWare Product Description mapped successfully";
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

        public ResultDTO DeleteResWareProductDescriptions(int rprodid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                ResWareProductDescriptions resWareProductDescriptions = _oMTDataContext.ResWareProductDescriptions.Where(x => x.ResWareProductDescriptionId == rprodid && x.IsActive).FirstOrDefault();

                if (resWareProductDescriptions != null)
                {
                    resWareProductDescriptions.IsActive = false;
                    _oMTDataContext.ResWareProductDescriptions.Update(resWareProductDescriptions);
                    _oMTDataContext.SaveChanges();

                    var res = _oMTDataContext.ResWareProductDescriptionMap.Where(x => x.ResWareProductDescriptionId == rprodid).ToList();

                    _oMTDataContext.ResWareProductDescriptionMap.RemoveRange(res);
                    _oMTDataContext.SaveChanges();

                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "ResWare Product Description has been deleted successfully.";
                }
                else
                {
                    resultDTO.StatusCode = "404";
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "ResWare Product Description is not found";

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

        public ResultDTO GetResWareProductDescriptionsMap(int? skillsetId)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                if (skillsetId == null)
                {
                    List<ResProductDescriptionsMapResponseDTO> res = (from rpdm in _oMTDataContext.ResWareProductDescriptionMap
                                                                      join ss in _oMTDataContext.SkillSet on rpdm.SkillSetId equals ss.SkillSetId
                                                                      join rpd in _oMTDataContext.ResWareProductDescriptions on rpdm.ResWareProductDescriptionId equals rpd.ResWareProductDescriptionId
                                                                      join pd in _oMTDataContext.ProductDescription on rpdm.ProductDescriptionId equals pd.ProductDescriptionId
                                                                      where pd.IsActive && rpd.IsActive
                                                                      orderby ss.SkillSetName, pd.ProductDescriptionName, rpd.ResWareProductDescriptionName
                                                                      select new ResProductDescriptionsMapResponseDTO
                                                                      {
                                                                          ResWareProductDescriptionId = rpdm.ResWareProductDescriptionId,
                                                                          ResWareProductDescriptionName = rpd.ResWareProductDescriptionName,
                                                                          ProductDescriptionId = rpdm.ProductDescriptionId,
                                                                          ProductDescriptionName = pd.ProductDescriptionName,
                                                                          SkillSetId = ss.SkillSetId,
                                                                          SkillSetName = ss.SkillSetName

                                                                      }).ToList();

                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "List of Product Descriptions and its Resware Product Descriptions";
                    resultDTO.Data = res;
                }
                else
                {
                    List<ResProductDescriptionsMapResponseDTO> res = (from rpdm in _oMTDataContext.ResWareProductDescriptionMap
                                                                      join ss in _oMTDataContext.SkillSet on rpdm.SkillSetId equals ss.SkillSetId
                                                                      join rpd in _oMTDataContext.ResWareProductDescriptions on rpdm.ResWareProductDescriptionId equals rpd.ResWareProductDescriptionId
                                                                      join pd in _oMTDataContext.ProductDescription on rpdm.ProductDescriptionId equals pd.ProductDescriptionId
                                                                      where pd.IsActive && rpd.IsActive && rpdm.SkillSetId == skillsetId
                                                                      orderby ss.SkillSetName, pd.ProductDescriptionName, rpd.ResWareProductDescriptionName
                                                                      select new ResProductDescriptionsMapResponseDTO
                                                                      {
                                                                          ResWareProductDescriptionId = rpdm.ResWareProductDescriptionId,
                                                                          ResWareProductDescriptionName = rpd.ResWareProductDescriptionName,
                                                                          ProductDescriptionId = rpdm.ProductDescriptionId,
                                                                          ProductDescriptionName = pd.ProductDescriptionName,
                                                                          SkillSetId = ss.SkillSetId,
                                                                          SkillSetName = ss.SkillSetName
                                                                      }).ToList();

                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "List of Product Description and its Resware Product Descriptions";
                    resultDTO.Data = res;
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
