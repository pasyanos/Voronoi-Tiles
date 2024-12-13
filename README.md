# Procedural Terrain Manipulation Using Voronoi Areas

Caroline Pasyanos

CSCI 716: Computational Geometry

### A playable demo is available [here](https://pasyanos.github.io/Voronoi-Tiles/)!

See the User Manual Section for more information on running the demo.

## Overview

Tile based terrain generation is a common approach to procedural terrain, but a major drawback is that it creates incredibly inorganic terrain. My project is a terrain generator that still relies on tiles while using Voronoi diagrams to create a more organic terrain generator. 

## User Manual

The goal was to expose the most impactful settings to the user to illustrate the flexibility of the system. 

## Presentation Video [Here](https://www.youtube.com/watch?v=8Xx3i11P-_8)

## Background
Much of this project is based on the developer blog Tiles to Curves from independent game developer Ludomotion. They created a tool for map content generation that outputs terrain information in the form of a tile map. For my final project, I decided to extend their idea to a procedural terrain generator. 

### Tile Maps

Tile maps encode terrain information in a convenient data structure and allow for the creation of many possible maps from relatively few tiles. However, they are also very rigid. By nature they are very blocky and inorganic looking, so it is often obvious where the underlying grid is. The key idea in Ludomotion’s post is to modulate the look of tiles using a Voronoi diagram, where each tile in a tile map is mapped to an area in the Voronoi diagram.

### Voronoi Diagrams

A voronoi diagram is a tessellation pattern where given points determine subdivisions of a plane into voronoi areas. Each voronoi area (also referred to as a cell) is defined such that every point enclosed in a given area is closer to the point defining that area than any other point.

## Implementation Details

### Step 1: Tile Map Generation

Note: Some settings are able to be tweaked in the demo. For more information, refer to the User Manual section above.

The tile generator creates a tile map, a 2D grid of tiles. Each tile is one of the following types of terrain:
Water
Shore
Ground
low hill
high mountain

The color and height of each terrain is customizable in the Unity editor, but these settings are not exposed in the demo. 
The mesh generation algorithm uses both the color and height settings. It relies on each subsequent terrain type to have a height higher than the previous type.

Terrain types are stored in a 2D array.

Before beginning generation, the algorithm creates a 2D look-up table of float values with the same dimensions as the tile grid. For both the x and y value, a value is interpolated from a minimum to a maximum value based on how close the value is to the map’s center. 
Rather than a linear interpolation, I make use of a [Unity animation curve](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/AnimationCurve.html). The values for the x and y axis are multiplied together then stored in the lookup table.
Like the terrain type setting, the animation curve is exposed in the Unity editor, but cannot be changed in the build.

First, each edge tile is filled in with water. The array has the dimensions rows by columns. Number of rows and columns can be set in the demo. 

For each internal tile, I sample a 2D Perlin noise texture to get a value in the range [0,1], then weigh it by the value stored in the look-up table. I convert this value to an integer in the range [0, 100]. Anything below 7 is a water tile. The range [7, 30) is a ground tile, [30, 45) is low hill, and anything 45 or above is a high mountain.

In a second pass,  I go through and change all ground tiles that have at least one water neighbor to shore tiles to give it even more of an island look. Hill or mountain tiles that border water will remain, to mimic the look of islands that have cliff edges, but it is unlikely.

### Step 2: Tile Center Manipulation

The key to creating organic looking terrain is seeding a 2D plane with points for each tile, then generating a Voronoi area. Augmenting the tile centers before generating the diagram is an incredibly necessary step for achieving organic maps. Without these augmentations, the resulting Voronoi diagram would just be a grid of uniform shapes.
I initialize a 2D array of Vector2s with the dimensions rows x columns representing the offset from the center of the tile. The default is (0, 0) - no offset.
Following Ludomotion’s post, tile centers are augmented in three ways.

1. Offsetting even columns

An optional toggle offsets the points in every other column by half of the height of a tile. This can be toggled on or off in the demo build.

2. Randomize points

The tile center is randomized by a small amount in the x and y direction, again using Perlin noise. This can be adjusted in the demo build.

3. Relax towards like neighbors

Finally, the center of the tile is moved towards orthogonal neighbors of the same type, and away from orthogonal neighbors of different types. The amount of movement can also be adjusted in the demo build.

### Step 3: Generating Voronoi Diagrams

In order to focus  more fully on procedural terrain generation and creating meshes, I use an external package to generate a Voronoi diagram. The package, [Unity-Delauney](https://github.com/jceipek/Unity-delaunay/tree/master) is unfortunately no longer maintained, but still works in the modern version of Unity I used. 
The package contains a C#/Unity port of Fortune's Algorithm. Fortune's Algorithm is a plane sweep approach to generating Voronoi areas. 

I flatten the 2D array of points into a 1D array, then pass it to the Voronoi generator. 

Once it is run, each Voronoi area in the diagram can be queried by the site's point, and is stored as a collection of points, which will be used in the mesh generation step.

### Step 4: Mesh Generation

Mesh generation proceeds as followws:

pseudocode here.

## Tools

- Unity Engine
- WebGL
- C#

## Project History

- 10/27/24 - Basic tile generation with placeholder assets finalized. First Github pages build published.
- 11/16/24 - Voronoi area generation added.
- 12/4/2024 - Mesh generation using Voronoi areas. Build updated.
- 12/7/2024 - UI updates, build updated.
- 12/11-12/12/2024 - Added UI hooks to demo to allow adjustment of parameters. Final build.

## References

This list may grow as the project continues.

- [Ludomotion: Tiles to Curves](https://www.ludomotion.com/blogs/tiles-to-curves/)
- [Ludomotion: Generating World Maps for Unexplored 2](https://www.ludomotion.com/blogs/generating-world-maps/)
- [Unity-delauney](https://github.com/jceipek/Unity-delaunay/tree/master)
- [How To Host Unity a WebGL Game on Github for Free](https://www.youtube.com/watch?v=4jvGgn4b1V8)
- [Creating a Mesh](https://catlikecoding.com/unity/tutorials/procedural-meshes/creating-a-mesh/)
- [Voronoi Diagrams](https://en.wikipedia.org/wiki/Voronoi_diagram)
- [The Fascinating World of Voronoi Diagrams](https://builtin.com/data-science/voronoi-diagram)
- [Fortune's Algorithm: An Intuitive Explanation](https://jacquesheunis.com/post/fortunes-algorithm/)
- [Perlin Noise](https://en.wikipedia.org/wiki/Perlin_noise)
