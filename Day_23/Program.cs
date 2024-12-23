var connections = new StringInputProvider("Input.txt").ToList();

var factory = new UniqueFactory<string, Computer>(w => new Computer(w));

foreach (var connection in connections)
{
    var names = connection.Split('-');

    if (names.Length != 2) throw new Exception();

    var computer1 = factory.GetOrCreateInstance(names[0]);
    var computer2 = factory.GetOrCreateInstance(names[1]);

    computer1.AddConnection(computer2);
    computer2.AddConnection(computer1);
}

List<HashSet<Computer>> cliques = new();

FindCliques([], factory.AllCreatedInstances.ToHashSet(), [], cliques);

var fullyConnectedTres = cliques.Where(w => w.Count == 3).ToList();

// need to handle those that are more than 3
var biggerCliques = cliques.Where(w => w.Count > 3).ToList();
foreach (var bc in biggerCliques)
{
    var combinations = bc.Combinations(3);
    fullyConnectedTres.AddRange(combinations.Select(w => w.ToHashSet()));
}

var graphOf3WithAtLeast1StartingWithT = fullyConnectedTres.Where(w => w.Any(ww => ww.Name.StartsWith("t"))).ToList();

// need to de-duplicate when going to 3, as a subset of 3 cliques can be in several max-size cliques that Bron–Kerbosch algorithm returns
// easiest to simply do with strings
var uniqueStringRepresentations = new HashSet<string>();

foreach (var c in graphOf3WithAtLeast1StartingWithT)
{
    var strRepresentation = string.Join("-", c.OrderBy(w => w.Name).Select(w => w.Name));
    uniqueStringRepresentations.Add(strRepresentation);
}

Console.WriteLine($"Part 1: {uniqueStringRepresentations.Count}");

var biggestClique = cliques.OrderByDescending(w => w.Count).First();

Console.WriteLine("Part 2: " + string.Join(",", biggestClique.OrderBy(w => w.Name).Select(w => w.Name)));

// Bron–Kerbosch algorithm, https://en.wikipedia.org/wiki/Bron%E2%80%93Kerbosch_algorithm
void FindCliques(HashSet<Computer> R, HashSet<Computer> P, HashSet<Computer> X, List<HashSet<Computer>> foundCliques)
{
    if (!P.Any() && !X.Any())
    {
        // report R as a maximal clique
        foundCliques.Add(R);
    }
    else
    {
        //choose a pivot vertex u in P ⋃ X, The pivot u should be chosen as one of the high-degree vertices, to minimize the number of recursive calls
        var pux = P.Union(X);
        var u = pux.OrderByDescending(w => w.ConnectedComputers.Count).First();

        var PminusN = P.Except(u.ConnectedComputers).ToHashSet();

        foreach (var v in PminusN)
        {
            var newR = R.ToHashSet();
            newR.Add(v);

            var newP = P.Intersect(v.ConnectedComputers).ToHashSet();

            var newX = X.Intersect(v.ConnectedComputers).ToHashSet();

            FindCliques(newR, newP, newX, foundCliques);

            P.Remove(v);
            X.Add(v);
        }
    }
}

[System.Diagnostics.DebuggerDisplay("{Name} - {ConnectedComputers.Count}")]
class Computer (string name)
{
    private readonly HashSet<Computer> connectedComputers = new();

    public IReadOnlyCollection<Computer> ConnectedComputers => this.connectedComputers.ToHashSet();

    public string Name => name;

    public void AddConnection(Computer computer)
    {
        connectedComputers.Add(computer);
    }
}