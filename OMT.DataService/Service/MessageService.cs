using Azure.Core;
using Microsoft.EntityFrameworkCore;
using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DataService.Utility;
using OMT.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataService.Service
{
    public class MessageService:IMessageService
    {
        private readonly OMTDataContext _oMTDataContext;
        public MessageService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }

        private bool ValidateEntityUserAsync<T>(DbSet<T> dbSet, int id) where T : class
        {
            return dbSet.Any(entity => EF.Property<int>(entity, "UserId") == id);
        }
        private string ValidateMessage(MessageRequestDTO messageRequestDTO)
        {
            if (string.IsNullOrWhiteSpace(messageRequestDTO.ChatMessage))
                return "NotificationMessage cannot be empty.";
            if (messageRequestDTO.SenderId <= 0 || messageRequestDTO.ReceiverId <= 0)
                return "Invalid UserId.";
            if (!ValidateEntityUserAsync(_oMTDataContext.UserProfile, messageRequestDTO.SenderId) || !ValidateEntityUserAsync(_oMTDataContext.UserProfile, messageRequestDTO.ReceiverId))
                return "Invalid UserId.";
            return string.Empty;
        }
        public Task<ResultDTO> GetMessages(int UserId)
        {

            var resultDTO = new ResultDTO { IsSuccess = true, StatusCode = "200" };

            try
            {
                var existingMessages = _oMTDataContext.Message.Where(x => x.SenderId == UserId || x.SenderId == UserId).ToList();
                if (existingMessages.Any())
                {
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Fetched specific user's messages successfully.";
                    resultDTO.Data = existingMessages;  // Assuming ResultDTO has a Data property to hold the messages
                    resultDTO.StatusCode = "200";
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.StatusCode = "404";
                    resultDTO.Message = "No messages found for the specified user.";
                }
                
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = $"An error occurred: {ex.Message}";
            }

            return Task.FromResult(resultDTO);
        }


        public async Task<ResultDTO> SendMessages(MessageRequestDTO messageRequestDTO)
        {
            var validationError = ValidateMessage(messageRequestDTO);
            if (!string.IsNullOrEmpty(validationError))
            {
                return new ResultDTO
                {
                    IsSuccess = false,
                    StatusCode = "400",
                    Message = validationError
                };
            }
            var resultDTO = new ResultDTO { IsSuccess = true, StatusCode = "200" };

            try
            {
                var sendMessage = new Message
                {
                    SenderId = messageRequestDTO.SenderId,
                    ReceiverId = messageRequestDTO.ReceiverId,
                    ChatMessage = messageRequestDTO.ChatMessage
                };

                await _oMTDataContext.Message.AddAsync(sendMessage);
                await _oMTDataContext.SaveChangesAsync();

                return new ResultDTO
                {
                    IsSuccess = true,
                    StatusCode = "201",
                    Message = "Message sent successfully."
                };
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = $"An error occurred: {ex.Message}";
            }

            return await Task.FromResult(resultDTO);
        }


        public async Task<ResultDTO> MarkMessagesAsRead(List<int> messageIds)
        {
            var resultDTO = new ResultDTO { IsSuccess = true, StatusCode = "200" };
            try
            {
                foreach (var messageId in messageIds)
                {

                    var existingMessage = _oMTDataContext.Message.Where(x => x.MessageId == messageId).FirstOrDefault();
                    if (existingMessage != null)
                    {
                        existingMessage.IsRead = true;
                        resultDTO.IsSuccess = true;
                        resultDTO.Message = "Messages marked as read.";
                        resultDTO.StatusCode = "201";
                    }
                    else
                    {
                        resultDTO.IsSuccess = false;
                        resultDTO.StatusCode = "404";
                        resultDTO.Message = "No messages found";
                    }
                }
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = $"An error occurred: {ex.Message}";
            }

            return await Task.FromResult(resultDTO);
        }
    }
}
