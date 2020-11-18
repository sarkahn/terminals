using Sark.Terminals;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ResizeTest : MonoBehaviour
{
    TerminalBehaviour _term;

    [SerializeField]
    int2 _size = new int2(20, 20);

    private void Awake()
    {
        _term = GetComponent<TerminalBehaviour>();
    }

    private void Start()
    {
        _term.Resize(_size.x, _size.y);
    }
}
