using OMT.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataService.Interface
{
    public interface IMessageService
    {
        Task<ResultDTO> GetMessages(int SenderId, int ReceiverId);
        Task<ResultDTO> SendMessages(MessageRequestDTO messageRequestDTO);
        Task<ResultDTO> UpdateMessages(MessageUpdateDTO messageUpdate);
        Task<ResultDTO> GetUpdateMessageStatus(int ReceiverId);
    }
}
