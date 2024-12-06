using System.Drawing;

var input = new StringInputProvider("Input.txt").ToList();

var world = new TileWorld(input, true, GetTile);

var printer = new WorldPrinter();
printer.Print(world);

var guard = GetGuardFromMap(input, world);

var initialGuardTile = guard.CurrentTile;
var initialGuardOrientation = guard.CurrentOrientation;

while (guard.Move()) ;

Console.WriteLine($"Part 1: {guard.UniqueVisitedTilesCount}");

var possibleObstacleLocations = guard.UniqueVisitedTiles.ToList();
possibleObstacleLocations.Remove(initialGuardTile);

var possibleObstacles = new List<Tile>();

foreach (var location in possibleObstacleLocations)
{
    var worldWithObstacle = new ReplaceableTileWorld(world, new Tile(location, false));
    guard = new Guard(initialGuardTile, initialGuardOrientation, worldWithObstacle);

    if (guard.DetectLoop())
    {
        possibleObstacles.Add(location);
    }
}

Console.WriteLine($"Part 2: {possibleObstacles.Count}");

Tile GetTile(int x, int y, char c, Func<Tile, IEnumerable<Tile>> func)
{
    return new Tile(x, y, c != '#', func);
}

Guard GetGuardFromMap(List<string> map, TileWorld world)
{
    for (int y = 0; y < map.Count; y++)
    {
        var row = map[y];
        for (int x = 0; x < map.Count; x++)
        {
            var cell = row[x];

            if (cell != '.' && cell != '#')
            {
                return new Guard(world.GetTileAt(x, y), 
                    cell switch
                    {
                        '^' => Orientation.Up,
                        '>' => Orientation.Right,
                        'v' => Orientation.Down,
                        '<' => Orientation.Left,
                        _ => throw new Exception("Unexpected format for guard")
                    }, 
                world);
            }
        }
    }

    throw new Exception("Guard not found, not expected");
}

enum Orientation { Up = 1, Right = 2, Down = 3, Left = 4 };

class Guard : IWorldObject
{
    public Tile CurrentTile { get; private set; }

    public Orientation CurrentOrientation { get; private set; }

    public Point Position => this.CurrentTile.Position;

    public char CharRepresentation => 'X';

    public int Z => 1;

    public IEnumerable<Tile> UniqueVisitedTiles => this.visitedStates.Select(w => w.Item1).ToHashSet();

    public int UniqueVisitedTilesCount => UniqueVisitedTiles.Count();

    private readonly HashSet<(Tile, Orientation)> visitedStates;
    private readonly TileWorld world;

    public Guard(Tile startTile, Orientation startOrientation, TileWorld world)
    {
        this.CurrentTile = startTile;
        this.CurrentOrientation = startOrientation;
        this.visitedStates = [(startTile, startOrientation)];
        this.world = world;
    }

    public bool Move()
    {
        var nextPosition = this.CurrentOrientation switch
        {
            Orientation.Up => this.CurrentTile.Position.Up(),
            Orientation.Right => this.CurrentTile.Position.Right(),
            Orientation.Down => this.CurrentTile.Position.Down(),
            Orientation.Left => this.CurrentTile.Position.Left(),
            _ => throw new Exception()
        };

        var nextTile = this.world.GetTileAtOrNull(nextPosition);

        if (nextTile == null)
        {
            return false;
        }

        if (!nextTile.IsTraversable)
        {
            this.CurrentOrientation = (Orientation)((int)CurrentOrientation + 1 > 4 ? 1 : (int)CurrentOrientation + 1);
            return Move();
        }
        else
        {
            this.CurrentTile = nextTile;
            this.visitedStates.Add((nextTile, this.CurrentOrientation));
            return true;
        }
    }

    public bool DetectLoop()
    {
        while (true)
        {
            var nextPosition = this.CurrentOrientation switch
            {
                Orientation.Up => this.CurrentTile.Position.Up(),
                Orientation.Right => this.CurrentTile.Position.Right(),
                Orientation.Down => this.CurrentTile.Position.Down(),
                Orientation.Left => this.CurrentTile.Position.Left(),
                _ => throw new Exception()
            };

            var nextTile = this.world.GetTileAtOrNull(nextPosition);

            if (nextTile == null)
            {
                return false;
            }

            if (!nextTile.IsTraversable)
            {
                this.CurrentOrientation = (Orientation)((int)CurrentOrientation + 1 > 4 ? 1 : (int)CurrentOrientation + 1);
            }
            else if (this.visitedStates.Contains((nextTile, CurrentOrientation)))
            {
                return true;
            }
            else
            {
                this.CurrentTile = nextTile;
                this.visitedStates.Add((nextTile, this.CurrentOrientation));
            }
        }
    }
}

class ReplaceableTileWorld : TileWorld
{
    public ReplaceableTileWorld(TileWorld originalWorld, Tile replacementTile) 
        : base(originalWorld)
    {
        this.allTiles.Remove(replacementTile.Position);
        this.allTiles.Add(replacementTile.Position, replacementTile);
    }
}