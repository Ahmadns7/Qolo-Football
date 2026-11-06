# Qolo Football architecture and M0 foundation

**Status:** M0 documentation only, 6 November 2026. No gameplay implementation, project build, device profiling, or target performance result is claimed here.

## 1. Product and technical decision

Qolo Football is a fully offline, low-to-mid-poly 3D Android football game with a momentum-led match model, a single-prodigy career RPG, and locally adaptive opponents. Android 8.0 / API 26 and ARM64 are the baseline. The requested budgets are 60 FPS on representative mid-range phones (Snapdragon 680 / Helio G85 class, 3 GB RAM), a graceful 30 FPS fallback on low-end devices, peak RAM below 1.5 GB, cold start below 6 seconds, match load below 8 seconds, installed size below 500 MB with a download target below 350 MB, and battery drain no more than 15% per 30 minutes. These are acceptance targets, not measured results.

### Chosen stack

- Unity **6.3 LTS**, pin one exact editor patch and package lockfile once project creation starts. Unity's release page reports 6.3 LTS support through December 2027. Recheck support and the current patch when starting implementation.
- Universal Render Pipeline (URP), tuned for mobile, with a small number of controlled renderer features. URP supports Android and GLES 3.x/Vulkan-capable devices; do not assume every optional renderer feature or GPU behaves the same across the device matrix.
- C# for game and tooling code. Use ScriptableObjects for authoring and validation, and a versioned JSON schema for user-editable and save-adjacent content where external editing is required.
- IL2CPP, ARM64 only for the baseline. Treat ARMv7 as a separate later size/compatibility decision, not a launch assumption.
- Unity Test Framework for Edit Mode and Play Mode tests. Unity build automation or a self-hosted Unity batch-mode runner is a candidate CI path, conditional on license, runner, signing, and budget verification.

**Why Unity + URP:** It is the user-requested default, fits a 3D mobile match, has a first-party Android build and profiling path, supports C# modularization and the requested data-driven authoring workflow, and URP is the engine's Android-capable render pipeline. It provides a realistic path to low/mid/high quality scaling and Android profiling without asserting that the budgets will automatically be met. Unity's player/runtime, native libraries, render pipeline, textures, scenes, and audio all contribute to size and memory; a content budget and profiling gates are necessary. A custom physics model and disciplined content scope matter more to the requested gameplay/performance than an engine label.

**Honest tradeoffs:** Unity adds a nontrivial runtime footprint and an editor/build licensing dependency. IL2CPP/ARM64 is a platform-compatible release choice, not proof of speed or size. Physics, AI, memory, startup, load time, battery, and packaged size are all unmeasured. Unity Personal terms currently state a $200,000 USD trailing-12-month financial threshold, and Unity's software terms specify a separate Unity Build Server subscription for its Build Server product and prohibit using that product with Unity Personal. Confirm applicable terms and runner licensing before selecting CI. This document is not a legal interpretation. Signing credentials must be stored as protected CI secrets or locally, never committed; the owner must supply and control the release keystore. Do not check in keystores, passwords, or signing keys.

## 2. Architecture

Use a modular monolith in one Unity project, split into assembly definition files and test assemblies. Avoid premature distributed systems and runtime network dependencies. Dependencies point inward toward domain rules; presentation and Unity adapters call domain-facing interfaces. Keep match rules and career simulation testable without scenes or a device.

| Module / assembly | Responsibility | Interfaces / boundaries |
|---|---|---|
| Qolo.Foundation | IDs, clocks, deterministic random source, result/error types, serialization contracts | Small shared primitives only; no gameplay policy |
| Qolo.Data | Content catalogs, schema/version validation, authoring assets, JSON import/export | `IContentCatalog`, `IContentValidator`; immutable runtime definitions |
| Qolo.Physics | Ball and player movement state, contact model, surface/weather parameters, deterministic tuning | `IBallSimulation`, `IPlayerMotion`, `IPhysicsTuning`; pure math separated from Unity scene adapters |
| Qolo.Gameplay | Match state, rules, possession, actions, scoring, clock, substitutions boundary | `IMatchSession`, `IMatchEvents`; orchestrates physics and AI, no UI references |
| Qolo.AI | Role decisions, behavior-tree execution, tactical layer, season tendency profile | `IAgentDecision`, `ITendencyProfile`; consumes compact match observations, no hidden stat buffs |
| Qolo.Career | Player progression, training outcomes, energy/form/morale, fixtures, procedural offers/events | `ICareerService`, `ISeasonSimulator`; content-driven and deterministic from stored seed |
| Qolo.Save | Multi-slot persistence, schema migrations, checksums, backup/restore | `ISaveStore`; versioned DTOs; writes are atomic where platform supports it |
| Qolo.UI | Menus, portrait/landscape flow, HUD, controls, accessibility, localization hooks | Calls application services; no domain state mutation outside commands |
| Qolo.Audio | Match event to SFX/music/crowd routing; optional commentary | `IAudioDirector`; must tolerate absent optional audio bundles |
| Qolo.Platform.Android | Lifecycle, safe areas, haptics, device tier signals, storage paths, Android build settings | Platform adapters behind interfaces; no required network permission/service |
| Qolo.Debug (development builds only) | FPS/frame time, memory, physics step, AI tick cost, trajectory view | Compile-gated and disabled in release; no gameplay dependency |

