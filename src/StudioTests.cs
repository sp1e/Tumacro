using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Threading;
sealed class InputTraceBox : TextBox {
    public readonly List<string> Messages=new List<string>();
    protected override void WndProc(ref Message message) {
        int kind=message.Msg;
        if(kind==0x102||kind==0x302) Messages.Add("MSG:"+kind.ToString("X")+":"+message.WParam+":open="+Native.GetOpenClipboardWindow()+":mods="+Control.ModifierKeys);
        base.WndProc(ref message);
        if(kind==0x302) Messages.Add("PASTE-RESULT:length="+TextLength);
    }
    protected override void OnKeyDown(KeyEventArgs e) { base.OnKeyDown(e); Messages.Add("DOWN:"+e.KeyCode+":handled="+e.Handled+":suppress="+e.SuppressKeyPress); }
    protected override bool ProcessCmdKey(ref Message msg,Keys keyData) { bool result=base.ProcessCmdKey(ref msg,keyData); Messages.Add("CMD:"+keyData+":"+result); return result; }
}
sealed class ClipboardHold : IDisposable {
    readonly ManualResetEvent ready=new ManualResetEvent(false),release=new ManualResetEvent(false);
    readonly Thread worker; readonly Form window=new Form(); bool acquired,disposed;
    public ClipboardHold() {
        IntPtr owner=window.Handle;
        worker=new Thread(delegate() {
            var until=DateTime.UtcNow.AddSeconds(1);
            while(!(acquired=Native.OpenClipboard(owner))&&DateTime.UtcNow<until) if(release.WaitOne(10)) return;
            ready.Set(); if(!acquired) return;
            try { release.WaitOne(); } finally { Native.CloseClipboard(); }
        }); worker.IsBackground=true; worker.Start();
        if(!ready.WaitOne(1200)||!acquired) { Dispose(); throw new Exception("Could not acquire controlled clipboard lock"); }
        if(Native.ClipboardAvailable()) { Dispose(); throw new Exception("Controlled clipboard lock was not exclusive"); }
    }
    public void Dispose() {
        if(disposed) return; disposed=true; release.Set();
        var until=DateTime.UtcNow.AddMilliseconds(1500); while(worker.IsAlive&&DateTime.UtcNow<until) { Application.DoEvents(); Thread.Sleep(5); }
        if(worker.IsAlive) throw new Exception("Clipboard lock worker did not stop");
        window.Dispose(); ready.Dispose(); release.Dispose();
    }
}
static partial class Native {
    [System.Runtime.InteropServices.DllImport("user32.dll")] public static extern IntPtr GetOpenClipboardWindow();
    [System.Runtime.InteropServices.DllImport("user32.dll")] public static extern bool DestroyIcon(IntPtr icon);
    [System.Runtime.InteropServices.DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr window,IntPtr context,uint flags);
    public static void TestKey(Keys key,bool up) { var input=new[]{Key((int)key,up)}; SendInput(1,input,System.Runtime.InteropServices.Marshal.SizeOf(typeof(Input))); }
}
sealed partial class MacroForm {
    public bool ConfigureWheelForTest(string value) { Chord chord; if(!Chord.Parse(value,out chord)) return false; settings.Wheel=chord.Text; wheelEnabled=true; return true; }
    public bool ConfigureHotkeyForTest(string trigger,string output) { settings.Macros.Add(new KeyMacro {Name="Test",Trigger=trigger,Action=output}); return RefreshHotkeys(); }
    public void SelectForTest(int value) { selected=value; BuildStudioView(); }
    void KeyClickForTest(string name,Action<int> pump) { FocusStudioControl(name); Native.FocusTestWindow(Handle); var control=FindStudioControl(name); if(control==null||!control.Focused) throw new Exception("Keyboard focus missing: "+name); Native.SendChord(new Chord {Key=Keys.Space,Text="Mellanslag"}); pump(180); }
    void RecordForTest(ShortcutBox recorder,Chord chord,Action<int> pump) {
        Native.FocusTestWindow(Handle); recorder.StartRecording();
        if(!recorder.Recording) throw new Exception("Explicit recording did not start: "+chord.Text);
        Native.SendChord(chord); pump(160);
        if(recorder.Recording) throw new Exception("Recording did not finish after key release: "+chord.Text);
    }
    public void ExerciseCrudForTest(Action<bool,string> assert,Action<int> pump) {
        KeyClickForTest("add-macro",pump); assert(macroName.Focused,"New macro receives keyboard focus"); macroName.Text="Kommandopalett";
        Chord trigger,action; Chord.Parse("Ctrl + Alt + F9",out trigger); Chord.Parse("Ctrl + Shift + P",out action);
        RecordForTest(macroTrigger,trigger,pump); RecordForTest(macroAction,action,pump);
        assert(macroTrigger.Text==trigger.Text&&macroAction.Text==action.Text,"Keyboard shortcut fields capture native combinations");
        KeyClickForTest("save-macro",pump); assert(settings.Macros.Count==1&&FindStudioControl("macro-0").Focused,"Keyboard save persists macro and focuses saved row");
        KeyClickForTest("macro-0",pump); int before=TestChordDispatchCount; RecordForTest(macroTrigger,trigger,pump);
        assert(macroTrigger.Text==trigger.Text&&TestChordDispatchCount==before,"Existing registered trigger is recorded without dispatch");
        RecordForTest(macroAction,trigger,pump); assert(macroAction.Text==trigger.Text&&TestChordDispatchCount==before,"Output recorder also blocks registered macro dispatch");
        KeyClickForTest("cancel-macro",pump); assert(FindStudioControl("macro-0").Focused,"Cancel returns focus to original macro row");
        CaptureStudioDraft(); selected=2; BuildStudioView(); wheelChoice.SelectedIndex=1; RecordForTest(wheelAction,trigger,pump);
        assert(wheelAction.Text==trigger.Text&&TestChordDispatchCount==before,"Wheel recorder blocks registered macro dispatch");
        wheelChoice.SelectedIndex=2; KeyClickForTest("save-wheel",pump); assert(settings.Wheel==action.Text&&wheelChoice.Focused,"Assign saved macro to wheel and retain selector focus");
        var target=new TextBox {Bounds=new Rectangle(250,70,450,36)}; Controls.Add(target); target.BringToFront(); bool received=false; target.KeyDown+=delegate(object sender,KeyEventArgs e) { if(e.Control&&e.Shift&&e.KeyCode==Keys.P) {received=true;e.SuppressKeyPress=true;} }; target.Focus(); Native.FocusTestWindow(Handle); Native.SendChord(trigger); pump(500);
        assert(received&&TestChordDispatchCount==before+1,"Normal global activation works after recorder leaves focus"); target.Dispose();
        TogglePause(); assert(hotkeys.Count==0&&!active,"Pause unregisters keyboard macros"); TogglePause(); assert(hotkeys.Count==1&&active,"Resume restores keyboard macro registration");
        FocusStudioControl("macro-0"); KeyClickForTest("macro-0",pump); macroName.Text="Osparat namn"; KeyClickForTest("cancel-macro",pump); assert(settings.Macros[0].Name=="Kommandopalett"&&FindStudioControl("macro-0").Focused,"Keyboard cancel preserves saved values and focus");
        KeyClickForTest("macro-0",pump); KeyClickForTest("delete-macro",pump); assert(settings.Macros.Count==0&&FindStudioControl("add-macro").Focused,"Keyboard delete returns focus to add control"); assert(wheelEnabled&&settings.Wheel==action.Text,"Deleting source macro preserves copied wheel assignment");
        KeyClickForTest("reset-wheel",pump); assert(!wheelEnabled&&wheelChoice.Focused,"Wheel reset restores middle click and selector focus");
        Size=MinimumSize; Application.DoEvents(); assert(sidebar.Controls["all-functions"].Bottom<=sidebar.ClientSize.Height&&footerPanel.Bottom<=ClientSize.Height,"Minimum window keeps sidebar action and footer reachable");
    }
    public void ExerciseRecorderForTest(Action<bool,string> assert,Action<int> pump) {
        selected=2; BuildStudioView(); OpenMacro(-1); macroName.Text="Recorder regression";
        assert(wheelAction==null&&wheelChoice==null&&macroTrigger!=null&&macroAction!=null,"Macro editor is separate from wheel settings");
        macroAction.FocusRecordButton(); Native.SendChord(new Chord {Key=Keys.Space}); pump(80);
        assert(macroAction.Recording&&!FindStudioControl("save-macro").Enabled,"Keyboard activation starts recording and disables Save until completion");
        Native.TestKey(Keys.ControlKey,false); Native.TestKey(Keys.K,false); pump(60); Native.TestKey(Keys.K,true); pump(60);
        assert(macroAction.Recording,"Recording waits for the modifier to be released"); Native.TestKey(Keys.ControlKey,true); pump(100);
        assert(!macroAction.Recording&&macroAction.Text=="Ctrl + K"&&FindStudioControl("save-macro").Enabled,"Releasing the final modifier completes recording and enables Save");
        string preserved=macroAction.Text; macroAction.StartRecording();
        Native.TestKey(Keys.ControlKey,false); Native.TestKey(Keys.K,false); Native.TestKey(Keys.C,false); Native.TestKey(Keys.C,true); Native.TestKey(Keys.K,true); Native.TestKey(Keys.ControlKey,true); pump(100);
        assert(!macroAction.Recording&&macroAction.Text==preserved,"A multi-key sequence does not silently save a partial chord");
        Size=MinimumSize; pump(80);
        var save=FindStudioControl("save-macro");
        Rectangle viewport=contentScroll.RectangleToScreen(contentScroll.ClientRectangle);
        assert(viewport.Contains(save.RectangleToScreen(save.ClientRectangle))&&viewport.Contains(macroTrigger.RectangleToScreen(macroTrigger.ClientRectangle))&&viewport.Contains(macroAction.RectangleToScreen(macroAction.ClientRectangle)),"Both recorder fields and Save fit in the minimum window");
        int before=TestChordDispatchCount;
        foreach(string shortcut in new[]{"Ctrl + Tab","Tab","Shift + Tab","Back","Delete","Enter","F5","Alt + Tab","Win + E","Ctrl + C","Ctrl + Shift + P","Ctrl + Alt + F9"}) {
            Chord chord; Chord.Parse(shortcut,out chord); IntPtr foreground=Handle;
            RecordForTest(macroAction,chord,pump);
            assert(macroAction.Text==chord.Text&&Native.GetForegroundWindow()==foreground&&TestChordDispatchCount==before,"Explicit recording captures and suppresses "+shortcut);
        }
        string previous=macroAction.Text; macroAction.StartRecording(); Native.SendChord(new Chord {Key=Keys.Escape}); pump(160);
        assert(!macroAction.Recording&&macroAction.Text==previous,"Escape cancels without overwriting the previous shortcut");
        Chord trigger,action; Chord.Parse("Ctrl + Alt + F9",out trigger); Chord.Parse("Ctrl + Shift + P",out action);
        RecordForTest(macroTrigger,trigger,pump); RecordForTest(macroAction,action,pump); KeyClickForTest("save-macro",pump);
        KeyClickForTest("macro-0",pump); macroAction.StartRecording();
        assert(hotkeys.Count==0&&recordingShortcut,"Recording suspends all registered macro triggers");
        Native.SendChord(trigger); pump(180);
        assert(macroAction.Text==trigger.Text&&hotkeys.Count==1&&TestChordDispatchCount==before,"Recording an existing trigger restores registration without executing it");
        macroTrigger.StartRecording(); macroAction.StartRecording();
        assert(!macroTrigger.Recording&&macroAction.Recording&&recordingShortcut,"Only one recorder can listen at a time");
        macroName.Focus(); pump(80);
        assert(!macroAction.Recording&&!recordingShortcut&&hotkeys.Count==1,"Moving focus to the name field cancels recording");
        previous=macroAction.Text; macroAction.StartRecording();
        using(var other=new Form {TopMost=true,Size=new Size(250,150)}) {
            other.Show(); Native.FocusTestWindow(other.Handle); pump(150);
            assert(!macroAction.Recording&&!recordingShortcut&&macroAction.Text==previous&&hotkeys.Count==1,"Changing windows cancels capture and restores global shortcuts"); other.Close();
        }
        Native.FocusTestWindow(Handle); KeyClickForTest("cancel-macro",pump);
        selected=2; BuildStudioView(); wheelChoice.SelectedIndex=2; CaptureStudioDraft();
        settings.Language="en"; BuildStudioView();
        assert(wheelChoice.SelectedIndex==2&&wheelAction.Text==action.Text,"Rebuilding the view preserves the selected saved wheel macro");
        RecordForTest(wheelAction,new Chord {Key=Keys.Tab,Text="Tab"},pump);
        assert(wheelChoice.SelectedIndex==1,"Recording a different wheel shortcut switches to a custom assignment");
        KeyClickForTest("save-wheel",pump); assert(settings.Wheel=="Tab"&&settings.WheelName=="","Saving a changed wheel recording clears the old macro name");
        using(var holder=new Form()) {
            bool registered=Native.RegisterHotKey(holder.Handle,909,3|0x4000,(uint)Keys.F10);
            assert(registered,"Controlled conflicting shortcut registered by another window");
            try {
                OpenMacro(-1); macroName.Text="Conflict"; macroTrigger.Text="Ctrl + Alt + F10"; macroAction.Text="Ctrl + P"; SaveMacro();
                assert(settings.Macros.Count==1&&editing==-1&&hotkeys.Count==1,"A registration conflict retains the draft and restores existing macros");
                KeyClickForTest("cancel-macro",pump);
            } finally { Native.UnregisterHotKey(holder.Handle,909); }
        }
        selected=2; BuildStudioView();
        wheelAction.StartRecording(); wheelAction.Dispose();
        assert(!recordingShortcut&&hotkeys.Count==1,"Disposing an active recorder removes its hook and restores shortcuts");
        BuildStudioView(); settings.Language="sv"; wheelAction.StartRecording();
        assert(wheelAction.Recording,"Application can close with an active recording");
    }
}
static partial class Program {
    static int StudioTest(string file) {
        var report=new List<string>();
        string folder=Path.Combine(Path.GetDirectoryName(Path.GetFullPath(file)),"config-test"); Directory.CreateDirectory(folder);
        try {
            Chord chord;
            Assert(Chord.Parse("Ctrl + Alt + 1",out chord)&&chord.Text=="Ctrl + Alt + 1","Shortcut normalization and numeric key",report);
            Assert(!Chord.Parse("Ctrl + Ctrl + P",out chord)&&!Chord.Parse("Ctrl + Garbage",out chord)&&!Chord.Parse("Ctrl + P + C",out chord),"Malformed shortcuts rejected",report);
            var settings=new StudioSettings {Swap=true,Theme="dark",Language="en",Wheel="Ctrl + Shift + P"}; settings.Macros.Add(new KeyMacro {Name="Kommandopalett",Trigger="Ctrl + Alt + 1",Action="Ctrl + Shift + P"}); string config=Path.Combine(folder,"settings.xml"); settings.Write(config); settings.Theme="light"; settings.Write(config); var read=StudioSettings.Read(config);
            Assert(read.Language=="en"&&read.Theme=="light"&&read.Swap&&read.Macros[0].Name=="Kommandopalett"&&read.Wheel=="Ctrl + Shift + P","Settings persist wheel, macros, theme and swap",report);
            Assert(StudioSettings.Read(config+".bak").Theme=="dark","Atomic settings replacement retains backup",report);
            string migrationRoot=Path.Combine(folder,"rename-"+Guid.NewGuid().ToString("N"));
            string legacyDirectory=Path.Combine(migrationRoot,"TumMakro"), currentDirectory=Path.Combine(migrationRoot,"Tumacro");
            Directory.CreateDirectory(legacyDirectory); read.Write(Path.Combine(legacyDirectory,"settings.xml")); read.Write(Path.Combine(legacyDirectory,"settings.xml")); File.WriteAllText(Path.Combine(legacyDirectory,"settings.txt"),"swap");
            Assert(StudioSettings.CopyLegacySettings(currentDirectory,legacyDirectory) && StudioSettings.Read(Path.Combine(currentDirectory,"settings.xml")).Macros[0].Name=="Kommandopalett" && File.Exists(Path.Combine(currentDirectory,"settings.xml.bak")) && File.ReadAllText(Path.Combine(currentDirectory,"settings.txt"))=="swap","Rename migrates legacy macros, preferences and backup",report);
            var migrated=StudioSettings.Read(Path.Combine(currentDirectory,"settings.xml")); migrated.Theme="dark"; migrated.Write(Path.Combine(currentDirectory,"settings.xml"));
            Assert(!StudioSettings.CopyLegacySettings(currentDirectory,legacyDirectory) && StudioSettings.Read(Path.Combine(currentDirectory,"settings.xml")).Theme=="dark" && StudioSettings.Read(Path.Combine(legacyDirectory,"settings.xml")).Theme=="light","Migration preserves existing Tumacro settings and leaves legacy files intact",report);
            using(var form=new MacroForm(true)) {
                var box=new TextBox {Multiline=true,Bounds=new Rectangle(24,66,600,50),Text="Studio integration"}; form.Controls.Add(box); box.BringToFront(); form.TopMost=true; form.Show(); Pump(100);
                Assert(form.HookInstalled,"Studio installs native mouse hook",report);
                bool received=false; box.KeyDown+=delegate(object sender,KeyEventArgs e) { if(e.Control&&e.Shift&&e.KeyCode==Keys.P) { received=true; e.SuppressKeyPress=true; } };
                Assert(form.ConfigureWheelForTest("Ctrl + Shift + P"),"Wheel accepts a keyboard shortcut",report);
                WaitFor(delegate { Native.FocusTestWindow(form.Handle); box.Focus(); return Native.GetForegroundWindow()==form.Handle&&box.Focused&&!Native.Modifiers(); }); Native.TestMiddle(); WaitFor(delegate { return received; }); Assert(received,"Native middle-button hook sends configured shortcut",report);
                received=false; form.TogglePause(); Native.TestMiddle(); Pump(200); Assert(!received,"Paused wheel passes through without macro",report); form.TogglePause();
                Assert(form.ConfigureHotkeyForTest("Ctrl + Alt + F9","Ctrl + Shift + P"),"Global keyboard macro registered",report);
                Chord trigger; Chord.Parse("Ctrl + Alt + F9",out trigger); Native.FocusTestWindow(form.Handle); box.Focus(); Native.SendChord(trigger); WaitFor(delegate { return received; }); Assert(received,"Registered keyboard macro sends action after key release",report);
                form.Quit();
            }
            using(var form=new MacroForm(true)) { form.SelectForTest(2); Assert(!form.IsDisposed,"Wheel configuration view builds",report); form.SelectForTest(0); form.SelectForTest(1); Assert(!form.IsDisposed,"Both thumb views build",report); }
            using(var form=new MacroForm(true)) { form.TopMost=true; form.Show(); Pump(100); Native.FocusTestWindow(form.Handle); form.ExerciseLanguageForTest(delegate(bool value,string name) { Assert(value,name,report); },Pump); form.Quit(); }
            using(var form=new MacroForm(true)) { form.TopMost=true; form.Show(); Pump(100); Native.FocusTestWindow(form.Handle); form.ExerciseCrudForTest(delegate(bool value,string name) { Assert(value,name,report); },Pump); form.Quit(); }
            using(var form=new MacroForm(true)) { form.TopMost=true; form.Show(); Pump(100); Native.FocusTestWindow(form.Handle); form.ExerciseRecorderForTest(delegate(bool value,string name) { Assert(value,name,report); },Pump); form.Quit(); }
            File.WriteAllLines(file,report.ToArray()); return 0;
        } catch(Exception error) { report.Add("FAIL: "+error); File.WriteAllLines(file,report.ToArray()); return 1; }
    }
}
