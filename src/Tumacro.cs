using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Threading;
using System.Collections.Generic;

static partial class Native {
    public delegate IntPtr HookProc(int code, IntPtr message, IntPtr data);
    [StructLayout(LayoutKind.Sequential)] public struct Point { public int x,y; }
    [StructLayout(LayoutKind.Sequential)] public struct Mouse { public Point pt; public uint mouseData, flags, time; public UIntPtr extra; }
    [StructLayout(LayoutKind.Sequential)] public struct Keyboard { public ushort key, scan; public uint flags,time; public UIntPtr extra; }
    [StructLayout(LayoutKind.Sequential)] public struct MouseInput { public int x,y; public uint data,flags,time; public UIntPtr extra; }
    [StructLayout(LayoutKind.Explicit)] public struct Union { [FieldOffset(0)] public Keyboard keyboard; [FieldOffset(0)] public MouseInput mouse; }
    [StructLayout(LayoutKind.Sequential)] public struct Input { public uint type; public Union value; }
    [DllImport("user32.dll", SetLastError=true)] public static extern IntPtr SetWindowsHookEx(int id, HookProc proc, IntPtr module, uint thread);
    [DllImport("user32.dll")] public static extern bool UnhookWindowsHookEx(IntPtr hook);
    [DllImport("user32.dll")] public static extern IntPtr CallNextHookEx(IntPtr hook,int code,IntPtr message,IntPtr data);
    [DllImport("kernel32.dll",CharSet=CharSet.Auto)] public static extern IntPtr GetModuleHandle(string name);
    [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll",CharSet=CharSet.Unicode)] public static extern int GetClassName(IntPtr hwnd,StringBuilder name,int count);
    [DllImport("user32.dll")] public static extern short GetAsyncKeyState(int key);
    [DllImport("user32.dll")] public static extern uint GetClipboardSequenceNumber();
    [DllImport("user32.dll",SetLastError=true)] public static extern uint SendInput(uint count,Input[] input,int size);
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hwnd);
    [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hwnd,int command);
    [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr hwnd,out uint process);
    [DllImport("kernel32.dll")] public static extern uint GetCurrentThreadId();
    [DllImport("user32.dll")] public static extern bool AttachThreadInput(uint first,uint second,bool attach);
    public static void FocusTestWindow(IntPtr hwnd) {
        uint pid; uint foregroundThread=GetWindowThreadProcessId(GetForegroundWindow(),out pid), current=GetCurrentThreadId();
        bool attached=foregroundThread!=0 && foregroundThread!=current && AttachThreadInput(current,foregroundThread,true);
        try { ShowWindow(hwnd,5); SetForegroundWindow(hwnd); } finally { if(attached) AttachThreadInput(current,foregroundThread,false); }
    }
    public static bool Modifiers() { foreach(int k in new int[]{0x10,0x11,0x12,0x5b,0x5c}) if((GetAsyncKeyState(k)&0x8000)!=0) return true; return false; }
    static Input Key(int key,bool up) { return new Input { type=1, value=new Union { keyboard=new Keyboard { key=(ushort)key,flags=up?2u:0u } } }; }
    public static bool Shortcut(int key,bool shift) {
        var input=new List<Input>(); input.Add(Key(0x11,false)); if(shift) input.Add(Key(0x10,false));
        input.Add(Key(key,false)); input.Add(Key(key,true)); if(shift) input.Add(Key(0x10,true)); input.Add(Key(0x11,true));
        if(SendInput((uint)input.Count,input.ToArray(),Marshal.SizeOf(typeof(Input)))==(uint)input.Count) return true;
        // Release our modifier keys if Windows accepted only part of the sequence.
        var release=new Input[]{Key(key,true),Key(0x10,true),Key(0x11,true)};
        SendInput((uint)release.Length,release,Marshal.SizeOf(typeof(Input))); return false;
    }
    public static void TestButton(int button) {
        Input[] a=new Input[2];
        a[0]=new Input { type=0,value=new Union { mouse=new MouseInput { data=(uint)button,flags=0x80 } } };
        a[1]=new Input { type=0,value=new Union { mouse=new MouseInput { data=(uint)button,flags=0x100 } } };
        if(SendInput(2,a,Marshal.SizeOf(typeof(Input)))!=2) throw new Exception("Test input failed");
    }
}

