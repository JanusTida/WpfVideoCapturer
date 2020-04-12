using CDFCCultures.Managers;
using CDFCEntities.Enums;
using CDFCMessageBoxes.MessageBoxes;
using CDFCPreviewer.Components;
using CDFCPreviewer.Contracts;
using CDFCPreviewer.Controls;
using EventLogger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static CDFCCultures.Managers.ManagerLocator;

namespace CDFCPreviewer.Controls {
    /// <summary>
    /// Interaction logic for PreviewerPlayer.xaml
    /// </summary>
    public partial class PreviewPlayer : UserControl {
        public PreviewPlayer() {
            InitializeComponent();
            this.Resources.MergedDictionaries.LoadLanguage("CDFCPreviewer");
        }
        private IBufferPlayer player;

        public static readonly DependencyProperty PlayerBufferProperty = DependencyProperty.Register("PlayerBuffer", typeof(IntPtr), typeof(PreviewPlayer),
            new PropertyMetadata(IntPtr.Zero, PlayerBuffer_PropertyChanged));
        private static void PlayerBuffer_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            PreviewPlayer previewer = d as PreviewPlayer;
            if (previewer != null) {
                previewer.player?.Stop();
                if ((IntPtr)e.NewValue != IntPtr.Zero) {
                    IBufferPlayer player = null;
                    switch (previewer.DeviceType) {
                        case DeviceTypeEnum.DaHua:
                        case DeviceTypeEnum.HaiKang:
                        case DeviceTypeEnum.WFS:
                            player = new DHBufferPlayer((IntPtr)e.NewValue, previewer.pbPlayer.Handle,previewer.BufferSize);
                            break;
                        default:
                            break;
                    }

                    if(player != null) {
                        try {
                            if (player.Play()) {
                                previewer.Playing = true;
                                previewer.player = player;
                            }
                            else {
                                CDFCMessageBox.Show($"{FindResourceString("FailedToInitializePreviewer")}");
                            }
                        }
                        catch (Exception ex) {
                            Logger.WriteLine($"{nameof(PreviewPlayer)}:{nameof(PlayerBuffer_PropertyChanged)}:{ex.Message}");
                            CDFCMessageBox.Show($"{FindResourceString("ErrorWhenInitializingPreviewer")}:{ex.Message}");
                        }
                    }
                }
                else {
                    previewer.player?.Stop();
                }
                previewer.pbPlayer.Refresh();
            }
            
        }
        
        public DeviceTypeEnum DeviceType {
            get { return (DeviceTypeEnum)this.GetValue(DeviceTypeProperty); }
            set { this.SetValue(DeviceTypeProperty, value); }
        }

        public static readonly DependencyProperty DeviceTypeProperty = DependencyProperty.Register("DeviceType", typeof(DeviceTypeEnum), typeof(PreviewPlayer),
            new PropertyMetadata(DeviceTypeEnum.Unknown, new PropertyChangedCallback(DeviceType_PropertyChanged)));

        public IntPtr PlayerBuffer {
            get { return (IntPtr)this.GetValue(PlayerBufferProperty); }
            set { this.SetValue(PlayerBufferProperty, value); }
        }

        private static void DeviceType_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            PreviewPlayer previewer = d as PreviewPlayer;
            if (previewer != null) {
                previewer.player?.Stop();
                if (previewer.PlayerBuffer != IntPtr.Zero) {
                    switch ((DeviceTypeEnum)e.NewValue) {
                        case DeviceTypeEnum.DaHua:
                        case DeviceTypeEnum.HaiKang:
                        case DeviceTypeEnum.WFS:
                            try {
                                var dhPlayer = new DHBufferPlayer(previewer.PlayerBuffer, previewer.pbPlayer.Handle, previewer.BufferSize);
                                dhPlayer.Play();
                                previewer.Playing = true;
                                previewer.player = dhPlayer;
                            }
                            catch(Exception ex) {
                                Logger.WriteLine($"{nameof(PreviewPlayer)}->{nameof(DeviceType_PropertyChanged)}:{ex.Message}");
                                CDFCMessageBox.Show(ex.Message);
                            }
                            
                            break;
                            
                    }
                    
                }
                
                previewer.pbPlayer.Refresh();
                
            }
        }

        public static readonly DependencyProperty BannerVisibleProperty = DependencyProperty.Register("BannerVisible", typeof(Visibility), typeof(PreviewPlayer),
            new PropertyMetadata(Visibility.Visible, new PropertyChangedCallback(BannerVisible_PropertyChanged)));

        public static readonly DependencyProperty BufferSizeProperty = DependencyProperty.Register("BufferSize", typeof(long), typeof(PreviewPlayer));

        public long BufferSize {
            get {   return (long)this.GetValue(BufferSizeProperty);  }
            set {   this.SetValue(BufferSizeProperty, value); }
        }
        private static void BannerVisible_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            PreviewPlayer previewer = d as PreviewPlayer;
            if (previewer != null) {
                previewer.playerBanner.Visibility = (Visibility)e.NewValue;
            }
        }
        
        public Visibility BannerVisible {
            get { return (Visibility)this.GetValue(BannerVisibleProperty); }
            set { this.SetValue(BannerVisibleProperty, value); }
        }

        public static readonly DependencyProperty PlayingProperty = DependencyProperty.Register("Playing", typeof(bool), typeof(PreviewPlayer),
            new PropertyMetadata(false));
        
        public bool Playing {
            get {
                return (bool)GetValue(PlayingProperty);
            }
            set {
                SetValue(PlayingProperty, value);
            }
        }

        private void Stop() {
            player?.Stop();
        }

        private void Play() {
            player?.Play();
        }


        [HandleProcessCorruptedStateExceptions]
        private void BtnPauseOrResume_Click(object sender, RoutedEventArgs e) {
            if (Playing == true) {
                if (player?.Pause() == true) {
                    Playing = false;
                }
            }
            else if (Playing == false) {
                if (player?.Resume() == true) {
                    Playing = true;
                }
            }
            else {
                try {
                    if (player?.Play() == true) {
                        Playing = true;
                    }
                }
                catch {

                }
                
            }
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e) {
            if (Playing == true || Playing == false) {
                player?.Stop();
                pbPlayer.Refresh();
            }
        }

        private void pbPlayer_Resize(object sender, EventArgs e) {
            pbPlayer.Refresh();
        }
    }
}
