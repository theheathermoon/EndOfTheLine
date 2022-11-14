Thank you for installing Q-MapGenetator!



Feedback:

lidan-357@mail.ru
connect.unity.com/u/5a7ae5be03b002001c080760
vk.com/lidan357rus



Preparation for work

Create new control buttons with names "Crouch" and "Run" (without quotes). I prefer to use the keys for them "left ctrl" and "left shift".

8 and 9 layers used for the correct operation of certain functions. Name them "Wall" and "Items in Hands" (without quotes).

Open Q-MapGenerator/_Scenes/Scene_1



Folders

_Scenes - here is the base scene of the project. It already has everything you need to work the generator.
More information about the content of the scene you can be read further.

NavMeshComponents - A tool for creating a NavMesh in real time. Thanks to the efforts of the Unity team. Sources and training on its use can be found here: https://unity3d.com/ru/learn/tutorials/topics/navigation/introduction-and-navigation-overview?playlist=17105

Prefabs - ready prefabs for the functioning of the generator.

	Alpha Big Room, Alpha Cabine, Alpha Intersection, Alpha Room A, Alpha Room B, Alpha Room C, Alpha Short Corridor, Alpha Standart Corridor - locations from which the map is built. Maket version, consist of simple white blocks.

	Basic Door - the doors, open when the player approaches.

	Door Spawn - marks the place where the door should appear. With a tag "MapGenerated_Clean".

	Locations Spawns - consists of a set of colliders, each of which is a potential location to create a location.

	Map Generator - so... it is map generator.

	Map System - painting player`s map.

	Minimap Wall - just a small cube, used when drawing a map.

	Pickup Spawn - place for the appearance of objects on the map.

	Player - player`s prefab. Creating by Map Generator.

	Player Icon - icon that show the player on the minimap.

	Player UI - displays the count of keys a player has and the animation when the player pickup them.

	Spawn Wall - wall, width and height equal to the door. Removed on contact with Door Spawn. Has an child object with a tag "MapGenerated_Clean". A child object creates the specified in it Locations Spawns.

	Start Room - the basic location in which the player starts.

	TestKey 1, 2, 3 - test keys that the player can pick up.



Scripts

	ApplyMinimapTexture - applies the minimap texture to the object.

	BasickDoor - is responsible for opening and closing the door. Designed to respond to several objects. For example: if the doors open when both the player and the monster approach, they will not close until player and monster are out of the zone.

	CreateMinimap - together with the script MapDraw draws a map of the player and transfers its image to the desired texture.

	CreatePlace_forDoor - is part of the Door Spawn object and removes components with the "Wall" tag on contact. As planned, it removes the Spawn Wall objects.

	DoorColor_onMap - part of the Basic Door object. Specifies the color of the door on the minimap.

	KeysSettings - determines the color of the items. It is necessary for the correct operation of the PlayerPickUp script.

	LocationSettings - is an integral part of the locations. Defines a collider tag in the Locations Spawns object in which the location should appear.

	LocationsSpawn - is responsible for the colliders in the Locations Spawns object to be deleted when collision witch objects with Rigidbody (floor in each location). Thus, locations never intersect.

	LocationsSpawner - part of the Spawn Wall object, creates the Locations Spawns.

	MapDraw - draws a minimap, which is then displayed by the player using the CreateMinimap script.

	MapGenerator - the main script. About its work will be told further.

	MapSystemOffset - moves the center of the minimap to its correct display.

	ObjectsSpawnSettings - составная часть объектов и мест для их создания, позволяет выдать всем местам для создания объекта тэг "ObjectSpawn", и индивидуально настроить тип объектов и места их создания.

	PlayerController - simple controller script.

	PlayerPickUp - part of the controller of the player. When collisions the items - pick uped them.

	PlayerUI - responsible for the correct display of the UI player.

	TestFigure - the initial position and the effect of the rotation of the test keys.



MapGenerator script work

In the script settings, you specify the player's prefabs (and whether it should be created), the generation rate (pause, which the script does between creating different objects, 0.05 proved to be the best option). 
Set list corridors (corridors can be duplicated in this list to change the ratio of corridors on the map - corridor selected randomly). 
Also script has a parameter is set up that is responsible for how many places to create locations should be maintained. If there are not enough of them, new corridors are being created. 
And then you specify a lists of locations and objects and adjust their minimum and maximum counts.

The work of the script can be divided into several stages:

	Determined the maximum number of locations and objects, set the length of the load bar in the UI.
	
	Selecting a location from the locations list, searching for a collider with a suitable tag (such colliders are part of the Locations Spawns objects, which is created by the Spawn Wall object) and creating a location in this collider. If, at the same time, there are not enough free places for location, new corridors are created containing new spawn colliders for locations.

	NavMesh create.

	Drawing up a list of places for creating objects (include places for creating doors, places for creation of keys). Mixing the list.

	Selecting an object and create an object in a suitable place.

	Clearing the map from the places of creation of objects (the tag "ObjectSpawn") and places of creation of new locations (tag "MapGenerated_Clean").

	Completion of generation, creation of the player prefab.



Process of creating new location:
https://youtu.be/JMULvSV6TPM

Tips for creating your own locations

Observe the dimension - in the test version, everything is calculated that the width of the door is 3 units, height - 2.5 units. All locations are stacked with each other as in Tetris.

Watch the turn of the locations and the places of their creation - so that the door of the location is clearly in line with the Spawn Wall objects.