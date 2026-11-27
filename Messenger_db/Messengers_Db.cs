using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace Messenger_db
{
    public class Messengers_Db : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer(@"
                Data Source = (localdb)\MSSQLLocalDB;
                Initial Catalog = Messenger_db;
                Integrated Security = True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Client>()
                .Property(x => x.Login)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<Client>()
                .Property(a => a.Password)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<Client>()
                .Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<Chat>()
                .Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<Message>()
                .Property(a => a.Text)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<Contact>()
                .HasOne(x => x.Client)
                .WithMany(m => m.Contacts)
                .HasForeignKey(x => x.ClientId);

            modelBuilder.Entity<ChatMember>()
                .HasOne(c => c.Clients)
                .WithMany(m => m.ChatMembers)
                .HasForeignKey(x => x.ClientId);

            modelBuilder.Entity<ChatMember>()
                .HasOne(c => c.Chats)
                .WithMany(m => m.ChatMembers)
                .HasForeignKey(x => x.ChatId);

            modelBuilder.Entity<Message>()
                .HasOne(c => c.Clients)
                .WithMany(m => m.Messages)
                .HasForeignKey(x => x.ClientId);

            modelBuilder.Entity<Message>()
                .HasOne(c => c.Chats)
                .WithMany(m => m.Messages)
                .HasForeignKey(x => x.ChatId);

            modelBuilder.Entity<User>()
                .Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<User>()
                .Property(u => u.DisplayName)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<User>()
                .Property(u => u.Password)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<User>()
                .Property(u => u.AvatarUrl)
                .HasMaxLength(500);

            modelBuilder.Entity<User>()
                .Property(u => u.Bio)
                .HasMaxLength(500);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<UserContact>()
                .HasOne(c => c.User)
                .WithMany(u => u.Contacts)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserContact>()
                .HasOne(c => c.ContactUser)
                .WithMany(u => u.AddedByContacts)
                .HasForeignKey(c => c.ContactUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserContact>()
                .HasIndex(c => new { c.UserId, c.ContactUserId })
                .IsUnique();

            modelBuilder.Entity<ContactRequest>()
                .HasOne(r => r.Sender)
                .WithMany(u => u.SentRequests)
                .HasForeignKey(r => r.SenderUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ContactRequest>()
                .HasOne(r => r.Receiver)
                .WithMany(u => u.ReceivedRequests)
                .HasForeignKey(r => r.ReceiverUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ContactRequest>()
                .HasIndex(r => new { r.SenderUserId, r.ReceiverUserId })
                .IsUnique();
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<ChatMember> ChatMembers { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserContact> UserContacts { get; set; }
        public DbSet<ContactRequest> ContactRequests { get; set; }
    }

    public class Client
    {
        public Client()
        {
            Contacts = new HashSet<Contact>();
            ChatMembers = new HashSet<ChatMember>();
            Messages = new HashSet<Message>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string IP { get; set; }
        public bool IsAdmin { get; set; }
        public ICollection<Contact> Contacts { get; set; }
        public ICollection<ChatMember> ChatMembers { get; set; }
        public ICollection<Message> Messages { get; set; }
    }

    public class Contact
    {
        public int Id { get; set; }
        public int? ClientId { get; set; }
        public Client Client { get; set; }
    }

    public class Chat
    {
        public Chat()
        {
            ChatMembers = new HashSet<ChatMember>();
            Messages = new HashSet<Message>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsGroup { get; set; }
        public DateTime CreatedTime { get; set; } = DateTime.Now;
        public ICollection<ChatMember> ChatMembers { get; set; }
        public ICollection<Message> Messages { get; set; }
    }

    public class ChatMember
    {
        public int Id { get; set; }
        public bool IsChatAdmin { get; set; }
        public int ChatId { get; set; }
        public int ClientId { get; set; }
        public Chat Chats { get; set; }
        public Client Clients { get; set; }
    }

    public class Message
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime SendTime { get; set; } = DateTime.Now;
        public int ClientId { get; set; }
        public int ChatId { get; set; }
        public Chat Chats { get; set; }
        public Client Clients { get; set; }
    }
}
