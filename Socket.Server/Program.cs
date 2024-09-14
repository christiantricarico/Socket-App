using System.Net;
using System.Net.Sockets;
using System.Text;

Console.WriteLine("Socket server app running ...");

var hostName = Dns.GetHostName();
IPHostEntry localhost = await Dns.GetHostEntryAsync(hostName);
// This is the IP address of the local machine
IPAddress ipAddress = localhost.AddressList[1];

IPEndPoint ipEndPoint = new(ipAddress, 11_000);

using Socket listener = new(
    ipEndPoint.AddressFamily,
    SocketType.Stream,
    ProtocolType.Tcp);

listener.Bind(ipEndPoint);
listener.Listen(100);

Console.WriteLine("Socket server listening ...");

while (true)
{
    var handler = await listener.AcceptAsync();

    while (true)
    {
        try
        {
            // Receive message.
            var buffer = new byte[1_024];
            var received = await handler.ReceiveAsync(buffer, SocketFlags.None);
            var response = Encoding.UTF8.GetString(buffer, 0, received);

            if (string.IsNullOrWhiteSpace(response))
                break;

            Console.WriteLine($"Client says: {response}");
            File.AppendAllText("received.txt", Environment.NewLine + response);

            var eom = "<|EOM|>";
            if (response.IndexOf(eom) > -1 /* is end of message */)
            {
                Console.WriteLine($"Socket server received message: \"{response.Replace(eom, "")}\"");

                var ackMessage = "<|ACK|>";
                var echoBytes = Encoding.UTF8.GetBytes(ackMessage);
                await handler.SendAsync(echoBytes, 0);
                Console.WriteLine(
                    $"Socket server sent acknowledgment: \"{ackMessage}\"");

                break;
            }

            // Sample output:
            //    Socket server received message: "Hi friends 👋!"
            //    Socket server sent acknowledgment: "<|ACK|>"
        }
        catch (SocketException se) when (se.ErrorCode == 10054)
        {
            Console.WriteLine("Client disconnected!");
            Console.WriteLine(se.Message);
            break;
        }
    }
}
