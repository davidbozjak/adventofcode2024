var wholeStringInput = new StringInputProvider("Input.txt");
var input = wholeStringInput.ToList();

var world = new TileWorld(input, true, GetTile);

var antennaGroups = world.WorldObjects.Where(w => w is AntennaTile).GroupBy(w => w.CharRepresentation).ToDictionary(w => w.Key, w => w.Cast<AntennaTile>().ToList());

Console.WriteLine($"Part 1: {GetAntinodes(2).Count}");
Console.WriteLine($"Part 2: {GetAntinodes(null).Count}");

HashSet<Tile> GetAntinodes(int? maxK)
{
    HashSet<Tile> antinodes = [];

    foreach (var group in antennaGroups.Keys)
    {
        var antennas = antennaGroups[group];

        for (int i = 0; i < antennas.Count; i++)
        {
            var first = antennas[i];
            for (int j = 0; j < antennas.Count; j++)
            {
                if (i == j) continue;

                var second = antennas[j];

                var diffX = first.Position.X - second.Position.X;
                var diffY = first.Position.Y - second.Position.Y;

                for (int k = maxK.HasValue ? 1 : 0; maxK.HasValue ? k < maxK.Value : true ; k++)
                {
                    var x1 = first.Position.X + k * diffX;
                    var y1 = first.Position.Y + k * diffY;

                    var tile = world.GetTileAtOrNull(x1, y1);
                    if (tile == null)
                    {
                        break;
                    }
                    else
                    {
                        antinodes.Add(tile);
                    }
                }
            }
        }
    }

    //var printer = new WorldPrinter();
    //var markedWorld = new WorldWithMarkings<Tile>(world, '#', antinodes);
    //printer.Print(markedWorld);

    return antinodes;
}

Tile GetTile(int x, int y, char c, Func<Tile, IEnumerable<Tile>> func)
{
    if (c == '.')
    {
        return new Tile(x, y, true, func);
    }
    else
    {
        return new AntennaTile(x, y, c, func);
    }
}

class AntennaTile : Tile
{
    private readonly char c;
    public override char CharRepresentation => c;

    public AntennaTile(int x, int y, char c, Func<Tile, IEnumerable<Tile>> fillTraversibleNeighboursFunc)
        : base(x, y, true, fillTraversibleNeighboursFunc)
    {
        this.c = c;
    }
}