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

        SortedList<string, string> MimeTypes { get; set; }



        public ServidorHttp(int port = 8080)
        {

            populateMimeTypes();
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
            if (connection.Connected)
            {
                byte[] byteRequest = new byte[1024];
                connection.Receive(byteRequest, byteRequest.Length, 0);
                string textRequest = Encoding.UTF8.GetString(byteRequest).Replace((char)0, ' ').Trim();
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

                    byte[] bytesCabecalho = null;
                    byte[] bytesConteudo = null;


                    FileInfo fiArchive = new FileInfo(resolvePath(recursoBuscado));
                    if (fiArchive.Exists)
                    {
                        if(MimeTypes.ContainsKey(fiArchive.Extension.ToLower()))
                        {
                         bytesConteudo = File.ReadAllBytes(fiArchive.FullName);
                         string mimeType = MimeTypes[fiArchive.Extension.ToLower()];
                         bytesCabecalho = generateHeader(httpVersion, mimeType, "200", bytesConteudo.Length);

                        }
                        else
                        {
                            bytesConteudo = Encoding.UTF8.GetBytes("<h1>Tipo de arquivo nao suportado</h1>");
                            bytesCabecalho = generateHeader(httpVersion, "text/html;charset=utf-8", "415", bytesConteudo.Length);
                        }

                    }
                    else 
                    {
                        bytesConteudo = Encoding.UTF8.GetBytes("<h1>Arquivo não encontrado</h1>");
                        bytesCabecalho = generateHeader(httpVersion, "text/html;charset=UTF-8", "404", bytesConteudo.Length);
                    }

                        int bytesEnviados = connection.Send(bytesCabecalho, bytesCabecalho.Length, 0);
                    bytesEnviados += connection.Send(bytesConteudo, bytesConteudo.Length, 0);
                    connection.Close();
                    Console.WriteLine($"Bytes enviados: {bytesEnviados}\n Requsição número: {numeroRequest}");
                }
                Console.WriteLine($"Requisição número ${numeroRequest} finalizada");
            }
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

        private void populateMimeTypes()
        {
            this.MimeTypes = new SortedList<string, string>();
            this.MimeTypes.Add(".html", "text/html");
            this.MimeTypes.Add(".htm", "text/html");
            this.MimeTypes.Add(".css", "text/css");
            this.MimeTypes.Add(".js", "application/javascript");
            this.MimeTypes.Add(".png", "image/png");
            this.MimeTypes.Add(".jpg", "image/jpeg");
            this.MimeTypes.Add(".jpeg", "image/jpeg");
            this.MimeTypes.Add(".gif", "image/gif");
            this.MimeTypes.Add(".txt", "text/plain");
            this.MimeTypes.Add(".json", "application/json");
        }

        private string resolvePath(string recurso)
        {
            if (recurso == "/")
                recurso = "index.html";

            recurso = recurso.TrimStart('/');

            return Path.Combine(WebRootPath, recurso);
        }


    }


}
