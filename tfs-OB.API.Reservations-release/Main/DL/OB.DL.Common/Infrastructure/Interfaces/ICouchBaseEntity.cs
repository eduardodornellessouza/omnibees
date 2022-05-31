
using OB.Domain;
namespace OB.DL.Common
{
    public interface ICouchBaseEntity
    {
        string Id { get; set; }

        string Type { get; set; }

        ulong Cas { get; set; }
    }


    public interface ICouchBaseEntity<T> : ICouchBaseEntity where T : DomainObject
    {
        T DomainObject { get;set;}
    }
}
