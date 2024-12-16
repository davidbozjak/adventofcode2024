using System.Drawing;

Tile startTile = null!;
Tile endTile = null!;

var world = new TileWorld(new StringInputProvider("Input.txt"), false, GetTile);

if (startTile == null || endTile == null) throw new Exception();

var startState = new AgentState(startTile, Orientation.Left, 0, 0);
var endState = new AgentState(endTile, Orientation.Left, 0, 0);

AgentState.endState = endState.Position;

var path = AStarPathfinder.FindPath(startState, endState, w => w.EstimateCostToN(w), w => w.GetPossiblePaths());

var printer = new WorldPrinter();
printer.Print(world);

int minScore = path.Last().TotalCost;

Console.WriteLine($"Part 1: {minScore}");

var uniquePoints = GetAllPointsOnBestPath(startState, []);
uniquePoints.Add(endTile.Position);

var markedWorld = new WorldWithMarkings<Tile>(world, 'O', uniquePoints.Select(world.GetTileAt));
printer.Print(markedWorld);

Console.WriteLine($"Part 2: {uniquePoints.Count}");

HashSet<Point> GetAllPointsOnBestPath(AgentState current, Dictionary<AgentState, int> bestTotalCostPerPoint)
{
    if (!bestTotalCostPerPoint.ContainsKey(current))
    {
        bestTotalCostPerPoint[current] = current.TotalCost;
    }
    else if (current.TotalCost > bestTotalCostPerPoint[current])
    {
        return [];
    }

    bestTotalCostPerPoint[current] = current.TotalCost;

    if (current.TotalCost > minScore)
    {
        return [];
    }

    if (current.Equals(endState) && current.TotalCost < minScore)
    {
        throw new Exception();
    }

    if (current.Equals(endState) && current.TotalCost == minScore)
    {
        Console.WriteLine($"{DateTime.Now.ToShortTimeString()}: Found");

        return [current.Position];
    }

    var uniquePointsOnPaths = new HashSet<Point>();

    foreach (var next in current.GetPossiblePaths())
    {
        var result = GetAllPointsOnBestPath(next, bestTotalCostPerPoint);

        foreach (var r in result)
        {
            uniquePointsOnPaths.Add(r);
        }
    }

    if (uniquePointsOnPaths.Any())
    {
        uniquePointsOnPaths.Add(current.Position);
    }

    return uniquePointsOnPaths;
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

enum Orientation { Up = 1, Right = 2, Down = 3, Left = 4 };

class AgentState : INode, IWorldObject, IEquatable<AgentState>
{
    public static Point endState { get; set; }

    public Orientation CurrentOrientation { get; }

    public int Cost { get; }

    public int TotalCost { get; }

    private readonly Tile currentTile;

    public Point Position => currentTile.Position;

    public char CharRepresentation => 'X';

    public int Z => 1;

    public AgentState(Tile tile, Orientation orientation, int cost, int totalCostToHere)
    {
        this.currentTile = tile;
        this.CurrentOrientation = orientation;
        this.Cost = cost;
        this.TotalCost = totalCostToHere + cost;
    }

    public bool Equals(AgentState? other)
    {
        if (other == null) return false;

        if (other.Position == endState)
        {
            //for end tile we don't check orientation
            return this.Position == other.Position;
        }

        return Point.Equals(this.Position, other.Position) && this.CurrentOrientation == other.CurrentOrientation;
    }

    public int EstimateCostToN(AgentState target)
    {
        return target.Position.Distance(Position);
    }

    public IEnumerable<AgentState> GetPossiblePaths()
    {
        var nextPosition = this.CurrentOrientation switch
        {
            Orientation.Up => this.Position.Up(),
            Orientation.Right => this.Position.Right(),
            Orientation.Down => this.Position.Down(),
            Orientation.Left => this.Position.Left(),
            _ => throw new Exception()
        };

        var nextTile = this.currentTile.TraversibleNeighbours.FirstOrDefault(w => w.Position == nextPosition);

        if (nextTile != null)
        {
            yield return new AgentState(nextTile, CurrentOrientation, 1, this.TotalCost);
        }

        Orientation nextOrientation = CurrentOrientation;

        while (true)
        {
            nextOrientation = (Orientation)((int)nextOrientation + 1 > 4 ? 1 : (int)nextOrientation + 1);

            if (nextOrientation != this.CurrentOrientation)
            {
                yield return new AgentState(this.currentTile, nextOrientation, 1000, this.TotalCost);
            }
            else break;
        }
    }

    public override int GetHashCode()
    {
        return (this.Position.GetHashCode() * 10000) +  this.CurrentOrientation.GetHashCode();
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