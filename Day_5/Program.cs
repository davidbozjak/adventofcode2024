using System.Text.RegularExpressions;

var updatesInput = new InputProvider<List<int>?>("Input_Updates.txt", GetIntList).Where(w => w != null).Cast<List<int>>().ToList();

var rulesInput = new InputProvider<(int, int)?>("Input_Order_Rules.txt", GetIntPair).Where(w => w != null).Cast<(int, int)>().ToList();

var rules = new Dictionary<int, List<int>>();

foreach (var rule in rulesInput)
{
    if (!rules.ContainsKey(rule.Item1))
    {
        rules[rule.Item1] = new List<int>();
    }

    rules[rule.Item1].Add(rule.Item2);
}

int sumRightOrder = 0;
int sumWrongOrder = 0;

foreach (var currentUpdate in updatesInput)
{
    bool isCorrect = true;

    for (int i = 0; isCorrect && i < currentUpdate.Count; i++)
    {
        var currentPage = currentUpdate[i];

        for (int j = 0; j < i; j++)
        {
            var pageThatShouldBeBefore = currentUpdate[j];

            if (rules.ContainsKey(pageThatShouldBeBefore))
            {
                var beforeRule = rules[pageThatShouldBeBefore];

                if (!beforeRule.Contains(currentPage))
                {
                    isCorrect = false;
                    break;
                }
            }
        }

        for (int j = i + 1; j < currentUpdate.Count; j++)
        {
            var pageThatShouldBeAfter = currentUpdate[j];

            if (rules.ContainsKey(pageThatShouldBeAfter))
            {
                var beforeRule = rules[pageThatShouldBeAfter];

                if (beforeRule.Contains(currentPage))
                {
                    isCorrect = false;
                    break;
                }
            }
        }
    }

    if (isCorrect)
    {
        var middleIndex = currentUpdate.Count / 2;
        var middleElement = currentUpdate[middleIndex];
        sumRightOrder += middleElement;
    }
    else
    {
        var newUpdateRule = new List<int>();

        // construct a new list that doesn't break any rule
        while (currentUpdate.Count > 0)
        {
            for (int i = 0; i < currentUpdate.Count; i++)
            {
                var current = currentUpdate[i];
                currentUpdate.Remove(current);

                if (!currentUpdate.Any(w => rules.ContainsKey(w) ? rules[w].Contains(current) : false))
                {
                    newUpdateRule.Add(current);
                }
                else
                {
                    currentUpdate.Insert(i, current);
                }
            }
        }

        var middleIndex = newUpdateRule.Count / 2;
        var middleElement = newUpdateRule[middleIndex];
        sumWrongOrder += middleElement;
    }
}

Console.WriteLine($"Part 1: {sumRightOrder}");
Console.WriteLine($"Part 2: {sumWrongOrder}");


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

static bool GetIntList(string? input, out List<int>? value)
{
    value = null;

    if (input == null) return false;

    Regex numRegex = new(@"-?\d+");

    value = numRegex.Matches(input).Select(w => int.Parse(w.Value)).ToList();

    return true;
}