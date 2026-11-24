using System;

/// Holds the random seed, current night and coins for the whole playthrough.
/// Call RunSession.StartNewRun() from your "New Game" button.
public static class RunSession
{
    static bool initialized;
    public static int Seed { get; private set; }
    public static int CurrentNight { get; set; } = 1;

    // ---- Coins ----
    public static int Coins { get; private set; }
    public static event Action<int> OnCoinsChanged;

    public static void AddCoins(int amount)
    {
        Coins += amount;
        OnCoinsChanged?.Invoke(Coins);
    }

    public static void SetCoins(int amount)
    {
        Coins = amount;
        OnCoinsChanged?.Invoke(Coins);
    }
    // ---------------

    public static void StartNewRun()
    {
        Seed = Environment.TickCount ^ Guid.NewGuid().GetHashCode();
        CurrentNight = 1;
        Coins = 0;
        initialized = true;
        OnCoinsChanged?.Invoke(Coins);
    }

    public static void EnsureInitialized()
    {
        if (!initialized) StartNewRun();
    }

    public static Random RngForNight(int night)
    {
        unchecked { return new Random(Seed * 397 ^ night * 7919); }
    }
}