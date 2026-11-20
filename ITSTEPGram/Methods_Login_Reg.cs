using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Messenger_db;

namespace ITSTEPGram
{
    public class Methods_Login_Reg
    {
        public string? Login { get; set; }
        public string? Password { get; set; }
        public string? Name { get; set; }

        public string Registration(string login, string password, string name)
        {
            Login = login;
            Password = password;
            Name = name;

            using (Messengers_Db db = new Messengers_Db())
            {

                string patternEmail = @"^[A-Za-z0-9._-]{4,}@[A-Za-z0-9]{2,}\.[A-Za-z]{2,}$";

                var regex = new Regex(patternEmail);

                Match match = regex.Match(login);
                if (match.Success)
                {
                    if (db.Clients.Any(l => l.Login == Login))
                    {
                        return "Такий email вже зареєстрований";
                    }
                }
                else
                {
                    return $"Email {login} некоректний";
                }

                if (match.Success)
                {
                    Match match2 = regex.Match(password);

                    bool success = Regex.IsMatch(password, @".{6,}") && Regex.IsMatch(password, @"[A-Z]+") && Regex.IsMatch(password, @"[a-z]+") && Regex.IsMatch(password, @"[0-9]+") && Regex.IsMatch(password, @"[_-]+") && Regex.IsMatch(password, @"[A-Z]+") && Regex.IsMatch(password, @"^[A-Za-z0-9_-]+$");
                    if (success)
                    {
                        Messenger_db.Client client = new Messenger_db.Client()
                        {
                            Login = this.Login,
                            Password = this.Password,
                            Name = this.Name,
                            IP = "",
                        };

                        db.Clients.Add(client);
                        db.SaveChanges();
                    }
                    else
                    {
                        return "Пароль: від 6 символів, великі й малі латинські літери, цифра та _ або -";
                    }
                }
            }
            return "";
        }

        public bool LoginMember(string login, string password)
        {
            Login = login;
            Password = password;

            using(Messengers_Db db = new Messengers_Db())
            {
                if(db.Clients.Any(l => l.Login == this.Login))
                {
                    if(db.Clients.Any(l => l.Login == this.Login && l.Password == this.Password))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
