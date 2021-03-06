﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Centaurus.Domain
{
    /// <summary>
    /// This class is dedicated to memory saving
    /// </summary>
    public class SubscriptionsManager
    {
        ConcurrentDictionary<string, BaseSubscription> subscriptions = new ConcurrentDictionary<string, BaseSubscription>();
        public BaseSubscription GetOrAddSubscription(BaseSubscription subscription)
        {
            return subscriptions.GetOrAdd(subscription.Name, subscription);
        }

        public bool TryGetSubscription(string name, out BaseSubscription subscription)
        {
            return subscriptions.TryGetValue(name, out subscription);
        }
    }
}