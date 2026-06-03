using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Tournament_Master.Models;

namespace Tournament_Master.Views
{
    public partial class SchedulePage : Page
    {
        private Tournament _tournament;

        public SchedulePage() : this(DataStorage.ActiveTournament)
        {
        }

        public SchedulePage(Tournament tournament)
        {
            InitializeComponent();

            _tournament = tournament ?? DataStorage.ActiveTournament;

            if (_tournament != null)
            {
                if (_tournament.Matches == null) _tournament.Matches = new ObservableCollection<Match>();
                if (_tournament.Participants == null) _tournament.Participants = new ObservableCollection<Participant>();
                if (_tournament.Teams == null) _tournament.Teams = new ObservableCollection<Team>();

                DataStorage.AllMatches = _tournament.Matches;
                DataStorage.AllParticipants = _tournament.Participants;
                DataStorage.AllTeams = _tournament.Teams;

                // Безпечний пошук панелі в XAML
                var PanelTeamSelection = FindName("PanelTeamSelection") as FrameworkElement;

                if (_tournament.Mode == TournamentMode.Team || (_tournament.TournamentType?.Contains("Командний") ?? false))
                {
                    _tournament.Mode = TournamentMode.Team;
                    DataStorage.IsTeamMode = true;

                    if (PanelTeamSelection != null)
                    {
                        PanelTeamSelection.Visibility = (DataStorage.AllTeams.Count == 0) ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
                else
                {
                    _tournament.Mode = TournamentMode.Single;
                    DataStorage.IsTeamMode = false;
                    if (PanelTeamSelection != null) PanelTeamSelection.Visibility = Visibility.Collapsed;
                }
            }

            RefreshMatchesList();
        }

        private void RefreshMatchesList()
        {
            if (MatchesList == null || DataStorage.AllMatches == null) return;

            string filter = TxtSearchMatch?.Text?.ToLower() ?? "";
            if (string.IsNullOrEmpty(filter))
            {
                MatchesList.ItemsSource = null;
                MatchesList.ItemsSource = DataStorage.AllMatches;
            }
            else
            {
                var filtered = DataStorage.AllMatches.Where(m =>
                    (m.Team1?.ToLower().Contains(filter) ?? false) ||
                    (m.Team2?.ToLower().Contains(filter) ?? false)
                ).ToList();
                MatchesList.ItemsSource = new ObservableCollection<Match>(filtered);
            }
        }

        private void BtnGenerateSchedule_Click(object sender, RoutedEventArgs e)
        {
            if (DataStorage.AllMatches == null) return;

            // Динамічний пошук елементів, щоб уникнути помилок компіляції
            var CmbTeamsCount = FindName("CmbTeamsCount") as ComboBox;
            var PanelTeamSelection = FindName("PanelTeamSelection") as FrameworkElement;
            var TxtStatusMessage = FindName("TxtStatusMessage") as TextBlock;

            DataStorage.AllMatches.Clear();
            List<string> targets = new List<string>();

            if (DataStorage.IsTeamMode)
            {
                if (DataStorage.AllTeams != null && DataStorage.AllTeams.Count >= 2)
                {
                    targets = DataStorage.AllTeams.Select(t => t.TeamName).ToList();
                }
                else
                {
                    if (DataStorage.AllParticipants == null || DataStorage.AllParticipants.Count < 2)
                    {
                        MessageBox.Show("Необхідно мати хоча б 2 учасників для створення команд!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (CmbTeamsCount == null || CmbTeamsCount.SelectedItem == null)
                    {
                        MessageBox.Show("Будь ласка, оберіть кількість команд для генерації!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (CmbTeamsCount.SelectedItem is ComboBoxItem selectedItem && int.TryParse(selectedItem.Content?.ToString(), out int chosenTeamsCount))
                    {
                        if (DataStorage.AllParticipants.Count < chosenTeamsCount)
                        {
                            MessageBox.Show($"Учасників менше ({DataStorage.AllParticipants.Count}), ніж обрана кількість команд ({chosenTeamsCount})!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        var shuffledPlayers = DataStorage.AllParticipants.OrderBy(p => Guid.NewGuid()).ToList();
                        DataStorage.AllTeams.Clear();

                        for (int i = 0; i < chosenTeamsCount; i++)
                        {
                            DataStorage.AllTeams.Add(new Team
                            {
                                TeamName = $"Команда {i + 1}",
                                Members = new List<Participant>()
                            });
                        }

                        for (int i = 0; i < shuffledPlayers.Count; i++)
                        {
                            DataStorage.AllTeams[i % chosenTeamsCount].Members.Add(shuffledPlayers[i]);
                        }

                        targets = DataStorage.AllTeams.Select(t => t.TeamName).ToList();

                        if (PanelTeamSelection != null) PanelTeamSelection.Visibility = Visibility.Collapsed;
                    }
                }
            }
            else
            {
                if (DataStorage.AllParticipants == null || DataStorage.AllParticipants.Count < 2)
                {
                    MessageBox.Show("Додайте хоча б 2 учасників для генерації розкладу!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                targets = DataStorage.AllParticipants.Select(p => p.FullName).ToList();
            }

            string type = _tournament?.TournamentType?.ToLower() ?? "";

            if (type.Contains("кругов") || type.Contains("круг"))
            {
                GenerateRoundRobin(targets);
            }
            else
            {
                GeneratePlayoffs(targets);
            }

            if (_tournament != null)
            {
                _tournament.Info = $"Матчів згенеровано: {DataStorage.AllMatches.Count}";
            }

            DataStorage.SaveAll();
            RefreshMatchesList();

            if (TxtStatusMessage != null)
            {
                TxtStatusMessage.Text = Application.Current.TryFindResource("ScheduleGenerated")?.ToString()
                                       ?? "Match schedule generated successfully!";

                TxtStatusMessage.Visibility = Visibility.Visible;
                var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
                timer.Tick += (s, args) => { TxtStatusMessage.Visibility = Visibility.Collapsed; timer.Stop(); };
                timer.Start();
            }
        }

        private void GenerateRoundRobin(List<string> list)
        {
            if (list == null || list.Count < 2) return;

            if (list.Count % 2 != 0)
            {
                list.Add("Вільний раунд");
            }

            int numTeams = list.Count;
            int numDays = numTeams - 1;
            int halfSize = numTeams / 2;

            List<string> teams = new List<string>(list);

            for (int day = 0; day < numDays; day++)
            {
                for (int i = 0; i < halfSize; i++)
                {
                    int first = i;
                    int second = numTeams - 1 - i;

                    if (teams[first] != "Вільний раунд" && teams[second] != "Вільний раунд")
                    {
                        DataStorage.AllMatches.Add(new Match
                        {
                            Round = (day + 1).ToString(),
                            Team1 = teams[first],
                            Team2 = teams[second],
                            Score1 = null,
                            Score2 = null,
                            IsFinished = false
                        });
                    }
                }

                string lastTeam = teams[numTeams - 1];
                teams.RemoveAt(numTeams - 1);
                teams.Insert(1, lastTeam);
            }
        }

        private void GeneratePlayoffs(List<string> list)
        {
            if (list == null || list.Count < 2) return;

            var shuffled = list.OrderBy(x => Guid.NewGuid()).ToList();

            for (int i = 0; i < shuffled.Count; i += 2)
            {
                if (i + 1 < shuffled.Count)
                {
                    DataStorage.AllMatches.Add(new Match
                    {
                        Round = "1",
                        Team1 = shuffled[i],
                        Team2 = shuffled[i + 1],
                        Score1 = null,
                        Score2 = null,
                        IsFinished = false
                    });
                }
                else
                {
                    DataStorage.AllMatches.Add(new Match
                    {
                        Round = "1",
                        Team1 = shuffled[i],
                        Team2 = "Прохід без гри (BYE)",
                        Score1 = 1,
                        Score2 = 0,
                        IsFinished = true
                    });
                }
            }
        }

        private void TxtSearchMatch_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Видалено неіснуючий RefreshUI()
            RefreshMatchesList();
        }

        private void BtnDeleteMatch_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Match match)
            {
                DataStorage.AllMatches.Remove(match);

                if (_tournament != null)
                {
                    _tournament.Info = $"Матчів: {DataStorage.AllMatches.Count}";
                }

                DataStorage.SaveAll();
                RefreshMatchesList();
            }
        }

        private void BtnSaveResults_Click(object sender, RoutedEventArgs e)
        {
            var TxtStatusMessage = FindName("TxtStatusMessage") as TextBlock;

            if (DataStorage.AllMatches != null)
            {
                foreach (var match in DataStorage.AllMatches)
                {
                    if (match.Score1.HasValue && match.Score2.HasValue)
                    {
                        match.IsFinished = true;
                    }
                    else
                    {
                        match.IsFinished = false;
                    }
                }
            }

            if (_tournament != null)
            {
                _tournament.Info = $"Матчів: {DataStorage.AllMatches.Count}";
            }

            DataStorage.SaveAll();

            if (TxtStatusMessage != null)
            {
                TxtStatusMessage.Text = Application.Current.TryFindResource("ResultsSaved")?.ToString()
                                       ?? "Results saved successfully!";

                TxtStatusMessage.Visibility = Visibility.Visible;
                var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
                timer.Tick += (s, args) => { TxtStatusMessage.Visibility = Visibility.Collapsed; timer.Stop(); };
                timer.Start();
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService?.CanGoBack == true)
                this.NavigationService.GoBack();
        }
    }
}