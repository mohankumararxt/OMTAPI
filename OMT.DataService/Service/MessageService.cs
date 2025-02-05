using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
        private async Task<bool> StatusOperations<T>(DbSet<T> dbSet, int status) where T : class
        {
            return await dbSet.AnyAsync(entity => EF.Property<int>(entity, "Id") == status);
        }
        private async Task<string> ValidateMessageStatus(int status)
        {
            if (!await StatusOperations(_oMTDataContext.MessagesStatus, status))
                return "Invalid Status";
            return string.Empty;
        }


        private bool ValidateEntityUserAsync<T>(DbSet<T> dbSet, int id) where T : class
        {
            return dbSet.Any(entity => EF.Property<int>(entity, "UserId") == id);
        }
        private string ValidateMessage(MessageRequestDTO messageRequestDTO)
        {
            if (string.IsNullOrWhiteSpace(messageRequestDTO.ChatMessage))
                return "Message cannot be empty.";
            if (messageRequestDTO.SenderId <= 0 || messageRequestDTO.ReceiverId <= 0)
                return "Invalid UserId.";
            //if (!ValidateEntityUserAsync(_oMTDataContext.UserProfile, messageRequestDTO.SenderId) || !ValidateEntityUserAsync(_oMTDataContext.UserProfile, messageRequestDTO.ReceiverId))
            //    return "Invalid UserId.";
            return string.Empty;
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
                    Message = "Message sent successfully.",
                    Data = new
                    {
                        MessageId = sendMessage.MessageId,
                        SenderId = sendMessage.SenderId,
                        ReceiverId = sendMessage.ReceiverId,
                        ChatMessage = sendMessage.ChatMessage
                    }
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


        //public async Task<ResultDTO> UpdateMessages( MessageUpdateDTO messageUpdate)
        //{
        //    var resultDTO = new ResultDTO { IsSuccess = true, StatusCode = "200" };
        //    try
        //    {
        //        var validationErrors = await ValidateMessageStatus(messageUpdate.Status);
        //        if (!string.IsNullOrEmpty(validationErrors))
        //        {
        //            return new ResultDTO
        //            {
        //                IsSuccess = false,
        //                StatusCode = "400",
        //                Message = validationErrors
        //            };
        //        }

        //        var status = _oMTDataContext.MessagesStatus.Where(x => x.Id == messageUpdate.Status).Select(t=> t.MessageStatus).FirstOrDefault();
        //        var existingMessage = _oMTDataContext.Message.Where(x => x.MessageId == messageUpdate.Id).FirstOrDefault();
        //        if (existingMessage != null)
        //        {
        //            existingMessage.MessageStatus = messageUpdate.Status;
        //            _oMTDataContext.Message.Update(existingMessage);
        //            _oMTDataContext.SaveChanges();
        //            resultDTO.IsSuccess = true;
        //            resultDTO.Data = new
        //            {
        //                MessageId = existingMessage.MessageId,
        //                SenderId = existingMessage.SenderId,
        //                ReceiverId = existingMessage.ReceiverId,
        //                MessageStatus = existingMessage.MessageStatus
        //            };
        //            resultDTO.Message = $"Messages marked as {status}.";
        //            resultDTO.StatusCode = "201";
        //        }
        //        else
        //        {
        //            resultDTO.IsSuccess = false;
        //            resultDTO.StatusCode = "404";
        //            resultDTO.Message = "No messages found";
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        resultDTO.IsSuccess = false;
        //        resultDTO.StatusCode = "500";
        //        resultDTO.Message = $"An error occurred: {ex.Message}";
        //    }

        //    return await Task.FromResult(resultDTO);
        //}

        public async Task<ResultDTO> UpdateMessages(MessageUpdateDTO messageUpdate)
        {
            var resultDTO = new ResultDTO { IsSuccess = true, StatusCode = "200" };

            try
            {
              

                // Fetch all messages in one query
                var messagesToUpdate = await _oMTDataContext.Message
                    .Where(m =>  messageUpdate.MessageIds.Contains(m.MessageId))
                    .ToListAsync();

                if (messagesToUpdate.Any()) // Check if there are messages to update
                {
                    foreach (var message in messagesToUpdate)
                    {
                        message.MessageStatus = messageUpdate.OutputMessageStatus; // Update status
                    }

                    await _oMTDataContext.SaveChangesAsync(); // Save changes in one batch

                    resultDTO.IsSuccess = true;
                    resultDTO.Data = messagesToUpdate.Select(msg => new
                    {
                        MessageId = msg.MessageId,
                        SenderId = msg.SenderId,
                        ReceiverId = msg.ReceiverId,
                        MessageStatus = msg.MessageStatus
                    }).ToList();

                    resultDTO.Message = $"Messages updated to status {messageUpdate.OutputMessageStatus}.";
                    resultDTO.StatusCode = "201";
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.StatusCode = "404";
                    resultDTO.Message = "No matching messages found for update.";
                }
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = $"An error occurred: {ex.Message}";
            }

            return resultDTO;
        }



        public Task<ResultDTO> GetMessages(int SenderId, int ReceiverId)
        {
            var resultDTO = new ResultDTO { IsSuccess = true, StatusCode = "200" };

            try
            {
                var existingMessages = _oMTDataContext.Message.Where(x => (x.SenderId == SenderId || x.SenderId ==  ReceiverId) && (x.ReceiverId == ReceiverId || x.ReceiverId == SenderId)).OrderBy(x=>x.CreateTimeStamp).ToList();
                if (existingMessages.Any())
                {
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Fetched specific user's messages successfully.";
                    resultDTO.Data = existingMessages;  // Assuming ResultDTO has a Data property to hold the messages
                    resultDTO.StatusCode = "200";
                }
                else
                {
                    resultDTO.IsSuccess = true;
                    resultDTO.StatusCode = "200";
                    resultDTO.Message = "No messages found.";
                    resultDTO.Data = existingMessages;
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

        public Task<ResultDTO> GetUpdateMessageStatus(int ReceiverId)
        {
            var resultDTO = new ResultDTO { IsSuccess = true, StatusCode = "200" };

            try
            {
                var existingMessages = _oMTDataContext.Message.Where(x => x.ReceiverId == ReceiverId).OrderBy(x => x.CreateTimeStamp).ToList();
                if (existingMessages.Any())
                {
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Fetched specific receiver's messages status successfully.";
                    resultDTO.Data = existingMessages;  // Assuming ResultDTO has a Data property to hold the messages
                    resultDTO.StatusCode = "200";
                }
                else
                {
                    resultDTO.IsSuccess = true;
                    resultDTO.StatusCode = "200";
                    resultDTO.Message = "No messages found.";
                    resultDTO.Data = existingMessages;
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

        public Task<ResultDTO> GetLatestMessage(int ReceiverId)
        {
            var resultDTO = new ResultDTO { IsSuccess = true, StatusCode = "200" };

            try
            {
                var existingMessages = _oMTDataContext.Message
                    .Where(x => x.ReceiverId == ReceiverId)
                    .OrderByDescending(x => x.CreateTimeStamp)
                    .ToList();

                var existingSenderMessages = _oMTDataContext.Message
                    .Where(x => x.SenderId == ReceiverId)
                    .OrderByDescending(x => x.CreateTimeStamp)
                    .ToList();

                // Combine both lists
                var allMessages = existingMessages.Concat(existingSenderMessages).ToList();

                // Get latest message per unique sender-receiver pair (bi-directional check)
                var latestMessages = allMessages
                    .GroupBy(x => new { MinId = Math.Min(x.SenderId, x.ReceiverId), MaxId = Math.Max(x.SenderId, x.ReceiverId) }) // Group sender & receiver together
                    .Select(g => g.OrderByDescending(x => x.CreateTimeStamp).First()) // Take latest message
                    .ToList();
                resultDTO.IsSuccess = true;
                resultDTO.Data = latestMessages;
                resultDTO.Message = "Fetch successfully..";
                resultDTO.StatusCode = "200";

            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = $"An error occurred: {ex.Message}";
            }

            return Task.FromResult(resultDTO);
        }

    }
}
