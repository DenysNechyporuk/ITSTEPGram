using Messenger_db;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace ITSTEPGram
{
    public class ChatItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Letter { get; set; } = "";
        public string Info { get; set; } = "";
        public string LastMessage { get; set; } = "";
        public string LastTime { get; set; } = "";
        public DateTime SortTime { get; set; }
        public bool IsGroup { get; set; }
        public string OtherLogin { get; set; } = "";
    }

    public class MessageItem
    {
        public string SenderName { get; set; } = "";
        public string Text { get; set; } = "";
        public string Time { get; set; } = "";
        public bool IsMine { get; set; }
    }

    public class MemberItem
    {
        public string Name { get; set; } = "";
        public string Login { get; set; } = "";
        public string Letter { get; set; } = "";
        public string Role { get; set; } = "";
    }

    public class UserItem
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public string Name { get; set; } = "";
        public string Login { get; set; } = "";
        public string Letter { get; set; } = "";
        public string Status { get; set; } = "";
        public bool IsContact { get; set; }
        public string RequestStatus { get; set; } = "";
    }

    public static class DataService
    {
        public static Messenger_db.Client? Login(string login, string password)
        {
            Methods_Login_Reg methods = new Methods_Login_Reg();
            if (!methods.LoginMember(login, password))
            {
                return null;
            }

            using (Messengers_Db db = new Messengers_Db())
            {
                Messenger_db.Client? client = db.Clients.FirstOrDefault(c => c.Login == login);
                if (client == null)
                {
                    return null;
                }

                client.IP = GetLocalIp();
                db.SaveChanges();

                User user = EnsureUser(db, client);
                user.IsOnline = true;
                user.LastSeenAt = DateTime.Now;
                db.SaveChanges();

                return client;
            }
        }
        public static List<UserItem> GetIncomingRequests(string myLogin)
        {
            using (Messengers_Db db = new Messengers_Db())
            {
                User? me = db.Users.FirstOrDefault(u => u.Username == myLogin);

                if (me == null)
                    return new List<UserItem>();

                return db.ContactRequests
                    .Where(r => r.ReceiverUserId == me.Id && r.Status == "Pending")
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => new UserItem
                    {
                        Id = r.Sender.Id,
                        RequestId = r.Id,
                        Name = r.Sender.DisplayName,
                        Login = r.Sender.Username,
                        Letter = r.Sender.DisplayName.Substring(0, 1),
                        Status = r.Sender.IsOnline ? "у мережі" : "не в мережі",
                        IsContact = false,
                        RequestStatus = "PendingIncoming"
                    })
                    .ToList();
            }
        }

        public static List<int> GetIncomingRequestIds(string myLogin)
        {
            using (Messengers_Db db = new Messengers_Db())
            {
                User? me = db.Users.FirstOrDefault(u => u.Username == myLogin);

                if (me == null)
                    return new List<int>();

                return db.ContactRequests
                    .Where(r => r.ReceiverUserId == me.Id && r.Status == "Pending")
                    .Select(r => r.Id)
                    .ToList();
            }
        }
        public static void RejectContactRequest(string myLogin, int requestId)
        {
            using Messengers_Db db = new Messengers_Db();

            User? me = db.Users.FirstOrDefault(u => u.Username == myLogin);

            if (me == null)
                return;

            ContactRequest? request = db.ContactRequests.FirstOrDefault(r =>
                r.Id == requestId &&
                r.ReceiverUserId == me.Id &&
                r.Status == "Pending");

            if (request == null)
                return;

            request.Status = "Rejected";
            db.SaveChanges();
        }

        public static void AcceptContactRequest(string myLogin, int requestId)
        {
            using (Messengers_Db db = new Messengers_Db())
            {
                User? me = db.Users.FirstOrDefault(u => u.Username == myLogin);

                if (me == null)
                    return;

                ContactRequest? request = db.ContactRequests.FirstOrDefault(r =>
                    r.Id == requestId &&
                    r.ReceiverUserId == me.Id &&
                    r.Status == "Pending");

                if (request == null)
                    return;

                bool firstExists = db.UserContacts.Any(c =>
                    c.UserId == me.Id &&
                    c.ContactUserId == request.SenderUserId);

                if (!firstExists)
                {
                    db.UserContacts.Add(new UserContact
                    {
                        UserId = me.Id,
                        ContactUserId = request.SenderUserId
                    });
                }

                bool secondExists = db.UserContacts.Any(c =>
                    c.UserId == request.SenderUserId &&
                    c.ContactUserId == me.Id);

                if (!secondExists)
                {
                    db.UserContacts.Add(new UserContact
                    {
                        UserId = request.SenderUserId,
                        ContactUserId = me.Id
                    });
                }

                request.Status = "Accepted";

                ContactRequest? reverseRequest = db.ContactRequests.FirstOrDefault(r =>
                    r.SenderUserId == me.Id &&
                    r.ReceiverUserId == request.SenderUserId &&
                    r.Status == "Pending");

                if (reverseRequest != null)
                    reverseRequest.Status = "Accepted";

                db.SaveChanges();
            }
        }

        public static string Register(string name, string login, string password)
        {
            Methods_Login_Reg methods = new Methods_Login_Reg();
            string error = methods.Registration(login, password, name);
            if (error != "")
            {
                return error;
            }

            using (Messengers_Db db = new Messengers_Db())
            {
                Messenger_db.Client? client = db.Clients.FirstOrDefault(c => c.Login == login);
                if (client != null)
                {
                    client.IP = GetLocalIp();
                    EnsureUser(db, client);
                    db.SaveChanges();
                }
            }
            return "";
        }

        public static void SetOffline(string login)
        {
            using (Messengers_Db db = new Messengers_Db())
            {
                User? user = db.Users.FirstOrDefault(u => u.Username == login);
                if (user != null)
                {
                    user.IsOnline = false;
                    user.LastSeenAt = DateTime.Now;
                    db.SaveChanges();
                }
            }
        }

        public static bool IsPasswordStrong(string password)
        {
            return Regex.IsMatch(password, @".{6,}")
                && Regex.IsMatch(password, @"[A-Z]+")
                && Regex.IsMatch(password, @"[a-z]+")
                && Regex.IsMatch(password, @"[0-9]+")
                && Regex.IsMatch(password, @"[_-]+")
                && Regex.IsMatch(password, @"^[A-Za-z0-9_-]+$");
        }

        public static string GetBio(string login)
        {
            using (Messengers_Db db = new Messengers_Db())
            {
                User? user = db.Users.FirstOrDefault(u => u.Username == login);
                if (user == null || user.Bio == null)
                {
                    return "";
                }
                return user.Bio;
            }
        }

        public static void UpdateProfile(int userId, string name, string bio, string newPassword)
        {
            using (Messengers_Db db = new Messengers_Db())
            {
                Messenger_db.Client? client = db.Clients.Find(userId);
                if (client == null)
                {
                    return;
                }

                client.Name = name;
                if (newPassword != "")
                {
                    client.Password = newPassword;
                }

                User user = EnsureUser(db, client);
                user.DisplayName = name;
                user.Bio = bio;
                user.Password = client.Password;

                db.SaveChanges();
            }
        }

        public static List<ChatItem> GetChats(int userId, string search)
        {
            List<ChatItem> result = new List<ChatItem>();
            using (Messengers_Db db = new Messengers_Db())
            {
                List<Chat> chats = db.ChatMembers
                    .Where(m => m.ClientId == userId)
                    .Select(m => m.Chats)
                    .ToList();

                foreach (Chat chat in chats)
                {
                    ChatItem item = new ChatItem();
                    item.Id = chat.Id;
                    item.IsGroup = chat.IsGroup;

                    List<ChatMember> members = db.ChatMembers
                        .Include(m => m.Clients)
                        .Where(m => m.ChatId == chat.Id)
                        .ToList();

                    if (chat.IsGroup)
                    {
                        item.Name = chat.Name;
                        item.Info = "група · учасників: " + members.Count;
                    }
                    else
                    {
                        ChatMember? other = members.FirstOrDefault(m => m.ClientId != userId);
                        if (other != null)
                        {
                            item.Name = other.Clients.Name;
                            item.OtherLogin = other.Clients.Login;
                            User? otherUser = db.Users.FirstOrDefault(u => u.Username == other.Clients.Login);
                            item.Info = GetStatus(otherUser);
                        }
                        else
                        {
                            item.Name = "Чат";
                            item.Info = "співрозмовник покинув чат";
                        }
                    }
                    item.Letter = GetLetter(item.Name);

                    Message? last = db.Messages
                        .Include(m => m.Clients)
                        .Where(m => m.ChatId == chat.Id)
                        .OrderByDescending(m => m.SendTime)
                        .FirstOrDefault();

                    if (last != null)
                    {
                        if (last.ClientId == userId)
                        {
                            item.LastMessage = "Ви: " + last.Text;
                        }
                        else if (chat.IsGroup)
                        {
                            item.LastMessage = last.Clients.Name + ": " + last.Text;
                        }
                        else
                        {
                            item.LastMessage = last.Text;
                        }
                        item.LastTime = FormatTime(last.SendTime);
                        item.SortTime = last.SendTime;
                    }
                    else
                    {
                        item.LastMessage = "Немає повідомлень";
                        item.SortTime = chat.CreatedTime;
                    }

                    if (search == "" || item.Name.ToLower().Contains(search.ToLower()))
                    {
                        result.Add(item);
                    }
                }
            }
            return result.OrderByDescending(c => c.SortTime).ToList();
        }

        public static List<int> GetGroupChatIds(int userId)
        {
            using (Messengers_Db db = new Messengers_Db())
            {
                return db.ChatMembers
                    .Where(m => m.ClientId == userId && m.Chats.IsGroup)
                    .Select(m => m.ChatId)
                    .ToList();
            }
        }

        public static List<MessageItem> GetMessages(int chatId, int userId, bool isGroup)
        {
            List<MessageItem> result = new List<MessageItem>();
            using (Messengers_Db db = new Messengers_Db())
            {
                List<Message> messages = db.Messages
                    .Include(m => m.Clients)
                    .Where(m => m.ChatId == chatId)
                    .OrderBy(m => m.SendTime)
                    .ToList();

                foreach (Message message in messages)
                {
                    MessageItem item = new MessageItem();
                    item.Text = message.Text;
                    item.Time = message.SendTime.ToString("HH:mm");
                    item.IsMine = message.ClientId == userId;
                    if (isGroup && !item.IsMine)
                    {
                        item.SenderName = message.Clients.Name;
                    }
                    result.Add(item);
                }
            }
            return result;
        }

        public static void AddMessage(int chatId, int userId, string text)
        {
            using (Messengers_Db db = new Messengers_Db())
            {
                Message message = new Message();
                message.ChatId = chatId;
                message.ClientId = userId;
                message.Text = text;
                message.SendTime = DateTime.Now;

                db.Messages.Add(message);
                db.SaveChanges();
            }
        }

        public static int CreatePrivateChat(int userId, string login, out string error)
        {
            error = "";
            using (Messengers_Db db = new Messengers_Db())
            {
                Messenger_db.Client? other = db.Clients.FirstOrDefault(c => c.Login == login);
                if (other == null)
                {
                    error = "Користувача з таким email не знайдено";
                    return 0;
                }
                if (other.Id == userId)
                {
                    error = "Не можна створити чат із собою";
                    return 0;
                }

                Chat? existing = db.Chats.FirstOrDefault(c => !c.IsGroup
                    && c.ChatMembers.Any(m => m.ClientId == userId)
                    && c.ChatMembers.Any(m => m.ClientId == other.Id));
                if (existing != null)
                {
                    return existing.Id;
                }

                Chat chat = new Chat();
                chat.Name = "Private";
                chat.IsGroup = false;
                chat.ChatMembers.Add(new ChatMember { ClientId = userId });
                chat.ChatMembers.Add(new ChatMember { ClientId = other.Id });

                db.Chats.Add(chat);
                db.SaveChanges();
                return chat.Id;
            }
        }

        public static int CreateGroup(int userId, string name, string logins, out string error)
        {
            error = "";
            using (Messengers_Db db = new Messengers_Db())
            {
                Chat chat = new Chat();
                chat.Name = name;
                chat.IsGroup = true;
                chat.ChatMembers.Add(new ChatMember { ClientId = userId, IsChatAdmin = true });

                string notFound = "";
                foreach (string part in logins.Split(','))
                {
                    string login = part.Trim();
                    if (login == "")
                    {
                        continue;
                    }

                    Messenger_db.Client? member = db.Clients.FirstOrDefault(c => c.Login == login);
                    if (member == null)
                    {
                        notFound += login + " ";
                        continue;
                    }
                    if (chat.ChatMembers.Any(m => m.ClientId == member.Id))
                    {
                        continue;
                    }
                    chat.ChatMembers.Add(new ChatMember { ClientId = member.Id });
                }

                db.Chats.Add(chat);
                db.SaveChanges();

                if (notFound != "")
                {
                    error = "Не знайдено користувачів: " + notFound;
                }
                return chat.Id;
            }
        }

        public static List<MemberItem> GetMembers(int chatId)
        {
            List<MemberItem> result = new List<MemberItem>();
            using (Messengers_Db db = new Messengers_Db())
            {
                List<ChatMember> members = db.ChatMembers
                    .Include(m => m.Clients)
                    .Where(m => m.ChatId == chatId)
                    .ToList();

                foreach (ChatMember member in members)
                {
                    MemberItem item = new MemberItem();
                    item.Name = member.Clients.Name;
                    item.Login = member.Clients.Login;
                    item.Letter = GetLetter(member.Clients.Name);
                    item.Role = member.IsChatAdmin ? "адмін" : "";
                    result.Add(item);
                }
            }
            return result;
        }

        public static bool IsChatAdmin(int chatId, int userId)
        {
            using (Messengers_Db db = new Messengers_Db())
            {
                return db.ChatMembers.Any(m => m.ChatId == chatId && m.ClientId == userId && m.IsChatAdmin);
            }
        }

        public static string AddMember(int chatId, string login)
        {
            using (Messengers_Db db = new Messengers_Db())
            {
                Messenger_db.Client? user = db.Clients.FirstOrDefault(c => c.Login == login);
                if (user == null)
                {
                    return "Користувача з таким email не знайдено";
                }
                if (db.ChatMembers.Any(m => m.ChatId == chatId && m.ClientId == user.Id))
                {
                    return "Користувач вже є в чаті";
                }

                db.ChatMembers.Add(new ChatMember { ChatId = chatId, ClientId = user.Id });
                db.SaveChanges();
                return "";
            }
        }

        public static void LeaveChat(int chatId, int userId)
        {
            using (Messengers_Db db = new Messengers_Db())
            {
                ChatMember? member = db.ChatMembers.FirstOrDefault(m => m.ChatId == chatId && m.ClientId == userId);
                if (member != null)
                {
                    db.ChatMembers.Remove(member);
                    db.SaveChanges();
                }
            }
        }

        public static List<UserItem> SearchUsers(string myLogin, string query)
        {
            List<UserItem> result = new List<UserItem>();
            using (Messengers_Db db = new Messengers_Db())
            {
                User? me = db.Users.FirstOrDefault(u => u.Username == myLogin);
                UserRepository repository = new UserRepository(db);

                foreach (User user in repository.Search(query))
                {
                    if (user.Username == myLogin)
                    {
                        continue;
                    }

                    UserItem item = ToUserItem(user);
                    if (me != null)
                    {
                        item.IsContact = db.UserContacts.Any(c => c.UserId == me.Id && c.ContactUserId == user.Id);
                    }
                    result.Add(item);
                }
            }
            return result;
        }

        public static List<UserItem> GetContacts(string myLogin)
        {
            List<UserItem> result = new List<UserItem>();
            using (Messengers_Db db = new Messengers_Db())
            {
                User? me = db.Users.FirstOrDefault(u => u.Username == myLogin);
                if (me == null)
                {
                    return result;
                }

                UserRepository repository = new UserRepository(db);
                foreach (User user in repository.GetContacts(me.Id))
                {
                    UserItem item = ToUserItem(user);
                    item.IsContact = true;
                    result.Add(item);
                }
            }
            return result;
        }

        public static string AddContact(string myLogin, int contactUserId)
        {
            using (Messengers_Db db = new Messengers_Db())
            {
                User? me = db.Users.FirstOrDefault(u => u.Username == myLogin);

                if (me == null)
                    return "Користувача не знайдено.";

                if (me.Id == contactUserId)
                    return "Не можна додати самого себе.";

                User? contact = db.Users.FirstOrDefault(u => u.Id == contactUserId);

                if (contact == null)
                    return "Такого користувача не існує.";

                bool alreadyContact = db.UserContacts.Any(c =>
                    c.UserId == me.Id && c.ContactUserId == contactUserId);

                if (alreadyContact)
                    return "Цей користувач уже є у контактах.";

                ContactRequest? request = db.ContactRequests.FirstOrDefault(r => r.SenderUserId == me.Id && r.ReceiverUserId == contactUserId);

                if (request != null && request.Status == "Pending")
                    return "Заявка вже надіслана.";

                bool incomingRequest = db.ContactRequests.Any(r => r.SenderUserId == contactUserId && r.ReceiverUserId == me.Id && r.Status == "Pending");

                if (incomingRequest)
                    return "Цей користувач уже надіслав вам заявку. Перевірте розділ «Заявки».";

                if (request != null)
                {
                    request.Status = "Pending";
                    request.CreatedAt = DateTime.Now;
                }
                else
                {
                    db.ContactRequests.Add(new ContactRequest
                    {
                        SenderUserId = me.Id,
                        ReceiverUserId = contactUserId,
                        Status = "Pending",
                        CreatedAt = DateTime.Now
                    });
                }

                db.SaveChanges();

                return "Заявку надіслано.";
            }
        }

        public static void RemoveContact(string myLogin, int contactUserId)
        {
            using (Messengers_Db db = new Messengers_Db())
            {
                User? me = db.Users.FirstOrDefault(u => u.Username == myLogin);
                if (me != null)
                {
                    UserRepository repository = new UserRepository(db);
                    repository.RemoveContact(me.Id, contactUserId);
                }
            }
        }

        private static User EnsureUser(Messengers_Db db, Messenger_db.Client client)
        {
            User? user = db.Users.FirstOrDefault(u => u.Username == client.Login);
            if (user == null)
            {
                user = new User();
                user.Username = client.Login;
                user.DisplayName = client.Name;
                user.Email = client.Login;
                user.Password = client.Password;
                db.Users.Add(user);
                db.SaveChanges();
            }
            return user;
        }

        private static UserItem ToUserItem(User user)
        {
            UserItem item = new UserItem();
            item.Id = user.Id;
            item.Name = user.DisplayName;
            item.Login = user.Username;
            item.Letter = GetLetter(user.DisplayName);
            item.Status = GetStatus(user);
            return item;
        }

        private static string GetStatus(User? user)
        {
            if (user == null)
            {
                return "";
            }
            if (user.IsOnline)
            {
                return "в мережі";
            }
            if (user.LastSeenAt.Date == DateTime.Today)
            {
                return "був(ла) о " + user.LastSeenAt.ToString("HH:mm");
            }
            return "був(ла) " + user.LastSeenAt.ToString("dd.MM");
        }

        private static string GetLocalIp()
        {
            foreach (IPAddress address in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    return address.ToString();
                }
            }
            return "127.0.0.1";
        }

        private static string GetLetter(string name)
        {
            if (name == "")
            {
                return "?";
            }
            return name.Substring(0, 1).ToUpper();
        }

        private static string FormatTime(DateTime time)
        {
            if (time.Date == DateTime.Today)
            {
                return time.ToString("HH:mm");
            }
            return time.ToString("dd.MM");
        }
    }
}
