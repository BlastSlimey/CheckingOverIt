# CheckingOverIt
Client mod for the Archipelago multiworld randomizer.

## Installation
1. Download BepInEx 5.4.23.2 (https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.2) and the zip file from the release page (`BlastSlimey-CheckingOverIt-X.X.X.zip`)
2. Open the directory of the game (the "game root" containing the game executable, e.g. `GettingOverIt.exe`)
   - This directory can be found by opening Steam > Library > Getting Over It [...] > Settings wheel or right click > Manage > Browse local files
3. Copy the contents of both the BepInEx zip and the mod zip to the game root
4. Open the game once and close it (so that BepInEx creates the necessary files, including the config file)
5. OPTIONAL BUT RECOMMENDED: Open `BepInEx/config/BepInEx.cfg` with a text editor, scroll to `[Logging.Console]`, and set `Enabled = ...` to `true`
6. Open `BepInEx/config/BlastSlimey.CheckingOverIt.cfg` with a text editor to enter your connection details

## Updating
Just download the mod zip and copy it to the game root

## TODO list
### Bugs

### Technicalities 

### Client QoL
- better config file

### APWorld Qol
- UT compatibility

### Gameplay
- Collectable set pieces
- Something with friction
- Teleport traps
- Monologue traps
- Disable saving any progress
- Goal enabling item
- Hammer force increasion (starts with 0, 3 is vanilla, up to 4)
- Option checks_per_over_it
- Deathlink:
  - Receiving deathlinks triggers… 
    - Teleport to start
    - Play progress losing audio clip
    - both
  - Unsure about ways of triggering deathlink:
    - listen to audio clips
    - hook into whatever the game outputs onto the console
    - custom buffer of last position of pot collision, height difference triggers
    - divide map into areas and trigger if being in earlier area than before
  - Option to also teleport the player to the start or close the game upon sending a deathlink
- Different item/progression modes:
  - Original mode (already implemented)
  - Traps only mode, with gravity increasion
  - Barrier mode: collect mcguffins to be able to go higher
