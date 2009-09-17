using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO;
using Xceed.Zip.ReaderWriter;

namespace Buttercup.Control.Common.Compression
{
    public class XceedZipFileReader : ZipFileReader
    {

        private ZipReader _zipReader;
        private ZipItemLocalHeader _zipItemLocalHeader;

        public XceedZipFileReader(Stream zipStream)
            : base(zipStream)
        {
            Licenser.LicenseKey = "ZRS10-EZB8A-CTGTY-A8MA";
            _zipReader = new ZipReader(zipStream);

            _zipItemLocalHeader = null;
        }

        public override bool MoveNext()
        {
            _zipItemLocalHeader = _zipReader.ReadItemLocalHeader();
            return _zipItemLocalHeader != null;
        }

        /// <summary>
        /// Copy the current zip file item to a stream
        /// </summary>
        /// <param name="stream"></param>
        public override void Copy(Stream stream)
        {
            if (!IsFolder)
            {

                byte[] buffer = new byte[BufferSize];

                while (true)
                {
                    int count = _zipReader.ReadItemData(buffer, 0, buffer.Length);
                    if (count > 0)
                    {
                        stream.Write(buffer, 0, count);
                    }
                    else
                    {
                        break;
                    }

                }
            }
            else
            {
                throw new Exception("Not possible for a Folder item");
            }
        }

        public override string FullPath
        {
            get
            {
                // TODO: Split to get filename without path.
                return _zipItemLocalHeader.FileName;
            }
        }

        public override bool IsFolder
        {
            get
            {
                return _zipItemLocalHeader.IsFolder;
            }
        }
    }
}
