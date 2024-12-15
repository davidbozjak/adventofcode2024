var world = new TileWorld(new StringInputProvider("Input.txt"), false, GetTile);

var printer = new WorldPrinter();
printer.Print(world);

var toProcess = world.WorldObjects.OfType<PatchTile>().ToList();

var regions = new List<List<PatchTile>>();

long sumScore = 0;

while (toProcess.Count > 0)
{
    var region = new List<PatchTile>();
    regions.Add(region);
    GrabReachableTiles(toProcess[0], region, toProcess);

    long perimiter = 0;

    foreach (var tile in region)
    {
        perimiter += 4 - tile.TraversibleNeighbours.Cast<PatchTile>().Count(w => w.Region == tile.Region);
    }

    sumScore += region.Count * perimiter;
}

Console.WriteLine($"Part 1: {sumScore}");

void GrabReachableTiles(PatchTile current, List<PatchTile> region, List<PatchTile> remaining)
{
    if (region.Contains(current))
        return;

    region.Add(current);
    remaining.Remove(current);

    foreach (PatchTile n in current.TraversibleNeighbours)
    {
        if (!remaining.Contains(n)) continue;

        if (current.Region != n.Region)
            continue;

        GrabReachableTiles(n, region, remaining);
    }
}

Tile GetTile(int x, int y, char c, Func<Tile, IEnumerable<Tile>> func)
{
    return new PatchTile(x, y, c, func);
}

class PatchTile : Tile
{
    public override char CharRepresentation => Region;

    public char Region { get; }

    public PatchTile(int x, int y, char region, Func<Tile, IEnumerable<Tile>> fillTraversibleNeighboursFunc)
        : base(x, y, true, fillTraversibleNeighboursFunc)
    {
        this.Region = region;
    }
}