using System;
using System.Collections.Generic;
using System.Text;

namespace Iomonitor.Interop.Exceptions
{
    class GetClipCursorFailedException : Exception
    {
        public GetClipCursorFailedException() : base("GetClipCursor has failed.")
        {
        }
    }
}
