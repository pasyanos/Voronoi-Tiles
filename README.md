# Procedural Terrain Manipulation Using Voronoi Areas

Caroline Pasyanos

CSCI 716: Computational Geometry

### A playable demo is available [here](https://pasyanos.github.io/Voronoi-Tiles/)!

See the User Manual Section for more information on running the demo.

## Overview

Tile based terrain generation is a common approach to procedural terrain, but a major drawback is that it creates incredibly inorganic terrain. My project is a terrain generator that still relies on tiles while using Voronoi diagrams to create a more organic terrain generator. 

## User Manual

Information needed to operate the playable demo will go here.

## Presentation Video [Here](https://www.youtube.com/watch?v=8Xx3i11P-_8)

## Background
Much of this project is based on the developer blog Tiles to Curves from independent game developer Ludomotion. They created a tool for map content generation that outputs terrain information in the form of a tile map. For my final project, I decided to extend their idea to a procedural terrain generator. 

### Tile Maps

Tile maps encode terrain information in a convenient data structure and allow for the creation of many possible maps from relatively few tiles. However, they are also very rigid. By nature they are very blocky and inorganic looking, so it is often obvious where the underlying grid is. The key idea in Ludomotion’s post is to modulate the look of tiles using a Voronoi diagram, where each tile in a tile map is mapped to an area in the Voronoi diagram.

### Voronoi Diagram

A voronoi diagram is a tessellation pattern where given points determine subdivisions of a plane into voronoi areas. Each voronoi area (also referred to as a cell) is defined such that every point enclosed in a given area is closer to the point defining that area than any other point.

## Implementation Details

Implementation Details here

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
