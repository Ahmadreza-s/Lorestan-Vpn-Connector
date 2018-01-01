using System;
using System.IO;
using System.Net;
using System.Text;

namespace LorestanVpnConnector
{
    class MyWebRequest
    {
        private WebRequest request;
        private Stream dataStream;

        public string Status { get; set; }

        public MyWebRequest(string url) => request = WebRequest.Create(url);

        public MyWebRequest(string url, string method)
            : this(url) => request.Method = method.Equals("GET") || method.Equals("POST")
            ? method
            : throw new Exception("Invalid Method Type");

        public MyWebRequest(string url, string method, string data)
            : this(url, method)
        {
            var postData = data;
            var byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;
            dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
        }

        public string GetResponse()
        {
            var response = request.GetResponse();
            Status = ((HttpWebResponse)response).StatusDescription;
            dataStream = response.GetResponseStream();
            var reader = new StreamReader(dataStream);
            var responseFromServer = reader.ReadToEnd();
            reader.Close();
            dataStream.Close();
            response.Close();
            return responseFromServer;
        }
    }
}
