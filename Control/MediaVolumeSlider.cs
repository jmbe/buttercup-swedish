using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Buttercup.Control
{
    public class MediaVolumeSlider : MediaSlider
    {
        #region Fields (1)

        private bool isMuted;

        #endregion Fields

        #region Constructors (1)

        public MediaVolumeSlider()
        {
            isMuted = false;
        }

        #endregion Constructors

        #region Delegates and Events (2)

        // Events (2) 

        /// <summary>
        /// Events thrown when slider thumb has been eleased
        /// </summary>
        public event EventHandler<EventArgs> ThumbDragCompleted;

        /// <summary>
        /// Events thrown when slider thumb has been clicked, dragged
        /// </summary>
        public event EventHandler<EventArgs> ThumbDragStarted;

        #endregion Delegates and Events

        #region Methods (7)

        // Public Methods (3) 

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Thumb thumb = GetTemplateChild("HorizontalThumb") as Thumb;
            if (thumb != null)
            {
                thumb.DragStarted += thumb_DragStarted;
                thumb.DragCompleted += thumb_DragCompleted;
            }
        }

        public virtual void OnThumbDragStarted(object sender, EventArgs e)
        {
            if (ThumbDragStarted != null)
                ThumbDragStarted(sender, e);
        }

        public void ToggleMute()
        {
            Thumb thumb = GetTemplateChild("HorizontalThumb") as Thumb;

            if (isMuted)
            {
                if (thumb != null)
                {
                    thumb.Template = (ControlTemplate)Application.Current.Resources["VolumeSliderThumbStyle"];
                }

                isMuted = false;
            }
            else
            {
                if (thumb != null)
                {
                    thumb.Template = (ControlTemplate)Application.Current.Resources["MutedVolumeSliderThumbStyle"];
                }

                isMuted = true;
            }
        }
        // Protected Methods (2) 

        /// <summary>
        /// Overrides the KeyDown event to prevent the control from intercepting keystrokes.
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            //Default Slider behaviour is to respond to Up, Down, Left, Right for changing value
            //Override this so it instead does not respond to key events
        }

        protected virtual void OnThumbDragCompleted(object sender, EventArgs e)
        {
            if (ThumbDragCompleted != null)
                ThumbDragCompleted(sender, e);
        }
        // Private Methods (2) 

        private void thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            OnThumbDragCompleted(this, new EventArgs());
        }

        private void thumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            OnThumbDragStarted(this, new EventArgs());
        }

        #endregion Methods
    }
}