using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Tournament_Master.Models;

namespace Tournament_Master.Views
{
    /// <summary>
    /// Логіка взаємодії для ArchivePage.xaml
    /// </summary>
    public partial class ArchivePage : Page
    {
        public ArchivePage()
        {
            InitializeComponent();
            this.Loaded += ArchivePage_Loaded;
        }

        private void ArchivePage_Loaded(object sender, RoutedEventArgs e)
        {
            DataStorage.LoadAll();
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (TournamentsCards == null) return;
            TournamentsCards.ItemsSource = null;

            if (DataStorage.SavedTournaments == null) return;

            string prefix = Application.Current.TryFindResource("ArchiveMatches")?.ToString() ?? "Matches:";

            foreach (var tournament in DataStorage.SavedTournaments)
            {
                tournament.Info = $"{prefix} {tournament.Matches?.Count ?? 0}";
            }

            TournamentsCards.ItemsSource = DataStorage.SavedTournaments.ToList();
        }

        private void ItemBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is Tournament tournament)
            {
                if (tournament.IsEditing) return;

                // ВЛАШТОВАНЕ ВИПРАВЛЕННЯ:
                // Викликаємо централізований метод, який очищає і наповнює робочі колекції.
                // Це зберігає посилання на об'єкти в пам'яті, і Data Binding у WPF не ламається!
                DataStorage.LoadSelectedTournamentSession(tournament);

                // Додатковий захист: якщо в старих JSON файлах ще немає Enum-моду, 
                // підстраховуємося перевіркою текстового типу карток.
                DataStorage.IsTeamMode = (tournament.Mode == TournamentMode.Team ||
                                         (tournament.TournamentType?.Contains("Командний") ?? false));

                // Переходимо на сторінку розкладу
                var schedulePage = new SchedulePage(tournament);
                if (this.NavigationService != null)
                {
                    this.NavigationService.Navigate(schedulePage);
                }
            }
        }

        private void BtnEditToggle_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Tournament tournament)
            {
                if (tournament.IsEditing)
                {
                    tournament.IsEditing = false;
                    DataStorage.SaveAll();
                    RefreshUI();
                }
                else
                {
                    tournament.IsEditing = true;
                }
            }
            e.Handled = true;
        }

        private void TxtTitleInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.DataContext is Tournament tournament)
            {
                if (tournament.IsEditing)
                {
                    tournament.IsEditing = false;
                    DataStorage.SaveAll();
                    RefreshUI();
                }
            }
        }

        private void TxtTitleInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (sender is TextBox textBox && textBox.DataContext is Tournament tournament)
                {
                    tournament.IsEditing = false;
                    DataStorage.SaveAll();
                    Keyboard.ClearFocus();
                    RefreshUI();
                }
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Tournament tournament)
            {
                DataStorage.SavedTournaments.Remove(tournament);

                if (DataStorage.ActiveTournament == tournament)
                {
                    DataStorage.ActiveTournament = null;
                }

                DataStorage.SaveAll();
                RefreshUI();
            }
            e.Handled = true;
        }
    }
}