using Sark.Terminals;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using Sark.Common.CameraExtensions;

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
    TerminalBehaviour _halfTerm;

    IEnumerator Start()
    {
        // Apparently PixelPerfectCamera takes a couple frames before it starts
        // adjusting the viewport...good stuff
        yield return null;
        yield return null;
        _term = TerminalBehaviour.CreateAtScreenPosition("Terminal", 
            44, 1, posX, posY, alignX, alignY);
        _term.Print(0, 0, "The quick brown fox jumps over the lazy dog.");

        // Initial alignment fails since WithFont gets called after construction...
        _halfTerm = TerminalBehaviour.CreateAtScreenPosition("HalfTerminal",
            44, 1, posX, posY, alignX, alignY).WithFont("Terminal8x16");
        _halfTerm.transform.position = Camera.main.GetAlignedViewportPosition(
            new float2(0, 1),
            new float2(1, -1),
            _halfTerm.GetWorldSize().xy);
        _halfTerm.transform.position += Vector3.down;
        _halfTerm.Print(0, 0, "The quick brown fox jumps over the lazy dog.");
    }

    // Update is called once per frame
    void Update()
    {
        //_term.WithScreenPosition(posX, posY, alignX, alignY);
    }
}
