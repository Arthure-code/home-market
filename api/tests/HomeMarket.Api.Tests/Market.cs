using HomeMarket.Api.Data;
using HomeMarket.Api.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HomeMarket.Api.Tests
{
    // A shop of its own for one test: an SQLite database in memory, alive
    // as long as its connection, with the schema of the real one. The
    // members and listings a test needs are added through it.
    public sealed class Market : IDisposable
    {
        private readonly SqliteConnection _connection;

        public Market()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();
            Context = new MarketContext(new DbContextOptionsBuilder<MarketContext>().UseSqlite(_connection).Options);
            Context.Database.EnsureCreated();
        }

        public MarketContext Context { get; }

        // A member who can sign in, or the store account when the hash is
        // left empty.
        public Account Member(string userName, string passwordHash = "hash")
        {
            var account = new Account { UserName = userName, PasswordHash = passwordHash, CreatedAt = DateTime.UtcNow };
            Context.Accounts.Add(account);
            Context.SaveChanges();
            return account;
        }

        public Product Listing(Account seller, string title, decimal price, int stock, string category = Category.Home, string photo = "")
        {
            var now = DateTime.UtcNow;
            var product = new Product
            {
                SellerId = seller.Id,
                Title = title,
                Brand = title.Split(' ')[0],
                Maker = title.Split(' ')[0] + " Co.",
                Price = price,
                Stock = stock,
                Category = category,
                Photo = photo,
                CreatedAt = now,
                UpdatedAt = now,
            };
            Context.Products.Add(product);
            Context.SaveChanges();
            return product;
        }

        public CartLine InCart(Account buyer, Product product, int quantity)
        {
            var line = new CartLine { AccountId = buyer.Id, ProductId = product.Id, Quantity = quantity, AddedAt = DateTime.UtcNow };
            Context.CartLines.Add(line);
            Context.SaveChanges();
            return line;
        }

        public Message Sent(Account from, Account to, string subject, DateTime? readAt = null)
        {
            var message = new Message { SenderId = from.Id, RecipientId = to.Id, Subject = subject, Body = subject + " body", SentAt = DateTime.UtcNow, ReadAt = readAt };
            Context.Messages.Add(message);
            Context.SaveChanges();
            return message;
        }

        // Forget what was loaded, so the next read comes from the database.
        public void Reload()
        {
            Context.ChangeTracker.Clear();
        }

        public void Dispose()
        {
            Context.Dispose();
            _connection.Dispose();
        }
    }
}
