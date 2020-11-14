using Sark.Terminals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetNormalsTangents : MonoBehaviour
{
    RenderFromCode r;

    Vector4[] _tangents;
    Vector3[] _normals;

    private void Awake()
    {
        r = GetComponent<RenderFromCode>();
    }

    private void Update()
    {
        if(Input.GetKey(KeyCode.Space))
        {
            var m = r._term.Mesh;
            _tangents = m.tangents;
            _normals = m.normals;
        }
    }

    private void OnGUI()
    {
        if (_tangents != null)
        {
            GUILayout.Label($"TANGENTS ({_tangents.Length}):");
            for (int i = 0; i < _tangents.Length; ++i)
            {
                GUILayout.Label($"{i}: {_tangents[i]}");
            }
        }

        if (_normals != null)
        {
            GUILayout.Label($"NORMALS ({_normals.Length}):");
            for (int i = 0; i < _normals.Length; ++i)
            {
                GUILayout.Label($"{i}: {_normals[i]}");
            }
        }
    }
}
