using System.Text;

// sample input for part 1:

//string instructionInput = "0,1,5,4,3,0";

//long a = 729;
//long b = 0;
//long c = 0;

// sample input for part 2, replace with real input for your puzzle

string instructionInput = "0,3,5,4,3,0";

long a = 2024;
long b = 0;
long c = 0;

var result = RunProgram();

int index = instructionInput.Length - 1;

Console.WriteLine($"Part 1: {result}");

long verifiedSoFar = 0;

while (result != instructionInput)
{
    verifiedSoFar = verifiedSoFar * 8;
    var current = instructionInput[index..];
    
    for (var startA = verifiedSoFar; ; startA++)
    {
        a = startA;
        b = 0;
        c = 0;

        result = RunProgram();

        if (result == current)
        {
            verifiedSoFar = startA;
            index -= 2;
            break;
        }
    }
}

a = verifiedSoFar;
b = 0;
c = 0;

var part2 = RunProgram();

if (part2 != instructionInput)
    throw new Exception();

Console.WriteLine($"Part 2: {verifiedSoFar}");

string RunProgram()
{
    var instructions = instructionInput.Split(',').Select(int.Parse).ToList();

    var output = new StringBuilder();

    for (int index = 0; index < instructions.Count; index += 2)
    {
        var instruction = instructions[index];
        var operand = instructions[index + 1];

        switch (instruction)
        {
            case 0: PerformADV(operand); break;
            case 1: PerformBXL(operand); break;
            case 2: PerformBST(operand); break;
            case 3: PerformJump(operand, ref index); break;
            case 4: PerformBXC(operand); break;
            case 5: PerformOut(operand, output); break;
            case 6: PerformBDV(operand); break;
            case 7: PerformCDV(operand); break;
            default: throw new Exception();
        };
    }

    return output.ToString();
}

void PerformADV(long operand)
{
    var num = a;
    var denominator = Math.Pow(2, GetComboOperand(operand));

    a = (long)(num / denominator);
}

void PerformBDV(long operand)
{
    var num = a;
    var denominator = Math.Pow(2, GetComboOperand(operand));

    b = (long)(num / denominator);
}

void PerformCDV(long operand)
{
    var num = a;
    var denominator = Math.Pow(2, GetComboOperand(operand));

    c = (long)(num / denominator);
}

void PerformBXL(long operand)
{
    var bxl = b ^ GetLiteralOperand(operand);
    b = bxl;
}

void PerformBST(long operand)
{
    var bst = GetComboOperand(operand) % 8;
    b = bst;
}

void PerformJump(long operand, ref int index)
{
    if (a == 0)
        return;

    index = (int)(GetLiteralOperand(operand) - 2); //-2 as the value will be incremented by 2 by the 4 loop, hack 
}

void PerformBXC(long operand)
{
    // For legacy reasons, this instruction reads an operand but ignores it.

    var bxc = b ^ c;
    b = bxc;
}

void PerformOut(long operand, StringBuilder output)
{
    var value = GetComboOperand(operand) % 8;

    if (output.Length > 0)
    {
        output.Append(",");
    }

    output.Append(value);
}

long GetLiteralOperand(long input)
{
    return input;
}

long GetComboOperand(long input)
{
    return input switch
    {
        >= 0 and < 4 => input,
        4 => a,
        5 => b,
        6 => c,
        _ => throw new Exception()
    };
}