using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class NeuralNetworkVisualizer : EditorWindow
{

    string myString = "";
    public void OnGUI()
    {

        var rect = EditorGUILayout.BeginHorizontal();
        Handles.color = Color.red;
        Handles.DrawLine(new Vector2(rect.x - 15, rect.y), new Vector2(rect.width + 15, rect.y - 30));
        //Handles.SphereHandleCap(1, new Vector3(10, 10, 1), Quaternion.Euler(0, 0, 0), 20, EventType.Repaint);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();


        
    }


    [MenuItem("Window/My Window")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(NeuralNetworkVisualizer));
    }
}
