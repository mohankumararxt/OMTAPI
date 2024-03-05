﻿using OMT.DataAccess.Context;
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
    public class TeamAssociationService : ITeamAssociationService
    {
        private readonly OMTDataContext _oMTDataContext;
        public TeamAssociationService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }
        public ResultDTO AddTeamAssociation(TeamAssociationCreateDTO teamAssociationCreateDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                var existing_TeamAssociationId = _oMTDataContext.TeamAssociation.Where(x => x.UserId == teamAssociationCreateDTO.UserId && x.TeamId == teamAssociationCreateDTO.TeamId).FirstOrDefault();
                if (existing_TeamAssociationId != null)
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Selected Team Association already exists";
                }
                else
                {
                    TeamAssociation teamAssociation = new TeamAssociation()
                    {
                        UserId = teamAssociationCreateDTO.UserId,
                        TeamId = teamAssociationCreateDTO.TeamId,
                        ThresholdCount = teamAssociationCreateDTO.ThresholdCount,
                        Description = teamAssociationCreateDTO.Description,
                        CreatedDate = DateTime.Now
                    };
                    _oMTDataContext.TeamAssociation.Add(teamAssociation);
                    _oMTDataContext.SaveChanges();
                    resultDTO.Message = "Team association has been added successfully";
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
        public ResultDTO DeleteTeamAssociation(int associationid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                TeamAssociation? teamAssociation = _oMTDataContext.TeamAssociation.Find(associationid);
                if (teamAssociation != null)
                {
                    _oMTDataContext.TeamAssociation.Remove(teamAssociation);
                    _oMTDataContext.SaveChanges();
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Team Association deleted successfully";
                }
                else
                {
                    resultDTO.StatusCode = "404";
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Team Association is not found";
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
        public ResultDTO GetTeamAssociationList()
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200"};
            try
            {
                List<TeamAssociationResponseDTO> ListofTeamAssociations = (from up in _oMTDataContext.UserProfile
                                                                           join ta in _oMTDataContext.TeamAssociation on up.UserId equals ta.UserId
                                                                           join t in _oMTDataContext.Teams on ta.TeamId equals t.TeamId
                                                                           select new TeamAssociationResponseDTO()
                                                                           {
                                                                               FirstName = up.FirstName,
                                                                               TeamName = t.TeamName,
                                                                               Description = ta.Description,
                                                                               ThresholdCount = ta.ThresholdCount
                                                                           }).ToList();
                resultDTO.Data = ListofTeamAssociations;
                resultDTO.IsSuccess = true;
                resultDTO.Message = "List of Team Associations";
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }
        public ResultDTO GetTeamAssociationByTeamId(int teamid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                List<TeamAssociationResponseDTO> ListofTeamAssociations = (from up in _oMTDataContext.UserProfile
                                                                           join ta in _oMTDataContext.TeamAssociation on up.UserId equals ta.UserId
                                                                           join t in _oMTDataContext.Teams on ta.TeamId equals t.TeamId
                                                                           where ta.TeamId == teamid
                                                                           select new TeamAssociationResponseDTO()
                                                                           {
                                                                               FirstName = up.FirstName,
                                                                               TeamName = t.TeamName,
                                                                               Description = ta.Description,
                                                                               ThresholdCount = ta.ThresholdCount
                                                                           }).ToList();
                resultDTO.Data = ListofTeamAssociations;
                resultDTO.IsSuccess = true;
                resultDTO.Message = "List of Team Associations";
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
