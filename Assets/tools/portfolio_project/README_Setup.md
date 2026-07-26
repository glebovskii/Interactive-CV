# Interactive Unity UI Toolkit Résumé

This package converts the supplied one-page CV into a compact, scrollable UI Toolkit document intended to appear when the local player enters an existing world trigger.

## 1. Resume analysis and grouping

The original CV uses a conventional two-column print layout. That layout is efficient on paper but becomes dense in a game UI, so the content is reorganized into vertically scannable cards:

1. Profile header
2. Professional summary
3. Skills, subdivided into Engine & Code, Technical Art, and Tools & Diagnostics
4. Artificial Core - its own job group
5. Optor Group - its own job group
6. RetroStyle Games - its own job group
7. Anatoliy Mesheryak Games - its own job group
8. Corepunk - its own project group
9. Last Pirate: Island Survival - its own project group
10. Selected Optor Group games - one portfolio group containing individual game cards
11. Education - its own group
12. Languages - its own group
13. Independent work and accomplishments - its own group
14. GitHub, LinkedIn, Unity Asset Store, itch.io, and CV - each stored as an independent UXML link template

Long paragraphs were shortened and job responsibilities were converted into bullet rows. No new job responsibility was added beyond the source résumé.

## 2. Folder structure

```text
Assets/PortfolioUI/
├── Icons/                      Monochrome SVG icons; tintable at runtime
├── Images/GameThumbnails/      Local project thumbnails
├── Scripts/
│   ├── PortfolioDocumentController.cs
│   ├── PortfolioLinkController.cs
│   └── PortfolioAccentController.cs
├── USS/
│   └── PortfolioDocument.uss
└── UXML/
    ├── PortfolioDocument.uxml
    └── Sections/               One UXML template per logical group
```

The root document does not hard-code Unity GUID-based Template references. The controller clones the section `VisualTreeAsset`s supplied in the Inspector. This keeps every group independently editable and avoids fragile project-specific asset GUIDs.

## 3. Import into Unity

1. Copy `Assets/PortfolioUI` into your project.
2. Allow Unity to import the PNG and SVG assets.
3. Select `PortfolioDocument.uxml` and attach `PortfolioDocument.uss` as its style sheet in UI Builder, or add the USS to your Panel Settings theme/style pipeline.
4. Create a GameObject, for example `PortfolioDocument`.
5. Add a `PanelRenderer` and assign:
   - your `PanelSettings`
   - `PortfolioDocument.uxml` as the visual tree
6. Add these components to the same GameObject:
   - `PortfolioDocumentController`
   - `PortfolioLinkController`
   - `PortfolioAccentController`

## 4. Assign section templates

On `PortfolioDocumentController`, populate **Content Sections** in this order:

1. `ProfileHeader`
2. `Summary`
3. `Skills`
4. `Job_ArtificialCore`
5. `Job_OptorGroup`
6. `Job_RetroStyle`
7. `Job_AnatoliyMesheryak`
8. `Game_Corepunk`
9. `Game_LastPirate`
10. `Game_OptorGroup`
11. `Education`
12. `Languages`
13. `Accomplishments`

Populate **Social Sections** in this order:

1. `Link_GitHub`
2. `Link_LinkedIn`
3. `Link_AssetStore`
4. `Link_Itch`
5. `Link_CV`

## 5. Configure links

Add `PortfolioLinkController`, then use the component context menu **Reset** once to populate all link bindings automatically. The names match the buttons inside the supplied UXML templates.

The link behavior is isolated in `PortfolioLinkController`; changing layout does not require changing URL logic.

## 6. Connect to the existing trigger

The trigger implementation is deliberately not included. Call one of these methods on the existing trigger events:

```csharp
portfolioDocument.Show();
portfolioDocument.Hide();
portfolioDocument.SetVisible(isInsideTrigger);
```

Keep the panel hidden initially by leaving **Visible On Enable** disabled.

## 7. Tint the social icons

Every SVG is monochrome white. The UXML icon elements use the `accent-tint` class. Set the runtime color with:

```csharp
portfolioAccent.SetIconTint(playerColor);
```

`PortfolioAccentController` uses `unityBackgroundImageTintColor`, so no new SVG files or materials are required for different colors.

## 8. Thumbnail notes

- Corepunk and Last Pirate use game screenshots.
- Water Sort, Sudoku, War Strategy, and World of Poker use their store artwork transformed into consistent 16:9 cards.
- The current racing listing did not expose a stable artwork URL during packaging, so `real_racing.png` is an explicitly neutral fallback thumbnail rather than unrelated store imagery.
- Replace any thumbnail by keeping the same filename; no UXML changes are required.

## 9. UI behavior and readability choices

- The panel uses one vertical `ScrollView`, preventing nested scrolling.
- Each logical section is a separate card with a clear title.
- Bullet rows use a dedicated dot element rather than font bullet glyphs, avoiding missing-glyph problems.
- Game cards use locally imported images, so the document does not need runtime HTTP image loading.
- Buttons are large enough for mouse and touch input.
- Widths use flexible wrapping, so cards degrade into a single column on narrower panels.

## 10. Build considerations

- `Application.OpenURL` is supported on desktop and WebGL, but browser popup policies may require the click to originate directly from user interaction. These links are opened from `Button.clicked`, which satisfies that requirement in normal cases.
- SVG importing may require Unity's Vector Graphics support depending on your Unity version. If SVGs are not imported, convert them to white PNGs; the same tint code works on white PNG icons.
- Do not put editor-only namespaces in runtime scripts.
