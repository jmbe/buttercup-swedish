using System;
using System.IO;
using System.Net;
using System.Text;

namespace Wilco.Windows.Browser.Net {

    public class HttpWebResponseEx : HttpWebResponse {

        private HttpWebRequestEx _request;
        private Stream _responseStream;
        private WebHeaderCollection _headers;

        public override long ContentLength {
            get {
                return Convert.ToInt64(Headers[HttpRequestHeader.ContentLength]);
            }
        }

        public override string ContentType {
            get {
                return Headers[HttpRequestHeader.ContentType];
            }
        }

        public WebHeaderCollection Headers {
            get {
                return _headers;
            }
        }

        public override string Method {
            get {
                return _request.Method;
            }
        }

        public override Uri ResponseUri {
            get {
                return _request.RequestUri;
            }
        }

        public override HttpStatusCode StatusCode {
            get {
                return (HttpStatusCode)Convert.ToInt32(_request.NativeRequest.GetProperty("status"));
            }
        }

        public override string StatusDescription {
            get {
                return (string)_request.NativeRequest.GetProperty("statusText");
            }
        }

        internal HttpWebResponseEx(HttpWebRequestEx request) {
            _request = request;
            _headers = new WebHeaderCollection();
            string responseHeaderString = (string)request.NativeRequest.Invoke("getAllResponseHeaders");
            foreach (string header in responseHeaderString.Split('\n')) {
                string[] pair = header.Split(':');
                if (pair.Length == 2) {
                    _headers[pair[0]] = pair[1].TrimEnd(); // Get rid of any trailing \r's.
                }
            }
        }

        public override Stream GetResponseStream() {
            if (_responseStream == null) {
                string responseXml = (string)_request.NativeRequest.GetProperty("responseText");
                _responseStream = new MemoryStream(Encoding.UTF8.GetBytes(responseXml));
            }
            return _responseStream;
        }

        public override void Close() {
            //Dispose(/* explicitDisposing */ true);
            if (_responseStream != null)
            {
                _responseStream.Dispose();
                _responseStream = null;
            }
            _request.ResetState();
        }
        
        //protected override void Dispose() { //(bool explicitDisposing) {
        //    if (_responseStream != null) {
        //        _responseStream.Dispose();
        //        _responseStream = null;
        //    }
        //    _request.ResetState();
        //}
    }
}