sealed partial class MacroForm : Form {
    readonly Label status=new Label(), lastButton=new Label();
    readonly Label activity=new Label();
    readonly ToolTip tips=new ToolTip();
    readonly Button pause=new Button(); readonly CheckBox swap=new CheckBox();
    readonly NotifyIcon tray=new NotifyIcon(); readonly System.Windows.Forms.Timer copyTimer=new System.Windows.Forms.Timer();
    Native.HookProc callback; IntPtr hook; volatile bool active=true; bool busy, pathReady, copyReady, quitting, capturePath;
    Thread hookThread; ApplicationContext hookContext;
    bool hooksStarted, backgroundMode;
    readonly bool testing; uint oldSequence; int attempts; IntPtr target; string savedPath;
    uint observedSequence; int stableTicks;
    readonly bool[] swallowed=new bool[4]; readonly bool[] held=new bool[4];
    string config=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Tumacro","settings.txt");
    public string TestError; public bool PathReady { get { return pathReady; } } public bool CopyReady { get { return copyReady; } }
    public bool Busy { get { return busy; } } public bool Active { get { return active; } }
    public string Status { get { return status.Text; } }
    public string SavedPathForTest { get { return savedPath; } }
    public MacroForm(bool test) { testing=test; InitStudio(); }
    void StartHooks() {
            if(hooksStarted) return;
            hooksStarted=true;
            callback=MouseHook; var ready=new ManualResetEvent(false);
            hookThread=new Thread(delegate() {
                hookContext=new ApplicationContext(); hook=Native.SetWindowsHookEx(14,callback,Native.GetModuleHandle(null),0);
                ready.Set(); if(hook!=IntPtr.Zero) Application.Run(hookContext);
            }); hookThread.IsBackground=true; hookThread.Start();
            ready.WaitOne(2000);
            if(hook==IntPtr.Zero) { active=false; status.Text=L("Kunde inte ansluta tumknapparna. Starta om programmet."); TestError="Hook installation failed"; } UpdateStates();
    }
    public void StartBackground() { backgroundMode=true; IntPtr window=Handle; StartHooks(); }
    public bool HookInstalled { get { return hook!=IntPtr.Zero; } }
    public bool TrayVisible { get { return tray.Visible; } }
    string StartupInfo() {
        try { using(var key=Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run")) {
            string command=key==null?null:(key.GetValue("Tumacro")??key.GetValue("TumMakro")) as string;
            return string.IsNullOrEmpty(command)?L("Start med Windows: inte inställd."):L("Start med Windows: inställd. Programmet öppnas vid klockan.");
        } } catch { return L("Start med Windows: kontrollera under Startappar i Windows."); }
    }
    public void TogglePause() { active=!active; Reset(); StopHotkeysForPause(); status.Text=active?L("Aktiv. Nästa tryck på båda tumknapparna kopierar."):L("Pausad. Musknappar och tangentgenvägar fungerar som vanligt."); UpdateStates(); }
    public void Reset() { copyTimer.Stop(); busy=false; pathReady=false; copyReady=false; savedPath=null; status.Text=L("Båda knapparna börjar om: nästa tryck kopierar. Windows-urklippet finns kvar."); UpdateStates(); }
    void UpdateStates() { RefreshStudio(); }
    IntPtr MouseHook(int code,IntPtr msg,IntPtr data) {
        long message=msg.ToInt64();
        if(code>=0 && (message==0x20b || message==0x20c || message==0x207 || message==0x208)) {
            Native.Mouse m=(Native.Mouse)Marshal.PtrToStructure(data,typeof(Native.Mouse));
            int b=(message==0x207 || message==0x208)?3:(int)((m.mouseData>>16)&0xffff);
            bool down=message==0x20b || message==0x207;
            if(b>=1 && b<=3 && (testing || (m.flags&1)==0)) {
                if(!down) { held[b]=false; if(swallowed[b]) { swallowed[b]=false; return (IntPtr)1; } }
                else if(held[b]) { if(swallowed[b]) return (IntPtr)1; }
                else {
                    held[b]=true; swallowed[b]=active && !recordingShortcut && (b!=3 || wheelEnabled);
                    if(swallowed[b]) { IntPtr foreground=Native.GetForegroundWindow();
                        try { BeginInvoke((Action)delegate { if(!IsDisposed) { if(b==3) ExecuteWheel(foreground); else Execute(b,foreground); } }); } catch(InvalidOperationException) { }
                        return (IntPtr)1;
                    }
                }
            }
        }
        return Native.CallNextHookEx(hook,code,msg,data);
    }
    void RecordButton(int button) {
        lastButton.Text=L("Senaste tryck: tumknapp ")+button+" · "+(swap.Checked?(button==2?L("Sökväg som text"):L("Vanlig kopiera / klistra in")):(button==1?L("Sökväg som text"):L("Vanlig kopiera / klistra in")));
    }
    void Execute(int button,IntPtr foreground) {
        RecordButton(button);
        if(!active||recordingShortcut) return;
        if(busy) { status.Text=L("Kopieringen pågår. Vänta på beskedet Sökvägen är sparad och tryck sedan igen."); return; }
        if(Native.Modifiers()) { status.Text=L("Släpp Ctrl, Shift, Alt och Windows-tangenten och försök igen."); return; }
        if(Native.GetForegroundWindow()!=foreground) { status.Text=L("Fönstret ändrades. Tryck igen i rätt fönster."); return; }
        bool path=swap.Checked?button==2:button==1;
        if((path && pathReady) || (!path && copyReady)) {
            try {
                if(path) Clipboard.SetText(savedPath);
                if(Native.GetForegroundWindow()!=foreground) { status.Text=L("Fönstret ändrades. Tryck igen i rätt fönster."); return; }
                if(!Native.Shortcut(0x56,false)) { status.Text=L("Windows kunde inte skicka Ctrl+V till det här fönstret."); return; }
                if(path) { pathReady=false; savedPath=null; } else copyReady=false;
                status.Text=path?L("Skickade den sparade sökvägen till målet. Nästa tryck på sökvägsknappen kopierar igen."):L("Skickade Ctrl+V till målet. Nästa tryck på Ctrl+C-knappen kopierar igen."); UpdateStates();
            } catch { status.Text=L("Urklipp är upptaget. Försök klistra in igen."); }
            return;
        }
        oldSequence=Native.GetClipboardSequenceNumber(); target=foreground; capturePath=path; attempts=0; busy=true;
        observedSequence=oldSequence; stableTicks=0;
        if(!Native.Shortcut(0x43,false)) { busy=false; status.Text=L("Windows kunde inte skicka Ctrl+C. Välj innehållet i ett program med samma behörighet som Tumacro."); return; }
        if(!path) { busy=false; copyReady=true; status.Text=L("Skickade Ctrl+C. Nästa tryck skickar Ctrl+V."); UpdateStates(); return; }
        status.Text=L("Kopierar markerad sökvägstext eller hämtar kopierade filers sökvägar…"); copyTimer.Start();
    }
    void CheckCopy(object sender,EventArgs e) {
        attempts++;
        uint sequence=Native.GetClipboardSequenceNumber();
        if(sequence!=observedSequence) { observedSequence=sequence; stableTicks=0; } else stableTicks++;
        if(sequence!=oldSequence && attempts>=3 && stableTicks>=2) {
            try {
                if(capturePath) {
                    string clean=PathsFromData(Clipboard.GetDataObject());
                    if(clean==null) { FinishFailure(L("Inget text- eller filinnehåll kopierades. Markera sökvägstext eller en fil som programmet kan kopiera och tryck igen.")); return; }
                    savedPath=clean; pathReady=true;
                } else copyReady=true;
                copyTimer.Stop(); busy=false; status.Text=L("Sökvägen är sparad. Klicka i målet och tryck samma tumknapp igen för att klistra in."); UpdateStates(); return;
            } catch { }
        }
        if(attempts>=30 || Native.GetForegroundWindow()!=target) FinishFailure(L("Ingen kopiering kunde bekräftas. Markera innehållet och försök igen."));
    }
    void FinishFailure(string message) { copyTimer.Stop(); busy=false; status.Text=message; }
    public static string CleanPaths(string text) {
        if(string.IsNullOrWhiteSpace(text)) return null;
        var lines=text.Split(new string[]{"\r\n","\n"},StringSplitOptions.RemoveEmptyEntries); var result=new List<string>();
        foreach(string line in lines) { string p=line.Trim(); if(p.Length>=2 && p[0]=='"' && p[p.Length-1]=='"') p=p.Substring(1,p.Length-2); if(p.Length>0) result.Add(p); }
        return result.Count==0?null:string.Join(Environment.NewLine,result.ToArray());
    }
    public static string PathsFromData(IDataObject data) {
        if(data==null) return null;
        if(data.GetDataPresent(DataFormats.FileDrop)) {
            string[] files=data.GetData(DataFormats.FileDrop) as string[];
            if(files!=null && files.Length>0) return string.Join(Environment.NewLine,files);
        }
        foreach(string format in new string[]{DataFormats.UnicodeText,DataFormats.Text}) {
            if(data.GetDataPresent(format)) { string text=data.GetData(format) as string; if(!string.IsNullOrWhiteSpace(text)) return CleanPaths(text); }
        }
        return null;
    }
    protected override void Dispose(bool disposing) {
        active=false;
        if(hook!=IntPtr.Zero) { Native.UnhookWindowsHookEx(hook); hook=IntPtr.Zero; }
        if(disposing) { if(hookContext!=null) hookContext.ExitThread(); DisposeStudio(); copyTimer.Dispose(); tips.Dispose(); tray.Visible=false; tray.Dispose(); } base.Dispose(disposing);
    }
    public void Quit() { quitting=true; Close(); }
    public void SetSwap(bool value) { swap.Checked=value; }
    public void RenderPreview(string folder) { RenderStudio(folder); }
}

