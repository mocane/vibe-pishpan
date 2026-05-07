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
        private readonly DispatcherTimer _autoSaveTimer;

        private TimeSpan _currentTaskTime;
        private TimeSpan _pomodoroTimeLeft;
        private int _pomodoroDurationMinutes = 25;
        private string? _selectedSoundPath;
        private string? _selectedWorkdaySoundPath;
        private bool _isTaskRunning;
        private bool _isPomodoroRunning;
        private string? _selectedMonth;
        private int _selectedYear;
        private int _daysWorkedInSelectedMonth;

        public MainViewModel()
        {
            _taskService = new TaskService();
            
            _taskTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _taskTimer.Tick += TaskTimer_Tick;

            _pomodoroTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _pomodoroTimer.Tick += PomodoroTimer_Tick;

            _autoSaveTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(5) };
            _autoSaveTimer.Tick += (s, e) => SaveTask();

            StartTaskCommand = new RelayCommand(_ => StartTask(), _ => !IsTaskRunning);
            StopTaskCommand = new RelayCommand(_ => StopTask(), _ => IsTaskRunning || CurrentTaskTime > TimeSpan.Zero);

            StartPomodoroCommand = new RelayCommand(_ => StartPomodoro(), _ => !IsPomodoroRunning);
            StopPomodoroCommand = new RelayCommand(_ => StopPomodoro(), _ => IsPomodoroRunning);
            AddFiveMinutesCommand = new RelayCommand(_ => AddFiveMinutes());

            LoadInitialData();
            LoadAvailableSounds();
            InitializeSelectors();
        }

        public TimeSpan CurrentTaskTime
        {
            get => _currentTaskTime;
            set { _currentTaskTime = value; OnPropertyChanged(); OnPropertyChanged(nameof(TaskTimeFormatted)); OnPropertyChanged(nameof(WorkdayFinishTimeFormatted)); }
        }

        public string TaskTimeFormatted => string.Format("{0:00}:{1:00}:{2:00}", (int)CurrentTaskTime.TotalHours, CurrentTaskTime.Minutes, CurrentTaskTime.Seconds);

        public string WorkdayFinishTimeFormatted
        {
            get
            {
                var workDay = TimeSpan.FromHours(8);
                if (CurrentTaskTime >= workDay) return "Workday Finished";
                
                var remaining = workDay - CurrentTaskTime;
                var finishTime = DateTime.Now.Add(remaining);
                return $"Estimated Finish: {finishTime:HH:mm:ss}";
            }
        }

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

        public string? SelectedWorkdaySoundPath
        {
            get => _selectedWorkdaySoundPath;
            set { _selectedWorkdaySoundPath = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> AvailableMonths { get; } = new ObservableCollection<string>();
        public ObservableCollection<int> AvailableYears { get; } = new ObservableCollection<int>();

        public string? SelectedMonth
        {
            get => _selectedMonth;
            set 
            { 
                _selectedMonth = value; 
                OnPropertyChanged(); 
                UpdateDaysWorked(); 
            }
        }

        public int SelectedYear
        {
            get => _selectedYear;
            set
            {
                _selectedYear = value;
                OnPropertyChanged();
                UpdateDaysWorked();
            }
        }

        public int DaysWorkedInSelectedMonth
        {
            get => _daysWorkedInSelectedMonth;
            set { _daysWorkedInSelectedMonth = value; OnPropertyChanged(); }
        }

        public ICommand StartTaskCommand { get; }
        public ICommand StartPomodoroCommand { get; }
        public ICommand StopPomodoroCommand { get; }
        public ICommand StopTaskCommand { get; }
        public ICommand AddFiveMinutesCommand { get; }

        private void LoadInitialData()
        {
            var all = _taskService.LoadAll();
            var todayEntry = all.FirstOrDefault(e => e.Date.Date == DateTime.Today);
            if (todayEntry != null)
            {
                CurrentTaskTime = todayEntry.TotalTime;
            }
        }

        private void InitializeSelectors()
        {
            // Months
            for (int i = 1; i <= 12; i++)
            {
                AvailableMonths.Add(new DateTime(DateTime.Today.Year, i, 1).ToString("MMMM"));
            }
            _selectedMonth = DateTime.Today.ToString("MMMM");
            OnPropertyChanged(nameof(SelectedMonth));

            // Years - from the first year in data to current + 1
            var all = _taskService.LoadAll();
            int minYear = all.Any() ? all.Min(e => e.Date.Year) : DateTime.Today.Year;
            int maxYear = DateTime.Today.Year;

            for (int y = minYear; y <= maxYear; y++)
            {
                AvailableYears.Add(y);
            }
            
            // If the current year is not in the list (empty file), add it
            if (!AvailableYears.Contains(DateTime.Today.Year))
            {
                AvailableYears.Add(DateTime.Today.Year);
            }

            _selectedYear = DateTime.Today.Year;
            OnPropertyChanged(nameof(SelectedYear));

            UpdateDaysWorked();
        }

        private void UpdateDaysWorked()
        {
            if (string.IsNullOrEmpty(SelectedMonth) || SelectedYear == 0) return;
            
            int monthIndex = DateTime.ParseExact(SelectedMonth, "MMMM", null).Month;
            DaysWorkedInSelectedMonth = _taskService.GetDaysWorkedInMonth(SelectedYear, monthIndex);
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
                SelectedWorkdaySoundPath = AvailableSounds.FirstOrDefault(s => s.Contains("tada")) ?? AvailableSounds.FirstOrDefault();
            }
        }

        private void TaskTimer_Tick(object? sender, EventArgs e)
        {
            var oldTime = CurrentTaskTime;
            CurrentTaskTime = CurrentTaskTime.Add(TimeSpan.FromSeconds(1));
            
            // Check for 8 hour milestone
            if (oldTime < TimeSpan.FromHours(8) && CurrentTaskTime >= TimeSpan.FromHours(8))
            {
                PlaySound(SelectedWorkdaySoundPath);
            }
        }

        private void StartTask()
        {
            IsTaskRunning = true;
            _taskTimer.Start();
            _autoSaveTimer.Start();
        }

        public void StopTask()
        {
            IsTaskRunning = false;
            _taskTimer.Stop();
            _autoSaveTimer.Stop();
            SaveTask();
        }

        public void SaveTask()
        {
            _taskService.SaveOrUpdate(new TaskEntry { Date = DateTime.Today, TotalTime = CurrentTaskTime });
            UpdateDaysWorked();
        }

        private void AddFiveMinutes()
        {
            CurrentTaskTime = CurrentTaskTime.Add(TimeSpan.FromMinutes(5));
            SaveTask();
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
                PlaySound(SelectedSoundPath);
            }
        }

        private void PlaySound(string? path)
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                try
                {
                    using (var player = new SoundPlayer(path))
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
