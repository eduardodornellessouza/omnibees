using OB.DL.Common.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OB.Domain.BE;
using System.Linq.Expressions;
using OB.DL.Common.Repositories.Impl.Couchbase;
using Couchbase.Core;
using OB.DL.Common.Interfaces;

namespace OB.DL.Common.Test.Mock
{
    internal class BEClosedDaysRepositoryMock : BEClosedDaysRepository
    {
        public Dictionary<string, BEClosedDays> OutputCouchbase { get; set; }
        public List<DateTime> OutputSql { get; set; }

        public BEClosedDaysRepositoryMock(IBucket bucket, ISqlManager sqlManager, IAppSettingRepository appSettingsRepo): base(bucket, sqlManager, appSettingsRepo)
        {
            OutputCouchbase = new Dictionary<string, BEClosedDays>();
            OutputSql = new List<DateTime>();
        }

        public override List<DateTime> GetBEClosedDays(long propertyUID, long channelUID, DateTime dateFrom, DateTime dateTo)
        {
            return base.GetBEClosedDays(propertyUID, channelUID, dateFrom, dateTo);
        }

        public override Dictionary<string, BEClosedDays> GetBEClosedDaysFromCouchbase(IList<string> documentIds)
        {
            return OutputCouchbase;
        }

        public override List<DateTime> GetBEClosedDaysFromSql(long propertyUID, long channelUID, DateTime dateFrom, DateTime dateTo)
        {
            return OutputSql;
        }
    }
}
