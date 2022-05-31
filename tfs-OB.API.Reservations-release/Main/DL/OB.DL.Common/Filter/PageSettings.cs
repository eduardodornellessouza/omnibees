using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.DL.Common.Filter
{
    public class PageSettings
    {
        /// <summary>
        /// Use the static method FromValues to initialize the PageSettings.
        /// </summary>
        private PageSettings()
        {
            ReturnTotal = false;
            PageIndex = -1;
            PageSize = -1;
        }

        public bool ReturnTotal { get; private set; }
        public int PageIndex { get; private set; }
        public int PageSize { get; private set; }

        public SortByInfo[] SortByInfo { get; private set; }

        public PageSettings FromValues(bool returnTotal = false, int pageIndex = -1, int pageSize = 1, SortByInfo[] sortByInfo = null)
        {
            return new PageSettings
            {
                ReturnTotal = returnTotal,
                PageIndex = pageIndex,
                PageSize = pageSize,
                SortByInfo = sortByInfo
            };
        }
    }
}
