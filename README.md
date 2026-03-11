# vibe-pishpan
Created with Antigravity, using Gemini free tier, no intervention, just agreed with the plan.  
Here is the prompt:  
  
"Don't write code, just plan the steps for creating the application with the folowing requirements: C#, WPF. It should have in the main window the possibility to start a task, pause it, and stop it. Under or besides the controls that makes this possible, the time passed since the current task was started should be displayed in a label with the format hh:mm:ss. Let's make it simple, a single task can be run in a day, no project based. An overview of tasks should be made available in a sort of calendar look, where for every month, task are evidentiated. Tasks are to be persisted in a text file format, readable, not encrypted and not compressed. In the same window create the controls that are making possible starting and stopping a pomodoro timer. When the pomodoro ends a sound should be played. The duration of the pomodora and the sound to be displayed should be configurable, The sound may be selected from the Windows sounds only, not mp3 or other formats, let's keep the things simple."

And the answer:  
Here is a detailed, step-by-step plan for creating your C# WPF application based on your requirements.

1. Project Setup & Architecture
Technology: C# WPF Application (.NET Core or latest .NET version).
Architecture: Use the MVVM (Model-View-ViewModel) pattern to keep the logic nicely separated from the UI.
Scope: Single-window application keeping all tools immediately accessible without navigating between pages.
2. User Interface Design (Main Window)
The Main Window will be divided into three logical sections using a Grid layout:

Section A: Daily Task Tracker

Display: A large, prominent TextBlock to display the active time passed. It will be bound to a string property formatted strictly as hh:mm:ss.
Controls: Three distinct buttons: Start, Pause, and Stop.
Details: Since it's a single daily task, pressing Start increments today's current tracking session. Stop finalizes the timer and triggers a file save.
Section B: Pomodoro Timer & Settings

Display: A secondary TextBlock dedicated to the remaining Pomodoro time (usually mm:ss).
Controls: Start Pomodoro and Stop/Reset buttons.
Settings Area (Under/Beside controls):
Duration: A TextBox (restricted to numbers) allowing the user to type in their preferred Pomodoro duration in minutes.
Sound Selection: A ComboBox populated with simple Windows sounds. Because external files (like mp3) aren't allowed, the list will include pre-defined paths to standard Windows sounds (e.g., files residing in C:\Windows\Media\ like Alarm01.wav, tada.wav, chord.wav, or simple SystemSounds).
Section C: Calendar Overview

Control: A standard WPF Calendar control.
Visuals: We will customize the CalendarDayButtonStyle (often done via an IValueConverter or XAML triggers). Any day that has a recorded task duration greater than 0 will be visually highlighted (e.g., bolded, colored background, or given a small indicator dot) so you can easily review your historical task activity month-by-month at a glance.
3. Data Persistence Logic
Format: A strictly raw, unencrypted, human-readable text file. Examples could be simple CSV (tasks.csv) or JSON (tasks.json).
Example Structure (CSV): Date, TotalSeconds (e.g., 2024-10-31, 14400).
Implementation: A FileService class will handle reading and writing. When the user stops a task, it appends or updates the line corresponding to "Today". When the app opens, it reads this file to populate the Calendar highlights and load any existing time already spent today.
4. Timer & Audio Logic
Task Logic: A System.Windows.Threading.DispatcherTimer that ticks every 1 second. On every tick, it increments a TimeSpan for the task and updates the UI to perfectly reflect hh:mm:ss.
Pomodoro Logic: A separate DispatcherTimer that ticks every 1 second, counting down from the configured duration.
Audio Alerts: When the Pomodoro timer hits exactly 00:00, it will instantiate a System.Media.SoundPlayer, pass in the .wav file path selected from the ComboBox (e.g., C:\Windows\Media\Alarm01.wav), and call .Play().
5. Execution Steps
Initialize Project: Create the Visual Studio solution and set up folders for Views, ViewModels, and Models.
Design XAML: Arrange the Task, Pomodoro, and Calendar sections into a clean grid layout inside MainWindow.xaml.
Build Persistence: Write the text file reader/writer classes to ensure tasks are saved cleanly to tasks.txt or tasks.csv locally.
Implement Timers: Connect the DispatcherTimers in the ViewModel and hook up the Start/Pause/Stop commands.
Add Audio & Configuration: Bind the ComboBox to available Windows sounds, pass the selected sound to the audio player, and ensure the Pomodoro configurable duration works.
Wire up the Calendar: Write the logic to read the saved text file, extract the dates, and apply the visual highlights on the WPF Calendar.
Let me know if you are happy with this plan or if you would like me to adjust any of the requirements before we start writing the code!