# TumMakro Studio 1.0

Ett litet Windows-program för två tumknappar, mushjulets mittklick och egna tangentkombinationer. Studio visar en schematisk mus: välj knapp 1, 2 eller 3 för att se dess funktion.

## Musknappar

- **Tumknapp 1:** första trycket kopierar markerad sökvägstext eller kopierade filers sökvägar. Nästa tryck klistrar in den sparade texten. Kopian behålls även om du använder Windows-urklippet däremellan.
- **Tumknapp 2:** första trycket skickar Ctrl+C och nästa Ctrl+V. Den använder det som finns i Windows-urklippet vid inklistringen.
- **Mushjul 3:** välj vanligt mittklick, en egen tangentkombination eller ett sparat tangentmakro. Tilldelningen körs när du trycker ned hjulet. Rullning uppåt och nedåt behåller sin funktion.

Sökvägsfunktionen har ingen spärr till Utforskaren. Programmet du kopierar från behöver kunna lämna text eller filinformation i urklippet. Du kan byta plats på tumknapparnas funktioner; deras två steg är oberoende.

## Tangentmakron

Välj Lägg till tangentmakro. Ange namn och tryck aktiveringen respektive kombinationen som ska skickas i fälten. Backsteg rensar ett kombinationsfält. Aktiveringen behöver Ctrl, Alt eller Win tillsammans med en tangent. Släpp genvägens tangenter efter trycket, så skickar TumMakro makrot till det aktuella programmet.

Programmet kontrollerar upptagna och dubblerade genvägar. Ett makro får inte skicka en genväg som aktiverar något av dina egna makron. Ett tangentmakro som tilldelas hjulet kopieras dit; hjulets sparade funktion finns kvar om du sedan tar bort tangentmakrot.

## Starta och använda

Kör TumMakro.exe. **Dölj till klockan** och fönstrets kryss behåller programmet i bakgrunden. Dubbelklicka på ikonen vid klockan eller starta programfilen igen för att öppna fönstret. Högerklicka på ikonen och välj **Avsluta** för att stänga programmet.

**Pausa** återger musknappar och registrerade tangentgenvägar till Windows. **Börja om** återställer tumknapparna till kopiering. Temaväljaren erbjuder Följ Windows, Ljust läge och Mörkt läge; valet sparas.

Razer Synapse behöver ge tumknapparna funktionerna Back/Bakåt och Forward/Framåt. Program som körs som administratör kan kräva att TumMakro startas med samma behörighet.

## Automatisk start

Lägg Installera-autostart.ps1 i samma mapp som TumMakro.exe och kör skriptet i PowerShell. Det installerar programmet i `%LOCALAPPDATA%\TumMakro` och ställer in start vid Windows-inloggning, dolt vid klockan. Ingen administratörsbehörighet krävs.

Parametern `-Avinstallera` tar bort autostarten och behåller filer och inställningar.

## Filer och systemkrav

Windows 10/11 x64 med .NET Framework 4.8. Ingen separat webbruntime behövs. Programmet använder inget nätverk och samlar inte in telemetri. Inställningar sparas i `%LOCALAPPDATA%\TumMakro\settings.xml`, med säkerhetskopia. Kopierad sökvägstext lagras i arbetsminnet tills den klistras in eller återställs. Programfilen saknar kodsigneringscertifikat.

## Bygga

Kör Bygg.ps1 i PowerShell. Källkod och Windows-manifest finns i src. Bygget använder Windows .NET Framework-kompilator och skapar TumMakro.exe.

Studio 1.0 är den nya designutgåvan. GitHub-taggen är `studio-v1.0.0`; äldre utgåvor med taggarna v1.0.0–v1.2.0 bevaras.
