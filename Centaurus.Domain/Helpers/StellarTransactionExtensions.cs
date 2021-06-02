﻿using Centaurus.Models;
using System;
using System.Collections.Generic;
using System.Text;
using stellar_dotnet_sdk.xdr;
using System.Linq;
using static stellar_dotnet_sdk.xdr.OperationType;

namespace Centaurus.Domain
{
    public static class StellarTransactionExtensions
    {
        public static List<WithdrawalWrapperItem> GetWithdrawals(this stellar_dotnet_sdk.Transaction transaction, Account sourceAccount, ConstellationSettings constellationSettings, string vault)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            if (sourceAccount == null)
                throw new ArgumentNullException(nameof(sourceAccount));

            if (vault == null)
                throw new ArgumentNullException(nameof(vault));

            var payments = transaction.Operations
                .Where(o => o is stellar_dotnet_sdk.PaymentOperation)
                .Cast<stellar_dotnet_sdk.PaymentOperation>();

            var withdrawals = new List<WithdrawalWrapperItem>();
            foreach (var payment in payments)
            {
                if (!constellationSettings.TryFindAssetSettings(payment.Asset, out var asset))
                    throw new BadRequestException("Asset is not allowed by constellation.");

                if (payment.SourceAccount?.AccountId != vault)
                    throw new BadRequestException("Only vault account can be used as payment source.");
                var amount = stellar_dotnet_sdk.Amount.ToXdr(payment.Amount);
                if (amount < constellationSettings.MinAllowedLotSize)
                    throw new BadRequestException($"Min withdrawal amount is {constellationSettings.MinAllowedLotSize} stroops.");
                if (!(sourceAccount.GetBalance(asset.Id)?.HasSufficientBalance(amount) ?? false))
                    throw new BadRequestException($"Insufficient balance.");

                withdrawals.Add(new WithdrawalWrapperItem
                {
                    Asset = asset.Id,
                    Amount = amount,
                    Destination = payment.Destination.PublicKey
                });
            }
            if (withdrawals.GroupBy(w => w.Asset).Any(g => g.Count() > 1))
                throw new BadRequestException("Multiple payments for the same asset.");

            return withdrawals;
        }

        private static bool TryGetAsset(this ExecutionContext context, Asset xdrAsset, out int asset)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var assetAlphaNum = stellar_dotnet_sdk.Asset.FromXdr(xdrAsset) as stellar_dotnet_sdk.AssetTypeCreditAlphaNum;

            asset = 0;
            if (xdrAsset.Discriminant.InnerValue == AssetType.AssetTypeEnum.ASSET_TYPE_NATIVE)
                return true;

            string assetSymbol = $"{assetAlphaNum.Code}-{assetAlphaNum.Issuer}";

            var assetSettings = context.Constellation.Assets.Find(a => a.ToString() == assetSymbol);
            if (assetSettings == null) return false;
            asset = assetSettings.Id;
            return true;
        }

        public static OperationTypeEnum[] SupportedDepositOperations = new OperationTypeEnum[] { OperationTypeEnum.PAYMENT };

        public static bool TryGetPayment(this ExecutionContext context, Operation.OperationBody operation, stellar_dotnet_sdk.KeyPair source, stellar_dotnet_sdk.KeyPair vault, PaymentResults pResult, byte[] transactionHash, out PaymentBase payment)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            payment = null;
            int asset;
            //check supported deposit operations is overkill, but we need to keep SupportedDepositOperations up to date
            bool result = false;
            if (!SupportedDepositOperations.Contains(operation.Discriminant.InnerValue))
                return false;
            switch (operation.Discriminant.InnerValue)
            {
                case OperationTypeEnum.PAYMENT:
                    if (!context.TryGetAsset(operation.PaymentOp.Asset, out asset))
                        return result;
                    var amount = operation.PaymentOp.Amount.InnerValue;
                    var destKeypair = stellar_dotnet_sdk.KeyPair.FromPublicKey(operation.PaymentOp.Destination.Ed25519.InnerValue);
                    if (vault.Equals((RawPubKey)destKeypair.PublicKey))
                        payment = new Deposit
                        {
                            Destination = new RawPubKey() { Data = source.PublicKey },
                            Amount = amount,
                            Asset = asset,
                            TransactionHash = transactionHash
                        };
                    else if (vault.Equals((RawPubKey)source.PublicKey))
                        payment = new Withdrawal { TransactionHash = transactionHash };
                    if (payment != null)
                    {
                        payment.PaymentResult = pResult;
                        result = true;
                    }
                    break;
                case OperationTypeEnum.PATH_PAYMENT_STRICT_SEND:
                case OperationTypeEnum.PATH_PAYMENT_STRICT_RECEIVE:
                    //TODO: handle path payment
                    break;
                case OperationTypeEnum.ACCOUNT_MERGE:
                    //TODO: handle account merge
                    break;
            }
            return result;
        }
    }
}
