﻿using Centaurus.Xdr;
using System;
using System.Collections.Generic;
using System.Text;

namespace Centaurus.Models
{
    [XdrContract]
    public abstract class ConstellationEffect : Effect
    {
        [XdrField(0)]
        public List<Vault> Vaults { get; set; }

        [XdrField(1)]
        public List<PaymentCursor> Cursors { get; set; }

        [XdrField(2)]
        public List<RawPubKey> Auditors { get; set; }

        [XdrField(3)]
        public long MinAccountBalance { get; set; }

        [XdrField(4)]
        public long MinAllowedLotSize { get; set; }

        [XdrField(5)]
        public List<AssetSettings> Assets { get; set; }

        [XdrField(6, Optional = true)]
        public RequestRateLimits RequestRateLimits { get; set; }
    }
}
