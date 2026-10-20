using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

// The keyboard hook exists only during an explicit recording. Suppress both
// halves of each captured key so Windows and registered shortcuts cannot run it.
sealed class ShortcutBox : UserControl {
    readonly Label valueLabel=new Label(), hint=new Label();
    readonly Button record=new Button(), clear=new Button();
    readonly HashSet<int> pressed=new HashSet<int>();
    Native.HookProc callback; IntPtr keyboardHook;
    Form owner; string value="", previous="", candidate, language="sv";
    bool completing, invalid;
    public event EventHandler RecordingChanged;
    public bool Recording { get { return keyboardHook!=IntPtr.Zero; } }
    string L(string text) { return UiText.Translate(text,language); }
    public override string Text {
        get { return value; }
        set { string next=value??""; if(this.value==next) { RefreshText(); return; } this.value=next; RefreshText(); OnTextChanged(EventArgs.Empty); }
    }
    public ShortcutBox() {
        Height=88; MinimumSize=new Size(280,88); TabStop=false;
        valueLabel.BorderStyle=BorderStyle.FixedSingle; valueLabel.Padding=new Padding(8,4,8,4); valueLabel.Height=32;
        record.Height=34; record.Width=132; record.FlatStyle=FlatStyle.Flat; record.Cursor=Cursors.Hand;
        clear.Height=34; clear.Width=80; clear.FlatStyle=FlatStyle.Flat; clear.Cursor=Cursors.Hand;
        hint.AutoEllipsis=true; hint.Font=new Font("Segoe UI",9); hint.Height=36;
        Controls.AddRange(new Control[]{valueLabel,record,clear,hint});
        record.Click+=delegate { if(Recording) CancelRecording(); else StartRecording(); };
        clear.Click+=delegate { CancelRecording(); Text=""; hint.Text=L("Ingen kombination vald."); record.Focus(); };
        Layout+=delegate { valueLabel.SetBounds(0,0,Width,32); record.Location=new Point(0,40); clear.Location=new Point(140,40); hint.SetBounds(230,40,Math.Max(0,Width-230),40); };
        VisibleChanged+=delegate { if(!Visible) CancelRecording(); }; EnabledChanged+=delegate { if(!Enabled) CancelRecording(); };
        Leave+=delegate { if(!ContainsFocus) CancelRecording(); };
        RefreshText();
    }
    public void SetLanguage(string code) { language=code; RefreshText(); }
    void RefreshText() {
        if(valueLabel==null) return;
        valueLabel.Text=value==""?L("Ingen kombination vald."):value;
        record.Text=Recording?L("Avbryt inspelning"):L("Spela in"); clear.Text=L("Rensa"); clear.Enabled=Enabled&&!Recording&&value!="";
        record.AccessibleName=L("Spela in tangentkombination"); clear.AccessibleName=L("Rensa tangentkombination");
        valueLabel.AccessibleName=AccessibleName; valueLabel.AccessibleDescription=valueLabel.Text;
        hint.Text=Recording?L("Tryck kombinationen och släpp tangenterna. Esc avbryter."):L("Klicka på Spela in för att ändra.");
    }
    public void StartRecording() {
        if(Recording||!Enabled||!Visible) return;
        owner=FindForm(); if(owner==null||Native.GetForegroundWindow()!=owner.Handle) return;
        // Starting with keys already held would leave part of a chord outside
        // the capture. Ask for release before installing the hook.
        for(int key=8;key<256;key++) if((Native.GetAsyncKeyState(key)&0x8000)!=0) { hint.Text=L("Släpp tangenterna och klicka på Spela in igen."); return; }
        previous=Text; candidate=null; completing=false; invalid=false; pressed.Clear(); callback=CaptureKey;
        keyboardHook=Native.SetWindowsHookEx(13,callback,Native.GetModuleHandle(null),0);
        if(keyboardHook==IntPtr.Zero) { owner=null; hint.Text=L("Inspelningen kunde inte starta. Försök igen."); return; }
        owner.Deactivate+=OwnerDeactivated; record.Focus(); RefreshText();
        if(RecordingChanged!=null) RecordingChanged(this,EventArgs.Empty);
    }
    void OwnerDeactivated(object sender,EventArgs e) { CancelRecording(); }
    static bool Modifier(int key) { return key==16||key==17||key==18||key==91||key==92||(key>=160&&key<=165); }
    bool Held(params int[] keys) { foreach(int key in keys) if(pressed.Contains(key)) return true; return false; }
    IntPtr CaptureKey(int code,IntPtr message,IntPtr data) {
        if(code<0||!Recording) return Native.CallNextHookEx(keyboardHook,code,message,data);
        if(owner==null||Native.GetForegroundWindow()!=owner.Handle) { CancelRecording(); return Native.CallNextHookEx(IntPtr.Zero,code,message,data); }
        int msg=message.ToInt32(), key=Marshal.ReadInt32(data); bool down=msg==0x100||msg==0x104, up=msg==0x101||msg==0x105;
        if(!down&&!up) return Native.CallNextHookEx(keyboardHook,code,message,data);
        if(completing) return (IntPtr)1;
        if(down) {
            if(!pressed.Add(key)) return (IntPtr)1;
            if(!Modifier(key)) {
                string prefix=(Held(17,162,163)?"Ctrl + ":"")+(Held(18,164,165)?"Alt + ":"")+(Held(16,160,161)?"Shift + ":"")+(Held(91,92)?"Win + ":"");
                if(key==27&&prefix=="") { candidate=null; invalid=true; }
                else if(candidate==null&&!invalid) { Chord chord; if(Chord.Parse(prefix+(Keys)key,out chord)) { candidate=chord.Text; valueLabel.Text=candidate; } else invalid=true; }
                else invalid=true; // A macro is one chord, not a sequence.
            }
        } else {
            pressed.Remove(key);
            if(pressed.Count==0&&(candidate!=null||invalid)) {
                completing=true;
                BeginInvoke((Action)delegate { if(Recording) FinishRecording(!invalid&&candidate!=null); });
            }
        }
        return (IntPtr)1;
    }
    void FinishRecording(bool accept) {
        if(!Recording) return;
        Native.UnhookWindowsHookEx(keyboardHook); keyboardHook=IntPtr.Zero;
        if(owner!=null) owner.Deactivate-=OwnerDeactivated; owner=null; pressed.Clear();
        Text=accept?candidate:previous; RefreshText();
        hint.Text=accept?L("Inspelat. Spara för att använda kombinationen."):L("Inspelningen avbröts. Tidigare val finns kvar.");
        if(RecordingChanged!=null) RecordingChanged(this,EventArgs.Empty);
    }
    public void CancelRecording() { FinishRecording(false); }
    public void FocusRecordButton() { record.Focus(); }
    protected override void Dispose(bool disposing) { if(disposing) CancelRecording(); base.Dispose(disposing); }
}
