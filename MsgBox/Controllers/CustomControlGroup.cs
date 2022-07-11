using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MsgBox.Controllers
{
    public abstract class CustomControlGroup<Ctrl> where Ctrl : Control, new()
    {
        private readonly struct Container : IReadOnlyList<Ctrl>
        {
            private readonly FlowLayoutPanel containter;

            public Container(FlowLayoutPanel containter) => this.containter = containter;

            public Ctrl this[int index] => (Ctrl)containter.Controls[index];

            public int Count => containter.Controls.Count;

            public IEnumerator<Ctrl> GetEnumerator()
            {
                var iterator = containter.Controls.GetEnumerator();
                while (iterator.MoveNext())
                    yield return (Ctrl)iterator.Current;
            }

            IEnumerator IEnumerable.GetEnumerator() => containter.Controls.GetEnumerator();

            public void Add(Ctrl control) => containter.Controls.Add(control);
        }

        private readonly Container container;

        public CustomControlGroup(FlowLayoutPanel containter)
        {
            if (containter.HasChildren)
                throw new ArgumentException("The container must be empty.");

            this.container = new Container(containter);
            containter.ControlAdded += OnControlAdded;
            containter.ControlRemoved += OnControlRemoved;
        }

        private void OnControlRemoved(object sender, ControlEventArgs e) => OnControlRemoved((Ctrl)e.Control);

        private void OnControlAdded(object sender, ControlEventArgs e) => OnControlAdded((Ctrl)e.Control);

        protected abstract void InitialzeControl(Ctrl control, dynamic data);

        protected abstract IEqualityComparer<dynamic> GetEqualityComparer();

        protected virtual void OnControlsInitialized() { }

        protected virtual void OnControlRemoved(Ctrl control) { }

        protected virtual void OnControlAdded(Ctrl control) { }

        protected IEnumerable<Ctrl> GetByTag(object tag) => container.Where(c => Equals(c.Tag, tag));

        public void ForEach(IEnumerable<dynamic> data)
        {
            foreach (dynamic item in data.Distinct(GetEqualityComparer()))
            {
                var control = new Ctrl();
                InitialzeControl(control, item);
                container.Add(control);
            }

            OnControlsInitialized();
        }

        public IReadOnlyList<Ctrl> Controls => container;
    }
}
