using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;
using Business;
using DataAccess;
using System.Windows.Media;

namespace NaturalGroundingPlayer {
    /// <summary>
    /// Interaction logic for RatingViewerControl.xaml
    /// </summary>
    public partial class RatingViewerControl : UserControl {
        public RatingViewerControl() {
            InitializeComponent();
            this.DataContext = null;
        }

        private EditRatingsBusiness business;
        private double? currentPreference;
        private Media displayVideo;
        private double ratio = 0;

        /// <summary>
        /// Gets or sets the video to display ratings for.
        /// Preference is updated into the database when changing this property.
        /// </summary>
        public Media Video {
            get {
                return displayVideo;
            }
            set {
                UpdatePreference();
                displayVideo = value;
                currentPreference = displayVideo.Preference;
                PreferenceText.IsEnabled = true;
                business = new EditRatingsBusiness(displayVideo, ratio);
                this.DataContext = business;
                DisplayData();
                DisplayToolTips();
            }
        }

        public double Ratio {
            get { return ratio; }
            set {
                ratio = value;
                if (business != null) {
                    business.Ratio = value;
                    DisplayData();
                }
            }
        }

        private void DisplayData() {
            DisplayValue(PMText, business.PM, Ratio);
            DisplayValue(PFText, business.PF, Ratio);
            DisplayValue(EMText, business.EM, Ratio);
            DisplayValue(EFText, business.EF, Ratio);
            DisplayValue(SMText, business.SM, Ratio);
            DisplayValue(SFText, business.SF, Ratio);
            DisplayValue(LoveText, business.Love, Ratio);
            DisplayValue(EgolessText, business.Egoless, Ratio);
            DisplayValue(Custom1Text, business.Custom1, Ratio);
            DisplayValue(Custom2Text, business.Custom2, Ratio);

            // Display intensity.
            RatingConverter Conv = new RatingConverter();
            RatingToColorConverter ColorConv = new RatingToColorConverter();
            double? IntensityValue = business.GetIntensity();
            IntensityText.Text = Conv.Convert(IntensityValue, typeof(string), null, null).ToString();
            IntensityText.Foreground = (SolidColorBrush)ColorConv.Convert(IntensityValue, typeof(SolidColorBrush), null, null);
        }

        public static void DisplayValue(TextBlock obj, MediaRating rating, double ratio) {
            RatingConverter Conv = new RatingConverter();
            RatingToColorConverter ColorConv = new RatingToColorConverter();
            double? Value = rating.GetValue(ratio);
            obj.Text = Conv.Convert(Value, typeof(string), null, null).ToString();
            obj.Foreground = (SolidColorBrush)ColorConv.Convert(Value, typeof(SolidColorBrush), null, null);
        }

        private void DisplayToolTips() {
            CreateToolTip(PMText, business.PM);
            CreateToolTip(PFText, business.PF);
            CreateToolTip(EMText, business.EM);
            CreateToolTip(EFText, business.EF);
            CreateToolTip(SMText, business.SM);
            CreateToolTip(SFText, business.SF);
            CreateToolTip(LoveText, business.Love);
            CreateToolTip(EgolessText, business.Egoless);
            CreateToolTip(Custom1Text, business.Custom1);
            CreateToolTip(Custom2Text, business.Custom2);

            // Add Intensity tooltip
            ToolTip NewToolTip = new ToolTip();
            IntensityText.ToolTip = NewToolTip;
            StackPanel NewPanel = new StackPanel();
            NewToolTip.Content = NewPanel;
            TextBlock NewTextBlock = new TextBlock();
            NewTextBlock.TextAlignment = TextAlignment.Center;
            NewPanel.Children.Add(NewTextBlock);

            List<double> ToolTipValues = business.HighestValues();
            if (ToolTipValues.Count() > 0) {
                NewTextBlock.Inlines.Add(new Run(string.Format("{0} highest", ToolTipValues.Count())));
                NewTextBlock.Inlines.Add(new LineBreak());
                string ToolTipText = string.Format("({0}) ÷ {1} = {2:0.0}", 
                    string.Join("×", ToolTipValues.Select(v => v.ToString("0.0"))), 
                    ToolTipValues.Count(),
                    business.GetIntensity());
                NewTextBlock.Inlines.Add(new Run(ToolTipText));
            } else
                IntensityText.ToolTip = null;
        }

        private void CreateToolTip(TextBlock ctrl, MediaRating rating) {
            if (rating.Height.HasValue || rating.Depth.HasValue) {
                ToolTip NewToolTip = new ToolTip();
                ctrl.ToolTip = NewToolTip;
                StackPanel NewPanel = new StackPanel();
                NewToolTip.Content = NewPanel;
                TextBlock NewTextBlock = new TextBlock();
                NewTextBlock.TextAlignment = TextAlignment.Center;
                NewPanel.Children.Add(NewTextBlock);

                if (rating.Height.HasValue) {
                    NewTextBlock.Inlines.Add(new Run("Height: " + rating.Height.Value));
                    NewTextBlock.Inlines.Add(new LineBreak());
                }
                if (rating.Depth.HasValue) {
                    NewTextBlock.Inlines.Add(new Run("Depth: " + rating.Depth.Value));
                    NewTextBlock.Inlines.Add(new LineBreak());
                }
                NewTextBlock.Inlines.Add(new Run(string.Format("{0} × {1} × 0.11 = {2:0.0}",
                    rating.Height.HasValue ? rating.Height.Value : rating.Depth.Value,
                    rating.Depth.HasValue ? rating.Depth.Value : rating.Height.Value,
                    rating.GetValue(0))));
            } else
                ctrl.ToolTip = null;
        }

        /// <summary>
        /// Updates the Preference field if edited.
        /// </summary>
        /// <returns>True if data was edited and updated.</returns>
        public bool UpdatePreference() {
            if (business != null) {
                if (PreferenceText.IsFocused)
                    PreferenceText.GetBindingExpression(TextBox.TextProperty).UpdateSource();

                if (business.Video.Preference != currentPreference) {
                    business.UpdatePreference(business.Video);
                    currentPreference = business.Video.Preference;
                    return true;
                }
            }
            return false;
        }
    }
}
