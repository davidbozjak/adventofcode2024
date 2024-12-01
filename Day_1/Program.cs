using System.Text.RegularExpressions;

var wholeStringConvertInput = new InputProvider<(int, int)?>("Input.txt", GetIntPair).Where(w => w != null).Cast<(int, int)>()
    .ToList();

var list1 = wholeStringConvertInput.Select(w => w.Item1).OrderBy(w => w).ToList();
var list2 = wholeStringConvertInput.Select(w => w.Item2).OrderBy(w => w).ToList();

if (list1.Count != list2.Count)
    throw new Exception();

int sum = 0;

for(int i = 0; i < list1.Count; i++)
{
    sum += Math.Abs(list1[i] - list2[i]);
}

Console.WriteLine($"Part 1: {sum}");

int similarity = 0;

foreach (var item in list1)
{
    var count = list2.Count(w => w == item);

    similarity += item * count;
}

Console.WriteLine($"Part 2: {similarity}");

static bool GetIntPair(string? input, out (int, int)? value)
{
    value = null;

    if (input == null) return false;

    Regex numRegex = new(@"-?\d+");

    var numbers = numRegex.Matches(input).Select(w => int.Parse(w.Value)).ToList();

    if (numbers.Count != 2) throw new Exception();

    value = (numbers[0], numbers[1]);

    return true;
}
