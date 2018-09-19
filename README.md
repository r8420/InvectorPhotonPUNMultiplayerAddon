# InvectorPhotonPUNMultiplayerAddon
This is meant as a addon for the Unity Invector Package to add multiplayer support. 

## Pre-Setup Process
Follow these steps to setup photon to use this package properly.

1. Go To: https://www.photonengine.com/pun
2. Sign Up for a free account (or us the one you already have)
3. Go to your account page and setup a new app
4. Grab your AppId

    <img src="https://raw.githubusercontent.com/wesleywh/InvectorPhotonPUNMultiplayerAddon/master/setup_app_id.PNG" width="60%;" height="100%;" />
5. Download PUN 2 from the Asset Store to Unity (NOT PUN)
   - assetstore.unity.com/packages/tools/network/pun-2-free-119922
6. When the project starts add your appid from your created photon app to the pop-up window.

    <img src="https://raw.githubusercontent.com/wesleywh/InvectorPhotonPUNMultiplayerAddon/master/add_app_id.PNG" width="40%;" height="100%;" />
7. Done!

## Setup
1. Make sure the Invector Package is imported
2. Make sure the pre-setup section is done!
3. Import this package only selecting the modify scripts
4. Open the menu Invector/Multiplayer/Add Multiplayer To Invector Scripts
5. When done re-import this package and select everything
6. Open Invector/Multiplayer/Make Player Multiplayer Compatible
7. Follow the help box instructions
8. When done make the player multiplayer compatible run the Invector/Multiplayer/Convert Scene To Multiplayer
  - Note: This allows you to select which objects to modify. It is suggested to leave the tick box checked to avoid any problems.

You're Done!

## Menu Options

### Convert Scene To Multiplayer
This will find all Invector components on gameobjects and Rigidbodies that can be made to correctly sync or work with multiplayer. 

### Add Multiplayer To Invector Scripts
This will modify the actual invector scripts to work with multiplayer.

### Create Network Manager
This creates the network manager gameobject. You don't really need to run this as the "Make Player Multiplayer Compatible" option will do the same thing.

### Make Player Multiplayer Compatible
This copies your gameobject, adds needed components, creates a network manager (with example UI), and setups up the PhotonView component with needed values. It also makes a prefab of this compatible player and assigns that player to the spawnable player for the network manager.
