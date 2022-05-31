using System;
using System.ComponentModel.DataAnnotations;

namespace OB.DL.Common.QueryResultObjects
{
    public class GuestActivitiesQR1
    {
        [Key]
        public long UID { get; internal set; }

        public long Guest_UID { get; internal set; }

        public string Name { get; internal set; }

    }
}