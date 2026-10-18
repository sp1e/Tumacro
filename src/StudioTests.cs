using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
static partial class Native {
    [System.Runtime.InteropServices.DllImport("user32.dll")] public static extern bool DestroyIcon(IntPtr icon);
    [System.Runtime.InteropServices.DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr window,IntPtr context,uint flags);
}
sealed partial class MacroForm {
    public bool ConfigureWheelForTest(string value) { Chord chord; if(!Chord.Parse(value,out chord)) return false; settings.Wheel=chord.Text; wheelEnabled=true; return true; }
    public bool ConfigureHotkeyForTest(string trigger,string output) { settings.Macros.Add(new KeyMacro {Name="Test",Trigger=trigger,Action=output}); return RefreshHotkeys(); }
    public void SelectForTest(int value) { selected=value; BuildStudioView(); }
    void KeyClickForTest(string name,Action<int> pump) { FocusStudioControl(name); Native.FocusTestWindow(Handle); var control=FindStudioControl(name); if(control==null||!control.Focused) throw new Exception("Keyboard focus missing: "+name); Native.SendChord(new Chord {Key=Keys.Space,Text="Mellanslag"}); pump(180); }
    public void ExerciseCrudForTest(Action<bool,string> assert,Action<int> pump) {
        KeyClickForTest("add-macro",pump); assert(macroName.Focused,"New macro receives keyboard focus"); macroName.Text="Kommandopalett";
        Chord trigger,action; Chord.Parse("Ctrl + Alt + F9",out trigger); Chord.Parse("Ctrl + Shift + P",out action);
        macroTrigger.Focus(); Native.SendChord(trigger); pump(180); macroAction.Focus(); Native.SendChord(action); pump(180);
        assert(macroTrigger.Text==trigger.Text&&macroAction.Text==action.Text,"Keyboard shortcut fields capture native combinations");
        KeyClickForTest("save-macro",pump); assert(settings.Macros.Count==1&&FindStudioControl("macro-0").Focused,"Keyboard save persists macro and focuses saved row");
        KeyClickForTest("macro-0",pump); macroTrigger.Focus(); int before=TestChordDispatchCount; Native.SendChord(trigger); pump(250);
        assert(macroTrigger.Text==trigger.Text&&TestChordDispatchCount==before,"Existing registered trigger is recorded without dispatch");
        macroAction.Focus(); Native.SendChord(trigger); pump(180); assert(macroAction.Text==trigger.Text&&TestChordDispatchCount==before,"Output recorder also blocks registered macro dispatch");
        CaptureStudioDraft(); selected=2; BuildStudioView(); wheelChoice.SelectedIndex=1; wheelAction.Focus(); Native.SendChord(trigger); pump(180);
        assert(wheelAction.Text==trigger.Text&&TestChordDispatchCount==before,"Wheel recorder blocks registered macro dispatch");
        wheelChoice.SelectedIndex=2; KeyClickForTest("save-wheel",pump); assert(settings.Wheel==action.Text&&wheelChoice.Focused,"Assign saved macro to wheel and retain selector focus");
        KeyClickForTest("cancel-macro",pump); assert(FindStudioControl("macro-0").Focused,"Cancel returns focus to original macro row");
        var target=new TextBox {Bounds=new Rectangle(250,70,450,36)}; Controls.Add(target); target.BringToFront(); bool received=false; target.KeyDown+=delegate(object sender,KeyEventArgs e) { if(e.Control&&e.Shift&&e.KeyCode==Keys.P) {received=true;e.SuppressKeyPress=true;} }; target.Focus(); Native.FocusTestWindow(Handle); Native.SendChord(trigger); pump(500);
        assert(received&&TestChordDispatchCount==before+1,"Normal global activation works after recorder leaves focus"); target.Dispose();
        TogglePause(); assert(hotkeys.Count==0&&!active,"Pause unregisters keyboard macros"); TogglePause(); assert(hotkeys.Count==1&&active,"Resume restores keyboard macro registration");
        FocusStudioControl("macro-0"); KeyClickForTest("macro-0",pump); macroName.Text="Osparat namn"; KeyClickForTest("cancel-macro",pump); assert(settings.Macros[0].Name=="Kommandopalett"&&FindStudioControl("macro-0").Focused,"Keyboard cancel preserves saved values and focus");
        KeyClickForTest("macro-0",pump); KeyClickForTest("delete-macro",pump); assert(settings.Macros.Count==0&&FindStudioControl("add-macro").Focused,"Keyboard delete returns focus to add control"); assert(wheelEnabled&&settings.Wheel==action.Text,"Deleting source macro preserves copied wheel assignment");
        KeyClickForTest("reset-wheel",pump); assert(!wheelEnabled&&wheelChoice.Focused,"Wheel reset restores middle click and selector focus");
        Size=MinimumSize; Application.DoEvents(); assert(sidebar.Controls["all-functions"].Bottom<=sidebar.ClientSize.Height&&footerPanel.Bottom<=ClientSize.Height,"Minimum window keeps sidebar action and footer reachable");
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
            var settings=new StudioSettings {Swap=true,Theme="dark",Wheel="Ctrl + Shift + P"}; settings.Macros.Add(new KeyMacro {Name="Kommandopalett",Trigger="Ctrl + Alt + 1",Action="Ctrl + Shift + P"}); string config=Path.Combine(folder,"settings.xml"); settings.Write(config); settings.Theme="light"; settings.Write(config); var read=StudioSettings.Read(config);
            Assert(read.Theme=="light"&&read.Swap&&read.Macros[0].Name=="Kommandopalett"&&read.Wheel=="Ctrl + Shift + P","Settings persist wheel, macros, theme and swap",report);
            Assert(StudioSettings.Read(config+".bak").Theme=="dark","Atomic settings replacement retains backup",report);
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
            using(var form=new MacroForm(true)) { form.TopMost=true; form.Show(); Pump(100); Native.FocusTestWindow(form.Handle); form.ExerciseCrudForTest(delegate(bool value,string name) { Assert(value,name,report); },Pump); form.Quit(); }
            File.WriteAllLines(file,report.ToArray()); return 0;
        } catch(Exception error) { report.Add("FAIL: "+error); File.WriteAllLines(file,report.ToArray()); return 1; }
    }
}
