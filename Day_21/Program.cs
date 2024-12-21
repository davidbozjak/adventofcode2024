var codes = new StringInputProvider("Input.txt").ToList();

long sum = 0;

Dictionary<string, HashSet<string>> memBaseInstructions = new();
Dictionary<string, string> memSubstrings = new();
Dictionary<string, long> memMoveCount = new();

foreach (var code in codes)
{
    var instructions = TransformCode(code, 2);

    var complexity = GetCodeComplexity(code, instructions);

    Console.WriteLine($"{DateTime.Now.ToShortTimeString()}: Code {code} Length: {instructions} Final Complexity: {complexity}");

    sum += complexity;
}

Console.WriteLine($"Part 1: {sum}");

foreach (var pair in memSubstrings.OrderBy(w => w.Key.Length))
{
    Console.WriteLine($"{pair.Key} -> {pair.Value}");
    if (pair.Value.Length != GetMovesCountForStr(pair.Key))
        throw new Exception();
}

long TransformCode(string code, int repeatsOfDirKeyboard)
{
    Console.WriteLine($"{DateTime.Now.ToShortTimeString()}: Starting new code: {code}");

    var paths = GetInstructionsForKeyboard_9(code).ToHashSet();

    long best = long.MaxValue;

    foreach (var path in paths)
    {
        long totalLength = 0;

        var parts = path.Split('A');
        parts = parts[..^1];

        var dict = parts.Select(w => w + 'A').GroupBy(w => w).ToDictionary(w => w.Key, w => (long)w.Count());

        for (int i = 0; i < repeatsOfDirKeyboard; i++)
        {
            var newDict = new Dictionary<string, long>();

            foreach (var part in dict.Keys)
            {
                var output = GetInstructionsForDirKeyboard(part);

                var outputParts = output.Split('A');
                outputParts = outputParts[..^1].Select(w => w + 'A').ToArray();

                foreach (var outputPart in outputParts)
                {
                    if (!newDict.ContainsKey(outputPart))
                    {
                        newDict[outputPart] = 0;
                    }
                    newDict[outputPart] += dict[part];
                }
            }

            dict = newDict;
        }

        totalLength = dict.Sum(w => w.Value * w.Key.Length);

        if (totalLength < best)
        {
            best = totalLength;
        }
    }

    return best;
}

long GetCodeComplexity(string code, long inputLength)
{
    return inputLength * int.Parse(code[..^1]);
}

IEnumerable<string> GetInstructionsForKeyboard_9(string desiredOutput)
{
    var startSate = new RobotArmKeyboard9State(Keyboard9Keys.KeyA, "", "");
    var endState = new RobotArmKeyboard9State(Keyboard9Keys.KeyA, "", desiredOutput);
    RobotArmKeyboard9State.EndStateString = desiredOutput;

    var paths = FindPaths(startSate, endState, RobotArmKeyboard9State.GetHeuristicScore, w => w.GetPossibleMoves());

    foreach (var path in paths)
    {
        var pathToHere = path.Last().PathToHere;

        var output = TypeInstructionKeyboard9(pathToHere);

        if (output != desiredOutput)
            throw new Exception();

        yield return pathToHere;
    }
}

