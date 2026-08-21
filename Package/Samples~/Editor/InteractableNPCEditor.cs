#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace NekoDialogue.Editor
{
    /// <summary>
    /// Custom Inspector for <see cref="InteractableNPC"/> providing quick play-mode and utility buttons.
    /// </summary>
    [CustomEditor(typeof(InteractableNPC))]
    public class InteractableNPCEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            InteractableNPC npc = (InteractableNPC)target;

            EditorGUILayout.Space(4f);

            // Shade: Disable the play-mode trigger button if the game is not currently playing
            GUI.enabled = Application.isPlaying;

            // Shade: Prominent button to trigger conversation testing directly from the inspector
            GUIContent startButtonContent = new GUIContent("Start Conversation", "Initiates the assigned conversation asset immediately.");
            if (GUILayout.Button(startButtonContent, GUILayout.Height(30f)))
            {
                npc.StartConversation();
            }

            // Shade: Button to reset/refresh the active conversation asset to the base variant
            GUIContent refreshButtonContent = new GUIContent("Refresh Conversation Asset", "Resets the active runtime conversation to m_BaseConversation.");
            if (GUILayout.Button(refreshButtonContent, GUILayout.Height(25f)))
            {
                npc.SetBaseConversationAsset();
            }

            GUI.enabled = true; // Shade: Re-enable GUI for the rest of the inspector

            EditorGUILayout.Space(6f);

            // Shade: Draw a subtle line separator
            Rect lineRect = EditorGUILayout.GetControlRect(false, 1f);
            EditorGUI.DrawRect(lineRect, new Color(0.5f, 0.5f, 0.5f, 0.3f));

            EditorGUILayout.Space(6f);

            // Shade: Draw the standard serialized fields
            DrawDefaultInspector();
        }
    }
}
#endif
