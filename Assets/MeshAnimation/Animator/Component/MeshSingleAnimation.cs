﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 播放单一动画的控制器
/// </summary>
public class MeshSingleAnimation : MonoBehaviour, IAnimation
{

    public MeshAnimationAsset MeshAnimation;

    [SerializeField] private bool loop = false;
    public bool Loop { get { return loop; } set { loop = value; } }

    [SerializeField] private bool playOnAwake = true;
    public bool PlayOnAwake { get { return playOnAwake; } set { playOnAwake = value; } }

    [SerializeField] private float speed = 1;
    public float Speed { get { return speed; } set { speed = value; } }

    public Action<string> AnimationFinished { get; set; }

    private int _AnimationIndex = 0;
    private int _AnimationLastIndex = -1;
    private float _AnimationPlayControl = 0;
    private Vector3[] _aniFragment;
    private List<Vector3> pointPos = new List<Vector3>();
    private int _framesInSecond = 60;
    private AnimationState _aniState = AnimationState.Stop;

    private Mesh _mesh, _newMesh;
    private MeshFilter mf;


    private void Awake()
    {
        mf = transform.GetComponent<MeshFilter>();
        if (mf == null)
        {
            Debug.LogError("MeshFilter is null");
            return;
        }
        _mesh = mf.mesh;
        _newMesh = new Mesh();
        _newMesh.name = _mesh.name + "(Clone)";
        _newMesh.vertices = _mesh.vertices;
        _newMesh.normals = _mesh.normals;
        _newMesh.uv = _mesh.uv;
        _newMesh.triangles = _mesh.triangles;
        mf.mesh = _newMesh;
    }

    private void OnEnable()
    {
        if (playOnAwake)
            Play();
    }

    private void FixedUpdate()
    {
        if (_aniState == AnimationState.Playing)
        {
            Playing();
        }
    }

    public void Play()
    {
        if (MeshAnimation == null)
        {
            Debug.LogError("MeshAnimation is null");
            return;
        }
        _AnimationIndex = 0;
        _AnimationLastIndex = -1;
        _aniFragment = new Vector3[MeshAnimation._VertexNumber];
        _AnimationPlayControl = 0;
        SetVertexFrame(0);
        _aniState = AnimationState.Playing;
    }

    private void Playing()
    {
        if (MeshAnimation == null)
        {
            Debug.LogError("MeshAnimation is null");
            return;
        }

        //播放到最后一帧，循环则重新播放，不循环则停止
        if (_AnimationIndex == MeshAnimation._FrameNumber - 1)
        {
            _aniState = AnimationState.Stop;
            _AnimationIndex = 0;
            _AnimationLastIndex = -1;
            Finished();
            if (loop)
            {
                Play();
            }
            else
            {
                print("播放结束");
                return;
            }
        }

        _AnimationLastIndex = _AnimationIndex;
        _aniFragment = new Vector3[MeshAnimation._VertexNumber];

        if (_AnimationIndex == _AnimationLastIndex)
        {
            _AnimationLastIndex = _AnimationIndex;
            for (int i = 0; i < MeshAnimation._VertexNumber; i++)
            {
                _aniFragment[i] = (MeshAnimation._VerticesAnimationArray[(_AnimationIndex + 1) * MeshAnimation._VertexNumber + i] - _newMesh.vertices[i]) / _framesInSecond * speed;
            }
            //SetVertexFrame(_AnimationIndex + 1);
        }

        for (int i = 0; i < MeshAnimation._VertexNumber; i++)
        {
            pointPos[i] += _aniFragment[i];
        }
        _AnimationPlayControl += 1;
        if (_AnimationPlayControl >= _framesInSecond / speed)
        {
            _AnimationPlayControl = 0;
            _AnimationIndex += 1;
        }
        _newMesh.vertices = pointPos.ToArray();
        _newMesh.RecalculateNormals();

    }

    private void SetVertexFrame(int frame)
    {
        pointPos.Clear();
        for (int i = 0; i < _mesh.vertexCount; i++)
        {
            pointPos.Add(MeshAnimation._VerticesAnimationArray[frame * MeshAnimation._VertexNumber + i]);
        }
        _newMesh.vertices = pointPos.ToArray();
        _newMesh.RecalculateNormals();

    }

    public void Pause()
    {
        _aniState = AnimationState.Pause;
    }

    public void Stop()
    {
        _aniState = AnimationState.Stop;
    }

    private void RefreshData()
    {
        _AnimationIndex = 0;
        _AnimationLastIndex = -1;
        _AnimationPlayControl = 0;
    }

    private void Finished()
    {
        AnimationFinished?.Invoke(MeshAnimation.name);
    }
    
}
