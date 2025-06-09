using API.Extensions;
using API.Services;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Helpers;

public class LogUserActivity : IAsyncActionFilter
{
    private readonly UserServices _userServices;

    public LogUserActivity(UserServices userService)
    {
        _userServices = userService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var resultContext = await next();
        if (context.HttpContext.User.Identity?.IsAuthenticated != true) return;

        var userId = resultContext.HttpContext.User.GetUserId();
        await _userServices.UpdateLastActive(userId);
    }
}