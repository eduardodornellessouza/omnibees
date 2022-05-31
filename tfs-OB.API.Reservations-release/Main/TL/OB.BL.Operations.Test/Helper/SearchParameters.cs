using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OB.Services.IntegrationTests.Helpers
{
    public class SearchParameters
    {
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public int AdultCount { get; set; }
        public int ChildCount { get; set; }
        public string[] Ages { get; set; }
    }
}
