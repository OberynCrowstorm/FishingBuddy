using System;
using System.Collections.Generic;
using System.Linq;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Modules.Managers;
using Microsoft.Xna.Framework;
using Oberyn.FishingBuddy.Models;
using Oberyn.FishingBuddy.Services;

namespace Oberyn.FishingBuddy.Controls
{
    public class MainView : Panel
    {
        private static readonly Dictionary<Cycle, Region[]> RegionsByCycle = new Dictionary<Cycle, Region[]>
        {
            { Cycle.Tyria, new[] { Region.Tyria, Region.CrystalDesert, Region.HornOfMaguuma, Region.Janthir } },
            { Cycle.CanthaCastora, new[] { Region.Cantha, Region.Castora } },
            { Cycle.Global, new[] { Region.Global } },
        };

        private readonly ContentsManager _contentsManager;
        private readonly AchievementProgressService _achievementProgress;
        private readonly Label _dailyLabel;
        private readonly DayNightBanner _tyriaBanner;
        private readonly DayNightBanner _canthaBanner;
        private readonly Dropdown _continentDropdown;
        private readonly Dropdown _regionDropdown;
        private readonly Dropdown _achievementDropdown;
        private readonly Dropdown _baitDropdown;
        private readonly Dropdown _availableNowDropdown;
        private readonly Dropdown _hideCollectedDropdown;
        private readonly TextBox _searchBox;
        private readonly Panel _tableRows;
        private readonly List<Panel> _rowControls = new List<Panel>();
        private readonly Dictionary<string, Region> _regionDisplayToValue = new Dictionary<string, Region>();
        private readonly Dictionary<string, Bait> _baitDisplayToValue = new Dictionary<string, Bait>();

        private const int RowHeight = 40;
        private const int TableTop = 25;

        public MainView(ContentsManager contentsManager, AchievementProgressService achievementProgress, int contentHeight)
        {
            _contentsManager = contentsManager;
            _achievementProgress = achievementProgress;

            _dailyLabel = new Label
            {
                Parent = this,
                Text = $"Today's daily: {DailyFisherRotation.GetToday()}",
                Location = new Point(0, 0),
                Width = 300,
                Height = 24,
            };

            _tyriaBanner = new DayNightBanner(contentsManager, "tyria", "Tyria", Cycle.Tyria)
            {
                Parent = this,
                Location = new Point(0, 40),
            };

            _canthaBanner = new DayNightBanner(contentsManager, "cantha", "Cantha/Castora", Cycle.CanthaCastora)
            {
                Parent = this,
                Location = new Point(130, 40),
            };

            var searchLabel = new Label
            {
                Parent = this,
                Text = "Search fish",
                Location = new Point(0, 460),
                Width = 250,
                Height = 20,
            };

            _searchBox = new TextBox
            {
                Parent = this,
                PlaceholderText = "Fish name...",
                Location = new Point(0, 480),
                Width = 225,
                Height = 24,
            };
            _searchBox.TextChanged += (s, e) => RebuildRows();

            var searchClearButton = new Label
            {
                Parent = this,
                Text = "X",
                Location = new Point(228, 480),
                Width = 22,
                Height = 24,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Middle,
                BasicTooltipText = "Clear search",
            };
            searchClearButton.LeftMouseButtonReleased += (s, e) =>
            {
                _searchBox.Text = string.Empty;
                RebuildRows();
            };

            const int filterX = 340;
            const int colWidth = 200;
            const int colGap = 20;
            int col1 = filterX;
            int col2 = filterX + colWidth + colGap;
            int col3 = filterX + 2 * (colWidth + colGap);
            int y = 0;

            var continentLabel = new Label { Parent = this, Text = "Filter by continent", Location = new Point(col1, y), Width = colWidth, Height = 20 };
            var regionLabel = new Label { Parent = this, Text = "Filter by region", Location = new Point(col2, y), Width = colWidth, Height = 20 };
            var baitLabel = new Label { Parent = this, Text = "Filter by bait", Location = new Point(col3, y), Width = colWidth, Height = 20 };
            y += 20;

            _continentDropdown = new Dropdown { Parent = this, Location = new Point(col1, y), Width = colWidth };
            foreach (var cycle in RegionsByCycle.Keys)
                _continentDropdown.Items.Add(CycleLabel(cycle));
            _continentDropdown.ValueChanged += (s, e) => { RebuildRegionDropdown(); RebuildAchievementDropdown(); RebuildBaitDropdown(); RebuildRows(); };

            _regionDropdown = new Dropdown { Parent = this, Location = new Point(col2, y), Width = colWidth };
            _regionDropdown.ValueChanged += (s, e) => { RebuildAchievementDropdown(); RebuildBaitDropdown(); RebuildRows(); };

            _baitDropdown = new Dropdown { Parent = this, Location = new Point(col3, y), Width = colWidth };
            _baitDropdown.ValueChanged += (s, e) => RebuildRows();
            y += 30;

            var achievementLabel = new Label { Parent = this, Text = "Filter by achievement", Location = new Point(col1, y), Width = colWidth, Height = 20 };
            var hideCollectedLabel = new Label { Parent = this, Text = "Hide collected", Location = new Point(col2, y), Width = colWidth, Height = 20 };
            var availableLabel = new Label { Parent = this, Text = "Show only available now", Location = new Point(col3, y), Width = colWidth, Height = 20 };
            y += 20;

            _achievementDropdown = new Dropdown { Parent = this, Location = new Point(col1, y), Width = colWidth };
            _achievementDropdown.ValueChanged += (s, e) => RebuildRows();

            _hideCollectedDropdown = new Dropdown { Parent = this, Location = new Point(col2, y), Width = colWidth };
            _hideCollectedDropdown.Items.Add("No");
            _hideCollectedDropdown.Items.Add("Yes");
            _hideCollectedDropdown.ValueChanged += (s, e) => RebuildRows();

            _availableNowDropdown = new Dropdown { Parent = this, Location = new Point(col3, y), Width = colWidth };
            _availableNowDropdown.Items.Add("No");
            _availableNowDropdown.Items.Add("Yes");
            _availableNowDropdown.ValueChanged += (s, e) => RebuildRows();
            y += 40;

            // --- Table ---
            BuildTableHeader(filterX, y);

            const int tableWidth = 712;

            _tableRows = new Panel
            {
                Parent = this,
                Location = new Point(filterX, y + TableTop),
                Width = tableWidth,
                Height = Math.Max(contentHeight - (y + TableTop) - 10, 100),
                CanScroll = true,
            };

            // Initial population
            RebuildRegionDropdown();
            RebuildAchievementDropdown();
            RebuildBaitDropdown();
            RebuildRows();
        }
        public void RefreshBanners()
        {
            _tyriaBanner.Refresh();
            _canthaBanner.Refresh();
        }

