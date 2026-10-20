---
name: Tumacro application icon
description: Icon identity for a Windows mouse macro utility; excludes the application interface.
colors:
  thumb-orange: "#e57845"
  porcelain: "#f1efe8"
  graphite: "#171d21"
rounded:
  icon-tile: "14px"
  icon-detail: "2px"
spacing:
  icon-inset: "2px"
  thumb-gap: "4px"
components:
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

# Design System: Tumacro application icon

## Overview

**Creative North Star: "Quiet industrial control graphics"**

Tumacro's icon identifies its mouse macro function through a broad mouse body and two highlighted thumb controls. The user chose recognition of this hardware interaction over recognition of the product name. The finished icon uses authored vector geometry and flat fills.

This document covers the application icon identity only. The native application's existing violet light and dark interface remains outside this palette and token set. Use the source SVG as the geometry authority when exporting the icon.

**Key Characteristics:**

- Broad asymmetric graphite mouse with a short porcelain wheel inset.
- Two separate orange thumb controls on a porcelain field.
- Static geometry with no lettering, arrow badge, texture, or shadow.

## Colors

Use orange to identify the two thumb controls and neutrals to separate the mouse from its field.

### Primary

- **Thumb orange:** Both side controls use this accent. Keep the gap between them visible after reduction.

### Neutral

- **Porcelain:** The rounded tile and wheel inset share this fill.
- **Graphite:** The mouse body uses a single solid fill.

**The Icon Scope Rule.** Apply these colors to the icon asset. Preserve the application's existing UI theme when extending native screens.

## Layout

The SVG uses a square viewBox (`0 0 64 64`). Frontmatter dimensions describe its geometry at a 64-pixel rendering; they are not application layout tokens. The tile starts at `(2, 2)`, leaving a transparent outer inset. The body extends from `(20, 10)` to `(51, 55)` and sits to the right of the tile's center. Both thumb controls start at `x=13`, with top edges at `y=28` and `y=40`. The wheel begins at `(33, 17)`.

Preserve these relationships by scaling the complete vector. The ICO contains nine square entries: 16, 20, 24, 32, 40, 48, 64, 128, and 256 pixels. The size proof covers 16, 20, 24, 32, 48, 64, and 128 pixels on light and dark grounds. The ICO directory establishes the other packaged sizes.

## Elevation & Depth

The icon uses flat fills without gradients, shadows, texture, or simulated lighting. Contrast between the graphite body, porcelain field, and orange controls supplies the shape hierarchy.

## Shapes

The porcelain tile has rounded corners. The broad filled mouse has unequal side curves, a rounded top, and a curved lower edge. Two rectangular thumb controls project from its left side. Each control and the wheel inset use the smaller corner radius. Preserve the SVG path instead of redrawing the body as an oval.

## Components

### Application icon

The icon is a static asset with no interactive states and no typography. Its source is [assets/Tumacro.svg](assets/Tumacro.svg); [assets/Tumacro-icon.png](assets/Tumacro-icon.png) is the raster preview and [assets/Tumacro.ico](assets/Tumacro.ico) is the Windows application resource.

`Build.ps1` embeds the ICO through `/win32icon`. `src/Studio.cs` extracts the executable's associated icon for the native window. The finish review accepted the extracted executable icon and native light and dark title-bar captures. The synthetic desktop proof checks visibility on a busy background; it does not establish the state of a user's desktop or Windows icon cache.

## Do's and Don'ts

### Do:

- Do preserve the source silhouette, separate thumb controls, and short wheel inset when exporting.
- Do check the icon at native small sizes on light and dark backgrounds after an asset change.
- Do keep the SVG, PNG preview, and all nine ICO entries consistent.

### Don't:

- Don't restore the rejected thin oval, letter T, or arrow badge.
- Don't add lettering, texture, ornament, or simulated depth to this icon.
- Don't apply the icon palette as a replacement for the native application's violet UI themes.
