# CheckingOverIt
Client mod for the Archipelago multiworld randomizer.

## TODO list
### Required

### Bugs

### Technicalities 

### Client QoL
- connection input windows before the game starts, works with current configs

### APWorld Qol

### Gameplay
- Collectable set pieces
- Something with friction
- Teleport traps
- Monologue traps
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
