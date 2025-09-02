ComputeMarchingCubes
====================

This is the modified version of [ComputeMarchingCubes](https://github.com/keijiro/ComputeMarchingCubes) by [Keijiro Takahashi](https://github.com/keijiro/).  
The original version is great to demostrate MarchingCubes algorithm with Unity's new Mesh API and ComputeShader.  
The problem is that it includes URP with shader graph which expands the project size about to GBs (in my case, it reached 1.5GB).  
Although the final rendering quality becomes poorer, I manually removed all related parts from the repository and reverted to built-in pipeline.  
