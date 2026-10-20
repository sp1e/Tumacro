using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

static partial class Native {
    [DllImport("user32.dll",SetLastError=true)] public static extern bool OpenClipboard(IntPtr owner);
    [DllImport("user32.dll")] public static extern bool CloseClipboard();
    public static bool ClipboardAvailable() {
        if(!OpenClipboard(IntPtr.Zero)) return false;
        try { return true; } finally { CloseClipboard(); }
    }
}

sealed partial class MacroForm {
    readonly Timer pasteTimer=new Timer();
    readonly Stopwatch pasteClock=new Stopwatch();
    bool pendingPaste,pastePath,pastePrepared; IntPtr pasteTarget;
    int pasteButton,pasteTicks,pasteStable; uint pasteSequence;
    void InitPaste() { pasteTimer.Interval=25; pasteTimer.Tick+=CheckPaste; }
    void BeginPaste(bool path,int button,IntPtr foreground) {
        pendingPaste=true; busy=true; pastePath=path; pasteButton=button; pasteTarget=foreground;
        pastePrepared=!path; pasteTicks=0; pasteStable=0; pasteSequence=Native.GetClipboardSequenceNumber();
        status.Text=L("Förbereder inklistring. Väntar på urklippet…"); pasteClock.Restart(); pasteTimer.Start();
    }
    void CancelPaste(string message) {
        if(!pendingPaste) return;
        pendingPaste=false; busy=false; pasteTimer.Stop(); pasteClock.Stop();
        if(message!=null) status.Text=message;
    }
    void CheckPaste(object sender,EventArgs e) {
        if(!pendingPaste) { pasteTimer.Stop(); return; }
        if(!active||recordingShortcut||Native.GetForegroundWindow()!=pasteTarget) {
            CancelPaste(L("Inklistringen avbröts. Tryck igen i rätt fönster.")); return;
        }
        // WinForms timer ticks can coalesce under load. The retry window is
        // elapsed time, rather than a count of messages delivered to the UI.
        pasteTicks++;
        if(pasteClock.ElapsedMilliseconds>=1500) { CancelPaste(L("Urklippet eller en tangent är upptagen. Nästa tryck försöker klistra in igen.")); return; }
        if(held[pasteButton]||Native.Modifiers()||!Native.ClipboardAvailable()) { pasteStable=0; return; }
        if(!pastePrepared) {
            try {
                var data=new DataObject(); data.SetText(savedPath,TextDataFormat.UnicodeText);
                // Timer-driven retries keep the UI responsive while another app
                // briefly owns the clipboard. Do not block the UI with OLE retries.
                Clipboard.SetDataObject(data,true,0,0); pastePrepared=true;
                pasteSequence=Native.GetClipboardSequenceNumber(); pasteStable=0;
            } catch(ExternalException) { pasteStable=0; }
            return;
        }
        uint sequence=Native.GetClipboardSequenceNumber();
        if(sequence!=pasteSequence) { pasteSequence=sequence; pasteStable=0; return; }
        if(++pasteStable<3||pasteTicks<4) return;
        if(Native.GetForegroundWindow()!=pasteTarget) { CancelPaste(L("Inklistringen avbröts. Tryck igen i rätt fönster.")); return; }
        if(!Native.Shortcut(0x56,false)) { CancelPaste(L("Windows kunde inte skicka Ctrl+V till det här fönstret.")); return; }
        pendingPaste=false; busy=false; pasteTimer.Stop(); pasteClock.Stop();
        if(pastePath) { pathReady=false; savedPath=null; } else copyReady=false;
        status.Text=pastePath?L("Skickade den sparade sökvägen till målet. Nästa tryck på sökvägsknappen kopierar igen."):L("Skickade Ctrl+V till målet. Nästa tryck på Ctrl+C-knappen kopierar igen.");
        UpdateStates();
    }
}
