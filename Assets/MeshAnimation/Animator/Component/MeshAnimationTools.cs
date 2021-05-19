using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR

/// <summary>
/// 网格动画编辑工具
/// </summary>
[ExecuteInEditMode, DisallowMultipleComponent, AddComponentMenu("MeshAnimation/模型网格动画编辑器")]
public class MeshAnimationTools : MonoBehaviour
{

    public MeshFilter _MeshFilter { get; private set; }


    public AnimationState _AnimState { get { return animState; } set { animState = value; } }
    private AnimationState animState = AnimationState.Stop;

    [SerializeField, Header("实时根据顶点小球刷新网格")] public bool UpdateVertices = true;
    [SerializeField, Header("每帧间隔")] public float _frameInterval = 1f;
    //顶点尺寸
    [SerializeField, Header("网格顶点尺寸"), Range(0, 1)] public float _VertexSize = 0.1f;
    //顶点尺寸缓存
    [System.NonSerialized] public float _LastVertexSize;
    //所有重复顶点集合
    [System.NonSerialized] public List<List<int>> _AllVerticesGroupList;
    //所有顶点集合
    [System.NonSerialized] public List<Vector3> _AllVerticesList;
    //筛选后的顶点集合
    [System.NonSerialized] public List<Vector3> _VerticesList;
    //需要移除的顶点集合
    [System.NonSerialized] public List<int> _VerticesRemoveList;
    //用于筛选的顶点集合
    [System.NonSerialized] public List<int> _VerticesSubList;
    //顶点圆球数量
    [System.NonSerialized] public int _VerticesObjNum = 0;
    //顶点数量
    [System.NonSerialized] public int _VerticesNum = 0;
    //顶点物体集合
    [System.NonSerialized] public GameObject[] _VerticesObjArray;
    //目标物体
    [System.NonSerialized] public GameObject _target;
    //网格
    [System.NonSerialized] public Mesh _Mesh;

