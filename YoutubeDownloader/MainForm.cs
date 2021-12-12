using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader
{
    public partial class MainForm : Form
    {
        CancellationTokenSource _cancelTokenSource;
        public MainForm()
        {
            InitializeComponent();
        }

        private async void btn_start_Click(object sender, EventArgs e)
        {
            rtb_output.Clear();
            progress.Value = 0;
            try
            {
                btn_cancel.Enabled = true;
                btn_start.Enabled = false;
                rtb_output.AppendText("Downloading Video Data." + Environment.NewLine);
                YoutubeClient client = new YoutubeClient();
                var video = await client.Videos.GetAsync(VideoId.Parse(tb_url.Text));
                var videoStreamInfo = await client.Videos.Streams.GetManifestAsync(video.Id);
                var audioStream = videoStreamInfo.GetAudioOnlyStreams().GetWithHighestBitrate();
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Title = "Save Audio File";
                sfd.Filter = $"{audioStream.Container.Name}|*.{audioStream.Container.Name}|MP3 File (*.mp3)|*.mp3";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    progress.Maximum = 100;
                    _cancelTokenSource = new CancellationTokenSource();
                    await client.Videos.Streams.DownloadAsync(
                        audioStream,
                        sfd.FileName,
                        new Progress<double>(download_progress),
                        _cancelTokenSource.Token
                    );
                    rtb_output.AppendText("Finished!" + Environment.NewLine);
                    progress.Value = 0;
                    lb_progress.Text = "Ready";
                }
                else
                {
                    rtb_output.AppendText("Save file dialog canceled operation." + Environment.NewLine);
                    throw new OperationCanceledException();
                }
            }
            catch (Exception exception)
            {
                rtb_output.AppendText(exception.ToString());
                progress.ForeColor = Color.Red;
                lb_progress.Text = "Failed";
            }
            finally
            {
                btn_cancel.Enabled = false;
                btn_start.Enabled = true;
            }
        }

        private void download_progress(double progress)
        {
            int downloadProgress = (int)(progress * 100);
            this.progress.Value = downloadProgress;
            lb_progress.Text = $"{downloadProgress}%";
        }

        private void btn_cancel_Click(object sender, EventArgs e)
        {
            _cancelTokenSource.Cancel();
        }
    }
}
