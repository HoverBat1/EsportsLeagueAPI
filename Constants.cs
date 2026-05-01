namespace EsportsLeagueApi01;

public static class League
{
    public static class Regions
    {
        public const string NA   = "NA";
        public const string EU   = "EU";
        public const string APAC = "APAC";
        public static readonly string[] All = [NA, EU, APAC];
        public static readonly int MaxLength = All.Max(a => a.Length);
    }

    public static class Roles
    {
        public const string Carry   = "Carry";
        public const string Support = "Support";
        public const string Tank    = "Tank";
        public const string Jungler = "Jungler";
        public const string Mid     = "Mid";
        public static readonly string[] All = [Carry, Support, Tank, Jungler, Mid];
        public static readonly int MaxLength = All.Max(a => a.Length);
    }

    public static class Team
    {
        public const int NameLengthMin = 2;
        public const int NameLengthMax = 50;
        public const int RosterMax = 5;
    }

    public static class Player
    {
        public const int UsernameLengthMin = 2;
        public const int UsernameLengthMax = 30;
        public const int SkillMin = 1;
        public const int SkillMax = 100;
    }

    public static class Modes
    {
        public const string Standard   = "Standard";
        public const string Tournament = "Tournament";
        public const string Practice   = "Practice";
        public static readonly string[] All = [Standard, Tournament, Practice];
        public static readonly int MaxLength = All.Max(a => a.Length);
    }

    public static class Events
    {
        public const string Started = "Started";
        public const string Goal    = "Goal";
        public const string Penalty = "Penalty";
        public const string Ended   = "Ended";
        public static readonly string[] All = [Started, Goal, Penalty, Ended];
        public static readonly int MaxLength = All.Max(a => a.Length);
    }

    public static class Status
    {
        public const string Scheduled = "Scheduled";
        public const string Live      = "Live";
        public const string Completed = "Completed";
        public static readonly string[] All = [Scheduled, Live, Completed];
        public static readonly int MaxLength = All.Max(a => a.Length);
    }

    public static class Auth
    {
        public const int UsernameLengthMin = 3;
        public const int UsernameLengthMax = 30;
        public const int PasswordLengthMin = 6;
        public static readonly string PasswordLengthMinErrorMessage = $"Password must be at least {PasswordLengthMin} characters";
    }
}