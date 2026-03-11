using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;
using PishpanTimeTracker.Models;
using PishpanTimeTracker.Services;

namespace PishpanTimeTracker.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly TaskService _taskService;
        private readonly DispatcherTimer _taskTimer;
        private readonly DispatcherTimer _pomodoroTimer;

        private TimeSpan _currentTaskTime;
        private TimeSpan _pomodoroTimeLeft;
        private int _pomodoroDurationMinutes = 25;
        private string? _selectedSoundPath;
        private bool _isTaskRunning;
        private bool _isPomodoroRunning;

        public MainViewModel()
        {
            _taskService = new TaskService();
            
            _taskTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _taskTimer.Tick += TaskTimer_Tick;

            _pomodoroTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _pomodoroTimer.Tick += PomodoroTimer_Tick;

            StartTaskCommand = new RelayCommand(_ => StartTask(), _ => !IsTaskRunning);
            PauseTaskCommand = new RelayCommand(_ => PauseTask(), _ => IsTaskRunning);
            StopTaskCommand = new RelayCommand(_ => StopTask(), _ => IsTaskRunning || CurrentTaskTime > TimeSpan.Zero);

            StartPomodoroCommand = new RelayCommand(_ => StartPomodoro(), _ => !IsPomodoroRunning);
            StopPomodoroCommand = new RelayCommand(_ => StopPomodoro(), _ => IsPomodoroRunning);

            LoadInitialData();
            LoadAvailableSounds();
        }

        public TimeSpan CurrentTaskTime
        {
            get => _currentTaskTime;
            set { _currentTaskTime = value; OnPropertyChanged(); }
        }

        public string TaskTimeFormatted => string.Format("{0:00}:{1:00}:{2:00}", (int)CurrentTaskTime.TotalHours, CurrentTaskTime.Minutes, CurrentTaskTime.Seconds);

        public TimeSpan PomodoroTimeLeft
        {
            get => _pomodoroTimeLeft;
            set { _pomodoroTimeLeft = value; OnPropertyChanged(); OnPropertyChanged(nameof(PomodoroTimeFormatted)); }
        }

        public string PomodoroTimeFormatted => $"{PomodoroTimeLeft.Minutes:D2}:{PomodoroTimeLeft.Seconds:D2}";

        public int PomodoroDurationMinutes
        {
            get => _pomodoroDurationMinutes;
            set { _pomodoroDurationMinutes = value; OnPropertyChanged(); }
        }

        public bool IsTaskRunning
        {
            get => _isTaskRunning;
            set { _isTaskRunning = value; OnPropertyChanged(); }
        }

        public bool IsPomodoroRunning
        {
            get => _isPomodoroRunning;
            set { _isPomodoroRunning = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> AvailableSounds { get; } = new ObservableCollection<string>();

        public string? SelectedSoundPath
        {
            get => _selectedSoundPath;
            set { _selectedSoundPath = value; OnPropertyChanged(); }
        }

        public ObservableCollection<DateTime> HighlightedDays { get; } = new ObservableCollection<DateTime>();

        public ICommand StartTaskCommand { get; }
        public ICommand StartPomodoroCommand { get; }
        public ICommand StopPomodoroCommand { get; }
        public ICommand PauseTaskCommand { get; }
        public ICommand StopTaskCommand { get; }

        private void LoadInitialData()
        {
            var all = _taskService.LoadAll();
            var todayEntry = all.FirstOrDefault(e => e.Date.Date == DateTime.Today);
            if (todayEntry != null)
            {
                CurrentTaskTime = todayEntry.TotalTime;
            }
            
            foreach (var date in all.Where(e => e.TotalTime > TimeSpan.Zero).Select(e => e.Date.Date))
            {
                if (!HighlightedDays.Contains(date))
                    HighlightedDays.Add(date);
            }
            
            OnPropertyChanged(nameof(TaskTimeFormatted));
        }

        private void LoadAvailableSounds()
        {
            string mediaPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Media");
            if (Directory.Exists(mediaPath))
            {
                var sounds = Directory.GetFiles(mediaPath, "*.wav");
                foreach (var sound in sounds)
                {
                    AvailableSounds.Add(sound);
                }
                SelectedSoundPath = AvailableSounds.FirstOrDefault(s => s.Contains("Alarm01")) ?? AvailableSounds.FirstOrDefault();
            }
        }

        private void TaskTimer_Tick(object? sender, EventArgs e)
        {
            CurrentTaskTime = CurrentTaskTime.Add(TimeSpan.FromSeconds(1));
            OnPropertyChanged(nameof(TaskTimeFormatted));
        }

        private void StartTask()
        {
            IsTaskRunning = true;
            _taskTimer.Start();
        }

        private void PauseTask()
        {
            IsTaskRunning = false;
            _taskTimer.Stop();
        }

        private void StopTask()
        {
            IsTaskRunning = false;
            _taskTimer.Stop();
            _taskService.SaveOrUpdate(new TaskEntry { Date = DateTime.Today, TotalTime = CurrentTaskTime });
            
            if (!HighlightedDays.Contains(DateTime.Today.Date))
            {
                HighlightedDays.Add(DateTime.Today.Date);
            }
        }

        private void StartPomodoro()
        {
            PomodoroTimeLeft = TimeSpan.FromMinutes(PomodoroDurationMinutes);
            IsPomodoroRunning = true;
            _pomodoroTimer.Start();
        }

        private void StopPomodoro()
        {
            IsPomodoroRunning = false;
            _pomodoroTimer.Stop();
        }

        private void PomodoroTimer_Tick(object? sender, EventArgs e)
        {
            if (PomodoroTimeLeft.TotalSeconds > 0)
            {
                PomodoroTimeLeft = PomodoroTimeLeft.Subtract(TimeSpan.FromSeconds(1));
            }
            else
            {
                StopPomodoro();
                PlaySound();
            }
        }

        private void PlaySound()
        {
            if (!string.IsNullOrEmpty(SelectedSoundPath) && File.Exists(SelectedSoundPath))
            {
                try
                {
                    using (var player = new SoundPlayer(SelectedSoundPath))
                    {
                        player.Play();
                    }
                }
                catch { /* Ignore sound errors */ }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
