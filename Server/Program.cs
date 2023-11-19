using System.Net;
using System.Net.WebSockets;
using System.Text;

namespace Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            app.MapGet("/", () => "Hello World!");

            var wsOptions = new WebSocketOptions 
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
            };

            app.UseWebSockets(wsOptions);
            app.Run(async (context) => 
            {
                if(context.Request.Path == "/send")
                {
                    if(context.WebSockets.IsWebSocketRequest) 
                    {
                        using(WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync()) 
                        {
                            await Send(context, webSocket);
                        }
                    }
                    else
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    }
                }
            });

            app.Run();
        }

        private static async Task Send(HttpContext context, WebSocket webSocket)
        {
            var buffer = new byte[1024*4];

            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if(result != null)
            {
                while(!result.CloseStatus.HasValue) 
                {
                    string message = Encoding.UTF8.GetString(new ArraySegment<byte>(buffer, 0, result.Count));

                    Console.Out.WriteLine($"client says: {message}");

                    await webSocket.SendAsync(
                        new ArraySegment<byte>(Encoding.UTF8.GetBytes($"server says: {DateTime.UtcNow:f}")),
                        result.MessageType,
                        result.EndOfMessage,
                        CancellationToken.None);

                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    // Console.Out.WriteLine(result);
                }
            }

            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }
}