        private static string CycleLabel(Cycle cycle)
        {
            switch (cycle)
            {
                case Cycle.Tyria: return "Tyria";
                case Cycle.CanthaCastora: return "Cantha/Castora";
                case Cycle.Global: return "Global";
                default: return cycle.ToString();
            }
        }

        private void BuildTableHeader(int x, int y)
        {
            string[] headers = { "Fish", "Found in", "Rarity", "Hole", "Bait", "Time" };
            int[] widths = { 150, 170, 60, 130, 80, 80 }; // sums to 670, well within the 712px table width
            int cx = x;

            var headerFont = GameService.Content.GetFont(
                ContentService.FontFace.Menomonia, ContentService.FontSize.Size16, ContentService.FontStyle.Bold);

            for (int i = 0; i < headers.Length; i++)
            {
                new Label
                {
                    Parent = this,
                    Text = headers[i],
                    Font = headerFont,
                    Location = new Point(cx, y),
                    Width = widths[i],
                    Height = 24,
                    HorizontalAlignment = i == 0 ? HorizontalAlignment.Left : HorizontalAlignment.Center,
                };
                cx += widths[i];
            }
        }

        private Cycle SelectedCycle()
        {
            var index = _continentDropdown.SelectedItem != null
                ? _continentDropdown.Items.IndexOf(_continentDropdown.SelectedItem)
                : 0;
            return RegionsByCycle.Keys.ElementAt(Math.Max(index, 0));
        }

        private void RebuildRegionDropdown()
        {
            var cycle = SelectedCycle();
            _regionDropdown.Items.Clear();
            _regionDisplayToValue.Clear();

            foreach (var region in RegionsByCycle[cycle])
            {
                var display = EnumDisplay.Format(region);
                _regionDropdown.Items.Add(display);
                _regionDisplayToValue[display] = region;
            }

            if (_regionDropdown.Items.Count > 0)
                _regionDropdown.SelectedItem = _regionDropdown.Items[0];
        }

