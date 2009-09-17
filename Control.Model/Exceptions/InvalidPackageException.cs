using System;

namespace Buttercup.Control.Model.Exceptions
{
    public class InvalidPackageException : Exception
    {
		#region Constructors (1) 

        public InvalidPackageException(string message)
            : base(message)
        {
        }

		#endregion Constructors 
    }
}