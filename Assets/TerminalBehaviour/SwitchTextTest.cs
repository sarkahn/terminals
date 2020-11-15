using Sark.Terminals;
using Sark.Terminals.TerminalExtensions;
using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

public class SwitchTextTest : MonoBehaviour
{
    TerminalBehaviour _term;

    [SerializeField]
    List<Material> _fonts = new List<Material>();

    int _curr = 1;

    // Start is called before the first frame update
    void Start()
    {
        _term = GetComponent<TerminalBehaviour>();

        _term.Tiles.ScrambleJob().Run();
        _term.SetDirty();
    }

    // Update is called once per frame
    void Update()
    {
        if( Input.GetKeyDown(KeyCode.Space))
        {
            _term.WithFont(_fonts[_curr++]);
            _curr = _curr % _fonts.Count;

            _term.Tiles.ScrambleJob().Run();
            _term.SetDirty();
        }
    }
}
