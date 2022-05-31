namespace OB.DL.Common.QueryResultObjects
{
    public class PropertyExternalSourceQR1
    {
        public PropertyExternalSourceQR1()
        {

        }
        public long UID { get; set; }
        public long Property_UID { get; set; }
        public long ExternalSource_UID { get; set; }
        public long ExternalSourceType_UID { get; set; }
        public bool IsActive { get; set; }
    }
}