using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using PrinterManager.Helpers;
using PrinterManager.Printing;
using PrinterManager.Database;
using Newtonsoft.Json;
using PrinterManager.Database.Repositories;
using System.Net.Security;
using System.Text.RegularExpressions;

namespace PrinterManager.Server
{
    public class TcpServerAsyncWebsockets
    {
        IPAddress host = IPAddress.Parse("127.0.0.1");
        const int PORT = 1516;
        private readonly TcpListener server;

        public TcpServerAsyncWebsockets()
        {
            this.server = new TcpListener(host, PORT);
            DatabaseSetup databaseAccess = new DatabaseSetup();
        }

        public void ServerStart()
        {
            server.Start();
            AcceptConnection();
        }

        private void AcceptConnection()
        {
            server.BeginAcceptTcpClient(HandleConnection, server);
        }

        private void HandleConnection(IAsyncResult result)
        {
            AcceptConnection();
            TcpClient client = server.EndAcceptTcpClient(result);

            try
            {
                NetworkStream ns = client.GetStream();

                while (true)
                {
                    while (!ns.DataAvailable) ;
                    while (client.Available < 3) ;

                    byte[] incomingData = new byte[client.Available];
                    int bytesCount = ns.Read(incomingData, 0, client.Available);
                    string decodedData = Encoding.UTF8.GetString(incomingData);

                    if (Regex.IsMatch(decodedData, "^GET", RegexOptions.IgnoreCase))
                    {
                        byte[] response = MakeHandshake(decodedData);
                        ns.Write(response, 0, response.Length);
                    }
                    else
                    {
                        bool fin = (incomingData[0] & 0b10000000) != 0,
                            mask = (incomingData[1] & 0b10000000) != 0; 

                        int opcode = incomingData[0] & 0b00001111, 
                            msglen = incomingData[1] - 128, 
                            offset = 2;

                        if (msglen == 126)
                        {
                            msglen = BitConverter.ToUInt16(new byte[] { incomingData[3], incomingData[2] }, 0);
                            offset = 4;
                        }

                        else if (mask)
                        {
                            byte[] decoded = new byte[msglen];
                            byte[] masks = new byte[4] { incomingData[offset], incomingData[offset + 1], incomingData[offset + 2], incomingData[offset + 3] };
                            offset += 4;

                            for (int i = 0; i < msglen; ++i)
                                decoded[i] = (byte)(incomingData[offset + i] ^ masks[i % 4]);

                            string text = Encoding.UTF8.GetString(decoded);

                            string writeJson;

                            ClientInput input = JsonConvert.DeserializeObject<ClientInput>(text);

                            if (!input.Authenticate())
                            {
                                writeJson = Status.Create(403, "Forbidden: Authentication failed.");
                                ns.Write(Encoding.Default.GetBytes(writeJson), 0, Encoding.Default.GetBytes(writeJson).Length);
                                client.Close();
                                break;
                            }

                            if (input.command != "print")
                            {
                                writeJson = Status.Create(400, "Bad Request: Incorrect command. Please try again.");
                                ns.Write(Encoding.Default.GetBytes(writeJson), 0, Encoding.Default.GetBytes(writeJson).Length);
                                client.Close();
                                break;
                            }

                            ZplString zpl = new ZplString(input.zpl);

                            if (!zpl.Validate())
                            {
                                writeJson = Status.Create(400, "Bad Request: The ZPL string is formatted incorrectly. Please send a properly formatted ZPL string.");
                                ns.Write(Encoding.Default.GetBytes(writeJson), 0, Encoding.Default.GetBytes(writeJson).Length);
                                client.Close();
                                break;
                            }

                            Printer printer = new Printer();

                            if (printer.GetName() == "")
                            {
                                writeJson = Status.Create(500, "Internal Server Error: A valid printer could not be found.");
                                ns.Write(Encoding.Default.GetBytes(writeJson), 0, Encoding.Default.GetBytes(writeJson).Length);
                                client.Close();
                                break;
                            }

                            printer.Print(zpl.text);

                            Dictionary<String, String> logEntry = new Dictionary<String, String>
                                {
                                    {"IPAddress", SystemHelper.GetLocalIP()},
                                    {"ComputerName", SystemHelper.GetComputerName()},
                                    {"ClientUsername", "username"},
                                    {"PrintContent", zpl.text}
                                };

                            PrintLogRepository.Log(logEntry);

                            writeJson = Status.Create(200, "OK: Action completed");
                            ns.Write(Encoding.Default.GetBytes(writeJson), 0, Encoding.Default.GetBytes(writeJson).Length);
                            client.Close();
                        }
                        else
                            Console.WriteLine("mask bit not set");

                        Console.WriteLine();
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionLogRepository.Log(ex.Message);
            }
        }

        private byte[] MakeHandshake(string message)
        {
            Console.WriteLine("=====Handshaking from client=====\n{0}", message);

            string swk = Regex.Match(message, "Sec-WebSocket-Key: (.*)").Groups[1].Value.Trim();
            string swka = swk + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
            byte[] swkaSha1 = System.Security.Cryptography.SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(swka));
            string swkaSha1Base64 = Convert.ToBase64String(swkaSha1);

            byte[] response = Encoding.UTF8.GetBytes(
                "HTTP/1.1 101 Switching Protocols\r\n" +
                "Connection: Upgrade\r\n" +
                "Upgrade: websocket\r\n" +
                "Sec-WebSocket-Accept: " + swkaSha1Base64 + "\r\n\r\n");

            return response;
        }
    }
}
