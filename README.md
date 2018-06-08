# Tehkne.QuickRepo

Repository patern implementation for entity framework

Using BaseRepository you can extend your own methods and be able to avoid some of the performance issues with bulk editions / updates.
These are done by deactivating AutoDetectChanges but this will prevent using pure Many to Many relations between entities.
I usually add an Id column to every relation entity so that I can manage them myself.

I'm open to changes and recomendations so feel free to create a pull request :)

Also my first open project so every feedback (good or bad) is apreciated!
