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
        public async Task<ResultDTO> GetMessages(int UserId)
        {
            var messages = await _messageService.GetMessages(UserId);
            return messages;
        }

        [HttpPost]
        [Route("SendMessages")]
        public async Task<ResultDTO> SendMessages(MessageRequestDTO messageRequestDTO)
        {
            return await _messageService.SendMessages(messageRequestDTO);
        }

        [HttpPatch]
        [Route("markAsRead")]
        public async Task<ResultDTO> MarkMessagesAsRead([FromBody]List<int> messageIds)
        {

           return await _messageService.MarkMessagesAsRead(messageIds);

        }

    }
}