        private Region? SelectedRegion()
        {
            if (_regionDropdown.SelectedItem == null)
                return null;

            return _regionDisplayToValue.TryGetValue(_regionDropdown.SelectedItem, out var region)
                ? region
                : (Region?)null;
        }

        private void RebuildAchievementDropdown()
        {
            _achievementDropdown.Items.Clear();
            _achievementDropdown.Items.Add("All");

            var region = SelectedRegion();
            if (region == null) return;

            var collectionNames = FishCatalog.All
                .Where(f => f.Region == region.Value && f.Collection != null)
                .Select(f => f.Collection)
                .Distinct()
                .OrderBy(name => name);

            foreach (var name in collectionNames)
                _achievementDropdown.Items.Add(name);

            _achievementDropdown.SelectedItem = "All";
        }

        private void RebuildBaitDropdown()
        {
            _baitDropdown.Items.Clear();
            _baitDisplayToValue.Clear();
            _baitDropdown.Items.Add("All");

            var region = SelectedRegion();
            if (region == null) return;

            var baits = FishCatalog.All
                .Where(f => f.Region == region.Value)
                .Select(f => f.Bait)
                .Distinct()
                .OrderBy(b => b.ToString());

            foreach (var bait in baits)
            {
                var display = EnumDisplay.Format(bait);
                _baitDropdown.Items.Add(display);
                _baitDisplayToValue[display] = bait;
            }

            _baitDropdown.SelectedItem = "All";
        }

