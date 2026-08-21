#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TMPro;

namespace NekoDialogue.Editor
{
    /// <summary>
    /// Custom property drawer for <see cref="FontSettings"/> that handles UI layout and auto-assigns default TMP fonts without layout bleeding.
    /// </summary>
    [CustomPropertyDrawer(typeof(FontSettings))]
    public class FontSettingsPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Shade: Assign system default TextMeshPro font asset if field is unassigned
            SerializedProperty fontProp = property.FindPropertyRelative("m_Font");
            if (fontProp != null && fontProp.objectReferenceValue == null)
            {
                TMP_FontAsset defaultFont = TMP_Settings.defaultFontAsset;
                if (defaultFont != null)
                {
                    fontProp.objectReferenceValue = defaultFont;
                    property.serializedObject.ApplyModifiedProperties();
                }
            }

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float verticalSpacing = EditorGUIUtility.standardVerticalSpacing;

            Rect foldoutRect = new Rect(position.x, position.y, position.width, lineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                float currentY = position.y + lineHeight + verticalSpacing;

                SerializedProperty childProp = property.Copy();
                SerializedProperty endProperty = childProp.GetEndProperty();

                // Shade: Safely iterate through nested properties restricted to FontSettings property depth
                if (childProp.NextVisible(true))
                {
                    do
                    {
                        if (SerializedProperty.EqualContents(childProp, endProperty))
                            break;

                        float fieldHeight = EditorGUI.GetPropertyHeight(childProp, true);
                        Rect fieldRect = new Rect(position.x, currentY, position.width, fieldHeight);

                        EditorGUI.PropertyField(fieldRect, childProp, true);
                        currentY += fieldHeight + verticalSpacing;
                    }
                    while (childProp.NextVisible(false));
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;

            if (!property.isExpanded)
            {
                return lineHeight;
            }

            float verticalSpacing = EditorGUIUtility.standardVerticalSpacing;
            float totalHeight = lineHeight + verticalSpacing;

            SerializedProperty childProp = property.Copy();
            SerializedProperty endProperty = childProp.GetEndProperty();

            if (childProp.NextVisible(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(childProp, endProperty))
                        break;

                    totalHeight += EditorGUI.GetPropertyHeight(childProp, true) + verticalSpacing;
                }
                while (childProp.NextVisible(false));
            }

            return totalHeight;
        }
    }
}
#endif
