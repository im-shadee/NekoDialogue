#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace NekoDialogue.Editor
{
    /// <summary>
    /// Pop-up editor window for modifying a single <see cref="DialogueLine"/> property in isolation using automatic IMGUI layout.
    /// </summary>
    public class DialogueLineEditorWindow : EditorWindow
    {
        private SerializedObject m_SerializedObject;
        private SerializedProperty m_LineProperty;
        private Vector2 m_ScrollPosition;

        /// <summary>
        /// Instantiates and displays the utility editor window targeted at a specific serialized dialogue line.
        /// </summary>
        public static void Open(SerializedObject serializedObject, SerializedProperty lineProperty, string tag)
        {
            DialogueLineEditorWindow window = GetWindow<DialogueLineEditorWindow>(true, $"Edit Line: {tag}", true);
            window.m_SerializedObject = serializedObject;
            window.m_LineProperty = lineProperty;
            window.minSize = new Vector2(420f, 550f);
            window.ShowUtility();
        }

        private void OnGUI()
        {
            if (m_SerializedObject == null || m_LineProperty == null)
            {
                Close();
                return;
            }

            m_SerializedObject.Update();

            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);
            EditorGUILayout.Space(8f);

            SerializedProperty copy = m_LineProperty.Copy();
            SerializedProperty endProperty = copy.GetEndProperty();

            // Shade: Automatically iterate through properties within scope without field overlap
            if (copy.NextVisible(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(copy, endProperty))
                        break;

                    EditorGUILayout.PropertyField(copy, true);
                }
                while (copy.NextVisible(false));
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.EndScrollView();

            m_SerializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
