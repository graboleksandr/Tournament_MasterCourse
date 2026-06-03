using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using Tournament_Master.Models;

namespace Tournament_Master.Views
{
    public partial class LeaderboardPage : Page
    {
        public LeaderboardPage()
        {
            InitializeComponent();
            LoadLeaderboard();
        }

        private void LoadLeaderboard()
        {
            var tournament = DataStorage.ActiveTournament;
            if (tournament == null) return;

            List<IContestant> contestants = new List<IContestant>();

            if (tournament.Mode == TournamentMode.Team)
            {
                contestants = tournament.Teams.Cast<IContestant>().ToList();
            }
            else
            {
                contestants = tournament.Participants.Cast<IContestant>().ToList();
            }

            // Скидання статистики перед перерахунком
            foreach (var c in contestants)
            {
                c.MatchesPlayed = 0;
                c.Wins = 0;
                c.Draws = 0;
                c.Losses = 0;
                c.Points = 0;
                c.GoalsScored = 0;
                c.GoalsConceded = 0;
            }

            var matches = tournament.Matches ?? new System.Collections.ObjectModel.ObservableCollection<Match>();

            foreach (var match in matches)
            {
                if (!match.IsFinished) continue;

                var c1 = contestants.FirstOrDefault(c => c.DisplayName == match.Team1);
                var c2 = contestants.FirstOrDefault(c => c.DisplayName == match.Team2);

                if (c1 != null && c2 != null)
                {
                    c1.MatchesPlayed++;
                    c2.MatchesPlayed++;

                    // БЕЗПЕЧНО: Використовуємо оператор ?? на випадок, якщо Score містить null
                    int score1 = match.Score1 ?? 0;
                    int score2 = match.Score2 ?? 0;

                    c1.GoalsScored += score1;
                    c1.GoalsConceded += score2;
                    c2.GoalsScored += score2;
                    c2.GoalsConceded += score1;

                    if (score1 > score2)
                    {
                        c1.Wins++; c2.Losses++;
                        c1.Points += 3;
                    }
                    else if (score1 < score2)
                    {
                        c2.Wins++; c1.Losses++;
                        c2.Points += 3;
                    }
                    else
                    {
                        c1.Draws++; c2.Draws++;
                        c1.Points += 1;
                        c2.Points += 1;
                    }
                }
            }

            // Покращене сортування: Очки -> Різниця голів -> Забиті голи
            var displayList = contestants.Select(c => new PlayerStatsDisplay
            {
                Name = c.DisplayName,
                MatchesPlayed = c.MatchesPlayed,
                GoalsScored = c.GoalsScored,
                GoalsConceded = c.GoalsConceded,
                Points = c.Points
            }).OrderByDescending(p => p.Points)
              .ThenByDescending(p => p.GoalsScored - p.GoalsConceded)
              .ThenByDescending(p => p.GoalsScored)
              .ToList();

            // Підсвічування призових місць
            for (int i = 0; i < displayList.Count; i++)
            {
                displayList[i].Position = i + 1;

                if (i == 0) // 1 місце — Золотий колір
                {
                    displayList[i].PosBackground = new SolidColorBrush(Color.FromRgb(255, 177, 12));
                    displayList[i].PosForeground = Brushes.White;
                }
                else if (i < 3) // 2 та 3 місце — Темний акцент
                {
                    displayList[i].PosBackground = new SolidColorBrush(Color.FromRgb(45, 45, 45));
                    displayList[i].PosForeground = Brushes.White;
                }
                else // Всі інші місця
                {
                    displayList[i].PosBackground = Brushes.Transparent;
                    displayList[i].PosForeground = Brushes.Gray;
                }
            }

            LeaderboardList.ItemsSource = displayList;
        }
    }

    public class PlayerStatsDisplay
    {
        public int Position { get; set; }
        public string Name { get; set; }
        public int MatchesPlayed { get; set; }
        public int GoalsScored { get; set; }
        public int GoalsConceded { get; set; }
        public int Points { get; set; }
        public string GoalsDiff => $"{GoalsScored}:{GoalsConceded}";
        public Brush PosBackground { get; set; }
        public Brush PosForeground { get; set; }
    }
}