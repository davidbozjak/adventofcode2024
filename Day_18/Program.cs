using System.Drawing;
using System.Text.RegularExpressions;

var intPairInput = new InputProvider<(int, int)?>("Input.txt", GetIntPair).Where(w => w != null).Cast<(int, int)>().ToList();

AgentState.obstacles = new UniqueFactory<(int, int), SimplePointWorldObject>(w => new SimplePointWorldObject(w.Item1, w.Item2, '#'));

var part1Limit = 1024;

int numRuns = 0;

Console.WriteLine($"Part 1: {Run(part1Limit).TotalCost}");

Console.WriteLine($"Part 2: {intPairInput[DevideAndConquerSearch(part1Limit, intPairInput.Count)]} - total runs {numRuns}");


int DevideAndConquerSearch(int min, int max)
{
    if (max - min <= 1)
        return min;

    int mid = (min + max) / 2;

    var result = Run(mid);

    if (result == null)
    {
        return DevideAndConquerSearch(min, mid);
    }
    else
    {
        return DevideAndConquerSearch(mid, max);
    }
}

AgentState? Run(int points)
{
    numRuns++;

    AgentState.obstacles = new UniqueFactory<(int, int), SimplePointWorldObject>(w => new SimplePointWorldObject(w.Item1, w.Item2, '#'));

    for (int i = 0; i < points; i++)
    {
        AgentState.obstacles.GetOrCreateInstance(intPairInput[i]);
    }

    var startState = new AgentState(new Point(0, 0), -1);
    var endState = new AgentState(new Point(70, 70), 0);

    AgentState.endState = endState.Position;

    var path = AStarPathfinder.FindPath(startState, endState, w => w.EstimateCostToN(w), w => w.GetPossiblePaths());

    return path?.Last();
}

static bool GetIntPair(string? input, out (int, int)? value)
{
    value = null;

    if (input == null) return false;

    Regex numRegex = new(@"-?\d+");

    var numbers = numRegex.Matches(input).Select(w => int.Parse(w.Value)).ToList();

    if (numbers.Count != 2) throw new Exception();

    value = (numbers[0], numbers[1]);

    return true;
}

class AgentState : INode, IWorldObject, IEquatable<AgentState>
{
    public static Point endState;

    public static UniqueFactory<(int, int), SimplePointWorldObject> obstacles;

    public int Cost => 1;

    public int TotalCost { get; }

    public Point Position { get; }

    public char CharRepresentation => 'O';

    public int Z => 1;

    public AgentState(Point position, int totalCostToHere)
    {
        this.Position = position;
        this.TotalCost = totalCostToHere + this.Cost;
    }

    public bool Equals(AgentState? other)
    {
        if (other == null) return false;

        return Point.Equals(this.Position, other.Position);
    }

    public int EstimateCostToN(AgentState target)
    {
        return target.Position.Distance(Position);
    }

    public IEnumerable<AgentState> GetPossiblePaths()
    {
        List<Point> possibilities = [this.Position.Up(), this.Position.Down(), this.Position.Left(), this.Position.Right()];

        foreach (var p in possibilities)
        {
            if (p.X < 0 || p.X > endState.X) continue;
            if (p.Y < 0 || p.Y > endState.Y) continue;

            if (obstacles.InstanceForIdentifierExists((p.X, p.Y)))
                continue;

            yield return new AgentState(p, this.TotalCost);
        }
    }

    public override int GetHashCode()
    {
        return (this.Position.GetHashCode() * 10000);
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
            return false;

        if (obj is AgentState otherstate)
        {
            return this.Equals(otherstate);
        }
        return false;
    }
}