long GetMovesCountForStr(string output)
{
    if (!memMoveCount.ContainsKey(output))
    {
        memMoveCount[output] = GetMovesCountForStrInternal(output);
    }
    return memMoveCount[output];

    long GetMovesCountForStrInternal(string output)
    {
        int minMoves = 0;
        for (int i = 0; i < output.Length; i++)
        {
            char prev = i > 0 ? output[i - 1] : 'A';
            if (output[i] == prev)
            {
                minMoves++;
            }
            else
            {
                if (output[i] == 'A')
                {
                    if ((new char[] { '^', '>' }).Contains(prev)) minMoves += 2;
                    else if (prev == 'v') minMoves += 3;
                    else minMoves += 4;
                }
                else if (output[i] == '^')
                {
                    if ((new char[] { 'A', 'v' }).Contains(prev)) minMoves += 2;
                    else minMoves += 3;
                }
                else if (output[i] == 'v')
                {
                    if ((new char[] { '^', '<', '>' }).Contains(prev)) minMoves += 2;
                    else minMoves += 3;
                }
                else if (output[i] == '>')
                {
                    if ((new char[] { 'A', 'v' }).Contains(prev)) minMoves += 2;
                    else minMoves += 3;
                }
                else if (output[i] == '<')
                {
                    if (prev == 'v') minMoves += 2;
                    else if ((new char[] { '^', '>' }).Contains(prev)) minMoves += 3;
                    else minMoves += 4;
                }
                else throw new Exception();
            }
        }
        return minMoves;
    }
}

string GetInstructionsForDirKeyboard(string desiredOutput)
{
    if (!memSubstrings.ContainsKey(desiredOutput))
    {
        memSubstrings[desiredOutput] = GetInstructionsForDirKeyboardInternal(desiredOutput);
    }
    return memSubstrings[desiredOutput];

    string GetInstructionsForDirKeyboardInternal(string desiredOutput)
    {
        var paths = FindPathsCachedDirKeyboard(desiredOutput);

        var tmp = paths.Select(w => new { Str = w, MovesNeeded = GetMovesCountForStr(w) }).OrderBy(w => w.MovesNeeded).ToList();

        return tmp.Select(w => w.Str).First();
    }
}

static string TypeInstructionDirKeyboard(string instruction)
{
    char c = 'A';

    var output = "";

    foreach (var i in instruction)
    {
        if (i == 'A') output += c;
        else if (i == '^')
        {
            if (c == 'A' || c == '^' || c == '<') throw new Exception();
            else if (c == 'v') c = '^';
            else if (c == '>') c = 'A';
            else throw new Exception();
        }
        else if (i == '<')
        {
            if (c == '^' || c == '<') throw new Exception();
            else if (c == 'A') c = '^';
            else if (c == '>') c = 'v';
            else if (c == 'v') c = '<';
            else throw new Exception();
        }
        else if (i == '>')
        {
            if (c == 'A' || c == '>') throw new Exception();
            else if (c == '^') c = 'A';
            else if (c == '<') c = 'v';
            else if (c == 'v') c = '>';
            else throw new Exception();
        }
        else if (i == 'v')
        {
            if (c == '<' || c == 'v' || c == '>') throw new Exception();
            else if (c == '^') c = 'v';
            else if (c == 'A') c = '>';
            else throw new Exception();
        }
        else throw new Exception();
    }

    return output;
}

static string TypeInstructionKeyboard9(string instruction)
{
    char c = 'A';

    var output = "";

    foreach (var i in instruction)
    {
        if (i == 'A') output += c;
        else if (i == '^')
        {
            if (c == '7' || c == '8' || c == '9') throw new Exception();
            else if (c == '4') c = '7';
            else if (c == '5') c = '8';
            else if (c == '6') c = '9';
            else if (c == '1') c = '4';
            else if (c == '2') c = '5';
            else if (c == '3') c = '6';
            else if (c == '0') c = '2';
            else if (c == 'A') c = '3';
            else throw new Exception();
        }
        else if (i == '<')
        {
            if (c == '7' || c == '4' || c == '1' || c == '0') throw new Exception();
            else if (c == '8') c = '7';
            else if (c == '5') c = '4';
            else if (c == '2') c = '1';
            else if (c == '9') c = '8';
            else if (c == '6') c = '5';
            else if (c == '3') c = '2';
            else if (c == 'A') c = '0';
            else throw new Exception();
        }
        else if (i == '>')
        {
            if (c == '9' || c == '6' || c == '3' || c == 'A') throw new Exception();
            else if (c == '8') c = '9';
            else if (c == '5') c = '6';
            else if (c == '2') c = '3';
            else if (c == '0') c = 'A';
            else if (c == '7') c = '8';
            else if (c == '4') c = '5';
            else if (c == '1') c = '2';
            else throw new Exception();
        }
        else if (i == 'v')
        {
            if (c == '1' || c == '0' || c == 'A') throw new Exception();
            else if (c == '7') c = '4';
            else if (c == '8') c = '5';
            else if (c == '9') c = '6';
            else if (c == '4') c = '1';
            else if (c == '5') c = '2';
            else if (c == '6') c = '3';
            else if (c == '2') c = '0';
            else if (c == '3') c = 'A';
        }
        else throw new Exception();
    }

    return output;
}

