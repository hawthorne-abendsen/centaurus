﻿using System;
using System.Collections.Generic;
using System.Text;
using Centaurus.Models;

namespace Centaurus.Domain
{
    public class LedgerQuantumHandler : AuditorBaseQuantumHandler
    {
        public LedgerQuantumHandler(AuditorContext context) 
            : base(context)
        {
        }

        public override MessageTypes SupportedMessageType { get; } = MessageTypes.TxCommitQuantum;
    }
}
