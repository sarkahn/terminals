using Sark.Terminals;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class SpawnTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var t = new SimpleTerminal(10, 10, Allocator.Temp);
    }

}
