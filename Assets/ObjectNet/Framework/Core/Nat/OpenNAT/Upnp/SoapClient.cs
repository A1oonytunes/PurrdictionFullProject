using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a SOAP client for making requests to a SOAP service.
    /// </summary>
    internal class SoapClient {
        // The service type string for the SOAP action header.
        private readonly string _serviceType;

        // The URL of the SOAP service.
        private readonly Uri _url;

        /// <summary>
        /// Initializes a new instance of the <see cref="SoapClient"/> class.
        /// </summary>
        /// <param name="url">The URL of the SOAP service.</param>
        /// <param name="serviceType">The service type string for the SOAP action header.</param>
        public SoapClient(Uri url, string serviceType) {
            _url = url;
            _serviceType = serviceType;
        }

#if NET35
    /// <summary>
    /// Invokes a SOAP operation asynchronously using .NET 3.5 Task-based Asynchronous Pattern (TAP).
    /// </summary>
    /// <param name="operationName">The name of the operation to invoke.</param>
    /// <param name="args">The arguments for the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the XML response.</returns>
    public Task<XmlDocument> InvokeAsync(string operationName, IDictionary<string, object> args)
    {
        // Log the SOAP action and URL.
        NatDiscoverer.TraceSource.TraceEvent(TraceEventType.Verbose, 0, "SOAPACTION: **{0}** url:{1}", operationName, _url);
        
        // Build the SOAP message body.
        byte[] messageBody = BuildMessageBody(operationName, args);
        
        // Create the HTTP request for the SOAP call.
        HttpWebRequest request = BuildHttpWebRequest(operationName, messageBody);

        // Define the task for getting the web response.
        Task<WebResponse> responseTask;
        
        // If there is a message body, write it to the request stream.
        if (messageBody.Length > 0)
        {
            Stream requestStream = null;
            // Begin the asynchronous request, write to the stream, and get the response.
            responseTask = Task.Factory.FromAsync<Stream>(request.BeginGetRequestStream, request.EndGetRequestStream, null)
                .ContinueWith(requestSteamTask =>
                {
                    requestStream = requestSteamTask.Result;
                    return Task.Factory.FromAsync<byte[], int, int>(requestStream.BeginWrite,
                        requestStream.EndWrite, messageBody, 0, messageBody.Length, null);
                })
                .Unwrap()
                .ContinueWith(streamWriteTask =>
                {
                    requestStream.Close();
                    return GetWebResponse(request);
                })
                .Unwrap();
        }
        else
        {
            // If there is no message body, just get the response.
            responseTask = GetWebResponse(request);
        }

        // Process the response and return the XML document.
        return responseTask.ContinueWith(task =>
        {
            using (WebResponse response = task.Result)
            {
                var stream = response.GetResponseStream();
                var contentLength = response.ContentLength;

                var reader = new StreamReader(stream, Encoding.UTF8);

                // Read the response body.
                var responseBody = contentLength != -1
                    ? reader.ReadAsMany((int)contentLength)
                    : reader.ReadToEnd();

                // Convert the response body to an XML document.
                var responseXml = GetXmlDocument(responseBody.Trim());

                response.Close();
                return responseXml;
            }
        });
    }
#else
        /// <summary>
        /// Invokes a SOAP operation asynchronously using .NET 4.5+ async/await pattern.
        /// </summary>
        /// <param name="operationName">The name of the operation to invoke.</param>
        /// <param name="args">The arguments for the operation.</param>
        /// <returns>A task representing the asynchronous operation, containing the XML response.</returns>
        public async Task<XmlDocument> InvokeAsync(string operationName, IDictionary<string, object> args) {
            // Log the SOAP action and URL.
            NatDiscoverer.TraceSource.TraceEvent(TraceEventType.Verbose, 0, "SOAPACTION: **{0}** url:{1}", operationName, _url);

            // Build the SOAP message body.
            byte[] messageBody = BuildMessageBody(operationName, args);

            // Create the HTTP request for the SOAP call.
            HttpWebRequest request = BuildHttpWebRequest(operationName, messageBody);

            // If there is a message body, write it to the request stream.
            if (messageBody.Length > 0) {
                using (var stream = await request.GetRequestStreamAsync()) {
                    await stream.WriteAsync(messageBody, 0, messageBody.Length);
                }
            }

            // Get the response and process it.
            using (var response = await GetWebResponse(request)) {
                var stream = response.GetResponseStream();
                var contentLength = response.ContentLength;

                var reader = new StreamReader(stream, Encoding.UTF8);

                // Read the response body.
                var responseBody = contentLength != -1
                                    ? reader.ReadAsMany((int)contentLength)
                                    : reader.ReadToEnd();

                // Convert the response body to an XML document.
                var responseXml = GetXmlDocument(responseBody);

                response.Close();
                return responseXml;
            }
        }
#endif

#if NET35
    /// <summary>
    /// Gets the web response for a given web request using .NET 3.5 Task-based Asynchronous Pattern (TAP).
    /// </summary>
    /// <param name="request">The web request to get the response for.</param>
    /// <returns>A task representing the asynchronous operation, containing the web response.</returns>
    private static Task<WebResponse> GetWebResponse(WebRequest request)
    {
        return Task.Factory
            .FromAsync<WebResponse>(request.BeginGetResponse, request.EndGetResponse, null)
            .ContinueWith(task =>
            {
                WebResponse response;
                if (!task.IsFaulted)
                {
                    response = task.Result;
                }
                else
                {
                    WebException ex = task.Exception.InnerException as WebException;
                    if (ex == null)
                    {
                        throw task.Exception;
                    }

                    // Log the WebException status.
                    NatDiscoverer.TraceSource.TraceEvent(TraceEventType.Verbose, 0, "WebException status: {0}", ex.Status);

                    // Even if the request "failed" we need to continue reading the response from the router.
                    response = ex.Response as HttpWebResponse;

                    if (response == null)
                    {
                        throw task.Exception;
                    }
                }

                return response;
            });
    }
#else
        /// <summary>
        /// Gets the web response for a given web request using .NET 4.5+ async/await pattern.
        /// </summary>
        /// <param name="request">The web request to get the response for.</param>
        /// <returns>A task representing the asynchronous operation, containing the web response.</returns>
        private static async Task<WebResponse> GetWebResponse(WebRequest request) {
            WebResponse response;
            try {
                response = await request.GetResponseAsync();
            } catch (WebException ex) {
                // Log the WebException status.
                NatDiscoverer.TraceSource.TraceEvent(TraceEventType.Verbose, 0, "WebException status: {0}", ex.Status);

                // Even if the request "failed" we need to continue reading the response from the router.
                response = ex.Response as HttpWebResponse;

                if (response == null)
                    throw;
            }
            return response;
        }
#endif

        /// <summary>
        /// Builds the HTTP web request for the SOAP call.
        /// </summary>
        /// <param name="operationName">The name of the operation to invoke.</param>
        /// <param name="messageBody">The message body as a byte array.</param>
        /// <returns>The configured <see cref="HttpWebRequest"/>.</returns>
        private HttpWebRequest BuildHttpWebRequest(string operationName, byte[] messageBody) {
#if NET35
        var request = (HttpWebRequest)WebRequest.Create(_url);
#else
            var request = WebRequest.CreateHttp(_url);
#endif
            request.KeepAlive = false;
            request.Method = "POST";
            request.ContentType = "text/xml; charset=\"utf-8\"";
            request.Headers.Add("SOAPACTION", "\"" + _serviceType + "#" + operationName + "\"");
            request.ContentLength = messageBody.Length;
            return request;
        }

        /// <summary>
        /// Builds the SOAP message body as a byte array.
        /// </summary>
        /// <param name="operationName">The name of the operation to invoke.</param>
        /// <param name="args">The arguments for the operation.</param>
        /// <returns>The message body as a byte array.</returns>
        private byte[] BuildMessageBody(string operationName, IEnumerable<KeyValuePair<string, object>> args) {
            var sb = new StringBuilder();
            // Construct the SOAP envelope with the provided arguments.
            sb.AppendLine("<s:Envelope ");
            sb.AppendLine("   xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\" ");
            sb.AppendLine("   s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">");
            sb.AppendLine("   <s:Body>");
            sb.AppendLine("	  <u:" + operationName + " xmlns:u=\"" + _serviceType + "\">");
            foreach (var a in args) {
                sb.AppendLine("		 <" + a.Key + ">" + Convert.ToString(a.Value, CultureInfo.InvariantCulture) +
                              "</" + a.Key + ">");
            }
            sb.AppendLine("	  </u:" + operationName + ">");
            sb.AppendLine("   </s:Body>");
            sb.Append("</s:Envelope>\r\n\r\n");
            string requestBody = sb.ToString();

            // Convert the SOAP envelope to a byte array.
            byte[] messageBody = Encoding.UTF8.GetBytes(requestBody);
            return messageBody;
        }

        /// <summary>
        /// Parses the SOAP response and returns an XML document.
        /// </summary>
        /// <param name="response">The SOAP response as a string.</param>
        /// <returns>The SOAP response as an <see cref="XmlDocument"/>.</returns>
        private XmlDocument GetXmlDocument(string response) {
            XmlNode node;
            var doc = new XmlDocument();
            doc.LoadXml(response);

            var nsm = new XmlNamespaceManager(doc.NameTable);

            // Error messages should be found under this namespace.
            nsm.AddNamespace("errorNs", "urn:schemas-upnp-org:control-1-0");

            // Check to see if we have a fault code message.
            if ((node = doc.SelectSingleNode("//errorNs:UPnPError", nsm)) != null) {
                int code = Convert.ToInt32(node.GetXmlElementText("errorCode"), CultureInfo.InvariantCulture);
                string errorMessage = node.GetXmlElementText("errorDescription");
                // Log the error and throw a MappingException.
                NatDiscoverer.TraceSource.LogWarn("Server failed with error: {0} - {1}", code, errorMessage);
                throw new MappingException(code, errorMessage);
            }

            return doc;
        }
    }

}