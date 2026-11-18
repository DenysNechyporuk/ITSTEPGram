namespace Messenger_db
{
    // Пошук користувачів і керування контактами
    public class UserRepository
    {
        private readonly Messengers_Db _db;

        public UserRepository(Messengers_Db db)
        {
            _db = db;
        }

        // Пошук користувачів за Username / DisplayName / Email
        public List<User> Search(string query)
        {
            query = query.Trim();
            return _db.Users
                .Where(u => u.Username.Contains(query)
                         || u.DisplayName.Contains(query)
                         || u.Email.Contains(query))
                .ToList();
        }

        // Додати контакт (без дублів, не можна додати самого себе)
        public void AddContact(int userId, int contactUserId)
        {
            if (userId == contactUserId)
                throw new ArgumentException("Не можна додати самого себе в контакти.");

            bool exists = _db.UserContacts.Any(c =>
                c.UserId == userId && c.ContactUserId == contactUserId);

            if (!exists)
            {
                _db.UserContacts.Add(new UserContact
                {
                    UserId = userId,
                    ContactUserId = contactUserId
                });
                _db.SaveChanges();
            }
        }

        // Видалити контакт
        public void RemoveContact(int userId, int contactUserId)
        {
            UserContact? contact = _db.UserContacts.FirstOrDefault(c =>
                c.UserId == userId && c.ContactUserId == contactUserId);

            if (contact != null)
            {
                _db.UserContacts.Remove(contact);
                _db.SaveChanges();
            }
        }

        // Список контактів користувача
        public List<User> GetContacts(int userId)
        {
            return _db.UserContacts
                .Where(c => c.UserId == userId)
                .Select(c => c.ContactUser)
                .ToList();
        }
    }
}
