using ChatPlatform.Application.DTOs.Messages;
using ChatPlatform.Application.IServices;
using ChatPlatform.Domain;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace ChatPlatform.Api.RealTime
{
    public class WebSocketConnectionHandler
    {
        private readonly ConnectionManager _connectionManager;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<WebSocketConnectionHandler> _logger;

        public WebSocketConnectionHandler(ConnectionManager connectionManager, IServiceScopeFactory scopeFactory, ILogger<WebSocketConnectionHandler> logger)
        {
            _connectionManager = connectionManager;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task HandleAsync(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Expected websocket request");
                return;
            }

            if(!Guid.TryParse(context.Request.Query["userId"], out Guid userId) )
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Invalid User Id");
                return;
            }

            WebSocket socket = await context.WebSockets.AcceptWebSocketAsync(); // handshake
            _connectionManager.AddConnection(userId, socket);

            try
            {
                while(socket.State == WebSocketState.Open) 
                {
                    byte[] buffer = new byte[4*1024];
                    MemoryStream stream = new MemoryStream();

                    while (true)
                    {
                        var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), default);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", default);
                        }
                        stream.Write(buffer,0,result.Count);

                        if (result.EndOfMessage)
                            break;
                    }

                    string json = Encoding.UTF8.GetString(stream.ToArray());

                    IncomingEnvelope? incommingEnvelope = JsonSerializer.Deserialize<IncomingEnvelope>(json,
                             new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (incommingEnvelope is null)
                        continue;

                    using IServiceScope scope = _scopeFactory.CreateScope();
                    

                    if(incommingEnvelope.Type == "new_message")
                    {
                        IMessageService messageService = scope.ServiceProvider.GetRequiredService<IMessageService>();
                        MessageRequest message = new MessageRequest
                        {
                            ChatRoomId = incommingEnvelope.ChatRoomId,
                            ReceiverId = incommingEnvelope.ReceiverId,
                            SenderId = userId,
                            Content = incommingEnvelope.Content ?? string.Empty

                        };

                        await messageService.SendMessageAsync(message);
                        continue;

                    }

                    if (incommingEnvelope.Type == "mark_as_read" && incommingEnvelope.ChatRoomId.HasValue)
                    {
                        IChatRoomService chatRoomService = scope.ServiceProvider.GetRequiredService<IChatRoomService>();
                        await chatRoomService.MarkAsReadAsync(incommingEnvelope.ChatRoomId.Value, userId);
                        continue;

                    }

                    if (incommingEnvelope.Type == "typing" && incommingEnvelope.ChatRoomId.HasValue)
                    {
                        IChatRoomService chatRoomService = scope.ServiceProvider.GetRequiredService<IChatRoomService>();
                        await chatRoomService.NotifyTypingAsync(incommingEnvelope.ChatRoomId.Value, userId, incommingEnvelope.IsTyping);
                    }



                }
            }

            catch (Exception ex)
            {
                if (socket.State == WebSocketState.Open)
                {
                    _logger.LogError(ex, "WebSocket receive loop crashed for user {UserId}", userId);

                    if (socket.State == WebSocketState.Open)
                        await socket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Server error", CancellationToken.None);
                }
            }

            finally
            {
                _connectionManager.RemoveConnection(userId, socket);
            }


        }
    }
}
