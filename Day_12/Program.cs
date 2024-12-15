var world = new TileWorld(new StringInputProvider("Input.txt"), false, GetTile);

var printer = new WorldPrinter();
printer.Print(world);

var toProcess = world.WorldObjects.OfType<PatchTile>().ToList();

var regions = new List<List<PatchTile>>();

long sumPricePerimeter = 0;
long sumPriceSides = 0;

while (toProcess.Count > 0)
{
    var region = new List<PatchTile>();
    regions.Add(region);
    GrabReachableTiles(toProcess[0], region, toProcess);
    char regionChar = region[0].Region;

    long perimiter = 0;

    foreach (var tile in region)
    {
        perimiter += 4 - tile.TraversibleNeighbours.Cast<PatchTile>().Count(w => w.Region == tile.Region);
    }

    sumPricePerimeter += region.Count * perimiter;

    var regionWorld = new SimpleWorld<PatchTile>(region);
    
    //printer.Print(regionWorld);

    long sides = 0;

    Dictionary<int, List<int>> verticalLinesInc = new();
    Dictionary<int, List<int>> verticalLinesDec = new();

    for (int y = regionWorld.MinY; y <= regionWorld.MaxY; y++)
    {
        for (int x = regionWorld.MinX; x <= regionWorld.MaxX + 1; x++)
        {
            PatchTile? prev = regionWorld.GetObjectAtOrNull(x - 1, y);
            PatchTile? current = regionWorld.GetObjectAtOrNull(x, y);

            if (prev?.Region != regionChar && current?.Region != regionChar)
                continue;

            if (current?.Region == regionChar && prev?.Region != regionChar)
            {
                if (!verticalLinesInc.ContainsKey(x))
                {
                    verticalLinesInc[x] = [y];
                    sides++;
                }
                else
                {
                    if (!verticalLinesInc[x].Contains(y - 1))
                    {
                        sides++;
                    }
                    verticalLinesInc[x].Add(y);
                }
            }
        }

        for (int x = regionWorld.MaxX; x >= regionWorld.MinX - 1; x--)
        {
            PatchTile? prev = regionWorld.GetObjectAtOrNull(x + 1, y);
            PatchTile? current = regionWorld.GetObjectAtOrNull(x, y);

            if (prev?.Region != regionChar && current?.Region != regionChar)
                continue;

            if (current?.Region == regionChar && prev?.Region != regionChar)
            {
                if (!verticalLinesDec.ContainsKey(x))
                {
                    verticalLinesDec[x] = [y];
                    sides++;
                }
                else
                {
                    if (!verticalLinesDec[x].Contains(y - 1))
                    {
                        sides++;
                    }
                    verticalLinesDec[x].Add(y);
                }
            }
        }
    }

    Dictionary<int, List<int>> horizontalLinesInc = new();
    Dictionary<int, List<int>> horizontalLinesDec = new();

    for (int x = regionWorld.MinX; x <= regionWorld.MaxX; x++)
    {
        for (int y = regionWorld.MinY; y <= regionWorld.MaxY + 1; y++)
        {
            PatchTile? prev = regionWorld.GetObjectAtOrNull(x, y - 1);
            PatchTile? current = regionWorld.GetObjectAtOrNull(x, y);

            if (prev?.Region != regionChar && current?.Region != regionChar)
                continue;

            if (current?.Region == regionChar && prev?.Region != regionChar)
            {
                if (!horizontalLinesInc.ContainsKey(y))
                {
                    horizontalLinesInc[y] = [x];
                    sides++;
                }
                else
                {
                    if (!horizontalLinesInc[y].Contains(x - 1))
                    {
                        sides++;
                    }
                    horizontalLinesInc[y].Add(x);
                }
            }
        }

        for (int y = regionWorld.MaxY; y >= regionWorld.MinY - 1; y--)
        {
            PatchTile? prev = regionWorld.GetObjectAtOrNull(x, y + 1);
            PatchTile? current = regionWorld.GetObjectAtOrNull(x, y);

            if (prev?.Region != regionChar && current?.Region != regionChar)
                continue;

            if (current?.Region == regionChar && prev?.Region != regionChar)
            {
                if (!horizontalLinesDec.ContainsKey(y))
                {
                    horizontalLinesDec[y] = [x];
                    sides++;
                }
                else
                {
                    if (!horizontalLinesDec[y].Contains(x - 1))
                    {
                        sides++;
                    }
                    horizontalLinesDec[y].Add(x);
                }
            }
        }
    }

    sumPriceSides += region.Count * sides;

    Console.WriteLine($"Size: {region.Count}, perimeter: {perimiter}, sides: {sides}");
    //Console.ReadKey();

}

Console.WriteLine($"Part 1: {sumPricePerimeter}");
Console.WriteLine($"Part 2: {sumPriceSides}");

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

[System.Diagnostics.DebuggerDisplay("{Region}({Position.X}, {Position.Y})")]
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