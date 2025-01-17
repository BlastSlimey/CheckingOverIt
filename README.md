# CheckingOverIt
Client mod for the Archipelago multiworld randomizer.

## Installation
1. Download BepInEx 5.4.23.2 (https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.2) and the zip file from the release page (`BlastSlimey-CheckingOverIt-X.X.X.zip`)
2. Open the directory of the game (the "game root" containing the `GettingOverIt.exe` executable)
3. Copy the contents of the BepInEx zip to the game root
4. Open the game once and close it (so that BepInEx creates the necessary files)
5. Copy the contents of the mod zip to `<game root>/BepInEx/plugins`
6. Open the game once and close it (again, so that the config file is created)
7. OPTIONAL BUT RECOMMENDED: Open `BepInEx/config/BepInEx.cfg` with a text editor, scroll to `[Logging.Console]`, and set `Enabled = ...` to `true`
8. Open `BepInEx/config/BlastSlimey.CheckingOverIt.cfg` with a text editor to enter your connection details

## TODO list
### Bugs
- Only process `GettingOverIt.exe` leads to only windows
- Wind trap should randomly blow the player to the left or right, but instead only blows to the left

### Technicalities 

### Client QoL
- better config file
- restructure installation
- Text client output in console

### APWorld Qol

### Gameplay
- Collectable set pieces
- Something with friction
- Teleport traps
- Monologue traps
- Disable saving any progress
- Deathlink:
  - Receiving deathlinks teleports to the start
  - Unsure about ways of triggering deathlink:
    - listen to audio clips
    - hook into whatever the game outputs onto the console
    - custom buffer of last position of pot collision, height difference triggers
    - divide map into areas and trigger if being in earlier area than before
- Different item/progression modes:
  - Original mode (already implemented)
  - Traps only mode, with gravity increasion
  - Barrier mode: collect mcguffins to be able to go higher
