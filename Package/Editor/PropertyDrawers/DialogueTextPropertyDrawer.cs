#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using NekoDialogue.Core.Conversation;

namespace NekoDialogue.Editor
{
    /// <summary>
    /// Custom property drawer for <see cref="DialogueText"/> to conditionally render input fields based on the selected mode.
    /// </summary>
    [CustomPropertyDrawer(typeof(DialogueText))]
    public class DialogueTextPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Shade: Retrieve child properties from the serialized DialogueText target
            SerializedProperty modeProp = property.FindPropertyRelative("m_DialogueTextMode");
            SerializedProperty plainTextProp = property.FindPropertyRelative("m_DialogueText");
            SerializedProperty localizedTextProp = property.FindPropertyRelative("m_LocalizedDialogueText");

            // Shade: Calculate layout rects for the property foldout and fields
            Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            // Shade: Draw the main foldout label for the DialogueText property
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                float lineHeight = EditorGUIUtility.singleLineHeight;
                float verticalSpacing = EditorGUIUtility.standardVerticalSpacing;
                float currentY = position.y + lineHeight + verticalSpacing;

                // Shade: Draw the mode selector dropdown field
                Rect modeRect = new Rect(position.x, currentY, position.width, lineHeight);
                EditorGUI.PropertyField(modeRect, modeProp);
                currentY += lineHeight + verticalSpacing;

                // Shade: Cast the enum value to evaluate which conditional text field to display
                eDialogueTextMode mode = (eDialogueTextMode)modeProp.enumValueIndex;

                switch (mode)
                {
                    case eDialogueTextMode.Plain:
                        // Shade: Render plain text field while preserving localized property data silently
                        Rect plainRect = new Rect(position.x, currentY, position.width, lineHeight);
                        EditorGUI.PropertyField(plainRect, plainTextProp, new GUIContent("Text"));
                        break;

                    case eDialogueTextMode.Localized:
                        // Shade: Render localized string field while preserving plain text property data silently
                        Rect localizedRect = new Rect(position.x, currentY, position.width, EditorGUI.GetPropertyHeight(localizedTextProp));
                        EditorGUI.PropertyField(localizedRect, localizedTextProp, new GUIContent("Localized Reference"), true);
                        break;

                    default:
                        // Shade: Log warning if an unhandled text mode is selected in the Editor UI
                        Rect errorRect = new Rect(position.x, currentY, position.width, lineHeight);
                        EditorGUI.HelpBox(errorRect, $"Unsupported DialogueTextMode: {mode}", MessageType.Warning);
                        break;
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float verticalSpacing = EditorGUIUtility.standardVerticalSpacing;

            if (!property.isExpanded)
            {
                return lineHeight;
            }

            // Shade: Calculate total height dynamically based on mode and expanded sub-properties
            float height = lineHeight + verticalSpacing + lineHeight + verticalSpacing; // Foldout + Mode Dropdown

            SerializedProperty modeProp = property.FindPropertyRelative("m_DialogueTextMode");
            eDialogueTextMode mode = (eDialogueTextMode)modeProp.enumValueIndex;

            if (mode == eDialogueTextMode.Plain)
            {
                height += lineHeight;
            }
            else if (mode == eDialogueTextMode.Localized)
            {
                SerializedProperty localizedTextProp = property.FindPropertyRelative("m_LocalizedDialogueText");
                height += EditorGUI.GetPropertyHeight(localizedTextProp);
            }

            return height;
        }
    }
}
#endif
