using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.DL.Common.Test.Helper
{
    [Serializable]
    public class RateChannelInputData
    {
        public List<long> ChannelUIDsToDelete { get; set; }

        public RateChannelInputData()
        {
            ChannelUIDsToDelete = new List<long>();
        }
    }

    [Serializable]
    public class RateChannelExpectedData
    {
        public Dictionary<long, int> ChannelUID_CountIsDeleted { get; set; }

        public RateChannelExpectedData()
        {
            ChannelUID_CountIsDeleted = new Dictionary<long, int>();
        }
    }
}
