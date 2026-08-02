using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Oberyn.FishingBuddy.Models;

namespace Oberyn.FishingBuddy.Services
{
    public static class RarityColors
    {
        private static readonly Dictionary<Rarity, Color> Colors = new Dictionary<Rarity, Color>
        {
            { Rarity.Junk,       new Color(170, 170, 170) },
            { Rarity.Basic,      new Color(255, 255, 255) },
            { Rarity.Fine,       new Color(98, 164, 218) },
            { Rarity.Masterwork, new Color(26, 147, 6) },
            { Rarity.Rare,       new Color(252, 208, 11) },
            { Rarity.Exotic,     new Color(255, 164, 5) },
            { Rarity.Ascended,   new Color(251, 62, 141) },
            { Rarity.Legendary,  new Color(136, 71, 255) },
        };

        public static Color Get(Rarity rarity) =>
            Colors.TryGetValue(rarity, out var color) ? color : Color.White;
    }
}