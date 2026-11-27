using System;
using System.Collections.Generic;

namespace Messenger_db
{
    public class User
    {
        public User()
        {
            Contacts = new HashSet<UserContact>();
            AddedByContacts = new HashSet<UserContact>();
            SentRequests = new HashSet<ContactRequest>();
            ReceivedRequests = new HashSet<ContactRequest>();
        }

        public int Id { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastSeenAt { get; set; } = DateTime.Now;
        public bool IsOnline { get; set; }

        public ICollection<UserContact> Contacts { get; set; }
        public ICollection<UserContact> AddedByContacts { get; set; }

        public ICollection<ContactRequest> SentRequests { get; set; }
        public ICollection<ContactRequest> ReceivedRequests { get; set; }
    }
}
