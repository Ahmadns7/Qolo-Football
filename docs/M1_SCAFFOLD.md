# Starter scaffold notes

**Status:** early project foundation, not an M1 playable vertical slice.

This increment adds source and data placeholders suitable for importing into a Unity 6.3 LTS Universal Render Pipeline project. It deliberately does not create a Unity scene, claim successful Unity compilation, or claim a playable match. The editor and Android toolchain are not present in the current workspace.

## Included

- `Assets/Qolo/Scripts/Foundation/BallSimulation.cs`: a small fixed-step-friendly ball state/integrator with gravity and linear drag, using Unity `Vector3`. Contact, spin, Magnus force, weather, strike impulse and pitch collision are not implemented.
- `Assets/Qolo/Scripts/Foundation/MatchClock.cs`: a deterministic elapsed-time clock with explicit start, pause and reset operations. It is a domain utility, not a full match-state machine.
- `Assets/Qolo/Content/Data/`: fictional example sides and tuning defaults in versioned JSON. These are starter values, not validated game balance.
- `Assets/Qolo/Tests/EditMode/`: a small NUnit test source sample for the Unity Test Framework. Tests have not been executed; the Unity test package may need enabling in Package Manager.
- `sites/`: responsive static project landing site and Vercel configuration.

## Suggested next implementation increment

1. Create the pinned Unity 6.3 LTS URP project with Unity Hub, then commit the exact generated editor/package version files.
2. Add a minimal pitch scene and original placeholder player/ball objects; create a launch-to-match-to-result loop before career features.
3. Add player movement, touch input, ball strikes, goals, restart and one simple opponent behavior, then expand pure domain tests.
4. Run Editor tests, build a development ARM64 APK and test offline on a physical Android device. Capture device/config, p95 frame time, memory, startup/load and package size before comparing with M0 targets.
5. Keep all content fictional, and update `docs/ASSET_LICENSES.md` before introducing non-original assets.
