using Buttercup.Control.Model.Entities;

namespace Buttercup.Control.Model
{
    public interface ISpeakable
    {
		#region Data Members (2) 

        Audio Audio { get; set; }

        string Text { get; set; }

		#endregion Data Members 
    }
}