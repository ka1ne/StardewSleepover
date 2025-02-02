# 🛏️ Stardew Sleepover

> Sleep in your friends' beds! (With permission, of course - 6+ hearts required)

A Stardew Valley mod that lets you have sleepovers at NPCs' houses. Once you've built up enough friendship with a villager, you can sleep in their bed just like your own.

## ✨ Features
- Sleep in any NPC's bed once you have 6+ hearts with them
- Works with all villager houses
- Respects marriage and relationships (need hearts with both partners for shared beds)
- Normal sleep mechanics (save game, restore energy, etc.)
- Can sleep from 8:00 PM (or 6:00 PM if raining)

## 🎮 Installation
1. Install [SMAPI](https://smapi.io/)
2. Download the latest release
3. Extract to your `Stardew Valley/Mods` folder
4. Run the game using SMAPI

## 🔧 Development

### Prerequisites
- SMAPI
- .NET 6.0 SDK
- Visual Studio/VS Code (optional)

### Quick Start
#### Clone the repository
```bash
git clone https://github.com/ka1ne/StardewSleepover.git
```

#### Build the mod
```bash
dotnet build
```
The mod will automatically be copied to your Stardew Valley Mods folder thanks to SMAPI's ModBuildConfig.

### Hot Reloading
While developing, you can:
1. Keep the game running
2. Make code changes
3. Run `dotnet build`
4. Try `reload_i18n` in SMAPI console (`~` key)
5. If changes don't appear, restart the game

Note: Hot reload capabilities are limited:
- ✅ Text/translations can be hot reloaded
- ❌ Code changes (bed coordinates, debug features, etc.) need a game restart
- ❌ Asset changes need a game restart

Most changes in this mod require a full game restart to take effect, as they involve code-level changes like bed coordinates and warp tracking.

### Debug Features
The mod includes several development aids:
- 🟥 Visual bed area rectangles (red semi-transparent boxes)
- 📝 Detailed warp tracking logs in SMAPI console
- 🎯 `sleepover_debug` command for checking tile coordinates
- 📊 On-screen debug text showing bed areas and coordinates

### Project Structure
```
StardewSleepover/
├── ModEntry.cs # Main mod code
├── manifest.json # Mod metadata
└── StardewSleepover.csproj # Project configuration
```

## 🤝 Contributing
Contributions are welcome! Feel free to:
- Report bugs
- Suggest features
- Submit pull requests

## 📝 License
[MIT License](LICENSE)

## 🙏 Acknowledgments
- ConcernedApe for Stardew Valley
- Pathoschild for SMAPI
- All the villagers for letting us sleep in their beds