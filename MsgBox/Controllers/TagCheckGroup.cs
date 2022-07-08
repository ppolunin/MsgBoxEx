using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MsgBox.Controllers
{
    public sealed class TagCheckGroup : CustomControlGroup<CheckBox>
    {
        public TagCheckGroup(FlowLayoutPanel containter) 
            : base(containter) { }

        protected override void InitialzeControl(CheckBox control, dynamic data)
        {
            control.Text = data.Text;
            control.Tag = data.Tag;
            control.AutoSize = true;
        }

        public IEnumerable<object> GetSelected() => Controls.Where(c => c.Checked). Select(c => c.Tag);

        public void SetSelected(IEnumerable<object> tags)
        {
            foreach (var control in Controls)
                control.Checked = false;

            var controls = Controls.Join(tags.Distinct().Take(Controls.Count), c => c.Tag, t => t, (c, t) => c);
            foreach (var control in controls)
                control.Checked = true;
        }

        protected override IEqualityComparer<dynamic> GetEqualityComparer() => EqualityComparerByTag.Default;
    }
}
