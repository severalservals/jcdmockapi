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
    /// <summary>Start and manage a TCP server to handle ZPL-print requests sent over the network.</summary>
    public class TcpServerAsyncWebsockets
    {
        // We default to using the local host as our TCP listener.
        IPAddress host = IPAddress.Parse("127.0.0.1");
        // QUESTION/COMMENT- If this is a number that is a convention for us, I'd like to put it in a constant or config. 
        const int PORT = 1516;
        private readonly TcpListener server;

        /// <summary>Start a TCP listener at the default host IP and port and set up a databaes connection for it.</summary>
        // QUESTION/COMMENT - We don't actually seem to use databaseAccess for anything, but maybe in real code we'd use it
        // when logging, or pass it to the Printer class to tell it where to look for printers? I'd want to explain that if 
        // possible. 
        public TcpServerAsyncWebsockets()
        {
            this.server = new TcpListener(host, PORT);
            DatabaseSetup databaseAccess = new DatabaseSetup();
        }

        /// <summary>Start the TCP server and get ready to handle print requests.</summary>
        public void ServerStart()
        {
            server.Start();
            AcceptConnection();
        }

        // Set the delegate method that the server will use to handle print requests it receives. 
        private void AcceptConnection()
        {
            server.BeginAcceptTcpClient(HandleConnection, server);
        }

        // The delegate that processes incoming print requests and, if possible, prints them.
        private void HandleConnection(IAsyncResult result)
        {
            // This call doesn't make sense. Isn't this the method we called to pass this method as a delegate?
            // So if we're in this method, we know we had to have successfully called AcceptConnection() already, 
            // and that method doesn't do anything besides specify this method. Why are we calling this? 
            AcceptConnection();
            TcpClient client = server.EndAcceptTcpClient(result);

            try
            {
                NetworkStream ns = client.GetStream();

                // QUESTION/COMMENT- I'm not sure why we have this while loop. It would make sense if we were reading incoming data in a synchronous way, but 
                // the rest of the code acts like result has all the data we're ever going to get. 
                while (true)
                {
                    // QUESTION/COMMENT- Silly questions, but ones any coder will ask - if we never start getting data
                    // in stream for some reason, or the client only sends two bytes of data for some weird
                    // reason, these loops are infinite. Is that a problem?
                    // The space between the parentheses and the semicolon looks deliberate, given the rest of 
                    // the formatting, so I'm taking that for a convention to indicate an empty while loop. 
                    while (!ns.DataAvailable) ;

                    // QUESTION/COMMENT- Below, is 3 a magic value for us in some way? If so, I'd like to replace with a named const. 
                    // From reading the next lines of code, it seems like once we get the data, we get it all - when we
                    // assign bytesCount, we never check again to see if it has changed - so what's up for us that we're not 
                    // saying "while (client.Available == 0)"?
                    // COMMENT - Available is the number of bytes of data received from the network and available to be read.
                    // Wait until we have some until starting work. 
                    while (client.Available < 3) ;

                    // incomingData holds all the data we got in the print request, which includes metadata as well as the 
                    // data that we want to print. 
                    byte[] incomingData = new byte[client.Available];
                    int bytesCount = ns.Read(incomingData, 0, client.Available);

                    // We only use decodedData to check for a handshake request from the client. For an actual print request, 
                    // we just use the incomingData byte array. 
                    string decodedData = Encoding.UTF8.GetString(incomingData);

                    // If the data we received starts with GET (case-insensitive), then just
                    // send a handshake response to the client. Don't do any other processing.
                    if (Regex.IsMatch(decodedData, "^GET", RegexOptions.IgnoreCase))
                    {
                        byte[] response = MakeHandshake(decodedData);
                        ns.Write(response, 0, response.Length);
                    }
                    // It wasn't a handshake request - it's a real print request. 
                    else
                    {
                        // We do a lot of low-level bit operations in this block to get and interpret the control bytes
                        // from the client request. 

                        // First, we do a bitwise comparison on the first two bytes of data to figure out if the fin and mask flags
                        // have been set. 
                        // 0b at the beginning of a number tells C# that the rest of the number are literal bits.
                        // fin is true if the first bit of the first incoming byte is 1, 
                        // mask is true if the first bit of the second incoming byte is 1.
                        // If the mask bit isn't set, we're not going to print anything. 

                        // QUESTION/COMMENT- We never use fin. Why do we bother getting it?
                        bool fin = (incomingData[0] & 0b10000000) != 0,
                            mask = (incomingData[1] & 0b10000000) != 0; 

                        // The last 4 bits of the first incoming byte give us the numeric value for opcode. 
                        // We strip the first 4 bits by AND-ing them with 0s. 
                        int opcode = incomingData[0] & 0b00001111, 
                            // The last seven bits of the second incoming byte give us our message length.
                            // We can find out what the last seven bits are by subtracting the 8th bit, which is worth 128
                            // if it's 1. That leaves us with a message length of 0-127 bytes. 
                            msglen = incomingData[1] - 128, 
                            // by default we assume that the first two bytes are metadata and the data starts at byte 3
                            // (0-indexed)
                            offset = 2;

                        // If the rightmost bit of the message length byte is 0 and the six bits to the right of that are 1's, 
                        // (126), that's a flag that the msglen is actually 
                        // a number that is two bytes long, that it is stored in bytes 3 and 4, and that our print data starts at byte, not byte 3.
                        // QUESTION/COMMENT- 126 might make a good named constant. 
                        if (msglen == 126) 
                        {
                            // The 0 argument just means start reading at the first (0-index) byte of the byte array we pass in the first argument.
                            msglen = BitConverter.ToUInt16(new byte[] { incomingData[3], incomingData[2] }, 0);
                            offset = 4;
                        }
                        // QUESTION/COMMENT- Is this an error? We went to some trouble in the lines above to get a two-byte message length,
                        // and an offset value that works with it, but with the "else" below, we're never going to use it. 
                        else if (mask)
                        {
                            // Get an array of msglen bytes to hold our print data.
                            byte[] decoded = new byte[msglen];
                            // The four bytes after the bytes with the opcode, msglen, fin and mask values are data masks that we'll use to 
                            // transform the data we read. The print data actually starts after the mask bytes. 
                            byte[] masks = new byte[4] { incomingData[offset], incomingData[offset + 1], incomingData[offset + 2], incomingData[offset + 3] };
                            offset += 4;

                            // QUESTION/COMMENT- is there a reason we use ++i in the for loop below? As far as I can tell it has the same effect as i++, but people are 
                            // really used to seeing i++ and when they see ++i, it makes them stop and wonder why and look for something they're missing. 
                            // If we just like using ++i, that's fine, I'm just offering my two cents from a training and onboarding perspectie. 

                            // COMMENT - read all of the actual incoming data, starting at the offset. 
                            // We use the mask bytes to transform the byte data we received and which we will ultimately deserialize below into
                            // an input object.
                            // To transform the data, as we read each data byte, we apply a bitwise exclusive OR operation (true if the bits compared are different,
                            // false otherwise) to the data byte and one of the mask bytes. We apply the mask bytes to the data bytes in groups of four. 
                            // The first data byte we read gets the first mask byte, the second data byte the second mask byte, etc., then we start again
                            // with the fifth data byte getting the first mask byte. 
                            for (int i = 0; i < msglen; ++i)
                                decoded[i] = (byte)(incomingData[offset + i] ^ masks[i % 4]);

                            // Turn our transformed data byte array into a JSON string.
                            string text = Encoding.UTF8.GetString(decoded);

                            // The output HTTP response we'll send back to the client. 
                            string writeJson;

                            // Finally, we have can turn our input JSON string into an input object.
                            ClientInput input = JsonConvert.DeserializeObject<ClientInput>(text);

                            if (!input.Authenticate())
                            {
                                writeJson = Status.Create(403, "Forbidden: Authentication failed.");
                                ns.Write(Encoding.Default.GetBytes(writeJson), 0, Encoding.Default.GetBytes(writeJson).Length);
                                client.Close();
                                break;
                            }

                            // QUESTION/COMMENT- "print" might be a good named constant. 
                            if (input.command != "print")
                            {
                                writeJson = Status.Create(400, "Bad Request: Incorrect command. Please try again.");
                                ns.Write(Encoding.Default.GetBytes(writeJson), 0, Encoding.Default.GetBytes(writeJson).Length);
                                client.Close();
                                break;
                            }

                            // Get a Zebra Print Language (ZPL) string from the input and validate it.
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

                            // Finally, we print to the printer we found in the new Printer() call above and log the whole interaction.
                            printer.Print(zpl.text);

                            // QUESTION/COMMENT- do we want to log errors? The way the code is written, we break on any error that we know how to 
                            // handle, like invalid ZPL data or printer not found, so we'll never see errors in our log. Exceptions go to the 
                            // ExceptionLogRepository, but errors we know how to handle are just lost. 
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

                            // QUESTION/COMMENT- We don't have a break here, so we stay in while(true). Is that what we want to do?
                            // We just closed the client, which we instantiated just to handle the result passed to this method, 
                            // so it doesn't seem like there's anything more for the while loop to do. 
                        }
                        else
                            // QUESTION/COMMENT- the code reads a little easier if you put a quick exit condition like this before all
                            // the lengthy data processing logic. 
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

        // Returns an array that the server can send to a client that has requested a websocket handshake. 
        // Documentation for this process is available here: 
        // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Sec-WebSocket-Accept
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
