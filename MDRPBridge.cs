using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace MusicBeePlugin
{
    public partial class Plugin
    {
        private MusicBeeApiInterface mbApiInterface;
        private PluginInfo about = new PluginInfo();

        public PluginInfo Initialise(IntPtr apiInterfacePtr)
        {
            mbApiInterface = new MusicBeeApiInterface();
            mbApiInterface.Initialise(apiInterfacePtr);
            about.PluginInfoVersion = PluginInfoVersion;
            about.Name = "MDRP Bridge";
            about.Description = "Bridges the audio information from MusicBee to Music Discord Rich Presence";
            about.Author = "Smaltin";
            about.TargetApplication = "";   //  the name of a Plugin Storage device or panel header for a dockable panel
            about.Type = PluginType.General;
            about.VersionMajor = 1;  // your plugin version
            about.VersionMinor = 0;
            about.Revision = 1;
            about.MinInterfaceVersion = MinInterfaceVersion;
            about.MinApiRevision = MinApiRevision;
            about.ReceiveNotifications = (ReceiveNotificationFlags.PlayerEvents | ReceiveNotificationFlags.TagEvents);
            about.ConfigurationPanelHeight = 0;
            return about;
        }

        public void ReceiveNotification(string sourceFileUrl, NotificationType type)
        {
            // perform some action depending on the notification type
            if (type == NotificationType.TrackChanged) {
                    string artist = mbApiInterface.NowPlaying_GetFileTag(MetaDataType.Artist);
                    string albumtitle = mbApiInterface.NowPlaying_GetFileTag(MetaDataType.Album);
                    string songtitle = mbApiInterface.NowPlaying_GetFileTag(MetaDataType.TrackTitle);
                    string action = (mbApiInterface.Player_GetPlayState.Invoke() == PlayState.Playing) ? "play" : "pause";
                    double timestamp = DateTime.Now.Ticks +
                                       mbApiInterface.NowPlaying_GetDuration.Invoke();
                    //string url = "localhost:2357?player=musicbee&timestamp=" + mbApiInterface.NowPlaying_GetDuration + "&action=" + action + "&title=" + songtitle + "&artist=" + artist + "&album=" + albumtitle;
                    string url = "localhost:2357";
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                    {
                        string json = string.Format("{{player:\"musicbee\",timestamp:\"{0}\",action:\"play\",title:\"{1}\",artist:\"{2}\",album:\"{3}\"}}", timestamp.ToString(), songtitle, artist, albumtitle);

                        streamWriter.Write(json);
                    }
                    request.Method = "POST";
                    request.ContentType = "text/json";
                    request.TransferEncoding = "UTF8";
                    request.ContentLength = url.Length;
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    response.Close();
            }
        }
    }
}