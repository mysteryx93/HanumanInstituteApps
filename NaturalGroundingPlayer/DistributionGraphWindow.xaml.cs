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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Business;
using DataAccess;

namespace NaturalGroundingPlayer {
    /// <summary>
    /// Interaction logic for DistributionGraph.xaml
    /// </summary>
    public partial class DistributionGraphWindow : Window {
        private DistributionGraphBusiness business = new DistributionGraphBusiness();
        private WindowHelper helper;

        /// <summary>
        /// Displays the Distribution Graph window.
        /// </summary>
        public static DistributionGraphWindow Instance() {
            DistributionGraphWindow NewForm = new DistributionGraphWindow();
            SessionCore.Instance.Windows.Show(NewForm);
            return NewForm;
        }

        public DistributionGraphWindow() {
            InitializeComponent();
            helper = new WindowHelper(this);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e) {
            GraphTypeCombo.ItemsSource = await business.GetRatingCategoriesAsync();
            //MediaList.Settings.ConditionField = FieldConditionEnum.IsInDatabase;
            //MediaList.Settings.ConditionValue = BoolConditionEnum.Yes;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private async void GraphTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            RatingCategory Selection = GraphTypeCombo.SelectedItem as RatingCategory;
            if (Selection != null && Selection.Name != "" && Selection.Name[0] != '-') {
                List<int> GraphData = await business.LoadGraphAsync(MediaType.Video, Selection, 0);
                DisplayGraph(GraphData);
            } else
                DisplayGraph(null);
            MediaList.Clear();
        }

        public void DisplayGraph(List<int> data) {
            // If data is null, generate blank graph.
            if (data == null) {
                data = new List<int>();
                for (int i = 0; i < 14; i++) {
                    data.Add(0);
                }
            }

            int Highest = data.Max();
            DisplayBar(Bar1, BarText1, data[0], Highest);
            DisplayBar(Bar2, BarText2, data[1], Highest);
            DisplayBar(Bar3, BarText3, data[2], Highest);
            DisplayBar(Bar4, BarText4, data[3], Highest);
            DisplayBar(Bar5, BarText5, data[4], Highest);
            DisplayBar(Bar6, BarText6, data[5], Highest);
            DisplayBar(Bar7, BarText7, data[6], Highest);
            DisplayBar(Bar8, BarText8, data[7], Highest);
            DisplayBar(Bar9, BarText9, data[8], Highest);
            DisplayBar(Bar10, BarText10, data[9], Highest);
            DisplayBar(Bar11, BarText11, data[10], Highest);
            DisplayBar(Bar12, BarText12, data[11], Highest);
            DisplayBar(Bar13, BarText13, data[12], Highest);
            DisplayBar(Bar14, BarText14, data[13], Highest);
        }

        private void DisplayBar(Rectangle bar, TextBlock barText, int value, int highest) {
            // Calculate the height of the bar.
            double Height;
            if (highest > 0)
                Height = (GraphContainer.ActualHeight - 20) / highest * value;
            else
                Height = 0;

            bar.Height = Height;

            // Changing the margin must be done this way.
            Thickness TextMargin = barText.Margin;
            TextMargin.Top = GraphContainer.ActualHeight - Height - 19;
            TextMargin.Bottom = 0;
            barText.Margin = TextMargin;

            barText.Text = value.ToString();
        }

        /// <summary>
        /// Loads the list of videos when clicking a bar.
        /// </summary>
        private void GraphContainer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            Point Pos = e.GetPosition(GraphContainer);

            foreach (Rectangle item in GraphContainer.Children.OfType<Rectangle>()) {
                double left = item.Margin.Left;
                double right = left + item.ActualWidth;
                if (Pos.X >= left && Pos.X <= right) {
                    if (item == Bar1)
                        LoadVideos(business.bars[0]);
                    else if (item == Bar2)
                        LoadVideos(business.bars[1]);
                    else if (item == Bar3)
                        LoadVideos(business.bars[2]);
                    else if (item == Bar4)
                        LoadVideos(business.bars[3]);
                    else if (item == Bar5)
                        LoadVideos(business.bars[4]);
                    else if (item == Bar6)
                        LoadVideos(business.bars[5]);
                    else if (item == Bar7)
                        LoadVideos(business.bars[6]);
                    else if (item == Bar8)
                        LoadVideos(business.bars[7]);
                    else if (item == Bar9)
                        LoadVideos(business.bars[8]);
                    else if (item == Bar10)
                        LoadVideos(business.bars[9]);
                    else if (item == Bar11)
                        LoadVideos(business.bars[10]);
                    else if (item == Bar12)
                        LoadVideos(business.bars[11]);
                    else if (item == Bar13)
                        LoadVideos(business.bars[12]);
                    else if (item == Bar14)
                        LoadVideos(business.bars[13]);
                }
            }
        }

        private async void LoadVideos(float[] minMax) {
            MediaList.Settings.RatingFilters.Clear();
            string Name = ((RatingCategory)GraphTypeCombo.SelectedItem).Name;
            if (!string.IsNullOrEmpty(Name)) {
                MediaList.Settings.RatingFilters.Add(new SearchRatingSetting() {
                    Category = Name,
                    Operator = OperatorConditionEnum.GreaterOrEqual,
                    Value = minMax[0] + .01f // It should be > instead of >=, so this fixed it.
                });
                MediaList.Settings.RatingFilters.Add(new SearchRatingSetting() {
                    Category = Name,
                    Operator = OperatorConditionEnum.Smaller,
                    Value = minMax[1] + .01f // It should be <= instead of <, so this fixed it.
                });
                await MediaList.LoadDataAsync();
            }
        }
    }
}
