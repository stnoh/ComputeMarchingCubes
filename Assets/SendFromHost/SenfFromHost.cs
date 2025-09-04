using UnityEngine;

using System.Threading.Tasks;

namespace MarchingCubes {

sealed class DynamicFieldVisualizer : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] Vector3Int _dimensions = new Vector3Int(64, 64, 64);
    [SerializeField] float _gridScale = 4.0f / 64;
    [SerializeField] int _triangleBudget = 1 << 16;

    #endregion

    #region Project asset references

    [SerializeField, HideInInspector] ComputeShader _builderCompute   = null;

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
        // compute voxel data from host (CPU)
        float sin_value = Mathf.Sin(m_speed * Time.realtimeSinceStartup);

        // use Parallel.For for speedup
        var result = Parallel.For(0, VoxelCount, (n, state) => 
        {
            int k = n / (_dimensions.x * _dimensions.y);
            int j = (n - k * _dimensions.x * _dimensions.y) / _dimensions.x;
            int i = (n - k * _dimensions.x * _dimensions.y) % _dimensions.x;

            const float r = 0.35f;
            float x = (float)i / (float)_dimensions.z - 0.5f;
            float y = (float)j / (float)_dimensions.y - 0.5f;
            float z = (float)k / (float)_dimensions.x - 0.5f;

            float val = r * r - (x * x + y * y + z * z) - 0.05f * sin_value;
            val = Mathf.Clamp(rescale * val, -1.0f, +1.0f);

            voxelBufferCPU[n] = val;
        });
        _voxelBuffer.SetData(voxelBufferCPU);

        // Isosurface reconstruction
        _builder.BuildIsosurface(_voxelBuffer, 0.0f, _gridScale);
        GetComponent<MeshFilter>().sharedMesh = _builder.Mesh;
    }

    #endregion
}

} // namespace MarchingCubes
