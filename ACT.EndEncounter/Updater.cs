using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ACT.EndEncounter
{
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

        public static async Task DownloadPluginAsync(string version)
        {
            var url = $"https://github.com/croagunk/ACT.EndEncounter/releases/download/v{version}/ACT.EndEncounter.dll";
            var path = System.Reflection.Assembly.GetExecutingAssembly().Location;

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
