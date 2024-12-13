using System.Text.RegularExpressions;

var parser = new MultiLineParser<Instruction>(() => new Instruction(), (g, v) => g.Add(v));
using var inputProvider = new InputProvider<Instruction?>("Input.txt", parser.AddLine)
{
    CheckBufferFirst = false,
    EndAtEmptyLine = false
};

var instructions = inputProvider.Where(w => w != null).Cast<Instruction>().ToList();

long totalSpentP1 = 0;
long totalSpentP2 = 0;

foreach (var instruction in instructions)
{
    var game = instruction.Build();
    List<int> costs = [];

    int maxMoves = 200;

    for (int aMoves = 0; aMoves < 100; aMoves++)
    {
        costs.Add(RunGame(game, aMoves, maxMoves - aMoves));
    }

    totalSpentP1 += costs.Where(w => w > 0).OrderBy(w => w).FirstOrDefault();

    totalSpentP2 += CalcSolution(instruction);
}

Console.WriteLine($"Part 1: {totalSpentP1}");
Console.WriteLine($"Part 2: {totalSpentP2}");

long CalcSolution(Instruction instruction)
{
    long rx = 10000000000000 + instruction.RX;
    long ry = 10000000000000 + instruction.RY;

    long aMult = instruction.BY * instruction.AX - instruction.BX * instruction.AY;
    long right = rx * instruction.BY - ry * instruction.BX;

    var mod = right % aMult;

    if (mod != 0)
    {
        return 0;
    }

    long a = right / aMult;

    mod = (ry - a * instruction.AY) % instruction.BY;

    if (mod != 0)
    {
        return 0;
    }

    long b = (ry - a * instruction.AY) / instruction.BY;

    return a * 3 + b;
}

int RunGame(Game game, int maxAMoves, int maxBMoves)
{
    int cost = 0;

    for (int i = 0; i < maxAMoves; i++)
    {
        game = game.ButtonA();
        cost += 3;

        if (game.IsOnTarget)
        {
            return cost;
        }

        if (game.IsAnyOver)
        {
            return 0;
        }
    }

    for (int i = 0; i < maxBMoves; i++)
    {
        game = game.ButtonB();

        cost += 1;

        if (game.IsOnTarget)
        {
            return cost;
        }

        if (game.IsAnyOver)
        {
            return 0;
        }
    }

    return 0;
}

class Instruction
{
    int noOfLines = 0;

    private Func<(long, long), (long, long)>? button1Func;
    private Func<(long, long), (long, long)>? button2Func;
    private (long, long)? rewardLocation;

    private long? ax, ay, bx, by, rx, ry;

    public long AX => ax ?? throw new Exception();
    public long AY => ay ?? throw new Exception();
    public long BX => bx ?? throw new Exception();
    public long BY => by ?? throw new Exception();
    public long RX => rx ?? throw new Exception();
    public long RY => ry ?? throw new Exception();

    public void Add(string input)
    {
        Regex numRegex = new(@"-?\d+");
        var numbers = numRegex.Matches(input).Select(w => long.Parse(w.Value)).ToList();

        if (numbers.Count != 2) throw new Exception();

        if (noOfLines == 0)
        {
            button1Func = ((long x, long y) pos) => (pos.x + numbers[0], pos.y += numbers[1]);
            ax = numbers[0];
            ay = numbers[1];
        }
        else if (noOfLines == 1)
        {
            button2Func = ((long x, long y) pos) => (pos.x + numbers[0], pos.y += numbers[1]);
            bx = numbers[0];
            by = numbers[1];
        }
        else if (noOfLines == 2)
        {
            rewardLocation = (numbers[0], numbers[1]);
            rx = numbers[0];
            ry = numbers[1];
        }
        else throw new Exception();

        noOfLines++;
    }

    public Game Build()
    {
        if (button1Func == null) throw new Exception();
        if (button2Func == null) throw new Exception();
        if (rewardLocation == null) throw new Exception();

        return new Game((0, 0), button1Func, button2Func, rewardLocation.Value);
    }
}

class Game
{
    private readonly Func<(long, long), (long, long)> button1Func;
    private readonly Func<(long, long), (long, long)> button2Func;
    private readonly (long, long) rewardLocation;
    private readonly (long, long) position;

    public long X => position.Item1;
    public long Y => position.Item1;

    public Game((long, long) position, Func<(long, long), (long, long)> button1Func, Func<(long, long), (long, long)> button2Func, (long, long) rewardLocation)
    {
        this.position = position;
        this.button1Func = button1Func;
        this.button2Func = button2Func;
        this.rewardLocation = rewardLocation;
    }

    public Game ButtonA()
    {
        var newPos = button1Func(position);
        return new Game(newPos, button1Func, button2Func, rewardLocation);
    }

    public Game ButtonB()
    {
        var newPos = button2Func(position);
        return new Game(newPos, button1Func, button2Func, rewardLocation);
    }

    public bool IsOnTarget => position == rewardLocation;

    public bool IsAnyOver => position.Item1 > rewardLocation.Item1 || position.Item2 > rewardLocation.Item2;
}