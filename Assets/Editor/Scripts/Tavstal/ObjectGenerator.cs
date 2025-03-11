using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Editor.Scripts
{
    /// <summary>
    /// A custom Unity editor window for generating and customizing GameObject instances.
    /// </summary>
    public class ObjectGenerator : EditorWindow
    {
        /// <summary>
        /// The root GameObject under which the generated objects will be parented.
        /// </summary>
        public GameObject root;

        /// <summary>
        /// The base GameObject template used for generating new objects.
        /// </summary>
        public GameObject baseValue;

        /// <summary>
        /// The number of objects to generate.
        /// </summary>
        public int amount = 1;

        /// <summary>
        /// Determines whether the naming of objects should be reversed.
        /// </summary>
        public bool reversed;

        /// <summary>
        /// Displays the Object Generator editor window in the Unity Editor.
        /// </summary>
        [MenuItem("Window/Tavstal/Object Generator")]
        public static void ShowWindow()
        {
            GetWindow<ObjectGenerator>("Object Generator");
        }

        /// <summary>
        /// Renders the custom editor GUI.
        /// </summary>
        private void OnGUI()
        {
            GUILayout.Label("Object Generator", EditorStyles.boldLabel);

            // Fields for user input
            root = (GameObject)EditorGUILayout.ObjectField("Root: ", root, typeof(GameObject), true);
            baseValue = (GameObject)EditorGUILayout.ObjectField("Base value: ", baseValue, typeof(GameObject), true);
            amount = EditorGUILayout.IntField("Amount", amount);
            reversed = EditorGUILayout.Toggle("Reversed", reversed);

            // Generate button
            if (GUILayout.Button("Generate"))
            {
                // Validate input fields
                if (!root)
                {
                    Debug.LogError("Root must have a value!");
                    return;
                }

                if (!baseValue)
                {
                    Debug.LogError("BaseValue must have a value!");
                    return;
                }

                if (amount < 1)
                {
                    Debug.LogError("Amount value must be bigger than 0!");
                    return;
                }

                // Generate objects
                for (int i = 0; i < amount; i++)
                {
                    // Instantiate a new GameObject based on the baseValue
                    GameObject newObj = Instantiate(baseValue, baseValue.transform);
                    Vector3 scale = baseValue.transform.localScale;
                    Quaternion rot = baseValue.transform.rotation;

                    // Activate the object and set its properties
                    newObj.SetActive(true);
                    newObj.name = newObj.name
                        .Replace("{Value}", (reversed ? amount - i : i + 1).ToString())
                        .Replace("(Clone)", "");
                    newObj.transform.SetParent(root.transform);
                    newObj.transform.localScale = scale;
                    newObj.transform.rotation = rot;

                    // Rename child transforms recursively
                    RenameTransform(newObj.transform, (reversed ? amount - i : i + 1).ToString(), true);
                }
            }
        }

        /// <summary>
        /// Recursively renames the transform and its children by replacing {Value} placeholders in the name.
        /// </summary>
        /// <param name="obj">The transform to rename.</param>
        /// <param name="newName">The value to replace {Value} in the name.</param>
        /// <param name="findChild">Determines whether child transforms should be renamed as well.</param>
        private void RenameTransform(Transform obj, string newName, bool findChild)
        {
            // Replace {Value} in the transform's name
            obj.name = Regex.Replace(obj.name, "{Value}", newName, RegexOptions.IgnoreCase);

            // If not renaming children, return early
            if (!findChild)
                return;

            // Recursively rename all child transforms
            for (int i = 0; i < obj.childCount; i++)
                RenameTransform(obj.GetChild(i), newName, true);
        }
    }
}
