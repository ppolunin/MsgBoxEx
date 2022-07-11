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
            Icon = MessageBoxEx.IconResource.Application.ToSmallIcon();

            msgBoxDef = new TagRadioGroup(flowLayoutPanel2);
            msgBoxDef.ForEach(GetDataFromEnum<MessageBoxDefaultButtonEx>());
            msgBoxDef.SetSelectedByTag((MessageBoxDefaultButtonEx)default);

            msgBoxBtns = new TagRadioGroup(flowLayoutPanel1);
            msgBoxBtns.ForEach(GetDataFromEnum<MessageBoxButtonsEx>());
            msgBoxBtns.CheckedChanged += MsgBoxBtns_CheckedChanged;
            msgBoxBtns.SetSelectedByTag((MessageBoxButtonsEx)default);

            msgBoxIcon = new TagRadioGroup(flowLayoutPanel3);
            msgBoxIcon.ForEach(GetDataFromEnum<MessageBoxIcon>());
            msgBoxIcon.SetSelectedByTag((MessageBoxIcon)default);

            msgBoxUserIcon = new TagRadioGroup(flowLayoutPanel4);
            msgBoxUserIcon.ForEach(GetDataFromEnum<AppRes>());
            msgBoxUserIcon.SetSelectedByTag(AppRes.idiIcon1);

            msgBoxOpt = new TagCheckGroup(flowLayoutPanel5);
            msgBoxOpt.ForEach(GetDataFromEnum<MessageBoxOptions>());

            cbUseOwner.CheckedChanged += ConfigureOpts;
            cbUseHelp.CheckedChanged += ConfigureOpts;
            cbUseHelp.CheckedChanged += UseHelp_CheckedChanged;


            tabAppRes.Tag = new Action(() =>
            {
                MessageBoxEx.Show(cbUseOwner.Checked ? this : null,
                    Resources.MsgBoxText,
                    Text,
                    (MessageBoxButtonsEx)msgBoxBtns.GetSelectedByTag(),
                    ((AppRes)msgBoxUserIcon.GetSelectedByTag()).Load(),
                    (MessageBoxDefaultButtonEx)msgBoxDef.GetSelectedByTag(),
                    CalcFromSelection<MessageBoxOptions>(msgBoxOpt.GetSelected()),
                    cbUseHelp.Checked ? HelpAction : (Action)default);
            });

            tabDefault.Tag = new Action(() =>
            {
                MessageBoxEx.Show(cbUseOwner.Checked ? this : null,
                    Resources.MsgBoxText,
                    Text,
                    (MessageBoxButtonsEx)msgBoxBtns.GetSelectedByTag(),
                    (MessageBoxIcon)msgBoxIcon.GetSelectedByTag(),
                    (MessageBoxDefaultButtonEx)msgBoxDef.GetSelectedByTag(),
                    CalcFromSelection<MessageBoxOptions>(msgBoxOpt.GetSelected()),
                    cbUseHelp.Checked ? HelpAction : (Action)default);
            });
        }

        private void MsgBoxDefEnableButtons(int count)
        {
            count = Math.Min(count, msgBoxDef.Controls.Count);
            int index = 0;
            while (index < count)
                msgBoxDef.Controls[index++].Enabled = true;
            while (index < msgBoxDef.Controls.Count)
                msgBoxDef.Controls[index++].Enabled = false;
        }

        private void MsgBoxBtns_CheckedChanged(RadioButton obj)
        {
            msgBoxDef.SetSelectedByTag((MessageBoxDefaultButtonEx)default);

            int count;
            switch ((MessageBoxButtonsEx)obj.Tag)
            {
                case MessageBoxButtonsEx.OK:
                    count = 1;
                    break;

                case MessageBoxButtonsEx.OKCancel:
                case MessageBoxButtonsEx.YesNo:
                case MessageBoxButtonsEx.RetryCancel:
                    count = 2;
                    break;

                case MessageBoxButtonsEx.AbortRetryIgnore:
                case MessageBoxButtonsEx.YesNoCancel:
                case MessageBoxButtonsEx.CancelRetryContinue:
                    count = 3;
                    break;

                default:
                    return;
            }

            if (cbUseHelp.Checked == true)
                count++;

            MsgBoxDefEnableButtons(count);
        }

        private void HelpAction()
        {
            MessageBoxEx.Show(Resources.HelpActionText,
                Text,
                MessageBoxButtonsEx.OK,
                AppRes.idiSample.Load());
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

        private void UseHelp_CheckedChanged(object sender, EventArgs e) => MsgBoxBtns_CheckedChanged(msgBoxBtns.GetSelected());

        private void ConfigureOpts(object sender, EventArgs e)
        {
            foreach (var button in msgBoxOpt.Controls)
                button.Checked = false;

            var buttons = msgBoxOpt.Controls.Join(new object[]
                {
                    MessageBoxOptions.DefaultDesktopOnly,
                    MessageBoxOptions.ServiceNotification
                },
                button => button.Tag,
                inKey => inKey,
                (button, key) => button);

            foreach (var button in buttons)
                button.Enabled = cbUseHelp.Checked == false && cbUseOwner.Checked == false;
        }
    }
}
