using System.Drawing;
using System.Text;

var wholeStringInput = new StringInputProvider("Input.txt");
var input = wholeStringInput.ToList();

Console.WriteLine(input.Count);

var world = new TileWorld(input, true, GetTile);

var printer = new WorldPrinter();
//printer.Print(world);

var strToFind = "XMAS";

List<List<Tile>> foundPaths = new();

foreach (Tile tile in world.WorldObjects)
{
    //Console.WriteLine(tile.Position);
    FindStringInGrid(tile, strToFind, new List<Tile>(), foundPaths);
}

Console.WriteLine($"Part 1: {foundPaths.Count}");

//foreach (var path in foundPaths)
//{
//    Console.WriteLine(GetStrRepresentation(path));
//}

strToFind = "MAS";

var foundCenter = new List<Tile>();

for (int y = 1; y < world.MaxY; y++)
{
    for (int x = 1; x < world.MaxX; x++)
    {
        var center = world.GetTileAt(x, y);

        if (center.CharRepresentation != 'A')
            continue;

        var edge1 = world.GetTileAt(x - 1, y - 1);
        var edge2 = world.GetTileAt(x + 1, y - 1);
        var edge3 = world.GetTileAt(x - 1, y + 1);
        var edge4 = world.GetTileAt(x + 1, y + 1);

        var str1 = $"{edge1.CharRepresentation}{center.CharRepresentation}{edge4.CharRepresentation}";
        var str2 = $"{edge4.CharRepresentation}{center.CharRepresentation}{edge1.CharRepresentation}";

        var str3 = $"{edge2.CharRepresentation}{center.CharRepresentation}{edge3.CharRepresentation}";
        var str4 = $"{edge3.CharRepresentation}{center.CharRepresentation}{edge2.CharRepresentation}";

        if ((str1 == strToFind || str2 == strToFind) &&
            (str3 == strToFind || str4 == strToFind))
        {
            foundCenter.Add(center);
        }
    }
}

Console.WriteLine($"Part 2: {foundCenter.Count}");

void FindStringInGrid(Tile currentTile, string str, List<Tile> path, List<List<Tile>> foundPaths)
{
    if (str.Length == 0)
    {
        if (IsAccepatblePath(path))
        {
            AddIfUnique(path, foundPaths);
        }
        return;
    }

    if (currentTile.CharRepresentation != str[0])
    {
        return;
    }

    if (path.Contains(currentTile))
    {
        return;
    }

    foreach (var n in currentTile.TraversibleNeighbours)
    {
        var substr = str[1..];
        var newPath = path.ToList();
        newPath.Add(currentTile);

        if (IsAccepatblePath(newPath))
        {
            FindStringInGrid(n, substr, newPath, foundPaths);
        }
    }
}

bool IsAccepatblePath(List<Tile> path)
{
    if (path.Select(w => w.Position.X).ToHashSet().Count == 1)
        return true;

    if (path.Select(w => w.Position.Y).ToHashSet().Count == 1)
        return true;

    //figure out if diagonal
    if (path.Count < 3)
        return true;

    var setDiffX = path[0].Position.X - path[1].Position.X;
    var setDiffY = path[0].Position.Y - path[1].Position.Y;

    for (int i = 2; i < path.Count; i++)
    {
        var diffX = path[i - 1].Position.X - path[i].Position.X;
        var diffY = path[i - 1].Position.Y - path[i].Position.Y;

        if (setDiffX != diffX || setDiffY != diffY)
            return false;
    }

    return true;
}

void AddIfUnique(List<Tile> path, List<List<Tile>> foundPaths)
{
    var newpath = GetStrRepresentation(path);
    bool unique = true;

    foreach (var oldPath in foundPaths)
    {
        if (newpath == GetStrRepresentation(oldPath))
        {
            unique = false;
            break;
        }
    }

    if (unique)
    {
        foundPaths.Add(path);
    }   
}

string GetStrRepresentation(List<Tile> path)
{
    var builder = new StringBuilder();
    foreach (var tile in path)
    {
        builder.Append(tile.Position + $"[{tile.CharRepresentation}]");
    }
    return builder.ToString();
}

Tile GetTile(int x, int y, char c, Func<Tile, IEnumerable<Tile>> func)
{
    return new CharTile(x, y, c, func);
}

class CharTile : Tile
{
    private readonly char c;
    public override char CharRepresentation => c;

    public CharTile(int x, int y, char c, Func<Tile, IEnumerable<Tile>> fillTraversibleNeighboursFunc)
        : base(x, y, true, fillTraversibleNeighboursFunc)
    {
        this.c = c;
    }
}