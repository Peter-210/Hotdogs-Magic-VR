# Hotdogs and Magic - User Manual

Please visit the website at https://peter-210.github.io/Hotdogs-Magic-Website/. It includes features like a demo video and a tutorial on how to play.

## Setup Guide

### Hardware Requirements
All you need is a VR headset linked to a powerful computer and VR controllers to track your hand movements. It is preferable that you have knuckle or wrist straps for your controllers and that you are wearing them since it is quite easy to accidentally throw your controller during play sessions.

### Play Space
This game requires you to dodge projectiles by moving left and right in the game. Be sure to have ample room to shuffle left and right and space to extend your arms forward. Also, it is important to tighten the strap of your headset. This will keep your headset from falling off when playing this game.

### Software Setup
Make sure that you have SteamVR installed on your computer. Under the "Releases" section of GitHub will be a Unity build for the VR game. To play the game, first download and unzip the "Builds" file before running the game "Hotdogs and Magic.exe" (DO NOT click on UnityCrashHandler64.exe). It's preferred that you add the game as a non-steam game and make sure to also include the game in your VR Library through the game's properties. That way you can launch the game directly through SteamVR. If you would like to clone the repo and see the code in action within Unity, make sure that you have Unity Editor version 2022.3.17f1 before launching the project and have SteamVR running in the background.

## Description of Implementation

This game was developed using the Unity Editor version 2022.3.17f1

### Special Assets
All models and objects were created by us using Blender, and the shaders used in the game were also written by us,
with the exception of the following items:
- projectile (with collision effects) asset "Fire Ice Projectile - Explosion" which came from the asset store
- low-poly chair model which came from Sketchfab
- starry-night-sky HDRI from PolyHaven (HDRIHaven)
- Forcefield shader from Ultimate 10+ shaders, modified by us to support VR
- Cinematic boom sound effect from Pixabay
- Fireball whoosh 2 sound effect from Pixabay
- Glass hit sound effect from Pixabay 

https://assetstore.unity.com/packages/vfx/particles/fire-ice-projectile-explosion-217688

https://sketchfab.com/3d-models/rustic-chair-77afa26a76614fc2b8ed845024af0b58#download

https://hdri-haven.com/hdri/starry-night-sky-dome

https://assetstore.unity.com/packages/vfx/shaders/ultimate-10-shaders-168611

https://pixabay.com/sound-effects/cinematic-boom-171285/

https://pixabay.com/sound-effects/fireball-whoosh-2-179126/

https://pixabay.com/sound-effects/glass-hit-192119/



### C# Script Implementation

#### MenuScene/StartMenuScene.cs
This script is attached to the camera rig and is active when the game starts. Since we wanted to avoid attaching components to the scene manually, we opted to use scripts to initialize and modify object components. 
The code within this script serves as an entry point into initializing other portions of the MenuScene by attaching other scripts and components to objects within the scene. This way of programming allowed us to bypass alot of 
merge conflicts that we would otherwise have with Git.

#### MenuScene/MenuSelection.cs
MenuSelection serves as an event listener and an initializer for the controller objects; It has update and trigger event functions that listen for specific events to occur. 
When they do, the script checks to see whether the object is interactable, and if it is, then it invokes the Select() function from that interactable object, which handles it accordingly. The script also is used in the dueling scene to 
handle interactions between potion throwing.

#### MenuScene/StartDuel.cs
This script listens for an interaction event with the wand in the game and retrieves data for the player's dominant hand. The script also starts the process of transitioning from the MenuScene to the DuelingScene.

#### MenuScene/BookinteractionHandler.cs
This class handles the interaction logic for the book. It receives a signal from the controllers via the MenuSelection class and uses trigonometry and vector mathematics to compute the angle at which the book should swing.
In the case that the book is opened by the player, a tutorial video is played. If it is closed, the tutorial video stops.

#### MenuScene/Video.cs
This script is attached to a game object and upon doing so, loads the tutorial video from the resources folder and prepares the game object for playing the video. The trigger for starting the video is located in the BookInteractionHandler.

#### DuelingScene/StartDuelScene.cs
This script is similar to the StartMenuScene script, but it instead prepares the DuelingScene using precomputed information retrieved from the menuScene. 
The script attaches listeners to the controllers, spawn crates, and initializes the enemy and player.

#### DuelingScene/Wand/SpawnWand.cs
This script instantiates a wand object and prepares the logic for its interactions by attaching a script to the wand object. The wand is translated and rotated such that it fits the controller's position/rotation. The hand which holds the wand is determined the moment the player grabs the wand on the menu scene.

#### DuelingScene/Wand/WandLogic.cs
This script controls the logic for the wand, allowing players to shoot projectiles and adding delay between their shots. Motion detection for flicking the wand has been implemented to calculate and determine if the player would like to fire a projectile. The script uses a coroutine and instantiates projectiles on the tip of the wand before releasing them in the direction that the wand is aiming towards.

#### MenuScene/PotionInteractionLogic.cs
This script has all of the logic needed for the player to grab the potion and throw it at the enemy. The class listens for an interaction event from the MenuSelection.cs class which is then passed into the PotionInteractionLogic.
It deals with the process of selecting the potion by hovering over it with the trigger pressed to grab it and the manipulation of the potion through the rotation of when it is in the hand of the player as well as the release (throwing) of the potion.

