using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using API.Services._Message;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class MessagesController(IUnitOfWork unitOfWork, IMapper mapper, ILogger<MessagesController> logger) : BaseApiController
{
    private readonly MessageService _messageHelper = new MessageService(unitOfWork, mapper);
    private readonly ILogger<MessagesController> _logger = logger;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="createMessageDto"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> CreateMessage(CreateMessageDto createMessageDto)
    {
        try
        {
            var username = User.GetUsername();
            var message = await _messageHelper.CreateMessage(createMessageDto, username);
            return Ok(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in MessagesController.CreateMessage");
            throw;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="messageParams"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery]MessageParams messageParams)
    {
        try
        {
            var messages = await _messageHelper.GetMessagesForUser(messageParams, User.GetUsername());
            Response.AddPaginationHeader(messages);
            return Ok(messages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in MessagesController.GetMessagesForUser");
            throw;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    [HttpGet("thread/{username}")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
    {
        try
        {
            var currentUsername = User.GetUsername();
            return Ok(await _messageHelper.GetMessageThread(currentUsername, username));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in MessagesController.GetMessageThread");
            throw;
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(int id)
    {
        try
        {
            var username = User.GetUsername();
            await _messageHelper.DeleteMessage(id, username);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in MessagesController.DeleteMessage");
            throw;
        }
    }
}