var inputs = new InputProvider<long>("Input.txt", long.TryParse).ToList();

long sum = 0;

List<List<int>> pricesAllBuyers = new();
List<List<int>> priceDiffsAllBuyers = new();

foreach (var input in inputs)
{
    var num = input;

    var prices = new List<int>();
    var priceDiffs = new List<int>() { -99999 };
    var prevPrice = -1;

    for (int i = 0; i < 2000; i++)
    {
        var price = (int)(num % 10);
        prices.Add(price);

        if (prevPrice > -1)
        {
            priceDiffs.Add(price - prevPrice);
        }
        prevPrice = price;
        num = EvolveNumber(num);
    }

    pricesAllBuyers.Add(prices);
    priceDiffsAllBuyers.Add(priceDiffs);

    sum += num;
}

Console.WriteLine($"Part 1: {sum}");

long bestSumBananas = long.MinValue;

for (int diff1 = -9; diff1 < 10; diff1++)
{
    for (int diff2 = -9; diff2 < 10; diff2++)
    {
        for (int diff3 = -9; diff3 < 10; diff3++)
        {
            for (int diff4 = -9; diff4 < 10; diff4++)
            {
                var sumBananas = GetBananasForSequence(diff1, diff2, diff3, diff4);

                if (sumBananas > bestSumBananas)
                {
                    bestSumBananas = sumBananas;
                }
            }
        }
    }
}



Console.WriteLine($"Part 2: {bestSumBananas}");

static long EvolveNumber(long number)
{
    var numX64 = number * 64;

    number = MixInto(number, numX64);
    number = Prune(number);

    var numD32 = number / 32;

    number = MixInto(number, numD32);
    number = Prune(number);

    var numX2048 = number * 2048;

    number = MixInto(number, numX2048);
    number = Prune(number);

    return number;
}

static long MixInto(long number, long mixIntoNumber)
{
    return number ^ mixIntoNumber;
}

static long Prune(long number)
{
    return number % 16777216;
}

long GetBananasForSequence(int diff1, int diff2, int diff3, int diff4)
{
    long sumBananas = 0;

    for (int buyerIndex = 0; buyerIndex < inputs.Count; buyerIndex++)
    {
        var prices = pricesAllBuyers[buyerIndex];
        var priceDiffs = priceDiffsAllBuyers[buyerIndex];

        for (int i = 3; i < priceDiffs.Count; i++)
        {
            if ((priceDiffs[i - 3] == diff1) &&
                (priceDiffs[i - 2] == diff2) &&
                (priceDiffs[i - 1] == diff3) &&
                (priceDiffs[i] == diff4))
            {
                sumBananas += prices[i];
                break;
            }
        }
    }

    return sumBananas;
}