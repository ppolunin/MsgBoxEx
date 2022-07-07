using System.Windows.Forms;

public static class AppRes
{
    public static MessageBoxEx.IconResource LoadIcon(ushort iconId) => MessageBoxEx.IconResource.Load(typeof(AppRes).Assembly, iconId);
}
