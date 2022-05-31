namespace OB.BL.Operations.Helper.Masking.Interfaces
{
    public interface IMasked<out T>
    {
        void AsMasked();
        void AsOriginal();
    }
}
