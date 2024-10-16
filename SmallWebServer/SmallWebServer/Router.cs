using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace SmallWebServer
{
    public class Router
    {
        public string WebsitePath { get; set; }

        private Dictionary<string, ExtensionInfo> extFolderMap;

        public const string POST = "post";
        public const string GET = "get";
        public const string PUT = "put";
        public const string DELETE = "delete";

        protected Server server;

        public Router(Server server) 
        {
            this.server = server;

            extFolderMap = new Dictionary<string, ExtensionInfo>()
            {
                {"ico", new ExtensionInfo() {Loader=ImageLoader, ContentType="image/ico"}},
                {"png", new ExtensionInfo() {Loader=ImageLoader, ContentType="image/png"}},
                {"jpg", new ExtensionInfo() {Loader=ImageLoader, ContentType="image/jpg"}},
                {"gif", new ExtensionInfo() {Loader=ImageLoader, ContentType="image/gif"}},
                {"bmp", new ExtensionInfo() {Loader=ImageLoader, ContentType="image/bmp"}},
                {"html", new ExtensionInfo() {Loader=PageLoader, ContentType="text/html"}},
                {"css", new ExtensionInfo() {Loader=FileLoader, ContentType="text/css"}},
                {"js", new ExtensionInfo() {Loader=FileLoader, ContentType="text/javascript"}},
                {"", new ExtensionInfo() {Loader=PageLoader, ContentType="text/html"}},
            };
        }
        /// <summary>
        /// Read in an image file and returns a ResponsePacket with the raw data.
        /// </summary>
        private ResponsePacket ImageLoader(string fullPath, string ext, ExtensionInfo extensionInfo)
        {
            FileStream fStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fStream);
            ResponsePacket ret = new ResponsePacket()
            {
                Data = br.ReadBytes((int)fStream.Length),
                ContentType =
                extensionInfo.ContentType
            };
            br.Close();
            fStream.Close();

            return ret;
        }
        /// <summary>
        /// Read in what is basically a text file and return a ResponsePacket with the text UTF8 encoded.
        /// </summary>
        private ResponsePacket FileLoader(string fullPath, string ext, ExtensionInfo extInfo)
        {
            string text = File.ReadAllText(fullPath);
            ResponsePacket ret = new ResponsePacket()
            {
                Data = Encoding.UTF8.GetBytes(text),
                ContentType = extInfo.ContentType,
                Encoding = Encoding.UTF8
            };

            return ret;
        }

        private ResponsePacket PageLoader(string fullPath, string ext, ExtensionInfo extInfo)
        {
            ResponsePacket ret = new ResponsePacket();

            if (fullPath == WebsitePath)
            {
                ret = Route(GET, "/test.html", null);
            }
            else
            {
                if (String.IsNullOrEmpty(ext))
                {
                    fullPath = fullPath + ".html";
                }

                fullPath = WebsitePath + "\\Pages" + "\\test.html";
                ret = FileLoader(fullPath, ext, extInfo);
            }
            return ret;
        }

        public ResponsePacket Route(string verb, string path, NameValueCollection parms)
        {
            string ext;
            if (path.Contains("."))
            {
                ext = path.Substring(path.LastIndexOf('.') + 1);
            }
            else
            {
                ext = "";
            }
            ExtensionInfo extInfo;
            ResponsePacket ret = null;

            if (extFolderMap.TryGetValue(ext, out extInfo))
            {
                string fullPath = WebsitePath + path;
                ret = extInfo.Loader(fullPath, ext, extInfo);
            }
            else
            {
                ret = new ResponsePacket() { Error = Server.ServerError.UnknownType };
            }

            return ret;
        }

        

        public class ResponsePacket
        {
            public string Redirect { get; set; }
            public byte[] Data { get; set; }
            public string ContentType { get; set; }
            public Encoding Encoding { get; set; }
            public Server.ServerError Error { get; set; }

            public ResponsePacket()
            {
                Error = Server.ServerError.OK;
            }
        }

        internal class ExtensionInfo
        {
            public string ContentType { get; set; }
            public Func<string,string, ExtensionInfo, ResponsePacket> Loader { get; set; }
        }
    }
}
