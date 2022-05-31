using OB.BL.Operations.Helper.Masking.Interfaces;
using System;

namespace OB.BL.Operations.Helper.Masking.Guard
{
    public class MaskGuard<T> : IDisposable
    {
        IMasked<T> Object { get; set; }

        public MaskGuard(T obj)
        {
            Object = obj as IMasked<T>;
            Object?.AsMasked();
        }

        public void Dispose()
        {
            Object?.AsOriginal();
        }
    }
}
