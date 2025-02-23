# CheckingOverIt
Client mod for the Archipelago multiworld randomizer.

## TODO list
### Required

### Bugs
- Gravity doesn't update correctly after reaching the top
- Fix multiple got over its being skippable by other slots collecting their items
  - idea: datastorage
  - idea: new playerprefs key

### Technicalities 

### Client QoL
- connection input windows before the game starts, works with current configs
- something to clear all AP-savedata from playerprefs

### APWorld Qol
- option: gravity reduction cap (none, 1 less, up to vanilla)

### Gameplay
- Collectable set pieces
- Something with friction
- Teleport traps
- Monologue traps
- Goal enabling item
- Hammer force increasion (starts with 0, 3 is vanilla, up to 4)
- Invisible jetpack item
  - activate by pressing a key
  - limited flight duration, reset by landing on something
- Option checks_per_over_it
- Deathlink:
  - Receiving deathlinks triggers… 
    - Teleport to start
    - Close the game without saving
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
  - Time trial mode: all checks are trials to reach the top in time
