using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LobbyMenuManager))]
public class LobbyMenuManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var manager = (LobbyMenuManager)target;

        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("Debug Tools", EditorStyles.boldLabel);

        if (GUILayout.Button("Выдать предметы из списка в сундук", GUILayout.Height(30)))
        {
            manager.GiveStarterItems();
        }

        if (GUILayout.Button("Выдать все пластины в сундук", GUILayout.Height(28)))
        {
            manager.GiveAllPlates();
        }

        if (GUILayout.Button("Сбросить сохранение", GUILayout.Height(24)))
        {
            manager.ResetInventorySave();
        }
    }
}
