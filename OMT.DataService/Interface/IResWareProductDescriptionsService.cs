using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OMT.DTO;

namespace OMT.DataService.Interface
{
    public interface IResWareProductDescriptionsService
    {
        ResultDTO GetResWareProductDescriptions();
        //ResultDTO GetResWareProductDescriptionsMap(int productId);
        //ResultDTO CreateResWareProductDescriptions(ResWareProductDescriptionsDTO resWareProductDescriptionsDTO);
        //ResultDTO UpdateResWareProductDescriptions();
        //ResultDTO DeleteResWareProductDescriptions(int rprodid);
    }
}
