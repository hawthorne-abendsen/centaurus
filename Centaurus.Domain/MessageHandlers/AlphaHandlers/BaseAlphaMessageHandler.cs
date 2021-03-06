﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Centaurus.Models;

namespace Centaurus.Domain
{
    public abstract class BaseAlphaMessageHandler: BaseMessageHandler<AlphaWebSocketConnection, AlphaContext>, IAlphaMessageHandler
    {
        public BaseAlphaMessageHandler(AlphaContext context) 
            : base(context)
        {
        }

        /// <summary>
        /// Indicates whether authorization is required for the handler.
        /// </summary>
        public virtual bool IsAuthRequired { get; } = true;

        /// <summary>
        /// If set to true, than messages will be handled only if the client is an auditor. 
        /// IsAuthRequired property should be set to true
        /// </summary>
        public abstract bool IsAuditorOnly { get; }

        public override async Task Validate(AlphaWebSocketConnection connection, IncomingMessage message)
        {
            //if auth is required, then we should check that the current client public key is set, and that the envelope signatures contains it
            if (IsAuthRequired 
                && (connection.ClientPubKey == null || !message.Envelope.Signatures.Any(s => s.Signer != connection.ClientPubKey)))
                throw new UnauthorizedException();

            if (IsAuditorOnly && !connection.Context.Constellation.Auditors.Contains(connection.ClientPubKey))
                throw new UnauthorizedException();

            if (connection.Account != null && !connection.Account.RequestCounter.IncRequestCount(DateTime.UtcNow.Ticks, out string error))
                throw new TooManyRequestsException(error);

            await base.Validate(connection, message);
        }
    }
}
