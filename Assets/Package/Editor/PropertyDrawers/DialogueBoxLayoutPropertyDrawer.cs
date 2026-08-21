#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace NekoDialogue.Editor
{
    /// <summary>
    /// Custom property drawer for <see cref="DialogueBoxLayout"/> that calculates dynamic inspector height
    /// and conditionally disables only the Position field when Box Placement is not set to Custom.
    /// </summary>
    [CustomPropertyDrawer(typeof(DialogueBoxLayout))]
    public class DialogueBoxLayoutPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float verticalSpacing = EditorGUIUtility.standardVerticalSpacing;

            Rect foldoutRect = new Rect(position.x, position.y, position.width, lineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                float currentY = position.y + lineHeight + verticalSpacing;

                // Shade: Retrieve placement property to check if it's set to Custom
                SerializedProperty placementProp = property.FindPropertyRelative("m_BoxPlacement");

                bool isCustomPlacement = placementProp != null && placementProp.enumNames[placementProp.enumValueIndex] == "Custom";

                SerializedProperty copy = property.Copy();
                SerializedProperty endProperty = copy.GetEndProperty();

                if (copy.NextVisible(true))
                {
                    do
                    {
                        if (SerializedProperty.EqualContents(copy, endProperty))
                            break;

                        float fieldHeight = EditorGUI.GetPropertyHeight(copy, true);
                        Rect fieldRect = new Rect(position.x, currentY, position.width, fieldHeight);

                        // Shade: Disable GUI interaction ONLY for the position field when placement is not Custom
                        bool isPositionField = copy.name == "m_Position" || copy.name == "m_BoxPosition";

                        if (isPositionField && !isCustomPlacement)
                        {
                            EditorGUI.BeginDisabledGroup(true);
                            EditorGUI.PropertyField(fieldRect, copy, true);
                            EditorGUI.EndDisabledGroup();
                        }
                        else
                        {
                            EditorGUI.PropertyField(fieldRect, copy, true);
                        }

                        currentY += fieldHeight + verticalSpacing;
                    }
                    while (copy.NextVisible(false));
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

            SerializedProperty copy = property.Copy();
            SerializedProperty endProperty = copy.GetEndProperty();

            if (copy.NextVisible(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(copy, endProperty))
                        break;

                    totalHeight += EditorGUI.GetPropertyHeight(copy, true) + verticalSpacing;
                }
                while (copy.NextVisible(false));
            }

            return totalHeight;
        }
    }
}
#endif
