# Tumacro icon

The current icon shows a broad asymmetric graphite mouse on a porcelain rounded tile, with two orange thumb controls and a short porcelain wheel inset. It identifies Tumacro's mouse macro function, following the user's choice to prioritize that function over the product name.

[`Tumacro.ico`](Tumacro.ico) includes nine square images: 16, 20, 24, 32, 40, 48, 64, 128, and 256 pixels. `Build.ps1` embeds this file in the executable through `/win32icon`; the window and tray use the embedded icon. [`Tumacro-icon.png`](Tumacro-icon.png) is the raster preview, and [`Tumacro.svg`](Tumacro.svg) contains the vector geometry in a `0 0 64 64` viewBox.

The SVG defines porcelain (`#f1efe8`), graphite (`#171d21`), and thumb orange (`#e57845`). Its background measures 60 by 60 units with a 14-unit corner radius and a 2-unit outer inset. The icon has no lettering, arrow badge, texture, or shadow. Preserve the two distinct side controls when exporting smaller sizes.

The icon was drawn from vector geometry for Tumacro. It uses no stock image or external artwork.

The accepted finish review checked the native size proofs on light and dark grounds, the extracted executable icon, and native title-bar captures. The busy-background desktop proof is synthetic; it does not verify a user's desktop or Windows shell-cache refresh.

See [DESIGN.md](../DESIGN.md) for the icon's design tokens. This identity applies to the icon only; the native application's violet light and dark interface keeps its existing palette.
