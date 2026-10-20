# Tumacro 1.0.2

A small Windows app for two mouse thumb buttons, the wheel click, and custom keyboard shortcuts. Select button 1, 2, or 3 in the mouse diagram to view or change its function. Choose Svenska or English in the header to switch languages. Tumacro saves your choice.

## Mouse buttons

- **Thumb button 1:** press once to copy selected path text or paths from copied files. Press again to paste the saved text. Tumacro keeps that copy even if you use the Windows clipboard for something else in between.
- **Thumb button 2:** press once to send Ctrl+C, then press again to send Ctrl+V. Pasting uses the current contents of the Windows clipboard.
- **Wheel button 3:** choose a normal middle click, a keyboard shortcut, or a saved keyboard macro. Press the wheel to run the assigned action. Scrolling up and down keeps its usual behavior.

Path copying works outside File Explorer. The source application must be able to copy text or file information to the clipboard. You can swap the two thumb-button functions; each button keeps its own copy/paste state.

## Keyboard macros

Use the add keyboard macro button and enter a name. Click **Record** for the trigger, press one key combination, then release all keys. Repeat for the shortcut to send, then click **Save macro**. The selected combination appears above each Record button. Use **Clear** to remove it, or **Esc** to cancel a recording and keep the previous value. A trigger requires Ctrl, Alt, or Win together with another key. Release the trigger keys so Tumacro can send the assigned shortcut to the active application.

While recording, Tumacro captures Tab, Alt+Tab, and Windows-key combinations without running them. It suspends its registered keyboard macros until recording ends. Moving focus to another field or leaving the window cancels recording. Record one simultaneous key combination per field; timed sequences and Windows secure shortcuts such as Ctrl+Alt+Delete are not supported.

Tumacro checks for duplicate triggers and shortcuts that another application has already registered. A macro cannot send a shortcut that triggers one of your own macros. Assigning a saved macro to the wheel copies its action, so deleting the keyboard macro keeps the wheel assignment intact.

## Running in the background

Run `Tumacro.exe`. Use the hide-to-tray button or close the window to keep the app running in the background. Double-click the tray icon or run the executable again to reopen the existing window. To quit, use the exit command in the tray icon's context menu.

Pausing restores normal mouse-button behavior and releases registered keyboard shortcuts. Reset returns both thumb buttons to the copy step. Choose the Windows system theme, light mode, or dark mode in the theme selector; Tumacro saves your choice.

In Razer Synapse, assign the thumb buttons to Back and Forward. Applications running as administrator may require you to run Tumacro with the same privileges.

## Windows startup

Place `Install-autostart.ps1` beside `Tumacro.exe` and run the script in PowerShell. It installs the app in `%LOCALAPPDATA%\Tumacro` and configures startup at Windows sign-in, hidden in the system tray. Installation uses your Windows account and requires no administrator privileges.

Run the script with `-Uninstall` to remove the startup entry while keeping the application files and settings.

## Requirements and local data

Tumacro requires Windows 10 or 11 x64 with .NET Framework 4.8. It runs without a separate web runtime. The application makes no network requests and collects no telemetry.

Settings are stored in `%LOCALAPPDATA%\Tumacro\settings.xml`, with a backup. Copied path text stays in memory until you paste it or reset the copy state. The executable is unsigned.

## Build from source

Run `Build.ps1` in PowerShell. The source code and Windows manifest are in `src`. The build uses the Windows .NET Framework compiler and produces `Tumacro.exe`.

## Version history

Tumacro continues the application previously named TumMakro Studio. The 1.0.2 patch replaces focus-based shortcut fields with explicit recording controls, separates the keyboard macro editor from wheel settings, and fixes saved wheel-selection state. Final copy/paste integration validation is pending because the Windows test session could not activate its target window. Version 1.0.1 introduced the mouse icon with two highlighted thumb controls. Earlier releases retain their original names and tags. On first launch, Tumacro copies existing settings from `%LOCALAPPDATA%\TumMakro` when no Tumacro settings exist. The installer replaces the old startup entry.
