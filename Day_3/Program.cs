using System.Text.RegularExpressions;

Regex mulCommandRegex = new(@"mul\((-?\d+),(-?\d+)\)");
Regex mulAndStateCommandRegex = new(@"mul\((-?\d+),(-?\d+)\)|do\(\)|don't\(\)");

var wholeStringInput = new StringInputProvider("Input.txt");

var input = wholeStringInput.First();

var matches = mulCommandRegex.Matches(input);

Console.WriteLine($"Part 1: {GetSumForMatches(mulCommandRegex.Matches(input))}");
Console.WriteLine($"Part 2: {GetSumForMatches(mulAndStateCommandRegex.Matches(input))}");

static long GetSumForMatches(MatchCollection matches)
{
    Regex numRegex = new(@"-?\d+");

    long sum = 0;

    bool enabled = true;

    foreach (var match in matches)
    {
        var substr = match.ToString();

        if (!enabled)
        {
            if (substr == "do()")
            {
                enabled = true;
            }
            continue;
        }
        else
        {
            if (substr == "do()")
                continue;

            if (substr == "don't()")
            {
                enabled = false;
                continue;
            }

            var numbers = numRegex.Matches(substr).Select(w => long.Parse(w.Value)).ToList();

            if (numbers.Count != 2)
                throw new Exception();

            sum += numbers[0] * numbers[1];
        }
    }

    return sum;
}