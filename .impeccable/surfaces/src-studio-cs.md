---
version: 1
slug: "src-studio-cs"
primary_target: "src/Studio.cs"
related_targets: ["src/StudioVisuals.cs","src/ShortcutRecorder.cs","src/Language.cs"]
---

# Tumacro Studio: control surface

## Confirmed brief

Windows desktop utility, C# and WinForms. Keep the selected Studio concept and its visual mouse. Give mouse assignments and custom keyboard macros equal visibility. The user asked for the precision of Apple product interfaces and the character of Teenage Engineering instruments. Preserve the app icon, Swedish/English switching, light/dark/system themes, background operation and startup, plus macro behavior. Fix recording and clipboard failures alongside the redesign.

## Direction

Mode: Operate. Build path: code. No raster comp is the visual authority.

The surface behaves as a compact control board: physical mouse inputs on the left, their current action and keyboard macros on the right. Actual button identities 1, 2, and 3 carry the numbering. The recording controls show selected keyboard codes in a readable monospaced field, with Record/Cancel/Clear as explicit states. Orange identifies an action or selected input. It does not decorate every region.

Mineral #F4F4EF, paper #FCFCF9, graphite #191D1C, line #D2D7CD, orange #F07A3F. Dark mode uses graphite #191D1C, raised surface #242A28, text #F0F1EA, and muted text #AEB6AC. The line value follows the finished C# implementation. The earlier direction contract used #D5D8D1. Body and controls use Segoe UI. Keyboard codes and physical input identifiers use Consolas. These are native Windows faces, not downloaded display assets.

The first viewport shows both independent copy/paste mappings, the wheel assignment, and the keyboard-macro section. A macro opens a dedicated editor with a name, trigger recorder, output recorder, and Save/Cancel controls. The footer holds global Pause, Reset, Hide, Help, and the latest operational message. No web hero, status-card grid, gradients, bitmap typography, or ornamental motion.

The signature is the physical mouse diagram joined to a precise, live next-action readout. Empty states explain how to add a macro. Disabled controls remain legible. Focus is visible. Normal and minimum window sizes must preserve the editor's Record and Save controls. Popup choices use a native menu that escapes the scroll container.

## Grounded alternatives and seed

The seven grounded systems considered were a compact audio controller, a camera tethering workbench, a routing matrix, native device preferences, a label-printer control panel, a transit control board, and a command-palette utility. Seed fcee713f selected candidate 6. The user's pinned Apple/Teenage Engineering direction governs its material expression, while its board grammar supplies clear input identities, persistent state, and a stable reading order.

The dealt challengers were bitmap type specimen, construction-grid specimen, garden guide map, boarding pass/gate board, vertical media feed, and variable-font specimen. They were declined for audience identification and product clarity: none improves the actual Windows macro task over the pinned control surface.

Raised disciplines kept from those comparisons:

- Bitmap specimen: make hierarchy survive small sizes, rather than enlarging display lettering.
- Construction grid: align the real controls precisely, without painting a decorative grid.
- Garden guide: keep current selection distinct from inactive controls.
- Gate board: preserve the changed state until the user can read it.
- Media feed: give an active editor one clear focus without hiding navigation context.
- Variable specimen: show the recorded combination as it changes.

## Verification and boundaries

The finished source has a docked header and footer, a fixed mouse sidebar, and a scrolling content column. The shared overview includes both thumb assignments, the wheel summary, and keyboard macros. The dedicated macro editor disables mouse navigation until Save or Cancel. Input and summary controls draw native focus cues. Recording changes Record to Cancel, adds an orange recording state, and disables Save. Disabled buttons use neutral fills and readable themed captions.

DESIGN.md records the native palette, point-size type hierarchy, measured layout, and control states. The schemaVersion 2 sidecar carries display metadata and documentation previews. Those HTML snippets do not replace the WinForms implementation. The application icon retains its exact SVG geometry and separate palette. PRODUCT.md now describes the authorized Studio scope while retaining its existing Windows platform label and schema.

Inspect the batched captures at normal and minimum Windows sizes, Swedish and English, light and dark. Native C# has no useful HTML/CSS detector result. The finish reviewer judges the screenshots and Windows interaction conventions. Source inspection establishes the control rules. Functional tests and screenshot acceptance belong to their separate evidence reports. DrawToBitmap captures do not prove the actual DWM frame color. This task does not recast Windows as another platform or run a product-schema migration.
