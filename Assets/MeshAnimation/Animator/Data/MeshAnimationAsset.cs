using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 网格动画动画文件
/// </summary>
public class MeshAnimationAsset : ScriptableObject
{
    public float Speed { get { return _speed; } set { _speed = value; } }
    private float _speed = 1f;

    public int _VertexNumber { get; set; }
    public int _FrameNumber { get; set; }

    public Vector3[] _VerticesAnimationArray { get; set; }
}
