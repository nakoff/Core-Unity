using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class EcsEntityMarker : Editor
{
    static EcsEntityMarker()
    {
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
    }

    private static void OnHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
        GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

        if (obj != null && obj.GetComponent<Core.LLEcs.EcsEntity>() != null)
        {
            Rect iconRect = new Rect(selectionRect.xMin - 2.0f, selectionRect.y - 2.0f, 18, 18);
            GUI.Label(iconRect, "🔸");
        }

        if (obj != null && obj.GetComponent<Core.LLEcs.EcsWorld>() != null)
        {
            Rect iconRect = new Rect(selectionRect.xMin - 2, selectionRect.y - 1, 18, 18);
            GUI.Label(iconRect, "💫");
        }
    }
}
