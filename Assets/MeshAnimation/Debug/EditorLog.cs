using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EditorLog {

    public static void EditorWindowLog(string message)
    {
        EditorUtility.DisplayDialog("提示", message, "确定");
    }
	
}
