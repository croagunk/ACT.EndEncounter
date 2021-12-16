using Advanced_Combat_Tracker;
using FFXIV_ACT_Plugin.Logfile;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ACT.EndEncounter
{
    public class EndEncounter : IActPluginV1
    {
        private Label statusLabel;
        private SettingsScreen settingsScreen;

        public string PluginLocation { get; private set; } = null;

        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
        {
            statusLabel = pluginStatusText;
            statusLabel.Text = "Plugin started";

            PluginLocation = ActGlobals.oFormActMain.PluginGetSelfData(this).pluginFile.FullName;

            settingsScreen = new SettingsScreen()
            {
                Dock = DockStyle.Fill,
            };
            pluginScreenSpace.Controls.Add(settingsScreen);
            pluginScreenSpace.Text = "EndEncounter Plugin";
            ActGlobals.oFormActMain.ValidateLists();

            ActGlobals.oFormActMain.OnLogLineRead += OnLogLineRead;

            statusLabel.Text = "Plugin initialized";

            CheckUpdate();
        }

        public void DeInitPlugin()
        {
            ActGlobals.oFormActMain.OnLogLineRead -= OnLogLineRead;

            statusLabel.Text = "Plugin exited";
        }

        private void OnLogLineRead(bool isImport, LogLineEventArgs logInfo)
        {
            // インポートのログは無視
            if (isImport) return;

            // メッセージの種類が「ChatLog」以外のログは無視
            var messageType = (LogMessageType)Enum.ToObject(typeof(LogMessageType), logInfo.detectedType);
            if (messageType != LogMessageType.ChatLog) return;

            var logLine = logInfo.logLine;
            ParseLogLine(logLine);
        }

        private void ParseLogLine(string logLine)
        {
            // 規定文字数未満のログは無視する
            // 書式: [HH:MM:SS.sss] ChatLog XX:XXXX
            if (logLine.Length < 30) return;

            var type = logLine.Substring(23, 7).ToUpper();
            var message = logLine.Substring(30, logLine.Length - 30);

            var matchingTypes = new string[]
            {
                "00:0038",  // echo
                "00:00B9",  // countdown (me)
                "00:0139",  // countdown (party)
                "00:0239",  // countdown (alliance)
            };
            var endKeywords = new string[]
            {
                "/endencounter",
                "戦闘開始まで"
            };

            if (matchingTypes.Contains(type) && endKeywords.Count(x => message.Contains(x)) > 0)
            {
                ActGlobals.oFormActMain.ActCommands("end");
            }
        }

        private async void CheckUpdate()
        {
            var isUpdateAvailable = await Updater.CheckUpdateAsync();
            if (isUpdateAvailable)
            {
                await Updater.DownloadPluginAsync(Updater.RemoteVersion, PluginLocation);

                var message = $"EndEncounter Plugin の新しいバージョン ({Updater.RemoteVersion}) をダウンロードしました。適用するには ACT を再起動してください。";
                //ActGlobals.oFormActMain.NotificationAdd("ACT.EndEncounter", message);
                TryRestartACT(true, message);
            }
        }

        private static bool TryRestartACT(bool showIgnoreButton, string additionalInfo)
        {
            var method = ActGlobals.oFormActMain.GetType().GetMethod("RestartACT");
            if (method == null)
            {
                return false;
            }
            method.Invoke(ActGlobals.oFormActMain, new object[] { showIgnoreButton, additionalInfo });
            return true;
        }
    }

    public static class Updater
    {
        private static readonly string REPO_VER_URL = @"https://raw.githubusercontent.com/croagunk/ACT.EndEncounter/master/@VERSION";

        public static string RemoteVersion { get; private set; } = null;

        public static async Task<bool> CheckUpdateAsync()
        {
            try
            {
                using (var wc = new WebClient())
                {
                    RemoteVersion = await wc.DownloadStringTaskAsync(REPO_VER_URL);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                return false;
            }

            Version localVer, remoteVer;
            localVer = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            if (!Version.TryParse(RemoteVersion, out remoteVer))
            {
                return false;
            }

            return localVer.CompareTo(remoteVer) < 0;
        }

        public static async Task DownloadPluginAsync(string version, string path)
        {
            var url = $"https://github.com/croagunk/ACT.EndEncounter/releases/download/v{version}/ACT.EndEncounter.dll";

            try
            {
                using (var wc = new WebClient())
                {
                    await wc.DownloadFileTaskAsync(url, path);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            }
        }
    }
}
