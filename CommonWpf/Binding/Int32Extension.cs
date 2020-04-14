using System;
using System.Windows.Markup;

namespace HanumanInstitute.CommonWpf {
    /// <summary>
    /// Allows to specify integer values in binding expressions.
    /// </summary>
    public sealed class Int32Extension : MarkupExtension {
        public Int32Extension(int value) { this.Value = value; }
        public int Value { get; set; }
        public override Object ProvideValue(IServiceProvider sp) { return Value; }
    };

    /// <summary>
    /// Allows to specify double values in binding expressions.
    /// </summary>
    public sealed class DoubleExtension : MarkupExtension {
        public DoubleExtension(int value) { this.Value = value; }
        public double Value { get; set; }
        public override Object ProvideValue(IServiceProvider sp) { return Value; }
    };
}