### Data and content

- Author static content as ScriptableObjects: fictional athletes, clubs, leagues, kits, training drills, event templates, item definitions, tactical presets, physics tuning, localization keys, and quality profiles.
- Use explicit stable IDs and a validation step for duplicate/missing IDs, invalid references, out-of-range stats, missing localization, and malformed event branches.
- Provide a versioned JSON roster import/export format for user-edited fictional rosters. Parse and validate in a staging object before replacing active data. Reject invalid or unknown schema versions safely.
- Keep all content needed for offline play in the base install. Addressables may organize optional asset groups or later downloads, but core progression/matches must never wait on a server. Optional commentary or cosmetic packs must degrade cleanly when absent.
- Never ship real club, league, player, kit, or logo branding. Record each asset's source, license, attribution requirements, modifications, and file path in `docs/ASSET_LICENSES.md` before inclusion.

### Runtime flow

`UI command -> application/match service -> match rules -> fixed-step simulation + staggered AI -> domain events -> UI/audio/save adapters`.

Match simulation owns the authoritative state. Rendering interpolates between fixed simulation states. The physics layer starts with a custom lightweight ball integrator rather than relying on default rigid-body tuning: gravity, drag, Magnus term, spin decay, surface restitution/friction, weather forces, and strike impulse inputs. Player locomotion is momentum-based with acceleration and turn-rate constraints. Build deterministic tuning around one `PhysicsTuning` asset and a seeded match context. Determinism is for repeatable tests/tuning, not a cross-device lockstep guarantee.

Career state is an offline domain model persisted through versioned save DTOs. Event/transfer text is templated from authored data and deterministic seeds. AI observes match events and updates a compact tendency profile; scouting UI surfaces the learned tendencies before a match. Adaptation changes tactical choices and decision quality, not physics or hidden player attributes.

## 3. Performance and quality plan

Profile first, optimize measured hot spots, keep a reproducible device/build/config record. Instrument frame time and p50/p95/p99, CPU main/render thread, GPU time, allocations, GC pauses, memory, physics-step cost, AI tick cost, load time, battery/thermal behavior, and build/download/install size. Run on real devices; Unity documents that Android emulators are not supported as a substitute for device testing.

- Frame pacing: target a 16.67 ms frame budget for 60 FPS and 33.33 ms for 30 FPS. Reserve explicit budgets for render, scripts/AI, physics, UI/audio, and slack only after profiling. Use a fixed physics timestep, cap catch-up steps, and interpolate visuals.
- Physics: 22 players plus one ball target under 2 ms per physics step on the named mid-range class. Use bounded fixed-step work, simple collision primitives, spatial partitioning if profiling shows need, and avoid per-frame allocations. Physics target unverified.
- AI: target under 1.5 ms per frame for all agents on target mid-range. Stagger decision ticks by role/urgency, reuse buffers, cap behavior-tree work, and distinguish tactical updates from per-frame animation. AI target unverified.
- Allocation control: pool transient effects/projectiles/UI rows; avoid LINQ, closures, boxing, string construction, and collection growth in hot loops. Add profiler markers and allocation regression checks around match simulation.
- Rendering: low-poly authored meshes; LODs; baked/static lighting where suitable; GPU instancing and batching where compatible; limit real-time shadows, transparency, post-processing and overdraw; texture atlases; ASTC where supported and an explicit fallback/texture-target strategy to avoid runtime decompression surprises. Test GLES/Vulkan device coverage.
- Audio: compressed assets, short SFX loaded by group, streamed music/commentary where appropriate, and optional commentary content excluded from required offline loop if size needs it.
- Size: establish a baseline before adding content; track per-asset/build-report contributors; strip unused code/assets, avoid broad Resources folders, compress meshes/animations/textures, use release compression, and separate optional content only where distribution supports it. Targets remain unverified until an actual signed/release-equivalent package and store-delivery path are measured.
- Quality tiers: Low / Medium / High selected from a conservative first-launch device profile and user override. Scale render resolution, shadow distance/quality, crowd density, effects, and frame-rate target. Re-evaluate after sustained thermal pressure where supported. Never silently change simulation rules or competitive fairness.
- Startup/load: async-load only where it reduces measured stalls, keep the initial scene and menu assets lean, report fully drawn after meaningful readiness, and time cold start/match load on physical devices.
- Battery/thermal: sustained 30-minute match soak, log frame pacing and thermal state; prioritize stable 30 FPS tier if throttling appears. No battery claim before device tests.

