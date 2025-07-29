using System;
using System.Linq;
using Editor.Scripts.Models;
using UnityEditor;
using UnityEngine;

namespace Editor.Scripts
{
    /// <summary>
    /// A custom Unity editor window for renaming GameObjects with various rename operations.
    /// </summary>
    public class CustomRenameTool : EditorWindow
    {
        /// <summary>
        /// The root GameObject whose children will be renamed.
        /// </summary>
        public GameObject root;

        /// <summary>
        /// The rename operation type (e.g., Prefix, Suffix, Replace, Set).
        /// </summary>
        public ERenameType type = ERenameType.Prefix;

        /// <summary>
        /// Whether the root GameObject should be included in the renaming operation.
        /// </summary>
        public bool includeRoot;

        /// <summary>
        /// The value to search for when using the Replace operation.
        /// </summary>
        public string valueName;

        /// <summary>
        /// The new value to apply during the renaming operation.
        /// </summary>
        public string value = "";

        /// <summary>
        /// Displays the Rename Tool editor window in the Unity Editor.
        /// </summary>
        [MenuItem("Window/Tavstal/Rename Tool")]
        public static void ShowWindow()
        {
            GetWindow<CustomRenameTool>("Custom Rename Tool");
        }

        /// <summary>
        /// Renders the custom editor GUI.
        /// </summary>
        void OnGUI()
        {
            GUILayout.Label("Custom Rename Tool", EditorStyles.boldLabel);

            // Input fields
            root = (GameObject)EditorGUILayout.ObjectField("Root: ", root, typeof(GameObject), true);
            Enum.TryParse(EditorGUILayout.EnumFlagsField(type).ToString(), out type);

            if (type == ERenameType.Replace)
            {
                valueName = EditorGUILayout.TextField("Value To Replace:", valueName);
            }

            value = EditorGUILayout.TextField("New Value:", value);
            includeRoot = EditorGUILayout.Toggle("Include Root", includeRoot);

            // Rename button
            if (GUILayout.Button("Rename"))
            {
                // Validate inputs
                if (!root)
                {
                    Debug.LogError("Root must have a value!");
                    return;
                }

                if (string.IsNullOrEmpty(value))
                {
                    Debug.LogError("New value must not be empty!");
                    return;
                }

                // Perform renaming
                GameObject[] objectsToRename = includeRoot
                    ? root.GetComponentsInChildren<Transform>(true).Select(t => t.gameObject).ToArray()
                    : root.GetComponentsInChildren<Transform>(true).Skip(1).Select(t => t.gameObject).ToArray();

                foreach (GameObject obj in objectsToRename)
                {
                    switch (type)
                    {
                        case ERenameType.Prefix:
                            obj.name = value + obj.name;
                            break;
                        case ERenameType.Suffix:
                            obj.name = obj.name + value;
                            break;
                        case ERenameType.Set:
                            obj.name = value;
                            break;
                        case ERenameType.Replace:
                            if (!string.IsNullOrEmpty(valueName))
                                obj.name = obj.name.Replace(valueName, value);
                            break;
                    }
                }

                Debug.Log("Renaming completed successfully.");
            }
        }
    }
}