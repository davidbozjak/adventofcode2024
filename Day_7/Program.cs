using System.Text.RegularExpressions;

var equations = new InputProvider<List<long>?>("Input.txt", GetNumbersList).Where(w => w != null).Cast<List<long>>().ToList();

long sum = 0;

foreach (var equation in equations)
{
    var target = equation[0];
    var elements = equation.Skip(1);

    if (FindResult(target, 0, elements))
    {
        sum += target;
    }
}

Console.WriteLine($"Part 1: {sum}");

static bool FindResult(long target, long current, IEnumerable<long> remaining)
{
    if (current > target)
        return false;

    if (!remaining.Any())
    {
        return current == target;
    }
    else
    {
        var first = remaining.First();
        var rest = remaining.Skip(1).ToList();

        foreach (var o in Enum.GetValues<Operators>())
        {
            var newCurrent = o switch
            {
                Operators.Addition => current + first,
                Operators.Multiplication => current * first,
                Operators.Concatination => long.Parse(current.ToString() + first.ToString()),
                _ => throw new Exception()
            };

            if (FindResult(target, newCurrent, rest))
            {
                return true;
            }
        }

        return false;
    }
}

static bool GetNumbersList(string? input, out List<long>? value)
{
    value = null;

    if (input == null) return false;

    Regex numRegex = new(@"-?\d+");

    value = numRegex.Matches(input).Select(w => long.Parse(w.Value)).ToList();

    if (value.Count < 2) throw new Exception();

    return true;
}

enum Operators { Addition, Multiplication, Concatination };