        private void RebuildRows()
        {
            foreach (var row in _rowControls)
                row.Dispose();
            _rowControls.Clear();

            bool onlyAvailableNow = _availableNowDropdown.SelectedItem == "Yes";
            bool hideCollected = _hideCollectedDropdown.SelectedItem == "Yes";
            string searchText = _searchBox.Text?.Trim();

            IEnumerable<Fish> fish;

            if (!string.IsNullOrEmpty(searchText))
            {
                fish = FishCatalog.All.Where(f =>
                    f.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            else
            {
                var region = SelectedRegion();
                if (region == null) return;

                string achievementFilter = _achievementDropdown.SelectedItem;
                string baitFilter = _baitDropdown.SelectedItem;

                fish = FishCatalog.All.Where(f => f.Region == region.Value);

                if (achievementFilter != null && achievementFilter != "All")
                    fish = fish.Where(f => f.Collection == achievementFilter);

                if (baitFilter != null && baitFilter != "All" && _baitDisplayToValue.TryGetValue(baitFilter, out var baitValue))
                    fish = fish.Where(f => f.Bait == baitValue);

                if (onlyAvailableNow)
                    fish = fish.Where(IsAvailableNow);

                if (hideCollected)
                    fish = fish.Where(f => !_achievementProgress.IsFishCaught(f));
            }

            int rowY = 0;
            foreach (var f in fish)
            {
                var row = BuildRow(f, rowY);
                _rowControls.Add(row);
                rowY += RowHeight;
            }
        }

        private static bool IsAvailableNow(Fish fish)
        {
            if (fish.Cycle == Cycle.Global)
                return fish.TimeOfDay == TimeOfDay.Any;

            var (state, _) = TyrianClock.GetState(fish.Cycle);
            return fish.IsCatchableAt(state);
        }

        private Panel BuildRow(Fish fish, int y)
        {
            var row = new Panel
            {
                Parent = _tableRows,
                Location = new Point(0, y),
                Width = 670,
                Height = RowHeight,
            };

            new Label { Parent = row, Text = fish.Name, Location = new Point(0, 0), Width = 150, Height = RowHeight, VerticalAlignment = VerticalAlignment.Middle };
            new Label { Parent = row, Text = FormatFoundInShort(fish), BasicTooltipText = FormatFoundInFull(fish), Location = new Point(150, 0), Width = 170, Height = RowHeight, VerticalAlignment = VerticalAlignment.Middle, HorizontalAlignment = HorizontalAlignment.Center };
            new Label { Parent = row, Text = fish.Rarity.ToString(), TextColor = RarityColors.Get(fish.Rarity), Location = new Point(320, 0), Width = 60, Height = RowHeight, VerticalAlignment = VerticalAlignment.Middle, HorizontalAlignment = HorizontalAlignment.Center };
            new Label { Parent = row, Text = FormatHolesShort(fish), BasicTooltipText = FormatHolesFull(fish), Location = new Point(380, 0), Width = 130, Height = RowHeight, VerticalAlignment = VerticalAlignment.Middle, HorizontalAlignment = HorizontalAlignment.Center };

            BuildBaitCell(row, fish, new Point(510, 0));

            BuildTimeOfDayCell(row, fish, new Point(590, 0));

            return row;
        }

        private const double PixelsPerChar = 7.0;

        private static string TruncateToWidth(string text, int columnWidthPx)
        {
            int maxChars = (int)(columnWidthPx / PixelsPerChar);
            if (text.Length <= maxChars)
                return text;

            return text.Substring(0, Math.Max(maxChars - 3, 1)) + "...";
        }
        private static string FormatFoundInShort(Fish fish)
        {
            if (string.IsNullOrEmpty(fish.FoundIn))
                return EnumDisplay.Format(fish.Location);

            var firstPart = fish.FoundIn.Split(new[] { " and ", "," }, StringSplitOptions.None)[0].Trim();
            bool hasMore = firstPart.Length < fish.FoundIn.Length;

            var shortText = hasMore ? $"{firstPart}, ..." : firstPart;
            return TruncateToWidth(shortText, 170);
        }

        private static string FormatFoundInFull(Fish fish)
        {
            return string.IsNullOrEmpty(fish.FoundIn)
                ? EnumDisplay.Format(fish.Location)
                : fish.FoundIn;
        }

        private void BuildTimeOfDayCell(Panel row, Fish fish, Point location)
        {
            var textures = TimeOfDayIcons.GetTextures(_contentsManager, fish);
            var tooltip = FormatTimeOfDayFull(fish);
            const int iconSize = 28;
            const int gap = 4;
            int totalWidth = textures.Count * iconSize + (textures.Count - 1) * gap;
            int startX = location.X + (80 - totalWidth) / 2;
            int iconY = location.Y + (RowHeight - iconSize) / 2;

            for (int i = 0; i < textures.Count; i++)
            {
                new Image
                {
                    Parent = row,
                    Texture = textures[i],
                    Location = new Point(startX + i * (iconSize + gap), iconY),
                    Size = new Point(iconSize, iconSize),
                    BasicTooltipText = tooltip,
                };
            }
        }

        private void BuildBaitCell(Panel row, Fish fish, Point location)
        {
            var texture = BaitIcons.GetTexture(fish.Bait);
            if (texture != null)
            {
                new Image
                {
                    Parent = row,
                    Texture = texture,
                    Location = new Point(location.X + 24, (RowHeight - 32) / 2),
                    Size = new Point(32, 32),
                    BasicTooltipText = EnumDisplay.Format(fish.Bait),
                };
            }
            else
            {
                var baitText = EnumDisplay.Format(fish.Bait);
                new Label
                {
                    Parent = row,
                    Text = TruncateToWidth(baitText, 80),
                    BasicTooltipText = baitText,
                    Location = location,
                    Width = 80,
                    Height = RowHeight,
                    VerticalAlignment = VerticalAlignment.Middle,
                    HorizontalAlignment = HorizontalAlignment.Center,
                };
            }
        }

        private static string FormatHolesShort(Fish fish)
        {
            var holes = new[] { fish.Hole1, fish.Hole2, fish.Hole3, fish.Hole4 }
                .Where(h => h.HasValue)
                .Select(h => EnumDisplay.Format(h.Value))
                .ToList();

            if (holes.Count == 0) return "Any";
            var shortText = holes.Count == 1 ? holes[0] : $"{holes[0]}, ...";
            return TruncateToWidth(shortText, 130);
        }

        private static string FormatHolesFull(Fish fish)
        {
            var holes = new[] { fish.Hole1, fish.Hole2, fish.Hole3, fish.Hole4 }
                .Where(h => h.HasValue)
                .Select(h => EnumDisplay.Format(h.Value));

            var joined = string.Join(", ", holes);
            return string.IsNullOrEmpty(joined) ? "Any" : joined;
        }

        private static string FormatTimeOfDayFull(Fish fish)
        {
            if (fish.TimeOfDay == TimeOfDay.Any)
            {
                return fish.HigherChance.HasValue
                    ? $"Any (favors {fish.HigherChance})"
                    : "Any";
            }

            if (fish.TimeOfDay2.HasValue)
            {
                var text = $"{fish.TimeOfDay}/{fish.TimeOfDay2}";
                return fish.HigherChance.HasValue ? $"{text} (favors {fish.HigherChance})" : text;
            }

            return fish.TimeOfDay.ToString();
        }
    }
}