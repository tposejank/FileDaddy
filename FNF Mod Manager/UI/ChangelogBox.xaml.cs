﻿using Microsoft.Win32;
using System;
using System.IO;
using System.Media;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Reflection;

namespace FNF_Mod_Manager.UI
{
    /// <summary>
    /// Interaction logic for ChangelogBox.xaml
    /// </summary>
    public partial class ChangelogBox : Window
    {
        public bool YesNo = false;
        public bool Skip = false;

        // TODO: Check how FileDaddy update looks like
        public ChangelogBox(GameBananaItemUpdate update, string packageName, string text, Uri preview, bool skip = false)
        {
            InitializeComponent();
            if (preview != null)
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = preview;
                bitmap.EndInit();
                PreviewImage.Source = bitmap;
                PreviewImage.Visibility = Visibility.Visible;
            }
            else
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                Stream iconStream = asm.GetManifestResourceStream("FNF_Mod_Manager.Assets.fdpreview.png");
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = iconStream;
                bitmap.EndInit();
                PreviewImage.Source = bitmap;
                PreviewImage.Visibility = Visibility.Visible;
            }
            ChangesGrid.ItemsSource = update.Changes;
            Title = $"{packageName} Changelog";
            VersionLabel.Content = $"Update: {update.Title}";
            Text.Text = text;
            // Remove html tags
            UpdateText.Text = Regex.Replace(update.Text, "<.*?>", string.Empty).Replace("&nbsp;", " ");
            if (skip)
                SkipButton.Visibility = Visibility.Visible;
            else
            {
                Grid.SetColumnSpan(YesButton, 2);
                Grid.SetColumnSpan(NoButton, 2);
            }
            PlayNotificationSound();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            YesNo = true;
            Close();
        }
        private void Skip_Button_Click(object sender, RoutedEventArgs e)
        {
            Skip = true;
            Close();
        }

        public void PlayNotificationSound()
        {
            bool found = false;
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"AppEvents\Schemes\Apps\.Default\Notification.Default\.Current"))
                {
                    if (key != null)
                    {
                        Object o = key.GetValue(null); // pass null to get (Default)
                        if (o != null)
                        {
                            SoundPlayer theSound = new SoundPlayer((String)o);
                            theSound.Play();
                            found = true;
                        }
                    }
                }
            }
            catch
            { }
            if (!found)
                SystemSounds.Beep.Play(); // consolation prize
        }
    }
}
