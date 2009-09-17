using Buttercup.Control.Model;
using Control.MVP.Views;

namespace Control.MVP.Presenters
{
    public class DisplaySettingsPresenter
    {
        #region Fields (3)

        private readonly ApplicationPresenter _mainPresenter;
        private readonly DisplaySettingsState _state;
        private readonly IDisplaySettingsView _view;

        #endregion Fields

        #region Constructors (1)

        /// <summary>
        /// This constructor initialises the dependent DisplaySettings view, and maintains references to
        /// the main Application Presenter and the Display Settings state (which holds properties such
        /// as current interface size and contrast scheme).
        /// </summary>
        /// <param name="mainPresenter"></param>
        /// <param name="viewReference"></param>
        /// <param name="state"></param>
        internal DisplaySettingsPresenter(IDisplaySettingsView viewReference, ApplicationPresenter mainPresenter,
                                          DisplaySettingsState state)
        {
            _mainPresenter = mainPresenter;
            _view = viewReference;
            _state = state;

            _view.ChangeInterfaceSize += ChangeInterfaceSize;
            _view.ChangeContrastScheme += ChangeContrastScheme;
            //_view.SpeakableElementSelected += _view_SpeakableElementSelected;
            //_view.SelfVoicingSpeakText += _view_SelfVoicingSpeakText;
            _view.UpdateInterfaceSize(_state.InterfaceSize);
            _view.UpdateContrastScheme(_state.ContrastScheme);

            _view.ApplicationView.DisplaySettingsFocusChanged += new System.EventHandler<PanelFocusedItemEventArgs>(ApplicationView_DisplaySettingsFocusChanged);
        }

        void ApplicationView_DisplaySettingsFocusChanged(object sender, PanelFocusedItemEventArgs e)
        {
            _view.FocusedItem = e.FocusedItem;
        }

        #endregion Constructors

        #region Methods (4)

        // Private Methods (4) 

        /// <summary>
        /// Changes the contrast scheme.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The EventArgs containing the desired contrast scheme.</param>
        private void ChangeContrastScheme(object sender, ContrastSchemeEventArgs e)
        {
            //This results in an event being raised to apply the setting to the UI.
            _state.ContrastScheme = e.ContrastScheme;
        }

        /// <summary>
        /// Changes the interface size.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The EventArgs containing the desired interface size.</param>
        private void ChangeInterfaceSize(object sender, InterfaceSizeEventArgs e)
        {
            //This results in an event being raised to apply the setting to the UI.
            _state.InterfaceSize = e.InterfaceSize;
        }

        #endregion Methods
    }
}