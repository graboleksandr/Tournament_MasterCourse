namespace Tournament_Master.Models
{
    public interface IContestant
    {
        string DisplayName { get; }
        int MatchesPlayed { get; set; }
        int Wins { get; set; }
        int Draws { get; set; }
        int Losses { get; set; }
        int Points { get; set; }
        int GoalsScored { get; set; }
        int GoalsConceded { get; set; }
        string GoalsDiff { get; }
    }
}