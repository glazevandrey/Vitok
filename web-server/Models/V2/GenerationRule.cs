using System;

namespace web_server.Models.V2
{
    public class GenerationRule
    {
        public bool Looped { get; set; }
        public DateTime FirstOccurrence { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan Duration { get; set; }
    }
}
