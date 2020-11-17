using Sark.Terminals;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class PureCodeTest : MonoBehaviour
{
    SimpleTerminal _term;

    private void OnEnable()
    {
        _term = new SimpleTerminal(10, 10, Allocator.Temp);

        //_term.Resize(20, 20);
    }

    private void OnDisable()
    {
        //_term.Dispose();
    }
}
