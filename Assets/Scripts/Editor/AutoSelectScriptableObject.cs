using UnityEditor;
using UnityEngine;

//[InitializeOnLoad]
public class AutoSelectScriptableObject
{
    static AutoSelectScriptableObject()
    {
        Selection.selectionChanged += OnSelectionChanged;
    }

    static void OnSelectionChanged()
    {
        if (Selection.activeGameObject != null)
        {
            var instance = Selection.activeGameObject.GetComponent<HexaContainer>();
            if (instance != null && instance.scriptable != null)
            {
                // Reporter la sélection à la prochaine frame de l'éditeur
                var target = instance.scriptable;
                EditorApplication.delayCall += () =>
                {
                    Selection.activeObject = target;
                    EditorGUIUtility.PingObject(target); // Optionnel
                };
            }
        }
    }
}

