using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

static class UiText {
    public static readonly Dictionary<string,string> Pairs=new Dictionary<string,string>(StringComparer.Ordinal) {
        {"Spela in","Record"},
        {"Rensa","Clear"},
        {"Avbryt inspelning","Cancel recording"},
        {"Ingen kombination vald.","No shortcut selected."},
        {"Klicka på Spela in för att ändra.","Click Record to change the shortcut."},
        {"Tryck kombinationen och släpp tangenterna. Esc avbryter.","Press the shortcut, then release the keys. Esc cancels."},
        {"Spela in tangentkombination","Record shortcut"},
        {"Rensa tangentkombination","Clear shortcut"},
        {"Släpp tangenterna och klicka på Spela in igen.","Release the keys, then click Record again."},
        {"Inspelningen kunde inte starta. Försök igen.","Recording could not start. Try again."},
        {"Inspelat. Spara för att använda kombinationen.","Recorded. Save to use the shortcut."},
        {"Inspelningen avbröts. Tidigare val finns kvar.","Recording cancelled. Your previous choice is unchanged."},
        {"Spela in en kombination för aktivering och en som makrot ska skicka.","Record a trigger shortcut and the shortcut the macro should send."},
        {"1. Aktiveras med","1. Trigger shortcut"},
        {"2. Skickar tangentkombination","2. Shortcut to send"},
        {"Tangentkombination att skicka","Shortcut to send"},
        {"Ge makrot ett namn innan du sparar.","Enter a name before saving the macro."},
        {"Spela in genvägen som ska aktivera makrot.","Record the shortcut that should trigger the macro."},
        {"Spela in kombinationen som makrot ska skicka.","Record the shortcut the macro should send."},
        {"Tumacro körs redan. Öppna det via ikonen vid klockan.","Tumacro is already running. Open it from the system tray."},
        {"Tema","Theme"},
        {"Språk","Language"},
        {"Följ Windows","Use Windows theme"},
        {"Ljust läge","Light mode"},
        {"Mörkt läge","Dark mode"},
        {"Pausa","Pause"},
        {"Aktivera","Resume"},
        {"Börja om","Reset"},
        {"Dölj till klockan","Hide to tray"},
        {"Hjälp","Help"},
        {"Välj en musknapp","Choose a button"},
        {"Välj musknapp","Choose mouse button"},
        {"Klicka på 1, 2 eller hjulet 3\nför att se och ändra funktionen.","Select 1, 2, or wheel button 3\nto view and change its action."},
        {"1 och 2: tumknappar\n3: klick på mushjulet","1 and 2: thumb buttons\n3: press the wheel"},
        {"Mushjul, knapp 3","Wheel, button 3"},
        {"Tumknapp ","Thumb button "},
        {"Visa alla funktioner","Show all actions"},
        {"Öppna Tumacro","Open Tumacro"},
        {"Pausa / aktivera","Pause / resume"},
        {"Börja om med kopiering","Reset copy steps"},
        {"Avsluta","Exit"},
        {"Redo. Markera innehållet och tryck en tumknapp. Nästa tryck på samma knapp klistrar in.","Ready. Select content and press a thumb button. Press the same button again to paste."},
        {"Knapparnas funktioner","Button actions"},
        {"Mushjulet · knapp 3","Wheel · button 3"},
        {"Sökväg som text","Path as text"},
        {"Sparad kopia som text","Saved text copy"},
        {"Windows-urklippet","Windows clipboard"},
        {"på nästa tryck","on the next press"},
        {"3   Mushjulet  ·  ","3   Wheel  ·  "},
        {"Vanligt mittklick","Normal middle click"},
        {"Välj mushjulets makro, knapp 3","Choose wheel macro, button 3"},
        {"Byt knappordning för tumknapparna","Swap the thumb-button actions"},
        {"Egna tangentkombinationer","Custom keyboard macros"},
        {"Välj en genväg som aktiverar makrot och kombinationen som skickas.","Choose a trigger shortcut and the shortcut the macro sends."},
        {"Redigera ","Edit "},
        {"+  Lägg till tangentmakro","+  Add keyboard macro"},
        {"Nytt tangentmakro","New keyboard macro"},
        {"Redigera tangentmakro","Edit keyboard macro"},
        {"Namn","Name"},
        {"Makrots namn","Macro name"},
        {"Aktiveras med · tryck genvägen","Trigger · press the shortcut"},
        {"Aktiveras med","Trigger shortcut"},
        {"Skickar kombinationen · tryck genvägen","Action · press the shortcut to send"},
        {"Skickar tangentkombination","Shortcut to send"},
        {"Spara makro","Save macro"},
        {"Avbryt","Cancel"},
        {"Ta bort","Delete"},
        {"Tangentmakrot är borttaget. Hjulets sparade tilldelning finns kvar.","Keyboard macro deleted. The saved wheel assignment is unchanged."},
        {"Välj makro för mushjulet","Choose a wheel macro"},
        {"Aktiveras när du trycker ned hjulet. Vanlig rullning påverkas inte.","Runs when you press the wheel. Scrolling keeps its usual behavior."},
        {"Funktion","Action"},
        {"Mushjulets funktion","Wheel action"},
        {"Egen tangentkombination","Custom shortcut"},
        {"Skickar tangentkombination · tryck genvägen","Shortcut to send · press the shortcut"},
        {"Hjulmakrots tangentkombination","Wheel macro shortcut"},
        {"Spara hjulmakro","Save wheel macro"},
        {"Ange tangentkombinationen som hjulet ska skicka.","Enter the shortcut the wheel should send."},
        {"Hjulmakrot får inte skicka en genväg som aktiverar ett av dina tangentmakron.","The wheel cannot send a shortcut that triggers one of your keyboard macros."},
        {"Hjulmakrot är sparat. Tryck på hjulet i målprogrammet för att köra det.","Wheel macro saved. Press the wheel in the target application to run it."},
        {"Vanligt mittklick är återställt.","Normal middle click restored."},
        {"Återställ mittklick","Restore middle click"},
        {"Mushjulet använder vanligt mittklick.","The wheel now uses normal middle click."},
        {"Sparad funktion: ","Saved action: "},
        {"Fyll i namn och två giltiga tangentkombinationer.","Enter a name and two valid keyboard shortcuts."},
        {"Aktiveringen behöver Ctrl, Alt eller Win tillsammans med en tangent.","The trigger needs Ctrl, Alt, or Win together with another key."},
        {"Den aktiveringen används redan av ett tangentmakro.","A keyboard macro already uses that trigger."},
        {"Ett makro får inte skicka en genväg som aktiverar något av dina makron.","A macro cannot send a shortcut that triggers one of your macros."},
        {"Aktiveringen används som utdata av hjulmakrot. Välj en annan genväg.","The wheel macro sends that shortcut. Choose a different trigger."},
        {"Makrot är sparat: ","Macro saved: "},
        {" skickar "," sends "},
        {". Släpp genvägens tangenter efter trycket.",". Release the trigger keys after pressing the shortcut."},
        {"Inställningarna kunde inte sparas. Kontrollera skrivåtkomsten till Tumacros mapp och försök igen.","Could not save settings. Check write access to the Tumacro folder and try again."},
        {"Ett sparat tangentmakro är ogiltigt. Redigera eller ta bort det.","A saved keyboard macro is invalid. Edit or delete it."},
        {"Genvägen ","The shortcut "},
        {" används redan av Windows eller ett annat program. Välj en annan."," is already in use by Windows or another application. Choose a different shortcut."},
        {"Makrot stoppades eftersom det skulle aktivera ett tangentmakro igen.","Macro stopped because it would trigger another keyboard macro."},
        {"Ett makro pågår. Vänta tills det är klart.","A macro is running. Wait for it to finish."},
        {"Makrot avbröts eftersom målprogrammet ändrades.","Macro cancelled because the target application changed."},
        {"Släpp genvägens tangenter och försök igen.","Release the shortcut keys and try again."},
        {"Skickade ","Sent "},
        {" till målprogrammet."," to the target application."},
        {"Windows kunde inte skicka kombinationen. Målprogrammet kan kräva högre behörighet.","Windows could not send the shortcut. The target application may require higher privileges."},
        {"● Aktiv","● Active"},
        {"● Pausad","● Paused"},
        {"Pausad","Paused"},
        {"Klistra in sökväg","Paste path"},
        {"Kopiera sökväg","Copy path"},
        {"Klistra in · Ctrl+V","Paste · Ctrl+V"},
        {"Kopiera · Ctrl+C","Copy · Ctrl+C"},
        {"aktiv","active"},
        {"pausad","paused"},
        {"Tumacro Studio 1.0.2 · hjälp","Tumacro 1.0.1 · help"},
        {"Kunde inte ansluta tumknapparna. Starta om programmet.","Could not connect the thumb buttons. Restart the app."},
        {"Start med Windows: inte inställd.","Windows startup: not configured."},
        {"Start med Windows: inställd. Programmet öppnas vid klockan.","Windows startup: configured. The app starts in the system tray."},
        {"Start med Windows: kontrollera under Startappar i Windows.","Windows startup: check Startup apps in Windows Settings."},
        {"Aktiv. Nästa tryck på båda tumknapparna kopierar.","Active. The next press of either thumb button copies."},
        {"Pausad. Musknappar och tangentgenvägar fungerar som vanligt.","Paused. Mouse buttons and keyboard shortcuts work as usual."},
        {"Båda knapparna börjar om: nästa tryck kopierar. Windows-urklippet finns kvar.","Both buttons reset: the next press copies. The Windows clipboard is unchanged."},
        {"Senaste tryck: tumknapp ","Last press: thumb button "},
        {"Vanlig kopiera / klistra in","Standard copy / paste"},
        {"Kopieringen pågår. Vänta på beskedet Sökvägen är sparad och tryck sedan igen.","Copying. Wait for confirmation that the path is saved, then press again."},
        {"Släpp Ctrl, Shift, Alt och Windows-tangenten och försök igen.","Release Ctrl, Shift, Alt, and the Windows key, then try again."},
        {"Fönstret ändrades. Tryck igen i rätt fönster.","The window changed. Press again in the correct window."},
        {"Windows kunde inte skicka Ctrl+V till det här fönstret.","Windows could not send Ctrl+V to this window."},
        {"Skickade den sparade sökvägen till målet. Nästa tryck på sökvägsknappen kopierar igen.","Sent the saved path to the target. The next press of the path button copies again."},
        {"Skickade Ctrl+V till målet. Nästa tryck på Ctrl+C-knappen kopierar igen.","Sent Ctrl+V to the target. The next press of the Ctrl+C button copies again."},
        {"Urklipp är upptaget. Försök klistra in igen.","The clipboard is busy. Try pasting again."},
        {"Windows kunde inte skicka Ctrl+C. Välj innehållet i ett program med samma behörighet som Tumacro.","Windows could not send Ctrl+C. Select content in an application running with the same privileges as Tumacro."},
        {"Skickade Ctrl+C. Nästa tryck skickar Ctrl+V.","Sent Ctrl+C. The next press sends Ctrl+V."},
        {"Kopierar markerad sökvägstext eller hämtar kopierade filers sökvägar…","Copying selected path text or retrieving paths from copied files…"},
        {"Inget text- eller filinnehåll kopierades. Markera sökvägstext eller en fil som programmet kan kopiera och tryck igen.","No text or file data was copied. Select path text or a file that the application can copy, then press again."},
        {"Sökvägen är sparad. Klicka i målet och tryck samma tumknapp igen för att klistra in.","Path saved. Click the target and press the same thumb button again to paste."},
        {"Ingen kopiering kunde bekräftas. Markera innehållet och försök igen.","Could not confirm a copy. Select the content and try again."},
        {"Dina tidigare inställningar används. De kunde inte flyttas till Tumacro-mappen.","Using your previous settings. They could not be copied to the Tumacro folder."},
        {"Inställningarna återställdes från säkerhetskopian.","Settings restored from the backup."},
        {"Inställningarna kunde inte läsas. Spara nya inställningar för att ersätta den skadade filen.","Could not read settings. Save new settings to replace the damaged file."},
        {"Ett ogiltigt tangentmakro har stängts av. Kontrollera dina inställningar.","An invalid keyboard macro was disabled. Check your settings."},
        {"Hjulmakrot var ogiltigt och stängdes av.","The wheel macro was invalid and has been disabled."},
        {"TUMKNAPPAR 1 OCH 2\nMarkera innehållet och tryck för att kopiera. Klicka i målet och tryck samma knapp igen för att klistra in. Sökvägsknappen sparar text eller kopierade filers sökvägar, även utanför Utforskaren. Källprogrammet måste stödja kopiering av text eller filinformation.\n\nMUSHJUL 3\nKlicka på 3 i musbilden, välj funktion och spara. Makrot körs när du trycker ned hjulet i målprogrammet. Rullning uppåt/nedåt påverkas inte. Ett tilldelat tangentmakro kopieras till hjulet.\n\nTANGENTMAKRON\nLägg till namn, aktivering och kombination. Klicka på Spela in, tryck en kombination och släpp tangenterna. Rensa tar bort valet. Esc avbryter inspelningen. Tab kan spelas in. Ctrl+Alt+Delete och andra skyddade systemgenvägar kan inte spelas in. Aktiveringen behöver Ctrl, Alt eller Win. Släpp genvägens tangenter efter trycket. Genvägar som redan används kan inte registreras.\n\nBAKGRUND OCH PAUS\nPausa släpper musknappar och tangentgenvägar till Windows. Dölj och kryss behåller programmet vid klockan. Högerklicka på ikonen och välj Avsluta för att stänga.\n\nRazer Synapse: tumknapparna ska vara Back/Bakåt och Forward/Framåt. Program som körs som administratör kan kräva att Tumacro startas med samma behörighet.","THUMB BUTTONS 1 AND 2\nSelect content and press a thumb button to copy. Click the target and press the same button again to paste. The path button saves selected text or paths from copied files, including applications outside File Explorer. The source application must support copying text or file data.\n\nWHEEL BUTTON 3\nSelect 3 in the mouse diagram, choose an action, and save. Press the wheel in the target application to run the macro. Scrolling up and down keeps its usual behavior. Assigning a saved keyboard macro copies its action to the wheel.\n\nKEYBOARD MACROS\nEnter a name, trigger, and action. Click Record, press one shortcut, and release the keys. Use Clear to remove it. Esc cancels recording. Tab can be recorded. Windows reserves Ctrl+Alt+Delete and other secure shortcuts. The trigger needs Ctrl, Alt, or Win. Release the trigger keys after pressing the shortcut. Shortcuts that another application has registered cannot be used as triggers.\n\nBACKGROUND AND PAUSE\nPausing restores normal mouse buttons and keyboard shortcuts. Hide to tray or close the window to keep the app running. Right-click the tray icon and choose Exit to quit.\n\nLANGUAGE\nChoose Svenska or English in the header. Tumacro saves your language choice.\n\nRazer Synapse: assign the thumb buttons to Back and Forward. Applications running as administrator may require Tumacro to run with the same privileges."}
    };
    public static string Translate(string value,string language) { string translated; return language=="en"&&Pairs.TryGetValue(value,out translated)?translated:value; }
    public static string StartupLanguage() { try { return StudioSettings.Read(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Tumacro","settings.xml")).Language; } catch { return "sv"; } }
    public static string Original(string value) { foreach(var pair in Pairs) if(pair.Value==value) return pair.Key; return value; }
}

sealed partial class MacroForm {
    readonly Dictionary<Control,string> languageChrome=new Dictionary<Control,string>();
    bool changingLanguage;
    string L(string value) { return UiText.Translate(value,settings.Language); }
    void CaptureLanguageChrome() {
        foreach(Control control in Descendants(this)) if(!(control is TextBox)&&!(control is ChoiceBox)&&control.Text!="") languageChrome[control]=UiText.Original(control.Text);
        foreach(ToolStripItem item in tray.ContextMenuStrip.Items) item.Tag=UiText.Original(item.Text);
    }
    void RefreshLanguageChrome() {
        foreach(var pair in languageChrome) if(!pair.Key.IsDisposed) pair.Key.Text=L(pair.Value);
        string[] labels={L("Följ Windows"),L("Ljust läge"),L("Mörkt läge")};
        for(int i=0;i<labels.Length;i++) themes.Items[i]=labels[i];
        themes.Text=themes.Items[themes.SelectedIndex].ToString(); themes.AccessibleName=L("Tema"); languages.AccessibleName=L("Språk");
        diagram.AccessibleName=L("Välj musknapp");
        for(int i=0;i<3;i++) diagram.Buttons[i].AccessibleName=i==2?L("Mushjul, knapp 3"):L("Tumknapp ")+(i+1);
        foreach(ToolStripItem item in tray.ContextMenuStrip.Items) item.Text=L((string)item.Tag);
    }
    void ChangeLanguage(string language) {
        if(changingLanguage||settings.Language==language)return;
        CaptureStudioDraft(); string previous=settings.Language; settings.Language=language;
        if(!SaveSettings()) { settings.Language=previous; changingLanguage=true; languages.SelectedIndex=previous=="en"?1:0; changingLanguage=false; return; }
        RefreshLanguageChrome(); BuildStudioView(); RefreshStudio(); languages.Focus();
        status.Text=language=="en"?"Language set to English. Your macros and copy steps are unchanged.":"Språket är nu svenska. Dina makron och kopieringssteg är oförändrade.";
    }
    public void ExerciseLanguageForTest(Action<bool,string> assert,Action<int> pump) {
        assert(settings.Language=="sv"&&languages.SelectedIndex==0,"Swedish remains the initial language");
        OpenMacro(-1); macroName.Text="Mitt eget namn"; macroTrigger.Text="Ctrl + Alt + F8"; macroAction.Text="Ctrl + Shift + P";
        pathReady=true; copyReady=true; savedPath="C:\\Svensk mapp\\fil.txt";
        languages.Focus(); Native.SendChord(new Chord {Key=Keys.Down}); pump(180);
        assert(settings.Language=="en"&&languages.Text=="English"&&pause.Text=="Pause","Keyboard language picker switches to English immediately");
        assert(macroName.Text=="Mitt eget namn"&&macroTrigger.Text=="Ctrl + Alt + F8"&&pathReady&&copyReady&&savedPath=="C:\\Svensk mapp\\fil.txt","Language switch preserves macro drafts, path data and independent copy states");
        assert(tray.ContextMenuStrip.Items[0].Text=="Open Tumacro"&&themes.Items[0].ToString()=="Use Windows theme"&&diagram.Buttons[2].AccessibleName=="Wheel, button 3","English covers tray menu, theme choices and accessible labels");
        bool helpTranslated=false; foreach(var pair in UiText.Pairs) if(pair.Key.StartsWith("TUMKNAPPAR")) helpTranslated=L(pair.Key).StartsWith("THUMB BUTTONS")&&L(pair.Key).Contains("LANGUAGE");
        assert(helpTranslated,"English help contains the macro guide and language instructions");
        TogglePause(); assert(pause.Text=="Resume"&&activity.Text=="● Paused"&&status.Text.StartsWith("Paused."),"Runtime pause status and actions use English"); TogglePause();
        int height=Height; Size=MinimumSize; Application.DoEvents(); assert(!languages.Bounds.IntersectsWith(themes.Bounds)&&languages.Left>=350&&languages.Right<activity.Left,"Minimum window keeps language, status and theme controls separate");
        languages.Focus(); Native.SendChord(new Chord {Key=Keys.Up}); pump(180);
        assert(settings.Language=="sv"&&pause.Text=="Pausa"&&tray.ContextMenuStrip.Items[0].Text=="Öppna Tumacro"&&macroName.Text=="Mitt eget namn","Keyboard picker switches back to Swedish and keeps the draft");
        editing=int.MinValue; BuildStudioView(); Reset();
    }
}
