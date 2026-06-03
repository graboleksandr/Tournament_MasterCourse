using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Tournament_Master.Models;

namespace Tournament_Master.Views
{
    public partial class TeamsPage : Page
    {
        // Колекції для прив'язки до XAML
        public ObservableCollection<Player> AvailablePlayers { get; set; }
        public ObservableCollection<Team> TeamsList { get; set; }

        public TeamsPage()
        {
            InitializeComponent();

            AvailablePlayers = new ObservableCollection<Player>();
            TeamsList = new ObservableCollection<Team>();

            // Прив'язка джерел даних до елементів XAML
            ListPlayersSelector.ItemsSource = AvailablePlayers;
            TeamsDisplay.ItemsSource = TeamsList;

            LoadDefaultPlayers();
        }

        // Наповнення початкового списку гравців
        private void LoadDefaultPlayers()
        {
            var names = new List<string>
            {
                "Олександр Іванов", "Дмитро Петров", "Марія Сидоренко",
                "Анна Ковальчук", "Сергій Мороз", "Віталій Кравченко",
                "Олена Ткаченко", "Ігор Шевченко"
            };

            foreach (var name in names)
            {
                AvailablePlayers.Add(new Player { FullName = name, IsSelected = false });
            }
        }

        // Автоматична генерація випадкових команд
        private void BtnAutoGenerate_Click(object sender, RoutedEventArgs e)
        {
            TxtErrorAuto.Visibility = Visibility.Collapsed;

            if (!int.TryParse(TxtPlayersPerTeam.Text, out int playersPerTeam) || playersPerTeam <= 0)
            {
                TxtErrorAuto.Text = "Введіть коректну кількість гравців (число більше 0).";
                TxtErrorAuto.Visibility = Visibility.Visible;
                return;
            }

            if (AvailablePlayers.Count < playersPerTeam)
            {
                TxtErrorAuto.Text = "Недостатньо гравців для створення команди.";
                TxtErrorAuto.Visibility = Visibility.Visible;
                return;
            }

            Random random = new Random();
            List<Player> shuffledPlayers = AvailablePlayers.OrderBy(p => random.Next()).ToList();
            int teamCounter = TeamsList.Count + 1;

            while (shuffledPlayers.Count >= playersPerTeam)
            {
                var teamMembers = shuffledPlayers.Take(playersPerTeam).ToList();
                shuffledPlayers.RemoveRange(0, playersPerTeam);

                Team newTeam = new Team
                {
                    TeamName = $"Команда {teamCounter++}",
                    // Розділяємо рядок на Ім'я та Прізвище
                    Members = teamMembers.Select(p =>
                    {
                        var parts = p.FullName.Split(new[] { ' ' }, 2);
                        return new Participant
                        {
                            FirstName = parts[0],
                            LastName = parts.Length > 1 ? parts[1] : string.Empty
                        };
                    }).ToList()
                };

                TeamsList.Add(newTeam);
            }

            if (shuffledPlayers.Count > 0)
            {
                TxtErrorAuto.Text = $"Команди сформовано. Залишилось без команди: {shuffledPlayers.Count} гравців.";
                TxtErrorAuto.Visibility = Visibility.Visible;
            }
        }

        // Ручне створення команди
        private void BtnCreateManual_Click(object sender, RoutedEventArgs e)
        {
            TxtErrorManual.Visibility = Visibility.Collapsed;

            string teamName = TxtTeamName.Text?.Trim();
            if (string.IsNullOrEmpty(teamName))
            {
                TxtErrorManual.Text = "Будь ласка, введіть назву команди.";
                TxtErrorManual.Visibility = Visibility.Visible;
                return;
            }

            var selectedPlayers = AvailablePlayers.Where(p => p.IsSelected).ToList();
            if (!selectedPlayers.Any())
            {
                TxtErrorManual.Text = "Оберіть хоча б одного гравця зі списку.";
                TxtErrorManual.Visibility = Visibility.Visible;
                return;
            }

            Team newTeam = new Team
            {
                TeamName = teamName,
                // Розділяємо рядок на Ім'я та Прізвище
                Members = selectedPlayers.Select(p =>
                {
                    var parts = p.FullName.Split(new[] { ' ' }, 2);
                    return new Participant
                    {
                        FirstName = parts[0],
                        LastName = parts.Length > 1 ? parts[1] : string.Empty
                    };
                }).ToList()
            };

            TeamsList.Add(newTeam);

            // Очищення полів вводу та прапорців
            TxtTeamName.Clear();
            foreach (var player in AvailablePlayers)
            {
                player.IsSelected = false;
            }
        }

        // Видалення картки команди (кнопка "✕")
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Team clickedTeam)
            {
                TeamsList.Remove(clickedTeam);
            }
        }
    }

    // Клас Player, який ми сховали прямо сюди для 100% працездатності проекту
    public class Player : INotifyPropertyChanged
    {
        private bool _isSelected;
        private string _fullName;

        public string FullName
        {
            get => _fullName;
            set { _fullName = value; OnPropertyChanged(); }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}