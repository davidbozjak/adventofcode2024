using System.Drawing;

Tile startTile = null!;
Tile endTile = null!;

var inputMap = new StringInputProvider("Input.txt").ToList();

var world = new TileWorld(inputMap, false, GetTile);

if (startTile == null || endTile == null) throw new Exception();

var startState = new AgentState(startTile, 0, 0);
var endState = new AgentState(endTile, 0, 0);

AgentState.endState = endState.Position;

var path = AStarPathfinder.FindPath(startState, endState, w => w.EstimateCostToN(w), w => w.GetPossiblePaths());

var pathPoints = path.Select(w => w.Position).ToList();

int baselineCost = path.Last().TotalCost;

Console.WriteLine("Baseline: " + baselineCost);

Console.WriteLine($"Part 1: {GetShortcutsForMaxDistance(2).Values.Count(w => w >= 100)}");

Console.WriteLine($"Part 2: {GetShortcutsForMaxDistance(20).Values.Count(w => w >= 100)}");

Dictionary<(Point, Point), int> GetShortcutsForMaxDistance(int maxDistance, bool storyStylePrintout = false)
{
    Dictionary<(Point, Point), int> shortcuts = new();

    for (int i = 0; i < pathPoints.Count; i++)
    {
        var start = pathPoints[i];

        var remaining = pathPoints.Skip(i + 1).ToHashSet();

        var cheatableDestinations = remaining
            .Where(w => w.Distance(start) <= maxDistance)
            .ToHashSet();

        foreach (var end in cheatableDestinations)
        {
            int endIndex = pathPoints.IndexOfFirst(end);

            if (endIndex == -1)
                throw new Exception();

            var distanceSaved = endIndex - i - end.Distance(start);

            shortcuts[(start, end)] = distanceSaved;
        }
    }

    if (storyStylePrintout)
    {
        var groups = shortcuts.GroupBy(w => w.Value).Where(w => w.Key > 0).Select(w => (w.Key, w.ToList()));

        foreach (var group in groups.OrderBy(w => w.Key))
        {
            Console.WriteLine($"{group.Item2.Count} that saves {group.Key}");
        }
    }

    return shortcuts;
}

Tile GetTile(int x, int y, char c, Func<Tile, IEnumerable<Tile>> func)
{
    if (c == 'S')
    {
        startTile = new Tile(x, y, true, func);
        return startTile;
    }

    if (c == 'E')
    {
        endTile = new Tile(x, y, true, func);
        return endTile;
    }

    return new Tile(x, y, c != '#', func);
}

class AgentState : INode, IWorldObject, IEquatable<AgentState>
{
    public static Point endState;

    public int Cost { get; }

    public int TotalCost { get; }

    private readonly Tile currentTile;

    public Tile Tile => currentTile;

    public Point Position => currentTile.Position;

    public char CharRepresentation => 'X';

    public int Z => 1;

    public AgentState(Tile tile, int cost, int totalCostToHere)
    {
        this.currentTile = tile;
        this.Cost = cost;
        this.TotalCost = totalCostToHere + cost;
    }

    public int EstimateCostToN(AgentState target)
    {
        return target.Position.Distance(Position);
    }

    public IEnumerable<AgentState> GetPossiblePaths()
    {
        return this.currentTile.TraversibleNeighbours.Select(w => new AgentState(w, 1, this.TotalCost));
    }

    public bool Equals(AgentState? other)
    {
        if (other == null) return false;

        if (other.Position == endState)
        {
            //for end state we don't check anything but location
            return this.Position == other.Position;
        }

        return Point.Equals(this.Position, other.Position);
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
