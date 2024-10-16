// See https://aka.ms/new-console-template for more information
using SmallWebServer;
using System;

namespace YourNamespace
{
    class Program
    {
        public static Server server;
        static void Main(string[] args)
        {
            server = new Server();
            server.OnError = ErrorHandler;
            server.Start(GetWebsitePath());
            Console.ReadLine();
        }
        public static string ErrorHandler(Server.ServerError error)
        {
            string ret = null;

            switch (error)
            {
                case Server.ServerError.ExpiredSession:
                    ret = "/ErrorPages/expiredSession.html";
                    break;
                case Server.ServerError.FileNotFound:
                    ret = "/ErrorPages/fileNotFound.html";
                    break;
                case Server.ServerError.NotAuthorized:
                    ret = "/ErrorPages/notAuthorized.html";
                    break;
                case Server.ServerError.PageNotFound:
                    ret = "/ErrorPages/pageNotFound.html";
                    break;
                case Server.ServerError.ServerError:
                    ret = "/ErrorPages/serverError.html";
                    break;
                case Server.ServerError.UnknownType:
                    ret = "/ErrorPages/unknownType.html";
                    break;
            }
            return ret;
        }
        static string GetWebsitePath()
        {
            string appPath = AppDomain.CurrentDomain.BaseDirectory;
            string websitePath = Path.GetFullPath(Path.Combine(appPath, @"..\..\..\Website"));


            return websitePath;
        }
    }
    
}






