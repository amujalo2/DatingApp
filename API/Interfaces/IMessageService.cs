using System;
using API.DTOs;
using API.Helpers;

namespace API.Interfaces;

public interface IMessageService
{
    Task<MessageDto> CreateMessage(CreateMessageDto createMessageDto, string username);
    Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams, string username);
    Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername);
    Task DeleteMessage(int id, string username);
}
