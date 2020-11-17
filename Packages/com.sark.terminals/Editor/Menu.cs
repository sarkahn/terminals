using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Sark.Terminals.EditorNS
{
    public static class Menu
    {
        [MenuItem("GameObject/Terminals/Create Terminal", priority = 10, validate = false)]
        public static void CreateTerminal()
        {
            var go = new GameObject("Terminal", typeof(TerminalBehaviour));
            Undo.RegisterCreatedObjectUndo(go, "Create Terminal");
        }
    }
}
