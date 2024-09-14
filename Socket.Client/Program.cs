using System.Net;
using System.Net.Sockets;
using System.Text;

Console.WriteLine("Socket client app running ...");

var hostName = Dns.GetHostName();
IPHostEntry localhost = await Dns.GetHostEntryAsync(hostName);
// This is the IP address of the local machine
IPAddress ipAddress = localhost.AddressList[1];

IPEndPoint ipEndPoint = new(ipAddress, 11_000);

using Socket client = new(
    ipEndPoint.AddressFamily,
    SocketType.Stream,
    ProtocolType.Tcp);

await client.ConnectAsync(ipEndPoint);

Console.WriteLine($"Socket connected. IP:{ipEndPoint.Address.ToString()}:{ipEndPoint.Port}");
Console.WriteLine("Enter message to send to server. Enter CIAO to exit.");

while (true)
{
    var message = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(message))
        continue;

    if (message.ToUpper() == "CIAO")
        break;

    // Send message.
    //var message = "Hi friends 👋!<|EOM|>";
    var messageBytes = Encoding.UTF8.GetBytes(message);
    var sentBytes = await client.SendAsync(messageBytes, SocketFlags.None);
    //Console.WriteLine($"Socket client sent message: \"{message}\"");

    // Receive ack.
    //var buffer = new byte[1_024];
    //var received = await client.ReceiveAsync(buffer, SocketFlags.None);
    //var response = Encoding.UTF8.GetString(buffer, 0, received);
    //if (response == "<|ACK|>")
    //{
    //    Console.WriteLine(
    //        $"Socket client received acknowledgment: \"{response}\"");
    //    break;
    //}

    // Sample output:
    //     Socket client sent message: "Hi friends 👋!<|EOM|>"
    //     Socket client received acknowledgment: "<|ACK|>"
}

client.Shutdown(SocketShutdown.Both);