# DESCRIPTION
_____________________________________________________

DMICustomDrawCall is a open source tool for using DrawMeshInstanced with a custom culling method. It also comes with a fully jobified and burst compatible system to improve performances.



## Prerequisites
_____________________________________________________

- Burst 1.4.11
- Unity mathematics 1.2.5

Above packages are required for the job system to work but the base script can be modified to work without a job system.


## Documentation
_____________________________________________________

.                     | DrawMeshInstanced  | BatchRendererGroup             | DMICustomDrawCall
 -------------        | -------------      | -------------                  | -------------
Culling               |  &cross;           | &check;                        | &check;
MaterialPropertyBlock |  &check;           | &cross; (Limited use case)     | &check;

<br />

The DMICustomCulling script attempts to combine the best of both DrawMeshInstanced and BatchRendererGroup. A traditional DrawMeshInstanced method will perpetually draw all the meshes even if not all meshes are visible on the screen, which can lead to unnecessary vert and tris count in the scene. A traditional BatchRendererGroup removes this issue by implementing a custom culling method but faces limitations such as requiring a shader graph to work if MaterialPropertyBlock is required.


By combining the advantages of both of these systems, this tool removes all limitations and will therefore be compatible in all projects. This tool comes with the following:

- One jobified system to manage the culling
- One jobified system to manage the meshes' position, scale and rotation
- Material property block to individually adjust a mesh's colour





## Showcase
_____________________________________________________

![](https://github.com/klazapp/DMICustomDrawCall-Jobified-/blob/main/Assets/GifShowCase/Showcase-1-Bounce.gif)


This gif shows that the the meshes are being culled as the number of vertices increase and decrease based on the number of meshes on screen.



## TODO
_____________________________________________________

- Compare jobified and non jobified system to determine performance improvements
- Extend system to DrawMeshInstancedIndirect