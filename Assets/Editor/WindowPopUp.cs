using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;

public class WindowPopUp : EditorWindow {
    public Md5sumCollector md5sum;
    public GUISkin skin;

    void OnGUI()
    {
        GUI.color = Color.red;
        EditorStyles.textField.wordWrap = false;
        GUI.contentColor = Color.red;
        EditorGUILayout.LabelField("Warning!!! Patch will apply on old standalone", EditorStyles.whiteBoldLabel);
        EditorGUILayout.LabelField("Please make backup of old standalone files", EditorStyles.whiteBoldLabel);
        EditorGUILayout.LabelField("Applying Patch will override old standalone!", EditorStyles.whiteBoldLabel);
        GUILayout.Space(30);
        if (GUILayout.Button("Continue")) Task.Run(() => md5sum.Patching());
        GUI.color = Color.white;
        GUILayout.Space(30);
        if (GUILayout.Button("Cancel")) this.Close();
    }
}
