namespace MicroOndas.Application.DTOs
{
    public class ProgramCreationDto
    {
        public string Name { get; set; } = string.Empty;
        public string Food { get; set; } = string.Empty;
        public int TimeInSeconds { get; set; }
        public int Power { get; set; }
        public char HeatingChar { get; set; }
        public string Instructions { get; set; } = string.Empty;
    }
}