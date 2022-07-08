using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Windows.Forms
{

    public static class MessageBoxEx
    {
        #region Win API

        private const uint WM_SETFOCUS = 7;
        private const int MB_HELP = 0x4000;
        private const int MB_USERICON = 0x0080;
        private const int MB_TASKMODAL = 0x2000;

        private delegate void MSGBOXPARAMS_MsgBoxCallback(IntPtr _);

        [StructLayout(LayoutKind.Sequential)]
        private struct MSGBOXPARAMS
        {
            public readonly int cbSize;
            public readonly IntPtr hwndOwner;
            public readonly IntPtr hInstance;
            public readonly String lpszText;
            public readonly String lpszCaption;
            public readonly int dwStyle;
            public readonly IntPtr lpszIcon;
            public readonly IntPtr dwContextHelpId;
            public readonly MSGBOXPARAMS_MsgBoxCallback lpfnMsgBoxCallback;
            public readonly int dwLanguageId;

            public interface IIconResource
            {
                IntPtr HInstance { get; }
                IntPtr IconId { get; }
            }

            public MSGBOXPARAMS(IWin32Window owner, String text, String caption, int style, Action onHelp, IIconResource icon, CultureInfo culture)
            {
                const string CantShowMBServiceWithHelp = "CantShowMBServiceWithHelp";
                const string CantShowMBServiceWithOwner = "CantShowMBServiceWithOwner";
                const string CantShowModalOnNonInteractive = "CantShowModalOnNonInteractive";

                bool serviceOpt = ((MessageBoxOptions)style & (MessageBoxOptions.ServiceNotification | MessageBoxOptions.DefaultDesktopOnly)) != 0;

                if (SystemInformation.UserInteractive == false && serviceOpt == false)
                    throw new InvalidOperationException(SR.GetString(CantShowModalOnNonInteractive));

                hwndOwner = default;

                if (serviceOpt == true)
                {
                    if (owner != null)
                        throw new InvalidOperationException(SR.GetString(CantShowMBServiceWithOwner));

                    if (onHelp != null)
                        throw new InvalidOperationException(SR.GetString(CantShowMBServiceWithHelp));
                }
                else
                {
                    if (owner != null)
                        hwndOwner = owner.Handle;
                    else
                    {
                        hwndOwner = GetActiveWindow();
                        if (hwndOwner == IntPtr.Zero)
                            style |= MB_TASKMODAL;
                    }
                }

                cbSize = Marshal.SizeOf<MSGBOXPARAMS>();
                lpszIcon = IntPtr.Zero;
                lpfnMsgBoxCallback = null;
                dwContextHelpId = IntPtr.Zero;
                hInstance = IntPtr.Zero;
                dwLanguageId = culture?.LCID ?? default;

                if (icon != null)
                {
                    hInstance = icon.HInstance;
                    lpszIcon = icon.IconId;
                    style |= MB_USERICON;   
                }

                if (onHelp != null)
                {
                    style |= MB_HELP;       
                    lpfnMsgBoxCallback = _ => onHelp();
                }

                lpszText = text;
                lpszCaption = caption;
                dwStyle = style;
            }
        };

        [DllImport("user32", EntryPoint = "MessageBoxIndirect")]
        private static extern int InternalShow(in MSGBOXPARAMS msgboxParams);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

        [DllImport("User32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        #endregion

        #region System.Windows.Forms internals

        private const BindingFlags HOOK_FLAGS = BindingFlags.Static
                | BindingFlags.InvokeMethod
                | BindingFlags.DeclaredOnly;

        private static class ApplicationHook
        {
            private const BindingFlags FLAGS = HOOK_FLAGS | BindingFlags.NonPublic;

            private static readonly MethodInfo _BeginModalMessageLoop = typeof(Application).GetMethod("BeginModalMessageLoop", FLAGS);
            private static readonly MethodInfo _EndModalMessageLoop = typeof(Application).GetMethod("EndModalMessageLoop", FLAGS);

            public static void BeginModalMessageLoop() => _BeginModalMessageLoop.Invoke(null, null);

            public static void EndModalMessageLoop() => _EndModalMessageLoop.Invoke(null, null);
        }

        private static class SR
        {
            private static readonly MethodInfo method = typeof(Application).Assembly
                .GetType("System.Windows.Forms.SR")
                .GetMethod("GetString", 
                    HOOK_FLAGS | BindingFlags.Public, 
                    null, 
                    new Type[] { 
                        typeof(string),  
                        typeof(object).MakeArrayType()
                    },
                    null);

            public static string GetString(string name, params object[] args) => (string)method.Invoke(null, new object[] { name, args });
        }

        #endregion

        /// <summary>
        /// Wrapper class that binds the HInstance of the <see cref="Assembly"/> to the icon resource ID. Encapsulates unmanaged resources.
        /// </summary>
        public struct IconResource : MSGBOXPARAMS.IIconResource
        {
            private readonly IntPtr hinstance;
            private readonly IntPtr iconId;

            private const int IDI_APPLICATION = 32512;

            /// <summary>
            /// Returns the default resource known as IDI_APPLICATION of the main process. 
            /// In the application, this is the default icon, setted in the project settings.
            /// </summary>
            public static IconResource Application = new IconResource(Assembly.GetEntryAssembly(), IDI_APPLICATION);

            #region MSGBOXPARAMS.IIconResource

            IntPtr MSGBOXPARAMS.IIconResource.HInstance => hinstance;

            IntPtr MSGBOXPARAMS.IIconResource.IconId => iconId;

            #endregion

            private IconResource(Assembly assembly, ushort iconId)
            {
                hinstance = Marshal.GetHINSTANCE(assembly.ManifestModule);
                this.iconId = (IntPtr)iconId;
            }

            /// <summary>
            /// Converts a resource into an icon.
            /// </summary>
            /// <returns>Returns <see cref="Icon"/> of the corresponding resource.</returns>
            public Icon ToIcon() => Icon.FromHandle(LoadIcon(hinstance, iconId));

            /// <summary>
            /// Loads unmanaged icon resources from a specific assembly for future use.
            /// </summary>
            public static IconResource Load(Assembly assembly, ushort iconId) => new IconResource(assembly, iconId);
        }

        private static DialogResultEx InternalShow(IWin32Window owner, 
            string text, 
            string caption, 
            MessageBoxButtonsEx buttons, 
            MessageBoxIcon icon,
            MessageBoxDefaultButton defaultButton, 
            MessageBoxOptions options, 
            Action onHelp, 
            MSGBOXPARAMS.IIconResource userIcon, 
            CultureInfo culture)
        {
            int flags = (int)buttons
                    | (int)defaultButton
                    | (int)options
                    | (int)icon;

            DialogResultEx result;
            var mbParams = new MSGBOXPARAMS(owner, text, caption, flags, onHelp, userIcon, culture);
            ApplicationHook.BeginModalMessageLoop();
            try
            {
                result = (DialogResultEx)InternalShow(in mbParams);
            }
            finally
            {
                ApplicationHook.EndModalMessageLoop();
            }
            SendMessage(mbParams.hwndOwner, WM_SETFOCUS, IntPtr.Zero, IntPtr.Zero);
            return result;
        }

        public static DialogResultEx Show(IWin32Window owner, 
            string text, 
            string caption, 
            MessageBoxButtonsEx buttons = MessageBoxButtonsEx.OK, 
            MessageBoxIcon icon = MessageBoxIcon.None,
            MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1, 
            MessageBoxOptions options = 0, 
            Action onHelp = null, 
            CultureInfo culture = null)
        {
            return InternalShow(owner, text, caption, buttons, icon, defaultButton, options, onHelp, null, culture);
        }

        public static DialogResultEx Show(IWin32Window owner, 
            string text, 
            string caption, 
            MessageBoxButtonsEx buttons, 
            IconResource icon,
            MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1, 
            MessageBoxOptions options = 0, 
            Action onHelp = null, 
            CultureInfo culture = null)
        {
            return InternalShow(owner, text, caption, buttons, 0, defaultButton, options, onHelp, icon, culture);
        }

        public static DialogResultEx Show(string text, 
            string caption, 
            MessageBoxButtonsEx buttons = MessageBoxButtonsEx.OK, 
            MessageBoxIcon icon = MessageBoxIcon.None, 
            Action onHelp = null)
        {
            return InternalShow(null, text, caption, buttons, icon, 0, 0, onHelp, null, null);
        }

        public static DialogResultEx Show(string text, 
            string caption, 
            MessageBoxButtonsEx buttons, 
            IconResource icon, 
            Action onHelp = null)
        {
            return InternalShow(null, text, caption, buttons, 0, 0, 0, onHelp, icon, null);
        }

        public static void Show(string text) => Show(text, null, 0, 0, null);
    }
}
