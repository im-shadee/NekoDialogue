#if UNITY_EDITOR
using NekoDialogue.Core.Conversation;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace NekoDialogue.Editor
{
    /// <summary>
    /// Custom Inspector editor for <see cref="ConversationAsset"/> providing a compact list view, 
    /// tailored read-only recaps, and pop-up editor integration.
    /// </summary>
    [CustomEditor(typeof(ConversationAsset))]
    public class ConversationAssetEditor : UnityEditor.Editor
    {
        private SerializedProperty m_ConversationLinesProp;
        private ReorderableList m_ReorderableList;

        // Shade: Struct and asset properties rendered sequentially within the inspector recap view
        private static readonly string[] m_kStandardRecapProperties = new string[]
        {
            "m_BoxLayout",
            "m_BoxStyle",
            "m_BoxAnimation",
            "m_TailSettings",
            "m_FontSettings",
            "m_DialogueEmotion",
            "m_VoiceProfile"
        };

        private void OnEnable()
        {
            m_ConversationLinesProp = serializedObject.FindProperty("m_ConversationLines");

            m_ReorderableList = new ReorderableList(serializedObject, m_ConversationLinesProp, true, true, true, true)
            {
                drawHeaderCallback = DrawHeader,
                drawElementCallback = DrawElement,
                elementHeightCallback = GetElementHeight,
                onAddDropdownCallback = OnAddDropdown
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(4f);
            m_ReorderableList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draws the header label for the conversation lines reorderable list.
        /// </summary>
        private void DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Conversation Lines", EditorStyles.boldLabel);
        }

        /// <summary>
        /// Renders an individual conversation line element, foldout header, action gear, and read-only preview recap.
        /// </summary>
        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty lineProp = m_ConversationLinesProp.GetArrayElementAtIndex(index);
            if (lineProp == null || lineProp.managedReferenceValue == null)
            {
                EditorGUI.LabelField(rect, $"Line {index}: [Null Reference]");
                return;
            }

            SerializedProperty tagProp = lineProp.FindPropertyRelative("m_DialogueTag");
            string lineTag = (tagProp != null && !string.IsNullOrEmpty(tagProp.stringValue))
                ? tagProp.stringValue
                : $"Line {index + 1} (Unassigned)";

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float verticalSpacing = EditorGUIUtility.standardVerticalSpacing;

            // Shade: Left padding prevents foldout arrow from overlapping reorderable list drag handle
            float leftPadding = 14f;

            Rect headerRect = new Rect(rect.x + leftPadding, rect.y + 2f, rect.width - 24f - leftPadding, lineHeight);
            Rect gearRect = new Rect(rect.x + rect.width - 20f, rect.y + 2f, 20f, lineHeight);

            // Shade: Render main element foldout toggle
            lineProp.isExpanded = EditorGUI.Foldout(headerRect, lineProp.isExpanded, lineTag, true);

            // Shade: Render popup editor trigger button
            GUIContent gearIcon = EditorGUIUtility.IconContent("_Popup");
            if (GUI.Button(gearRect, gearIcon, EditorStyles.label))
            {
                // Shade: Pass the array property and index to draw sync button
                DialogueLineEditorWindow.Open(serializedObject, m_ConversationLinesProp, index, lineTag);
            }

            // Shade: Render disabled read-only inspector preview
            if (lineProp.isExpanded)
            {
                EditorGUI.indentLevel++;
                float currentY = rect.y + lineHeight + verticalSpacing + 4f;

                EditorGUI.BeginDisabledGroup(true);

                // Shade: Custom text field evaluating raw or localized dialogue payload
                SerializedProperty textProp = lineProp.FindPropertyRelative("m_DialogueText");
                if (textProp != null)
                {
                    string textContent = GetResolvedDialogueText(lineProp);
                    Rect textRect = new Rect(rect.x, currentY, rect.width - 24f, lineHeight);

                    EditorGUI.TextField(textRect, "Text Content", textContent);
                    currentY += lineHeight + verticalSpacing;
                }

                // Shade: Iterate and draw remaining standard recap fields
                foreach (string propName in m_kStandardRecapProperties)
                {
                    SerializedProperty subProp = lineProp.FindPropertyRelative(propName);
                    if (subProp != null)
                    {
                        float propHeight = EditorGUI.GetPropertyHeight(subProp, true);
                        Rect propRect = new Rect(rect.x, currentY, rect.width - 24f, propHeight);

                        EditorGUI.PropertyField(propRect, subProp, true);
                        currentY += propHeight + verticalSpacing;
                    }
                }

                EditorGUI.EndDisabledGroup();
                EditorGUI.indentLevel--;
            }
        }

        /// <summary>
        /// Calculates the total dynamic height required to render a conversation line based on foldout expansion state.
        /// </summary>
        private float GetElementHeight(int index)
        {
            SerializedProperty lineProp = m_ConversationLinesProp.GetArrayElementAtIndex(index);
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float verticalSpacing = EditorGUIUtility.standardVerticalSpacing;

            if (lineProp == null || !lineProp.isExpanded)
            {
                return lineHeight + 6f;
            }

            // Shade: Account for foldout header and custom dialogue text preview row
            float totalHeight = lineHeight + verticalSpacing + lineHeight + verticalSpacing + 6f;

            // Shade: Accumulate total heights of all standard recap properties
            foreach (string propName in m_kStandardRecapProperties)
            {
                SerializedProperty subProp = lineProp.FindPropertyRelative(propName);
                if (subProp != null)
                {
                    totalHeight += EditorGUI.GetPropertyHeight(subProp, true) + verticalSpacing;
                }
            }

            return totalHeight + 4f;
        }

        /// <summary>
        /// Retrieves the resolved string payload directly from the underlying DialogueLine instance.
        /// </summary>
        private string GetResolvedDialogueText(SerializedProperty lineProp)
        {
            // Shade: Extract object reference via managed reference value on polymorphic dialogue line
            if (lineProp != null && lineProp.managedReferenceValue is DialogueLine line)
            {
                if (line.DialogueText != null)
                {
                    string result = line.DialogueText.GetText();
                    if (!string.IsNullOrEmpty(result))
                    {
                        return result;
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Context menu callback for instantiating polymorphic dialogue line concrete types.
        /// </summary>
        private void OnAddDropdown(Rect buttonRect, ReorderableList list)
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Standard Dialogue Line"), false, () => AddLine(new StandardDialogueLine()));
            menu.AddItem(new GUIContent("Branching Dialogue Line"), false, () => AddLine(new BranchingDialogueLine()));

            menu.ShowAsContext();
        }

        /// <summary>
        /// Appends a new polymorphic dialogue line instance to the conversation collection property.
        /// </summary>
        private void AddLine(DialogueLine newLine)
        {
            serializedObject.Update();
            int index = m_ConversationLinesProp.arraySize;
            m_ConversationLinesProp.InsertArrayElementAtIndex(index);

            SerializedProperty newProp = m_ConversationLinesProp.GetArrayElementAtIndex(index);
            newProp.managedReferenceValue = newLine;

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
