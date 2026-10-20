using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

class StudioButton : Button {
    bool hover,pressed;
    public StudioButton() { DoubleBuffered=true; }
    protected override void OnMouseEnter(EventArgs e) { hover=true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hover=pressed=false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnMouseDown(MouseEventArgs e) { pressed=true; Invalidate(); base.OnMouseDown(e); }
    protected override void OnMouseUp(MouseEventArgs e) { pressed=false; Invalidate(); base.OnMouseUp(e); }
    protected override void OnKeyDown(KeyEventArgs e) { if(e.KeyCode==Keys.Space) { pressed=true; Invalidate(); } base.OnKeyDown(e); }
    protected override void OnKeyUp(KeyEventArgs e) { pressed=false; Invalidate(); base.OnKeyUp(e); }
    protected virtual void DrawCaption(Graphics graphics,Rectangle bounds,Color color) {
        var flags=TextFormatFlags.VerticalCenter|TextFormatFlags.SingleLine|TextFormatFlags.EndEllipsis;
        flags|=TextAlign==ContentAlignment.MiddleLeft?TextFormatFlags.Left:TextFormatFlags.HorizontalCenter;
        if(!ShowKeyboardCues) flags|=TextFormatFlags.HidePrefix;
        TextRenderer.DrawText(graphics,Text,Font,bounds,color,flags);
    }
    protected override void OnPaint(PaintEventArgs e) {
        Color background=BackColor;
        if(!Enabled) {
            Color context=Parent==null?background:Parent.BackColor;
            background=context.GetBrightness()<.45f?Color.FromArgb(52,62,53):Color.FromArgb(230,233,223);
        }
        else if(pressed&&!FlatAppearance.MouseDownBackColor.IsEmpty) background=FlatAppearance.MouseDownBackColor;
        else if(Enabled&&hover&&!FlatAppearance.MouseOverBackColor.IsEmpty) background=FlatAppearance.MouseOverBackColor;
        Color foreground=Enabled?ForeColor:background.GetBrightness()<.45f?Color.FromArgb(174,182,172):Color.FromArgb(92,100,91);
        using(var brush=new SolidBrush(background)) e.Graphics.FillRectangle(brush,ClientRectangle);
        using(var pen=new Pen(FlatAppearance.BorderColor)) e.Graphics.DrawRectangle(pen,0,0,Math.Max(0,Width-1),Math.Max(0,Height-1));
        var bounds=new Rectangle(Padding.Left,Padding.Top,Math.Max(0,Width-Padding.Horizontal),Math.Max(0,Height-Padding.Vertical));
        if((string)Tag=="record-active") { using(var brush=new SolidBrush(foreground)) e.Graphics.FillEllipse(brush,10,Height/2-3,6,6); bounds.X+=10; bounds.Width-=10; }
        DrawCaption(e.Graphics,bounds,foreground);
        if(Focused&&ShowFocusCues&&Enabled) ControlPaint.DrawFocusRectangle(e.Graphics,Rectangle.Inflate(ClientRectangle,-4,-4),foreground,background);
    }
}

sealed class MacroSummaryButton : StudioButton {
    public string MacroName,Combination;
    protected override void DrawCaption(Graphics graphics,Rectangle bounds,Color color) {
        TextRenderer.DrawText(graphics,MacroName,Font,new Rectangle(bounds.Left,bounds.Top+2,bounds.Width-18,24),color,TextFormatFlags.Left|TextFormatFlags.SingleLine|TextFormatFlags.EndEllipsis|TextFormatFlags.NoPrefix);
        using(var mono=new Font("Consolas",9.5f)) TextRenderer.DrawText(graphics,Combination,mono,new Rectangle(bounds.Left,bounds.Top+27,bounds.Width-18,22),color,TextFormatFlags.Left|TextFormatFlags.SingleLine|TextFormatFlags.EndEllipsis|TextFormatFlags.NoPrefix);
        using(var pen=new Pen(color,1.5f)) graphics.DrawLines(pen,new[]{new Point(bounds.Right-7,bounds.Top+20),new Point(bounds.Right-2,bounds.Top+25),new Point(bounds.Right-7,bounds.Top+30)});
    }
}

static partial class Native {
    [DllImport("dwmapi.dll")] static extern int DwmSetWindowAttribute(IntPtr hwnd,int attribute,ref int value,int size);
    public static void SetFrameTheme(IntPtr window,bool dark) { try { int value=dark?1:0; if(DwmSetWindowAttribute(window,20,ref value,4)!=0) DwmSetWindowAttribute(window,19,ref value,4); } catch(DllNotFoundException) { } catch(EntryPointNotFoundException) { } }
}

sealed partial class MouseDiagram {
    void PaintInstrument(PaintEventArgs e) {
        e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;
        var body=new Rectangle(39,14,120,190);
        using(var path=new GraphicsPath()) {
            path.AddBezier(39,74,39,30,62,14,96,14); path.AddBezier(96,14,137,14,159,34,159,74);
            path.AddLine(159,74,159,140); path.AddBezier(159,140,159,181,137,204,100,204); path.AddBezier(100,204,59,204,39,182,39,145); path.CloseFigure();
            using(var brush=new SolidBrush(Dark?Color.FromArgb(38,45,41):Color.FromArgb(231,234,225))) e.Graphics.FillPath(brush,path);
            using(var pen=new Pen(Dark?Color.FromArgb(91,104,91):Color.FromArgb(174,184,171),1.5f)) e.Graphics.DrawPath(pen,path);
        }
        using(var pen=new Pen(Dark?Color.FromArgb(91,104,91):Color.FromArgb(174,184,171),1.2f)) { e.Graphics.DrawLine(pen,99,15,99,80); e.Graphics.DrawLine(pen,40,79,157,79); }
        using(var brush=new SolidBrush(Dark?Color.FromArgb(174,182,172):Color.FromArgb(92,100,91))) e.Graphics.FillRectangle(brush,95,40,8,22);
    }
}

sealed partial class MacroForm {
    Label navigationHint;
    void RefreshNavigationHint() { if(navigationHint!=null) navigationHint.Text=editing==int.MinValue?L("Välj 1, 2 eller hjulet 3\nför att se knappens funktion."):L("Spara eller avbryt makrot\nför att välja en musknapp."); }
    void ApplyRecorderColors(ShortcutBox recorder) { recorder.ApplyPalette(dark,text,muted,accent,edge,page,surface); }
    void SetReviewLanguage(string language) {
        settings.Language=language; languages.SelectedIndex=language=="en"?1:0; RefreshLanguageChrome();
        status.Text=editing==int.MinValue?L("Redo. Markera innehållet och tryck en tumknapp. Nästa tryck på samma knapp klistrar in."):L("Namnge makrot, spela in båda kombinationerna och spara.");
    }
    public void RenderStudioReview(string folder) {
        System.IO.Directory.CreateDirectory(folder); hooksStarted=true; Show(); Application.DoEvents(); Size normal=Size;
        foreach(bool night in new[]{false,true}) {
            editing=int.MinValue; SetReviewLanguage("sv"); themes.SelectedIndex=night?2:1; ApplyTheme(); Size=normal; selected=-1; BuildStudioView();
            CaptureStudio(System.IO.Path.Combine(folder,(night?"dark":"light")+"-sv-normal-overview.png"));
            SetReviewLanguage("en"); Size=MinimumSize; BuildStudioView();
            CaptureStudio(System.IO.Path.Combine(folder,(night?"dark":"light")+"-en-minimum-overview.png"));
            Size=normal; selected=2; BuildStudioView(); wheelChoice.SelectedIndex=1; wheelAction.Text="Ctrl + Shift + P";
            CaptureStudio(System.IO.Path.Combine(folder,(night?"dark":"light")+"-en-normal-wheel.png"));
            SetReviewLanguage(night?"en":"sv"); OpenMacro(-1); macroName.Text=night?"Command palette":"Kommandopalett"; macroTrigger.Text="Ctrl + Alt + F9"; macroAction.Text="Ctrl + Shift + P";
            CaptureStudio(System.IO.Path.Combine(folder,(night?"dark-en":"light-sv")+"-normal-editor.png"));
            CaptureStudioDraft(); SetReviewLanguage(night?"sv":"en"); Size=MinimumSize; BuildStudioView();
            CaptureStudio(System.IO.Path.Combine(folder,(night?"dark-sv":"light-en")+"-minimum-editor.png"));
            editing=int.MinValue;
        }
        SetReviewLanguage("sv"); themes.SelectedIndex=2; Size=new Size(960,800); selected=-1; BuildStudioView();
        CaptureStudio(System.IO.Path.Combine(folder,"dark-sv-user-960-overview.png"));
        settings.Macros.Add(new KeyMacro {Name="Kommandopalett",Trigger="Ctrl + Alt + F9",Action="Ctrl + Shift + P"}); BuildStudioView();
        CaptureStudio(System.IO.Path.Combine(folder,"dark-sv-user-960-saved-macro.png"));
        OpenMacro(-1); Native.FocusTestWindow(Handle); macroAction.StartRecording();
        if(macroAction.Recording) CaptureStudio(System.IO.Path.Combine(folder,"dark-sv-user-960-recording.png"));
        macroAction.CancelRecording();
    }
}
