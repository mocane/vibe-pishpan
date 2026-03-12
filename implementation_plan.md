# Implementation Plan - WPF Time Tracker & Pomodoro (Modifications)

This document outlines the design and implementation steps for modifying the existing WPF application based on the new requirements.

## Goal Description
1. Remove the Pause button.
2. Auto-start the timer if there is already time recorded for today when the app opens.
3. Display the estimated time when the 8-hour workday finishes (`hh:mm:ss`), updated live.
4. Add a workday completion sound, configurable like the Pomodoro sound, played when 8 hours are reached.
5. Remove the calendar control and its associated logic.
6. Add a Month and Year selector that displays the number of days worked in the selected month/year based on the saved text file.
7. Auto-save the tracked time to the file every 5 minutes to prevent data loss.

## User Review Required
No critical concerns. The month selector will populate months that have recorded activity (or a static list of 1-12) to keep it simple. We will use a standard `ComboBox` for month selection.

## Proposed Changes

### Core Models & Services

#### [MODIFY] `TaskService.cs`
- Add a method to get the number of days worked for a specific year and month to support the new UI feature.
- Method signature: `int GetDaysWorkedInMonth(int year, int month)`.

### ViewModels

#### [MODIFY] `MainViewModel.cs`
- **Timer Autostart**: In `LoadInitialData()`, if `CurrentTaskTime > TimeSpan.Zero`, automatically call `StartTask()`.
- **Remove Pause**: Remove `PauseTaskCommand` and its associated methods.
- **Estimated Finish Time**:
  - Add property `WorkdayFinishTimeFormatted`.
  - In `TaskTimer_Tick`, calculate: `DateTime.Now + (TimeSpan.FromHours(8) - CurrentTaskTime)`. If the time exceeds 8 hours, it can display "Finished" or the time it was completed.
- **Workday Completion Sound**:
  - Add `SelectedWorkdaySoundPath` property (similar to `SelectedSoundPath` for Pomodoro).
  - Add a check in `TaskTimer_Tick` so that when `CurrentTaskTime.TotalHours` reaches exactly 8, it plays the selected sound.
- **Auto-Save**:
  - Create a dedicated `_autoSaveTimer` with a 5-minute interval that calls `_taskService.SaveOrUpdate()` while the task is running.
- **Month & Year Selector & Stats**:
  - Remove `HighlightedDays`.
  - Add `ObservableCollection<string> AvailableMonths` and `ObservableCollection<int> AvailableYears`.
  - Add `SelectedMonth` and `SelectedYear` properties. When either changes, update `DaysWorkedInSelectedMonth`.
  - Add `DaysWorkedInSelectedMonth` property for the UI.

### User Interface

#### [MODIFY] `MainWindow.xaml`
- **Task Section**:
  - Remove the "Pause" `<Button>`.
  - Add a `<TextBlock>` below the timer to display `Estimated Finish: {Binding WorkdayFinishTimeFormatted}`.
  - Add a `<ComboBox>` for the Workday Finish Sound, similar to the Pomodoro sound selector.
- **Right Section (formerly Calendar)**:
  - Remove `<Calendar>` and calendar-related resources.
  - Add two `<ComboBox>` controls bound to `AvailableYears`/`SelectedYear` and `AvailableMonths`/`SelectedMonth`.
  - Add a `<TextBlock>` displaying "Days worked this month: {Binding DaysWorkedInSelectedMonth}".

#### [DELETE] `DateToHighlightConverter.cs` (or remove its usage)
- Remove the `DateToHighlightConverter` class as the calendar is being removed.

## Verification Plan

### Automated Tests
- None requested.

### Manual Verification
- **Auto-start**: Open the app with existing time in `tasks.txt` and verify the timer is running.
- **Estimated Finish Time**: Verify the displayed estimated finish time is `Current Time + (8 hours - Recorded Time)`.
- **Workday Sound**: Manually set the timer close to 8 hours and verify the selected sound plays when it hits 8 hours.
- **Month Selector**: Select a month from the dropdown and verify the correct count of days worked is displayed based on `tasks.txt`.
