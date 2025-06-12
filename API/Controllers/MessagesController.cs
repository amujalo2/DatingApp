using API.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using API.Services._Message;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace API.Controllers;

[Authorize]
public class MessagesController(IMessageService messageService, ILogger<MessagesController> logger) : BaseApiController
{
    private readonly IMessageService _messageHelper = messageService;
    private readonly ILogger<MessagesController> _logger = logger;

    /// <summary>
    /// POST /api/messages
    /// Creates a new message.
    /// </summary>
    /// <param name="createMessageDto"></param>
    /// <returns></returns>
    /// [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<IActionResult> CreateMessage(CreateMessageDto createMessageDto)
    {
        try
        {
            _logger.LogDebug($"MessagesController - {nameof(CreateMessage)} invoked. (createMessageDto: {createMessageDto})");
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
    /// GET /api/messages?container={messageParams}
    /// Retrieves messages for the current user based on the specified parameters.
    /// </summary>
    /// <param name="messageParams"></param>
    /// <returns></returns>
    /// [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(ActionResult<IEnumerable<MessageDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
    {
        try
        {
            _logger.LogDebug($"MessagesController - {nameof(GetMessagesForUser)} invoked. (messageParams: {messageParams})");
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
    /// GET /api/messages/thread/{username}
    /// Retrieves the message thread between the current user and the specified username.
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    /// [AllowAnonymous]
    [HttpGet("thread/{username}")]
    [ProducesResponseType(typeof(ActionResult<IEnumerable<MessageDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
    {
        try
        {
            _logger.LogDebug($"MessagesController - {nameof(GetMessageThread)} invoked. (username: {username})");
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
    /// DELETE /api/messages/{id}
    /// Deletes a message by its ID for the current user.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// [AllowAnonymous]
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult> DeleteMessage(int id)
    {
        try
        {
            _logger.LogDebug($"MessagesController - {nameof(DeleteMessage)} invoked. (id: {id})");
            var username = User.GetUsername();
            await _messageHelper.DeleteMessage(id, username);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in MessagesController.DeleteMessage");
            throw;
        }
    }
}