﻿//using FortBackend.src.App.TCP_XMPP.Helpers.Resources;
//using System.Net.Security;
//using System.Net.Sockets;
//using System.Security.Authentication;
//using System.Text;

//namespace FortBackend.src.App.XMPP_Server.TCP
//{
//    public class Handle
//    {
//        private async Task HandleTcpClientAsync(TcpClient client, string clientId)
//        {
//            DataSaved dataSaved = new DataSaved();
//            try
//            {
//                DataSaved.connectedClients.TryAdd(clientId, client);
//                //byte[] buffer = new byte[0];
//                //XDocument xmlDoc;
//                // NetworkStream stream = client.GetStream();
//                string logFilePath = "test.txt";
//                using (StreamWriter sw = File.AppendText(logFilePath))
//                {
//                    using (SslStream sslStream = new SslStream(client.GetStream(), false))
//                    {
//                        await sslStream.AuthenticateAsServerAsync(certificate, false, SslProtocols.Tls12, true);

//                        byte[] buffer = new byte[1024];
//                        int totalBytesRead = 0;
//                        while (true)
//                        {
//                            int bytesRead = await sslStream.ReadAsync(buffer, totalBytesRead, buffer.Length - totalBytesRead);
//                            if (bytesRead == 0)
//                            {
//                                break;
//                            }
//                            totalBytesRead += bytesRead;
//                            string receivedMessage = Encoding.UTF8.GetString(buffer, 0, totalBytesRead);
//                            Console.WriteLine($"Received message: {receivedMessage}");

//                            //receivedMessageBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
//                            if (receivedMessage.EndsWith(">"))
//                            {
//                                sw.WriteLine($"Received message: {receivedMessage}");
//                                Console.WriteLine($"Received message: {receivedMessage}");

//                                if (receivedMessage.Contains("stream:stream"))
//                                {
//                                    string responseXml1 = "<?xml version='1.0' encoding='UTF-8'?>" +
//                                "<stream:stream xmlns:stream='http://etherx.jabber.org/streams' " +
//                                $"xmlns='jabber:client' from='127.0.0.1'  id='{clientId}' " +
//                                "xml:lang='und' version='1.0'>";


//                                    byte[] responseBytes1 = Encoding.UTF8.GetBytes(responseXml1);
//                                    await sslStream.WriteAsync(responseBytes1, 0, responseBytes1.Length);
//                                    Console.WriteLine("Response sent");
//                                    await Task.Delay(100);
//                                    string responseXml2 = "<stream:features><starttls xmlns='urn:ietf:params:xml:ns:xmpp-tls'><required/></starttls>" +
//                                        "<mechanisms xmlns='urn:ietf:params:xml:ns:xmpp-sasl'><mechanism>PLAIN</mechanism></mechanisms></stream:features>";


//                                    byte[] responseBytes2 = Encoding.UTF8.GetBytes(responseXml2);
//                                    await sslStream.WriteAsync(responseBytes2, 0, responseBytes2.Length);
//                                    Console.WriteLine("Response sent");
//                                }
//                                else if (receivedMessage.Contains("starttls"))
//                                {
//                                    string proceedXml = "<proceed xmlns='urn:ietf:params:xml:ns:xmpp-tls'/>";
//                                    byte[] proceedBytes = Encoding.UTF8.GetBytes(proceedXml);
//                                    await sslStream.WriteAsync(proceedBytes, 0, proceedBytes.Length);
//                                    Console.WriteLine("Response sent");

//                                }
//                                totalBytesRead = 0;
//                            }
//                        }
//                    }
//                }

//                //        using (NetworkStream stream = client.GetStream())
//                //{
//                //    byte[] buffer = new byte[1024];
//                //    int bytesRead;

//                //    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
//                //    {
//                //                buffer = new byte[1024];
//                //                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
//                //                if (bytesRead == 0)
//                //                {
//                //                   // Console.WriteLine("Client disconnected!");
//                //                    break;
//                //                }
//                //                Console.WriteLine("Received BYTE: " + buffer);
//                //                string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
//                //                receivedData.Append(data);


//                //                Console.WriteLine("Received TCP message: " + data);
//                //                if (data.EndsWith(">"))
//                //                {
//                //                    Console.WriteLine("TRUEW");
//                //                }

//                //                if (data.EndsWith(">"))
//                //                {
//                //                    string completeMessage = receivedData.ToString();
//                //                    Console.WriteLine("Received TCP message: " + completeMessage);

//                //                    JToken test = "";
//                //                    try
//                //                    {
//                //                        test = JToken.Parse(receivedData.ToString());
//                //                        Console.WriteLine("DISCONNETING USER AS FAKE RESPONSE // JSON");
//                //                        //dataSaved.receivedMessage = "";
//                //                        return;
//                //                    }
//                //                    catch (JsonReaderException ex)
//                //                    {
//                //                        //return; // wow
//                //                    }
//                //                    if (data.Contains("stream:stream"))
//                //                    {
//                //                        string responseXml1 = "<?xml version='1.0' encoding='UTF-8'?>" +
//                //                "<stream:stream xmlns:stream='http://etherx.jabber.org/streams' " +
//                //                $"xmlns='jabber:client' from='127.0.0.1'  id='{clientId}' " +
//                //                "xml:lang='und' version='1.0'>";


