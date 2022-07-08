using MsgBox.Controllers;
using MsgBox.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MsgBox
{
    public partial class MainForm : Form
    {
        private readonly TagRadioGroup msgBoxBtns;
        private readonly TagRadioGroup msgBoxDef;
        private readonly TagRadioGroup msgBoxIcon;
        private readonly TagRadioGroup msgBoxUserIcon;
        private readonly TagCheckGroup msgBoxOpt;

        public MainForm()
        {
            InitializeComponent();
            Icon = MessageBoxEx.IconResource.Application.ToIcon();

            msgBoxBtns = new TagRadioGroup(flowLayoutPanel1);
            msgBoxBtns.ForEach(GetDataFromEnum<MessageBoxButtonsEx>());
            msgBoxBtns.SetSelected((MessageBoxButtonsEx)default);

            msgBoxDef = new TagRadioGroup(flowLayoutPanel2);
            msgBoxDef.ForEach(GetDataFromEnum<MessageBoxDefaultButton>());
            msgBoxDef.SetSelected((MessageBoxDefaultButton)default);

            msgBoxIcon = new TagRadioGroup(flowLayoutPanel3);
            msgBoxIcon.ForEach(GetDataFromEnum<MessageBoxIcon>());
            msgBoxIcon.SetSelected((MessageBoxIcon)default);

            msgBoxUserIcon = new TagRadioGroup(flowLayoutPanel4);
            msgBoxUserIcon.ForEach(new object[]
            {
                new { Text = nameof(AppRes.IDI_ICON1), Tag = AppRes.IDI_ICON1 },
                new { Text = nameof(AppRes.IDI_ICON2), Tag = AppRes.IDI_ICON2 }
            });
            msgBoxUserIcon.SetSelected(AppRes.IDI_ICON1);

            msgBoxOpt = new TagCheckGroup(flowLayoutPanel5);
            msgBoxOpt.ForEach(GetDataFromEnum<MessageBoxOptions>());

            tabAppRes.Tag = new Action(() =>
            {
                MessageBoxEx.Show(cbUseOwner.Checked ? this : null,
                    Resources.MsgBoxText,
                    Text,
                    (MessageBoxButtonsEx)msgBoxBtns.GetSelected(),
                    AppRes.LoadIcon((ushort)msgBoxUserIcon.GetSelected()),
                    (MessageBoxDefaultButton)msgBoxDef.GetSelected(),
                    CalcFromSelection<MessageBoxOptions>(msgBoxOpt.GetSelected()),
                    cbUseHelp.Checked ? HelpAction : (Action)default);
            });

            tabDefault.Tag = new Action(() =>
            {
                MessageBoxEx.Show(cbUseOwner.Checked ? this : null,
                    Resources.MsgBoxText,
                    Text,
                    (MessageBoxButtonsEx)msgBoxBtns.GetSelected(),
                    (MessageBoxIcon)msgBoxIcon.GetSelected(),
                    (MessageBoxDefaultButton)msgBoxDef.GetSelected(),
                    CalcFromSelection<MessageBoxOptions>(msgBoxOpt.GetSelected()),
                    cbUseHelp.Checked ? HelpAction : (Action)default);
            });
        }

        private void HelpAction()
        {
            MessageBoxEx.Show(Resources.HelpActionText,
                Text,
                MessageBoxButtonsEx.OK,
                AppRes.LoadIcon(AppRes.IDI_SAMPLE));
        }

#pragma warning disable IDE0051

        private static IEnumerable<object> GetObjectFromFlags<E>(E value) where E : Enum
        {
            int test = (int)Enum.ToObject(typeof(E), value);

            return Enum.GetValues(typeof(E))
                .Cast<int>()
                .Where(v => (v & test) == v)
                .Cast<E>()
                .Cast<object>();
        }

#pragma warning restore IDE0051

        private static E CalcFromSelection<E>(IEnumerable<object> selection) where E : Enum
        {
            return (E)Enum.ToObject(typeof(E), selection.Aggregate(0, (result, tag) => result | (int)tag));
        }

        private static IEnumerable<dynamic> GetDataFromEnum<E>() where E : Enum
        {
            return Enum.GetValues(typeof(E))
                .Cast<E>()
                .Distinct()
                .Select(value => new 
                { 
                    Tag = value, 
                    Text = value.ToString() 
                });
        }

        private void Show_Click(object sender, EventArgs e) => ((Action)tabContol.SelectedTab.Tag).Invoke();
    }
}
