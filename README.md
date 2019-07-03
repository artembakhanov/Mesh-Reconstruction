# Mesh-Reconstruction

This project is an attempt of using ARCore Toolkit for reconstructing 3D mesh.
We are using IPD algorithm that is described [here](https://www.sciencedirect.com/science/article/pii/S0010448503000642).

The algorithm is slow and requires some optimizations. Since the cloud of points that arcore gives does not always resemble the outlines of the real world, the algorithm may produce unexpected shapes. The algorithm was tested on simple geometric shapes (with a high density of points) . Because of this, it is recommended to scan smoother surfaces and objects. The project is under development.
## Get started
It is highly recommended to use Unity 2019.03 (or higher).
The following packages are required:
- **AR Foundation** (Version 2.0.2)
- **ARCore XR Plugin** (Version 2.0.2)

Minimum API level must be **Android 7.0 (Nougat)**.
Multithreaded rendering must be disabled.

The code is inside **IPDMeshCreator** class. 
To use it you need to create an instance of **VoxelSet** and add points with `voxelSet.AddPoint(...)`.
To get triangles of reconstructed mesh call `IPDMeshCreator.ComputeMeshTriangles()`.
## Credits
The [code](http://fileadmin.cs.lth.se/cs/Personal/Tomas_Akenine-Moller/code/tritri_isectline.txt) of Tomas Akenine-Möller was used. This code is used for triangles intersection testing.

## Info
The code is written by Artem Bahanov (except triangle intersection test of Tomas Akenine-Möller that was rewritten from C to C#).