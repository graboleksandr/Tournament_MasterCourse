using System;
using System.Windows;
using System.Windows.Controls;
using Tournament_Master.Models;

namespace Tournament_Master.Views
{
    public partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();

            // Динамічно підставляємо лише ім'я, щоб не затирати локалізований текст префіксу
            if (!string.IsNullOrEmpty(DataStorage.CurrentUser))
            {
                RunUser.Text = $" {DataStorage.CurrentUser}";
            }
        }

        /// <summary>
        /// Допоміжний метод для зменшення дублювання коду та оновлення інтерфейсу головного вікна
        /// </summary>
        private void CompleteTournamentCreation(Tournament tournament)
        {
            if (tournament == null) return;

            // Безпечно налаштовуємо сесію
            DataStorage.SetupNewTournamentSession(tournament);

            // Зберігаємо зміни на диск
            DataStorage.SaveAll();

            // Отримуємо посилання на MainWindow та оновлюємо сторінки
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                // Перестворюємо сторінки в MainWindow, щоб вони підтягнули дані нового турніру
                mainWindow.RefreshPages();

                try
                {
                    // Викликаємо оновлення меню за допомогою dynamic (безпечно)
                    ((dynamic)mainWindow).UpdateMenuAccess();
                }
                catch
                {
                    // Якщо методу немає, програма не впаде
                }
            }

            // Переходимо на сторінку учасників
            if (this.NavigationService != null)
            {
                var participantsPage = new ParticipantsPage(tournament);
                this.NavigationService.Navigate(participantsPage);
            }
        }

        private void BtnCreateRoundRobin_Click(object sender, RoutedEventArgs e)
        {
            Tournament newTournament = new Tournament
            {
                Title = "Новий Круговий Турнір",
                TournamentType = "Кругова",
                Mode = TournamentMode.Single,
                Icon = "Trophy",
                Date = DateTime.Now.ToString("dd.MM.yyyy HH:mm"),
                Info = "Матчів: 0",
                Creator = DataStorage.CurrentUser
            };

            CompleteTournamentCreation(newTournament);
        }

        private void BtnCreateElimination_Click(object sender, RoutedEventArgs e)
        {
            Tournament newTournament = new Tournament
            {
                Title = "Новий Турнір Плей-офф",
                TournamentType = "Олімпійська",
                Mode = TournamentMode.Single,
                Icon = "Award",
                Date = DateTime.Now.ToString("dd.MM.yyyy HH:mm"),
                Info = "Матчів: 0",
                Creator = DataStorage.CurrentUser
            };

            CompleteTournamentCreation(newTournament);
        }

        private void BtnCreateTeamRoundRobin_Click(object sender, RoutedEventArgs e)
        {
            Tournament newTournament = new Tournament
            {
                Title = "Новий Командний Круговий Турнір",
                TournamentType = "Командний круговий",
                Mode = TournamentMode.Team,
                Icon = "Trophy",
                Date = DateTime.Now.ToString("dd.MM.yyyy HH:mm"),
                Info = "Матчів: 0",
                Creator = DataStorage.CurrentUser
            };

            CompleteTournamentCreation(newTournament);
        }

        private void BtnCreateTeamElimination_Click(object sender, RoutedEventArgs e)
        {
            Tournament newTournament = new Tournament
            {
                Title = "Новий Командний Плей-офф",
                TournamentType = "Командний плей-офф",
                Mode = TournamentMode.Team,
                Icon = "Award",
                Date = DateTime.Now.ToString("dd.MM.yyyy HH:mm"),
                Info = "Матчів: 0",
                Creator = DataStorage.CurrentUser
            };

            CompleteTournamentCreation(newTournament);
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.IsLogoutAction = true;
            }

            // Очищаємо все ПРИ ВИХОДІ
            DataStorage.ClearData();

            var authWindow = new Tournament_Master.Views.AuthWindow();
            authWindow.Show();

            if (mainWindow != null)
            {
                mainWindow.Close();
            }
        }
    }
}