HashSet<string> FindPathsCachedDirKeyboard(string desiredOutput)
{
    if (!memBaseInstructions.ContainsKey(desiredOutput))
    {
        var startSate = new RobotArmKeyDirboardState(KeyboardDirKeys.KeyA, "", "");
        var endState = new RobotArmKeyDirboardState(KeyboardDirKeys.KeyA, "", desiredOutput);
        RobotArmKeyDirboardState.EndStateString = desiredOutput;

        var paths = FindPaths(startSate, endState, RobotArmKeyDirboardState.GetHeuristicScore, w => w.GetPossibleMoves())
            .Select(w => w.Last().PathToHere)
            .ToHashSet();

        memBaseInstructions[desiredOutput] = paths;
    }

    return memBaseInstructions[desiredOutput];
}

static IEnumerable<List<T>> FindPaths<T>(T start, T goal, Func<T, int?> GetHeuristicCost, Func<T, IEnumerable<T>> GetNeighbours)
        where T : class, ITotalCostNode, IEquatable<T>
{
    var openSet = new PriorityQueue<T, int>();
    openSet.Enqueue(start, 0);

    var cameFrom = new Dictionary<T, T>();
    var gScore = new Dictionary<T, int>
    {
        [start] = 0
    };

    var fScore = new Dictionary<INode, int>
    {
        [start] = GetHeuristicCost(start) ?? throw new Exception("Always expecting the start to be able to reach end")
    };

    int best = int.MaxValue;

    while (openSet.Count > 0)
    {
        var current = openSet.Dequeue();

        if (current.Equals(goal))
        {
            if (current.TotalCost <= best)
            {
                best = current.TotalCost;
            }
            else yield break;

            //reconstruct path!
            var path = new List<T>() { current };

            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Insert(0, current);
            }

            yield return path;
        }

        foreach (var neighbour in GetNeighbours(current))
        {
            var tentativeScore = gScore[current] + neighbour.Cost;

            var g = gScore.ContainsKey(neighbour) ? gScore[neighbour] : int.MaxValue;
            if (tentativeScore < g)
            {
                cameFrom[neighbour] = current;
                gScore[neighbour] = tentativeScore;

                var estimated = GetHeuristicCost(neighbour);

                if (estimated == null)
                    continue;

                var priority = tentativeScore + estimated.Value;

                fScore[neighbour] = priority;
                openSet.Enqueue(neighbour, priority); //Considering somehow handling duplicates?
            }
        }
    }
}

interface ITotalCostNode : INode
{
    public int TotalCost { get; }
}

//    +---+---+
//    | ^ | A |
//+---+---+---+
//| < | v | > |
//+---+---+---+
enum KeyboardDirKeys { KeyUp, KeyDown, KeyLeft, KeyRight, KeyA }

