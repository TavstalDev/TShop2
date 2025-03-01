using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Editor.Scripts
{
    /// <summary>
    /// A custom Unity editor window for generating progress bars with specified properties.
    /// </summary>
    public class ProgressbarGenerator : EditorWindow
    {
        /// <summary>
        /// The parent GameObject for the progress bar.
        /// </summary>
        public GameObject progressBar;

        /// <summary>
        /// The template GameObject for the progress bar values.
        /// </summary>
        public GameObject progressBarBaseValue;

        /// <summary>
        /// The naming format for generated progress bar values. Must include {index}.
        /// </summary>
        public string valueFormat = "ProgressBar_Value#{index}";

        /// <summary>
        /// The maximum value for the progress bar.
        /// </summary>
        public int valueMax = 100;

        /// <summary>
        /// Determines whether the base value object should be deleted automatically after generation.
        /// </summary>
        public bool valueDeleteBaseValue;

        /// <summary>
        /// A list to store the last generated GameObjects for easy deletion.
        /// </summary>
        private readonly List<GameObject> _lastGeneratedObjects = new();

        /// <summary>
        /// Displays the ProgressBar Generator editor window in the Unity Editor.
        /// </summary>
        [MenuItem("Window/Tavstal/ProgressBar Generator")]
        public static void ShowWindow()
        {
            GetWindow<ProgressbarGenerator>("ProgressBar Generator");
        }

        /// <summary>
        /// Renders the custom editor GUI.
        /// </summary>
        void OnGUI()
        {
            GUILayout.Label("ProgressBar Generator", EditorStyles.boldLabel);

            // Input fields
            progressBar =
                (GameObject)EditorGUILayout.ObjectField("Progressbar: ", progressBar, typeof(GameObject), true);
            progressBarBaseValue = (GameObject)EditorGUILayout.ObjectField("Progressbar base value: ",
                progressBarBaseValue, typeof(GameObject), true);
            valueFormat = EditorGUILayout.TextField("Value name:", valueFormat);
            valueMax = EditorGUILayout.IntField("Progress bar max value:", valueMax);
            valueDeleteBaseValue = EditorGUILayout.Toggle("Auto delete default value", valueDeleteBaseValue);

            // Generate button
            if (GUILayout.Button("Generate"))
            {
                // Validate inputs
                if (!progressBar)
                {
                    Debug.LogError("ProgressBar must have a value!");
                    return;
                }

                if (!progressBarBaseValue)
                {
                    Debug.LogError("ProgressBarBaseValue must have a value!");
                    return;
                }

                if (valueMax < 1)
                {
                    Debug.LogError("Max value must be greater than 0!");
                    return;
                }

                if (string.IsNullOrEmpty(valueFormat))
                {
                    Debug.LogError("Value Format must not be empty!");
                    return;
                }

                if (!valueFormat.Contains("{index}"))
                {
                    Debug.LogError("Value Format must contain {index}!");
                    return;
                }

                // Generate progress bar values
                for (int i = 0; i <= valueMax; i++)
                {
                    GameObject newObj = Instantiate(progressBarBaseValue, progressBarBaseValue.transform);
                    newObj.SetActive(true);
                    newObj.name = valueFormat.Replace("{index}", i.ToString());

                    // Configure slider value
                    Slider slider = newObj.GetComponent(typeof(Slider)) as Slider;
                    if (slider)
                    {
                        if (!slider.wholeNumbers)
                            slider.value = (float)i / valueMax;
                        else
                            slider.value = (float)i / valueMax * 100;
                        Debug.Log($"Slider is valid! Value: {slider.value}");
                    }
                    else
                    {
                        Debug.LogError("Slider component is missing!");
                    }

                    // Set as a child of the progress bar
                    newObj.transform.SetParent(progressBar.transform);
                    _lastGeneratedObjects.Add(newObj);
                }

                // Optionally delete the base value
                if (valueDeleteBaseValue && progressBarBaseValue)
                {
                    DestroyImmediate(progressBarBaseValue);
                }
            }

            // Delete last generated objects
            if (GUILayout.Button("Delete Last Generated"))
            {
                if (_lastGeneratedObjects.Count <= 0)
                    return;

                foreach (GameObject gameObject in _lastGeneratedObjects)
                    DestroyImmediate(gameObject);

                _lastGeneratedObjects.Clear();
            }
        }
    }
}
