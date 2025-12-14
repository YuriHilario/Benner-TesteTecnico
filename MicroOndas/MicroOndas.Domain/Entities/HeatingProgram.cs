using MicroOndas.Domain.Enums;

namespace MicroOndas.Domain.Entities
{
    public class HeatingProgram
    {
        private const int MaxProcessingLength = 40;

        public int TimeInSeconds { get; private set; }
        public int Power { get; private set; }

        public int TimeRemaining { get; private set; }
        public HeatingStatus Status { get; private set; } = HeatingStatus.Stopped;

        public string ProcessingString { get; private set; } = string.Empty;
        public string DisplayTimeFormatted { get; private set; } = string.Empty;

        public char HeatingChar { get; private set; } = '*';
        public bool IsPredefinedProgram { get; private set; }

        public HeatingProgram(
            int timeInSeconds,
            int power,
            char heatingChar = '*',
            bool isPredefined = false)
        {
            TimeInSeconds = timeInSeconds;
            Power = power;
            TimeRemaining = timeInSeconds;
            HeatingChar = heatingChar;
            IsPredefinedProgram = isPredefined;

            UpdateDisplayTime();
        }

        public void Start()
        {
            Status = HeatingStatus.InProgress;
        }

        public void Pause()
        {
            if (Status == HeatingStatus.InProgress)
                Status = HeatingStatus.Paused;
        }

        public void Cancel()
        {
            Status = HeatingStatus.Stopped;
            TimeRemaining = 0;
            ProcessingString = string.Empty;
            UpdateDisplayTime();
        }

        public void IncrementTime(int seconds)
        {
            if (IsPredefinedProgram || Status != HeatingStatus.InProgress)
                return;

            TimeRemaining += seconds;
            UpdateDisplayTime();
        }

        public void DecrementTime()
        {
            // Se estiver concluído, no próximo tick volta para Stopped
            if (Status == HeatingStatus.Completed)
            {
                Status = HeatingStatus.Stopped;
                return;
            }

            if (Status != HeatingStatus.InProgress || TimeRemaining <= 0)
                return;

            TimeRemaining--;
            UpdateDisplayTime();
            AppendProcessingString();

            if (TimeRemaining == 0)
            {
                Status = HeatingStatus.Completed;
                ProcessingString += " Aquecimento concluído";
            }
        }

        private void AppendProcessingString()
        {
            if (ProcessingString.Length >= MaxProcessingLength)
                return;

            var toAppend = new string(HeatingChar, Power);
            ProcessingString += toAppend;

            if (ProcessingString.Length > MaxProcessingLength)
            {
                ProcessingString = ProcessingString.Substring(0, MaxProcessingLength);
            }
        }

        private void UpdateDisplayTime()
        {
            if (TimeRemaining >= 60)
            {
                int minutes = TimeRemaining / 60;
                int seconds = TimeRemaining % 60;
                DisplayTimeFormatted = $"{minutes}:{seconds:D2}";
            }
            else
            {
                DisplayTimeFormatted = $"{TimeRemaining}s";
            }
        }
    }
}
