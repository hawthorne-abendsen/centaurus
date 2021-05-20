using System;
using System.Collections.Generic;
using Centaurus.Xdr;
using MessagePack;

namespace Centaurus.Models
{
    [MessagePackObject]
    [XdrContract]
    public class Balance
    {
        [Key(0)]
        [XdrField(0)]
        public int Asset { get; set; }

        [Key(1)]
        [XdrField(1)]
        public long Amount { get; set; }

        [Key(2)]
        [XdrField(2)]
        public long Liabilities { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Balance balance &&
                   Asset == balance.Asset &&
                   Amount == balance.Amount &&
                   Liabilities == balance.Liabilities;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Asset, Amount, Liabilities);
        }

        public override string ToString()
        {
            return $"Asset: {Asset}, amount: {Amount}, liabilities: {Liabilities}.";
        }
    }
}
