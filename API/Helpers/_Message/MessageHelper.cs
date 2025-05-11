using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;

namespace API.Helpers._Message;

public class MessageHelper
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public MessageHelper(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<(bool Success, string? ErrorMessage, MessageDto? Message)> CreateMessage(CreateMessageDto createMessageDto, string username)
    {
        if (username == createMessageDto.RecipientUsername.ToLower())
        {
            return (false, "You cannot message yourself", null);
        }

        var sender = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
        var recipient = await _unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

        if (recipient == null || sender == null || sender.UserName == null || recipient.UserName == null) 
            return (false, "Cannot send message at this time", null);

        var message = new Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = sender.UserName,
            RecipientUsername = recipient.UserName,
            Content = createMessageDto.Content
        };

        _unitOfWork.MessageRepository.AddMessage(message);

        if (await _unitOfWork.Complete()) 
            return (true, null, _mapper.Map<MessageDto>(message));

        return (false, "Failed to save message", null);
    }

    public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams, string username)
    {
        messageParams.Username = username;
        return await _unitOfWork.MessageRepository.GetMessageForUser(messageParams);
    }

    public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
    {
        return await _unitOfWork.MessageRepository.GetMessageThread(currentUsername, recipientUsername);
    }

    public async Task<(bool Success, string? ErrorMessage)> DeleteMessage(int id, string username)
    {
        var message = await _unitOfWork.MessageRepository.GetMessage(id);

        if (message == null) 
            return (false, "Cannot delete this message");

        if (message.SenderUsername != username && message.RecipientUsername != username) 
            return (false, "Unauthorized to delete this message");

        if (message.SenderUsername == username) 
            message.SenderDeleted = true;

        if (message.RecipientUsername == username) 
            message.RecipientDeleted = true;

        if (message is {SenderDeleted: true, RecipientDeleted: true}) 
        {
            _unitOfWork.MessageRepository.DeleteMessage(message);
        }

        if (await _unitOfWork.Complete()) 
            return (true, null);

        return (false, "Problem deleting message!");
    }
}