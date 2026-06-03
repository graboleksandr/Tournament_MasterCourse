using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Tournament_Master.Models;
using Tournament_Master;

namespace Tournament_Master.Views
{
    /// <summary>
    /// Вікно авторизації та реєстрації користувачів. 
    /// </summary>
    public partial class AuthWindow : Window
    {
        private readonly string UsersFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UsersData", "users.json");

        public AuthWindow()
        {
            InitializeComponent();

            string usersDir = Path.GetDirectoryName(UsersFilePath);
            if (!Directory.Exists(usersDir))
            {
                Directory.CreateDirectory(usersDir);
            }
        }

        private string GetLocalizedText(string resourceKey, string fallbackText)
        {
            return Application.Current.TryFindResource(resourceKey) as string ?? fallbackText;
        }

        private void Input_Changed(object sender, EventArgs e)
        {
            if (StatusLabel != null) StatusLabel.Text = "";
        }

        private void RegInput_Changed(object sender, EventArgs e)
        {
            if (StatusLabelReg != null) StatusLabelReg.Text = "";
        }

        // --- ЛОГІКА ВХОДУ ---
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            var users = LoadUsers();

            string login = LoginField.Text.Trim();
            string passwordHash = DataStorage.HashPassword(PasswordField.Password);

            var user = users.FirstOrDefault(u => u.Login.Equals(login, StringComparison.OrdinalIgnoreCase) && u.PasswordHash == passwordHash);

            if (user != null)
            {
                // ВИПРАВЛЕНО: Видалено помилкове посилання на DataStorage.ClearData()
                DataStorage.CurrentUser = user.Login;
                DataStorage.LoadAll();

                MainWindow mainWindow = new MainWindow();
                Application.Current.MainWindow = mainWindow;
                mainWindow.Show();
                this.Close();
            }
            else
            {
                // Змінено префікс на m_
                StatusLabel.Text = GetLocalizedText("m_ErrorWrongCredentials", "Невірний логін або пароль");
            }
        }

        // --- ЛОГІКА РЕЄСТРАЦІЇ ---
        private void BtnDoRegister_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(RegLogin.Text) || RegPassword.Password.Length < 4)
            {
                StatusLabelReg.Foreground = Brushes.Orange;
                // Змінено префікс на m_
                StatusLabelReg.Text = GetLocalizedText("m_ErrorInvalidInput", "Перевірте логін та пароль (мін. 4 симв.)");
                return;
            }

            var users = LoadUsers();

            if (users.Any(u => u.Login.Equals(RegLogin.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                StatusLabelReg.Foreground = Brushes.Red;
                // Змінено префікс на m_
                StatusLabelReg.Text = GetLocalizedText("m_ErrorLoginTaken", "Цей логін вже зайнятий");
                return;
            }

            users.Add(new UserData
            {
                Login = RegLogin.Text.Trim(),
                PasswordHash = DataStorage.HashPassword(RegPassword.Password),
                FirstName = RegFirstName.Text,
                LastName = RegLastName.Text,
                Gender = (RegGender.SelectedItem as ComboBoxItem)?.Content.ToString()
            });

            try
            {
                File.WriteAllText(UsersFilePath, JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true }));
                StatusLabelReg.Foreground = Brushes.Green;
                // Змінено префікс на m_
                StatusLabelReg.Text = GetLocalizedText("m_RegSuccess", "Успіх! Тепер увійдіть");
            }
            catch (Exception ex)
            {
                StatusLabelReg.Foreground = Brushes.Red;
                StatusLabelReg.Text = "Помилка запису файлу!";
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private void ShowRegister_Click(object sender, RoutedEventArgs e)
        {
            LoginPanel.Visibility = Visibility.Collapsed;
            RegisterPanel.Visibility = Visibility.Visible;
        }

        private void ShowLogin_Click(object sender, RoutedEventArgs e)
        {
            RegisterPanel.Visibility = Visibility.Collapsed;
            LoginPanel.Visibility = Visibility.Visible;
        }

        private List<UserData> LoadUsers()
        {
            if (!File.Exists(UsersFilePath)) return new List<UserData>();
            try { return JsonSerializer.Deserialize<List<UserData>>(File.ReadAllText(UsersFilePath)) ?? new List<UserData>(); }
            catch { return new List<UserData>(); }
        }

        private void BtnExitApp_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}