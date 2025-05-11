using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using API.Helpers._Message;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class MessagesController : BaseApiController
{
    private readonly MessageHelper _messageHelper;
    private readonly IUnitOfWork _unitOfWork;

    public MessagesController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _messageHelper = new MessageHelper(unitOfWork, mapper);
        _unitOfWork = unitOfWork;
    }

    [HttpPost]
    public async Task<IActionResult> CreateMessage(CreateMessageDto createMessageDto)
    {
        var username = User.GetUsername();
        var (success, errorMessage, message) = await _messageHelper.CreateMessage(createMessageDto, username);
        
        if (!success) return BadRequest(errorMessage);
        
        return Ok(message);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery]MessageParams messageParams)
    {
        var messages = await _messageHelper.GetMessagesForUser(messageParams, User.GetUsername());
        Response.AddPaginationHeader(messages);
        return messages;
    }

    [HttpGet("thread/{username}")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
    {
        var currentUsername = User.GetUsername();
        return Ok(await _messageHelper.GetMessageThread(currentUsername, username));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(int id)
    {
        var username = User.GetUsername();
        var (success, errorMessage) = await _messageHelper.DeleteMessage(id, username);
        
        if (!success) return BadRequest(errorMessage);
        
        return Ok();
    }
}