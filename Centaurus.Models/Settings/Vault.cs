﻿using Centaurus.Xdr;
using System;
using System.Collections.Generic;
using System.Text;

namespace Centaurus.Models
{
    [XdrContract]
    public class Vault
    {
        [XdrField(0)]
        public PaymentProvider Provider { get; set; }

        [XdrField(1)]
        public string AccountId { get; set; }
    }
}
