using System.Drawing;

Robot robot = null!;

Console.WriteLine($"Part1: {SimulateRobotMoves(new StringInputProvider("Map.txt"))
    .WorldObjects.OfType<BoxTile>()
    .Sum(GetGpsScore)}");

Console.WriteLine($"Part2: {SimulateRobotMoves(new InputProvider<string?>("Map.txt", GetTransformedString).Cast<string>())
    .WorldObjects.OfType<HalfBoxTile>().Where(w => w.IsLeft)
    .Sum(GetGpsScore)}");

IWorld SimulateRobotMoves(IEnumerable<string> map)
{
    var world = new ShiftingWorld(map, false, GetTile);

    if (robot == null) throw new Exception();

    var instructions = new StringInputProvider("Movements.txt").SelectMany(w => w.ToCharArray());

    foreach (var instruction in instructions)
    {
        var nextTileFunc = GetNextTileFunc(instruction);

        bool canMove = true;

        HashSet<Point> posToMove = [robot.Position];
        HashSet<Point> newObstacles = new();

        while (canMove)
        {
            HashSet<Point> newPosToMove = new();

            foreach (var pos in posToMove)
            {
                var nextTile = world.GetTileAt(nextTileFunc(pos));

                if (nextTile is HalfBoxTile nextHalfTile)
                {
                    newObstacles.Add(nextTile.Position);
                    newObstacles.Add(nextHalfTile.CorrespondingTile.Position);
                    newPosToMove.Add(nextTile.Position);
                    newPosToMove.Add(nextHalfTile.CorrespondingTile.Position);
                    continue;
                }

                if (nextTile is BoxTile)
                {
                    newObstacles.Add(nextTile.Position);
                    newPosToMove.Add(nextTile.Position);
                    continue;
                }

                if (!nextTile.IsTraversable)
                {
                    canMove = false;
                    break;
                }
            }

            var newItemsToMove = newPosToMove.Any(w => !posToMove.Contains(w));

            if (!canMove || !newItemsToMove)
            {
                break;
            }
            else
            {
                posToMove = newPosToMove;
            }
        }

        if (canMove)
        {
            robot.MoveToPosition(nextTileFunc(robot.Position));
            world = new ShiftingWorld(world, newObstacles, nextTileFunc);
        }
    }

    return world;
}

static Func<Point, Point> GetNextTileFunc(char instruction)
{
    return instruction switch
    {
        '<' => (Point p) => p.Left(),
        '>' => (Point p) => p.Right(),
        '^' => (Point p) => p.Up(),
        'v' => (Point p) => p.Down(),
        _ => throw new Exception()
    };
}

Tile GetTile(int x, int y, char c, Func<Tile, IEnumerable<Tile>> func)
{
    if (c == '.') return new Tile(x, y, true, func);
    else if (c == '#') return new Tile(x, y, false, func);
    else if (c == 'O')
    {
        return new BoxTile(x, y, func);
    }
    else if (c == '@')
    {
        robot = new Robot(x, y);
        return new Tile(x, y, true, func);
    }
    else if (c == '[')
    {
        return new HalfBoxTile(x, y, true, func);
    }
    else if (c == ']')
    {
        return new HalfBoxTile(x, y, false, func);
    }
    else throw new Exception();
}

static bool GetTransformedString(string? input, out string? value)
{
    value = null;
    if (string.IsNullOrWhiteSpace(input)) return false;

    value = input;
    value = value.Replace("#", "##");
    value = value.Replace("O", "[]");
    value = value.Replace(".", "..");
    value = value.Replace("@", "@.");

    return true;
}

static long GetGpsScore(IWorldObject tile)
{
    return tile.Position.Y * 100 + tile.Position.X;
}

class BoxTile : Tile
{
    public override char CharRepresentation => 'O';

    public BoxTile(Tile origTile, Point newPos) : base(origTile, false, newPos)
    {
    }

    public BoxTile(int x, int y, Func<Tile, IEnumerable<Tile>> fillTraversibleNeighboursFunc) 
        : base(x, y, false, fillTraversibleNeighboursFunc)
    {
    }
}

class HalfBoxTile : BoxTile
{
    public bool IsLeft { get; }

