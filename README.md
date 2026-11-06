# Qolo Football

Qolo Football is an offline-first 3D Android football game in early foundation work. This repository now contains the preserved M0 architecture and asset-provenance docs, a Unity-oriented source/data scaffold, and a responsive static project site in [`sites/`](sites/). It is not a finished or currently playable game, and it does not include a Unity-generated Android build.

## Project status

- **Foundation:** architecture and asset-license ledger retained from M0 in `docs/`.
- **Starter scaffold:** small, deterministic ball-motion foundation, match-clock foundation, fictional team data, and initial gameplay configuration under `Assets/Qolo/`.
- **Website:** static HTML/CSS/JS site in `sites/`, with no build step or external runtime dependency. It is suitable for a Vercel project whose Root Directory is `sites`.
- **Not yet implemented:** a Unity scene, player movement, touch controls, full 22-player match, AI, career mode, save system, final art/audio, and release signing.
- **Verification limits:** Unity Editor, Android build modules and physical Android devices were not available in this workspace. No Unity compilation, APK/AAB build, gameplay test, offline-device test or performance measurement is claimed.

## Repository map

```text
Assets/Qolo/
  Content/Data/       fictional sample team data and tuning JSON
  Scripts/Foundation/ small reusable simulation and clock source
  Tests/EditMode/     initial Unity Test Framework test source
  README.md           scaffold scope and import notes
docs/
  ARCHITECTURE.md     preserved M0 design and engineering decisions
  ASSET_LICENSES.md   preserved asset provenance ledger
  M1_SCAFFOLD.md      scope, boundaries and next implementation steps
sites/
  index.html          responsive project website
  styles.css          responsive styling
  script.js           small progressive-enhancement interactions
  vercel.json         static-site Vercel configuration
```

## Set up the Unity project

This repository intentionally does not invent an unverified Unity editor patch or package lock. Use Unity Hub to install Unity 6.3 LTS with Android Build Support, SDK, NDK and OpenJDK, then create a **Universal 3D (URP)** project. Copy/import this repository's `Assets/Qolo/` folder into that project, and add the supplied test source after enabling Unity Test Framework in Package Manager. Keep the generated `ProjectSettings/ProjectVersion.txt`, `Packages/manifest.json` and package lock from the editor you actually use; commit them after confirming the exact editor and package versions. See `docs/ARCHITECTURE.md` before choosing CI or release signing.

The starter ball simulation is plain C# with Unity `Vector3` math. `BallSimulation.Step` integrates gravity and linear drag; it is a foundation for future contact, spin, surface and strike models, not the complete match physics described in M0.

## Build and test in Unity

1. Open the generated Unity 6.3 LTS URP project in Unity Hub.
2. Import `Assets/Qolo/` and resolve any editor/package prompts using the versions selected for that project.
3. Open **Window > General > Test Runner**, enable or install Unity Test Framework if needed, and run the Edit Mode tests.
4. In Build Profiles, select Android, set ARM64 and API 26 minimum per the M0 plan, and configure a development APK first.
5. Build and run on a physical Android device. Only report performance or offline claims after measuring the target device/build; signing a release requires an owner-controlled keystore and protected credentials.

No Unity editor or device validation was run as part of this scaffold delivery.

## Deploy the project site to Vercel

The site is plain static HTML, CSS and JavaScript. In Vercel, import this repository and set **Root Directory** to `sites`. Leave Framework Preset as **Other**, Build Command blank, and Output Directory blank (the selected root is served as static files). The included `sites/vercel.json` enables clean URLs and does not require a server function. Alternatively, from this folder run `vercel` after signing in to the Vercel CLI. A deployment is only complete once Vercel returns a deployment URL and its page has been checked.

## Asset and content rules

All example team and player names are fictional placeholders. No third-party game assets are included. Record any future art, audio, font, animation or other external asset in `docs/ASSET_LICENSES.md` before committing it. Do not use real club, league, player, kit or logo branding.

## Current milestone

Foundation and starter source artifacts only. M0 remains the architecture baseline; this work does not claim M1 completion. The M1 slice and its acceptance evidence remain as defined in `docs/ARCHITECTURE.md`.
