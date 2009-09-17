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
using System.Collections.Generic;

namespace Buttercup.Control.Common.Compression
{
    public abstract class ZipFileReader
    {
        private long _bufferSize = 900000; // Default
        private Dictionary<string, Stream> _dictionary = null;

        public ZipFileReader(Stream zipStream)
        {
            // Always start at the beginning of the Stream
            zipStream.Seek(0, SeekOrigin.Begin);
        }

        public virtual long BufferSize
        {
            get
            {
                return _bufferSize;
            }
            set
            {
                _bufferSize = value;
            }
        }

        /// <summary>
        /// Copies each file in the zip to a flat Dictionary structure. 
        /// </summary>
        public virtual Dictionary<string, Stream> CopyToDictionary
        {
            get
            {
                if (_dictionary == null)
                {
                    _dictionary = new Dictionary<string, Stream>();

                    string fileName;

                    // Iterate through each file in the zipfile.
                    while (MoveNext())
                    {
                        if (!IsFolder)
                        {
                            MemoryStream ms = new MemoryStream();
                            
                            // Have to do this for each item in the zip file until we reach the
                            // file we need.
                            Copy(ms);
                            ms.Seek(0, SeekOrigin.Begin);

                            // Get the name of the current file within the zip file
                            fileName = FullPath.ToLower();

                            _dictionary.Add(fileName, ms);
                            
                        }
                    }
                }
                return _dictionary;
            }
        }

        public abstract bool MoveNext();
        /// <summary>
        /// Copies the current item to the supplied Stream
        /// </summary>
        /// <param name="stream"></param>
        public abstract void Copy(Stream stream);

        public abstract bool IsFolder { get; }

        public abstract string FullPath { get; }

    }
}
