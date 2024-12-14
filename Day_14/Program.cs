using System.Drawing;
using System.Text.RegularExpressions;

var agents = new InputProvider<Agent?>("Input.txt", GetAgent).Where(w => w != null).Cast<Agent>().ToList();

int worldSizeX = 101;
int worldSizeY = 103;

int globalStepCount = 0;

ApplyStepsToAll(100);

var quadrants = GetQuadrants();

var sum = quadrants.Values.Select(w => w.Count).Aggregate(1, (mul, next) => mul * next);

// Fast forward based on experimentation
ApplyStepsToAll(6500);

// Print one the agents are mostly in the same quadrant, then visually confirm :)

var world = new SimpleWorld<Agent>(agents);
var printer = new WorldPrinter();

while (true)
{
    ApplyStepsToAll(1);

    quadrants = GetQuadrants();

    if (quadrants.Values.Any(w => w.Count > agents.Count / 2))
    {
        printer.Print(world);
        Console.WriteLine($"Part 2, current picture after {globalStepCount} steps");
        Console.WriteLine($"Part 1: {sum}");
        await Task.Delay(100);
        Console.ReadLine();

    }
}

Dictionary<int?, List<Agent>> GetQuadrants()
{
    return agents.GroupBy(w => GetQuadrant(w.Position)).Where(w => w.Key != null).ToDictionary(w => w.Key, w => w.ToList());

    int? GetQuadrant(Point p)
    {
        int midX = worldSizeX / 2;
        int midY = worldSizeY / 2;

        if (p.X < 0)
            throw new Exception();

        if (p.X > worldSizeX)
            throw new Exception();

        if (p.Y < 0)
            throw new Exception();

        if (p.Y > worldSizeY)
            throw new Exception();

        if (p.X < midX)
        {
            if (p.Y < midY)
            {
                return 0;
            }
            else if (p.Y > midY)
            {
                return 1;
            }
        }
        else if (p.X > midX)
        {
            if (p.Y < midY)
            {
                return 2;
            }
            else if (p.Y > midY)
            {
                return 3;
            }
        }

        return null;
    }
}

void ApplyStepsToAll(int steps)
{
    foreach (var agent in agents)
    {
        agent.MakeSteps(steps, worldSizeX, worldSizeY);
    }

    globalStepCount += steps;
}

static bool GetAgent(string? input, out Agent? value)
{
    value = null;

    if (input == null) return false;

    Regex numRegex = new(@"-?\d+");

    var numbers = numRegex.Matches(input).Select(w => int.Parse(w.Value)).ToList();

    if (numbers.Count != 4) throw new Exception();

    value = new Agent(numbers[0], numbers[1], numbers[2], numbers[3]);

    return true;
}

class Agent : IWorldObject
{
    public Point Position { get; private set; }

    public Point Velocity { get; }

    public char CharRepresentation => 'X';

    public int Z => 1;

    public Agent(int x, int y, int vx, int vy)
    {
        this.Position = new Point(x, y);
        this.Velocity = new Point(vx, vy);
    }

    public void MakeSteps(int noOfSteps, int maxX, int maxY)
    {
        var newX = this.Position.X + noOfSteps * this.Velocity.X;

        while (newX < 0)
        {
            newX += maxX;
        }

        while (newX >= maxX)
        {
            newX -= maxX;
        }

        var newY = this.Position.Y + noOfSteps * this.Velocity.Y;

        while (newY < 0)
        {
            newY += maxY;
        }

        while (newY >= maxY)
        {
            newY -= maxY;
        }

        this.Position = new Point(newX, newY);
    }
}