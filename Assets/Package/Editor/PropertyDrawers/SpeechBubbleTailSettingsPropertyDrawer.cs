#if UNITY_EDITOR
using NekoDialogue;
using UnityEditor;
using UnityEngine;

namespace EZDialogue.Editor
{
    /// <summary>
    /// Shade: Custom property drawer that conditionally renders speech bubble tail configuration options.
    /// </summary>
    [CustomPropertyDrawer(typeof(SpeechBubbleTailSettings))]
    public class SpeechBubbleTailSettingsDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            SerializedProperty enableProp = property.FindPropertyRelative("m_EnableTail");
            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            bool m_bEnableTail = enableProp != null && enableProp.boolValue;
            if (m_bEnableTail)
            {
                height += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2;

                SerializedProperty autoPosProp = property.FindPropertyRelative("m_AutoPositionX");
                bool m_bAutoPos = autoPosProp != null && autoPosProp.boolValue;

                if (!m_bAutoPos)
                {
                    height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty enableProp = property.FindPropertyRelative("m_EnableTail");
            SerializedProperty vertEdgeProp = property.FindPropertyRelative("m_VerticalEdge");
            SerializedProperty autoPosProp = property.FindPropertyRelative("m_AutoPositionX");
            SerializedProperty normXProp = property.FindPropertyRelative("m_NormalizedXPosition");

            Rect drawRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(drawRect, property.isExpanded, label, true);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(drawRect, enableProp);

                bool m_bEnableTail = enableProp != null && enableProp.boolValue;

                // Shade: Hide positioning controls when the bubble tail feature is toggled off
                if (m_bEnableTail)
                {
                    drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    EditorGUI.PropertyField(drawRect, vertEdgeProp);

                    drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    EditorGUI.PropertyField(drawRect, autoPosProp);

                    bool m_bAutoPos = autoPosProp != null && autoPosProp.boolValue;

                    // Shade: Hide manual placement slider when auto-positioning is active to clean up UI
                    if (!m_bAutoPos)
                    {
                        drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        EditorGUI.PropertyField(drawRect, normXProp);
                    }
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }
    }
}
#endif
