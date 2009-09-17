using System;

namespace Wilco.Windows.Browser.Net {

    public class RequestCompletedEventArgs : EventArgs {

        public Response Response {
            get;
            private set;
        }

        public RequestCompletedEventArgs(Response response) {
            Response = response;
        }
    }
}
