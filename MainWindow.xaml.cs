using System;
using System.Windows;
using LibVLCSharp.Shared;

namespace DVDPlayerWithFilter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        LibVLC libVLC;
        MediaPlayer vlcPlayer;
        SkipMuteFilterFile filter;
        int curFilterIndex = 0;
        bool isFiltering = true;
        bool isMuting = false;
        string filterFileName = "";

        public MainWindow()
        {
            InitializeComponent();

            //get the filter file
            Microsoft.Win32.OpenFileDialog d = new Microsoft.Win32.OpenFileDialog();
            d.Title = "Choose a filter file before you begin";
            d.Filter = "JSON Filter file|*.filter";
            d.FileName = Properties.Settings.Default.LastFileName;
            var x = d.ShowDialog();
            if (x == false || !x.HasValue) return;
            Properties.Settings.Default.LastFileName = d.FileName;
            Properties.Settings.Default.Save();
            filterFileName = d.FileName;

            ReloadFilterFile();
            videoView.Loaded += VideoView_Loaded;
        }

        void VideoView_Loaded(object sender, RoutedEventArgs e)
        {
            Core.Initialize();

            libVLC = new LibVLC();
            vlcPlayer = new MediaPlayer(libVLC);
            videoView.MediaPlayer = vlcPlayer;
            //vlcPlayer.TimeChanged += MediaPlayer_TimeChanged;
            vlcPlayer.PositionChanged += VlcPlayer_PositionChanged;
            var test = vlcPlayer.Play(new Media(libVLC, new Uri("dvd:///e:")));
        }

        private void VlcPlayer_PositionChanged(object sender, MediaPlayerPositionChangedEventArgs e)
        {
            
            var milliseconds = e.Position * vlcPlayer.Length;
            var t = TimeSpan.FromMilliseconds(milliseconds);

            if (isFiltering && curFilterIndex < filter.Filters.Count)
            {
                var curFilter = filter.Filters[curFilterIndex];
                //see if we're done with this muting filter
                if (curFilter.EndTime.TotalMilliseconds < milliseconds)
                {
                    if (isMuting)
                    {
                        //turn off muting
                        filterLabel.Dispatcher.Invoke(() => filterLabel.Text = "");
                        vlcPlayer.Mute = false;
                        isMuting = false;
                    }

                    curFilterIndex++;
                    isFiltering = curFilterIndex < filter.Filters.Count;
                }

                //see if we need to execute this filter
                if (milliseconds >= curFilter.StartTime.TotalMilliseconds && milliseconds <= curFilter.EndTime.TotalMilliseconds)
                {
                    if (curFilter.Type == "Mute")
                    {
                        //turn on muting
                        isMuting = true;
                        filterLabel.Dispatcher.Invoke(() => filterLabel.Text = "Muting");
                        vlcPlayer.Mute = true;
                    }
                    else if (curFilter.Type == "Skip")
                    {
                        //skip
                        float desiredPosition = (float)curFilter.EndTime.TotalMilliseconds / (float)vlcPlayer.Length;
                        vlcPlayer.Pause();
                        vlcPlayer.Position = desiredPosition;
                        vlcPlayer.Play();
                        curFilterIndex++;
                    }
                }
            }

            videoTime.Dispatcher.Invoke(() =>
            {
                videoTime.Text = t.ToString();
                positionSlider.Value = vlcPlayer.Position * 1000;
            });
            
        }

        private void PositionSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {

            var send = ((FormattedSlider)sender);
            var isPlaying = vlcPlayer.IsPlaying;
            send.Tag = isFiltering;
            if (isPlaying) vlcPlayer.Pause();

            var milliseconds = send.Value / 1000 * vlcPlayer.Length;
            var t = TimeSpan.FromMilliseconds(milliseconds);
            send.AutoToolTipContent = t.ToString();
        }

        private void PositionSlider_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var send = ((FormattedSlider)sender);

            var milliseconds = send.Value / 1000 * vlcPlayer.Length;
            var t = TimeSpan.FromMilliseconds(milliseconds);
            send.AutoToolTipContent = t.ToString();
        }

        private void PositionSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            var send = ((FormattedSlider)sender);
            var isPlaying = (bool)send.Tag;
            float desiredPosition = (float)send.Value / 1000;            
            vlcPlayer.Position = desiredPosition;
            if (isPlaying) vlcPlayer.Play();

            ResetNextFilter();
        }

        private void PrevButton_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            vlcPlayer.PreviousChapter();

        }

        private void Back30Button_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            bool isPlaying = vlcPlayer.IsPlaying;
            if (isPlaying) vlcPlayer.Pause();
            var milliseconds = vlcPlayer.Position * vlcPlayer.Length;
            var desiredPosition = (float)(milliseconds - 30000) / (float)vlcPlayer.Length;
            vlcPlayer.Position = desiredPosition;
            if (isPlaying) vlcPlayer.Play();

            ResetNextFilter();
        }

        private void Back10Button_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            bool isPlaying = vlcPlayer.IsPlaying;
            if (isPlaying) vlcPlayer.Pause();
            var milliseconds = vlcPlayer.Position * vlcPlayer.Length;
            var desiredPosition = (float)(milliseconds - 10000) / (float)vlcPlayer.Length;
            vlcPlayer.Position = desiredPosition;
            if (isPlaying) vlcPlayer.Play();

            ResetNextFilter();
        }

        private void PlayButton_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            vlcPlayer.Play();
        }

        private void PauseButton_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            vlcPlayer.Pause();
        }

        private void Forward10Button_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            bool isPlaying = vlcPlayer.IsPlaying;
            if (isPlaying) vlcPlayer.Pause();
            var milliseconds = vlcPlayer.Position * vlcPlayer.Length;
            var desiredPosition = (float)(milliseconds + 10000) / (float)vlcPlayer.Length;
            vlcPlayer.Position = desiredPosition;
            if (isPlaying) vlcPlayer.Play();

            ResetNextFilter();
        }

        private void Forward30Button_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            bool isPlaying = vlcPlayer.IsPlaying;
            if (isPlaying) vlcPlayer.Pause();
            var milliseconds = vlcPlayer.Position * vlcPlayer.Length;
            var desiredPosition = (float)(milliseconds + 30000) / (float)vlcPlayer.Length;
            vlcPlayer.Position = desiredPosition;
            if (isPlaying) vlcPlayer.Play();

            ResetNextFilter();
        }

        private void NextButton_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            vlcPlayer.NextChapter();
        }

        private void Back1Button_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            bool isPlaying = vlcPlayer.IsPlaying;
            if (isPlaying) vlcPlayer.Pause();
            var milliseconds = vlcPlayer.Position * vlcPlayer.Length;
            var desiredPosition = (float)(milliseconds - 1000) / (float)vlcPlayer.Length;
            vlcPlayer.Position = desiredPosition;
            if (isPlaying) vlcPlayer.Play();

            ResetNextFilter();
        }

        private void Forward1Button_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            bool isPlaying = vlcPlayer.IsPlaying;
            if (isPlaying) vlcPlayer.Pause();
            var milliseconds = vlcPlayer.Position * vlcPlayer.Length;
            var desiredPosition = (float)(milliseconds + 1000) / (float)vlcPlayer.Length;
            vlcPlayer.Position = desiredPosition;
            if (isPlaying) vlcPlayer.Play();

            ResetNextFilter();
        }

        private void ResetNextFilter()
        {
            if (vlcPlayer == null) return;

            var milliseconds = vlcPlayer.Position * vlcPlayer.Length;
            for (var i = 0; i < filter.Filters.Count; i++)
            {
                if (filter.Filters[i].StartTime.TotalMilliseconds > milliseconds)
                {
                    curFilterIndex = i;
                    return;
                }
            }

            curFilterIndex = filter.Filters.Count;
        }

        private void ReloadFilterFile()
        {

            //load filter
            filter = Newtonsoft.Json.JsonConvert.DeserializeObject<SkipMuteFilterFile>(System.IO.File.ReadAllText(filterFileName));

            //sort filters
            filter.Filters.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));
            isFiltering = filter.Filters.Count > 0;

            //add offset
            filter.Filters.ForEach(one =>
            {
                one.StartTime = one.StartTime.Add(filter.FilterOffset);
                one.EndTime = one.EndTime.Add(filter.FilterOffset);
            });

            ResetNextFilter();
        }

        private void ReloadFilter_Click(object sender, RoutedEventArgs e)
        {
            ReloadFilterFile();
        }
    }
}
