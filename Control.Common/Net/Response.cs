using System;
using System.IO;
using System.Net;

namespace Wilco.Windows.Browser.Net {

    public class Response : IDisposable {

        private Stream _responseStream;

        public HttpWebResponse NativeResponse {
            get;
            private set;
        }

        internal Response(HttpWebResponse response) {
            NativeResponse = response;
        }

        public StreamReader OpenRead() {
            EnsureResponseStream();
            return new StreamReader(_responseStream);
        }

        public string ReadAllText() {
            EnsureResponseStream();
            return new StreamReader(_responseStream).ReadToEnd();
        }

        public void Close() {
            ((IDisposable)this).Dispose();
        }

        void IDisposable.Dispose() {
            if (_responseStream != null) {
                _responseStream.Close();
                _responseStream = null;
            }

            if (NativeResponse != null) {
                NativeResponse.Close();
                NativeResponse = null;
            }
        }

        private void EnsureResponseStream() {
            if (_responseStream == null) {
                _responseStream = NativeResponse.GetResponseStream();
            }
            _responseStream.Position = 0;
        }
    }
}
