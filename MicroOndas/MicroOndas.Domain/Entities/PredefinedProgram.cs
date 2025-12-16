public class PredefinedProgram
{
    public string Name { get; }
    public string Description { get; }
    public int TimeInSeconds { get; }
    public int Power { get; }
    public char HeatingChar { get; }
    public string Instructions { get; }

    public PredefinedProgram(string name, string description, int timeInSeconds, int power, char heatingChar, string instructions)
    {
        Name = name;
        Description = description;
        TimeInSeconds = timeInSeconds;
        Power = power;
        HeatingChar = heatingChar;
        Instructions = instructions;
    }
}
