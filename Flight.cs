namespace FlappyBird;

internal sealed class Flight
{
    internal const float Width = 800, Height = 450, Floor = 414, PlayerX = 136, Radius = 13;
    internal float Y { get; private set; }
    internal int Score { get; private set; }
    internal bool Running { get; private set; }
    internal bool Crashed { get; private set; }
    internal Gate[] Gates { get; } = [new(), new(), new()];
    private float velocity;

    internal Flight() => Reset();

    internal void Reset()
    {
        Y = 203;
        velocity = 0;
        Score = 0;
        Running = Crashed = false;
        for (int i = 0; i < Gates.Length; i++)
            Gates[i].Place(425 + i * 275);
    }

    internal void Flap()
    {
        if (Crashed) return;
        Running = true;
        velocity = -310;
    }

    internal void Step(float seconds)
    {
        if (!Running) return;
        velocity += 860 * seconds;
        Y += velocity * seconds;
        foreach (var gate in Gates) gate.X -= 170 * seconds;
        if (Y - Radius <= 0 || Y + Radius >= Floor ||
            Gates.Any(g => PlayerX + Radius > g.X && PlayerX - Radius < g.X + Gate.Width &&
                (Y - Radius < g.Center - Gate.HalfGap || Y + Radius > g.Center + Gate.HalfGap)))
        {
            Running = false;
            Crashed = true;
            return;
        }

        foreach (var gate in Gates)
        {
            if (!gate.Passed && gate.X + Gate.Width < PlayerX - Radius)
            {
                Score++;
                gate.Passed = true;
            }
            if (gate.X + Gate.Width < 0)
                gate.Place(Gates.Max(g => g.X) + 275);
        }
    }
}

internal sealed class Gate
{
    internal const float Width = 56, HalfGap = 82;
    internal float X { get; set; }
    internal float Center { get; private set; }
    internal bool Passed { get; set; }

    internal void Place(float x)
    {
        X = x;
        Center = Random.Shared.Next(135, 281);
        Passed = false;
    }
}
