using Sark.Terminals;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SpawnTest : MonoBehaviour
{

    [SerializeField]
    [Range(0, 1)]
    float posX = 0;

    [SerializeField]
    [Range(0, 1)]
    float posY = 0;

    [SerializeField]
    [Range(-1, 1)]
    float alignX = 0;

    [SerializeField]
    [Range(-1, 1)]
    float alignY = 0;

    TerminalBehaviour _term;

    IEnumerator Start()
    {
        // Apparently PixelPerfectCamera takes a couple frames before it starts
        // adjusting the viewport...good stuff
        yield return null;
        yield return null;
        _term = TerminalBehaviour.CreateAtScreenPosition("Terminal", 
            10, 10, posX, posY, alignX, alignY);
        
    }

    // Update is called once per frame
    void Update()
    {
        //_term.WithScreenPosition(posX, posY, alignX, alignY);
    }
}
