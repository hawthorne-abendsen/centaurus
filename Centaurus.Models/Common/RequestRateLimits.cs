using Centaurus.Xdr;
using MessagePack;
using System;

namespace Centaurus.Models
{
    [MessagePackObject]
    [XdrContract]
    public class RequestRateLimits
    {
        [Key(0)]
        [XdrField(0)]
        public uint MinuteLimit { get; set; }

        [Key(1)]
        [XdrField(1)]
        public uint HourLimit { get; set; }

        public override bool Equals(object obj)
        {
            return obj is RequestRateLimits limits &&
                   MinuteLimit == limits.MinuteLimit &&
                   HourLimit == limits.HourLimit;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(MinuteLimit, HourLimit);
        }
    }
}
