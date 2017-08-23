using System;

namespace EmergenceGuardian.WpfCommon {
    /// <summary>
    /// Represents an item to display in the ComboBox.
    /// </summary>
    public class ListItem<T> {
        public string Text { get; set; }
        public T Value { get; set; }

        public ListItem() {
        }

        public ListItem(string text, T value) {
            this.Text = text;
            this.Value = value;
        }
    }
}
