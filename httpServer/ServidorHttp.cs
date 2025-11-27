using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace httpServer
{
    public class ServidorHttp
    {
        private TcpListener Controller { get; set; }
        
        private int qntRequests { get; set; }
        private int Port { get; set; }

      public ServidorHttp(int port = 8080)
        {
            this.Port = port;
        try 
	{	        
		    this.Controller = new TcpListener(IPAddress.Parse("127.0.0.1"), this.Port);
            this.Controller.Start();
            Console.WriteLine($"Servidor HTTP iniciado na porta {this.Port}.");
            Console.WriteLine("Para acessar digite no navegador: localhost:8080");
            Task servidorHttpTask = Task.Run(() => awaitResponse());
            servidorHttpTask.GetAwaiter().GetResult();
            }
	catch (Exception e)
	{
        Console.WriteLine("Erro ao iniciar o servidor: " + e.Message);
            }
        }

        private async Task awaitResponse()
        {
            while (true)
            {
                Socket connection = await this.Controller.AcceptSocketAsync();
                qntRequests++;
                Task task = Task.Run(() => processarRequest(connection, this.qntRequests));
            }

        }

        private void processarRequest(Socket connection, int numeroRequest)
        {
            Console.WriteLine($"Processando requisição número ${numeroRequest}...\n");
            if(connection.Connected)
            {
                byte[] byteRequest = new byte[1024];
                connection.Receive(byteRequest, byteRequest.Length, 0);
                string textRequest = Encoding.UTF8.GetString(byteRequest).Replace((char) 0, ' ').Trim();
            if (textRequest.Length > 0)
            {
                    Console.WriteLine(textRequest);
                    connection.Close();
            }
            }
            Console.WriteLine($"Requisição número ${numeroRequest} finalizada");
        }



    }

}
