﻿using Centaurus.Domain;
using Centaurus.Models;
using Centaurus.Xdr;
using stellar_dotnet_sdk;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Centaurus
{
    public static class MessageEnvelopeExtenstions
    {
        /// <summary>
        /// Compute SHA256 hash for the wrapped message.
        /// </summary>
        /// <param name="messageEnvelope">Envelope</param>
        /// <param name="buffer">Buffer to use for serialization.</param>
        /// <returns>Message hash</returns>
        public static byte[] ComputeMessageHash(this MessageEnvelope messageEnvelope, byte[] buffer = null)
        {
            if (messageEnvelope == null)
                throw new ArgumentNullException(nameof(messageEnvelope));
            return messageEnvelope.Message.ComputeHash(buffer);
        }

        /// <summary>
        /// Signs an envelope with a given <see cref="KeyPair"/> and appends the signature to the <see cref="MessageEnvelope.Signatures"/>.
        /// </summary>
        /// <param name="messageEnvelope">Envelope to sign</param>
        /// <param name="keyPair">Key pair to use for signing</param>
        public static MessageEnvelope Sign(this MessageEnvelope messageEnvelope, KeyPair keyPair)
        {
            if (messageEnvelope == null)
                throw new ArgumentNullException(nameof(messageEnvelope));
            if (keyPair == null)
                throw new ArgumentNullException(nameof(keyPair));
            var signature = messageEnvelope.ComputeMessageHash().Sign(keyPair);
            messageEnvelope.Signatures.Add(signature);
            return messageEnvelope;
        }

        /// <summary>
        /// Signs an envelope with a given <see cref="KeyPair"/> and appends the signature to the <see cref="MessageEnvelope.Signatures"/>.
        /// </summary>
        /// <param name="messageEnvelope">Envelope to sign</param>
        /// <param name="keyPair">Key pair to use for signing</param>
        /// <param name="buffer">Buffer to use for computing hash code.</param>
        public static MessageEnvelope Sign(this MessageEnvelope messageEnvelope, KeyPair keyPair, byte[] buffer)
        {
            if (messageEnvelope == null)
                throw new ArgumentNullException(nameof(messageEnvelope));
            if (keyPair == null)
                throw new ArgumentNullException(nameof(keyPair));
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            using var writer = new XdrBufferWriter(buffer);
            XdrConverter.Serialize(messageEnvelope.Message, writer);
            var signature = writer.ToArray().ComputeHash().Sign(keyPair);
            messageEnvelope.Signatures.Add(signature);
            return messageEnvelope;
        }

        /// <summary>
        /// Checks that all envelope signatures are valid
        /// </summary>
        /// <param name="envelope">Target envelope</param>
        /// <returns>True if all signatures valid, otherwise false</returns>
        public static bool AreSignaturesValid(this MessageEnvelope envelope)
        {
            if (envelope == null)
                throw new ArgumentNullException(nameof(envelope));
            if (envelope.Signatures.Count < 1)
                return true;
            var messageHash = envelope.Message.ComputeHash();
            return envelope.Signatures.AreSignaturesValid(messageHash);
        }

        /// <summary>
        /// Checks that all envelope signatures are valid
        /// </summary>
        /// <param name="envelope">Target envelope</param>
        /// <returns>True if all signatures valid, otherwise false</returns>
        public static bool AreSignaturesValid(this List<Ed25519Signature> signatures, byte[] hash)
        {
            if (signatures == null)
                throw new ArgumentNullException(nameof(signatures));
            for (var i = 0; i < signatures.Count; i++)
            {
                if (!signatures[i].IsValid(hash))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Checks that envelope is signed with specified key
        /// !!!This method doesn't validate signature
        /// </summary>
        /// <param name="envelope">Target envelope</param>
        /// <param name="pubKey">Required signer public key</param>
        /// <returns>True if signed, otherwise false</returns>
        public static bool IsSignedBy(this MessageEnvelope envelope, RawPubKey pubKey)
        {
            if (envelope == null)
                throw new ArgumentNullException(nameof(envelope));
            if (pubKey == null)
                throw new ArgumentNullException(nameof(pubKey));
            return envelope.Signatures.Any(s => s.Signer.Equals(pubKey));
        }

        private static TResultMessage CreateResult<TResultMessage>(this MessageEnvelope envelope, ResultStatusCodes status = ResultStatusCodes.InternalError, List<Effect> effects = null)
            where TResultMessage : ResultMessage
        {
            if (envelope == null)
                throw new ArgumentNullException(nameof(envelope));
            var resultMessage = Activator.CreateInstance<TResultMessage>();
            resultMessage.OriginalMessage = envelope;
            resultMessage.Status = status;
            resultMessage.Effects = effects ?? new List<Effect>();
            return resultMessage;
        }

        public static ResultMessage CreateResult(this MessageEnvelope envelope, ResultStatusCodes status = ResultStatusCodes.InternalError, List<Effect> effects = null)
        {
            if (envelope == null)
                throw new ArgumentNullException(nameof(envelope));
            var messageType = envelope.Message.MessageType;
            if (envelope.Message is RequestQuantum)
                messageType = ((RequestQuantum)envelope.Message).RequestEnvelope.Message.MessageType;

            switch (messageType)
            {
                case MessageTypes.HandshakeInit:
                    return CreateResult<HandshakeResult>(envelope, status, effects);
                case MessageTypes.AccountDataRequest:
                    return CreateResult<AccountDataResponse>(envelope, status, effects);
                case MessageTypes.WithdrawalRequest:
                    return CreateResult<ITransactionResultMessage>(envelope, status, effects);
                default:
                    return CreateResult<ResultMessage>(envelope, status, effects);
            }
        }

        public static ResultMessage CreateResult(this MessageEnvelope envelope, Exception exc)
        {
            if (envelope == null)
                throw new ArgumentNullException(nameof(envelope));
            if (exc == null)
                throw new ArgumentNullException(nameof(exc));
            var result = envelope.CreateResult(exc.GetStatusCode());
            if (!(result.Status == ResultStatusCodes.InternalError || string.IsNullOrWhiteSpace(exc.Message)))
                result.ErrorMessage = exc.Message;
            else
                result.ErrorMessage = result.Status.ToString();
            return result;
        }
    }
}
