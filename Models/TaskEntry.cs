using System;

namespace PishpanTimeTracker.Models
{
    public class TaskEntry
    {
        public DateTime Date { get; set; }
        public TimeSpan TotalTime { get; set; }

        public override string ToString()
        {
            return $"{Date:yyyy-MM-dd} | {TotalTime:hh\\:mm\\:ss}";
        }
    }
}
