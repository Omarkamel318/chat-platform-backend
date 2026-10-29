
using ChatPlatform.Api.Middlewares;
using ChatPlatform.Api.RealTime;
using ChatPlatform.Application;
using ChatPlatform.Application.IServices;
using ChatPlatform.Application.Services;
using ChatPlatform.Infrastructure;
using ChatPlatform.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace ChatPlatform.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddCors(option =>
            {
                option.AddPolicy("default", policy =>
                {
                    policy
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
                    
                });
            });

            builder.Services.AddDbContext<ChatDbContext>(option =>
            {
                option.UseSqlServer(builder.Configuration.GetConnectionString("default"));
            });

            builder.Services.AddScoped<IUnitOfWork,UnitOfWork>();
            builder.Services.AddScoped<IChatRoomService,ChatRoomService>();
            builder.Services.AddScoped<IMessageService,MessageService>();
            builder.Services.AddScoped<IUserService, UserService>();

            builder.Services.AddSingleton<ConnectionManager>();
            builder.Services.AddSingleton<IRealtimeNotifier, WebSocketNotifier>();
            builder.Services.AddSingleton<WebSocketConnectionHandler>();

            var app = builder.Build();

            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseWebSockets();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors("default");

            app.UseAuthorization();

            app.Map("/ws/chat", async context =>
            {
                WebSocketConnectionHandler handler = context.RequestServices.GetRequiredService<WebSocketConnectionHandler>();
                await handler.HandleAsync(context);
            });


            app.MapControllers();

            app.Run();
        }
    }
}
