# ✦ FNAF Rewritten: '87 — Omniscient Auto-Pilot & Tactical HUD

[![Release](https://img.shields.io/badge/release-v1.0.0-brightgreen.svg)](https://github.com/0-cole/fnaf-87-autopilot/releases)
[![Platform](https://img.shields.io/badge/platform-Windows%20x64-blue.svg)]()
[![Target](https://img.shields.io/badge/game-FNAF%20Rewritten%3A%20'87-red.svg)]()
[![License](https://img.shields.io/badge/license-MIT-purple.svg)]()

An omniscient, in-engine autonomous bot and real-time tactical ESP HUD for ***FNAF Rewritten: '87***.

The Auto-Pilot hooks directly into the game's native Unity Mono runtime to monitor every animatronic state machine, automatically neutralize all incoming threats, maintain power breakers, keep the Puppet's music box fully wound, and repel aggressive incursions with zero delay.

---

## 🎮 In-Game Controls

| Keybind | Action | Description |
| :---: | :--- | :--- |
| **`[B]`** | **Toggle Auto-Pilot** | Switches between **ACTIVE (Auto-Defending)** and **STANDBY (Manual Play)**. |
| **`[H]`** | **Toggle HUD Overlay** | Toggles the tactical cyber-HUD overlay on and off. |

---

## 🛡️ Threat Breakdown & Countermeasures

| Threat / Mechanic | In-Game Target | Auto-Pilot Reaction |
| :--- | :--- | :--- |
| **Toy Freddy (Breaker Boxes)** | `ToyFreddyPositionScript` | Detects when Toy Freddy arrives at any of the 4 breaker boxes (`choice1`–`choice4`). Instantly trips `flash9 = true`, resets `OutageTimer = 40f`, hides his model, and triggers his retreat for 60–90 seconds. |
| **Breakers & Power Outages** | `BreakerSwitchScript` & `LightManagerScript` | Continuously audits all 56 breaker switches across the 4 panels. If any switch is tripped (`!on`), flips it back on immediately, preventing power shutdowns and completely locking out Power Freddy. |
| **The Puppet (Music Box)** | `PuppetScript` | Perpetually locks `timer = 100f` and resets `deathTimer = 40f`. Warning triangles never appear and the box never unwinds. |
| **Withered Foxy** | `FoxyBrainScript` | Keeps hallway kill timer `Death = 40f`. The moment Foxy becomes active in the hall, sets `flash9 = true` to force immediate retreat (`StartTimer = 60–80s`). |
| **Balloon Boy (Vent Infiltration)** | `BBVentScript` | Eliminates manual flashlight timing; when active, sets `flash3 = true`, calls `LeaveBB()`, and resets `timer = 40f` to banish him back to stage wandering. |
| **Office Animatronics** | `OfficeCharacterScript` | Withered Freddy, Withered Bonnie, Withered Chica: Automatically equips the security mask (`OfficeLightScript.mask = true`, 3000 Hz lowpass filter, trigger animations). Nullifies pending kill events (`killed = false`). |
| **Vent Animatronics** | `VentCharacter` | Toy Bonnie and Toy Chica: Detects vent emergence, equips security mask, and forces `LeaveVent`. |
| **Mangle** | `MangleHallScript` | Equips mask when Mangle is hanging from the office ceiling (`Down`), nullifying kill timers. |
| **Shadow Bonnie** | `ShadowBonnieMainScript` | Freezes death timer at 60s, preventing random crash and jumpscare triggers. |
| **Omniscient Immortality** | `OfficeLightScript` | Enforces `OfficeLightScript.dead = false` and `ols.deths = 0` every frame. |

---

## 💻 1-Click Laptop Installation

1. Go to the [Releases](https://github.com/0-cole/fnaf-87-autopilot/releases) tab and download **`fnaf87-autopilot-v1.0.0.zip`**.
2. Extract the zip anywhere on your laptop.
3. Run **`install.bat`** (or `ModInjector.exe`).
   - The installer will automatically detect your game's `FNAFRewritten87_Data\Managed` directory (or prompt you if installed in a custom location).
   - It creates an automatic backup: `Assembly-CSharp.dll.bak`.
   - It injects the `AutoPilot.Attach(this)` hook into `OfficeLightScript.Start()`.
4. Launch **`FNAFRewritten87.exe`** and start any Night!
   - Press **`[B]`** to toggle Auto-Pilot on/off at will.
   - Press **`[H]`** to toggle the HUD.

---

## 🔄 Uninstallation

To restore the game back to 100% vanilla state:
- Run **`uninstall.bat`**, or:
- Navigate to `FNAFRewritten87_Data\Managed\`, delete `Assembly-CSharp.dll`, rename `Assembly-CSharp.dll.bak` back to `Assembly-CSharp.dll`, and delete `FNAF87Mod.dll`.

---

## 🛠️ Building from Source

Requirements: [.NET 10 SDK](https://dotnet.microsoft.com/download)

```powershell
# 1. Clone the repository
git clone https://github.com/0-cole/fnaf-87-autopilot.git
cd fnaf-87-autopilot

# 2. Build the FNAF87Mod assembly (netstandard2.1)
dotnet build src/FNAF87Mod/FNAF87Mod.csproj -c Release

# 3. Build the standalone single-file installer (win-x64)
dotnet publish src/ModInjector/ModInjector.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o release/
```

---

## 📜 License

This project is licensed under the MIT License.
