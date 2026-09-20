namespace HomeMarket.Api.Data
{
    // The shop's categories, as stored and as shown. A listing must name
    // one of them; the catalogue can be browsed by any of them.
    public static class Category
    {
        public const string Electronics = "Electronics";
        public const string Home = "Home";
        public const string Kitchen = "Kitchen";
        public const string Office = "Office";
        public const string Accessories = "Accessories";
        public const string BagsAndTravel = "Bags & travel";
        public const string PersonalCare = "Personal care";
        public const string SportsAndOutdoors = "Sports & outdoors";

        public static readonly IReadOnlyList<string> All = new[]
        {
            Electronics, Home, Kitchen, Office, Accessories, BagsAndTravel, PersonalCare, SportsAndOutdoors,
        };

        public static bool Exists(string name)
        {
            return All.Contains(name);
        }
    }
}
