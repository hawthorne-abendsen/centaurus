﻿using Centaurus.Models;
using Centaurus.DAL.Models.Analytics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Centaurus.DAL
{
    public interface IAnalyticsStorage
    {
        /// <summary>
        /// Fetches frames in reversed order (frames are LIFO collection)
        /// </summary>
        /// <param name="cursorTimeStamp">Cursor time-stamp</param>
        /// <param name="toUnixTimeStamp">Indicates up to what date frames should be taken. We cannot use limit, because there could be gaps if there were no trades during the period.</param>
        /// <param name="asset"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        Task<List<PriceHistoryFrameModel>> GetPriceHistory(int cursorTimeStamp, int toUnixTimeStamp, int asset, PriceHistoryPeriod period);

        Task SaveAnalytics(List<PriceHistoryFrameModel> frames);

        Task<int> GetFirstPriceHistoryFrameDate(int market, PriceHistoryPeriod period);
    }
}
