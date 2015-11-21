using System;

namespace Extreme.Fgmres
{
    public class RightPreconditionerRequestEventArgs : EventArgs
    {
        public NativeVector Src { get; }
        public NativeVector Dst { get; }

        public RightPreconditionerRequestEventArgs(NativeVector src, NativeVector dst)
        {
            Src = src;
            Dst = dst;
        }
    }
}
