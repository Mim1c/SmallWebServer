using System.Collections.Specialized;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using static SmallWebServer.Router;

namespace SmallWebServer
{
    public static class Server
    {
        private static HttpListener listener;
        private static Router router = new Router();
        /// <summary>
        /// Returns list of IP addresses assigned to localhost network devices, such as hardwired ethernet, wireless, etc.
        /// </summary>
        private static List<IPAddress> GetLocalHostIPs()
        {
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            List<IPAddress> ips = host.AddressList.Where(ip => ip.AddressFamily ==
            AddressFamily.InterNetwork).ToList();

            return ips;
        }

        //Adds prefixes for the listener to be returned
        private static HttpListener InitializeListener(List<IPAddress> localhostIPs) 
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost/");

            localhostIPs.ForEach(ip =>
            {
                Console.WriteLine("Listening on IP " + ip.ToString() + "/");
                listener.Prefixes.Add("http://" + ip.ToString() + "/");
            });

            return listener;
        }

        public static int maxSimultaneousConnections = 20;
        private static Semaphore sem = new Semaphore(maxSimultaneousConnections, maxSimultaneousConnections);

        //Start worker thread which listens for connections
        //PORT: 80 HTTP - 433 HTTPS
        private static void Start(HttpListener listener)
        {
            listener.Start();
            Task.Run(() => RunServer(listener));
        }

        //Runs in seperate thread
        private static void RunServer(HttpListener listener)
        {
            while (true)
            {
                sem.WaitOne();
                StartConnectionListener(listener);
            }
        }

        private static async void StartConnectionListener(HttpListener listener)
        {
            //Wait for connection
            HttpListenerContext context = await listener.GetContextAsync();

            //Release semaphore so other listeners can be started up
            sem.Release();
            Log(context.Request);

            HttpListenerRequest request = context.Request;
            string path = request.RawUrl;
            string verb = request.HttpMethod;
            NameValueCollection parms = request.QueryString;

            Log(parms);

            var resp = router.Route(verb, path, parms);
            Respond(context.Response, resp);
        }

        private static void Respond(HttpListenerResponse response, ResponsePacket resp)
        {
            response.ContentType = resp.ContentType;
            response.ContentLength64 = resp.Data.Length;
            response.OutputStream.Write(resp.Data, 0, resp.Data.Length);
            response.ContentEncoding = resp.Encoding;
            response.StatusCode = (int)HttpStatusCode.OK;
            response.OutputStream.Close();
        }

        public static void Start(string websitePath)
        {
            router.WebsitePath = websitePath;
            List<IPAddress> localHostIPs = GetLocalHostIPs();
            HttpListener listener = InitializeListener(localHostIPs);
            Start(listener);
        }

        private static void Log(HttpListenerRequest request)
        {
            Console.WriteLine(request.RemoteEndPoint + " sends " + request.HttpMethod + " to /" + 
                request.Url.AbsoluteUri);
        }

        private static void Log(NameValueCollection parms)
        {
            var items = parms.AllKeys.SelectMany(parms.GetValues, (k, v) => new { key = k, value = v });

            foreach ( var item in items )
            {
                Console.WriteLine(item.key + " : " + Uri.UnescapeDataString(item.value.ToString()));
            }
        }

        public static string GetWebsitePath()
        {
            string appPath = AppDomain.CurrentDomain.BaseDirectory;
            string websitePath = Path.GetFullPath(Path.Combine(appPath, @"..\..\..\Website"));


            return websitePath;
        }
    }
}
