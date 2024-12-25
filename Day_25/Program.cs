var parser = new MultiLineParser<Instruction>(() => new Instruction(), (g, v) => g.Add(v));
var inputProvider = new InputProvider<Instruction?>("Input.txt", parser.AddLine)
{
    CheckBufferFirst = false,
    EndAtEmptyLine = false
};

var instructions = inputProvider.Where(w => w != null).ToList();

var keys = instructions.Where(w => w.IsKey).ToList();

var locks = instructions.Where(w => w.IsLock).ToList();

List<(Instruction, Instruction)> matches = new();

foreach (var key in keys)
{
    foreach (var l in locks)
    {
        bool isMatch = true;
        for (int i = 0; i < 5; i++)
        {
            int sum = key[i] + l[i];

            if (sum > 5)
            {
                isMatch = false; break;
            }
        }
        if (isMatch)
        {
            matches.Add((key, l));
        }
    }
}

Console.WriteLine($"Part 1: {matches.Count}");

class Instruction
{
    private readonly List<string> lines = new();

    private readonly Cached<bool> cachedIsKey;
    public bool IsKey => cachedIsKey.Value;

    private readonly Cached<bool> cachedIsLock;
    public bool IsLock => cachedIsLock.Value;

    private readonly Cached<int[]> cachedTumblerHeight;

    public int this[int index] => this.cachedTumblerHeight.Value[index];

    public Instruction()
    {
        this.cachedIsKey = new Cached<bool>(() => this.lines[6].All(w => w == '#'));
        this.cachedIsLock = new Cached<bool>(() => this.lines[0].All(w => w == '#'));
        this.cachedTumblerHeight = new Cached<int[]>(() =>
        {
            var heights = new int[this.lines[0].Length];
            
            for (int i = 0; i < heights.Length; i++)
            {
                if (this.IsKey)
                {
                    for (int j = this.lines.Count - 1; j > 1; j--)
                    {
                        heights[i] += this.lines[this.lines.Count - j][i] == '#' ? 1 : 0;
                    }
                }
                else if (this.IsLock)
                {
                    for (int j = 1; j < this.lines.Count; j++)
                    {
                        heights[i] += this.lines[j][i] == '#' ? 1 : 0;
                    }
                }
                else throw new Exception();
            }

            return heights;
        });
    }

    public void Add(string line)
    {
        this.lines.Add(line);
        this.cachedIsKey.Reset();
        this.cachedIsLock.Reset();
        this.cachedTumblerHeight.Reset();
    }

    public override string ToString()
    {
        return string.Join("", this.cachedTumblerHeight.Value);
    }
}