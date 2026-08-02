using System;
using System.Collections.Generic;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Modules.Managers;
using Microsoft.Xna.Framework;
using Oberyn.AnglerAssociate.Models;
using Oberyn.AnglerAssociate.Services;

namespace Oberyn.AnglerAssociate.Controls
{
    // One region's day/night display: a big banner for the current state, plus two
    // small tiles below it for the next two states, each with a live "X in H:MM:SS"
    // countdown. Textures are loaded once from icons/{assetPrefix}_{state}_{l|s}.png -
    // matches the naming convention already used for tyria_*/cantha_* assets.
    //
    // Refresh() is called externally (by the module's own timer, once per second) -
    // Control.Update isn't overridable here, so this control doesn't tick itself.
    public class DayNightBanner : Panel
    {
        private readonly Cycle _cycle;
        private readonly Dictionary<TimeOfDay, AsyncTexture2D> _largeTextures;
        private readonly Dictionary<TimeOfDay, AsyncTexture2D> _smallTextures;

        private readonly Label _stateLabel;
        private readonly Image _bigImage;
        private readonly Label _upcoming1Label;
        private readonly Image _upcoming1Image;
        private readonly Label _upcoming2Label;
        private readonly Image _upcoming2Image;

        public DayNightBanner(ContentsManager contentsManager, string assetPrefix, string displayName, Cycle cycle)
        {
            if (cycle == Cycle.Global)
                throw new ArgumentException("Global has no day/night cycle to display.", nameof(cycle));

            Width = 120;
            Height = 410;

            _cycle = cycle;
            _largeTextures = LoadTextures(contentsManager, assetPrefix, "l");
            _smallTextures = LoadTextures(contentsManager, assetPrefix, "s");

            const int bannerWidth = 120;
            const int imageX = (bannerWidth - 90) / 2;

            var regionLabel = new Label
            {
                Parent = this,
                Text = displayName,
                Location = new Point(0, 0),
                Width = bannerWidth,
                Height = 24,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            _stateLabel = new Label
            {
                Parent = this,
                Location = new Point(0, 24),
                Width = bannerWidth,
                Height = 22,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            _bigImage = new Image
            {
                Parent = this,
                Location = new Point(imageX, 48),
                Size = new Point(90, 180),
            };

            _upcoming1Label = new Label
            {
                Parent = this,
                Location = new Point(0, 234),
                Width = bannerWidth,
                Height = 32,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            _upcoming1Image = new Image
            {
                Parent = this,
                Location = new Point(imageX, 268),
                Size = new Point(90, 45),
            };

            _upcoming2Label = new Label
            {
                Parent = this,
                Location = new Point(0, 319),
                Width = bannerWidth,
                Height = 32,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            _upcoming2Image = new Image
            {
                Parent = this,
                Location = new Point(imageX, 353),
                Size = new Point(90, 45),
            };

            Refresh();
        }

        private static Dictionary<TimeOfDay, AsyncTexture2D> LoadTextures(
            ContentsManager contentsManager, string assetPrefix, string sizeSuffix)
        {
            return new Dictionary<TimeOfDay, AsyncTexture2D>
            {
                { TimeOfDay.Dawn,  contentsManager.GetTexture($"icons/{assetPrefix}_dawn_{sizeSuffix}.png") },
                { TimeOfDay.Day,   contentsManager.GetTexture($"icons/{assetPrefix}_day_{sizeSuffix}.png") },
                { TimeOfDay.Dusk,  contentsManager.GetTexture($"icons/{assetPrefix}_dusk_{sizeSuffix}.png") },
                { TimeOfDay.Night, contentsManager.GetTexture($"icons/{assetPrefix}_night_{sizeSuffix}.png") },
            };
        }

        public void Refresh()
        {
            var upcoming = TyrianClock.GetUpcomingStates(_cycle, 3);

            var current = upcoming[0];
            _stateLabel.Text = current.State.ToString();
            _bigImage.Texture = _largeTextures[current.State];

            var next1 = upcoming[1];
            _upcoming1Label.Text = $"{next1.State} in {Format(next1.TimeUntilStart)}";
            _upcoming1Image.Texture = _smallTextures[next1.State];

            var next2 = upcoming[2];
            _upcoming2Label.Text = $"{next2.State} in {Format(next2.TimeUntilStart)}";
            _upcoming2Image.Texture = _smallTextures[next2.State];
        }

        private static string Format(TimeSpan span) =>
            $"{(int)span.TotalHours}:{span.Minutes:D2}:{span.Seconds:D2}";
    }
}