static partial class Program {
    [STAThread] static int Main(string[] args) {
        if(args.Length>0&&args[0].StartsWith("--self-test")) Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
        Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false);
        if(args.Length>1 && args[0]=="--render-studio") { using(MacroForm preview=new MacroForm(true)) preview.RenderStudio(args[1]); return 0; }
        if(args.Length>0 && args[0]=="--self-test-studio") return StudioTest(args.Length>1?args[1]:"studio-test.txt");
        if(args.Length>1 && args[0]=="--render-ui") { using(MacroForm preview=new MacroForm(true)) preview.RenderPreview(args[1]); return 0; }
        if(args.Length>0 && args[0]=="--self-test") return SelfTest(args.Length>1?args[1]:"self-test.txt");
        if(args.Length>0 && args[0]=="--self-test-background") return BackgroundTest(args.Length>1?args[1]:"background-test.txt");
        bool background=Array.IndexOf(args,"--background")>=0;
        // Legacy IPC names prevent duplicate hooks when an older executable is launched.
        bool created; using(Mutex mutex=new Mutex(true,"Local\\TumMakro-v1",out created)) {
            if(!created) { if(!background) { try { using(var signal=EventWaitHandle.OpenExisting("Local\\TumMakro.Studio.Open")) signal.Set(); } catch(WaitHandleCannotBeOpenedException) { MessageBox.Show(UiText.Translate("Tumacro körs redan. Öppna det via ikonen vid klockan.",UiText.StartupLanguage()),"Tumacro"); } } return 0; }
            using(MacroForm form=new MacroForm(false)) {
                if(background) { form.StartBackground(); Application.Run(); } else Application.Run(form);
            } return 0;
        }
    }
    static int BackgroundTest(string file) {
        var report=new List<string>();
        progressFile=file; File.WriteAllText(file,"Starting background test"+Environment.NewLine);
        try {
            using(MacroForm f=new MacroForm(false)) {
                f.StartBackground(); Pump(200);
                Assert(!f.Visible,"Background launch leaves window hidden",report);
                Assert(f.TrayVisible,"Background launch exposes tray icon",report);
                Assert(f.HookInstalled && f.Active,"Background launch installs active mouse hook",report);
                f.Show(); Pump(100); Assert(f.Visible && f.HookInstalled,"Tray window can open after background launch",report);
                f.Hide(); Pump(100); Assert(!f.Visible && f.HookInstalled,"Hiding window preserves active hook",report);
                f.Quit();
            }
            File.WriteAllLines(file,report.ToArray()); return 0;
        } catch(Exception e) { report.Add("FAIL: "+e); File.WriteAllLines(file,report.ToArray()); return 1; }
    }
    static void Pump(int ms) { DateTime end=DateTime.UtcNow.AddMilliseconds(ms); do { Application.DoEvents(); Thread.Sleep(10); } while(DateTime.UtcNow<end); }
    static void WaitFor(Func<bool> condition) { DateTime end=DateTime.UtcNow.AddSeconds(4); while(!condition() && DateTime.UtcNow<end) Pump(40); }
    static string progressFile;
    static void Assert(bool value,string name,List<string> report) { if(!value) throw new Exception(name); report.Add("PASS: "+name); if(progressFile!=null) File.AppendAllText(progressFile,"PASS: "+name+Environment.NewLine); }
    static void PressTestButton(MacroForm form,TextBox box,int button) {
        Pump(150);
        // Injected mouse buttons use the real pointer position. Keep the pointer
        // over the test target instead of activating whichever app is beneath it.
        Cursor.Position=box.PointToScreen(new System.Drawing.Point(box.Width/2,box.Height/2));
        WaitFor(delegate {
            if(Native.GetForegroundWindow()!=form.Handle) Native.FocusTestWindow(form.Handle);
            box.Focus();
            return Native.GetForegroundWindow()==form.Handle && box.Focused && !Native.Modifiers();
        });
        if(Native.GetForegroundWindow()!=form.Handle || !box.Focused || Native.Modifiers()) throw new Exception("Controlled test input could not obtain a focused window with released modifiers");
        Native.TestButton(button);
    }
    static int SelfTest(string file) {
        var report=new List<string>(); DataObject previous=null; System.Drawing.Point previousPointer=Cursor.Position;
        try { IDataObject current=Clipboard.GetDataObject(); if(current!=null) { previous=new DataObject(); foreach(string format in current.GetFormats(false)) { object data=current.GetData(format,false); if(data!=null) previous.SetData(format,false,data); } } } catch { previous=null; }
        try {
            Assert(Marshal.SizeOf(typeof(Native.Input))==(IntPtr.Size==8?40:28),"Windows INPUT layout",report);
            Assert(MacroForm.CleanPaths("\"C:\\Test mapp\\fil.txt\"\r\n\"\\\\server\\share\\fil.txt\"")=="C:\\Test mapp\\fil.txt"+Environment.NewLine+"\\\\server\\share\\fil.txt","Local and network paths, multiple selections, quote removal",report);
            Assert(MacroForm.CleanPaths("../relative/file.txt")=="../relative/file.txt" && MacroForm.CleanPaths("/home/test/my file.txt")=="/home/test/my file.txt" && MacroForm.CleanPaths("https://example.com/file.txt")=="https://example.com/file.txt","Relative, Unix and URL path text is accepted without existence checks",report);
            Assert(MacroForm.CleanPaths("")==null && MacroForm.PathsFromData(null)==null,"Empty copied data is rejected",report);
            DataObject fileData=new DataObject(); fileData.SetData(DataFormats.FileDrop,new string[]{"C:\\test\\a.txt",@"\\server\share\b.txt"});
            fileData.SetData(DataFormats.UnicodeText,"display name only");
            Assert(MacroForm.PathsFromData(fileData)=="C:\\test\\a.txt"+Environment.NewLine+@"\\server\share\b.txt","Copied file paths take priority over file display text",report);
            using(MacroForm f=new MacroForm(true)) {
                f.SetSwap(false); TextBox box=new TextBox { Multiline=true,Text="Tumacro copy test",Bounds=new Rectangle(24,65,530,42) }; f.Controls.Add(box); box.BringToFront();
                bool copyFiles=false; var keyTrace=new List<string>();
                box.KeyDown+=delegate(object sender,KeyEventArgs key) { keyTrace.Add(key.KeyCode+":"+key.Modifiers); };
                box.LostFocus+=delegate { var name=new StringBuilder(256); Native.GetClassName(Native.GetForegroundWindow(),name,name.Capacity); keyTrace.Add("FOCUS-LOST:"+name+":"+(f.ActiveControl==null?"none":f.ActiveControl.GetType().Name+":"+f.ActiveControl.Name)); };
                box.KeyDown+=delegate(object s,KeyEventArgs e) { if(copyFiles && e.Control && e.KeyCode==Keys.C) { var files=new System.Collections.Specialized.StringCollection(); files.Add("C:\\Tumacro test\\sample.txt"); files.Add(@"\\server\share\other.txt"); Clipboard.SetFileDropList(files); e.SuppressKeyPress=true; } };
                f.TopMost=true; f.Show(); Pump(100);
                WaitFor(delegate { Native.FocusTestWindow(f.Handle); box.Focus(); return Native.GetForegroundWindow()==f.Handle && box.Focused; });
                Assert(f.TestError==null,"Global mouse hook installed",report);
                var focusClass=new StringBuilder(256); Native.GetClassName(Native.GetForegroundWindow(),focusClass,focusClass.Capacity);
                Assert(Native.GetForegroundWindow()==f.Handle,"Controlled test window focused; foreground="+Native.GetForegroundWindow()+"; target="+f.Handle+"; class="+focusClass+"; visible="+f.Visible+"; active="+(Form.ActiveForm==f)+"; box="+box.Focused,report);
                box.SelectAll(); PressTestButton(f,box,2);
                WaitFor(delegate { return f.CopyReady && Clipboard.ContainsText() && Clipboard.GetText()=="Tumacro copy test"; });
                Assert(f.CopyReady && Clipboard.GetText()=="Tumacro copy test","Mouse button 2 sends Ctrl+C and arms paste; "+f.Status+"; focused="+box.Focused+"; selection="+box.SelectionLength+"; keys="+string.Join(",",keyTrace.ToArray()),report);
                box.Clear(); PressTestButton(f,box,2); WaitFor(delegate { return box.Text=="Tumacro copy test"; });
                Assert(!f.CopyReady && box.Text=="Tumacro copy test","Second mouse button 2 press sends Ctrl+V and resets; "+f.Status+"; focused="+box.Focused+"; length="+box.TextLength+"; keys="+string.Join(",",keyTrace.ToArray()),report);
                box.Text="/home/test/my file.txt"; box.SelectAll(); PressTestButton(f,box,1); WaitFor(delegate { return f.PathReady; }); Assert(f.PathReady,"Path button copies selected text in an ordinary text window, outside Explorer; "+f.Status+"; focused="+box.Focused+"; selection="+box.SelectionLength+"; busy="+f.Busy+"; keys="+string.Join(",",keyTrace.ToArray()),report);
                Clipboard.SetText("intervening clipboard content"); box.Clear(); PressTestButton(f,box,1); WaitFor(delegate { return box.Text=="/home/test/my file.txt"; });
                Assert(!f.PathReady && box.Text=="/home/test/my file.txt","Path paste restores selected text after unrelated clipboard copying",report);
                copyFiles=true; box.Text="selected file items"; box.SelectAll(); PressTestButton(f,box,1); WaitFor(delegate { return f.PathReady; });
                string capturedFiles="C:\\Tumacro test\\sample.txt"+Environment.NewLine+@"\\server\share\other.txt";
                Assert(f.PathReady && f.SavedPathForTest==capturedFiles,"Path button captures copied file-drop data from a non-Explorer window",report);
                copyFiles=false; box.Clear(); PressTestButton(f,box,1); string expectedFiles="C:\\Tumacro test\\sample.txt"+Environment.NewLine+@"\\server\share\other.txt";
                WaitFor(delegate { return box.Text==expectedFiles; });
                Assert(!f.PathReady && box.Text==expectedFiles,"Multiple copied files paste as path text, including network paths: "+f.Status+"; focused="+box.Focused+"; text length="+box.TextLength+"; clipboard length="+(Clipboard.ContainsText()?Clipboard.GetText().Length:0)+"; keys="+string.Join(",",keyTrace.ToArray()),report);
                f.TogglePause(); box.SelectAll(); PressTestButton(f,box,2); Pump(150);
                Assert(!f.CopyReady && !f.Active,"Paused buttons do not run macros",report);
                f.TogglePause(); f.SetSwap(true); box.Text="swapped copy"; box.SelectAll(); PressTestButton(f,box,1); WaitFor(delegate { return f.CopyReady && Clipboard.ContainsText() && Clipboard.GetText()=="swapped copy"; });
                Assert(f.CopyReady && Clipboard.GetText()=="swapped copy","Swapped button 1 uses copy macro: "+f.Status+"; focused="+box.Focused,report);
                f.Reset(); Assert(!f.CopyReady && !f.PathReady && !f.Busy,"Reset clears independent phases and pending copy",report);
                f.Quit();
            }
            report.Add("Controlled integration tests passed. Physical Razer buttons and File Explorer are not covered by this test.");
            File.WriteAllLines(file,report.ToArray()); return 0;
        } catch(Exception e) { report.Add("FAIL: "+e.Message); File.WriteAllLines(file,report.ToArray()); return 1; }
        finally { Cursor.Position=previousPointer; try { if(previous!=null) Clipboard.SetDataObject(previous,true); } catch { } }
    }
}