    private void Awake()
    {
        _MeshFilter = transform.GetComponent<MeshFilter>();
        if (_MeshFilter == null)
        {
            EditorLog.EditorWindowLog("物体缺少 MeshFilter 组件！");
            DestroyImmediate(GetComponent<MeshAnimationTools>());
            return;
        }
        if (GetComponent<MeshRenderer>() == null)
        {
            EditorLog.EditorWindowLog("物体缺少 MeshRender 组件！");
            DestroyImmediate(GetComponent<MeshAnimationTools>());
            return;
        }
        Init();
    }

    
    private void Init()
    {
        _AllVerticesGroupList = new List<List<int>>();
        _AllVerticesList = new List<Vector3>(_MeshFilter.sharedMesh.vertices);
        _VerticesList = new List<Vector3>(_MeshFilter.sharedMesh.vertices);
        _VerticesRemoveList = new List<int>();

        //循环遍历并记录重复顶点
        for (int i = 0; i < _VerticesList.Count; i++)
        {
            EditorUtility.DisplayProgressBar("识别顶点", "正在识别顶点（" + i + "/" + _VerticesList.Count + "）......", 1.0f / _VerticesList.Count * i);
            //已存在于删除集合的顶点不计算在内
            if (_VerticesRemoveList.IndexOf(i) >= 0)
                continue;

            _VerticesSubList = new List<int>();
            _VerticesSubList.Add(i);
            int j = i + 1;
            //如果发现重复顶点，将之记录在内，并加入待删除集合
            while (j < _VerticesList.Count)
            {
                if (_VerticesList[i] == _VerticesList[j])
                {
                    _VerticesSubList.Add(j);
                    _VerticesRemoveList.Add(j);
                }
                j++;
            }
            //记录重复顶点集合
            _AllVerticesGroupList.Add(_VerticesSubList);
        }

        //整理待删除集合
        _VerticesRemoveList.Sort();
        //删除重复顶点
        for (int i = _VerticesRemoveList.Count - 1; i >= 0; i--)
        {
            _VerticesList.RemoveAt(_VerticesRemoveList[i]);
        }
        _VerticesRemoveList.Clear();


        #region 创建顶点
        _VerticesObjNum = _VerticesList.Count;
        _VerticesNum = _VerticesObjNum;
        //创建顶点，应用顶点尺寸，顶点位置为删除完重复顶点之后的集合位置
        _VerticesObjArray = new GameObject[_VerticesNum];
        for (int i = 0; i < _VerticesNum; i++)
        {
            EditorUtility.DisplayProgressBar("创建顶点", "正在创建顶点（" + i + "/" + _VerticesNum + "）......", 1.0f / _VerticesNum * i);
            _VerticesObjArray[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _VerticesObjArray[i].name = "Vertex " + i;
            _VerticesObjArray[i].transform.localScale = new Vector3(_VertexSize, _VertexSize, _VertexSize);
            _VerticesObjArray[i].transform.position = transform.localToWorldMatrix.MultiplyPoint3x4(_VerticesList[i]);
            _VerticesObjArray[i].transform.SetParent(transform);
        }
        _LastVertexSize = _VertexSize;
        #endregion

        #region 重构网格
        Transform trans = transform.Find(transform.name + "(Clone)");
        if (trans != null)
            DestroyImmediate(trans.gameObject);
        trans = null;

        _target = new GameObject(transform.name + "(Clone)");
        _target.transform.position = transform.position;
        _target.transform.rotation = transform.rotation;
        _target.transform.localScale = transform.localScale;
        _target.transform.SetParent(transform);

        _target.AddComponent<MeshFilter>();
        _target.AddComponent<MeshRenderer>();
        _target.GetComponent<MeshRenderer>().sharedMaterial = GetComponent<MeshRenderer>().sharedMaterial;

        _Mesh = new Mesh();
        _Mesh.Clear();
        _Mesh.vertices = _AllVerticesList.ToArray();
        _Mesh.triangles = _MeshFilter.sharedMesh.triangles;
        _Mesh.uv = _MeshFilter.sharedMesh.uv;
        _Mesh.name = "克隆体" + transform.name;
        _Mesh.RecalculateNormals();
        _target.GetComponent<MeshFilter>().sharedMesh = _Mesh;

        GetComponent<MeshRenderer>().enabled = false;
        EditorUtility.ClearProgressBar();
        #endregion
    }

    private void Update()
    {
        if (UpdateVertices)
            RefreshMesh();
        else
            RefreshVertexObj();
        if (_LastVertexSize != _VertexSize)
            RefreshVertexSize();
    }

    //刷新网格
    public void RefreshMesh()
    {
        if (_Mesh != null)
        {
            //重新应用并设置
            for (int i = 0; i < _VerticesObjArray.Length; i++)
            {
                for (int j = 0; j < _AllVerticesGroupList[i].Count; j++)
                {
                    _AllVerticesList[_AllVerticesGroupList[i][j]] = _target.transform.worldToLocalMatrix.MultiplyPoint3x4(_VerticesObjArray[i].transform.position);
                }
            }
            //刷新网格
            _Mesh.vertices = _AllVerticesList.ToArray();
            _Mesh.RecalculateNormals();
        }
    }

    //刷新顶点小球位置
    public void RefreshVertexObj()
    {
        for (int i = 0; i < _VerticesObjArray.Length; i++)
        {
            for (int j = 0; j < _AllVerticesGroupList[i].Count; j++)
            {
                _VerticesObjArray[i].transform.position = _target.transform.localToWorldMatrix.MultiplyPoint3x4(_AllVerticesList[_AllVerticesGroupList[i][j]]);
            }
        }
    }

    //刷新顶点大小
    private void RefreshVertexSize()
    {
        if (_VerticesObjArray.Length > 0)
        {
            if (_LastVertexSize != _VertexSize)
            {
                for (int i = 0; i < _VerticesObjArray.Length; i++)
                {
                    _VerticesObjArray[i].transform.localScale = new Vector3(_VertexSize, _VertexSize, _VertexSize);
                }
                _LastVertexSize = _VertexSize;
            }
        }
    }

    //根据顶点数据匹配顶点小球位置
    public void RefreshVertexObjWitchVertices(Vector3[] vertices)
    {
        for (int i = 0; i < _VerticesObjNum; i++)
        {
            _VerticesObjArray[i].transform.position = _target.transform.localToWorldMatrix.MultiplyPoint3x4(vertices[i]);
        }

    }

    //编辑结束
    private void OnDestroy()
    {
        for (int i = 0; i < _VerticesNum; i++)
        {
            EditorUtility.DisplayCancelableProgressBar("应用顶点", "正在应用顶点（" + i + "/" + _VerticesNum + "）......", 1.0f / _VerticesNum * i);
            if (_VerticesObjArray[i] != null)
                DestroyImmediate(_VerticesObjArray[i]);
        }
        EditorUtility.ClearProgressBar();
    }

}

#endif

public enum AnimationState
{
    Stop,
    Playing,
    Pause
}
