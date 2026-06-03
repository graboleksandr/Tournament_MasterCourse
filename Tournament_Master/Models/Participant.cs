namespace Tournament_Master.Models
{
    public class Participant : IContestant
    {
        public string Id { get; set; } = System.Guid.NewGuid().ToString();
        public string FirstName { get; set; } // Вимога 5: Ім'я
        public string LastName { get; set; }  // Вимога 5: Прізвище
        public string AdditionalInfo { get; set; } // Вимога 5: Додаткові дані
        public string FullName => $"{FirstName} {LastName}".Trim();

        public bool IsSelected { get; set; }

        // --- Реалізація інтерфейсу IContestant ---
        public string DisplayName => FullName;
        public int MatchesPlayed { get; set; }
        public int Wins { get; set; }
        public int Draws { get; set; }
        public int Losses { get; set; }
        public int Points { get; set; }
        public int GoalsScored { get; set; }
        public int GoalsConceded { get; set; }
        public string GoalsDiff => $"{GoalsScored}:{GoalsConceded}";

        public Participant() { }
    }
}