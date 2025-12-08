using UnityEngine;

/// A static class to temporarily hold game data during the scene transition.
public static class MatchResultsStorage
{
    public static int MyScore;
    public static int MyFlexibility;
    public static int MyBurnout;
    public static int MyTokens;
    public static int OpponentScore;
    public static string GameOutcome; // "Victory", "Defeat", or "Draw"

    /// Saves the data based on which player "I" am (Host or Client).
    public static void SetData(bool amIHost, int hScore, int hFlex, int hBurn, int hTok, int cScore, int cFlex, int cBurn, int cTok)
    {
        if (amIHost)
        {
            MyScore = hScore;
            MyFlexibility = hFlex;
            MyBurnout = hBurn;
            MyTokens = hTok;
            OpponentScore = cScore;

            if (hScore >= 15 && cScore >= 15) GameOutcome = "Both Player's Won!";
            else if (hScore >= 15) GameOutcome = "You won!";
            else GameOutcome = "A failure is a step closer to victory!";
        }
        else
        {
            // I am the Client
            MyScore = cScore;
            MyFlexibility = cFlex;
            MyBurnout = cBurn;
            MyTokens = cTok;
            OpponentScore = hScore;

            if (hScore >= 15 && cScore >= 15) GameOutcome = "Both Player's Won!";
            else if (cScore >= 15) GameOutcome = "Victory!";
            else GameOutcome = "A failure is a step closer to victory!";
        }
    }
}