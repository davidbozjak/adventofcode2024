var input = new StringInputProvider("Input.txt").First();

var memory = InitializeMemoryFromInput(input);

for (int i = 0, j = memory.Count - 1;  i < j;)
{
    while (memory[i] >= 0) i++;
    while (memory[j] < 0) j--;

    if (j <= i) break;

    memory[i] = memory[j];
    memory[j] = -1;
}

Console.WriteLine($"Part 1: {GetChecksum(memory)}");

memory = InitializeMemoryFromInput(input);

List<long> alreadyMoved = [];

for (int j = memory.Count - 1; j > 0;)
{
    long fileId = memory[j];

    if (!alreadyMoved.Contains(fileId))
    {
        int segmentLength = 1;
        for (; (j - segmentLength) > 0 && memory[j - segmentLength] == fileId; segmentLength++) ;

        if ((j - segmentLength) <= 0)
            break; //done!

        bool fits = false;

        int i = 0;
        int gapLength = 0;

        while (!fits && i < j)
        {
            while (memory[i] >= 0) i++;

            if (i > j)
                break;

            gapLength = (int)-memory[i];

            fits = gapLength >= segmentLength;

            if (!fits)
            {
                i += gapLength;

                if (i >= memory.Count)
                    break;
            }
        }

        if (fits)
        {
            if (i > j)
                throw new Exception("Only move towards left");

            for (int c = 0; c < segmentLength; c++, i++, j--)
            {
                memory[i] = memory[j];
                memory[j] = -1;
            }
        }

        alreadyMoved.Add(fileId);
    }

    while (memory[j] == fileId) j--;
    while (memory[j] < 0) j--;
}

Console.WriteLine($"Part 2: {GetChecksum(memory)}");

List<long> InitializeMemoryFromInput(string input)
{
    List<long> memory = [];

    bool file = true;
    long fileId = 0;

    for (int i = 0; i < input.Length; i++)
    {
        int count = input[i] - '0';

        if (file)
        {
            long insert = fileId;
            fileId++;
            file = false;

            for (int j = 0; j < count; j++)
            {
                memory.Add(insert);
            }
        }
        else
        {
            for (; count > 0; count--)
            {
                memory.Add(-count);
            }

            file = true;
        }
    }

    return memory;
}

long GetChecksum(List<long> memory)
{
    long checksum = 0;

    for (int i = 0; i < memory.Count; i++)
    {
        checksum += i * (memory[i] < 0 ? 0 : memory[i]);
    }

    return checksum;
}

void PrintMemory(List<long> memory)
{
    for (int i = 0; i < memory.Count; i++)
    {
        Console.WriteLine(memory[i]);

        if (i % 25 == 0)
        {
            Console.WriteLine("Press any key to continue printing");
            Console.ReadKey();
            Console.WriteLine($"Cont, i: {i}");
        }
    }
}