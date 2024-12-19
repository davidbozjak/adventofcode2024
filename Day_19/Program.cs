var avaliablePatterns = new StringInputProvider("InputPatterns.txt").First().Split([",", " "], StringSplitOptions.RemoveEmptyEntries);
var wishlist = new StringInputProvider("InputWishlist.txt").ToList();

int distinct = 0;
long sum = 0;

Dictionary<string, long> memDict = new();

foreach (var wish in wishlist)
{
    var patternCount = AssemblePattern(wish);
    if (patternCount > 0) distinct++;
    sum += patternCount;
}

Console.WriteLine($"Part 1: {distinct}");
Console.WriteLine($"Part 2: {sum}");

long AssemblePattern(string key)
{
    if (!memDict.ContainsKey(key))
    {
        memDict[key] = AssemblePatternInternal(key);
    }

    return memDict[key];

    long AssemblePatternInternal(string key)
    {
        if (key.Length == 0)
            return 1;

        long sum = 0;

        foreach (var pattern in avaliablePatterns)
        {
            if (key.StartsWith(pattern))
            {
                sum += AssemblePattern(key[pattern.Length..]);
            }
        }

        return sum;
    }
}