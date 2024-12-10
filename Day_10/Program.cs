var wholeStringInput = new StringInputProvider("Input.txt");
var input = wholeStringInput.ToList();

var world = new TileWorld(input, false, GetTile);

var trailheads = world.WorldObjects.Cast<HeightTile>().Where(w => w.Height == 0).ToList();

int sumPeakCount = 0;
int sumScores = 0;

foreach (var trailhead in trailheads)
{
    sumPeakCount += GetPeaks(trailhead).Count;
    sumScores += GetScore(trailhead, [trailhead]);
}

Console.WriteLine($"Part 1: {sumPeakCount}");
Console.WriteLine($"Part 2: {sumScores}");

HashSet<HeightTile> GetPeaks(HeightTile currentTile)
{
    if (currentTile.Height == 9)
        return [currentTile];

    var allPeaks = new HashSet<HeightTile>();

    foreach (HeightTile neighbour in currentTile.TraversibleNeighbours)
    {
        if (neighbour.Height == currentTile.Height + 1)
        {
            var peaks = GetPeaks(neighbour);

            foreach (var peak in peaks)
            {
                allPeaks.Add(peak);
            }
        }
    }

    return allPeaks;
}

int GetScore(HeightTile currentTile, List<HeightTile> path)
{
    if (currentTile.Height == 9)
        return 1;

    int sum = 0;

    foreach (HeightTile neighbour in currentTile.TraversibleNeighbours)
    {
        if (path.Contains(neighbour))
            continue;

        if (neighbour.Height == currentTile.Height + 1)
        {
            var newPath = path.ToList();
            newPath.Add(neighbour);

            sum += GetScore(neighbour, newPath);
        }
    }

    return sum;
}

Tile GetTile(int x, int y, char c, Func<Tile, IEnumerable<Tile>> func)
{
    return new HeightTile(x, y, c - '0', func);
}

class HeightTile : Tile
{
    public override char CharRepresentation => Height.ToString()[0];

    public int Height { get; }

    public HeightTile(int x, int y, int height, Func<Tile, IEnumerable<Tile>> fillTraversibleNeighboursFunc) 
        : base(x, y, true, fillTraversibleNeighboursFunc)
    {
        this.Height = height;
    }
}