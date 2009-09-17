using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows.Browser;

namespace Wilco.Windows.Browser.Net {

    public class Request {

        private Uri _requestUri;
        private string _method;
        private string _bodyString;
        private Action<StreamWriter> _bodyReader;
        private Dictionary<string, string> _headers;
        private Action<Response> _defaultSendCallback;

        public event EventHandler<RequestCompletedEventArgs> Completed;

        private Request(Uri requestUri) {
            _requestUri = requestUri;
            _headers = new Dictionary<string, string>();
            _method = "GET";
        }

        public static Request To(Uri requestUri) {
            return new Request(requestUri);
        }

        public static Request To(string requestUri) {
            return new Request(new Uri(requestUri, UriKind.RelativeOrAbsolute));
        }

        public static Request BasedOn(Request sourceRequest) {
            var request = new Request(sourceRequest._requestUri) {
                _bodyReader = sourceRequest._bodyReader,
                _bodyString = sourceRequest._bodyString,
                _method = sourceRequest._method
            };
            foreach (var entry in sourceRequest._headers) {
                request._headers.Add(entry.Key, entry.Value);
            }
            return request;
        }

        public Response Send() {
            if (Request.IsCrossDomain(_requestUri)) {
                throw new NotSupportedException("Synchronous cross-domain requests are not supported at this time.");
            }

            var request = (HttpWebRequestEx)CreateRequest();
            LoadBody(request);
            return new Response(request.GetResponse());
        }

        public void SendAsync() {
            SendAsync(/* defaultSendCallback */ null);
        }

        public void SendAsync(Action<Response> defaultSendCallback) {
            _defaultSendCallback = defaultSendCallback;
            HttpWebRequest request = CreateRequest();
            LoadBodyAndSendAsync(request);
        }

        public Request WithBody(string body) {
            _bodyString = body;
            return this;
        }

        public Request WithBody(Action<StreamWriter> body) {
            _bodyReader = body;
            return this;
        }

        public Request WithHeader(string key, string value) {
            _headers[key] = value;
            return this;
        }

        public Request WithHeaders(params string[] keyValuePairs) {
            if ((keyValuePairs.Length & 0x1) == 0x1)
                throw new ArgumentException("Odd number of arguments implies an incomplete pair.");

            for (int i = 0; i < keyValuePairs.Length; i += 2) {
                _headers[keyValuePairs[i]] = keyValuePairs[i + 1];
            }
            return this;
        }

        public Request WithHeaders(Dictionary<string, string> headers) {
            foreach (var entry in headers) {
                _headers[entry.Key] = entry.Value;
            }
            return this;
        }

        public Request WithMethod(string method) {
            _method = method;
            return this;
        }

        private void LoadBody(HttpWebRequestEx request) {
            WriteBody(request.GetRequestStream());
        }

        private void LoadBodyAndSendAsync(HttpWebRequest request) {
            if (_bodyReader != null || _bodyString != null) {
                request.BeginGetRequestStream(asyncRequestStreamResult => {
                    WriteBody(request.EndGetRequestStream(asyncRequestStreamResult));
                    SendAsyncRequest(request);
                }, null);
            }
            else {
                SendAsyncRequest(request);
            }
        }

        private void SendAsyncRequest(HttpWebRequest request) {
            request.BeginGetResponse(asyncResponseResult => {
                var response = (HttpWebResponse)request.EndGetResponse(asyncResponseResult);
                ProcessResponse(response);
            }, null);
        }

        private void ProcessResponse(HttpWebResponse response) {
            var handler = Completed;
            var richResponse = new Response(response);
            if (handler != null) {
                handler(this, new RequestCompletedEventArgs(richResponse));
            }
            if (_defaultSendCallback != null) {
                _defaultSendCallback(richResponse);
            }

            _defaultSendCallback = null;
        }

        protected virtual HttpWebRequest CreateRequest() {
            var request = HttpWebRequestEx.Create(_requestUri);
            SetRequestProperties(request);
            return request;
        }

        private void SetRequestProperties(HttpWebRequest request) {
            request.Method = _method;
            foreach (var entry in _headers) {
                request.Headers[entry.Key] = entry.Value;
            }
        }

        private void WriteBody(Stream requestStream) {
            using (StreamWriter writer = new StreamWriter(requestStream)) {
                if (_bodyReader != null) {
                    _bodyReader(writer);
                    _bodyReader = null;
                }
                else if (_bodyString != null) {
                    writer.Write(_bodyString);
                    _bodyString = null;
                }
            }
        }

        public static bool IsCrossDomain(Uri requestUri) {
            if (!requestUri.IsAbsoluteUri)
                return false;

            string requestDomain = requestUri.GetComponents(UriComponents.SchemeAndServer | UriComponents.Port, UriFormat.Unescaped);
            string currentDomain = HtmlPage.Document.DocumentUri.GetComponents(UriComponents.SchemeAndServer | UriComponents.Port, UriFormat.Unescaped);
            return !requestDomain.Equals(currentDomain);
        }
    }
}
