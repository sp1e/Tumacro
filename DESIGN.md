---
name: Tumacro Studio and application icon
description: A native Windows control surface for mouse and keyboard macros, with a preserved icon identity.
colors:
  action-orange: "#f07a3f"
  action-hover: "#ff8f53"
  mineral: "#f4f4ef"
  paper: "#fcfcf9"
  studio-graphite: "#191d1c"
  raised-graphite: "#242a28"
  ivory-text: "#f0f1ea"
  muted-light: "#586151"
  muted-dark: "#aeb6ac"
  line-light: "#d2d7cd"
  line-dark: "#4b564b"
  soft-light: "#e6e9df"
  soft-dark: "#343e35"
  disabled-text-light: "#5c645b"
  active-light: "#1b6f48"
  active-dark: "#96dab9"
  mouse-fill-light: "#e7eae1"
  mouse-fill-dark: "#262d29"
  mouse-edge-light: "#aeb8ab"
  mouse-edge-dark: "#5b685b"
  thumb-orange: "#e57845"
  porcelain: "#f1efe8"
  graphite: "#171d21"
typography:
  brand:
    fontFamily: "Segoe UI"
    fontSize: "20pt"
    fontWeight: 700
  title:
    fontFamily: "Segoe UI"
    fontSize: "17pt"
    fontWeight: 700
  sidebar-title:
    fontFamily: "Segoe UI"
    fontSize: "14pt"
    fontWeight: 700
  section-title:
    fontFamily: "Segoe UI"
    fontSize: "13pt"
    fontWeight: 700
  state:
    fontFamily: "Segoe UI"
    fontSize: "12pt"
    fontWeight: 700
  item-title:
    fontFamily: "Segoe UI"
    fontSize: "11pt"
    fontWeight: 700
  body:
    fontFamily: "Segoe UI"
    fontSize: "10pt"
    fontWeight: 400
  label:
    fontFamily: "Segoe UI"
    fontSize: "9pt"
    fontWeight: 400
  input-id:
    fontFamily: "Consolas"
    fontSize: "11pt"
    fontWeight: 700
  row-id:
    fontFamily: "Consolas"
    fontSize: "12pt"
    fontWeight: 700
  chord:
    fontFamily: "Consolas"
    fontSize: "11pt"
    fontWeight: 400
  summary-chord:
    fontFamily: "Consolas"
    fontSize: "9.5pt"
    fontWeight: 400
rounded:
  studio-control: "0px"
  icon-tile: "14px"
  icon-detail: "2px"
spacing:
  button-inset-x: "12px"
  button-inset-y: "4px"
  button-gap: "8px"
  content-inset-x: "28px"
  content-inset-y: "20px"
  sidebar-inset: "24px"
  icon-inset: "2px"
  thumb-gap: "4px"
components:
  button-primary:
    backgroundColor: "{colors.action-orange}"
    textColor: "{colors.studio-graphite}"
    typography: "{typography.body}"
    rounded: "{rounded.studio-control}"
    padding: "4px 12px"
  button-primary-hover:
    backgroundColor: "{colors.action-hover}"
    textColor: "{colors.studio-graphite}"
  button-secondary-light:
    backgroundColor: "{colors.paper}"
    textColor: "{colors.studio-graphite}"
    rounded: "{rounded.studio-control}"
    padding: "4px 12px"
  button-secondary-dark:
    backgroundColor: "{colors.raised-graphite}"
    textColor: "{colors.ivory-text}"
    rounded: "{rounded.studio-control}"
    padding: "4px 12px"
  button-disabled-light:
    backgroundColor: "{colors.soft-light}"
    textColor: "{colors.disabled-text-light}"
  button-disabled-dark:
    backgroundColor: "{colors.soft-dark}"
    textColor: "{colors.muted-dark}"
  shortcut-value-light:
    backgroundColor: "{colors.mineral}"
    textColor: "{colors.studio-graphite}"
    typography: "{typography.chord}"
    padding: "4px 8px"
    height: "32px"
  shortcut-value-dark:
    backgroundColor: "{colors.studio-graphite}"
    textColor: "{colors.ivory-text}"
    typography: "{typography.chord}"
    padding: "4px 8px"
    height: "32px"
  mouse-input-selected:
    backgroundColor: "{colors.action-orange}"
    textColor: "{colors.studio-graphite}"
    typography: "{typography.input-id}"
    width: "46px"
    height: "44px"
  macro-summary:
    typography: "{typography.body}"
    rounded: "{rounded.studio-control}"
    height: "66px"
  application-icon:
    size: "64px"
  icon-tile:
    backgroundColor: "{colors.porcelain}"
    rounded: "{rounded.icon-tile}"
    width: "60px"
    height: "60px"
  thumb-control:
    backgroundColor: "{colors.thumb-orange}"
    rounded: "{rounded.icon-detail}"
    width: "12px"
    height: "8px"
  wheel-inset:
    backgroundColor: "{colors.porcelain}"
    rounded: "{rounded.icon-detail}"
    width: "5px"
    height: "8px"
