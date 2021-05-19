using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(MeshAnimationTools))]
public class MeshAnimatorEditor : Editor
{
    private MeshAnimationTools _meshAnimatorTools;
    private Transform _transform;


    #region 模型相关属性
    private int _vertexNumber = 0;
    private Mesh _mesh;
    #endregion

    #region 编辑工具属性



    //非公开属性
    private int _maxVertexNumver = 10000;//最大处理顶点数
    private static List<MeshFrame> _framesList = new List<MeshFrame>();//帧队列
    private static MeshFrame _currentSelectFrame = null;//当前选择的帧
    private int _aniFrameIndex = 0;//动画播放帧数
    private int _aniFrameLastIndex = -1;//上一帧播放的序列
    private Vector3[] _aniFragment;//动画片段
    private float _AnimationPlayControl;//动画控制器
    #endregion

    Vector2 _frameSCVPos;
    private void OnEnable()
    {
        _meshAnimatorTools = target as MeshAnimationTools;
        _transform = _meshAnimatorTools.transform;
        _mesh = _meshAnimatorTools._Mesh;
        _vertexNumber = _meshAnimatorTools._VerticesNum;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Frame"))
        {
            AddFrame();
        }
        if (GUILayout.Button("Delegate Frame"))
        {
            DeleFrame();
        }
        if (GUILayout.Button("Apply"))
        {
            ApplyFrame();
        }
        GUILayout.EndHorizontal();
        if (_framesList.Count > 0)
        {
            if (_currentSelectFrame != null)
            {
                GUILayout.Label("当前选择帧: Ani Frame " + _currentSelectFrame._SelfIndex);
            }
            else
            {
                GUILayout.Label("当前选择帧: NULL ");
            }
            _frameSCVPos = GUILayout.BeginScrollView(_frameSCVPos, false, true, GUILayout.Height(100));
            for (int i = 0; i < _framesList.Count; i++)
            {
                if (GUILayout.Button("Ani Frame " + i))
                {
                    SelectFrame(i);
                }
            }

            GUILayout.EndScrollView();

        }
        else
            _currentSelectFrame = null;

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Play") && _meshAnimatorTools._AnimState != AnimationState.Playing)
            PlayAni();
        if (GUILayout.Button("Pause") && _meshAnimatorTools._AnimState == AnimationState.Playing)
            PauseAni();
        if (GUILayout.Button("Stop") && _meshAnimatorTools._AnimState == AnimationState.Playing)
            StopAni();
        if (GUILayout.Button("Export Anim"))
            ExportAni();
        GUILayout.EndHorizontal();
    }

    private void OnSceneGUI()
    {


    }

    //添加帧
    private void AddFrame()
    {
        //int index = _currentSelectFrame == null ? _framesList.Count : _currentSelectFrame._SelfIndex + 1;
        int index = _framesList.Count;
        MeshFrame newFrame = new MeshFrame(_mesh.vertices, index);
        //_framesList.Insert(index, newFrame);
        _framesList.Add(newFrame);
        _currentSelectFrame = newFrame;
    }

    //删除帧
    private void DeleFrame()
    {
        if (_framesList.Count <= 0)
            return;
        int index = _currentSelectFrame == null ? _framesList.Count - 1 : _currentSelectFrame._SelfIndex;
        _framesList.RemoveAt(index);
        //重置后面的帧的index
        for (int i = index; i < _framesList.Count; i++)
        {
            _framesList[i]._SelfIndex = i;
        }
        if (_framesList.Count <= 0)
            _currentSelectFrame = null;
        else
            _currentSelectFrame = index < _framesList.Count ? _framesList[index] : _framesList[_framesList.Count - 1];
    }

    //选择帧
    private void SelectFrame(int i)
    {
        _currentSelectFrame = _framesList[i];
        _meshAnimatorTools.RefreshVertexObjWitchVertices(_currentSelectFrame.Vertices);
        _meshAnimatorTools.RefreshMesh();

    }

    //应用当前帧
    private void ApplyFrame()
    {
        if (_currentSelectFrame == null)
            return;
        _currentSelectFrame.Vertices = _mesh.vertices;
    }

