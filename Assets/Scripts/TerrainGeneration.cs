using System;
using System.Linq;
using UnityEngine;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine.Serialization;

public enum TRIANGLES_GENERATION_MODE
{
    Source = 0,
    InsideOut =1,
    BothSides =2
}

public class TerrainGeneration : MonoBehaviour
{
    [FormerlySerializedAs("meshFilter")]
    [Header("Dependence")]
    [SerializeField] private MeshFilter m_meshFilter;
    
    [Header("Parameters")]
    [SerializeField] private Vector3[] m_verticesArray = new Vector3[]
    {
        new Vector3(0, 0, 0),
        new Vector3(0, 0, 1),
        new Vector3(1, 0, 0),
        new Vector3(1, 0, 1)
    };
    [SerializeField] private int[] m_trianglesArray = new int[]
    {
        0, 1, 2,
        2, 1, 3
    };
    [Space(5)]
    [SerializeField] private TRIANGLES_GENERATION_MODE m_generationMode = TRIANGLES_GENERATION_MODE.Source;
    
    [FormerlySerializedAs("mesh")]
    [Header("Auto create")]
    [SerializeField] private Mesh m_mesh;

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
        
        GenerateMesh();
    }

    [Button("Generate Mesh")]
    public bool GenerateMesh()
    {
        if (IsNullWLog("GenerateMesh", m_meshFilter))
            return false;

        if (m_mesh != null)
            m_mesh.Clear();
        else
            m_mesh = new Mesh();
        
        m_meshFilter.mesh = m_mesh;

        int[] triangles;
        int i = 0;
        
        switch (m_generationMode)
        {
            case TRIANGLES_GENERATION_MODE.BothSides:
                triangles = new int[m_trianglesArray.Length * 2];
                
                for (int j = 0; j < m_trianglesArray.Length; j++)
                {
                    Debug.Log($"i={i} | j={j}");
                    triangles[i] = m_trianglesArray[j];
                    i++;
                }
                for (int j = m_trianglesArray.Length - 1; j >= 0; j--)
                {
                    Debug.Log($"i={i} | j={j}");
                    triangles[i] = m_trianglesArray[j];
                    i++;
                }

                break;
            case TRIANGLES_GENERATION_MODE.InsideOut:
                triangles = new int[m_trianglesArray.Length];
                
                for (int j = m_trianglesArray.Length - 1; j >= 0; j--)
                {
                    Debug.Log($"i={i} | j={j}");
                    triangles[i] = m_trianglesArray[j];
                    i++;
                }

                break;
            
            case TRIANGLES_GENERATION_MODE.Source:
            default:
                triangles = m_trianglesArray;
                break;
        }
        
        Debug.Log($"triangles : {m_trianglesArray.Length} | {triangles.Length}");
        
        m_mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        m_mesh.vertices = m_verticesArray;
        m_mesh.triangles = triangles;
        
        m_mesh.RecalculateBounds();
        m_mesh.RecalculateNormals();
        m_mesh.RecalculateTangents();

        return true;
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
