using System.Collections.Generic;
using Blish_HUD;
using Blish_HUD.Content;
using Oberyn.FishingBuddy.Models;

namespace Oberyn.FishingBuddy.Services
{
    public static class BaitIcons
    {
        //showing Any as text because no icon
        public static readonly Dictionary<Bait, int> AssetIds = new Dictionary<Bait, int>
        {
            { Bait.FishEgg,           2594404 },
            { Bait.Leech,             2594406 },
            { Bait.LightningBug,      2594408 },
            { Bait.FreshwaterMinnow,  2594410 },
            { Bait.HaijuMinnow,       2594410 },
            { Bait.RamshornSnail,     2594412 },
            { Bait.Shrimpling,        2594414 },
            { Bait.SparkflyLarva,     2594416 },
            { Bait.GlowWorm,          2594418 },
            { Bait.LavaBeetle,        2594420 },
            { Bait.Sardine,           2594422 },
            { Bait.Scorpion,          2594424 },
            { Bait.Nightcrawler,      2594426 },
            { Bait.Mackerel,          2594639 },
        };

        // fall back to text in case a new bait shows up and module hasn't been updated yet
        public static int? GetAssetId(Bait bait)
        {
            return AssetIds.TryGetValue(bait, out var id) ? id : (int?)null;
        }
        public static AsyncTexture2D GetTexture(Bait bait)
        {
            var assetId = GetAssetId(bait);
            return assetId.HasValue
                ? GameService.Content.DatAssetCache.GetTextureFromAssetId(assetId.Value)
                : null;
        }
    }
}