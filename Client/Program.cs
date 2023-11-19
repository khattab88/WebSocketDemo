using System.Net.WebSockets;
using System.Text;

namespace Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Press enter to continue...");
            Console.ReadLine();

            using(ClientWebSocket client = new ClientWebSocket()) 
            {
                Uri serviceUri = new Uri("ws://localhost:5000/send");

                var cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromSeconds(120));

                try
                {
                    await client.ConnectAsync(serviceUri, cts.Token);

                    var i = 0;
                    while (client.State == WebSocketState.Open) 
                    {
                        Console.Out.WriteLine("enter message to send: ");
                        string message = Console.ReadLine();

                        if (!string.IsNullOrEmpty(message))
                        {
                            ArraySegment<byte> byteToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
                            await client.SendAsync(byteToSend, WebSocketMessageType.Text, true, cts.Token);

                            var responseBuffer = new Byte[1024];
                            var offset = 0;
                            var packetSize = 1024;

                            while (true)
                            {
                                ArraySegment<byte> bytesReceived = new ArraySegment<byte>(responseBuffer, offset, packetSize);
                                WebSocketReceiveResult response = await client.ReceiveAsync(bytesReceived, cts.Token);

                                var responseMessage = Encoding.UTF8.GetString(responseBuffer, offset, response.Count);
                                Console.Out.WriteLine(responseMessage);

                                if(response.EndOfMessage)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (WebSocketException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            Console.ReadLine();
        }
    }
}