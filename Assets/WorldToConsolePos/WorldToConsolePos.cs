using Sark.Terminals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldToConsolePos : MonoBehaviour
{
    TerminalBehaviour _term;

    private void Awake()
    {
        _term = GetComponent<TerminalBehaviour>();
    }

    private void Update()
    {
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var p = _term.WorldPosToTileIndex(mousePos);

        if (!_term.IsInBounds(p))
            return;

        if(Input.GetMouseButtonDown(0))
        {
            _term.Set(p.x, p.y, '.');
        }
    }

    private void OnGUI()
    {
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var p = _term.WorldPosToTileIndex(mousePos);
        GUILayout.Label($"WorldPos {mousePos}, ConsolePos {p}", GUI.skin.box);
    }
}
