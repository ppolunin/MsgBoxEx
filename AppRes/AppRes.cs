using System.Windows.Forms;

public static class AppRes
{
    public const ushort IDI_ICON1	= 2;
    public const ushort IDI_ICON2	= 3;
	public const ushort IDI_SAMPLE	= 177;

    public static MessageBoxEx.IconResource LoadIcon(ushort iconId) => MessageBoxEx.IconResource.Load(typeof(AppRes).Assembly, iconId);
}