#### DuelingScene/AILogic/EnemyStateManager.cs
This script is a manager for a finite state machine (FSM) that controls the behaviour of the enemy at various times during the battle. The script initializes the enemy object by attaching an EnemyObject script to it and updates the state for the enemy by changing the current state that it is in.

#### DuelingScene/AILogic/EnemyStateIdle.cs
This script controls the idle state of the enemy, where it avoids the player's attacks. During this state, the enemy does not shoot back at the player. The script uses a random number generator to control where the enemy moves and updates the enemy's position based on the elapsed time. After reaching its destination, it will set up a probabilistic FSM to attack the player by shooting a projectile (70%) or throwing a splash potion (30%).

#### DuelingScene/AILogic/EnemyStateShoot.cs
The script makes the enemy shoot a projectile at the player by standing still to charge up and instantiate a projectile object before releasing it in the player's direction.

#### DuelingScene/AILogic/EnemyStatePotion.cs
The enemy will throw a potion into the air. This potion will have the script "PotionEnemy.cs" which will give the logic of the potion from the homing missile nature of the potion to the aiming and launching of the potion's movements.

#### DuelingScene/Damage/IDamage.cs
This interface would collect the projectile and potion types in the game under a common superclass. The interface provides an abstract method for these objects which is called when they hit another game object which does not ignore collisions.

#### DuelingScene/Damage/PlayerDamage.cs
This script handles the data and functions for the player object in the scene; It updates the hitbox position and size based on the player's head movements when they move, and also handles whenever they take damage or die in the duel. The script also adds components to the player object necessary for detecting collisions within the game.

#### DuelingScene/Damage/EnemyDamage.cs
This script provides functionality for the enemy object in the scene. It is similar to the player object in that it handles damage and dying for the enemy, alongside adding components.

#### DuelingScene/Hitbox/Entity.cs
An interface that has basic functionality for the logic of different game objects which represent Entities in the game. This mainly is for GameObjects which have an amount of health which must be modified through actions
which take place in the game. 

#### DuelingScene/Hitbox/CrateObject.cs
Adding on to the Entity.cs interface, this script manages the health of the crate when a potion or projectile collides with it. When the crate's health hits zero, the crate is destroyed.

#### DuelingScene/Hitbox/PlayerObject.cs
Adding on to the Entity.cs interface, this script manages the health of the player when a potion or projectile collides with it. When the player's health hits zero, the player loses and their view fades to transition back to the menu scene. This script also deals with the position of the potion bottle's respawn point as indicated by a bubble-like object within the game.

#### DuelingScene/Hitbox/EnemyObject.cs
Adding on to the Entity.cs interface, this script manages the health of the enemy when a potion or projectile collides with it. When the enemy's health hits zero, the enemy is defeated and the player wins. When the enemy reaches zero health, it will fall down as a clear indicator that the player has won the duel.

#### DuelingScene/Projectile/ProjectileAbstract.cs
This is a superclass for the player and enemy projectiles within the game. It provides base functionality which is common to both the player and enemy projectiles. It also serves to pass on hit events to the subclass functions which handle respective functionality for both projectiles. One special feature of this script is that it implements bounding boxes to the projectiles. These boxes are collier boxes that change in size depending on how fast the projectile is moving.

#### DuelingScene/Projectile/ProjectileParticle.cs
A script that controls how long the particles from a collided projectile can last. When the time has expired, destroy the particle effect.

#### DuelingScene/Projectile/ProjectilePlayer.cs
This is a subclass of ProjectileAbstract which further defines specialized functionality for the player projectiles, such as special effects and movement.
It inherits some functionality from the ProjectileAbstract class.

#### DuelingScene/Projectile/ProjectileEnemy.cs
This is a subclass of ProjectileAbstract which is similar to ProjectilePlayer. It defines specialized functionality for the Enemy projectiles which are fired at the player.

#### DuelingScene/Projectile/PotionAbstract.cs
An abstract class that provides some basic functionality of the potion. This includes the delay before having to enable the homing missile logic and the different sound and particle effects that the potion will use.

#### DuelingScene/Projectile/PotionPlayer.cs
This script adds to the functionality of the potion abstract class by having an aiming component that targets the enemy by calculating a vertical angle. This does not include the horizontal angle because during testing, the game seemed harder to use potions for the player when horizontal tracking was enabled.

#### DuelingScene/Projectile/PotionEnemy.cs
The script adds to the potion abstract class with an aiming component that targets the player. This tracking has both horizontal and vertical angle tracking.

#### GameDefaults.cs
This script instantiates the GameManager.cs script for global variables that may be important to the state of the game. Information such as the dominant side of the player, or the health of different entities are defined here before sending the data to GameManager.cs.

#### GameManager.cs
This is a script that contains classes used to store data used to set different difficulty and environment parameters in the game. Some of these parameters change as the player interacts with objects and others may be fixed after instantiating.

#### SpatialAudio.cs
This class manages how in-game sounds should be heard when moving around the scene. This script is used to determine the location of the projectiles that are shot by the player or the enemy. Audio volume and panning (left or right ear) are adjusted based on the location of the projectile and the player.

#### TransitionScene.cs
This is a helper class that contains functionality used to fade in and out a player's view and transition them to another scene.

#### Other:
We directly modified the SteamVR_Behaviour_Skeleton script to add fixes relating to animation and rendering issues on SteamVR's side when running our project. 

We also modified the ForceField shader from the Ultimate 10+ shaders from the Unity asset store to enable VR compatibility and allow usage in our project.
