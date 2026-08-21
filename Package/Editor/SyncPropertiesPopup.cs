#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NekoDialogue.Editor
{
    /// <summary>
    /// A small popup window content class that displays checkboxes for selecting which properties to sync.
    /// </summary>
    public class SyncPropertiesPopup : PopupWindowContent
    {
        private readonly string[] m_Properties;
        private readonly string[] m_DisplayNames;
        private readonly bool[] m_Selections;
        private readonly Action<List<string>> m_OnSyncConfirm;

        public SyncPropertiesPopup(string[] properties, string[] displayNames, Action<List<string>> onSyncConfirm)
        {
            m_Properties = properties;
            m_DisplayNames = displayNames;
            m_OnSyncConfirm = onSyncConfirm;
            m_Selections = new bool[properties.Length];

            // Shade: Default all selections to true when the window opens
            for (int i = 0; i < m_Selections.Length; i++)
            {
                m_Selections[i] = true;
            }
        }

        public override Vector2 GetWindowSize()
        {
            // Shade: Dynamically size the window based on the number of properties + padding for the button
            return new Vector2(220f, (m_Properties.Length * EditorGUIUtility.singleLineHeight) + 85f);
        }

        public override void OnGUI(Rect rect)
        {
            GUILayout.Label("Select Properties to Sync", EditorStyles.boldLabel);
            EditorGUILayout.Space(2f);

            // Shade: Draw a checkbox for every property
            for (int i = 0; i < m_Properties.Length; i++)
            {
                m_Selections[i] = EditorGUILayout.Toggle(m_DisplayNames[i], m_Selections[i]);
            }

            EditorGUILayout.Space(6f);

            // Shade: Draw a nice visual line separator
            Rect lineRect = EditorGUILayout.GetControlRect(false, 1f);
            EditorGUI.DrawRect(lineRect, new Color(0.5f, 0.5f, 0.5f, 0.3f));
            EditorGUILayout.Space(2f);

            // Shade: Apply Button
            if (GUILayout.Button("Apply Sync", GUILayout.Height(24f)))
            {
                List<string> selectedProperties = new List<string>();
                for (int i = 0; i < m_Selections.Length; i++)
                {
                    if (m_Selections[i])
                    {
                        selectedProperties.Add(m_Properties[i]);
                    }
                }

                // Shade: Fire the callback with only the properties the user checked
                m_OnSyncConfirm?.Invoke(selectedProperties);

                // Shade: Close the popup window automatically
                editorWindow.Close();
            }
        }
    }
}
#endif
