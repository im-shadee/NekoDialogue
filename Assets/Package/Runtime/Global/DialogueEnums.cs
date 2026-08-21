namespace NekoDialogue
{
    public enum eDialogueTextMode
    {
        Plain = 0, // Shade: Plain string
        Localized, // Shade: Text issued from Unity's LocalizedString
    }

    public enum eDialogueBoxPlacement
    {
        // Shade: Standard Canvas Anchors
        TopLeft = 0,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight,

        // Shade: Additional Custom Options
        Custom,
        Speaker,
    }

    public enum eSpeechBubbleTailPosition
    {
        None = 0,
        Left,
        Middle,
        Right // Shade: Flipped version of Left to save on disk size and clutter
    }

    public enum eSpeechBubbleVerticalEdge
    {
        AutoDetect = 0, // Shade: Auto-detect Top vs Bottom depending on box position related to npc
        Bottom, // Shade: Force Bottom
        Top // Shade: Force Top
    }

    public enum eDialogueEmotion
    {
        Neutral = 0,
        Angry,
        Excited,
        Sad,
        Scared
    }
}