//                //                        byte[] responseBytes1 = Encoding.UTF8.GetBytes(responseXml1);
//                //                        await stream.WriteAsync(responseBytes1, 0, responseBytes1.Length);
//                //                        Console.WriteLine("Response sent");
//                //                        await Task.Delay(100);
//                //                        string responseXml2 = "<stream:features><starttls xmlns='urn:ietf:params:xml:ns:xmpp-tls'><required/></starttls>" +
//                //                            "<mechanisms xmlns='urn:ietf:params:xml:ns:xmpp-sasl'><mechanism>PLAIN</mechanism></mechanisms></stream:features>";


//                //                        byte[] responseBytes2 = Encoding.UTF8.GetBytes(responseXml2);
//                //                        await stream.WriteAsync(responseBytes2, 0, responseBytes2.Length);
//                //                        Console.WriteLine("Response sent");


//                //                        //stream.Write(responseBytes, 0, responseBytes.Length);
//                //                    }
//                //                    if (!completeMessage.Contains("stream:stream"))
//                //                    {
//                //                        xmlDoc = XDocument.Parse(completeMessage);
//                //                        Console.WriteLine($"Value of 'to' attribute: {xmlDoc.Root?.Name.LocalName}");
//                //                        switch (xmlDoc.Root?.Name.LocalName)
//                //                        {
//                //                            case "starttls":
//                //                                string proceedXml = "<proceed xmlns='urn:ietf:params:xml:ns:xmpp-tls'/>";
//                //                                byte[] proceedBytes = Encoding.UTF8.GetBytes(proceedXml);
//                //                                await stream.WriteAsync(proceedBytes, 0, proceedBytes.Length);
//                //                                Console.WriteLine("Proceed sent");
//                //                                break;
//                //                            case "iq":
//                //                                Iq.Init(client, xmlDoc, clientId, dataSaved);
//                //                                break;
//                //                            default: break;
//                //                        }

//                //                        ClientFix.Init(client, dataSaved, clientId);

//                //                    }

//                //                    receivedData.Clear();
//                //                }
//                //            else
//                //            {
//                //                if (data.Contains("stream:stream"))
//                //                {
//                //                    string responseXml1 = "<?xml version='1.0' encoding='UTF-8'?>" +
//                //                "<stream:stream xmlns:stream='http://etherx.jabber.org/streams' " +
//                //                $"xmlns='jabber:client' from='127.0.0.1'  id='{clientId}' " +
//                //                "xml:lang='und' version='1.0'>";


//                //                    byte[] responseBytes1 = Encoding.UTF8.GetBytes(responseXml1);
//                //                    await stream.WriteAsync(responseBytes1, 0, responseBytes1.Length);
//                //                    Console.WriteLine("Response sent");
//                //                    await Task.Delay(100);
//                //                    string responseXml2 = "<stream:features><starttls xmlns='urn:ietf:params:xml:ns:xmpp-tls'><required/></starttls>" +
//                //                        "<mechanisms xmlns='urn:ietf:params:xml:ns:xmpp-sasl'><mechanism>PLAIN</mechanism></mechanisms></stream:features>";


//                //                    byte[] responseBytes2 = Encoding.UTF8.GetBytes(responseXml2);
//                //                    await stream.WriteAsync(responseBytes2, 0, responseBytes2.Length);
//                //                    Console.WriteLine("Response sent");
//                //                        //string startTlsXml = "<starttls xmlns='urn:ietf:params:xml:ns:xmpp-tls'/>";
//                //                        //byte[] responseBytes = Encoding.UTF8.GetBytes(startTlsXml);
//                //                        //Console.WriteLine("SENDING BACK");
//                //                        //await client.Client.SendAsync(responseBytes, SocketFlags.None);
//                //                        // client.Client.SendAsync
//                //                        //string streamOpeningResponse = "<stream:stream to=\"127.0.0.1\" xmlns:stream=\"http://etherx.jabber.org/streams\" xmlns=\"jabber:client\" version=\"1.0\">";
//                //                        //byte[] responseBytes = Encoding.UTF8.GetBytes(streamOpeningResponse);
//                //                        // Console.WriteLine("SENDING BACK");
//                //                        // await client.Client.SendAsync(responseBytes, SocketFlags.None);
//                //                        receivedData.Clear();
//                //                    }
//                //            }
//                //                Array.Clear(buffer, 0, buffer.Length);
//                //            }
//                //            Console.WriteLine("Client disconnected.");
//                //            break;
//                //        }



//                dataSaved.receivedMessage = "";


//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Error handling client: {ex.Message}");
//            }
//            finally
//            {
//                client.Close();
//                Console.WriteLine("Client disconnected.");
//            }
//        }
//    }
//}
