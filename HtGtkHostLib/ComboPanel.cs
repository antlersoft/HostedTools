using Gtk;

namespace com.antlersoft.HostedTools.GtkHostLib
{
    class ComboPanel : VBox
    {
        public ComboPanel(Widget top, Widget bottom)
        {
            PackStart(top, false, true, 0);
            PackEnd(bottom, true, true, 0);
        }
    }
}
