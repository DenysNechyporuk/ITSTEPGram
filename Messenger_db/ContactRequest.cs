using System;

namespace Messenger_db
{
    public class ContactRequest
    {
        public int Id { get; set; }
        public int SenderUserId { get; set; }
        public int ReceiverUserId { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public User Sender { get; set; } = null!;
        public User Receiver { get; set; } = null!;
    }
}
