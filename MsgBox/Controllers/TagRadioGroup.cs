using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MsgBox.Controllers
{
    public sealed class TagRadioGroup : CustomControlGroup<RadioButton>
    {
        public TagRadioGroup(FlowLayoutPanel containter)
            : base(containter) { }

        protected override void InitialzeControl(RadioButton control, dynamic data)
        {
            control.Text = data.Text;
            control.Tag = data.Tag;
            control.AutoSize = true;
        }

        public object GetSelected() => Controls.First(s => s.Checked).Tag;

        public void SetSelected(object tag)
        {
            var control = Controls.FirstOrDefault(c => Equals(c.Tag, tag)); 
            if (control != null)
                control.Checked = true;
        }

        protected override IEqualityComparer<dynamic> GetEqualityComparer() => EqualityComparerByTag.Default;

        private void DoCheckedChanged(object button, EventArgs _) => OnCheckedChanged?.Invoke((RadioButton)button);

        event Action<RadioButton> OnCheckedChanged;
    }
}
