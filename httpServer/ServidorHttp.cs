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
        public string WebRootPath { get; } =
    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "wwwroot");





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
                   Console.WriteLine($"\n{textRequest}\n");

                    string[] linhas = textRequest.Split("\r\n");
                    int iPrimeiroEspaco = linhas[0].IndexOf(' ');
                    int iSegundoEspaco = linhas[0].IndexOf(' ', iPrimeiroEspaco + 1);
                    string metodoHttp = linhas[0].Substring(0, iPrimeiroEspaco);
                    string recursoBuscado = linhas[0].Substring(iPrimeiroEspaco + 1, iSegundoEspaco - iPrimeiroEspaco - 1);
                    string httpVersion = linhas[0].Substring(iSegundoEspaco + 1);
                    iPrimeiroEspaco = linhas[1].IndexOf(' ');
                    string host = linhas[1].Substring(iPrimeiroEspaco + 1);



                    var bytesConteudo = readFile(recursoBuscado);
                    var bytesCabecalho = generateHeader("HTTP/1.1", "text/html;charset=UTF-8", "200", bytesConteudo.Length);
                   int bytesEnviados = connection.Send(bytesCabecalho, bytesCabecalho.Length, 0);
                    bytesEnviados += connection.Send(bytesConteudo, bytesConteudo.Length, 0);
                    connection.Close();
                   Console.WriteLine($"Bytes enviados: {bytesEnviados}\n Requsição número: {numeroRequest}");

                }
            }
            Console.WriteLine($"Requisição número ${numeroRequest} finalizada");
        }

        public byte[] generateHeader(string httpVersion, string mimeType, string httpCode, int qntBytes = 0)
        {
            StringBuilder header = new StringBuilder();
            header.Append($"{httpVersion} {httpCode}{Environment.NewLine}");
            header.Append($"Server: Teste de http simples {Environment.NewLine}");
            header.Append($"Content-Type: {mimeType}{Environment.NewLine}");
            header.Append($"Content-Length: {qntBytes}{Environment.NewLine}{Environment.NewLine}");
            return Encoding.UTF8.GetBytes(header.ToString());
        }

        public byte[] readFile(string filePath)
        {
            if (filePath == "/")
                filePath = "index.html";

            filePath = filePath.TrimStart('/');

            string absolute = Path.Combine(WebRootPath, filePath);

            if (File.Exists(absolute))
                return File.ReadAllBytes(absolute);

            return Encoding.UTF8.GetBytes("<h1>404 - Arquivo não encontrado</h1>");
        }



    }

}
