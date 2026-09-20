using HomeMarket.Api.Models;

namespace HomeMarket.Api.Data
{
    // The catalogue the store opens with: twenty-eight everyday things,
    // listed by the store account, each in a category and with a stock.
    // The photos are public pictures on Unsplash, linked by URL under the
    // Unsplash License.
    public static class Catalogue
    {
        public const string StoreAccount = "homemarket";

        public static IEnumerable<Product> Listings(int sellerId, DateTime at)
        {
            var products = new[]
            {
            Listing("Everyday backpack, navy", "Nordpack", "Nordpack Oy", 89, Category.BagsAndTravel, 14, "https://images.unsplash.com/photo-1553062407-98eeb64c6a62",
                "Twenty-litre backpack in water-repellent canvas, padded laptop sleeve, two side pockets."),
            Listing("Leather backpack, chestnut", "Harlow", "Harlow Leather Co.", 179, Category.BagsAndTravel, 5, "https://images.unsplash.com/photo-1622560480605-d83c853bc5c3",
                "Full-grain leather, brass zips, fits a 14-inch laptop. Ages well."),
            Listing("Compact umbrella, yellow", "Pluvia", "Pluvia Ltd.", 24, Category.Accessories, 40, "https://images.unsplash.com/photo-1554219374-6d2c029f855e",
                "Folds to 25 cm, opens with one press, fibreglass ribs that flex in the wind."),
            Listing("Desk fan, chrome", "Breeza", "Breeza Appliances", 59, Category.Home, 9, "https://images.unsplash.com/photo-1565151443833-29bf2ba5dd8d",
                "Three speeds, oscillating head, a whisper on the lowest setting."),
            Listing("Mini USB fan, copper", "Breeza", "Breeza Appliances", 22, Category.Home, 25, "https://images.unsplash.com/photo-1564510182791-29645da7fac4",
                "Runs on any USB port, tilts, metal cage. For the corner of a desk."),
            Listing("Smartphone, 128 GB", "Nova", "Nova Mobile", 699, Category.Electronics, 7, "https://images.unsplash.com/photo-1571380401583-72ca84994796",
                "6.1-inch OLED, dual camera, two days of battery, unlocked for any carrier."),
            Listing("Wireless headphones, noise cancelling", "Sonora", "Sonora Audio", 249, Category.Electronics, 12, "https://images.unsplash.com/photo-1618366712010-f4ae9c647dcb",
                "Thirty hours per charge, active noise cancelling, folds flat in its case."),
            Listing("Wired headphones, white", "Sonora", "Sonora Audio", 49, Category.Electronics, 30, "https://images.unsplash.com/photo-1577174881658-0f30ed549adc",
                "Closed back, 40 mm drivers, 1.2 m cable with a microphone."),
            Listing("Electric kettle, 1.7 L", "Kettl", "Kettl Home", 45, Category.Kitchen, 18, "https://images.unsplash.com/photo-1748408082799-94daff13e792",
                "Boils a litre in three minutes, auto shut-off, hidden heating element."),
            Listing("Two-slice toaster", "Kettl", "Kettl Home", 39, Category.Kitchen, 11, "https://images.unsplash.com/photo-1613221699807-4940ba9b83f4",
                "Six browning levels, bagel and defrost settings, removable crumb tray."),
            Listing("Table lamp, brass", "Lumo", "Lumo Lighting", 65, Category.Home, 6, "https://images.unsplash.com/photo-1570974802254-4b0ad1a755f5",
                "Adjustable neck, E14 socket, warm brass finish. Bulb not included."),
            Listing("Twin-bell alarm clock", "Tick", "Tick Clockworks", 19, Category.Home, 22, "https://images.unsplash.com/photo-1524678714210-9917a6c619c2",
                "Loud enough for heavy sleepers. One AA battery lasts a year."),
            Listing("Insulated bottle, 750 ml, green", "Quokka", "Quokka Bottles", 32, Category.Kitchen, 35, "https://images.unsplash.com/photo-1602143407151-7111542de6e8",
                "Keeps cold for 24 hours and hot for 12. Stainless steel, no plastic taste."),
            Listing("Steel bottle, 1 L, grey", "Quokka", "Quokka Bottles", 35, Category.Kitchen, 28, "https://images.unsplash.com/photo-1544003484-3cd181d17917",
                "Wide mouth for ice, screw cap with a carry loop, dishwasher safe."),
            Listing("Sunglasses, club frame", "Ocular", "Ocular Optics", 79, Category.Accessories, 15, "https://images.unsplash.com/photo-1584036553516-bf83210aa16c",
                "Polarised lenses, UV400, acetate frame with metal bridge."),
            Listing("Watch, white silicone", "Minute", "Minute Watches", 120, Category.Accessories, 10, "https://images.unsplash.com/photo-1523275335684-37898b6baf30",
                "Quartz movement, 38 mm case, water resistant to 50 m."),
            Listing("Chronograph, leather strap", "Minute", "Minute Watches", 260, Category.Accessories, 3, "https://images.unsplash.com/photo-1542496658-e33a6d0d50f6",
                "Three sub-dials, tachymeter bezel, sapphire crystal, 42 mm."),
            Listing("Running sneakers", "Stride", "Stride Footwear", 110, Category.SportsAndOutdoors, 16, "https://images.unsplash.com/photo-1560769629-975ec94e6a86",
                "Breathable mesh upper, cushioned midsole, 260 g in size 42."),
            Listing("Robot vacuum", "Sweepr", "Sweepr Robotics", 329, Category.Home, 4, "https://images.unsplash.com/photo-1653990480360-31a12ce9723e",
                "Maps the flat, avoids cables, returns to its dock. Two hours per charge."),
            Listing("Portable speaker", "Sonora", "Sonora Audio", 89, Category.Electronics, 20, "https://images.unsplash.com/photo-1608043152269-423dbba4e7e1",
                "Waterproof, twelve hours of play, pairs two for stereo."),
            Listing("Mechanical keyboard, 75%", "Keycap", "Keycap Works", 139, Category.Office, 8, "https://images.unsplash.com/photo-1618384887929-16ec33fab9ef",
                "Hot-swappable switches, PBT keycaps, USB-C, no software needed."),
            Listing("Wireless mouse, black", "Keycap", "Keycap Works", 59, Category.Office, 26, "https://images.unsplash.com/photo-1615663245857-ac93bb7c39e7",
                "63 g, 26 000 DPI sensor, 70 hours on a charge."),
            Listing("Ceramic mug, white, 350 ml", "Kettl", "Kettl Home", 12, Category.Kitchen, 60, "https://images.unsplash.com/photo-1650959858546-d09833d5317b",
                "Thick walls keep coffee warm, dishwasher and microwave safe."),
            Listing("Office chair, blue", "Sitwell", "Sitwell Furniture", 249, Category.Office, 5, "https://images.unsplash.com/photo-1541558869434-2840d308329a",
                "Adjustable height and tilt, padded arms, five-wheel base."),
            Listing("Hair dryer, red", "Aero", "Aero Care", 85, Category.PersonalCare, 13, "https://images.unsplash.com/photo-1727755868077-22f0d2ff8353",
                "1 800 W, ionic, two speeds and three heat settings, cool-shot button."),
            Listing("Electric toothbrush", "Brisk", "Brisk Oral", 69, Category.PersonalCare, 17, "https://images.unsplash.com/photo-1641130331708-dd0cc94ae8e5",
                "Sonic, two-minute timer, three weeks per charge, travel case included."),
            Listing("DSLR camera with 18-55 mm lens", "Optik", "Optik Imaging", 549, Category.Electronics, 2, "https://images.unsplash.com/photo-1502920917128-1aa500764cbd",
                "24 megapixels, full HD video, articulated screen. Body and kit lens."),
            Listing("Tablet, 11-inch", "Nova", "Nova Mobile", 499, Category.Electronics, 9, "https://images.unsplash.com/photo-1561154464-82e9adf32764",
                "Laminated display, stylus support, 256 GB, all-day battery."),
            };
            foreach (var product in products)
            {
                product.SellerId = sellerId;
                product.CreatedAt = at;
                product.UpdatedAt = at;
                at = at.AddMinutes(1);
            }
            return products;
        }

        private static Product Listing(string title, string brand, string maker, decimal price, string category, int stock, string photo, string description)
        {
            return new Product
            {
                Title = title,
                Brand = brand,
                Maker = maker,
                Price = price,
                Category = category,
                Stock = stock,
                Photo = photo,
                Description = description,
            };
        }
    }
}
