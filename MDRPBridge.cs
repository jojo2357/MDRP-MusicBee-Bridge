using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

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
			about.Author = "Smaltin & jojo2357";
			about.TargetApplication = ""; //  the name of a Plugin Storage device or panel header for a dockable panel
			about.Type = PluginType.General;
			about.VersionMajor = 1; // your plugin version
			about.VersionMinor = 0;
			about.Revision = 1;
			about.MinInterfaceVersion = MinInterfaceVersion;
			about.MinApiRevision = MinApiRevision;
			about.ReceiveNotifications = ReceiveNotificationFlags.PlayerEvents;
			about.ConfigurationPanelHeight = 0;
			return about;
		}

		public void ReceiveNotification(string sourceFileUrl, NotificationType type)
		{
			// perform some action depending on the notification type
			if (type == NotificationType.TrackChanged || type == NotificationType.PlayStateChanged)
			{
				Console.WriteLine("Ping pong");
				string artist = mbApiInterface.NowPlaying_GetFileTag(MetaDataType.Artist);
				string albumtitle = mbApiInterface.NowPlaying_GetFileTag(MetaDataType.Album);
				string songtitle = mbApiInterface.NowPlaying_GetFileTag(MetaDataType.TrackTitle);
				string action = (mbApiInterface.Player_GetPlayState.Invoke() == PlayState.Playing) ? "play" : "pause";
				long timestamp = (long) (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds +
				                 (mbApiInterface.Player_GetPlayState.Invoke() == PlayState.Playing
					                 ? mbApiInterface.NowPlaying_GetDuration.Invoke() -
					                   mbApiInterface.Player_GetPosition.Invoke()
					                 : 10000);
				Console.WriteLine(mbApiInterface.Player_GetPosition.Invoke());
				string json = string.Format(
					"{{action:\"{5}\",player:\"musicbee\",timestamp:\"{0}\",action:\"{1}\",title:\"{2}\",artist:\"{3}\",album:\"{4}\"}}",
					timestamp.ToString(), action, songtitle.Replace("\"", "\\\""), artist.Replace("\"", "\\\""),
					albumtitle.Replace("\"", "\\\""), type);
				Console.WriteLine(json);
				try
				{
					HttpWebRequest request = doRequest(json);
					HttpWebResponse response = (HttpWebResponse) request.GetResponse();
					string text = "";
					using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
					{
						text = reader.ReadToEnd();
					}

					Console.WriteLine(text);
					response.Close();
				}
				catch (Exception)
				{
					Console.WriteLine("MDRP not open");
				}
			}
			/*else if (type == NotificationType.ShutdownStarted)
			{
				try
				{
					string json = "{{player:\"musicbee\",action:\"shutdown\"}}";
					HttpWebRequest request = doRequest(json);
					HttpWebResponse response = (HttpWebResponse) request.GetResponse();
					string text = "";
					using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
					{
						text = reader.ReadToEnd();
					}

					Console.WriteLine(text);
					response.Close();
				}
				catch (Exception)
				{
					Console.WriteLine("MDRP not open");
				}
			}
			else
			{
				try
				{
					string json = "{" + type + "}";
					HttpWebRequest request = doRequest(json);
					HttpWebResponse response = (HttpWebResponse) request.GetResponse();
					string text = "";
					using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
					{
						text = reader.ReadToEnd();
					}

					Console.WriteLine(text);
					response.Close();
				}
				catch (Exception)
				{
					Console.WriteLine("MDRP not open");
				}
			}*/
		}
		
		public void Close(PluginCloseReason reason)
		{
			try
			{
				string json = "{player:\"musicbee\",action:\"shutdown\"}";
				HttpWebRequest request = doRequest(json);
				HttpWebResponse response = (HttpWebResponse) request.GetResponse();
				string text = "";
				using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
				{
					text = reader.ReadToEnd();
				}

				Console.WriteLine(text);
				response.Close();
			}
			catch (Exception)
			{
				Console.WriteLine("MDRP not open");
			}
		}

		public HttpWebRequest doRequest(string json)
		{
			Uri url = new Uri("http://localhost:2357/");
			HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
			request.Method = "POST";
			request.ContentType = "text/json";
			string urlEncoded = Uri.EscapeUriString(json);
			byte[] arr = Encoding.UTF8.GetBytes(urlEncoded);
			try
			{
				var rs = request.GetRequestStream();
				rs.Write(arr, 0, arr.Length);
			}
			catch (Exception)
			{
				return null;
			}

			return request;
		}
	}
}