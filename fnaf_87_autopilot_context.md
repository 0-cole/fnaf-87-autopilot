# FNAF '87 & Freddy's Reanimated Auto-Pilot — Project Context

## 1. Executive Summary & High-Level Objective
This workspace contains reverse-engineered engine mods and omniscient autonomous auto-pilot bots for two FNAF fangames:
1. **Freddy's: Reanimated** (`C:\Games\FNAF-Transfer\freddys-r-v1-0-2.exe`):
   - Engine: Godot (GDScript bytecode modded via GDRE / pck repack).
   - Wednesday minigame speed patched to 2.0s (`minigame_speed = 2.0`).
   - Friday night speed accelerated by 1.5x (`night_speed = 0.8`).
   - In-engine Godot Auto-Pilot bot injected into `res://Scripts/OfficeInteractions/camera_toggle.gd` with Bonnie dragging, Foxy `load_anim()`, Sparky forced reset, and camera pan integration.
2. **FNAF Rewritten: '87** (`C:\Games\FNAF-Rewritten-87\FNAFRewritten87.exe`):
   - Engine: Unity 2021.3.9f1 (Mono x64).
   - Omniscient Auto-Pilot bot and cyber-tactical ESP HUD built in C# (`FNAF87Mod.dll`).
   - Standalone single-file 1-click patcher tool (`ModInjector.exe`) built with Mono.Cecil.
   - Published to GitHub repository: [0-cole/fnaf-87-autopilot](https://github.com/0-cole/fnaf-87-autopilot) with release [v1.0.0](https://github.com/0-cole/fnaf-87-autopilot/releases/tag/v1.0.0).

---

## 2. Critical Architecture & Invariants

### FNAF Rewritten: '87 Game Layout
- **Root Directory**: `C:\Games\FNAF-Rewritten-87\`
- **Executable**: `FNAFRewritten87.exe`
- **Managed DLL Folder**: `C:\Games\FNAF-Rewritten-87\FNAFRewritten87_Data\Managed\`
- **Target Assembly**: `Assembly-CSharp.dll` (Target: `netstandard2.1` / Unity Mono 4.x)
- **Backup Assembly**: `Assembly-CSharp.dll.bak`
- **Mod Assembly**: `FNAF87Mod.dll`

### Threat Mechanics & Auto-Pilot Handling
- **Toy Freddy Switchboard Mechanic**:
  - `ToyFreddyPositionScript.Arrived == true` triggers when Toy Freddy enters one of 4 breaker boxes.
  - Bot reaction: Sets `flash9 = true`, resets `OutageTimer = 40f`, hides model, resets `Arrived = false`, and commands retreat for 60–90 seconds (`brain.Moving = true`).
- **Breakers & Power Outage**:
  - Total switches across the 4 panels is 56. When sum < 56, power cuts and Power Freddy spawns.
  - Bot reaction: Scans `BreakerSwitchScript` every frame; if any switch is `!on`, forces `on = true; sw.anim.SetBool("On", true); sw.bs.Switches++;`. Power outage is completely locked out.
- **The Puppet (Music Box)**:
  - `PuppetScript`: Unwinds at `dif / 3f`. Warning triggers below 30% and 15%.
  - Bot reaction: Locks `puppet.timer = 100f`, `puppet.deathTimer = 40f`, keeping box permanently wound.
- **Withered Foxy**:
  - `FoxyBrainScript`: 40s hallway attack timer. Requires 5 pulsed flashes with $\ge 0.3s$ delay.
  - Bot reaction: Freezes `Death = 40f`. When `Active`, sets `flash9 = true`, triggering retreat for 60–80s.
- **Balloon Boy**:
  - `BBVentScript`: Mask does not work.
  - Bot reaction: When `Active`, sets `flash3 = true; bb.LeaveBB(); bb.Active = false; bb.timer = 40f;`.
- **Office Characters (Withered Freddy, Bonnie, Chica)**:
  - `OfficeCharacterScript`: Check `OfficeLightScript.mask`. If false, `killed = true`.
  - Bot reaction: If `inoffice` or `active`, sets `OfficeLightScript.mask = true`, sets lowpass to 3000 Hz, and wipes `killed = false; dietimer = 2f;`. Takes mask off once office is clear.
- **Vent Characters (Toy Bonnie, Toy Chica)**:
  - `VentCharacter`: If `active`, equips mask until `LeaveVent` triggers.
- **Mangle**:
  - `MangleHallScript`: Equips mask when hanging from ceiling (`Down`) and nullifies kill events.
- **Shadow Bonnie**:
  - `ShadowBonnieMainScript`: Locks `deathtimer = 60f`.
- **Immortality Guard**:
  - `OfficeLightScript.dead = false` and `ols.deths = 0` enforced every frame.

### In-Game Controls
- **`[B]`**: Toggle Auto-Pilot (Active / Standby)
- **`[H]`**: Toggle Tactical HUD overlay

---

## 3. Repository & Packaging

- **GitHub Repository**: `https://github.com/0-cole/fnaf-87-autopilot`
- **Release v1.0.0 Asset**: `https://github.com/0-cole/fnaf-87-autopilot/releases/download/v1.0.0/fnaf87-autopilot-v1.0.0.zip`
- **Source Structure**:
  - `src/FNAF87Mod/AutoPilot.cs`: In-engine Auto-Pilot `MonoBehaviour` with threat logic & `OnGUI()` HUD.
  - `src/FNAF87Mod/FNAF87Mod.csproj`: Targets `netstandard2.1` referencing Unity modules.
  - `src/ModInjector/Program.cs`: Console injector tool using `Mono.Cecil 0.11.6`.
  - `src/ModInjector/ModInjector.csproj`: Standalone single-file win-x64 executable.
  - `release/install.bat`: 1-click batch script for laptop users.
  - `release/uninstall.bat`: Restores vanilla `Assembly-CSharp.dll.bak`.

---

## 4. Recent Modifications & Verified State
- `Assembly-CSharp.dll` hooked cleanly at `OfficeLightScript.Start() -> AutoPilot.Attach(this)`.
- Verified game boot: MonoManager reloads assemblies in 0.108s without crashing.
- Verified game shutdown: Process `FNAFRewritten87.exe` successfully closed.
- Git repository initialized, committed, and pushed to `main` branch.
- GitHub Release `v1.0.0` published with attached 33MB zip installer package.

---

## 5. Immediate Next Steps for Fresh AI Instance
1. If user asks to test on laptop: Instruct them to download `fnaf87-autopilot-v1.0.0.zip` from GitHub Releases and run `install.bat`.
2. If user requests additional visual mods (ESP bounding boxes, camera feed overlays, custom night presets), modify `src/FNAF87Mod/AutoPilot.cs`, rebuild with `dotnet build`, and redeploy `FNAF87Mod.dll`.
