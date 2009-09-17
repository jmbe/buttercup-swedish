using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace Buttercup.Control.Common.Compression
{
    public class SharpZipFileReader : ZipFileReader
    {
        private readonly ZipInputStream _zipStream;
        private ZipEntry _zippedFile;

        public SharpZipFileReader(Stream zipStream)
            : base(zipStream)
        {
            _zipStream = new ZipInputStream(zipStream);
        }

        public override bool IsFolder
        {
            get { return _zippedFile.IsDirectory; }
        }

        public override string FullPath
        {
            get { return _zippedFile.Name; }
        }

        public override bool MoveNext()
        {
            _zippedFile = _zipStream.GetNextEntry();
            return _zippedFile != null;
        }

        public override void Copy(Stream stream)
        {
            if (!IsFolder)
            {
                byte[] buffer = new byte[BufferSize];

                while (true)
                {
                    int count = _zipStream.Read(buffer, 0, buffer.Length);
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
    }
}