class RobotArmKeyDirboardState :
    ITotalCostNode, IEquatable<RobotArmKeyDirboardState>
{
    private static readonly Dictionary<int, int> heuristicMem = new();
    public static string EndStateString { get; set { heuristicMem.Clear(); field = value; } }

    public int Cost => 1;

    public int TotalCost => this.PathToHere.Length;

    public KeyboardDirKeys CurrentPosition { get; }

    public string PathToHere { get; }

    public string Output { get; }

    public RobotArmKeyDirboardState(KeyboardDirKeys currentPosition, string pathToHere, string output)
    {
        this.CurrentPosition = currentPosition;
        this.PathToHere = pathToHere;
        this.Output = output;
    }

    public static int? GetHeuristicScore(RobotArmKeyDirboardState state)
    {
        if (!EndStateString.StartsWith(state.Output))
            return null;

        return EndStateString.Length - state.Output.Length;

        //return 0;

        //if (state.PathToHere.Length > (state.Output.Length + 1) * 4)
        //    return null;
    }

    public IEnumerable<RobotArmKeyDirboardState> GetPossibleMoves()
    {
        switch (CurrentPosition)
        {
            case KeyboardDirKeys.KeyA:
                yield return new RobotArmKeyDirboardState(this.CurrentPosition, this.PathToHere + 'A', this.Output + 'A');
                yield return new RobotArmKeyDirboardState(KeyboardDirKeys.KeyRight, this.PathToHere + 'v', this.Output);
                yield return new RobotArmKeyDirboardState(KeyboardDirKeys.KeyUp, this.PathToHere + '<', this.Output);
                break;
            case KeyboardDirKeys.KeyUp:
                yield return new RobotArmKeyDirboardState(this.CurrentPosition, this.PathToHere + 'A', this.Output + '^');
                yield return new RobotArmKeyDirboardState(KeyboardDirKeys.KeyDown, this.PathToHere + 'v', this.Output);
                yield return new RobotArmKeyDirboardState(KeyboardDirKeys.KeyA, this.PathToHere + '>', this.Output);
                break;
            case KeyboardDirKeys.KeyLeft:
                yield return new RobotArmKeyDirboardState(this.CurrentPosition, this.PathToHere + 'A', this.Output + '<');
                yield return new RobotArmKeyDirboardState(KeyboardDirKeys.KeyDown, this.PathToHere + '>', this.Output);
                break;
            case KeyboardDirKeys.KeyDown:
                yield return new RobotArmKeyDirboardState(this.CurrentPosition, this.PathToHere + 'A', this.Output + 'v');
                yield return new RobotArmKeyDirboardState(KeyboardDirKeys.KeyUp, this.PathToHere + '^', this.Output);
                yield return new RobotArmKeyDirboardState(KeyboardDirKeys.KeyLeft, this.PathToHere + '<', this.Output);
                yield return new RobotArmKeyDirboardState(KeyboardDirKeys.KeyRight, this.PathToHere + '>', this.Output);
                break;
            case KeyboardDirKeys.KeyRight:
                yield return new RobotArmKeyDirboardState(this.CurrentPosition, this.PathToHere + 'A', this.Output + '>');
                yield return new RobotArmKeyDirboardState(KeyboardDirKeys.KeyA, this.PathToHere + '^', this.Output);
                yield return new RobotArmKeyDirboardState(KeyboardDirKeys.KeyDown, this.PathToHere + '<', this.Output);
                break;
            default: throw new Exception();
        }

        yield break;
    }

    public bool Equals(RobotArmKeyDirboardState? other)
    {
        if (other == null)
            return false;

        if (other.Output == EndStateString)
        {
            //for end state we only care abotu the output
            return this.Output == other.Output;
        }

        if (this.CurrentPosition != other.CurrentPosition || this.Output != other.Output || this.PathToHere != other.PathToHere)
            return false;

        return true;
    }

    public override string ToString()
    {
        return $"[{Output}]_[{CurrentPosition}]_[{PathToHere}]";
    }

    public override int GetHashCode()
    {
        return this.ToString().GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
            return false;

        if (obj is RobotArmKeyboard9State otherstate)
        {
            return this.Equals(otherstate);
        }
        return false;
    }
}

