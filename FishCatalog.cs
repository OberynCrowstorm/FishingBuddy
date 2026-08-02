using System.Collections.Generic;
using System.Linq;
using Oberyn.AnglerAssociate.Data;
using Oberyn.AnglerAssociate.Models;

namespace Oberyn.AnglerAssociate.Services
{
    public static class FishCatalog
    {
        public static IEnumerable<Fish> All =>
            TyriaFishData.All
                .Concat(CanthaFishData.All)
                .Concat(CastoraFishData.All)
                .Concat(CrystalDesertFishData.All)
                .Concat(GlobalFishData.All)
                .Concat(HornOfMaguumaFishData.All)
                .Concat(JanthirFishData.All);

        // global (world and saltwater) fish have no cycle and are included whenever they're TimeOfDay.Any
        public static IEnumerable<Fish> GetCatchableNow()
        {
            var (tyriaState, _) = TyrianClock.GetState(Cycle.Tyria);
            var (canthaState, _) = TyrianClock.GetState(Cycle.CanthaCastora);

            foreach (var fish in All)
            {
                if (fish.Cycle == Cycle.Global)
                {
                    if (fish.TimeOfDay == TimeOfDay.Any)
                        yield return fish;

                    continue;
                }

                var state = fish.Cycle == Cycle.CanthaCastora ? canthaState : tyriaState;
                if (fish.IsCatchableAt(state))
                    yield return fish;
            }
        }
    }
}
