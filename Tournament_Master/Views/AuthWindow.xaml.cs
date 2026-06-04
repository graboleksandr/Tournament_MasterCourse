using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation; // Додано для підтримки анімацій
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
                DataStorage.CurrentUser = user.Login;
                DataStorage.LoadAll();

                MainWindow mainWindow = new MainWindow();
                Application.Current.MainWindow = mainWindow;
                mainWindow.Show();
                this.Close();
            }
            else
            {
                StatusLabel.Text = GetLocalizedText("m_ErrorWrongCredentials", "Невірний логін або пароль");
            }
        }

        // --- ЛОГІКА РЕЄСТРАЦІЇ ---
        private void BtnDoRegister_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(RegLogin.Text) || RegPassword.Password.Length < 4)
            {
                StatusLabelReg.Foreground = Brushes.Orange;
                StatusLabelReg.Text = GetLocalizedText("m_ErrorInvalidInput", "Перевірте логін та пароль (мін. 4 симв.)");
                return;
            }

            var users = LoadUsers();

            if (users.Any(u => u.Login.Equals(RegLogin.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                StatusLabelReg.Foreground = Brushes.Red;
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
                StatusLabelReg.Text = GetLocalizedText("m_RegSuccess", "Успіх! Тепер увійдіть");
            }
            catch (Exception ex)
            {
                StatusLabelReg.Foreground = Brushes.Red;
                StatusLabelReg.Text = "Помилка запису файлу!";
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        // --- НАВІГАЦІЯ З ПЛАВНОЮ АНІМАЦІЄЮ ---
        private void ShowRegister_Click(object sender, RoutedEventArgs e)
        {
            AnimatePanelTransition(LoginPanel, RegisterPanel);
        }

        private void ShowLogin_Click(object sender, RoutedEventArgs e)
        {
            AnimatePanelTransition(RegisterPanel, LoginPanel);
        }

        /// <summary>
        /// Плавно ховає одну панель за допомогою Opacity і проявляє іншу.
        /// </summary>
        private void AnimatePanelTransition(StackPanel toHide, StackPanel toShow)
        {
            var duration = TimeSpan.FromSeconds(0.25);

            // Анімація згасання для поточної панелі
            var fadeOut = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = new Duration(duration)
            };
            fadeOut.Completed += (s, e) => toHide.Visibility = Visibility.Hidden;

            // Анімація проявлення для нової панелі
            var fadeIn = new DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                Duration = new Duration(duration)
            };

            toShow.Visibility = Visibility.Visible;

            // Запуск анімації властивості Opacity
            toHide.BeginAnimation(UIElement.OpacityProperty, fadeOut);
            toShow.BeginAnimation(UIElement.OpacityProperty, fadeIn);
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