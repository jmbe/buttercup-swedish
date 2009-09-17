namespace Buttercup.Control.Model
{
    public enum PlayerMode
    {
        /// <summary>
        /// Indicates that the player is disabled (i.e. has no content to play),
        /// therefore is obviously not playing.
        /// </summary>
        Disabled = 0,
        /// <summary>
        /// Indicates that the player has just been initialised with a new book.
        /// </summary>
        Initialised,
        /// <summary>
        /// Indicates that the player is playing.
        /// </summary>
        Playing,
        /// <summary>
        /// Indicates that the player is paused, but may play content.
        /// </summary>
        Paused,
        /// <summary>
        /// Indicates that the player has finished playing the current book.
        /// </summary>
        Finished
    }
}