using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace Master_passwords
{
    public partial class MainForm : Form
    {
        private List<PasswordEntry> entries = new List<PasswordEntry>();

        // Просто за пример, фиксиран ключ и IV (16 байта)
        private readonly byte[] key = Encoding.UTF8.GetBytes("1234567890123456");
        private readonly byte[] iv = Encoding.UTF8.GetBytes("6543210987654321");

        public MainForm()
        {
            InitializeComponent();

            btnSave.Click += BtnSave_Click;

            SetupListView();
        }

        private void SetupListView()
        {
            // Ако не си го добавил в Designer-а, направи го тук
            listView1.Columns.Clear();
            listView1.Columns.Add("Website", 150);
            listView1.Columns.Add("Username", 150);
            listView1.Columns.Add("Password", 150);
            listView1.View = View.Details;
            listView1.FullRowSelect = true;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            string website = txtWebsite.Text.Trim();
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(website) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Моля, попълнете всички полета.");
                return;
            }

            string encryptedPassword = EncryptString(password);

            entries.Add(new PasswordEntry()
            {
                Website = website,
                Username = username,
                EncryptedPassword = encryptedPassword
            });

            RefreshListView();

            // Изчистваме полетата за нов запис
            txtWebsite.Text = "";
            txtUsername.Text = "";
            txtPassword.Text = "";
        }

        private void RefreshListView()
        {
            listView1.Items.Clear();

            foreach (var entry in entries)
            {
                string decryptedPassword = DecryptString(entry.EncryptedPassword);

                ListViewItem item = new ListViewItem(entry.Website);
                item.SubItems.Add(entry.Username);
                item.SubItems.Add(decryptedPassword);

                listView1.Items.Add(item);
            }
        }

        private string EncryptString(string plainText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

                byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

                return Convert.ToBase64String(encryptedBytes);
            }
        }

        private string DecryptString(string cipherText)
        {
            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;

                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    byte[] cipherBytes = Convert.FromBase64String(cipherText);

                    byte[] decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

                    return Encoding.UTF8.GetString(decryptedBytes);
                }
            }
            catch
            {
                return "[Грешка при декриптиране]";
            }
        }
    }

    public class PasswordEntry
    {
        public string Website { get; set; }
        public string Username { get; set; }
        public string EncryptedPassword { get; set; }
    }
}

