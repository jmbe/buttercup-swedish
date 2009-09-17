using System;
using System.IO;
using System.Net;
using System.Windows.Browser;
using Wilco.Windows.Browser.Net;
using Wilco.Windows.Browser.Extensions;

namespace Wilco.Windows.Browser.Net {

    public class HttpWebRequestEx : HttpWebRequest {

        private Uri _requestUri;
        private ScriptObject _nativeRequest;
        private Stream _requestStream;
        private HttpWebResponse _response;
        private SimpleAsyncResult _responseAsyncResult;

        public override bool HaveResponse {
            get {
                return (_response != null);
            }
        }

        public override string Method {
            get;
            set;
        }

        internal ScriptObject NativeRequest {
            get {
                return _nativeRequest;
            }
        }

        public override Uri RequestUri {
            get {
                return _requestUri;
            }
        }

        private HttpWebRequestEx(Uri requestUri) {
            _requestUri = requestUri;
            _nativeRequest = HttpWebRequestEx.CreateNativeRequest();
            Method = "GET";
        }

        public override void Abort() {
            _nativeRequest.Invoke("abort");
            ResetState();
        }

        public override IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state) {
            if (_requestStream != null) {
                throw new InvalidOperationException("A request stream has already been created.");
            }

            SimpleAsyncResult result = new SimpleAsyncResult() {
                AsyncState = state,
                CompletedSynchronously = true,
                IsCompleted = true
            };
            callback(result);
            return result;
        }

        public override Stream EndGetRequestStream(IAsyncResult asyncResult) {
            return GetRequestStream();
        }

        public Stream GetRequestStream() {
            if (_requestStream == null) {
                _requestStream = new RequestStream();
            }
            return _requestStream;
        }

        public override IAsyncResult BeginGetResponse(AsyncCallback callback, object state) {
            if (HaveResponse) {
                throw new InvalidOperationException("A response object associated with this request has not been closed.");
            }
            if (_responseAsyncResult != null) {
                throw new InvalidOperationException("Already retrieving the response.");
            }

            _responseAsyncResult = new SimpleAsyncResult() {
                AsyncCallback = callback,
                AsyncState = state
            };

            _nativeRequest.Invoke("open", Method, _requestUri.OriginalString);
            foreach (var headerName in Headers.AllKeys) {
                _nativeRequest.Invoke("setRequestHeader", headerName, Headers[headerName]);
            }

            _nativeRequest.SetProperty("onreadystatechange",
                ScriptObjectUtility.ToScriptFunction((Action)ReadyStateChanged, args => HtmlPage.Window.CreateInstance("Array")));
            _nativeRequest.Invoke("send", GetBody());

            _responseAsyncResult.CompletedSynchronously = HaveResponse;
            _responseAsyncResult.IsCompleted = HaveResponse;

            return _responseAsyncResult;
        }

        public override WebResponse EndGetResponse(IAsyncResult asyncResult) {
            if (HaveResponse) {
                throw new InvalidOperationException("A response object associated with this request has not been closed.");
            }
            if (_responseAsyncResult != asyncResult) {
                throw new InvalidOperationException("Async result object not associated with this request.");
            }

            return _response = new HttpWebResponseEx(this);
        }

        public virtual HttpWebResponse GetResponse() {
            if (HaveResponse) {
                throw new InvalidOperationException("A response object associated with this request has not been closed.");
            }

            _nativeRequest.Invoke("open", Method, _requestUri.OriginalString, /* async */ false);
            foreach (var headerName in Headers.AllKeys) {
                _nativeRequest.Invoke("setRequestHeader", headerName, Headers[headerName]);
            }
            _nativeRequest.Invoke("send", GetBody());

            return _response = new HttpWebResponseEx(this);
        }

        public static new HttpWebRequest Create(Uri requestUri) {
            if (requestUri.IsAbsoluteUri) {
                string requestDomain = requestUri.GetComponents(UriComponents.SchemeAndServer | UriComponents.Port, UriFormat.Unescaped);
                string currentDomain = HtmlPage.Document.DocumentUri.GetComponents(UriComponents.SchemeAndServer | UriComponents.Port, UriFormat.Unescaped);

                // Use the request type that supports cross-domain requests.
                if (!requestDomain.Equals(currentDomain, StringComparison.OrdinalIgnoreCase)) {
                    return (HttpWebRequest)HttpWebRequest.Create(requestUri);
                }
            }

            // Use the XMLHTTP based request type.
            return new HttpWebRequestEx(requestUri);
        }

        internal void ResetState() {
            _requestStream = null;
            _nativeRequest.SetProperty("onreadystatechange", null);
            _response = null;
        }

        private void ReadyStateChanged() {
            if (Convert.ToInt32(_nativeRequest.GetProperty("readyState")) == 4) {
                _responseAsyncResult.AsyncCallback(_responseAsyncResult);
            }
        }

        private string GetBody() {
            if (_requestStream == null)
                return String.Empty;

            _requestStream.Position = 0;
            using (StreamReader reader = new StreamReader(_requestStream)) {
                return reader.ReadToEnd();
            }
        }

        private static ScriptObject CreateNativeRequest() {
            if (HtmlPage.Window.GetProperty("XMLHttpRequest") == null) {
                return HtmlPage.Window.CreateInstance("ActiveXObject", "Mxsml2.XMLHTTP.3.0");
            }
            return HtmlPage.Window.CreateInstance("XMLHttpRequest");
        }

        private class RequestStream : MemoryStream {

            protected override void Dispose(bool disposing) {
                if (!disposing) {
                    base.Dispose(disposing);
                }
            }

            public void ForceDispose() {
                base.Dispose(/* disposing */ true);
            }
        }

        private class SimpleAsyncResult : IAsyncResult {

            internal AsyncCallback AsyncCallback {
                get;
                set;
            }

            public object AsyncState {
                get;
                set;
            }

            public System.Threading.WaitHandle AsyncWaitHandle {
                get {
                    return null;
                }
            }

            public bool CompletedSynchronously {
                get;
                set;
            }

            public bool IsCompleted {
                get;
                set;
            }
        }
    }
}