## 4. M0-to-M1 vertical slice plan

M0 defines the slice and does not implement it. M1 should prove a small playable match loop, not career breadth or all modes.

**Slice:** one compact fictional stadium/pitch, two fictional teams of 11, one controllable outfield player with touch movement and pass/shoot/sprint/tackle-switch controls, simplified teammates/opponents, one ball using the first custom momentum model, two goals, score and match timer, restart after goal, pause/settings, and a result screen. Include a dry surface only in M1. Keep presentation low-poly and use placeholder/original or properly licensed assets. No real brands.

**Deliberate exclusions from the slice:** career save/progression, transfers, 80 branching events, six training games, weather/surface variety, season adaptation, cups, local multiplayer, commentary, full customization, real roster import UX, production art, monetization, release signing. These remain later milestone content.

**M1 completion evidence:** playable on a physical Android 8+ ARM64 device, offline after install and first launch, match can start/score/end/restart without network, touch input visibly responds, basic AI completes a match loop, automated tests pass, and profiler captures are recorded for a declared device/config. The user target says 60 FPS mid-range, but M1 acceptance must report the measured frame-time distribution and device; do not declare success based on Editor play mode. Do not silently replace 60 FPS target if missed; document gap and mitigation.

## 5. Planned tests and build pipeline

No tests/build pipeline exist yet. Proposed checks by milestone:

1. **Edit Mode/unit:** physics math invariants and tuning bounds; ball force integration under seeded inputs; surface parameters alter expected outcomes; player acceleration/turning and strike input transformations; deterministic career XP/age/progression; event branching/flags; AI utility/behavior-tree decisions and profile updates; save encode/decode, migration, checksum and corrupt-backup behavior; JSON schema and content references.
2. **Play Mode:** fixed-step match state transitions; input-to-action binding; goal/out-of-play/restart; team role behavior; pause/resume and lifecycle; safe areas/aspect ratios; quality tier selection and overrides; offline path with network disabled.
3. **Integration/soak:** full M1 match; later full season simulation and career-save reload; 50-match and 3-season release soak per acceptance criteria; corrupted-save recovery; airplane-mode cold start and repeated launches. These are planned, not run.
4. **Android build:** pinned Unity editor and package versions; clean checkout; content validation; Edit Mode tests; headless Play Mode where supported; development ARM64 APK; release candidate AAB and debug APK only after signing/secret handling is configured. Keep unsigned/debug artifacts distinct from release-signed artifacts. Archive build reports, test results, symbols, editor logs, and provenance.
5. **CI:** Unity batch mode on a licensed runner, with a separate Android toolchain only if the Unity build path requires it. Unity Build Automation is an option subject to current quotas/pricing and license fit; self-hosted batch mode needs compliant editor seats/build-machine rights. Do not assume Unity Personal can use Unity Build Server. Protect release signing keys as CI secrets or use a controlled signing step; CI signing is blocked until an owner-provided keystore and credentials plus compliant runner/license are available. No credentials in repository. CI config should initially run validation/tests and produce a debug artifact; release AAB signing remains gated.
6. **Perf gates:** save device model, OS/API, SoC/GPU, RAM, Unity patch, render API, quality tier, build type, thermal state, and test scene with each capture. Report p95/p99 frame time, physics/AI costs, RAM, load/startup, package size, and battery results against the targets. Device matrix must include a Snapdragon 680-class and Helio G85-class phone if available, one low-end fallback phone, and a higher tier. Exact devices and access are unconfirmed.

