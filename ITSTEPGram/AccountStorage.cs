using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ITSTEPGram
{
    public static class AccountStorage
    {
        private static readonly string FolderPath =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ITSTEPGram");

        private static readonly string FilePath =
            Path.Combine(FolderPath, "accounts.txt");

        public static List<string> Load()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    return new List<string>();
                }

                return File.ReadAllLines(FilePath)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        public static void Save(string login)
        {
            if (string.IsNullOrWhiteSpace(login))
            {
                return;
            }

            try
            {
                Directory.CreateDirectory(FolderPath);

                List<string> accounts = Load();

                accounts.RemoveAll(x =>
                    string.Equals(x, login, StringComparison.OrdinalIgnoreCase));

                accounts.Insert(0, login);
                File.WriteAllLines(FilePath, accounts);
            }
            catch
            {
            }
        }
    }
}
