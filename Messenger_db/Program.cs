//TEST

using Messenger_db;
using Microsoft.EntityFrameworkCore;

internal class Program
{
    private static void Main(string[] args)
    {
        using var db = new Messengers_Db();
        db.Database.Migrate(); // застосувати міграції

        var repo = new UserRepository(db);

        if (!db.Users.Any())
        {
            db.Users.AddRange(
                new User { Username = "alice", DisplayName = "Alice", Email = "alice@example.com", Password = "1234" },
                new User { Username = "bob", DisplayName = "Bob", Email = "bob@example.com", Password = "1234" }
            );
            db.SaveChanges();
        }

        var alice = db.Users.First(u => u.Username == "alice");
        var bob = db.Users.First(u => u.Username == "bob");

        repo.AddContact(alice.Id, bob.Id);

        Console.WriteLine("Search 'ali': " + string.Join(", ", repo.Search("ali").Select(u => u.Username)));
        Console.WriteLine("Contacts: " + string.Join(", ", repo.GetContacts(alice.Id).Select(u => u.Username)));
    }
}
