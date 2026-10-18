using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml.Serialization;
using Microsoft.Win32;
using System.Reflection;
using System.Threading;
[assembly: AssemblyTitle("TumMakro Studio")]
[assembly: AssemblyDescription("Musmakron och tangentkombinationer")]
[assembly: AssemblyProduct("TumMakro Studio")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

public class KeyMacro { public string Name=""; public string Trigger=""; public string Action=""; }
public class StudioSettings {
    public int Schema=1; public bool Swap; public string Theme="system"; public string Wheel=""; public string WheelName="";
    public List<KeyMacro> Macros=new List<KeyMacro>();
    public static StudioSettings Read(string file) {
        var serializer=new XmlSerializer(typeof(StudioSettings));
        using(var stream=File.OpenRead(file)) { var value=(StudioSettings)serializer.Deserialize(stream); if(value.Schema!=1 || value.Macros==null || value.Macros.Count>100) throw new InvalidDataException("Inställningsformatet stöds inte."); return value; }
    }
    public void Write(string file) {
        Directory.CreateDirectory(Path.GetDirectoryName(file)); string temp=file+".tmp";
        try {
            using(var stream=new FileStream(temp,FileMode.Create,FileAccess.Write,FileShare.None)) { new XmlSerializer(typeof(StudioSettings)).Serialize(stream,this); stream.Flush(true); }
            if(File.Exists(file)) File.Replace(temp,file,file+".bak"); else File.Move(temp,file);
        } finally { if(File.Exists(temp)) File.Delete(temp); }
    }
}
sealed class Chord {
    public uint Modifiers; public Keys Key; public string Text;
    public static bool Parse(string text,out Chord chord) {
        chord=null; if(string.IsNullOrWhiteSpace(text)) return false;
        uint mods=0; Keys key=Keys.None; var names=new List<string>();
        foreach(string raw in text.Split('+')) {
            string part=raw.Trim(); uint mod=0;
            if(part.Equals("Ctrl",StringComparison.OrdinalIgnoreCase)||part.Equals("Control",StringComparison.OrdinalIgnoreCase)) mod=2;
            else if(part.Equals("Alt",StringComparison.OrdinalIgnoreCase)) mod=1;
            else if(part.Equals("Shift",StringComparison.OrdinalIgnoreCase)) mod=4;
            else if(part.Equals("Win",StringComparison.OrdinalIgnoreCase)) mod=8;
            if(mod!=0) { if((mods&mod)!=0) return false; mods|=mod; continue; }
            Keys parsed;
            if(part.Length==1 && char.IsDigit(part[0])) parsed=Keys.D0+(part[0]-'0');
            else if(part.Equals("Mellanslag",StringComparison.OrdinalIgnoreCase)) parsed=Keys.Space;
            else if(!Enum.TryParse<Keys>(part,true,out parsed)) return false;
            if(key!=Keys.None || parsed==Keys.None || parsed==Keys.ControlKey || parsed==Keys.ShiftKey || parsed==Keys.Menu || parsed==Keys.LWin || parsed==Keys.RWin || (int)parsed>255) return false;
            key=parsed;
        }
        if(key==Keys.None) return false;
        if((mods&2)!=0) names.Add("Ctrl"); if((mods&1)!=0) names.Add("Alt"); if((mods&4)!=0) names.Add("Shift"); if((mods&8)!=0) names.Add("Win");
        names.Add(key>=Keys.D0 && key<=Keys.D9?((int)key-(int)Keys.D0).ToString():key==Keys.Space?"Mellanslag":key.ToString());
        chord=new Chord { Modifiers=mods,Key=key,Text=string.Join(" + ",names.ToArray()) }; return true;
    }
    public bool Same(Chord other) { return other!=null && Modifiers==other.Modifiers && Key==other.Key; }
}
static partial class Native {
    [DllImport("user32.dll",SetLastError=true)] public static extern bool RegisterHotKey(IntPtr hwnd,int id,uint modifiers,uint key);
    [DllImport("user32.dll")] public static extern bool UnregisterHotKey(IntPtr hwnd,int id);
    public static bool SendChord(Chord chord) {
        var inputs=new List<Input>(); var modifiers=new List<int>();
        if((chord.Modifiers&2)!=0) modifiers.Add(0x11); if((chord.Modifiers&1)!=0) modifiers.Add(0x12); if((chord.Modifiers&4)!=0) modifiers.Add(0x10); if((chord.Modifiers&8)!=0) modifiers.Add(0x5b);
        foreach(int modifier in modifiers) inputs.Add(Key(modifier,false)); inputs.Add(Key((int)chord.Key,false)); inputs.Add(Key((int)chord.Key,true));
        for(int i=modifiers.Count-1;i>=0;i--) inputs.Add(Key(modifiers[i],true));
        if(SendInput((uint)inputs.Count,inputs.ToArray(),Marshal.SizeOf(typeof(Input)))==(uint)inputs.Count) return true;
        var release=new List<Input>(); release.Add(Key((int)chord.Key,true)); foreach(int modifier in modifiers) release.Add(Key(modifier,true)); SendInput((uint)release.Count,release.ToArray(),Marshal.SizeOf(typeof(Input))); return false;
    }
    public static void TestMiddle() { Input[] input={new Input {type=0,value=new Union {mouse=new MouseInput {flags=0x20}}},new Input {type=0,value=new Union {mouse=new MouseInput {flags=0x40}}}}; SendInput(2,input,Marshal.SizeOf(typeof(Input))); }
}
sealed class ChoiceBox : Button {
    public readonly List<object> Items=new List<object>(); int index=-1; ContextMenuStrip choices;
    public event EventHandler SelectedIndexChanged;
    public int SelectedIndex { get { return index; } set { if(value<0||value>=Items.Count) throw new ArgumentOutOfRangeException("value"); if(index==value)return; index=value; Text=Items[index].ToString(); Invalidate(); if(SelectedIndexChanged!=null)SelectedIndexChanged(this,EventArgs.Empty); } }
    public ChoiceBox() { Height=32; TextAlign=ContentAlignment.MiddleLeft; Padding=new Padding(8,0,24,0); FlatStyle=FlatStyle.Flat; Cursor=Cursors.Hand; AccessibleDescription="Tryck för att välja. Piltangenter byter val."; }
    protected override void OnClick(EventArgs e) { base.OnClick(e); if(choices!=null)choices.Dispose(); choices=new ContextMenuStrip {Font=Font}; for(int i=0;i<Items.Count;i++) { int value=i; var item=new ToolStripMenuItem(Items[i].ToString()) {Checked=i==index}; item.Click+=delegate { SelectedIndex=value; Focus(); }; choices.Items.Add(item); } choices.Show(this,0,Height); }
    protected override void OnKeyDown(KeyEventArgs e) { if((e.KeyCode==Keys.Down||e.KeyCode==Keys.Up)&&Items.Count>0) { SelectedIndex=(index+(e.KeyCode==Keys.Down?1:Items.Count-1))%Items.Count; e.Handled=true; return; } base.OnKeyDown(e); }
    protected override void OnPaint(PaintEventArgs e) { base.OnPaint(e); using(var pen=new Pen(ForeColor,1.5f)) { int x=Width-16,y=Height/2; e.Graphics.DrawLines(pen,new[]{new Point(x-3,y-2),new Point(x,y+1),new Point(x+3,y-2)}); } }
    protected override void Dispose(bool disposing) { if(disposing&&choices!=null) choices.Dispose(); base.Dispose(disposing); }
}
sealed class ShortcutBox : TextBox {
    public ShortcutBox() { ReadOnly=true; ShortcutsEnabled=false; AccessibleDescription="Tryck tangentkombinationen. Backsteg rensar fältet."; }
    protected override void OnKeyDown(KeyEventArgs e) {
        if(e.KeyCode==Keys.Tab) { base.OnKeyDown(e); return; }
        e.SuppressKeyPress=true;
        if(e.KeyCode==Keys.Back && e.Modifiers==Keys.None) { Clear(); return; }
        if(e.KeyCode==Keys.ControlKey || e.KeyCode==Keys.ShiftKey || e.KeyCode==Keys.Menu || e.KeyCode==Keys.LWin || e.KeyCode==Keys.RWin) return;
        string value=(e.Control?"Ctrl + ":"")+(e.Alt?"Alt + ":"")+(e.Shift?"Shift + ":"")+(((Native.GetAsyncKeyState(0x5b)|Native.GetAsyncKeyState(0x5c))&0x8000)!=0?"Win + ":"")+e.KeyCode;
        Chord chord; if(Chord.Parse(value,out chord)) Text=chord.Text;
    }
}
sealed class MouseDiagram : Panel {
    public readonly Button[] Buttons=new Button[3]; public int Selected=-1; public bool Dark; public Action<int> SelectButton;
    public MouseDiagram() {
        DoubleBuffered=true; Size=new Size(190,220); AccessibleName="Välj musknapp";
        for(int i=0;i<3;i++) { int index=i; var button=new Button {Text=(i+1).ToString(),Size=new Size(46,44),FlatStyle=FlatStyle.Flat,Cursor=Cursors.Hand,AccessibleName=i==2?"Mushjul, knapp 3":"Tumknapp "+(i+1)}; button.Click+=delegate { if(SelectButton!=null) SelectButton(index); }; Buttons[i]=button; Controls.Add(button); }
        Buttons[0].Location=new Point(6,134); Buttons[1].Location=new Point(6,84); Buttons[2].Location=new Point(91,30);
    }
    protected override void OnPaint(PaintEventArgs e) {
        base.OnPaint(e); e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;
        using(var pen=new Pen(Dark?Color.FromArgb(76,89,108):Color.FromArgb(199,209,224),2)) {
            var rect=new Rectangle(30,18,116,184); using(var path=new GraphicsPath()) { path.AddArc(rect.X,rect.Y,100,100,180,90); path.AddArc(rect.Right-100,rect.Y,100,100,270,90); path.AddArc(rect.Right-100,rect.Bottom-100,100,100,0,90); path.AddArc(rect.X,rect.Bottom-100,100,100,90,90); path.CloseFigure(); e.Graphics.DrawPath(pen,path); }
            e.Graphics.DrawLine(pen,88,20,88,78); e.Graphics.DrawRectangle(pen,82,41,11,25);
        }
    }
}
sealed partial class MacroForm {
    StudioSettings settings; readonly List<int> hotkeys=new List<int>(); volatile bool wheelEnabled;
    string settingsFile=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"TumMakro","settings.xml");
    Panel header,footerPanel,sidebar,contentScroll; FlowLayoutPanel sections; MouseDiagram diagram; Label viewTitle,startupLabel; ChoiceBox themes;
    Color page,surface,text,muted,accent,edge,soft; bool dark,refreshing; int selected=-1,editing=int.MinValue;
    TextBox macroName; ShortcutBox macroTrigger,macroAction,wheelAction; ChoiceBox wheelChoice;
    string draftName="",draftTrigger="",draftAction="",draftWheel; int draftWheelChoice=-1;
    readonly System.Windows.Forms.Timer chordTimer=new System.Windows.Forms.Timer(); Chord pendingChord; IntPtr pendingTarget; int chordWait; Keys pendingTrigger;
    EventWaitHandle openSignal; readonly System.Windows.Forms.Timer openTimer=new System.Windows.Forms.Timer();
    readonly List<Label> dynamicStates=new List<Label>(); string loadWarning;
    public int TestChordDispatchCount;
    void InitStudio() {
        settings=new StudioSettings();
        if(!testing) {
            try { if(File.Exists(settingsFile)) settings=StudioSettings.Read(settingsFile); else if(File.Exists(config)) settings.Swap=File.ReadAllText(config).Trim()=="swap"; }
            catch { try { settings=StudioSettings.Read(settingsFile+".bak"); loadWarning="Inställningarna återställdes från säkerhetskopian."; } catch { loadWarning="Inställningarna kunde inte läsas. Spara nya inställningar för att ersätta den skadade filen."; } }
        }
        var validated=new List<KeyMacro>(); var loadedTriggers=new HashSet<string>();
        foreach(var macro in settings.Macros) { Chord input,output; if(macro==null||string.IsNullOrWhiteSpace(macro.Name)||!Chord.Parse(macro.Trigger,out input)||!Chord.Parse(macro.Action,out output)||(input.Modifiers&11)==0||!loadedTriggers.Add(input.Text)) { loadWarning="Ett ogiltigt tangentmakro har stängts av. Kontrollera dina inställningar."; continue; } validated.Add(new KeyMacro {Name=macro.Name,Trigger=input.Text,Action=output.Text}); }
        settings.Macros=validated;
        Chord wheel; if(!string.IsNullOrEmpty(settings.Wheel)&&!Chord.Parse(settings.Wheel,out wheel)) { settings.Wheel=""; loadWarning="Hjulmakrot var ogiltigt och stängdes av."; }
        wheelEnabled=!string.IsNullOrEmpty(settings.Wheel); swap.Checked=settings.Swap;
        Text="TumMakro Studio · 1.0"; Font=new Font("Segoe UI",10); AutoScaleMode=AutoScaleMode.Dpi; StartPosition=FormStartPosition.CenterScreen; MinimumSize=new Size(850,720); ClientSize=new Size(980,Math.Min(800,Screen.PrimaryScreen.WorkingArea.Height-90));
        Icon=MakeIcon();
        header=new Panel {Dock=DockStyle.Top,Height=66,Padding=new Padding(22,12,18,12)}; Controls.Add(header);
        var brand=LabelOf("TumMakro",19,true); brand.Location=new Point(24,15); header.Controls.Add(brand);
        var version=LabelOf("STUDIO 1.0",9,false); version.Location=new Point(220,28); header.Controls.Add(version);
        activity.AutoSize=true; activity.Font=new Font(Font,FontStyle.Bold); activity.Anchor=AnchorStyles.Top|AnchorStyles.Right; header.Controls.Add(activity);
        themes=new ChoiceBox {FlatStyle=FlatStyle.Flat,Width=190,Anchor=AnchorStyles.Top|AnchorStyles.Right,AccessibleName="Tema"}; themes.Items.AddRange(new object[]{"Följ Windows","Ljust läge","Mörkt läge"}); themes.SelectedIndex=settings.Theme=="dark"?2:settings.Theme=="light"?1:0; header.Controls.Add(themes);
        header.Layout+=delegate { themes.Location=new Point(header.Width-214,20); activity.Location=new Point(header.Width-305,24); };
        footerPanel=new Panel {Dock=DockStyle.Bottom,Height=130,Padding=new Padding(20,10,20,10)}; Controls.Add(footerPanel);
        var actions=new FlowLayoutPanel {Dock=DockStyle.Top,Height=49,WrapContents=false}; footerPanel.Controls.Add(actions);
        pause.Text="Pausa"; StyleStudioButton(pause,true); pause.Click+=delegate { TogglePause(); RefreshHotkeys(); pendingChord=null; chordTimer.Stop(); }; actions.Controls.Add(pause);
        AddButton(actions,"Börja om",delegate { Reset(); },false); AddButton(actions,"Dölj till klockan",delegate { Hide(); },false); AddButton(actions,"Hjälp",delegate { StudioHelp(); },false);
        startupLabel=LabelOf("",9,false); startupLabel.AutoSize=false; startupLabel.Width=270; startupLabel.Height=42; startupLabel.TextAlign=ContentAlignment.MiddleRight; actions.Controls.Add(startupLabel);
        status.Dock=DockStyle.Bottom; status.Height=55; status.Padding=new Padding(0,5,0,0); status.Font=new Font("Segoe UI",9); footerPanel.Controls.Add(status);
        var body=new Panel {Dock=DockStyle.Fill}; Controls.Add(body); body.BringToFront();
        sidebar=new Panel {Dock=DockStyle.Left,Width=238,Padding=new Padding(20,23,18,18)}; body.Controls.Add(sidebar);
        var choose=LabelOf("Välj en musknapp",13,true); choose.Location=new Point(19,22); sidebar.Controls.Add(choose);
        var instruction=LabelOf("Klicka på 1, 2 eller hjulet 3\nför att se och ändra funktionen.",9,false); instruction.Location=new Point(20,57); sidebar.Controls.Add(instruction);
        diagram=new MouseDiagram {Location=new Point(20,109)}; diagram.SelectButton=delegate(int button) { CaptureStudioDraft(); selected=button; BuildStudioView(); }; sidebar.Controls.Add(diagram);
        var caption=LabelOf("1 och 2: tumknappar\n3: klick på mushjulet",9,false); caption.Location=new Point(38,347); sidebar.Controls.Add(caption);
        var overview=new Button {Name="all-functions",Text="Visa alla funktioner",Width=192,Height=42,Location=new Point(20,414)}; StyleStudioButton(overview,false); overview.Click+=delegate { CaptureStudioDraft(); selected=-1; BuildStudioView(); }; sidebar.Controls.Add(overview);
        var scroll=new Panel {Dock=DockStyle.Fill,AutoScroll=true,Padding=new Padding(24,20,24,22)}; contentScroll=scroll; body.Controls.Add(scroll); scroll.BringToFront();
        sections=new FlowLayoutPanel {AutoSize=true,FlowDirection=FlowDirection.TopDown,WrapContents=false,Dock=DockStyle.Top,Padding=new Padding(0)}; scroll.Controls.Add(sections);
        scroll.Resize+=delegate { SizeSections(scroll); }; sections.ControlAdded+=delegate { SizeSections(scroll); };
        themes.SelectedIndexChanged+=delegate { settings.Theme=themes.SelectedIndex==2?"dark":themes.SelectedIndex==1?"light":"system"; ApplyTheme(); SaveSettings(); };
        swap.CheckedChanged+=delegate { settings.Swap=swap.Checked; Reset(); SaveSettings(); CaptureStudioDraft(); BuildStudioView(); };
        var menu=new ContextMenuStrip(); menu.Items.Add("Öppna TumMakro",null,delegate { Show(); WindowState=FormWindowState.Normal; Activate(); }); menu.Items.Add("Pausa / aktivera",null,delegate { TogglePause(); RefreshHotkeys(); pendingChord=null; chordTimer.Stop(); }); menu.Items.Add("Börja om med kopiering",null,delegate { Reset(); }); menu.Items.Add(new ToolStripSeparator()); menu.Items.Add("Avsluta",null,delegate { quitting=true; Close(); }); tray.Icon=Icon; tray.ContextMenuStrip=menu; tray.Visible=!testing; tray.DoubleClick+=delegate { Show(); WindowState=FormWindowState.Normal; Activate(); };
        copyTimer.Interval=40; copyTimer.Tick+=CheckCopy;
        chordTimer.Interval=25; chordTimer.Tick+=SendPendingChord;
        FormClosing+=delegate(object sender,FormClosingEventArgs e) { if(!quitting&&!testing&&e.CloseReason==CloseReason.UserClosing) { e.Cancel=true; Hide(); } };
        Shown+=delegate { StartHooks(); RefreshHotkeys(); }; HandleCreated+=delegate { if(!testing) RefreshHotkeys(); };
        FormClosed+=delegate { if(backgroundMode) Application.ExitThread(); };
        SystemEvents.UserPreferenceChanged+=SystemThemeChanged;
        if(!testing) { openSignal=new EventWaitHandle(false,EventResetMode.AutoReset,"Local\\TumMakro.Studio.Open"); openTimer.Interval=350; openTimer.Tick+=delegate { if(openSignal.WaitOne(0)) { Show(); WindowState=FormWindowState.Normal; Activate(); } }; openTimer.Start(); }
        ApplyTheme(); BuildStudioView(); status.Text=loadWarning??"Redo. Markera innehållet och tryck en tumknapp. Nästa tryck på samma knapp klistrar in.";
    }
    Icon MakeIcon() {
        using(var bitmap=new Bitmap(32,32)) using(var graphics=Graphics.FromImage(bitmap)) { graphics.Clear(Color.FromArgb(108,76,182)); using(var pen=new Pen(Color.White,2)) { graphics.DrawEllipse(pen,8,3,16,26); graphics.DrawLine(pen,16,4,16,12); } IntPtr handle=bitmap.GetHicon(); try { using(var borrowed=System.Drawing.Icon.FromHandle(handle)) return (Icon)borrowed.Clone(); } finally { Native.DestroyIcon(handle); } }
    }
    Label LabelOf(string value,float size,bool bold) { return new Label {Text=value,AutoSize=true,Font=new Font("Segoe UI",size,bold?FontStyle.Bold:FontStyle.Regular),Margin=new Padding(0,0,0,10)}; }

    void StyleStudioButton(Button button,bool primary) { button.AutoSize=true; button.MinimumSize=new Size(80,42); button.Padding=new Padding(12,4,12,4); button.Margin=new Padding(0,0,8,8); button.FlatStyle=FlatStyle.Flat; button.Tag=primary?"primary":"button"; button.Cursor=Cursors.Hand; button.UseVisualStyleBackColor=false; }
    Button AddButton(Control parent,string label,Action action,bool primary) { var button=new Button {Text=label}; StyleStudioButton(button,primary); button.Click+=delegate { action(); }; parent.Controls.Add(button); return button; }
    void AddText(string value,float size,bool bold) { var label=LabelOf(value,size,bold); sections.Controls.Add(label); }
    void SizeSections(Panel scroll) { if(sections==null) return; int width=Math.Max(360,scroll.ClientSize.Width-scroll.Padding.Horizontal-SystemInformation.VerticalScrollBarWidth); foreach(Control child in sections.Controls) { if(child is Label || child is Button&&child.AutoSize) child.MaximumSize=new Size(width,0); if(child is Panel || child is TextBox || child is ChoiceBox || child is CheckBox) child.Width=width; } }
    Control FindStudioControl(string name) { foreach(Control control in Descendants(sections)) if(control.Name==name) return control; return null; }
    static IEnumerable<Control> Descendants(Control parent) { foreach(Control control in parent.Controls) { yield return control; foreach(Control nested in Descendants(control)) yield return nested; } }
    void FocusStudioControl(string name) { var control=FindStudioControl(name); if(control!=null) { control.Focus(); contentScroll.ScrollControlIntoView(control); } }
    void PrepareRecorder(ShortcutBox box) { box.Enter+=delegate { pendingChord=null; chordTimer.Stop(); }; }
    ShortcutBox FocusedRecorder() { if(Native.GetForegroundWindow()!=Handle) return null; foreach(var recorder in new[]{macroTrigger,macroAction,wheelAction}) if(recorder!=null&&!recorder.IsDisposed&&recorder.Focused) return recorder; return null; }
    void CaptureStudioDraft() { if(macroName!=null&&!macroName.IsDisposed) { draftName=macroName.Text; draftTrigger=macroTrigger.Text; draftAction=macroAction.Text; } if(wheelAction!=null&&!wheelAction.IsDisposed) { draftWheel=wheelAction.Text; draftWheelChoice=wheelChoice.SelectedIndex; } }
    void ClearSections() { swap.Parent=null; foreach(Control control in new List<Control>(ControlsList(sections))) control.Dispose(); dynamicStates.Clear(); macroName=null; macroTrigger=null; macroAction=null; wheelAction=null; wheelChoice=null; }
    static IEnumerable<Control> ControlsList(Control parent) { foreach(Control c in parent.Controls) yield return c; }
    void BuildStudioView() {
        ClearSections(); viewTitle=LabelOf(selected<0?"Knapparnas funktioner":selected==2?"Mushjulet · knapp 3":"Tumknapp "+(selected+1),16,true); sections.Controls.Add(viewTitle);
        if(selected==2) BuildWheelEditor();
        else {
            for(int type=0;type<2;type++) { int physical=settings.Swap?1-type:type; if(selected>=0&&selected!=physical) continue; var row=new Panel {Height=112,MinimumSize=new Size(0,112),Margin=new Padding(0,10,0,8)};
                var number=LabelOf((physical+1).ToString(),13,true); number.Location=new Point(4,8); row.Controls.Add(number);
                var title=LabelOf(type==0?"Sökväg som text":"Ctrl+C / Ctrl+V",12,true); title.Location=new Point(43,5); row.Controls.Add(title);
                var hint=LabelOf(type==0?"Sparad kopia som text":"Windows-urklippet",9,false); hint.Location=new Point(43,33); row.Controls.Add(hint);
                var state=LabelOf("",13,true); state.Tag="state:"+type; state.Location=new Point(43,60); row.Controls.Add(state); dynamicStates.Add(state);
                var next=LabelOf("på nästa tryck",9,false); next.Location=new Point(43,87); row.Controls.Add(next);
                row.Paint+=delegate(object sender,PaintEventArgs e) { using(var pen=new Pen(edge)) e.Graphics.DrawLine(pen,0,row.Height-1,row.Width,row.Height-1); }; sections.Controls.Add(row);
            }
            if(selected<0) { var wheelRow=AddButton(sections,"3   Mushjulet  ·  "+(wheelEnabled?(settings.WheelName==""?settings.Wheel:settings.WheelName):"Vanligt mittklick")+"   →",delegate { CaptureStudioDraft(); selected=2; BuildStudioView(); },false); wheelRow.AccessibleName="Välj mushjulets makro, knapp 3"; }
        }
        swap.Text="Byt knappordning för tumknapparna"; swap.AutoSize=true; swap.Margin=new Padding(0,12,0,22); sections.Controls.Add(swap);
        AddText("Egna tangentkombinationer",12,true); AddText("Välj en genväg som aktiverar makrot och kombinationen som skickas.",9,false);
        for(int i=0;i<settings.Macros.Count;i++) { int index=i; var macro=settings.Macros[i]; var row=AddButton(sections,macro.Name+"    "+macro.Trigger+"  →  "+macro.Action,delegate { OpenMacro(index); },false); row.Name="macro-"+i; row.AccessibleName="Redigera "+macro.Name; }
        if(editing>=-1 && editing!=int.MinValue) BuildMacroEditor();
        else for(int slot=settings.Macros.Count;slot<Math.Max(3,settings.Macros.Count+1);slot++) AddButton(sections,"+  Lägg till tangentmakro",delegate { OpenMacro(-1); },false).Name="add-macro";
        ApplyColors(this); RefreshStudio();
    }
    void OpenMacro(int index) { CaptureStudioDraft(); editing=index; KeyMacro macro=index<0?new KeyMacro():settings.Macros[index]; draftName=macro.Name; draftTrigger=macro.Trigger; draftAction=macro.Action; BuildStudioView(); macroName.Focus(); }
    void BuildMacroEditor() {
        // Editor state is retained while selecting another physical mouse button.
        AddText(editing<0?"Nytt tangentmakro":"Redigera tangentmakro",11,true);
        AddText("Namn",9,false); macroName=new TextBox {Text=draftName,Width=450,MaxLength=60,AccessibleName="Makrots namn",BorderStyle=BorderStyle.FixedSingle}; sections.Controls.Add(macroName);
        AddText("Aktiveras med · tryck genvägen",9,false); macroTrigger=new ShortcutBox {Text=draftTrigger,Width=450,AccessibleName="Aktiveras med"}; sections.Controls.Add(macroTrigger);
        AddText("Skickar kombinationen · tryck genvägen",9,false); macroAction=new ShortcutBox {Text=draftAction,Width=450,AccessibleName="Skickar tangentkombination"}; sections.Controls.Add(macroAction);
        PrepareRecorder(macroTrigger); PrepareRecorder(macroAction);
        var actions=new FlowLayoutPanel {AutoSize=true,WrapContents=true,Margin=new Padding(0,10,0,16)}; sections.Controls.Add(actions);
        AddButton(actions,"Spara makro",SaveMacro,true).Name="save-macro"; AddButton(actions,"Avbryt",delegate { CaptureStudioDraft(); int previous=editing; editing=int.MinValue; BuildStudioView(); FocusStudioControl(previous>=0?"macro-"+previous:"add-macro"); },false).Name="cancel-macro";
        if(editing>=0) AddButton(actions,"Ta bort",delegate { CaptureStudioDraft(); int previous=editing; var old=settings.Macros[editing]; settings.Macros.RemoveAt(editing); if(!SaveSettings()) { settings.Macros.Insert(editing,old); return; } editing=int.MinValue; RefreshHotkeys(); BuildStudioView(); FocusStudioControl(settings.Macros.Count==0?"add-macro":"macro-"+Math.Min(previous,settings.Macros.Count-1)); status.Text="Tangentmakrot är borttaget. Hjulets sparade tilldelning finns kvar."; },false).Name="delete-macro";
    }
    void BuildWheelEditor() {
        AddText("Välj makro för mushjulet",12,true); AddText("Aktiveras när du trycker ned hjulet. Vanlig rullning påverkas inte.",9,false);
        AddText("Funktion",9,false); wheelChoice=new ChoiceBox {Name="wheel-choice",FlatStyle=FlatStyle.Flat,Width=450,AccessibleName="Mushjulets funktion"}; wheelChoice.Items.Add("Vanligt mittklick"); wheelChoice.Items.Add("Egen tangentkombination"); foreach(var macro in settings.Macros) wheelChoice.Items.Add(macro.Name+" · "+macro.Action);
        int choice=draftWheelChoice>=2?1:draftWheelChoice>=0?draftWheelChoice:wheelEnabled?1:0; wheelChoice.SelectedIndex=choice; sections.Controls.Add(wheelChoice);
        AddText("Skickar tangentkombination · tryck genvägen",9,false); wheelAction=new ShortcutBox {Text=draftWheel??settings.Wheel,Width=450,Enabled=choice!=0,AccessibleName="Hjulmakrots tangentkombination"}; sections.Controls.Add(wheelAction);
        PrepareRecorder(wheelAction);
        wheelChoice.SelectedIndexChanged+=delegate { wheelAction.Enabled=wheelChoice.SelectedIndex!=0; if(wheelChoice.SelectedIndex>=2) wheelAction.Text=settings.Macros[wheelChoice.SelectedIndex-2].Action; };
        var actions=new FlowLayoutPanel {AutoSize=true,WrapContents=true,Margin=new Padding(0,10,0,12)}; sections.Controls.Add(actions);
        AddButton(actions,"Spara hjulmakro",delegate {
            CaptureStudioDraft();
            Chord chord; string action=wheelChoice.SelectedIndex==0?"":wheelAction.Text;
            if(action!=""&&!Chord.Parse(action,out chord) || wheelChoice.SelectedIndex!=0&&action=="") { status.Text="Ange tangentkombinationen som hjulet ska skicka."; wheelAction.Focus(); return; }
            if(action!=""&&OutputConflicts(action,settings.Macros)) { status.Text="Hjulmakrot får inte skicka en genväg som aktiverar ett av dina tangentmakron."; return; }
            string old=settings.Wheel,oldName=settings.WheelName; settings.Wheel=action; settings.WheelName=wheelChoice.SelectedIndex>=2?settings.Macros[wheelChoice.SelectedIndex-2].Name:"Egen tangentkombination";
            if(!SaveSettings()) { settings.Wheel=old; settings.WheelName=oldName; return; }
            wheelEnabled=action!=""; draftWheel=null; draftWheelChoice=-1; BuildStudioView(); FocusStudioControl("wheel-choice"); status.Text=wheelEnabled?"Hjulmakrot är sparat. Tryck på hjulet i målprogrammet för att köra det.":"Vanligt mittklick är återställt.";
        },true).Name="save-wheel";
        AddButton(actions,"Återställ mittklick",delegate { CaptureStudioDraft(); string old=settings.Wheel; settings.Wheel=""; if(!SaveSettings()) { settings.Wheel=old; return; } wheelEnabled=false; draftWheel=null; draftWheelChoice=-1; BuildStudioView(); FocusStudioControl("wheel-choice"); status.Text="Mushjulet använder vanligt mittklick."; },false).Name="reset-wheel";
        AddText("Sparad funktion: "+(wheelEnabled?settings.Wheel:"Vanligt mittklick"),10,true);
    }
    bool OutputConflicts(string output,List<KeyMacro> macros) { Chord target; if(!Chord.Parse(output,out target)) return true; foreach(var macro in macros) { Chord trigger; if(Chord.Parse(macro.Trigger,out trigger)&&trigger.Same(target)) return true; } return false; }
    void SaveMacro() {
        CaptureStudioDraft(); Chord trigger,action;
        if(string.IsNullOrWhiteSpace(draftName)||!Chord.Parse(draftTrigger,out trigger)||!Chord.Parse(draftAction,out action)) { status.Text="Fyll i namn och två giltiga tangentkombinationer."; return; }
        if((trigger.Modifiers&11)==0) { status.Text="Aktiveringen behöver Ctrl, Alt eller Win tillsammans med en tangent."; macroTrigger.Focus(); return; }
        var candidate=new List<KeyMacro>(settings.Macros); var replacement=new KeyMacro {Name=draftName.Trim(),Trigger=trigger.Text,Action=action.Text}; if(editing<0) candidate.Add(replacement); else candidate[editing]=replacement;
        var seen=new HashSet<string>(); foreach(var macro in candidate) { Chord normalized; Chord.Parse(macro.Trigger,out normalized); if(!seen.Add(normalized.Text)) { status.Text="Den aktiveringen används redan av ett tangentmakro."; return; } if(OutputConflicts(macro.Action,candidate)) { status.Text="Ett makro får inte skicka en genväg som aktiverar något av dina makron."; return; } }
        if(wheelEnabled&&OutputConflicts(settings.Wheel,candidate)) { status.Text="Aktiveringen används som utdata av hjulmakrot. Välj en annan genväg."; return; }
        var old=settings.Macros; settings.Macros=candidate;
        if(!RefreshHotkeys()) { settings.Macros=old; RefreshHotkeys(); return; }
        if(!SaveSettings()) { settings.Macros=old; RefreshHotkeys(); return; }
        int savedIndex=editing<0?candidate.Count-1:editing; editing=int.MinValue; BuildStudioView(); FocusStudioControl("macro-"+savedIndex); status.Text="Makrot är sparat: "+replacement.Trigger+" skickar "+replacement.Action+". Släpp genvägens tangenter efter trycket.";
    }
    bool SaveSettings() { if(testing) return true; try { settings.Write(settingsFile); return true; } catch { status.Text="Inställningarna kunde inte sparas. Kontrollera skrivåtkomsten till TumMakros mapp och försök igen."; return false; } }
    bool RefreshHotkeys() {
        if(!IsHandleCreated) return true;
        foreach(int id in hotkeys) Native.UnregisterHotKey(Handle,id); hotkeys.Clear(); if(!active) return true;
        for(int i=0;i<settings.Macros.Count;i++) { Chord chord,output; if(!Chord.Parse(settings.Macros[i].Trigger,out chord)||!Chord.Parse(settings.Macros[i].Action,out output)||(chord.Modifiers&11)==0||OutputConflicts(settings.Macros[i].Action,settings.Macros)) { status.Text="Ett sparat tangentmakro är ogiltigt. Redigera eller ta bort det."; return false; }
            int id=100+i; if(!Native.RegisterHotKey(Handle,id,chord.Modifiers|0x4000,(uint)chord.Key)) { foreach(int registered in hotkeys) Native.UnregisterHotKey(Handle,registered); hotkeys.Clear(); status.Text="Genvägen "+chord.Text+" används redan av Windows eller ett annat program. Välj en annan."; return false; } hotkeys.Add(id);
        } return true;
    }
    protected override void WndProc(ref Message message) { if(message.Msg==0x312) { int index=message.WParam.ToInt32()-100; if(active&&index>=0&&index<settings.Macros.Count) { var recorder=FocusedRecorder(); if(recorder!=null) { recorder.Text=settings.Macros[index].Trigger; message.Result=IntPtr.Zero; return; } Chord trigger; Chord.Parse(settings.Macros[index].Trigger,out trigger); QueueChord(settings.Macros[index].Action,Native.GetForegroundWindow(),trigger.Key); } } base.WndProc(ref message); }
    void QueueChord(string value,IntPtr foreground,Keys trigger) { Chord chord; if(!active||FocusedRecorder()!=null||!Chord.Parse(value,out chord)) return; if(OutputConflicts(value,settings.Macros)) { status.Text="Makrot stoppades eftersom det skulle aktivera ett tangentmakro igen."; return; } if(pendingChord!=null||busy) { status.Text="Ett makro pågår. Vänta tills det är klart."; return; } pendingChord=chord; pendingTarget=foreground; pendingTrigger=trigger; chordWait=0; chordTimer.Start(); }
    void ExecuteWheel(IntPtr foreground) { if(active&&wheelEnabled) QueueChord(settings.Wheel,foreground,Keys.None); }
    void SendPendingChord(object sender,EventArgs e) {
        if(!active||Native.GetForegroundWindow()!=pendingTarget) { chordTimer.Stop(); pendingChord=null; status.Text="Makrot avbröts eftersom målprogrammet ändrades."; return; }
        if(Native.Modifiers()||(pendingTrigger!=Keys.None&&(Native.GetAsyncKeyState((int)pendingTrigger)&0x8000)!=0)) { if(++chordWait<100) return; chordTimer.Stop(); pendingChord=null; status.Text="Släpp genvägens tangenter och försök igen."; return; }
        chordTimer.Stop(); Chord chord=pendingChord; pendingChord=null; TestChordDispatchCount++; status.Text=Native.SendChord(chord)?"Skickade "+chord.Text+" till målprogrammet.":"Windows kunde inte skicka kombinationen. Målprogrammet kan kräva högre behörighet.";
    }
    void SystemThemeChanged(object sender,UserPreferenceChangedEventArgs e) { if(settings.Theme=="system"&&!IsDisposed&&IsHandleCreated) try { BeginInvoke((Action)ApplyTheme); } catch(InvalidOperationException) { } }
    void ApplyTheme() {
        dark=settings.Theme=="dark"; if(settings.Theme=="system") try { using(var key=Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize")) dark=key!=null&&Convert.ToInt32(key.GetValue("AppsUseLightTheme",1))==0; } catch { }
        page=dark?Color.FromArgb(24,29,36):Color.FromArgb(237,241,247); surface=dark?Color.FromArgb(36,43,53):Color.White; text=dark?Color.FromArgb(238,240,248):Color.FromArgb(28,36,48); muted=dark?Color.FromArgb(183,197,217):Color.FromArgb(79,99,124); accent=dark?Color.FromArgb(199,175,241):Color.FromArgb(104,72,177); edge=dark?Color.FromArgb(75,88,107):Color.FromArgb(203,213,228); soft=dark?Color.FromArgb(62,49,83):Color.FromArgb(240,234,250);
        ApplyColors(this); if(diagram!=null) { diagram.Dark=dark; diagram.Invalidate(); } RefreshStudio();
    }
    Color PrimaryHover() { return dark?Color.FromArgb(215,196,247):Color.FromArgb(87,57,153); }
    void ApplyColors(Control parent) { parent.BackColor=parent==sidebar||parent==footerPanel?page:parent.Parent==null?surface:parent.Parent.BackColor; parent.ForeColor=text; foreach(Control control in parent.Controls) { ApplyColors(control); var button=control as Button; if(button!=null) { bool primary=(string)button.Tag=="primary"; button.BackColor=primary?accent:surface; button.ForeColor=primary?(dark?Color.FromArgb(36,25,55):Color.White):text; button.FlatAppearance.BorderColor=edge; button.FlatAppearance.MouseOverBackColor=primary?PrimaryHover():soft; button.FlatAppearance.MouseDownBackColor=primary?accent:soft; } if(control is TextBox || control is ChoiceBox) control.BackColor=page; var label=control as Label; if(label!=null) { label.BackColor=Color.Transparent; if(label.Font.Size<10.1f) label.ForeColor=muted; } } }
    void RefreshStudio() {
        if(diagram==null||refreshing) return; refreshing=true;
        activity.Text=active?"● Aktiv":"● Pausad"; activity.ForeColor=active?(dark?Color.FromArgb(150,218,185):Color.FromArgb(27,111,72)):muted; pause.Text=active?"Pausa":"Aktivera"; startupLabel.Text=StartupInfo();
        foreach(var label in dynamicStates) { int type=int.Parse(((string)label.Tag).Substring(6)); label.Text=!active?"Pausad":type==0?(pathReady?"Klistra in sökväg":"Kopiera sökväg"):(copyReady?"Klistra in · Ctrl+V":"Kopiera · Ctrl+C"); label.ForeColor=active?accent:muted; }
        for(int i=0;i<3;i++) { diagram.Buttons[i].BackColor=i==selected?accent:page; diagram.Buttons[i].ForeColor=i==selected?(dark?Color.FromArgb(36,25,55):Color.White):text; diagram.Buttons[i].FlatAppearance.BorderColor=edge; diagram.Buttons[i].FlatAppearance.MouseOverBackColor=i==selected?PrimaryHover():soft; diagram.Buttons[i].FlatAppearance.MouseDownBackColor=i==selected?accent:soft; }
        tray.Text="TumMakro Studio 1.0 · "+(active?"aktiv":"pausad"); refreshing=false;
    }
    void StudioHelp() { MessageBox.Show(this,"TUMKNAPPAR 1 OCH 2\nMarkera innehållet och tryck för att kopiera. Klicka i målet och tryck samma knapp igen för att klistra in. Sökvägsknappen sparar text eller kopierade filers sökvägar, även utanför Utforskaren. Källprogrammet måste stödja kopiering av text eller filinformation.\n\nMUSHJUL 3\nKlicka på 3 i musbilden, välj funktion och spara. Makrot körs när du trycker ned hjulet i målprogrammet. Rullning uppåt/nedåt påverkas inte. Ett tilldelat tangentmakro kopieras till hjulet.\n\nTANGENTMAKRON\nLägg till namn, aktivering och kombination. Tryck genvägen i fältet; Backsteg rensar. Aktiveringen behöver Ctrl, Alt eller Win. Släpp genvägens tangenter efter trycket. Genvägar som redan används kan inte registreras.\n\nBAKGRUND OCH PAUS\nPausa släpper musknappar och tangentgenvägar till Windows. Dölj och kryss behåller programmet vid klockan. Högerklicka på ikonen och välj Avsluta för att stänga.\n\nRazer Synapse: tumknapparna ska vara Back/Bakåt och Forward/Framåt. Program som körs som administratör kan kräva att TumMakro startas med samma behörighet.","TumMakro Studio 1.0 · hjälp",MessageBoxButtons.OK,MessageBoxIcon.Information); }
    void DisposeStudio() { SystemEvents.UserPreferenceChanged-=SystemThemeChanged; foreach(int id in hotkeys) if(IsHandleCreated) Native.UnregisterHotKey(Handle,id); hotkeys.Clear(); chordTimer.Dispose(); openTimer.Dispose(); if(openSignal!=null) openSignal.Dispose(); }
    public void StopHotkeysForPause() { RefreshHotkeys(); pendingChord=null; chordTimer.Stop(); }
    void CaptureStudio(string file) { Application.DoEvents(); using(var bitmap=new Bitmap(Width,Height)) { DrawToBitmap(bitmap,new Rectangle(0,0,Width,Height)); bitmap.Save(file); } }
    public void RenderStudio(string folder) {
        Directory.CreateDirectory(folder); hooksStarted=true; Show(); Application.DoEvents(); editing=int.MinValue; Size normal=Size;
        foreach(string theme in new[]{"light","dark"}) {
            Size=normal; themes.SelectedIndex=theme=="dark"?2:1; ApplyTheme();
            foreach(int view in new[]{-1,0,1,2}) { selected=view; BuildStudioView(); CaptureStudio(Path.Combine(folder,theme+"-"+(view<0?"overview":(view+1).ToString())+".png")); }
            selected=-1; BuildStudioView(); Point oldCursor=Cursor.Position;
            try { Cursor.Position=pause.PointToScreen(new Point(pause.Width/2,pause.Height/2)); Application.DoEvents(); CaptureStudio(Path.Combine(folder,theme+"-hover.png")); }
            finally { Cursor.Position=oldCursor; Application.DoEvents(); }
            Size=MinimumSize;
            foreach(int view in new[]{-1,2}) { selected=view; BuildStudioView(); CaptureStudio(Path.Combine(folder,theme+"-minimum-"+(view<0?"overview":"wheel")+".png")); }
            Size=normal; selected=2; OpenMacro(-1); macroName.Text="Kommandopalett"; macroTrigger.Text="Ctrl + Alt + F9"; macroAction.Text="Ctrl + Shift + P"; Application.DoEvents(); contentScroll.AutoScrollPosition=new Point(0,Math.Max(0,macroName.Top-70)); CaptureStudio(Path.Combine(folder,theme+"-editor.png")); editing=int.MinValue;
        }
        Size=normal; themes.SelectedIndex=0; selected=-1; BuildStudioView(); CaptureStudio(Path.Combine(folder,"system-overview.png"));
    }

}
