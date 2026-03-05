using System;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.Serialization;

public enum HEIGHT_INFO_PARAMETER
{
    r,
    g,
    b,
    a,
    rgbAvrg
}

public class TerrainGeneration : MonoBehaviour
{
    [FormerlySerializedAs("meshFilter")]
    [Header("Dependence")]
    [SerializeField] private MeshFilter m_meshFilter;

    [SerializeField] private Texture2D m_heightMap;

    [Header("Parameters")]
    [SerializeField] private float m_tileSize = 1f;
    [SerializeField] private float m_heightFactor = 1f;
    [Space(5)]
    [SerializeField] private Gradient m_heightGradient;
    
    private Vector3[] m_verticesArray;
    private Vector2[] m_uvsArray;
    private Color[] m_heightColor;
    private int[] m_trianglesArray;
    [Space(5)]
    [SerializeField] private TRIANGLES_GENERATION_MODE m_generationMode = TRIANGLES_GENERATION_MODE.Source;
    [SerializeField] private HEIGHT_INFO_PARAMETER m_heightParameter = HEIGHT_INFO_PARAMETER.r;
    
    [FormerlySerializedAs("mesh")]
    [Header("Auto create")]
    [SerializeField] private Mesh m_mesh;
    
    private Color[] m_pixelsArray;
    
    private void OnValidate()
    {
        if (m_meshFilter == null)
            TryGetComponent<MeshFilter>(out m_meshFilter);
    }
    private void Start()
    {
        if (m_meshFilter == null && !TryGetComponent<MeshFilter>(out m_meshFilter))
        {
            IsNullWLog("GenerateMesh", m_meshFilter);
            return;
        }

        GenerateHeightMap();
        
        GenerateMesh();
    }

    [Button("Clear")]
    public void ClearMesh()
    {
        m_mesh.Clear();
        m_verticesArray = Array.Empty<Vector3>();
        m_uvsArray = Array.Empty<Vector2>();
        m_heightColor = Array.Empty<Color>();
        m_trianglesArray = Array.Empty<int>();
    }
    
    //[Button("Generate Mesh")]
    public bool GenerateMesh()
    {
        if (IsNullWLog("GenerateMesh", m_meshFilter))
            return false;

        if (m_mesh != null)
            m_mesh.Clear();
        else
            m_mesh = new Mesh();
        
        m_meshFilter.mesh = m_mesh;
        m_mesh.name = "Terrain Mesh";
        
        Debug.Log($"triangles : {m_trianglesArray.Length}");
        
        m_mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        m_mesh.vertices = m_verticesArray;
        m_mesh.triangles = m_trianglesArray;
        m_mesh.uv = m_uvsArray;
        m_mesh.colors = m_heightColor;
        
        m_mesh.RecalculateBounds();
        m_mesh.RecalculateNormals();
        m_mesh.RecalculateTangents();

        return true;
    }

