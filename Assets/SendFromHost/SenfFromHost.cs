using UnityEngine;

namespace MarchingCubes {

sealed class DynamicFieldVisualizer : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] Vector3Int _dimensions = new Vector3Int(64, 64, 64);
    [SerializeField] float _gridScale = 4.0f / 64;
    [SerializeField] int _triangleBudget = 1 << 16;

    #endregion

    #region Project asset references

    [SerializeField, HideInInspector]  ComputeShader _builderCompute   = null;

    #endregion

    #region Private members

    int VoxelCount => _dimensions.x * _dimensions.y * _dimensions.z;

    const float rescale = 20.0f;
    public float m_speed = 2.0f;
    float[] voxelBufferCPU;

    ComputeBuffer _voxelBuffer;
    MeshBuilder _builder;

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        voxelBufferCPU = new float[VoxelCount];

        _voxelBuffer = new ComputeBuffer(VoxelCount, sizeof(float));
        _builder = new MeshBuilder(_dimensions, _triangleBudget, _builderCompute);
    }

    void OnDestroy()
    {
        _voxelBuffer.Dispose();
        _builder.Dispose();
    }

    void Update()
    {
        // [TEMPORARY] idiotic way to compute voxel data
        float sin_value = Mathf.Sin(m_speed * Time.realtimeSinceStartup);

        for (int k = 0; k < _dimensions.z; k++)
        for (int j = 0; j < _dimensions.y; j++)
        for (int i = 0; i < _dimensions.x; i++)
        {
            const float r = 0.35f;
            float x = (float)i / (float)_dimensions.z - 0.5f;
            float y = (float)j / (float)_dimensions.y - 0.5f;
            float z = (float)k / (float)_dimensions.x - 0.5f;

            float val = r * r - (x * x + y * y + z * z) - 0.05f * sin_value;
            val = Mathf.Clamp(rescale * val, -1.0f, +1.0f);

            voxelBufferCPU[i + j * _dimensions.x + k * _dimensions.x * _dimensions.y] = val;
        }
        _voxelBuffer.SetData(voxelBufferCPU);

        // Isosurface reconstruction
        _builder.BuildIsosurface(_voxelBuffer, 0.0f, _gridScale);
        GetComponent<MeshFilter>().sharedMesh = _builder.Mesh;
    }

    #endregion
}

} // namespace MarchingCubes
