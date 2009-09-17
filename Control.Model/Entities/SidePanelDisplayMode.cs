namespace Buttercup.Control.Model.Entities
{
    /// <summary>
    /// Denotes a display mode of the side panel.
    /// </summary>
    public enum SidePanelDisplayMode
    {
        /// <summary>
        /// Indicates that the panel is hidden.
        /// </summary>
        Hidden = 0,
        /// <summary>
        /// Indicates that the panel displays the navigation controls (table of contents or bookmarks).
        /// </summary>
        Navigation,
        /// <summary>
        /// Indicates that the panel displays the interface's display settings.
        /// </summary>
        DisplaySettings,
        /// <summary>
        /// Indicates that the panel displays the interface's search component.
        /// </summary>
        Search,
        /// <summary>
        /// Indicates that the panel displays the interface's help display
        /// </summary>
        Help
    }
}