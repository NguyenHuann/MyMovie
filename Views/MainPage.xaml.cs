using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using Windows.Storage;
using CommunityToolkit.Mvvm.Messaging;
using MyMovie.Models;

namespace MyMovie.Views
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            UpdateGreeting();
            InitializeThemeIcon();

            WeakReferenceMessenger.Default.Register<UsernameChangedMessage>(this, (r, m) =>
            {
                UpdateGreeting(m.Value);
            });

            this.Loaded += (s, e) => {
                if (ContentFrame.Content == null)
                {
                    NavView.SelectedItem = NavView.MenuItems[0];
                    ContentFrame.Navigate(typeof(HomePage));
                }
            };
        }

        private void InitializeThemeIcon()
        {
            var savedTheme = ApplicationData.Current.LocalSettings.Values["AppTheme"]?.ToString();
            ThemeIcon.Glyph = (savedTheme == "Light") ? "\xE708" : "\xE706";
        }

        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.m_window?.Content is FrameworkElement rootElement)
            {
                if (rootElement.RequestedTheme == ElementTheme.Dark ||
                   (rootElement.RequestedTheme == ElementTheme.Default && ApplicationData.Current.LocalSettings.Values["AppTheme"]?.ToString() != "Light"))
                {
                    rootElement.RequestedTheme = ElementTheme.Light;
                    ApplicationData.Current.LocalSettings.Values["AppTheme"] = "Light";
                    ThemeIcon.Glyph = "\xE708";
                }
                else
                {
                    rootElement.RequestedTheme = ElementTheme.Dark;
                    ApplicationData.Current.LocalSettings.Values["AppTheme"] = "Dark";
                    ThemeIcon.Glyph = "\xE706";
                }
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            GreetingText.Visibility = Visibility.Collapsed;
            ThemeButton.Visibility = Visibility.Collapsed;
            SearchButton.Visibility = Visibility.Collapsed;
            SortButton.Visibility = Visibility.Collapsed;

            HeaderSearchBox.Visibility = Visibility.Visible;
            CloseSearchButton.Visibility = Visibility.Visible;
            HeaderSearchBox.Focus(FocusState.Programmatic);
        }

        private void CloseSearchButton_Click(object sender, RoutedEventArgs e)
        {
            HeaderSearchBox.Visibility = Visibility.Collapsed;
            HeaderSearchBox.Text = string.Empty;
            CloseSearchButton.Visibility = Visibility.Collapsed;

            GreetingText.Visibility = Visibility.Visible;
            ThemeButton.Visibility = Visibility.Visible;
            SearchButton.Visibility = Visibility.Visible;
            SortButton.Visibility = Visibility.Visible;

            WeakReferenceMessenger.Default.Send(new SearchMessage(""));
        }

        private void HeaderSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                WeakReferenceMessenger.Default.Send(new SearchMessage(sender.Text.ToLower()));
            }
        }

        private void SortButton_Click(object sender, RoutedEventArgs e)
        {
            var flyout = new MenuFlyout();

            var sortByNameAsc = new MenuFlyoutItem { Text = "Tên phim (A đến Z)", Icon = new FontIcon { Glyph = "\xE74B" } };
            sortByNameAsc.Click += (s, args) => WeakReferenceMessenger.Default.Send(new SortMessage("NameAsc"));

            var sortByNameDesc = new MenuFlyoutItem { Text = "Tên phim (Z đến A)", Icon = new FontIcon { Glyph = "\xE74A" } };
            sortByNameDesc.Click += (s, args) => WeakReferenceMessenger.Default.Send(new SortMessage("NameDesc"));

            flyout.Items.Add(sortByNameAsc);
            flyout.Items.Add(sortByNameDesc);

            flyout.ShowAt((FrameworkElement)sender);
        }

        private void UpdateGreeting(string? userName = null)
        {
            userName ??= ApplicationData.Current.LocalSettings.Values["UserName"]?.ToString() ?? "user";
            int hour = DateTime.Now.Hour;
            string time = hour < 12 ? "Morning" : (hour < 18 ? "Afternoon" : "Evening");
            GreetingText.Text = $"{time}, {userName}!";
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                ContentFrame.Navigate(typeof(SettingsPage));
            }
            else if (args.SelectedItemContainer != null)
            {
                string tag = args.SelectedItemContainer.Tag.ToString()!;
                Type pageType = tag switch
                {
                    "home" => typeof(HomePage),
                    "favorites" => typeof(FavoritesPage),
                    "history" => typeof(HistoryPage),
                    "settings" => typeof(SettingsPage),
                    _ => typeof(HomePage)
                };

                if (ContentFrame.CurrentSourcePageType != pageType) ContentFrame.Navigate(pageType);
            }
        }

        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (ContentFrame.CanGoBack) ContentFrame.GoBack();
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            NavView.IsBackEnabled = ContentFrame.CanGoBack;

            if (e.SourcePageType == typeof(AddMoviePage) ||
                e.SourcePageType == typeof(SettingsPage) ||
                e.SourcePageType == typeof(MovieDetailsPage))
            {
                SearchButton.Visibility = Visibility.Collapsed;
                SortButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                SearchButton.Visibility = Visibility.Visible;
                SortButton.Visibility = Visibility.Visible;
            }

            if (e.SourcePageType.Name.Contains("PlayerPage"))
            {
                HeaderPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                HeaderPanel.Visibility = Visibility.Visible;
                if (HeaderSearchBox.Visibility == Visibility.Visible) CloseSearchButton_Click(null!, null!);
                UpdateGreeting();
            }
        }
    }
}