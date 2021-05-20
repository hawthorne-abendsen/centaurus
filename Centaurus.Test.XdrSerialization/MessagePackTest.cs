using Centaurus.Models;
using Centaurus.Xdr;
using MessagePack;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Centaurus.Test
{
    class MessagePackTest
    {
        [SetUp]
        public void Setup()
        {
            DynamicSerializersInitializer.Init();
        }

        [Test]
        [Explicit]
        public void PerformanceTest()
        {
            var accountsCount = 1_000_000;

            var accounts = new List<Account>(accountsCount);
            for (var i = 0; i < accountsCount; i++)
            {
                accounts.Add(new Account
                {
                    Id = i,
                    Nonce = i,
                    Pubkey = stellar_dotnet_sdk.KeyPair.Random(),
                    Withdrawal = i,
                    Balances = new List<Balance> {
                        new Balance {
                            Amount = 100,
                            Asset = 1,
                            Liabilities = 11
                        },
                        new Balance {
                            Amount = 99,
                            Asset = 0,
                            Liabilities = 1
                        },
                    },
                    RequestRateLimits = i % 2 == 0 ? new RequestRateLimits { HourLimit = 12, MinuteLimit = 1 } : null
                });
            }

            var deserializedAccounts = Measure(accounts, XdrConverter.Serialize, XdrConverter.Deserialize<Account>, "Xdr");

            Assert.IsTrue(Enumerable.SequenceEqual(deserializedAccounts, accounts), "Xdr deserialized objects are not equal to source");

            deserializedAccounts = Measure(accounts,
                acc => MessagePackSerializer.Serialize(acc),
                bytes => MessagePackSerializer.Deserialize<Account>(bytes),
                "MessagePack");

            Assert.IsTrue(Enumerable.SequenceEqual(deserializedAccounts, accounts), "MessagePack deserialized objects are not equal to source");
        }

        static List<Account> Measure(List<Account> accounts, Func<Account, byte[]> serializationFn, Func<byte[], Account> deserializationFn, string algo)
        {
            var sw = new Stopwatch();
            sw.Start();

            var rawAccounts = new List<byte[]>(accounts.Count);
            foreach (var acc in accounts)
            {
                var raw = serializationFn(acc);
                rawAccounts.Add(raw);
            }

            sw.Stop();
            var serTime = sw.ElapsedMilliseconds;

            var deserializedAccounts = new List<Account>(accounts.Count);
            sw.Restart();
            for (var i = 0; i < rawAccounts.Count; i++)
            {
                deserializedAccounts.Add(deserializationFn(rawAccounts[i]));
            }
            sw.Stop();

            TestContext.Out.WriteLine($"{algo}: {{ objects count: {accounts.Count}, serialization time: {serTime}ms, deserialization time: {sw.ElapsedMilliseconds}ms, total time: {serTime + sw.ElapsedMilliseconds}ms, totalBytes: {rawAccounts.Sum(x => x.Length)} }}");

            return deserializedAccounts;
        }
    }
}
