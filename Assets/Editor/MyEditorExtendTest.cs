using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

/*
 * MenuItem属性について https://docs.unity3d.com/ja/2017.4/ScriptReference/MenuItem.html
 */

public class MyEditorExtendTest : EditorWindow
{

    [MenuItem("Window/My Window")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(MyEditorExtendTest));
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 100, 100));
        GUILayout.Button("Click me");
        GUILayout.Button("Or me");
        GUILayout.Button("Or me");
        GUILayout.EndArea();
    }
}
