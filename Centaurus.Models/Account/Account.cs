using System;
using System.Collections.Generic;
using System.Linq;
using Centaurus.Xdr;
using MessagePack;

namespace Centaurus.Models
{
    [MessagePackObject]
    [XdrContract]
    public class Account
    {
        [Key(0)]
        [XdrField(0)]
        public int Id { get; set; }

        [Key(1)]
        [XdrField(1)]
        public RawPubKey Pubkey { get; set; }

        [Key(2)]
        [XdrField(2)]
        public long Nonce { get; set; }

        [Key(3)]
        [XdrField(3)]
        public List<Balance> Balances { get; set; }

        [Key(4)]
        [XdrField(4, Optional = true)]
        public RequestRateLimits RequestRateLimits { get; set; }

        [Key(5)]
        [XdrField(5)]
        public long Withdrawal { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Account account &&
                   Id == account.Id &&
                   Pubkey.Equals(account.Pubkey) &&
                   Nonce == account.Nonce &&
                   Enumerable.SequenceEqual(Balances, account.Balances) &&
                    (RequestRateLimits == null && account.RequestRateLimits == null || RequestRateLimits.Equals(account.RequestRateLimits)) &&
                   Withdrawal == account.Withdrawal;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Pubkey, Nonce, Balances, RequestRateLimits, Withdrawal);
        }
    }
}
