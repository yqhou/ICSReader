using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Appointments;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace ICSReader
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Appointment appointment;
        public  MainPage current;
        public MainPage()
        {
            this.InitializeComponent();
            current = this;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e != null && e.Parameter != null)
            {
                var file = e.Parameter as StorageFile;
                if( file!= null)
                    ImportIcsFile(file);
            }
        }

        private async void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.List;
            openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            openPicker.FileTypeFilter.Add(".ics");
            StorageFile file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                FileNameTextBlock.Text = file.Path;
                var lines = await FileIO.ReadLinesAsync(file);
                appointment = Model.ICSReader.Read(lines);
                var rect = GetElementRect(sender as FrameworkElement);
                var appointmentId = await AppointmentManager.ShowAddAppointmentAsync(appointment, rect, Windows.UI.Popups.Placement.Default);
                if( ! string.IsNullOrEmpty(appointmentId))
                {
                    StatusTextBlock.Text = "事件添加成功.";
                }
                else
                {
                    StatusTextBlock.Text = "事件未成功添加.";
                }
                
            }
            else
                FileNameTextBlock.Text = "operation cancelled";
            //var tzi = TimeZoneInfo.GetSystemTimeZones();
        }

        private async void ImportIcsFile(StorageFile file)
        {
            if (file == null)
                return;
            FileNameTextBlock.Text = file.Path;
            var lines = await FileIO.ReadLinesAsync(file);
            appointment = Model.ICSReader.Read(lines);
            var rect = GetElementRect(SelectFileButton as FrameworkElement);
            var appointmentId = await AppointmentManager.ShowAddAppointmentAsync(appointment, rect, Windows.UI.Popups.Placement.Default);
            if (!string.IsNullOrEmpty(appointmentId))
            {
                StatusTextBlock.Text = "事件添加成功.";
            }
            else
            {
                StatusTextBlock.Text = "事件未成功添加.";
            }
        }

        public  void ImportIcsFileStatic(StorageFile file )
        {
            ImportIcsFile(file);
        }

        private Rect GetElementRect(FrameworkElement frameworkElement)
        {
            Windows.UI.Xaml.Media.GeneralTransform transform = frameworkElement.TransformToVisual(null);
            Point point = transform.TransformPoint(new Point());
            return new Rect(point, new Size(frameworkElement.ActualWidth, frameworkElement.ActualHeight));
        }
    }
}
