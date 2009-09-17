using System;
using System.Net;
using System.IO;

namespace Buttercup.Control.Common.IO
{
    public class InMemoryFileStream: MemoryStream
    {
        public event EventHandler Disposing;
        public event EventHandler Closing;

        public InMemoryFileStream()
            : base()
        {
        }

        public InMemoryFileStream(byte[] buffer)
            : base(buffer)
        {
        }

        public override void Close()
        {
            if (Closing != null)
            {
                Closing(this, new EventArgs());
            }
            base.Close();
        }
        protected override void Dispose(bool disposing)
        {
            if (Disposing != null)
            {
                Disposing(this, new EventArgs());
            }
            base.Dispose(disposing);
        }
    }
}
