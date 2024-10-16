// See https://aka.ms/new-console-template for more information
using SmallWebServer;

Server.Start(GetWebsitePath());
Console.ReadLine();


static string GetWebsitePath()
{
    string appPath = AppDomain.CurrentDomain.BaseDirectory;
    string websitePath = Path.GetFullPath(Path.Combine(appPath, @"..\..\..\Website"));


    return websitePath;
}