    public override char CharRepresentation => IsLeft ? '[' : ']';

    public HalfBoxTile? CorrespondingTile { get; set; }

    public HalfBoxTile(int x, int y, bool isLeft, Func<Tile, IEnumerable<Tile>> fillTraversibleNeighboursFunc) 
        : base(x, y, fillTraversibleNeighboursFunc)
    {
        this.IsLeft = isLeft;
    }

    public HalfBoxTile(HalfBoxTile orig, Point newPos)
        : base(orig, newPos)
    {
        this.IsLeft = orig.IsLeft;
    }
}

class Robot : IWorldObject
{
    public Point Position { get; private set; }

    public char CharRepresentation => '@';

    public int Z => 2;

    public Robot(int initialX, int initialY)
    {
        this.Position = new Point(initialX, initialY);
    }

    public void MoveToPosition(Point newPosition)
    {
        this.Position = newPosition;
    }
}

class ShiftingWorld : TileWorld
{
    public ShiftingWorld(TileWorld orig, HashSet<Point> oldObstacles, Func<Point, Point> moveFunc) 
        : base(orig)
    {
        HashSet<Tile> newObstacles = [];

        var toProcess = oldObstacles.ToList();

        while (toProcess.Count > 0)
        {
            var pos = toProcess.First();
            toProcess.Remove(pos);

            var tile = this.GetTileAt(pos);

            if (tile is HalfBoxTile halvBoxTile)
            {
                toProcess.Remove(halvBoxTile.CorrespondingTile.Position);

                var newPos1 = moveFunc(halvBoxTile.Position);
                var newPos2 = moveFunc(halvBoxTile.CorrespondingTile.Position);

                var tile1 = newObstacles.FirstOrDefault(w => w.Position == newPos1) ?? new HalfBoxTile(halvBoxTile, newPos1);
                var tile2 = newObstacles.FirstOrDefault(w => w.Position == newPos2) ?? new HalfBoxTile(halvBoxTile.CorrespondingTile, newPos2);

                ((HalfBoxTile)tile1).CorrespondingTile = (HalfBoxTile)tile2;
                ((HalfBoxTile)tile2).CorrespondingTile = (HalfBoxTile)tile1;

                newObstacles.Add(tile1);
                newObstacles.Add(tile2);
            }
            else if (tile is BoxTile)
            {
                var newPos = moveFunc(pos);

                var newTile = new BoxTile(tile, newPos);
                newObstacles.Add(newTile);
            }
            else throw new Exception();
        }

        var newlyTraversible = oldObstacles.Where(w => !newObstacles.Any(ww => w == ww.Position)).ToList();

        foreach (var pos in newlyTraversible)
        {
            var tile = this.GetTileAt(pos);
            this.allTiles.Remove(pos);
            this.allTiles.Add(pos, new Tile(tile, true));
        }

        foreach (var tile in newObstacles)
        {
            this.allTiles.Remove(tile.Position);
            this.allTiles.Add(tile.Position, tile);
        }
    }

    public ShiftingWorld(IEnumerable<string> map, bool allowDiagnoalNeighbours, Func<int, int, char, Func<Tile, IEnumerable<Tile>>, Tile> tileCreatingFunc, Func<Tile, Tile, bool> isValidNeighbourFunc = null) 
        : base(map, allowDiagnoalNeighbours, tileCreatingFunc, isValidNeighbourFunc)
    {
        this.MapCorrespondingTiles();
    }

    private void MapCorrespondingTiles()
    {
        var unHandledWideBoxHalves = this.WorldObjects.OfType<HalfBoxTile>().ToList();

        while (unHandledWideBoxHalves.Count > 0)
        {
            var tile = unHandledWideBoxHalves.First(w => w.IsLeft);

            var corresponding = unHandledWideBoxHalves.First(w => w.Position.Y == tile.Position.Y && w.Position.X == tile.Position.X + 1);

            tile.CorrespondingTile = corresponding;
            corresponding.CorrespondingTile = tile;

            unHandledWideBoxHalves.Remove(tile);
            unHandledWideBoxHalves.Remove(corresponding);
        }
    }
}