    //[Button("Generate Terrain Info")]
    public bool GenerateHeightMap()
    {
        if (IsNullWLog("GetHeightMap", m_heightMap))
            return false;

        m_pixelsArray = m_heightMap.GetPixels();
        
        int vertexCount = m_heightMap.width * m_heightMap.height;
        int tileCount = (m_heightMap.width -1) * (m_heightMap.height - 1);
        m_verticesArray = new Vector3[vertexCount];
        m_uvsArray = new Vector2[vertexCount];
        m_heightColor = new Color[vertexCount];
        m_trianglesArray = new int[tileCount * 2 * 3];

        for (int i = 0; i < m_heightMap.height; i++)
        {
            for (int j = 0; j < m_heightMap.width; j++)
            {
                int index = Index2DTo1D(i, j);

                m_uvsArray[index] = Index2DToUV(i, j);
                m_heightColor[index] = m_heightGradient.Evaluate(GetHeight(i, j) / m_heightFactor);
                m_verticesArray[index] = Index2DToPosition(i, j);
            }
        }

        if (m_generationMode == TRIANGLES_GENERATION_MODE.BothSides)
        {
            Debug.LogWarning($"{name} at GenerateHeightMap : bothSides terrain generation is not possible, set at 'Source' by default.");
        }
        
        for (int i = 0; i < m_heightMap.height -1; i++)
        {
            for (int j = 0; j < m_heightMap.width - 1; j++)
            {
                switch (m_generationMode)
                {
                    case TRIANGLES_GENERATION_MODE.InsideOut:
                    {
                        int triIndex = (i + j * (m_heightMap.width - 1)) * 6;
                        
                        m_trianglesArray[triIndex] = Index2DTo1D(i+1, j);
                        m_trianglesArray[triIndex + 1] = Index2DTo1D(i, j+1);
                        m_trianglesArray[triIndex + 2] = Index2DTo1D(i, j);
                        m_trianglesArray[triIndex + 3] = Index2DTo1D(i+1, j+1);
                        m_trianglesArray[triIndex + 4] = Index2DTo1D(i, j+1);
                        m_trianglesArray[triIndex + 5] = Index2DTo1D(i+1, j);

                        break;
                    }

                    case TRIANGLES_GENERATION_MODE.BothSides:
                    case TRIANGLES_GENERATION_MODE.Source:
                    default:
                    {
                        int triIndex = (i + j * (m_heightMap.width - 1)) * 6;

                        m_trianglesArray[triIndex] = Index2DTo1D(i, j);
                        m_trianglesArray[triIndex + 1] = Index2DTo1D(i, j+1);
                        m_trianglesArray[triIndex + 2] = Index2DTo1D(i+1, j);
                        m_trianglesArray[triIndex + 3] = Index2DTo1D(i+1, j);
                        m_trianglesArray[triIndex + 4] = Index2DTo1D(i, j+1);
                        m_trianglesArray[triIndex + 5] = Index2DTo1D(i+1, j+1);

                        break;
                    }
                }
            }
        }

        return true;
    }
    
    [Button("Generate Terrain")]
    public void GenerateTerrain()
    {
        GenerateHeightMap();
        GenerateMesh();
    }

    public int Index2DTo1D(Vector2Int index)
    {
        return Index2DTo1D(index.x, index.y);
    }
    public int Index2DTo1D(int i, int j)
    {
        return i + j * m_heightMap.width;
    }

    public Vector3 Index2DToPosition(Vector2Int index)
    {
        return Index2DToPosition(index.x, index.y);
    }
    public Vector3 Index2DToPosition(int i, int j)
    {
        return new Vector3(i * m_tileSize, GetHeight(i, j), j * m_tileSize);
    }

    public Vector2 Index2DToUV(Vector2Int index)
    {
        return Index2DToUV(index.x, index.y);
    }
    public Vector2 Index2DToUV(int i, int j)
    {
        return new Vector2(i / (m_heightMap.height - 1), j / (m_heightMap.width - 1));
    }

    public float GetHeight(Vector2Int index)
    {
        return GetHeight(index.x, index.y);
    }
    public float GetHeight(int i, int j)
    {
        Color pixel = m_pixelsArray[Index2DTo1D(i, j)];
        
        switch (m_heightParameter)
        {
            case HEIGHT_INFO_PARAMETER.r:
                return pixel.r * m_heightFactor;
                break;
            case HEIGHT_INFO_PARAMETER.g:
                return pixel.g * m_heightFactor;
                break;
            case HEIGHT_INFO_PARAMETER.b:
                return pixel.b * m_heightFactor;
                break;
            case HEIGHT_INFO_PARAMETER.a:
                return pixel.a * m_heightFactor;
                break;
            case HEIGHT_INFO_PARAMETER.rgbAvrg:
                return ((pixel.r + pixel.g + pixel.b) / 3) * m_heightFactor;
                break;
            
            default:
                return 0;
        }
    }
    
    public bool IsNullWLog<T>(string a_methodName, T a_testedClass)
    {
        if (m_meshFilter == null)
        {
            Debug.LogError($"{name} at {a_methodName}: {nameof(a_testedClass)} is missing or null.");
            return true;
        }

        return false;
    }
}
