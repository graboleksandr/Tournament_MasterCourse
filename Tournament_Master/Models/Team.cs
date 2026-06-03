using System;
using System.Collections.Generic;
using System.Linq;

namespace Tournament_Master.Models
{
    public class Team : IContestant
    {
        // Основна інформація про команду
        public string TeamName { get; set; }

        // Список гравців, що входять до команди (Сценарій 2)
        public List<Participant> Members { get; set; } = new List<Participant>();

        // Властивість для відображення кількості гравців у інтерфейсі TeamsPage
        public string PlayersCountText => $"Гравців у команді: {Members?.Count ?? 0}";

        // --- Статистика команди (Реалізація IContestant для відображення у LeaderboardPage) ---
        public int MatchesPlayed { get; set; }
        public int Wins { get; set; }
        public int Draws { get; set; }
        public int Losses { get; set; }
        public int GoalsScored { get; set; }
        public int GoalsConceded { get; set; }
        public int Points { get; set; }

        // Властивості інтерфейсу для уніфікації
        public string DisplayName => TeamName;
        public string GoalsDiff => $"{GoalsScored}:{GoalsConceded}";
    }
}