using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
    [ComVisible(true)]
    public enum DialogResultEx
    {
        None = DialogResult.None,
        OK = DialogResult.OK,
        Cancel = DialogResult.Cancel,
        Abort = DialogResult.Abort,
        Retry = DialogResult.Retry,
        Ignore = DialogResult.Ignore,
        Yes = DialogResult.Yes,
        No = DialogResult.No,
        TryAgain = 10,
        Continue = 11
    }
}
