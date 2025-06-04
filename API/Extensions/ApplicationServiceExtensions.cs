using System;
using API.Data;
using API.Helpers;
using API.Interfaces;
using API.Services;
using API.Services._Account;
using API.Services._Admin;
using API.Services._Likes;
using API.Services._Message;
using API.Services._User;
using API.SignalR;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddControllers();
        services.AddApplicationInsightsTelemetry();
        services.AddDbContext<DataContext>(opt =>
        {
            opt.UseSqlServer(config.GetConnectionString("DefaultConnection"));
        });
        services.AddCors();
        // services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPhotoService, PhotoService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<ILikesService, LikesService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<UserServices>();
        // repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ILikesRepository, LikesRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<ITagsRepository, TagsRepository>();
        services.AddScoped<IPhotoRepository, PhotoRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<LogUserActivity>();
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));
        services.AddSignalR();
        services.AddSingleton<PresenceTracker>();
        return services;
    }

}
