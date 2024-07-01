using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.DataService.Service
{
    public class TeamsService : ITeamsService
    {
        private readonly OMTDataContext _oMTDataContext;
        public TeamsService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="createTeamsDTO"></param>
        /// <returns></returns>
        public ResultDTO CreateTeams(TeamsCreateDTO createTeamsDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                string existingTeamName = _oMTDataContext.Teams.Where(x => x.TeamName == createTeamsDTO.TeamName && x.IsActive).Select(_ => _.TeamName).FirstOrDefault();
                if (existingTeamName != null)
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "The TeamName already exists. Please try to create with different name.";
                }
                else
                {
                    Teams teams  = new Teams()
                    {
                        TeamName = createTeamsDTO.TeamName,
                        Description = createTeamsDTO.Description,
                        IsActive = true
                    };

                    _oMTDataContext.Teams.Add(teams);
                    _oMTDataContext.SaveChanges();
                    resultDTO.Message = "Teams Created Successfully.";
                    resultDTO.IsSuccess = true;

                    resultDTO.Data = GetTeamsList().Data;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="teamId"></param>
        /// <returns></returns>
        public ResultDTO DeleteTeam(int teamId)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                Teams? teamsObj = _oMTDataContext.Teams.Where(x=>x.TeamId == teamId && x.IsActive).FirstOrDefault();
                if (teamsObj == null)
                {
                    resultDTO.StatusCode = "404";
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Team is not found";
                }
                else
                {
                    teamsObj.IsActive = false;
                    _oMTDataContext.Teams.Update(teamsObj);
                    _oMTDataContext.SaveChanges();

                    var teamasso = _oMTDataContext.TeamAssociation.Where(x => x.TeamId == teamId).ToList();

                    _oMTDataContext.TeamAssociation.RemoveRange(teamasso);
                    _oMTDataContext.SaveChanges();

                    resultDTO.Message = "Teams Deleted Successfully.";
                    resultDTO.IsSuccess = true;
                    resultDTO.Data = GetTeamsList().Data;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="teamId"></param>
        /// <returns></returns>
        public ResultDTO GetTeamById(int teamId)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                TeamsResponseDTO? teamsResponseDTO = _oMTDataContext.Teams.Where(x => x.TeamId == teamId && x.IsActive).Select(_=> new TeamsResponseDTO() { Description = _.Description, TeamId=_.TeamId, TeamName=_.TeamName }).FirstOrDefault();
                if (teamsResponseDTO == null)
                {
                    resultDTO.StatusCode = "404";
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Team is not found";
                }
                else
                {
                    resultDTO.Data = teamsResponseDTO;
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Team Details fetched Successfully";
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ResultDTO GetTeamsList()
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                List<TeamsResponseDTO> ListofTeams = _oMTDataContext.Teams.Where(x => x.IsActive).OrderBy(x => x.TeamName).Select(_ => new TeamsResponseDTO() { Description = _.Description, TeamId = _.TeamId, TeamName = _.TeamName }).ToList();
                resultDTO.Data = ListofTeams;
                resultDTO.IsSuccess = true;
                resultDTO.Message = "List of Teams";
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="teamname"></param>
        /// <returns></returns>
        public ResultDTO teamFilterBYKeyword(string teamname)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                if (string.IsNullOrWhiteSpace(teamname))
                {
                    resultDTO.StatusCode = "500";
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = $"teamname cannot be empty";
                }
                else
                {
                    List<TeamsResponseDTO> teamsList = _oMTDataContext.Teams.Where(x=>(x.TeamName.Contains(teamname) || x.Description.Contains(teamname)) && x.IsActive).OrderBy(x => x.TeamName).Select(_=> new TeamsResponseDTO() { Description=_.Description, TeamId=_.TeamId, TeamName=_.TeamName }).ToList();
                    
                    if (teamsList != null && teamsList.Count >0)
                    {
                        resultDTO.IsSuccess = true;
                        resultDTO.Message = $"Team Details found with keyword {teamname}";
                        resultDTO.Data = teamsList;

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="teamsResponseDTO"></param>
        /// <returns></returns>
        public ResultDTO UpdateTeams(TeamsResponseDTO teamsResponseDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "201" };
            try
            {
                Teams? teams = _oMTDataContext.Teams.Find(teamsResponseDTO.TeamId);
                if (teams == null)
                {
                    resultDTO.StatusCode = "404";
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Team is not found";
                }
                else
                {
                    teams.TeamName = teamsResponseDTO.TeamName;
                    teams.Description = teamsResponseDTO.Description;
                    teams.IsActive = true;
                    _oMTDataContext.Teams.Update(teams);
                    _oMTDataContext.SaveChanges();
                    resultDTO.Message = "Teams Updated Successfully.";
                    resultDTO.Data = teams;
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
    }
}
