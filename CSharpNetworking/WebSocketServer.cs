﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CSharpNetworking
{
    public class WebSocketServer : IServer<SocketStream>
    {
        const string HTTP_TERMINATOR = "\r\n\r\n";
        public enum StreamType { Unsecured, SecuredLocalhost, SecuredRemote }
        public StreamType streamType = StreamType.Unsecured;
        public Socket socket;

        public event EventHandler OnListening = delegate { };
        public event EventHandler<SocketStream> OnAccepted = delegate { };
        public event EventHandler<Message<SocketStream>> OnMessage = delegate { };
        public event EventHandler<SocketStream> OnDisconnected = delegate { };
        public event EventHandler OnStopListening = delegate { };
        public event EventHandler<Exception> OnError = delegate { };

        public WebSocketServer(string uriString)
        {
            StartServer(uriString);
        }

        private void StartServer(string uriString)
        {
            var uri = new Uri(uriString);
            var host = uri.Host;
            var port = uri.Port;

            if (uri.Scheme.ToLower() == "wss")
                if (host.ToLower() == "localhost") streamType = StreamType.SecuredLocalhost;
                else if (host.ToLower() != "localhost") streamType = StreamType.SecuredRemote;

            IPEndPoint localEndPoint = null;
            if (host.ToLower() != "any" && host != "*")
            {
                Console.WriteLine($"WebSocketServer: Starting on {host}:{port}...");
                var ipHostInfo = Dns.GetHostEntry(host);
                var ipAddress = ipHostInfo.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).FirstOrDefault();
                localEndPoint = new IPEndPoint(ipAddress, port);
            }
            else
            {
                Console.WriteLine($"WebSocketServer: Starting on IPAddress.Any:{port}...");
                localEndPoint = new IPEndPoint(IPAddress.Any, port);
            }
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(localEndPoint);
            socket.Listen(100);
            OnListening.Invoke(this, null);
            Console.WriteLine($"WebSocketServer: Listening...");
            var doNotWait = AcceptNewClient();
        }

        public void StopServer()
        {
            if (socket != null)
            {
                socket.Close();
                socket.Dispose();
            }
            OnStopListening.Invoke(this, null);
            Console.WriteLine($"WebSocketServer: Stop Listening...");
        }

        private async Task AcceptNewClient()
        {
            Console.WriteLine("WebSocketServer: Waiting for a new client connection...");
            var clientSocket = await socket.AcceptAsync();
            var stream = GetNetworkStream(clientSocket);
            var client = new SocketStream(clientSocket, stream);
            OnAccepted.Invoke(this, client);
            Console.WriteLine($"WebSocketServer: A new client has connected {client.IP}:{client.Port}...");
            StartHandshakeWithClient(client);
            var doNotWait = AcceptNewClient();
        }

        private Stream GetNetworkStream(Socket socket)
        {
            var networkStream = new NetworkStream(socket);
            X509Certificate2 serverCertificate = null;
            byte[] bytes = null;
            SslStream sslStream = null;
            var currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            var path = "";
            try
            {
                switch (streamType)
                {
                    case StreamType.Unsecured:
                        return networkStream;
                    case StreamType.SecuredLocalhost:
                        path = Path.Combine(currentDirectory, "localhost.pfx");
                        Console.WriteLine($"Reading certificate from {path}");
                        bytes = File.ReadAllBytes(path);
                        serverCertificate = new X509Certificate2(bytes, "1234");
                        Console.WriteLine("Certificate Read [localhost]...");
                        sslStream = new SslStream(networkStream);
                        sslStream.AuthenticateAsServer(serverCertificate,
                                enabledSslProtocols: SslProtocols.Tls,
                                clientCertificateRequired: false,
                                checkCertificateRevocation: false);
                        return sslStream;
                    case StreamType.SecuredRemote:
                        path = Path.Combine(currentDirectory, "letsencrypt.pfx");
                        Console.WriteLine($"Reading certificate from {path}");
                        bytes = File.ReadAllBytes(path);
                        serverCertificate = new X509Certificate2(bytes, "1234");
                        Console.WriteLine("Certificate Read [letsencrypt]...");
                        sslStream = new SslStream(networkStream);
                        sslStream.AuthenticateAsServer(serverCertificate,
                                enabledSslProtocols: SslProtocols.Tls,
                                clientCertificateRequired: false,
                                checkCertificateRevocation: false);
                        return sslStream;
                    default:
                        throw new Exception("Invalid stream type");
                }
            }
            catch (Exception ex) { Console.WriteLine(ex); return null; }
        }

        private async void StartHandshakeWithClient(SocketStream client)
        {
            try
            {
                var message = "";
                while (client.socket.Connected)
                {
                    var buffer = new byte[2048];
                    var bytesRead = await client.stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    message += Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    if (message.Contains(HTTP_TERMINATOR))
                    {
                        Console.WriteLine($"WebSocketServer: A HandShake received from {client.IP}:{client.Port}...");
                        //var messages = message.Split(new[] { HTTP_TERMINATOR }, StringSplitOptions.RemoveEmptyEntries);
                        if (Regex.IsMatch(message, "^GET", RegexOptions.IgnoreCase))
                        {
                            // 1. Obtain the value of the "Sec-WebSocket-Key" request header without any leading or trailing whitespace
                            // 2. Concatenate it with "258EAFA5-E914-47DA-95CA-C5AB0DC85B11" (a special GUID specified by RFC 6455)
                            // 3. Compute SHA-1 and Base64 hash of the new value
                            // 4. Write the hash back as the value of "Sec-WebSocket-Accept" response header in an HTTP response
                            string swk = Regex.Match(message, "Sec-WebSocket-Key: (.*)").Groups[1].Value.Trim();
                            string swka = swk + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
                            byte[] swkaSha1 = System.Security.Cryptography.SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(swka));
                            string swkaSha1Base64 = Convert.ToBase64String(swkaSha1);

                            // HTTP/1.1 defines the sequence CR LF as the end-of-line marker
                            var outgoingMessage = "HTTP/1.1 101 Switching Protocols\r\n" +
                                "Connection: Upgrade\r\n" +
                                "Upgrade: websocket\r\n" +
                                "Sec-WebSocket-Accept: " + swkaSha1Base64 + "\r\n\r\n";
                            byte[] response = Encoding.UTF8.GetBytes(outgoingMessage);
                            Console.WriteLine($"WebSocketServer: Replying to HandShake for {client.IP}:{client.Port}...");
                            await client.stream.WriteAsync(response, 0, response.Length);
                        }
                        else
                            throw new Exception("WebSocketServer: Incoming websocket handshake message was not in the right format.");

                        StartReceivingFromClient(client);
                        break;
                    }
                }
            }
            catch (Exception exception)
            {
                OnError.Invoke(this, exception);
                Disconnect(client);
            }
        }

        private async void StartReceivingFromClient(SocketStream client)
        {
            try
            {
                var received = new List<byte>();
                var buffer = new byte[2048];
                while (client.socket.Connected)
                {
                    var bytesRead = await client.stream.ReadAsync(buffer, 0, buffer.Length);
                    var incomingBytes = buffer.Take(bytesRead);
                    received.AddRange(incomingBytes);

                    if (!WebSocket.IsDiconnectPacket(incomingBytes))
                    {
                        while (received.Count >= WebSocket.PacketLength(received))
                        {
                            var message = WebSocket.BytesToString(received.ToArray());
                            received.RemoveRange(0, (int)WebSocket.PacketLength(received));
                            OnMessage.Invoke(this, new Message<SocketStream>(client, message));
                            Console.WriteLine($"WebSocketServer: Received from {client.IP}:{client.Port}: {message}");
                        }
                    }
                    else break; // aka disconnect
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
            finally
            {
                Disconnect(client);
            }
        }

        private void Disconnect(SocketStream client)
        {
            try
            {
                if (client.socket.Connected) client.socket.Disconnect(false);
                OnDisconnected.Invoke(this, client);
                Console.WriteLine($"WebSocketServer: Client {client.IP}:{client.Port} disconnected normally.");
            }
            catch (Exception exception)
            {
                DisconnectError(exception, client);
            }
        }

        private void DisconnectError(Exception exception, SocketStream client)
        {
            OnError.Invoke(this, exception);
            OnDisconnected.Invoke(this, client);
            Console.WriteLine($"WebSocketServer: Client {client.IP}:{client.Port} unexpectadely disconnected. {exception.Message}");
        }

        public void Send(SocketStream client, string message)
        {
            var doNotWait = SendAsync(client, message);
        }

        public static async Task SendAsync(SocketStream client, string message)
        {
            var bytes = WebSocket.StringToBytes(message, false);
            await client.stream.WriteAsync(bytes, 0, bytes.Length);
            Console.WriteLine($"WebSocket Server: Sent to {client.IP}:{client.Port}: {message}");
        }
    }
}
