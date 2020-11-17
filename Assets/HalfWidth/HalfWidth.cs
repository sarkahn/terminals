using Sark.Terminals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class HalfWidth : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var term = GetComponent<TerminalBehaviour>();

        term.Print(0, 1, "The quick brown fox jumps over the lazy dog.");
        term.Print(0, 4, "The quick brown fox jumps over the lazy dog.");
    }
}
