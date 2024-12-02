using System.Text.RegularExpressions;

var reportsInput = new InputProvider<List<int>?>("Input.txt", GetReports).Where(w => w != null).Cast<List<int>?>().ToList();

var safeReports = reportsInput.Where(IsSafe);

Console.WriteLine($"Part 1: {safeReports.Count()}");

safeReports = reportsInput.Where(IsSafeBySkippingMaxOne);

Console.WriteLine($"Part 2: {safeReports.Count()}");

static bool IsSafeBySkippingMaxOne(List<int> report)
{
    if (IsSafe(report))
        return true;

    for (int i = 0; i < report.Count; i++)
    {
        var newList = new List<int>();
        newList.AddRange(report[..i]);
        newList.AddRange(report[(i + 1)..]);

        if (IsSafe(newList))
            return true;
    }

    return false;
}

static bool IsSafe(List<int> report)
{
    bool? isIncreasing = null;

    for (int i = 1; i < report.Count; i++)
    {
        var diff = report[i] - report[i - 1];
        var absDiff = Math.Abs(diff);

        if (absDiff < 1 || absDiff > 3)
            return false;

        if (isIncreasing == null)
        {
            isIncreasing = diff > 0;
        }
        else if (isIncreasing == false)
        {
            if (diff > 0)
            {
                return false;
            }
        }
        else if (isIncreasing == true)
        {
            if (diff < 0)
            {
                return false;
            }
        }
        else throw new Exception();
    }

    return true;
}

static bool GetReports(string? input, out List<int>? value)
{
    value = null;

    if (input == null) return false;

    Regex numRegex = new(@"-?\d+");

    var numbers = numRegex.Matches(input).Select(w => int.Parse(w.Value)).ToList();

    if (numbers.Count == 0) throw new Exception();

    value = numbers;

    return true;
}