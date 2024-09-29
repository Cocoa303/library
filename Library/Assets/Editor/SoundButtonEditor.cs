using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(Common.SoundButton))]
public class SoundButtonEditor : Editor
{
    protected override void OnHeaderGUI()
    {
        base.OnHeaderGUI();
    }

    public override void OnInspectorGUI()
    {
        //== Sound Manager를 탐색하여 가지고있는 데이터들을 출력하기
        var names = Manager.Sound.Instance.EditorClipNames;
        var data = (Common.SoundButton)target;

        EditorGUILayout.LabelField("Select Sound Clip");
        //== 탐색
        int findIndex = names.FindIndex(name => name == data.SoundKey);
        if (findIndex != -1)
        {
            int select = EditorGUILayout.Popup("Select Option", findIndex, names.ToArray());
            data.SoundKey = names[select];
        }
        else
        {
            names.Insert(0, "Select Sound Name");
            int select = EditorGUILayout.Popup("Select Option", 0, names.ToArray());
            data.SoundKey = names[select];
        }

        base.OnInspectorGUI();
    }
}

