#if UNITY_EDITOR
using NekoDialogue.Core.Conversation;
using System;
using System.Collections.Generic;
using System.Reflection;
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
        private SerializedProperty m_ArrayProperty;
        private int m_Index;
        private SerializedProperty m_LineProperty;
        private Vector2 m_ScrollPosition;

        // Shade: The specific fields we want to sync from the previous line (ignoring the Text and Tag properties)
        private static readonly string[] m_kSyncProperties = new string[]
        {
            "m_BoxLayout",
            "m_BoxStyle",
            "m_BoxAnimation",
            "m_TailSettings",
            "m_FontSettings",
            "m_bUseTypeWriter",
            "m_DialogueEmotion",
            "m_VoiceProfile"
        };

        // Shade: Clean display names so the popup looks professional
        private static readonly string[] m_kSyncDisplayNames = new string[]
        {
            "Box Layout",
            "Box Style",
            "Box Animation",
            "Tail Settings",
            "Font Settings",
            "Use Typewriter",
            "Dialogue Emotion",
            "Voice Profile"
        };

        /// <summary>
        /// Instantiates and displays the utility editor window targeted at a specific serialized dialogue line.
        /// </summary>
        public static void Open(SerializedObject serializedObject, SerializedProperty arrayProp, int index, string tag)
        {
            DialogueLineEditorWindow window = GetWindow<DialogueLineEditorWindow>(true, $"Edit Line: {tag}", true);
            window.m_SerializedObject = serializedObject;
            window.m_ArrayProperty = arrayProp;
            window.m_Index = index;
            window.m_LineProperty = arrayProp.GetArrayElementAtIndex(index);

            // Shade: Explicitly update the title whenever the window is opened/re-focused for a different line
            // This prevents stale title when this is called while a window already exists
            window.titleContent = new GUIContent($"Edit Line: {tag}");

            window.minSize = new Vector2(420f, 550f);
            window.ShowUtility();
        }

        private void OnGUI()
        {
            // Shade: Check if the serialized object or its target has become null/destroyed
            if (m_SerializedObject == null || m_SerializedObject.targetObject == null || m_LineProperty == null)
            {
                Close();
                return;
            }

            try
            {
                m_SerializedObject.Update();
            }
            catch (Exception)
            {
                // Shade: Fallback catch if the underlying C++ object is already disposed
                Close();
                return;
            }

            // Shade: Render Sync Button if this is not the first entry in the conversation
            if (m_Index > 0)
            {
                EditorGUILayout.Space(6f);
                if (GUILayout.Button("Sync Parameters with Previous Line...", GUILayout.Height(24f)))
                {
                    // Shade: Grab the Rect of the button we just drew so the popup knows where to anchor itself
                    Rect buttonRect = GUILayoutUtility.GetLastRect();

                    // Shade: Open the custom checkbox popup
                    PopupWindow.Show(buttonRect, new SyncPropertiesPopup(m_kSyncProperties, m_kSyncDisplayNames, SyncWithPreviousLine));
                }
                EditorGUILayout.Space(2f);

                // Shade: Draw a small visual separator
                Rect lineRect = EditorGUILayout.GetControlRect(false, 1f);
                EditorGUI.DrawRect(lineRect, new Color(0.5f, 0.5f, 0.5f, 0.3f));
            }

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

        private void OnSelectionChange()
        {
            // Shade: Automatically close the window if the user selects a different asset or clears selection,
            // preventing stale references to destroyed/deserialized objects.
            Close();
        }

        /// <summary>
        /// Safely copies selected layout, theme, and audio parameters from the previous conversation line in the array.
        /// </summary>
        private void SyncWithPreviousLine(List<string> propertiesToSync)
        {
            if (propertiesToSync == null || propertiesToSync.Count == 0 
                || m_SerializedObject == null || m_SerializedObject.targetObject == null)
            {
                return;
            }

            // Shade: Apply any uncommitted changes from GUI controls
            m_SerializedObject.ApplyModifiedProperties();

            // Shade: Extract the raw C# objects holding the data
            object prevLineObj = m_ArrayProperty.GetArrayElementAtIndex(m_Index - 1).managedReferenceValue;
            object currentLineObj = m_LineProperty.managedReferenceValue;

            if (prevLineObj != null && currentLineObj != null)
            {
                // Shade: Register undo state before modifying the C# backend
                Undo.RecordObject(m_SerializedObject.targetObject, "Sync Dialogue Line Parameters");

                // Shade: Target the abstract base class explicitly. 
                // Reflection cannot automatically find private fields of a base class when querying a derived class
                Type baseType = typeof(DialogueLine);

                foreach (string fieldName in propertiesToSync)
                {
                    FieldInfo field = baseType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                    if (field != null)
                    {
                        // Shade: Extract from previous and inject into current
                        object prevValue = field.GetValue(prevLineObj);
                        field.SetValue(currentLineObj, prevValue);
                    }
                    else
                    {
                        NekoDialogueDebug.LogWarning($"Could not find field '{fieldName}' to sync.");
                    }
                }

                // Shade: Force Unity to recognize the backend changes
                EditorUtility.SetDirty(m_SerializedObject.targetObject);
            }

            // Shade: Pull the newly updated C# data back into the SerializedProperty UI wrappers
            m_SerializedObject.Update();

            // Shade: Force the EditorWindow to visually refresh immediately
            Repaint();
        }
    }
}
#endif
