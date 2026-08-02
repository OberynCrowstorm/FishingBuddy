namespace Oberyn.AnglerAssociate.Models
{
    // fish can reference this up to 3 times:
    // TimeOfDay - primary (can be any)
    // TimeOfDay2 - secondary (usually dawn/dusk)
    // HigherChance - displays increased chance of catching fish
    public enum TimeOfDay
    {
        Any = 0,
        Dawn = 1,
        Day = 2,
        Dusk = 3,
        Night = 4
    }
}
