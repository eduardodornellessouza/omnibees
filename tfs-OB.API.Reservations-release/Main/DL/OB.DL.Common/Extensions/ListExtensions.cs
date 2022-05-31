using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.DL.Common.Extensions
{
    public static class ListExtensions
    {
        public static DataTable ToDataTable<T>(this List<T> ids)
        {
            DataTable table = new DataTable();
            table.Columns.Add("Value", typeof(T));
            foreach (var id in ids)
            {
                table.Rows.Add(id);
            }
            return table;
        }
    }
}
