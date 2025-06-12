using System;

namespace API.Helpers;

public class MessageParams : PaginationParams
{
    public required string? Username { get; set; }
    public MessageContainer Container { get; set; } = MessageContainer.Unread;
}