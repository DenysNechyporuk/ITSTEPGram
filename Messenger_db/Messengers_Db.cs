using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

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
                                            Integrated Security = True;"
            );
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Client>()
                .Property(x => x.Login)
                .IsRequired(true)
                .HasMaxLength(50);

            modelBuilder.Entity<Client>()
                .Property(a => a.Password)
                .IsRequired(true)
                .HasMaxLength(50);
            modelBuilder.Entity<Client>()
                .Property(a => a.Name)
                .IsRequired(true)
                .HasMaxLength(50);

            modelBuilder.Entity<Chat>()
                .Property(a => a.Name)
                .IsRequired(true)
                .HasMaxLength(50);

            modelBuilder.Entity<Message>()
                .Property(a => a.Text)
                .IsRequired(true)
                .HasMaxLength(50);

            // 1 .... *

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

        }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<ChatMember> ChatMembers { get; set; }
        public DbSet<Message> Messages { get; set; }

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
