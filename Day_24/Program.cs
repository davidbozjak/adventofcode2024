var gateStrings = new StringInputProvider("Gates.txt").ToList();

var overridableGates = new Dictionary<string, GateString>();

foreach (var str in gateStrings)
{
    var parts = str.Split([" ", "->"], StringSplitOptions.RemoveEmptyEntries);

    if (parts.Length != 4)
        throw new Exception();

    var name = parts[3];

    overridableGates.Add(name, new GateString(name, parts[1], parts[0], parts[2]));
}

var registers = RunForConfiguration();

Console.WriteLine($"Part 1: {registers.z}");

List<string> wrongWires = new();

// the intial 6 wires found by XOR being mismatched
// XOR should always be used in X__ XOR Y__ --> z__ fashion (except for last z, z45)

foreach (var gateZ in overridableGates.Values.Where(w => w.Name != "z45").Where(w => w.Name.StartsWith("z")))
{
    if (gateZ.Operation != "XOR")
    {
        wrongWires.Add(gateZ.Name);
    }
}

foreach (var XORGate in overridableGates.Values.Where(w => w.Operation == "XOR"))
{
    if (!XORGate.Name.StartsWith("z"))
    {
        var firstChar = XORGate.ToString()[0];
        if (firstChar != 'x' && firstChar != 'y')
            wrongWires.Add(XORGate.Name);
    }
}

if (wrongWires.Count != 6)
    throw new Exception();

var initialSum = registers.x + registers.y;

var bitsThatMatch = registers.z ^ initialSum;

// The bit that doesn't match in the middle is caused by x and y wires to be swaped, index determines which ones

var indexOfXZWiresToBeSwaped = Convert.ToString(bitsThatMatch, 2).IndexOf('1', 5);

wrongWires.AddRange(overridableGates.Values.Where(w => w.IncludesInput($"x{indexOfXZWiresToBeSwaped}")).Select(w => w.OriginalName));

if (wrongWires.Count != 8)
    throw new Exception();

// find the right order of swaps to confirm that this is truly the solution:

foreach (var orderedWrongWires in wrongWires.GetAllOrdersOfList())
{
    try
    {
        SetSwaps(orderedWrongWires);

        registers = RunForConfiguration();

        var sum = registers.x + registers.y;

        if (registers.z == sum)
        {
            string[] outputToOrder = orderedWrongWires.OrderBy(w => w).ToArray();

            Console.WriteLine($"Part 2: {string.Join(",", outputToOrder.OrderBy(w => w))}");
            break;
        }
    }
    catch
    {
        // some swaps are not valid, ignore
    }
    finally
    {
        //reset all
        foreach (var gate in overridableGates.Values)
        {
            gate.Reset();
        }
    }
}

void SetSwaps(IList<string> swap)
{
    if (swap.Count != 8) throw new Exception();

    for (int i = 0; i < swap.Count; i+=2)
    {
        overridableGates[swap[i]].SetOverridedName(swap[i+1]);
        overridableGates[swap[i+1]].SetOverridedName(swap[i]);
    }
}


(long x, long y, long z) RunForConfiguration()
{
    var initialValueStrings = new StringInputProvider("InitialValues.txt").ToList();

    var wireFactory = new UniqueFactory<string, Wire>(w => new Wire(w));
    Wire.wireFactory = wireFactory;

    foreach (var str in initialValueStrings)
    {
        var parts = str.Split([' ', ':'], StringSplitOptions.RemoveEmptyEntries);

        var wire = wireFactory.GetOrCreateInstance(parts[0]);

        wire.SetFixedValue(parts[1] == "1");
    }

    foreach (var gate in overridableGates.Values)
    {
        var gateStr = gate.ToString();
        var parts = gateStr.Split([" ", "->"], StringSplitOptions.RemoveEmptyEntries);

        var outputWire = wireFactory.GetOrCreateInstance(parts[3]);
        outputWire.SetGate(parts[1], parts[0], parts[2]);
    }

    var initialX = GetValueOnWires("x");
    var initialY = GetValueOnWires("y");

    var initialZOutput = GetValueOnWires("z");

    return (initialX, initialY, initialZOutput);

    long GetValueOnWires(string wirePrefix)
    {
        long outpuValue = 0;
        int indexCount = 0;

        while (true)
        {
            var str = $"{wirePrefix}{indexCount.ToString().PadLeft(2, '0')}";

            if (!wireFactory.InstanceForIdentifierExists(str))
                break;

            var wire = wireFactory.GetInstanceOrThrow(str);

            outpuValue += wire.Value ? (long)Math.Pow(2, indexCount) : 0L;
            indexCount++;
        }

        return outpuValue;
    }
}
class Wire(string name)
{
    public static UniqueFactory<string, Wire> wireFactory;

    public string Name => name;

    private bool? fixedValue;

    private Cached<bool>? gateInput;

    public bool Value => this.fixedValue ?? gateInput?.Value ?? throw new Exception();

    public void SetFixedValue(bool input)
    {
        fixedValue = input;
    }

    public void SetGate(string gateType, string wire1Name, string wire2Name)
    {
        this.gateInput = new Cached<bool>(() =>
        {
            var wire1 = wireFactory.GetInstanceOrThrow(wire1Name);
            var wire2 = wireFactory.GetInstanceOrThrow(wire2Name);

            if (gateType == "OR")
            {
                return wire1.Value || wire2.Value;
            }
            else if (gateType == "AND")
            {
                return wire1.Value && wire2.Value;
            }
            else if (gateType == "XOR")
            {
                return wire1.Value != wire2.Value;
            }
            else throw new Exception();
        });
    }
}

class GateString(string name, string type, string input1, string input2)
{
    private string? overridedName;

    public string OriginalName => name;

    public string Name => overridedName ?? name;

    public string Operation => type;

    public void SetOverridedName(string overridedName)
    {
        this.overridedName = overridedName;
    }

    public bool IncludesInput(string str)
    {
        return input1 == str || input2 == str;
    }

    public void Reset()
    {
        this.overridedName = null;
    }

    public override string ToString()
    {
        return $"{input1} {type} {input2} -> {Name}";
    }
}