//    +---+---+---+
//    | 7 | 8 | 9 |
//    +---+---+---+
//    | 4 | 5 | 6 |
//    +---+---+---+
//    | 1 | 2 | 3 |
//    +---+---+---+
//        | 0 | A |
//        +---+---+
enum Keyboard9Keys { Key0, Key1, Key2, Key3, Key4, Key5, Key6, Key7, Key8, Key9, KeyA}

class RobotArmKeyboard9State : 
    ITotalCostNode, IEquatable<RobotArmKeyboard9State>
{
    public static string EndStateString;

    public int Cost => 1;

    public int TotalCost => this.PathToHere.Length;

    public Keyboard9Keys CurrentPosition { get; }

    public string PathToHere { get; }

    public string Output { get; }

    public RobotArmKeyboard9State(Keyboard9Keys currentPosition, string pathToHere, string output)
    {
        this.CurrentPosition = currentPosition;
        this.PathToHere = pathToHere;
        this.Output = output;
    }

    public static int? GetHeuristicScore(RobotArmKeyboard9State state)
    {
        if (!EndStateString.StartsWith(state.Output))
            return null;

        return EndStateString.Length - state.Output.Length;
    }

    public IEnumerable<RobotArmKeyboard9State> GetPossibleMoves()
    {
        switch (CurrentPosition)
        {
            case Keyboard9Keys.KeyA:
                yield return new RobotArmKeyboard9State(this.CurrentPosition, this.PathToHere + 'A', this.Output + 'A');
                yield return new RobotArmKeyboard9State(Keyboard9Keys.Key3, this.PathToHere + '^', this.Output);
                yield return new RobotArmKeyboard9State(Keyboard9Keys.Key0, this.PathToHere + '<', this.Output);
                break;
            case Keyboard9Keys.Key0:
                yield return new RobotArmKeyboard9State(this.CurrentPosition, this.PathToHere + 'A', this.Output + '0');
                yield return new RobotArmKeyboard9State(Keyboard9Keys.Key2, this.PathToHere + '^', this.Output);
                yield return new RobotArmKeyboard9State(Keyboard9Keys.KeyA, this.PathToHere + '>', this.Output);
                break;
            case Keyboard9Keys.Key1:
                yield return new RobotArmKeyboard9State(this.CurrentPosition, this.PathToHere + 'A', this.Output + '1');
                yield return new RobotArmKeyboard9State(Keyboard9Keys.Key4, this.PathToHere + '^', this.Output);
                yield return new RobotArmKeyboard9State(Keyboard9Keys.Key2, this.PathToHere + '>', this.Output);
                break;
            case Keyboard9Keys.Key2:
                yield return new RobotArmKeyboard9State(this.CurrentPosition, this.PathToHere + 'A', this.Output + '2');
                yield return new RobotArmKeyboard9State(Keyboard9Keys.Key5, this.PathToHere + '^', this.Output);
                yield return new RobotArmKeyboard9State(Keyboard9Keys.Key0, this.PathToHere + 'v', this.Output);
                yield return new RobotArmKeyboard9State(Keyboard9Keys.Key1, this.PathToHere + '<', this.Output);
                yield return new RobotArmKeyboard9State(Keyboard9Keys.Key3, this.PathToHere + '>', this.Output);
                break;
            case Keyboard9Keys.Key3:
                yield return new RobotArmKeyboard9State(this.CurrentPosition, this.PathToHere + 'A', this.Output + '3');
                yield return new RobotArmKeyboard9State(Keyboard9Keys.Key6, this.PathToHere + '^', this.Output);
                yield return new RobotArmKeyboard9State(Keyboard9Keys.KeyA, this.PathToHere + 'v', this.Output);
                yield return new RobotArmKeyboard9State(Keyboard9Keys.Key2, this.PathToHere + '<', this.Output);
                break;
            case Keyboard9Keys.Key4:
                yield return new RobotArmKeyboard9State(this.CurrentPosition, this.PathToHere + 'A', this.Output + '4');
                yield return new RobotArmKeyboard9State(Keyboard9Keys.Key7, this.PathToHere + '^', this.Output);
                yield return new RobotArmKeyboard9State(Keyboard9Keys.Key1, this.PathToHere + 'v', this.Output);
                yield return new RobotArmKeyboard9State(Keyboard9Keys.Key5, this.PathToHere + '>', this.Output);
                break;
            case Keyboard9Keys.Key5:
                yield return new RobotArmKeyboard9State(this.CurrentPosition, this.PathToHere + 'A', this.Output + '5');
                yield return new RobotArmKeyboard9State(Keyboard9Keys.Key8, this.PathToHere + '^', this.Output);
                yield return new RobotArmKeyboard9State(Keyboard9Keys.Key2, this.PathToHere + 'v', this.Output);
                yield return new RobotArmKeyboard9State(Keyboard9Keys.Key4, this.PathToHere + '<', this.Output);
                yield return new RobotArmKeyboard9State(Keyboard9Keys.Key6, this.PathToHere + '>', this.Output);
                break;
            case Keyboard9Keys.Key6:
                yield return new RobotArmKeyboard9State(this.CurrentPosition, this.PathToHere + 'A', this.Output + '6');
                yield return new RobotArmKeyboard9State(Keyboard9Keys.Key9, this.PathToHere + '^', this.Output);
                yield return new RobotArmKeyboard9State(Keyboard9Keys.Key3, this.PathToHere + 'v', this.Output);
                yield return new RobotArmKeyboard9State(Keyboard9Keys.Key5, this.PathToHere + '<', this.Output);
                break;
            case Keyboard9Keys.Key7:
                yield return new RobotArmKeyboard9State(this.CurrentPosition, this.PathToHere + 'A', this.Output + '7');
                yield return new RobotArmKeyboard9State(Keyboard9Keys.Key4, this.PathToHere + 'v', this.Output);
                yield return new RobotArmKeyboard9State(Keyboard9Keys.Key8, this.PathToHere + '>', this.Output);
                break;
            case Keyboard9Keys.Key8:
                yield return new RobotArmKeyboard9State(this.CurrentPosition, this.PathToHere + 'A', this.Output + '8');
                yield return new RobotArmKeyboard9State(Keyboard9Keys.Key5, this.PathToHere + 'v', this.Output);
                yield return new RobotArmKeyboard9State(Keyboard9Keys.Key7, this.PathToHere + '<', this.Output);
                yield return new RobotArmKeyboard9State(Keyboard9Keys.Key9, this.PathToHere + '>', this.Output);
                break;
            case Keyboard9Keys.Key9:
                yield return new RobotArmKeyboard9State(this.CurrentPosition, this.PathToHere + 'A', this.Output + '9');
                yield return new RobotArmKeyboard9State(Keyboard9Keys.Key6, this.PathToHere + 'v', this.Output);
                yield return new RobotArmKeyboard9State(Keyboard9Keys.Key8, this.PathToHere + '<', this.Output);
                break;
            default: throw new Exception();
        }

        yield break;
    }

    public bool Equals(RobotArmKeyboard9State? other)
    {
        if (other == null)
            return false;

        if (other.Output == EndStateString)
        {
            //for end state we only care abotu the output
            return this.Output == other.Output;
        }

        if (this.CurrentPosition != other.CurrentPosition || this.Output != other.Output || this.PathToHere != other.PathToHere)
            return false;

        return true;
    }

    public override string ToString()
    {
        return $"[{Output}]_[{CurrentPosition}]_[{PathToHere}]";
    }

    public override int GetHashCode()
    {
        return this.ToString().GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
            return false;

        if (obj is RobotArmKeyboard9State otherstate)
        {
            return this.Equals(otherstate);
        }
        return false;
    }
}