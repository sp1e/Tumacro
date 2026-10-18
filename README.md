# TumMakro Studio 1.0

A small Windows app for two mouse thumb buttons, the wheel click, and custom keyboard shortcuts. Select button 1, 2, or 3 in the mouse diagram to view or change its function. The application interface is currently in Swedish.

## Mouse buttons

- **Thumb button 1:** press once to copy selected path text or paths from copied files. Press again to paste the saved text. TumMakro keeps that copy even if you use the Windows clipboard for something else in between.
- **Thumb button 2:** press once to send Ctrl+C, then press again to send Ctrl+V. Pasting uses the current contents of the Windows clipboard.
- **Wheel button 3:** choose a normal middle click, a keyboard shortcut, or a saved keyboard macro. Press the wheel to run the assigned action. Scrolling up and down keeps its usual behavior.

Path copying works outside File Explorer. The source application must be able to copy text or file information to the clipboard. You can swap the two thumb-button functions; each button keeps its own copy/paste state.

## Keyboard macros

Use the add keyboard macro button. Enter a name, then press the trigger shortcut and the shortcut to send in their respective fields. Backspace clears a shortcut field. A trigger requires Ctrl, Alt, or Win together with another key. Release the trigger keys so TumMakro can send the assigned shortcut to the active application.

TumMakro checks for duplicate triggers and shortcuts that another application has already registered. A macro cannot send a shortcut that triggers one of your own macros. Assigning a saved macro to the wheel copies its action, so deleting the keyboard macro keeps the wheel assignment intact.

## Running in the background

Run `TumMakro.exe`. Use the hide-to-tray button or close the window to keep the app running in the background. Double-click the tray icon or run the executable again to reopen the existing window. To quit, use the exit command in the tray icon's context menu.

Pausing restores normal mouse-button behavior and releases registered keyboard shortcuts. Reset returns both thumb buttons to the copy step. Choose the Windows system theme, light mode, or dark mode in the theme selector; TumMakro saves your choice.

In Razer Synapse, assign the thumb buttons to Back and Forward. Applications running as administrator may require you to run TumMakro with the same privileges.

## Windows startup

Place `Installera-autostart.ps1` beside `TumMakro.exe` and run the script in PowerShell. It installs the app in `%LOCALAPPDATA%\TumMakro` and configures startup at Windows sign-in, hidden in the system tray. Installation uses your Windows account and requires no administrator privileges.

Run the script with `-Avinstallera` to remove the startup entry while keeping the application files and settings.

## Requirements and local data

TumMakro requires Windows 10 or 11 x64 with .NET Framework 4.8. It runs without a separate web runtime. The application makes no network requests and collects no telemetry.

Settings are stored in `%LOCALAPPDATA%\TumMakro\settings.xml`, with a backup. Copied path text stays in memory until you paste it or reset the copy state. The executable is unsigned.

## Build from source

Run `Bygg.ps1` in PowerShell. The source code and Windows manifest are in `src`. The build uses the Windows .NET Framework compiler and produces `TumMakro.exe`.

## Version history

Studio 1.0 introduces the Studio interface. Its GitHub tag is `studio-v1.0.0`. Earlier releases retain their original tags, `v1.0.0` through `v1.2.0`.
