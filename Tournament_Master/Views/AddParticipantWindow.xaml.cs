using System;
using System.Linq;
using System.Windows;
using Tournament_Master.Models;

namespace Tournament_Master.Views
{
    public partial class AddParticipantWindow : Window
    {
        public AddParticipantWindow()
        {
            InitializeComponent();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // 1. Очищуємо попередні помилки
            ErrorLabel.Visibility = Visibility.Collapsed;
            string firstName = TxtFirstName.Text.Trim();
            string lastName = TxtLastName.Text.Trim();

            // 2. Валідація
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                ShowError("Будь ласка, заповніть усі поля");
                return;
            }

            if (firstName.Length < 2 || lastName.Length < 2)
            {
                ShowError("Ім'я та прізвище мають бути довші за 1 символ");
                return;
            }

            // Перевірка, чи немає в імені цифр
            if (firstName.Any(char.IsDigit) || lastName.Any(char.IsDigit))
            {
                ShowError("Ім'я не може містити цифри");
                return;
            }

            // 3. Якщо все добре — додаємо
            try
            {
                DataStorage.AllParticipants.Add(new Participant
                {
                    FirstName = firstName,
                    LastName = lastName
                });

                DataStorage.SaveAll();
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                ShowError("Помилка збереження: " + ex.Message);
            }
        }

        // Допоміжний метод для виводу помилки
        private void ShowError(string message)
        {
            ErrorLabel.Text = message;
            ErrorLabel.Visibility = Visibility.Visible;
        }
    }
}