using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Errors;
using API.Helpers;
using API.Interfaces;
using AutoMapper;

namespace API.Services._Message;

public class MessageService(IUnitOfWork unitOfWork, IMapper mapper)
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<MessageDto> CreateMessage(CreateMessageDto createMessageDto, string username)
    {
        if (username == createMessageDto.RecipientUsername.ToLower())
            throw new BadRequestException("You cannot message yourself");
        
        var sender = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
        var recipient = await _unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

        if (recipient == null || sender == null || sender.UserName == null || recipient.UserName == null) 
            throw new BadRequestException("User not found");

        var message = new Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = sender.UserName,
            RecipientUsername = recipient.UserName,
            Content = createMessageDto.Content
        };

        _unitOfWork.MessageRepository.AddMessage(message);

        if (!await _unitOfWork.Complete()) 
            throw new Exception("Failed to save message");

        return _mapper.Map<MessageDto>(message);
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

    public async Task DeleteMessage(int id, string username)
    {
        var message = await _unitOfWork.MessageRepository.GetMessage(id) ?? throw new NotFoundException("Message not found");
        
        if (message.SenderUsername != username && message.RecipientUsername != username) 
            throw new UnauthorizedAccessException("You are not authorized to delete this message");

        if (message.SenderUsername == username) 
            message.SenderDeleted = true;

        if (message.RecipientUsername == username) 
            message.RecipientDeleted = true;

        if (message is {SenderDeleted: true, RecipientDeleted: true}) 
        {
            _unitOfWork.MessageRepository.DeleteMessage(message);
        }

        if (!await _unitOfWork.Complete()) 
            throw new Exception("Problem deleting message");
    }
}