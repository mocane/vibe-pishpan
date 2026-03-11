# Implementation Plan - WPF Time Tracker & Pomodoro

This document outlines the design and implementation steps for a single-window WPF application that combines daily task tracking, a Pomodoro timer, and a monthly historical view.

## Proposed Changes

### Core Models & Services

#### [NEW] `TaskEntry.cs`
- Store `DateTime Date` and `TimeSpan TotalTime`.
- Simple data holder for daily records.

#### [NEW] `TaskService.cs`
- Responsibility: Read and write task entries to a text file (`tasks.txt`).
- Format: `yyyy-MM-dd | hh:mm:ss`. This keeps it human-readable and easy to parse.
- Methods: `LoadAll()`, `Save(TaskEntry)`.

### ViewModels

#### [NEW] `MainViewModel.cs`
- **Task Tracking State**: `IsRunning`, `IsPaused`, `CurrentTaskTime` (TimeSpan).
- **Pomodoro State**: `PomodoroTimeLeft`, `ConfiguredPomodoroMinutes`, `IsPomodoroRunning`.
- **Logic**: Use `DispatcherTimer` for ticking.
- **Commands**: `StartTaskCommand`, `PauseTaskCommand`, `StopTaskCommand`, `StartPomodoroCommand`, `StopPomodoroCommand`.
- **Sound Logic**: Integrate `System.Media.SoundPlayer` to play Windows `.wav` sounds.

### User Interface

#### [MODIFY] `MainWindow.xaml`
- Use a `Grid` with two columns (left for controls/timers, right for the calendar).
- **Task Section**: Large `TextBlock` for `hh:mm:ss`. Buttons for Start, Pause, Stop.
- **Pomodoro Section**: `TextBlock` for countdown. Inputs for minutes and sound selection (from `C:\Windows\Media`).
- **Calendar Section**: WPF `Calendar` control with custom styles to highlight days with tasks.

## Verification Plan

### Automated Tests
- None requested, but manual verification of timer precision will be done.

### Manual Verification
- **Timer Accuracy**: Start the timer and verify it increments every second.
- **Persistence**: Stop a task, close the app, and check if `tasks.txt` contains the correct time.
- **Pomodoro**: Set a 1-minute Pomodoro, wait for it to end, and verify the selected sound plays.
- **Calendar**: Verify that days with saved time are visually distinct in the calendar.
