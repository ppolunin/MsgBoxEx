using System.Windows.Forms;

public enum AppRes : ushort
{
    idiIcon1    = 2,
    idiIcon2    = 3,
    idiSample   = 177
}

public static class AppResLoader
{
    public static MessageBoxEx.IconResource Load(this AppRes res) => MessageBoxEx.IconResource.Load(typeof(AppRes).Assembly, (ushort)res);
}