    //播放动画
    private void PlayAni()
    {
        //动画帧数为空
        if (_framesList.Count <= 0)
        {
            return;
        }
        if (_meshAnimatorTools._AnimState == AnimationState.Stop)
        {
            //播放从第一帧开始
            _aniFrameIndex = 0;
            //重置上一帧序列
            _aniFrameLastIndex = -1;
            //重建新的动画片段
            _aniFragment = new Vector3[_vertexNumber];
            //重置动画控制器
            _AnimationPlayControl = 0;
            //动画进入第一帧
            //_mesh.vertices = _framesList[0].Vertices;
            _meshAnimatorTools.RefreshVertexObjWitchVertices(_framesList[0].Vertices);
        }

        _meshAnimatorTools._AnimState = AnimationState.Playing;
        EditorApplication.update += PlayingAni;
    }

    private void PlayingAni()
    {
        if (_meshAnimatorTools._AnimState != AnimationState.Playing)
        {
            return;
        }
        //播放到最后一帧，动画播放完毕
        if (_aniFrameIndex >= _framesList.Count - 1)
        {
            _meshAnimatorTools._AnimState = AnimationState.Stop;
            //清除刷新动画函数
            EditorApplication.update -= PlayingAni;
            //动画回到第一帧
            //_mesh.vertices = _framesList[0].Vertices;
            _meshAnimatorTools.RefreshVertexObjWitchVertices(_framesList[0].Vertices);
            return;
        }
        //当前动画播放不等于上一帧，进入下一帧
        if (_aniFrameIndex != _aniFrameLastIndex)
        {
            _aniFrameLastIndex = _aniFrameIndex;
            //分隔动画
            for (int i = 0; i < _aniFragment.Length; i++)
            {
                //每个顶点需要移动的方向和距离
                _aniFragment[i] = (_framesList[_aniFrameIndex + 1].Vertices[i] - _framesList[_aniFrameIndex].Vertices[i]) / _meshAnimatorTools._frameInterval;
            }
        }
        //动画播放中
        for (int i = 0; i < _meshAnimatorTools._VerticesObjNum; i++)
        {
            _meshAnimatorTools._VerticesObjArray[i].transform.position += _aniFragment[i];
        }
        //动画控制器计数
        _AnimationPlayControl += 1;
        if (_AnimationPlayControl >= _meshAnimatorTools._frameInterval)
        {
            _AnimationPlayControl = 0;
            _aniFrameIndex += 1;
        }
        _meshAnimatorTools.RefreshMesh();
    }


    //暂停动画
    private void PauseAni()
    {
        _meshAnimatorTools._AnimState = AnimationState.Pause;
        EditorApplication.update -= PlayingAni;
        _currentSelectFrame = _framesList[_aniFrameLastIndex];
    }

    //停止动画
    private void StopAni()
    {
        _meshAnimatorTools._AnimState = AnimationState.Stop;
        EditorApplication.update -= PlayingAni;
        _meshAnimatorTools.RefreshVertexObjWitchVertices(_framesList[0].Vertices);
    }

    //导出成文件
    private void ExportAni()
    {
        //动画帧数小于等于1不允许导出
        if (_framesList.Count <= 1)
            return;

        //创建动画数据文件
        MeshAnimationAsset asset = CreateInstance<MeshAnimationAsset>();
        //记录动画顶点数
        asset._VertexNumber = _mesh.vertexCount;
        //记录动画帧数
        asset._FrameNumber = _framesList.Count;
        //记录动画帧数据
        asset._VerticesAnimationArray = new Vector3[_framesList.Count * _mesh.vertexCount];
        for (int i = 0; i < _framesList.Count; i++)
        {
            for (int j = 0; j < _framesList[i].Vertices.Length; j++)
            {
                asset._VerticesAnimationArray[i * asset._VertexNumber + j] = _framesList[i].Vertices[j];
                EditorUtility.DisplayProgressBar("导出动画", string.Format("正在到处顶点数据：{0}/{1}", i * asset._VertexNumber + j, _framesList.Count * asset._VertexNumber), i * asset._VertexNumber + j / _framesList.Count * asset._VertexNumber);
            }
        }
        Debug.Log(asset._VertexNumber);
        Debug.Log(asset._FrameNumber);
        Debug.Log(asset._VerticesAnimationArray.Length);

        //创建本地文件
        //string path = "Assets/" + _meshAnimatorTools._MeshFilter.sharedMesh.name + "AnimationData.asset";
        //AssetDatabase.CreateAsset(asset, path);
        AssetsCreateEditor.CreateAnimationAsset(asset);

        EditorUtility.ClearProgressBar();
    }

}



