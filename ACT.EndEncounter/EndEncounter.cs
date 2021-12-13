using System;
using System.Linq;
using System.Windows.Forms;

using Advanced_Combat_Tracker;
using FFXIV_ACT_Plugin.Logfile;

namespace ACT.EndEncounter
{
    public class EndEncounter : IActPluginV1
    {
        private Label statusLabel;
        private SettingsScreen settingsScreen;

        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
        {
            statusLabel = pluginStatusText;
            statusLabel.Text = "Plugin started";

            settingsScreen = new SettingsScreen()
            {
                Dock = DockStyle.Fill,
            };
            pluginScreenSpace.Controls.Add(settingsScreen);
            pluginScreenSpace.Text = "EndEncounter Plugin";
            ActGlobals.oFormActMain.ValidateLists();

            ActGlobals.oFormActMain.OnLogLineRead += OnLogLineRead;

            statusLabel.Text = "Plugin initialized";
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
                "00:0139",  // countdown (other)
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
    }
}
