using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PishpanTimeTracker.Models;

namespace PishpanTimeTracker.Services
{
    public class TaskService
    {
        private const string FileName = "tasks.txt";
        private readonly string _filePath;

        public TaskService()
        {
            _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileName);
        }

        public List<TaskEntry> LoadAll()
        {
            var entries = new List<TaskEntry>();
            if (!File.Exists(_filePath)) return entries;

            var lines = File.ReadAllLines(_filePath);
            foreach (var line in lines)
            {
                var parts = line.Split('|').Select(p => p.Trim()).ToArray();
                if (parts.Length == 2 &&
                    DateTime.TryParse(parts[0], out var date) &&
                    TimeSpan.TryParse(parts[1], out var time))
                {
                    entries.Add(new TaskEntry { Date = date, TotalTime = time });
                }
            }
            return entries;
        }

        public void SaveOrUpdate(TaskEntry entry)
        {
            var allEntries = LoadAll();
            var existing = allEntries.FirstOrDefault(e => e.Date.Date == entry.Date.Date);

            if (existing != null)
            {
                existing.TotalTime = entry.TotalTime;
            }
            else
            {
                allEntries.Add(entry);
            }

            var lines = allEntries.Select(e => e.ToString());
            File.WriteAllLines(_filePath, lines);
        }
    }
}
