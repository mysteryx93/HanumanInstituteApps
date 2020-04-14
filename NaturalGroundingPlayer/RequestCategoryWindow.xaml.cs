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
using EmergenceGuardian.NaturalGroundingPlayer.Business;
using EmergenceGuardian.NaturalGroundingPlayer.DataAccess;

namespace NaturalGroundingPlayer {
    /// <summary>
    /// Interaction logic for RequestCategoryWindow.xaml
    /// </summary>
    public partial class RequestCategoryWindow : Window {
        public static SearchSettings Instance() {
            RequestCategoryWindow NewForm = new RequestCategoryWindow();
            SessionCore.Instance.Windows.ShowDialog(NewForm);
            return NewForm.result;
        }

        private WindowHelper helper;
        private SearchSettings settings = new SearchSettings();
        private SearchSettings result;

        public RequestCategoryWindow() {
            InitializeComponent();
            helper = new WindowHelper(this);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e) {
            this.DataContext = settings;
            EditPlaylistBusiness business = new EditPlaylistBusiness();
            RatingCategoryCombo.ItemsSource = await business.GetRatingCategoriesAsync(false);
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e) {
            OkButton.Focus();
            if (!string.IsNullOrEmpty(settings.Search) || (!string.IsNullOrEmpty(settings.RatingCategory) && settings.RatingValue.HasValue)) {
                result = settings;
                this.Close();                
            }  else {
                ErrorText.Visibility = Visibility.Visible;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }
}
