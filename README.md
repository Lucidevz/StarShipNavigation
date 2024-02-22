# ct4101---assignment-2-s4203133
ct4101---assignment-2-s4203133 created by GitHub Classroom

CITATIONS - 

3D models -

Kenney (2020) Space kit · Kenney, · Kenney. Available at: https://kenney.nl/assets/space-kit (Accessed: 19 May 2023). 

Audio - 

Kenney (2020a) Sci-Fi sounds · Kenney, · Kenney. Available at: https://kenney.nl/assets/sci-fi-sounds (Accessed: 19 May 2023). 

Kenney (2020a) Interface sounds · Kenney, · Kenney. Available at: https://kenney.nl/assets/interface-sounds (Accessed: 19 May 2023). 



HOW TO USE PROJECT -

Upon opening the application the player will be presented with a randomly generated star map. A UI on the left hand side of the screen allows options to be customised to change the details about the star map, and the genrate button can be pressed to create a new map with the changes applied.

The player can use the left mouse button to click on any star in the map, where an A* pathfinding algorithm will generate a route between the star the player's ship is currently at, and the newly selected star. A warning will show on the UI if:
The path is too long (the default maximum jump distance is 12 stars per route, including the start and end star), or the player doesn't have enough fuel to make the journey. If no route could be detected, then an error message is presented. If an acceptable route is found, then a green line appears between the stars, visualising the path, and a button will appear on the UI allowing the player to travel the selected route (where the ship is moved between each star using a custom lerp library), or cancel it to generate a new one. The UI on the right hand side displays the starting star, end star, all stars that are part of the route, and the overall path length.

As the player travels the galaxy, the fuel on their ship is decreased. This is represented by a green bar at the bottom of the screen. When the player has reached their destination, they also have the option to colonise the star they are currently at. A menu will appear, allowing the player to type in a unique name to give to that star. If the player attempts to enter a name less than 3 characters or greater than 16 characters, then the name is rejected, and a warning appears on screen.
When the player is happy with their naming choice, they can press the confirm button, colonising that star. Alternatively, they can cancel the action if they desire. When a star is colonised, there is a 50% chance of regaining 5% fuel, meaning the player can refill to carry on their journey. A flag is then created on that star, showing the player that they now own it.

Somewhere in the map will be a randomly placed pirate space volume. Any star that lies within this area is a dangerous star, and costs more to travel through. The A* pathfinding algorithm adjusts the route as to avoid going through it if possible. This will result in a longer route, but allows the ship to travel around it. If the player travels through a dangerous star, there is a 20% chance of being attacked by pirates, which can result in a loss of fuel. 