---

# Design System: Tumacro Studio and application icon

## Overview

**Creative North Star: "Quiet industrial control graphics"**

Tumacro Studio puts mouse assignments and keyboard macros on a native Windows control board. The user chose Apple interface restraint and Teenage Engineering instrument character: flat materials, readable input identities, and orange controls with a clear purpose. The physical mouse diagram and live next-action readouts connect each numbered input to its current function.

This system covers the WinForms Studio interface and the existing application icon. The icon retains its own palette and authored geometry. Use the C# controls as the interface authority and the source SVG as the icon authority. The sidecar's HTML previews illustrate the controls for documentation only.

**Key Characteristics:**

- Mouse assignments and keyboard macros share the overview.
- Mineral and paper in light mode. Graphite surfaces in dark mode.
- Orange identifies enabled primary actions, selected inputs, and active recording.
- Segoe UI carries interface copy; Consolas identifies inputs and key combinations.
- Flat control surfaces and a fixed mouse sidebar keep operation predictable.

## Colors

The Studio palette pairs warm neutral surfaces with an orange action accent. Frontmatter values come from `ApplyTheme`, `StudioButton.OnPaint`, and the mouse painter.

### Primary

- **Action orange:** Enabled primary actions, selected mouse inputs, and the active recording control. Primary hover uses the brighter action-hover token. Pressed primary controls return to action orange.
- **Thumb orange:** The two thumb controls in the icon only. Preserve this asset color independently of the Studio action accent.

### Neutral

- **Mineral / paper:** Light-mode page and raised control surfaces.
- **Studio graphite / raised graphite:** Dark-mode page and raised control surfaces. Studio graphite also supplies light-mode text and text on orange.
- **Ivory text / muted light / muted dark:** Main dark-mode text and supporting copy in each theme.
- **Line light / line dark:** One-pixel control borders and row dividers. The light line follows the implementation rather than the earlier direction draft.
- **Soft light / soft dark:** Neutral hover, pressed secondary, and disabled button surfaces.
- **Disabled text light / muted dark:** Legible captions on disabled neutral surfaces.
- **Mouse fill / mouse edge:** The diagram's body and outline use their theme-specific pairs.
- **Porcelain / graphite:** The icon's tile and wheel inset, and its solid mouse body.

The activity indicator uses a separate green pair for Active. Paused status uses muted text. Green conveys operational state and does not replace the action accent.

**The Action Accent Rule.** Reserve Studio orange for enabled primary actions, selected inputs, and active recording. Disabled buttons use neutral surfaces in both themes.

**The Icon Boundary Rule.** Preserve the icon's exact palette and SVG geometry. Studio palette changes do not recolor the icon.

## Typography

**Interface font:** Segoe UI, using the native Windows font renderer.
**Input and chord font:** Consolas.

The application uses point sizes and `AutoScaleMode.Dpi`. Frontmatter font sizes describe the C# `Font` values, not CSS pixel substitutions. Native text measurement determines line height. The source defines no separate line-height or tracking scale.

### Hierarchy

- **Brand:** Bold, 20pt, in the header.
- **Title:** Bold, 17pt, for the selected view and macro editor.
- **Sidebar title / section title:** Bold, 14pt and 13pt, for the mouse area and keyboard-macro section.
- **State / item title:** Bold, 12pt and 11pt, for next-action readouts and assignment names.
- **Body / label:** Regular, 10pt and 9pt, for controls, supporting instructions, and status text.
- **Input IDs:** Bold Consolas, 11pt in diagram buttons and 12pt beside assignment rows.
- **Recorded chords:** Regular Consolas, 11pt in the recorder and 9.5pt in summary rows.

**The Input Type Rule.** Use Consolas for physical input identifiers and key combinations. Keep interface labels and instructions in Segoe UI.

## Layout

Studio uses a docked header, footer, fixed mouse sidebar, and vertically scrolling content column. The header is 72 logical pixels high. The footer is 108. The sidebar is 224 wide. Content has 28-pixel horizontal insets, a 20-pixel top inset, and an 18-pixel bottom inset. The source keeps content at least 360 pixels wide and subtracts the Windows scrollbar width when sizing sections.

The form has an 850 × 720 minimum outer size. Its initial client width is 980 pixels. Client height is the smaller of 800 pixels and the primary working-area height minus 90. These are native WinForms measurements at the baseline DPI; Windows scales them. There are no web breakpoints or mobile layouts.

Keep mouse navigation to the left of the current action. The overview shows both thumb assignments, wheel summary, and keyboard-macro section. An active macro editor takes the content column and disables mouse navigation until Save or Cancel. Footer controls and the latest operational message remain in place. More saved macros extend the scrolling column.

The common button uses 12-pixel horizontal and 4-pixel vertical padding, an 84 × 38 minimum size, and an 8-pixel trailing gap. Assignment rows are 80 pixels high. Saved macro rows are 66 pixels high. The wheel summary is 56. Retain these measured distinctions rather than forcing unrelated controls to one height.

