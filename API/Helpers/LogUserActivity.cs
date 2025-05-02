using System;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Helpers;

public class LogUserActivity : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var resulContext = await next();
        if (context.HttpContext.User.Identity?.IsAuthenticated != true) return;
        var userId = resulContext.HttpContext.User.GetUserId();
        var repo = resulContext.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
        var user = await repo.GetUserByIdAsync(userId);
        if(user == null) return;
        user.LastActive = DateTime.UtcNow;
        await repo.SaveAllAsync();
    }
}
