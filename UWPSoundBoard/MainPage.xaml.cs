using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UWPSoundBoard.Model;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWPSoundBoard
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ObservableCollection<Sound> Sounds;
        private List<MenuItem> MenuItems;
        private List<string> Suggestions;

        public MainPage()
        {
            this.InitializeComponent();

            Sounds = new ObservableCollection<Sound>();
            SoundManager.GetAllSounds(Sounds);

            MenuItems = new List<MenuItem>();
            MenuItems.Add(new MenuItem { IconFile = "Assets/Icons/animals.png", Category = SoundCategory.Animals });
            MenuItems.Add(new MenuItem { IconFile = "Assets/Icons/cartoon.png", Category = SoundCategory.Cartoons });
            MenuItems.Add(new MenuItem { IconFile = "Assets/Icons/taunt.png", Category = SoundCategory.Taunts });
            MenuItems.Add(new MenuItem { IconFile = "Assets/Icons/warning.png", Category = SoundCategory.Warnings });

            BackButton.Visibility = Visibility.Collapsed;
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e) {
            MainSplitView.IsPaneOpen = !MainSplitView.IsPaneOpen;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) {
            SoundManager.GetAllSounds(Sounds);
            MenuItemsListView.SelectedItem = null;
            CategoryTextBlock.Text = "All Sounds";
            BackButton.Visibility = Visibility.Collapsed;
        }

        private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args) {
            SoundManager.GetSoundsByName(Sounds, sender.Text);
            CategoryTextBlock.Text = sender.Text;
            BackButton.Visibility = Visibility.Visible;
            MenuItemsListView.SelectedItem = null;
        }

        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) {
            if (Sounds.Count == 0) {
                SoundManager.GetAllSounds(Sounds);
            }
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput) {
                Suggestions = Sounds.Where(p => p.Name.StartsWith(sender.Text)).Select(p => p.Name).ToList();
                SearchBox.ItemsSource = Suggestions;
            }
        }

        private void MenuItemsListView_ItemClick(object sender, ItemClickEventArgs e) {
            var menuItem = e.ClickedItem as MenuItem;

            // filter on category
            CategoryTextBlock.Text = menuItem.Category.ToString();
            SoundManager.GetSoundsByCategory(Sounds, menuItem.Category);

            BackButton.Visibility = Visibility.Visible;
            MainSplitView.IsPaneOpen = false;
        }

        private void SoundGridView_ItemClick(object sender, ItemClickEventArgs e) {
            var sound = e.ClickedItem as Sound;
            SoundMediaElement.Source = new Uri(this.BaseUri, sound.AudioFile);
        }

        private async void SoundGridView_Drop(object sender, DragEventArgs e) {
            if (e.DataView.Contains(StandardDataFormats.StorageItems)) {
                var items = await e.DataView.GetStorageItemsAsync();

                if (items.Any()) {
                    var storageFile = items[0] as StorageFile;
                    var contentType = storageFile.ContentType;

                    var folder = ApplicationData.Current.LocalFolder;

                    if (contentType == "audio/wav" || contentType == "audio/mpeg") {
                        var newFile = await storageFile.CopyAsync(folder, storageFile.Name);

                        SoundMediaElement.SetSource(await storageFile.OpenAsync(FileAccessMode.Read), contentType);
                        SoundMediaElement.Play();
                    }
                }
            }
        }

        private void SoundGridView_DragOver(object sender, DragEventArgs e) {
            e.AcceptedOperation = DataPackageOperation.Copy;

            e.DragUIOverride.Caption = "Drag and drop to play this sound";
            e.DragUIOverride.IsCaptionVisible = true;
            e.DragUIOverride.IsContentVisible = true;
            e.DragUIOverride.IsGlyphVisible = true;
        }
    }
}