## Elevation & Depth

Studio uses flat fills, one-pixel borders, and tonal separation between page and control surfaces. The source defines no shadows, gradients, or animated transitions. Hover and pressed states change fill immediately. Windows owns the title bar and native menus; `SetFrameTheme` requests the matching light or dark frame.

The icon uses flat fills without texture or simulated lighting. Contrast between its graphite mouse, porcelain field, and orange controls supplies the hierarchy.

## Shapes

Studio controls are square-cornered rectangles with thin borders. The authored mouse diagram supplies the curved silhouette. Its selectable input controls remain rectangular and carry the actual input numbers 1, 2, and 3. Keyboard focus uses a native focus rectangle inset by four pixels.

The separate icon has a rounded porcelain tile, an asymmetric mouse body, two projecting thumb controls, and a short wheel inset. The tile and detail radii in frontmatter describe the SVG at 64 pixels. They are not Studio control radii. Preserve the SVG path instead of redrawing the body as an oval.

## Components

### Buttons

Use flat native controls with readable captions. Enabled primary buttons use action orange and Studio graphite text. Secondary buttons use the theme's raised surface and main text. Secondary hover and pressed states use the soft neutral surface. `StudioButton` draws disabled controls with soft neutral fills and the theme's disabled text, regardless of their enabled primary style. It draws the native focus rectangle for enabled keyboard focus.

### Mouse navigation and assignment rows

The 190 × 220 diagram contains three 46 × 44 input buttons. Selected inputs use orange. Inactive inputs use the page surface and main text. Below the diagram, All functions returns to the shared overview. The two assignment rows show an actual input number, its function, supporting text, and a live Next press readout. Pausing changes those readouts to Paused.

### Macro summaries

Each saved macro uses a two-line button: user-supplied name above a monospaced trigger/output mapping, with a drawn chevron at the right. Clicking it opens the dedicated editor. The wheel summary uses the same pattern with its shorter row height. Retain the user-authored name across interface language changes.

### Inputs and shortcut recorder

The name uses a native single-line text field. The shortcut recorder is 88 pixels high with a bordered, 32-pixel-high value display. Record and Clear sit below it, with supporting text alongside. Record changes to Cancel and gains an orange surface plus a small drawn recording dot while capture is active. Clear is unavailable during capture or when no combination exists.

The recorder accepts one chord after the user releases its keys. Escape cancels and keeps the previous value. Losing window focus, hiding the recorder, or disabling it cancels capture. Save is disabled during recording. Native text fields and popup menus retain Windows behavior. Do not claim a custom theme for OS-owned menu surfaces.

### Choice controls and theme behavior

Header choices and the wheel selector use a square native button with a drawn chevron. A `ContextMenuStrip` opens outside the scrolling container; Up and Down change the selection. The header exposes Swedish/English and Follow Windows/Light/Dark choices. Explicit themes persist in settings. System mode reads the Windows app-theme preference and responds to preference changes while that mode remains selected.

### Application icon

The icon is a static asset with no interactive states or typography. Its source is [assets/Tumacro.svg](assets/Tumacro.svg); [assets/Tumacro-icon.png](assets/Tumacro-icon.png) is the raster preview and [assets/Tumacro.ico](assets/Tumacro.ico) is the Windows resource. The SVG has viewBox `0 0 64 64`. Its tile starts at `(2, 2)`. The mouse spans `(20, 10)` to `(51, 55)`. Thumb controls start at `x=13`, with top edges at `y=28` and `y=40`. The wheel starts at `(33, 17)`.

Scale the complete vector. The ICO contains nine square entries: 16, 20, 24, 32, 40, 48, 64, 128, and 256 pixels. The existing size proof covers 16, 20, 24, 32, 48, 64, and 128 pixels on light and dark grounds. `Build.ps1` embeds the ICO through `/win32icon`; `Studio.cs` extracts the associated executable icon. Prior icon acceptance covers extracted-icon and native-title-bar evidence. Synthetic desktop proof establishes visibility on a busy background, not a user's Windows icon-cache state.

## Do's and Don'ts

### Do:

- Do keep both mouse assignments and keyboard macros visible in the overview.
- Do retain readable disabled captions and native keyboard focus in both themes.
- Do preserve explicit theme choices when Windows changes its preference.
- Do keep input numbers and key combinations distinct through Consolas.
- Do preserve the icon silhouette, separate thumb controls, and short wheel inset when exporting.
- Do check the icon at small native sizes on light and dark backgrounds after an asset change.
- Do keep the SVG, PNG preview, and all nine ICO entries consistent.

### Don't:

- Don't use the enabled orange fill for disabled controls.
- Don't replace input identities with decorative numbering.
- Don't add gradients, texture, simulated depth, or ornamental motion to Studio.
- Don't restore the icon's rejected thin oval, letter T, or arrow badge.
- Don't add lettering, texture, ornament, or simulated depth to the icon.
- Don't merge the Studio and icon palettes into one recolored asset set.
