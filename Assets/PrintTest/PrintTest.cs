using Sark.Terminals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrintTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var term = GetComponent<TerminalBehaviour>();

        term.Print(1, 0, "Line 0");
        term.Print(1, 2, "Line 2");
        term.Print(1, 4, "Line 4");
        term.Print(1, 7, "Line 7");

        term.Set(0, 0, 'a');
        term.Set(term.Width - 1, 0, 'b');
        term.Set(0, term.Height - 1, 'c');
        term.Set(term.Width - 1, term.Height - 1, 'd');
    }

}
