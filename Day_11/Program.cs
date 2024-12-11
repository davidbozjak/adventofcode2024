using System.Text.RegularExpressions;

var stoneList = new InputProvider<List<long>?>("Input.txt", GetIntList).Where(w => w != null).Cast<List<long>>().First();

Dictionary<(long, long), long> memcache = new();

long sum25 = stoneList.Sum(w => GetNumberOfStonesForStoneAtFinalBlink(w, 0, 25));

Console.WriteLine($"Part 1: {sum25}");
memcache.Clear();

long sum75 = stoneList.Sum(w => GetNumberOfStonesForStoneAtFinalBlink(w, 0, 75));

Console.WriteLine($"Part 2: {sum75}");

long GetNumberOfStonesForStoneAtFinalBlink(long stone, long currentBlink, long finalBlink)
{
    if (!memcache.ContainsKey((stone, currentBlink)))
    {
        memcache[(stone, currentBlink)] = GetNumberOfStonesForStoneAtFinalBlink_Internal(stone, currentBlink);
    }

    return memcache[(stone, currentBlink)];

    long GetNumberOfStonesForStoneAtFinalBlink_Internal(long stone, long currentBlink)
    {
        if (currentBlink == finalBlink)
        {
            return 1;
        }
        else
        {
            List<long> lst;
            if (stone == 0)
            {
                lst = [1];
            }
            else
            {
                var str = stone.ToString();
                if (str.Length % 2 == 0)
                {
                    var halfIndex = str.Length / 2;
                    var left = str[..halfIndex];
                    var right = str[halfIndex..];

                    lst = [long.Parse(left), long.Parse(right)];
                }
                else
                {
                    lst = [stone * 2024];
                }
            }

            long sum = 0;
            foreach (var next in lst)
            {
                sum += GetNumberOfStonesForStoneAtFinalBlink(next, currentBlink + 1, finalBlink);
            }
            return sum;
        }
    }
}

static bool GetIntList(string? input, out List<long>? value)
{
    value = null;

    if (input == null) return false;

    Regex numRegex = new(@"-?\d+");

    value = numRegex.Matches(input).Select(w => long.Parse(w.Value)).ToList();

    return true;
}