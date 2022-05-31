using OB.BL.Operations.Helper.Masking.Interfaces;
using System;

namespace OB.BL.Operations.Helper.Masking.Guard
{
    public class UnmaskGuard<T> : IDisposable
    {
        IMasked<T> Object { get; set; }

        public UnmaskGuard(T obj)
        {
            Object = obj as IMasked<T>;
            Object?.AsOriginal();
        }

        public void Dispose()
        {
            Object?.AsMasked();
        }
    }
}
