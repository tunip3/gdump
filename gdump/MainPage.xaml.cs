using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Popups;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace gdump
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");
            Windows.Storage.StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                path.Text = folder.Path;
            }
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            working.Visibility = Visibility.Visible;
            await RecursivelyCreateDirectoryAsync(path.Text);
            StorageFolder storageFolder = await StorageFolder.GetFolderFromPathAsync(path.Text);
            var odd = PinvokeFilesystem.GetItems("O:");
            await CopyFolderAsync(odd, storageFolder, "game");
            await new MessageDialog("Complete").ShowAsync();
        }

        private void TextBlock_SelectionChanged_1(object sender, RoutedEventArgs e)
        {

        }

        //copied from durango ftp
        public async Task<bool> ItemExists(string path)
        {
            string ParentPath = System.IO.Path.GetDirectoryName(path);
            string FileName = System.IO.Path.GetFileName(path);
            IStorageItem file = null;
            try
            {
                var folder = await StorageFolder.GetFolderFromPathAsync(ParentPath);
                file = await folder.TryGetItemAsync(FileName);
            }
            catch { }
            return file != null;
        }

        //copied from durango ftp
        public async Task<bool> FolderExists(string path)
        {
            StorageFolder folder = null;
            try
            {
                folder = await StorageFolder.GetFolderFromPathAsync(path);
            }
            catch { }
            return folder != null;
        }

        //also copied from durango ftp
        public async Task RecursivelyCreateDirectoryAsync(string path)
        {
            int parentcount = 0;
            bool hitbase = false;
            var itemexists = await FolderExists(path);
            var folderexists = itemexists;
            while (!folderexists)
            {
                string TempPath = path;
                for (int i = 0; i < parentcount; i++)
                {
                    TempPath = Path.Combine(TempPath, "..");
                }
                TempPath = new Uri(TempPath).LocalPath;
                if (TempPath.EndsWith("\\"))
                {
                    TempPath = TempPath.Substring(0, TempPath.Length - "\\".Length);
                }
                var TempParentPath = new Uri(Path.Combine(TempPath, "..")).LocalPath;
                if (TempParentPath.EndsWith("\\"))
                {
                    TempParentPath = TempParentPath.Substring(0, TempParentPath.Length - "\\".Length);
                }
                if (TempParentPath == TempPath || TempPath == null)
                {
                    throw new InvalidOperationException("messed up somewhere chief");
                }
                itemexists = await FolderExists(TempParentPath);
                var TempParentPathExists = itemexists;
                if (TempParentPathExists)
                {
                    string name = Path.GetFileName(TempPath);
                    StorageFolder parent = await StorageFolder.GetFolderFromPathAsync(TempParentPath);
                    await parent.CreateFolderAsync(name);
                    hitbase = true;
                }
                if (!hitbase)
                {
                    parentcount++;
                }
                else
                {
                    parentcount--;
                }
                itemexists = await FolderExists(path);
                folderexists = itemexists;
            }
        }



        //thanks to jerome s for this short method
        public static async Task CopyFolderAsync(List<MonitoredFolderItem> source, StorageFolder destinationContainer, string desiredName = null)
        {
            string name = System.IO.Path.GetFileName(source[0].ParentFolderPath);
            StorageFolder destinationFolder = null;
            destinationFolder = await destinationContainer.CreateFolderAsync(
                desiredName ?? name, CreationCollisionOption.ReplaceExisting);

            foreach (var item in source) 
            {
                if (item.IsDir)
                {
                    string temppath = Path.Combine(item.ParentFolderPath, item.Name);
                    List<MonitoredFolderItem> listing = PinvokeFilesystem.GetItems(temppath);
                    await CopyFolderAsync(listing, destinationFolder);
                } else {
                    string temppath = Path.Combine(item.ParentFolderPath, item.Name);
                    StorageFile tempfile = await StorageFile.GetFileFromPathAsync(temppath);
                    await tempfile.CopyAsync(destinationFolder, tempfile.Name, NameCollisionOption.ReplaceExisting);

                }
            }
        }
    }
}
