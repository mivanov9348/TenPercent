// --- Services/ScoutingEngine.cs ---
namespace TenPercent.Application.Services
{
    using System;
    using TenPercent.Application.Interfaces;
    public class ScoutingEngine : IScoutingEngine
    {
        private readonly Random _rand = new Random();

        public string MaskAttribute(int trueValue, int scoutingLevel)
        {
            // Level 3: Perfect scouting - we see the exact value
            if (scoutingLevel >= 3)
            {
                return trueValue.ToString();
            }

            // Level 2: Good scouting - narrow range (+/- 3)
            if (scoutingLevel == 2)
            {
                int lowerBound = Math.Max(1, trueValue - _rand.Next(1, 4));
                int upperBound = Math.Min(99, trueValue + _rand.Next(1, 4));
                return $"{lowerBound}-{upperBound}";
            }

            // Level 1: Weak/Initial scouting - wide range (+/- 8)
            // Default behavior
            int lower = Math.Max(1, trueValue - _rand.Next(4, 9));
            int upper = Math.Min(99, trueValue + _rand.Next(4, 9));

            return $"{lower}-{upper}";
        }
    }
}