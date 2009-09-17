using System.IO;

namespace Buttercup.Control.Common.Helpers
{
    public class FileHelpers
    {
        private const int _bufferSize = 90000;

        /// <summary>
        /// Copy the contents of one Stream to another Stream
        /// </summary>
        /// <param name="dest">Destination Stream to copy to</param>
        /// <param name="src">Source Stream to copy from</param>
        public static void CopyStream(Stream dest, Stream src)
        {
            src.Seek(0, SeekOrigin.Begin);

            //copy the result stream into the cache stream
            byte[] buffer = new byte[_bufferSize];

            while (true)
            {
                int count = src.Read(buffer, 0, buffer.Length);
                if (count > 0)
                {
                    dest.Write(buffer, 0, count);
                }
                else
                {
                    break;
                }
            }

            //reset both streams to the beginning
            dest.Seek(0, SeekOrigin.Begin);
            src.Seek(0, SeekOrigin.Begin);
        }
    }
}