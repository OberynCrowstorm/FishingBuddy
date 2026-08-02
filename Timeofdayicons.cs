using System.Collections.Generic;
using Blish_HUD.Content;
using Blish_HUD.Modules.Managers;
using Oberyn.FishingBuddy.Models;

namespace Oberyn.FishingBuddy.Services
{
    public static class TimeOfDayIcons
    {
        private static AsyncTexture2D _favorDay;
        private static AsyncTexture2D _favorNight;
        private static AsyncTexture2D _favorAny;
        private static AsyncTexture2D _duskDawn;
        private static bool _loaded;

        private static void EnsureLoaded(ContentsManager contentsManager)
        {
            if (_loaded) return;

            _favorDay = contentsManager.GetTexture("icons/favor_day.png");
            _favorNight = contentsManager.GetTexture("icons/favor_night.png");
            _favorAny = contentsManager.GetTexture("icons/favor_any.png");
            _duskDawn = contentsManager.GetTexture("icons/dusk_dawn.png");
            _loaded = true;
        }

        public static List<AsyncTexture2D> GetTextures(ContentsManager contentsManager, Fish fish)
        {
            EnsureLoaded(contentsManager);

            AsyncTexture2D baseIcon = GetBaseIcon(fish);
            AsyncTexture2D biasIcon = GetBiasIcon(fish);

            var result = new List<AsyncTexture2D> { baseIcon };
            if (biasIcon != null && biasIcon != baseIcon)
                result.Add(biasIcon);

            return result;
        }

        private static AsyncTexture2D GetBaseIcon(Fish fish)
        {
            if (fish.TimeOfDay2.HasValue)
                return _duskDawn;

            if (fish.TimeOfDay == TimeOfDay.Day)
                return _favorDay;

            if (fish.TimeOfDay == TimeOfDay.Night)
                return _favorNight;

            return _favorAny;
        }

        private static AsyncTexture2D GetBiasIcon(Fish fish)
        {
            if (fish.HigherChance == TimeOfDay.Day)
                return _favorDay;

            if (fish.HigherChance == TimeOfDay.Night)
                return _favorNight;

            return null;
        }
    }
}