## 6. Risk register

| Risk | Likelihood / impact | Signal | Mitigation / owner action | Status |
|---|---|---|---|---|
| Mid-range 60 FPS target missed under 22-player rendering/simulation | Medium / High | p95 frame time exceeds 16.67 ms, thermal drops | Early physical-device vertical slice, CPU/GPU profiling, tiered graphics and staggered AI; reduce effects/crowd before altering core feel | Unmeasured |
| Physics budget or feel fails under custom forces/collisions | Medium / High | >2 ms step, unstable contacts, inconsistent outcomes | Pure math tests, fixed timestep, one tuning asset, trajectory debug view, repeatable seeds, tune with device captures | Unmeasured |
| AI budget/adaptation is expensive or feels unfair | Medium / High | >1.5 ms/frame, obvious rubber-banding, profile has no visible consequence | Staggered updates, compact profile, tactical-only adaptations, scouting explanations, tests ensuring no stat/physics cheating | Unmeasured |
| Unity runtime/assets exceed size, RAM, startup or battery budgets | Medium / High | Package >350 MB target, peak RAM >1.5 GB, startup/load/battery over budget | Establish baseline, aggressively budget assets, ASTC/LOD/baked light, optional content groups, measure release-equivalent builds and soak | Unmeasured |
| Device/GPU/API fragmentation and thermal throttling | High / High | rendering defects, crashes, sustained frame-rate loss | GLES/Vulkan compatibility matrix, quality fallback, current Unity patches, physical-device testing including low-end and 16 KB page-size compatibility | Unmeasured |
| Unity license terms or CI runner rights/pricing block builds | Medium / High | editor activation or batch/CI build denied, terms mismatch | Verify current tier eligibility, per-machine rights, Build Automation pricing/quotas, and Build Server distinction before committing to CI | Open dependency |
| Release signing is blocked or compromised | Medium / High | no owner-controlled keystore, signing secret leaked | Owner-controlled keystore backup, restricted CI secrets, separate debug/release lanes, never commit keys, test recovery procedure | Open dependency |
| Offline requirement is broken by an optional SDK, content pack or platform call | Low / High | airplane-mode startup/match failure or network permission requirement | Audit dependencies and permissions, block required network calls in tests, keep full core content installed | Unverified |
| Save corruption/version migration causes career loss | Medium / High | failed parse, failed migration, restore failure | Versioned DTOs, checksums, atomic writes, rotating backups, migration and fault-injection tests | Not implemented |
| Third-party asset/license or real-world football IP infringement | Medium / High | missing provenance, recognizable protected marks/content | Fictional-only content, asset license ledger, pre-commit provenance review, original or cleared assets | Not implemented |
| Scope overwhelms solo implementation and delays core USP | High / High | adding career breadth before match feel/metrics | Gate in milestone order, keep M1 slice narrow, defer stretch local multiplayer and non-core polish | Active constraint |

## 7. M0 exit and unresolved items

M0 documentation is complete when architecture boundaries, stack rationale, performance strategy, vertical-slice scope, tests/build plan, and risks are reviewed. This document records those decisions only. No Unity project has been generated and no M1 work should start until the user explicitly advances the milestone.

Unverified: target-device FPS/physics/AI performance, memory, startup/match-load times, battery drain, APK/AAB size, offline behavior, gameplay feel, Unity editor patch/package lock, available Unity license, CI/build-server rights and budget, device availability, release keystore/signing process, and any asset licenses for future content.

## 8. References

- Unity 6 releases and LTS support: https://unity.com/releases/unity-6
- Unity download archive (release patch listings): https://unity.com/releases/editor/archive
- Unity Android requirements and compatibility: https://docs.unity3d.com/Manual/android-requirements-and-compatibility.html
- Unity URP requirements and compatibility: https://docs.unity3d.com/Manual/urp/requirements.html
- Unity Android build settings: https://docs.unity.com/en-us/engine/6000.0/manual/platform-specific/android/building-and-delivering/build-settings
- Unity Android distribution size guidance: https://docs.unity.com/en-us/engine/6000.0/manual/platform-specific/android/building-and-delivering/optimize-distribution-size
- Unity Android device profiling: https://docs.unity.com/en-us/engine/6000.0/manual/platform-specific/android/developing/testing-and-debugging/profile-on-an-android-device
- Unity Editor software terms, last updated June 30, 2026: https://unity.com/legal/editor-terms-of-service/software
