namespace Oberyn.AnglerAssociate.Models
{
    public class Fish
    {
        public string Name { get; set; }
        public Rarity Rarity { get; set; }
        public Cycle Cycle { get; set; }
        public Region Region { get; set; }
        public Location Location { get; set; }

        // up to 4 hole slots per fish - most fish only use Hole1, the rest stay null.
        public FishingHole? Hole1 { get; set; }
        public FishingHole? Hole2 { get; set; }
        public FishingHole? Hole3 { get; set; }
        public FishingHole? Hole4 { get; set; }

        public Bait Bait { get; set; }

        public TimeOfDay TimeOfDay { get; set; }
        public TimeOfDay? TimeOfDay2 { get; set; }
        public TimeOfDay? HigherChance { get; set; }

        // keeping but unused - missing values for JW and Castora, some fish are assigned two different values which makes it even more difficult
        public string FishingPower { get; set; }

        public string Collection { get; set; }
        public int? CollectionId { get; set; }
        public string AvidCollection { get; set; }
        public int? AvidCollectionId { get; set; }

        // collections go not by fish ID but in a set order, so the check goes against the bits array rather than fish ID, please see /v2/achievements?id={CollectionId}
        // also: advid collections share the same order, so no need for a separate index for them
        public int? BitIndex { get; set; }

        //pulle from the fish hint text, hopefully is accurate
        public string FoundIn { get; set; }

        // true for fish catchable during the given state
        public bool IsCatchableAt(TimeOfDay currentState)
        {
            if (TimeOfDay == TimeOfDay.Any)
                return true;

            return TimeOfDay == currentState || TimeOfDay2 == currentState;
        }

        // true for higher chance of catching fish, independent from IsCatchableAt (can be different values)
        public bool HasHigherChance(TimeOfDay currentState)
        {
            return HigherChance.HasValue && HigherChance.Value == currentState;
        }
    }
}
