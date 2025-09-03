using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

using System.IO;

namespace MarchingCubes {

sealed class VolumeDataVisualizer : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] string filepath = "./data/CThead.bytes";
    [SerializeField] Vector3Int _dimensions = new Vector3Int(256, 256, 113);
    [SerializeField] float _gridScale = 4.0f / 256;
    [SerializeField] int _triangleBudget = 65536 * 16;

    #endregion

    #region Project asset references

    [SerializeField, HideInInspector] ComputeShader _converterCompute = null;
    [SerializeField, HideInInspector] ComputeShader _builderCompute = null;

    #endregion

    #region Target isovalue

    [CreateProperty] public float TargetValue { get; set; } = 0.4f;
    float _builtTargetValue;

    #endregion

    #region Private members

    int VoxelCount => _dimensions.x * _dimensions.y * _dimensions.z;

    ComputeBuffer _voxelBuffer;
    MeshBuilder _builder;

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        _voxelBuffer = new ComputeBuffer(VoxelCount, sizeof(float));
        _builder = new MeshBuilder(_dimensions, _triangleBudget, _builderCompute);

        // Voxel data conversion (ushort -> float)
        using var readBuffer = new ComputeBuffer(VoxelCount / 2, sizeof(uint));

        byte[] bytes = File.ReadAllBytes(filepath);

        readBuffer.SetData(bytes);

        _converterCompute.SetInts("Dims", _dimensions);
        _converterCompute.SetBuffer(0, "Source", readBuffer);
        _converterCompute.SetBuffer(0, "Voxels", _voxelBuffer);
        _converterCompute.DispatchThreads(0, _dimensions);

        // UI data source
        FindFirstObjectByType<UIDocument>().rootVisualElement.dataSource = this;
    }

    void OnDestroy()
    {
        _voxelBuffer.Dispose();
        _builder.Dispose();
    }

    void Update()
    {
        // Rebuild the isosurface only when the target value has been changed.
        if (TargetValue == _builtTargetValue) return;

        _builder.BuildIsosurface(_voxelBuffer, TargetValue, _gridScale);
        GetComponent<MeshFilter>().sharedMesh = _builder.Mesh;

        _builtTargetValue = TargetValue;
    }

    #endregion
}

} // namespace MarchingCubes
