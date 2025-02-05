using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.APIHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessageController(IMessageService messageService)
        {
            _messageService = messageService;
        }


        [HttpGet]
        [Route("GetMessages")]
        public async Task<ResultDTO> GetMessages(int SenderId,int ReceiverId)
        {
            var messages = await _messageService.GetMessages(SenderId, ReceiverId);
            return messages;
        }


        [HttpPost]
        [Route("SendMessages")]
        public async Task<ResultDTO> SendMessages(MessageRequestDTO messageRequestDTO)
        {
            return await _messageService.SendMessages(messageRequestDTO);
        }

        [HttpPatch]
        [Route("UpdateMessages")]
        public async Task<ResultDTO> UpdateMessages(MessageUpdateDTO messageUpdate)
        {

           return await _messageService.UpdateMessages(messageUpdate);

        }

        [HttpGet]
        [Route("GetUpdateMessageStatus")]
        public async Task<ResultDTO> GetUpdateMessageStatus(int ReceiverId)
        {
            return await _messageService.GetUpdateMessageStatus(ReceiverId);
        }

        [HttpGet]
        [Route("GetLatestMessage")]
        public async Task<ResultDTO> GetLatestMessage(int ReceiverId)
        {
            return await _messageService.GetLatestMessage(ReceiverId);
        }

    }
}
