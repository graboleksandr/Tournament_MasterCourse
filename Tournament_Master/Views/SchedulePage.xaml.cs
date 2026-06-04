using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Tournament_Master.Models;

namespace Tournament_Master.Views
{
    public partial class SchedulePage : Page
    {
        private Tournament _tournament;
        private DispatcherTimer _statusTimer;

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

                if (_tournament.Mode == TournamentMode.Team || (_tournament.TournamentType?.Contains("Командний") ?? false))
                {
                    _tournament.Mode = TournamentMode.Team;
                    DataStorage.IsTeamMode = true;

                    // Відображаємо панель вибору кількості команд, якщо вони ще не створені
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

        // МЕТОД ДЛЯ ВИВЕДЕННЯ ПОВІДОМЛЕНЬ ВПРОГРАМІ ЗНИЗУ
        private void ShowStatus(string message, bool isError = false)
        {
            if (TxtStatusMessage == null) return;

            // Зупиняємо попередній таймер, якщо він працював
            _statusTimer?.Stop();

            TxtStatusMessage.Text = message;

            // Якщо помилка — підсвічуємо червоним, якщо успіх — кольором акценту теми
            if (isError)
            {
                TxtStatusMessage.Foreground = new SolidColorBrush(Color.FromRgb(211, 47, 47)); // Червоний
            }
            else
            {
                TxtStatusMessage.Foreground = (Brush)FindResource("AccentColor");
            }

            TxtStatusMessage.Visibility = Visibility.Visible;

            // Сховуємо повідомлення автоматично через 3 секунди
            _statusTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            _statusTimer.Tick += (s, args) =>
            {
                TxtStatusMessage.Visibility = Visibility.Collapsed;
                _statusTimer.Stop();
            };
            _statusTimer.Start();
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
                        ShowStatus("Необхідно мати хоча б 2 учасників для створення команд!", true);
                        return;
                    }

                    if (CmbTeamsCount == null || CmbTeamsCount.SelectedItem == null)
                    {
                        ShowStatus("Будь ласка, оберіть кількість команд для генерації!", true);
                        return;
                    }

                    if (CmbTeamsCount.SelectedItem is ComboBoxItem selectedItem && int.TryParse(selectedItem.Content?.ToString(), out int chosenTeamsCount))
                    {
                        if (DataStorage.AllParticipants.Count < chosenTeamsCount)
                        {
                            ShowStatus($"Учасників менше ({DataStorage.AllParticipants.Count}), ніж обрана кількість команд ({chosenTeamsCount})!", true);
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

                        // Після успішної генерації ховаємо панель вибору
                        if (PanelTeamSelection != null) PanelTeamSelection.Visibility = Visibility.Collapsed;
                    }
                }
            }
            else
            {
                if (DataStorage.AllParticipants == null || DataStorage.AllParticipants.Count < 2)
                {
                    ShowStatus("Додайте хоча б 2 учасників для генерації розкладу!", true);
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

            string successMsg = Application.Current.TryFindResource("ScheduleGenerated")?.ToString() ?? "Розклад матчів успішно згенеровано!";
            ShowStatus(successMsg, false);
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
                ShowStatus("Матч видалено з розкладу", false);
            }
        }

        private void BtnSaveResults_Click(object sender, RoutedEventArgs e)
        {
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

            string saveMsg = Application.Current.TryFindResource("ResultsSaved")?.ToString() ?? "Результати успішно збережено!";
            ShowStatus(saveMsg, false);
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService?.CanGoBack == true)
                this.NavigationService.GoBack();
        }
    }
}