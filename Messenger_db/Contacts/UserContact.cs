namespace Messenger_db
{
    // Зв'язок "користувач -> контакт" (новий клас; старий Contact не чіпаємо)
    public class UserContact
    {
        public int Id { get; set; }
        public int UserId { get; set; }          // хто додав контакт
        public int ContactUserId { get; set; }    // кого додали в контакти
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public User User { get; set; }            // власник списку контактів
        public User ContactUser { get; set; }     // сам контакт
    }
}
