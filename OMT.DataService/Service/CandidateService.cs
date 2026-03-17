using OMT.DataService.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataService.Service
{
    public class CandidateService : ICandidateService
    {
        private readonly IActivityLogger _logger;

        public CandidateService(IActivityLogger logger)
        {
            _logger = logger;
        }

        public string CreateCandidate(string name)
        {
            // Your logic here
            _logger.Log($"Candidate created: {name}");

            return "Candidate Created Successfully";
        }
    }
}
