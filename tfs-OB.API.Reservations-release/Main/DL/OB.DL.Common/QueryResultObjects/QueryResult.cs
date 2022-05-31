using System.Linq;

namespace OB.DL.Common.QueryResultObjects
{
    public class QueryResult<T>
    {
        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public int TotalRecords { get; set; }

        public IQueryable<T> Results { get; set; }
    }
}