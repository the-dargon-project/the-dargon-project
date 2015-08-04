using Dargon.Client.ViewModels;
using ItzWarty.Collections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Color = System.Windows.Media.Color;

namespace Dargon.Client.Views {
   /// <summary>
   /// Interaction logic for ModificationEntryProgressIndicator.xaml
   /// </summary>
   public partial class ModificationEntryProgressIndicator : UserControl {
      private readonly IReadOnlyDictionary<ModificationEntryStatus, string> colorKeysByStatus = ImmutableDictionary.Of<ModificationEntryStatus, string>(
         ModificationEntryStatus.None, "ModificationDisabledColor",
         ModificationEntryStatus.Enabled, "ModificationEnabledColor",
         ModificationEntryStatus.Disabled, "ModificationDisabledColor",
         ModificationEntryStatus.Broken, "ModificationBrokenColor",
         ModificationEntryStatus.UpdateAvailable, "ModificationUpdateAvailableColor",
         ModificationEntryStatus.Updating, "ModificationUpdatingColor"
      );

      private Color lastColor;

      public ModificationEntryProgressIndicator() {
         InitializeComponent();
      }

      // DataContext randomly changes to MS.Internal.NamedObject 
      // see: http://stackoverflow.com/questions/20124604/cant-convert-object-of-type-ms-internal-namedobject-to-system-windows-datatempl
      private ModificationViewModel viewModelCache = null;
      public ModificationViewModel ViewModel => (viewModelCache ?? (viewModelCache = DataContext as ModificationViewModel));

      private bool firstRender = true;
      protected override void OnRender(DrawingContext drawingContext) {
         base.OnRender(drawingContext);
         if (firstRender && ViewModel != null) {
            firstRender = false;
            fred.Background = new SolidColorBrush();
            ViewModel.PropertyChanged += HandleViewModelPropertyChanged;
            UpdateProgressBarProgress();
            UpdateProgressBarColor(true);
         }
      }

      private void HandleViewModelPropertyChanged(object sender, PropertyChangedEventArgs e) {
         switch (e.PropertyName) {
            case nameof(ViewModel.StatusProgress):
               UpdateProgressBarProgress();
               break;
            case nameof(ViewModel.Status):
               UpdateProgressBarColor(false);
               break;
         }
      }

      private void UpdateProgressBarProgress() {
         Dispatcher.BeginInvoke(new Action(() => {
            fred.Width = ActualWidth * ViewModel.StatusProgress;
         }), DispatcherPriority.Send);
      }

      private void UpdateProgressBarColor(bool firstRun) {
         if (firstRun) {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
               var nextColor = GetCurrentStatusColor();
               lastColor = nextColor;
               ((SolidColorBrush)fred.Background).Color = nextColor;
            }), DispatcherPriority.Send);
         } else {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
               var nextColor = GetCurrentStatusColor();
               if (nextColor != lastColor) {
                  var storyboard = new Storyboard();
                  var colorAnimation = new ColorAnimation(lastColor, nextColor, new Duration(TimeSpan.FromMilliseconds(100)));
                  storyboard.Children.Add(colorAnimation);
                  Storyboard.SetTarget(colorAnimation, fred);
                  Storyboard.SetTargetProperty(colorAnimation, new PropertyPath("(Grid.Background).(SolidColorBrush.Color)"));
                  storyboard.Begin();
                  lastColor = nextColor;
               }
            }), DispatcherPriority.Send);
         }
      }

      private Color GetCurrentStatusColor() {
         string colorKey;
         if (colorKeysByStatus.TryGetValue(ViewModel.Status, out colorKey)) {
            return (Color)FindResource(colorKey);
         } else {
            return Colors.Magenta;
         }
      }
   }
}
