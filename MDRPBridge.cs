using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MusicBeePlugin
{
	public partial class Plugin
	{
		//there *should* only ever be one
		public static Plugin pluginstance;
		private MusicBeeApiInterface mbApiInterface;
		private PluginInfo about = new PluginInfo();
		private Control penel;
		private string persistentpath;
		private Settings _settings;
		private Menu menu;
		private Exception lastEx = null;
		private const string defaultAssetEndpoint = "https://jojo2357.github.io/MDRP-Bridge-Assets";
		private const int PackRescanFrequency = 1800000;
		private static string[] _listOfPacks;
		private static readonly Stopwatch _lastPackCheckTimer = new Stopwatch();
		private bool debugging = true;
		private static readonly char[] illegalChars = Path.GetInvalidPathChars();

		public void Refresh()
		{
			penel.Refresh();
		}

		private enum MDRPStatus
		{
			Keyed,
			Unkeyed,
			Paused,
			KeyedWrong,
			NotRunning
		}

		private MDRPStatus currentStatus = MDRPStatus.NotRunning;

		private Image[] images = new Image[5];
		private TextBox _mdrpLocationBox;
		private CheckBox AutoRunButton;
		private CheckBox AutoCloseButton;
		private ComboBox _skinSelector;
		private Label ValidMarker;

		public Settings GetCurrentSettings()
		{
			return _settings ?? SetSettings();
		}

		public Settings SetSettings()
		{
			return SetSettings(Settings.DEFAULT);
		}

		public Settings SetSettings(Settings settings)
		{
			//SendToDebugServer("Saving to " + Path.Combine(persistentpath, "MDRP-Bridge\\mdrpbridgesettings.dat"));
			File.WriteAllText(Path.Combine(persistentpath, "MDRP-Bridge\\mdrpbridgesettings.dat"), settings.ToJson());
			return _settings = settings;
		}

		private string GetVersionString()
		{
			return about.VersionMajor + "." + about.VersionMinor + "." + about.Revision;
		}

		public PluginInfo Initialise(IntPtr apiInterfacePtr)
		{
			try
			{
				pluginstance = this;
				mbApiInterface = new MusicBeeApiInterface();
				mbApiInterface.Initialise(apiInterfacePtr);
				about.PluginInfoVersion = PluginInfoVersion;
				about.Name = "MDRP Bridge";
				about.Description = "Bridges the audio information from MusicBee to Music Discord Rich Presence";
				about.Author = "Smaltin & jojo2357";
				about.TargetApplication = "MDRP Status"; //  the name of a Plugin Storage device or panel header for a dockable panel
				about.Type = PluginType.General;
				about.VersionMajor = 1; // your plugin version
				about.VersionMinor = 0;
				about.Revision = 0;
				about.MinInterfaceVersion = MinInterfaceVersion;
				about.MinApiRevision = MinApiRevision;
				about.ReceiveNotifications = ReceiveNotificationFlags.PlayerEvents;
				about.ConfigurationPanelHeight = 150;
				persistentpath = mbApiInterface.Setting_GetPersistentStoragePath();
				_settings = File.Exists(Path.Combine(persistentpath, "MDRP-Bridge\\mdrpbridgesettings.dat")) ? Settings.FromJson(File.ReadAllText(Path.Combine(mbApiInterface.Setting_GetPersistentStoragePath(), "MDRP-Bridge\\mdrpbridgesettings.dat"))) : Settings.DEFAULT;
				CheckAndUpdateStatus();
				File.WriteAllText(Path.Combine(persistentpath, "MDRP-Bridge\\latest.log"), "");
				menu = new Menu();
			}
			catch (Exception e)
			{
				lastEx = e;
			}

			return about;
		}

		private void CheckAndUpdateStatus()
		{
			try
			{
				HttpWebResponse response = (HttpWebResponse)doRequest("{player:\"MusicBee\"}", 2357).GetResponse();
				string resptext;
				using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
				{
					resptext = reader.ReadToEnd();
				}

				response.Close();
				if (resptext.Contains("MDRP"))
				{
					currentStatus = MDRPStatus.Paused;
				}
			}
			catch (Exception e)
			{
				SendToDebugServer("MDRP not open");
				if (_settings.AutoRun && File.Exists(_settings.MDRPLocation) && _settings.MDRPLocation.EndsWith("MDRP.exe"))
				{
					ProcessStartInfo startInfo = new ProcessStartInfo();
					startInfo.CreateNoWindow = true;
					startInfo.UseShellExecute = true;
					startInfo.FileName = _settings.MDRPLocation;
					startInfo.WindowStyle = ProcessWindowStyle.Minimized;
					startInfo.Arguments = "MusicBee";
					Process.Start(startInfo);
					try
					{
						HttpWebResponse response = (HttpWebResponse)doRequest("{player:\"MusicBee\"}", 2357, 1500).GetResponse();
						string resptext;
						using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
						{
							resptext = reader.ReadToEnd();
						}

						response.Close();
						if (resptext.Contains("MDRP"))
						{
							currentStatus = MDRPStatus.Paused;
						}
						else
						{
							SendToDebugServer("This isnt mdrp");
						}
					}
					catch (Exception ex)
					{
						SendToDebugServer("Did not start in time");
					}
				}
			}
		}

		public void ReceiveNotification(string sourceFileUrl, NotificationType type)
		{
			if (lastEx != null)
			{
				SendToDebugServer(lastEx.ToString());
			}

			// perform some action depending on the notification type
			if (type == NotificationType.TrackChanged || type == NotificationType.PlayStateChanged)
			{
				//Console.WriteLine("Ping pong");
				string artist = mbApiInterface.NowPlaying_GetFileTag(MetaDataType.Artist);
				string albumtitle = mbApiInterface.NowPlaying_GetFileTag(MetaDataType.Album);
				string songtitle = mbApiInterface.NowPlaying_GetFileTag(MetaDataType.TrackTitle);
				string action = (mbApiInterface.Player_GetPlayState.Invoke() == PlayState.Playing) ? "play" : "pause";
				long timestamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds +
				                 (mbApiInterface.Player_GetPlayState.Invoke() == PlayState.Playing
					                 ? mbApiInterface.NowPlaying_GetDuration.Invoke() -
					                   mbApiInterface.Player_GetPosition.Invoke()
					                 : 10000);
				Console.WriteLine(mbApiInterface.Player_GetPosition.Invoke());
				string json = string.Format(
					"{{debugaction:\"{5}\",player:\"musicbee\",timestamp:\"{0}\",action:\"{1}\",title:\"{2}\",artist:\"{3}\",album:\"{4}\"}}",
					timestamp.ToString(), action, songtitle.Replace("\"", "\\\""), artist.Replace("\"", "\\\""),
					albumtitle.Replace("\"", "\\\""), type);
				Console.WriteLine(json);
				try
				{
					HttpWebRequest request = doRequest(json, 2357);
					HttpWebResponse response = (HttpWebResponse)request.GetResponse();
					string text = "";
					using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
					{
						text = reader.ReadToEnd();
					}

					text = text.ToLower();
					MDRPStatus lastStatus = currentStatus;
					if (action == "pause")
					{
						currentStatus = MDRPStatus.Paused;
					}
					else if (text.Contains("response:\"keyed successfully\""))
					{
						currentStatus = MDRPStatus.Keyed;
					}
					else if (text.Contains("response:\"keyed incorrectly\""))
					{
						currentStatus = MDRPStatus.KeyedWrong;
					}
					else if (text.Contains("response:\"no key\""))
					{
						currentStatus = MDRPStatus.Unkeyed;
					}

					if (currentStatus != lastStatus)
						penel.Refresh();
					Console.WriteLine(text);
					response.Close();
				}
				catch (Exception e)
				{
					SendToDebugServer(e.ToString());
					currentStatus = MDRPStatus.NotRunning;
					penel.Refresh();
					//Console.WriteLine("MDRP not open");
				}
			}
		}

		//In the plugins menu, this is the fields that appear
		public bool Configure(IntPtr panelHandle)
		{
			if (panelHandle != IntPtr.Zero)
			{
				_settings = GetCurrentSettings();
				Panel configPanel = (Panel)Control.FromHandle(panelHandle);

				Label infoLabel = new Label();
				infoLabel.Location = new Point(0, 4);
				infoLabel.Text = "Version: " + GetVersionString();
				
				Label closeMDRPLabel = new Label();
				closeMDRPLabel.Text = "Close MDRP on MB close";
				closeMDRPLabel.Location = new Point(17, 18);
				closeMDRPLabel.TextAlign = ContentAlignment.MiddleRight;
				closeMDRPLabel.Size = new System.Drawing.Size(150, 30);

				AutoCloseButton = new CheckBox();
				AutoCloseButton.Location = new Point(174, 22);
				AutoCloseButton.Size = new System.Drawing.Size(21, 24);

				Label AutoRunLabel = new Label();
				AutoRunLabel.Text = "Automatically Run MDRP";
				AutoRunLabel.Location = new Point(16, 58);
				AutoRunLabel.Size = new System.Drawing.Size(152, 21);
				AutoRunLabel.TextAlign = ContentAlignment.MiddleRight;

				AutoRunButton = new CheckBox();
				AutoRunButton.Location = new Point(174, 58);
				AutoRunButton.Size = new Size(21, 21);

				Label MDRPLocationLabel = new Label();
				MDRPLocationLabel.Location = new Point(68, 94);
				MDRPLocationLabel.Text = "MDRP Location";
				MDRPLocationLabel.TextAlign = ContentAlignment.MiddleRight;
				MDRPLocationLabel.Size = new Size(100, 20);

				_mdrpLocationBox = new TextBox();
				_mdrpLocationBox.TextChanged += handleChange;
				_mdrpLocationBox.Location = new Point(174, 94);
				_mdrpLocationBox.Size = new Size(261, 20);

				_mdrpLocationBox.BackColor = Color.FromArgb(mbApiInterface.Setting_GetSkinElementColour(SkinElement.SkinInputControl,
					ElementState.ElementStateDefault,
					ElementComponent.ComponentBackground));
				_mdrpLocationBox.ForeColor = Color.FromArgb(mbApiInterface.Setting_GetSkinElementColour(SkinElement.SkinInputControl,
					ElementState.ElementStateDefault,
					ElementComponent.ComponentForeground));
				_mdrpLocationBox.BorderStyle = BorderStyle.FixedSingle;

				Label skinSelectorLabel = new Label();
				skinSelectorLabel.Location = new Point(27, 129);
				skinSelectorLabel.Text = "Image pack";
				skinSelectorLabel.TextAlign = ContentAlignment.MiddleRight;
				skinSelectorLabel.Size = new Size(140, 21);
				//skinSelectorLabel.Width = runMDRPLabel.Width;

				_skinSelector = new ComboBox();
				_skinSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
				_skinSelector.Location = new Point(174, 129);
				_skinSelector.Size = new Size(261, 21);
				_skinSelector.DataSource = getSkins();

				System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Menu));
				ValidMarker = new Label();
				ValidMarker.Image = ((System.Drawing.Image)(resources.GetObject("ValidMarker.Image")));
				ValidMarker.Location = new System.Drawing.Point(441, 94);
				ValidMarker.Size = new System.Drawing.Size(19, 20);
				ValidMarker.Visible = false;

				_mdrpLocationBox.Text = _settings.MDRPLocation;
				AutoCloseButton.Checked = _settings.KillOnClose;
				AutoRunButton.Checked = _settings.AutoRun;
				for (int i = 0; i < _skinSelector.Items.Count; i++)
				{
					if (((SkinSelectorItem)_skinSelector.Items[i]).Text == _settings.AssetPackName)
					{
						_skinSelector.SelectedItem = _skinSelector.Items[i];
						break;
					}
				}

				configPanel.Controls.AddRange(new Control[] { infoLabel, MDRPLocationLabel, _mdrpLocationBox, AutoRunLabel, AutoRunButton, closeMDRPLabel, AutoCloseButton, skinSelectorLabel, _skinSelector, ValidMarker });
			}

			return false;
		}

		public void SaveSettings()
		{
			if (_settings.AssetPackName != ((SkinSelectorItem)_skinSelector.SelectedItem).Text)
				LoadAllImages(((SkinSelectorItem)_skinSelector.SelectedItem).Text);
			SetSettings(new Settings(_mdrpLocationBox.Text, AutoRunButton.Checked, AutoCloseButton.Checked, ((SkinSelectorItem)_skinSelector.SelectedItem).Text));
		}

		private void handleChange(object obj, EventArgs args)
		{
			ValidMarker.Visible = InputTextHasMDRP(_mdrpLocationBox);
		}

		public static bool InputTextHasMDRP(TextBox box)
		{
			if (box.Text.Where(letter => illegalChars.Contains(letter)).Count() > 0)
				return false;
			box.SelectionLength = 0;
			if (File.Exists(Path.Combine(box.Text, "MDRP.exe")))
			{
				box.Text = Path.Combine(box.Text, "MDRP.exe");
				box.SelectionStart = box.Text.Length;
				return true;
			}
			else if (File.Exists(Path.Combine(box.Text, "MDRP\\bin\\release\\MDRP.exe")))
			{
				box.Text = Path.Combine(box.Text, "MDRP\\bin\\release\\MDRP.exe");
				box.SelectionStart = box.Text.Length;
				return true;
			}
			else if (File.Exists(box.Text) && box.Text.EndsWith("MDRP.exe"))
			{
				box.SelectionStart = box.Text.Length;
				return true;
			}
			else
			{
				return false;
			}
		}

		public void Close(PluginCloseReason reason)
		{
			try
			{
				string json = "{player:\"musicbee\",action:\"shutdown\"}";
				HttpWebRequest request = doRequest(json, 2357, 100);
				HttpWebResponse response = (HttpWebResponse)request.GetResponse();
				string text = "";
				using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
				{
					text = reader.ReadToEnd();
				}

				Console.WriteLine(text);
				response.Close();
			}
			catch (Exception e)
			{
				//SendToDebugServer(e.ToString());
			}

			if (_settings.KillOnClose)
				foreach (Process process in Process.GetProcessesByName("MDRP"))
				{
					process.Kill();
				}
		}

		public HttpWebRequest doRequest(string json, int channel)
		{
			return doRequest(json, channel, 500);
		}

		private HttpWebRequest doRequest(string json, int channel, int timeout)
		{
			Uri url = new Uri("http://localhost:" + channel + "/");
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			request.Method = "POST";
			request.ContentType = "text/json";
			request.Timeout = timeout;
			string urlEncoded = Uri.EscapeUriString(json);
			byte[] arr = Encoding.UTF8.GetBytes(urlEncoded);
			try
			{
				Stream rs = request.GetRequestStream();
				rs.Write(arr, 0, arr.Length);
			}
			catch (Exception e)
			{
				SendToDebugServer(e.ToString());

				return null;
			}

			return request;
		}

		public static SkinSelectorItem[] getSkins()
		{
			if (_listOfPacks == null || _lastPackCheckTimer.ElapsedMilliseconds > PackRescanFrequency)
			{
				try
				{
					WebClient wc = new WebClient();
					//SendToDebugServer(defaultAssetEndpoint + "/hostedpacks.dat");
					string allpacks = wc.DownloadString(defaultAssetEndpoint + "/hostedpacks.dat");
					_listOfPacks = allpacks.Split('\n').Where((str) => str.Length > 0).ToArray();
					_lastPackCheckTimer.Restart();
				}
				catch (Exception)
				{
					return new[] { new SkinSelectorItem() { ID = 1, Text = "standard" } };
				}
			}

			SkinSelectorItem[] itemsOut = new SkinSelectorItem[_listOfPacks.Length];
			for (int i = 0; i < itemsOut.Length; i++)
			{
				itemsOut[i] = new SkinSelectorItem { ID = i + 1, Text = _listOfPacks[i] };
			}

			return itemsOut;
		}

		//  presence of this function indicates to MusicBee that this plugin has a dockable panel. MusicBee will create the control and pass it as the panel parameter
		//  you can add your own controls to the panel if needed
		//  you can control the scrollable area of the panel using the mbApiInterface.MB_SetPanelScrollableArea function
		//  to set a MusicBee header for the panel, set about.TargetApplication in the Initialise function above to the panel header text
		public int OnDockablePanelCreated(Control panel)
		{
			//    return the height of the panel and perform any initialisation here
			//    MusicBee will call panel.Dispose() when the user removes this panel from the layout configuration
			//    < 0 indicates to MusicBee this control is resizable and should be sized to fill the panel it is docked to in MusicBee
			//    = 0 indicates to MusicBee this control resizeable
			//    > 0 indicates to MusicBee the fixed height for the control.Note it is recommended you scale the height for high DPI screens(create a graphics object and get the DpiY value)
			float dpiScaling = 0;
			using (Graphics g = panel.CreateGraphics())
			{
				dpiScaling = g.DpiY / 96f;
			}

			LoadAllImages(_settings.AssetPackName);
			panel.Name = "MDRP status";
			panel.Paint += panel_Paint;
			panel.Click += panel_Click;
			penel = panel;
			return Convert.ToInt32(64 * dpiScaling);
		}

		private void panel_Click(object sender, EventArgs e)
		{
			try
			{
				if (menu == null || menu.IsDisposed) menu = new Menu();
				//todo add menu coloring to match theme
				/*menu.SetColors(Color.FromArgb(mbApiInterface.Setting_GetSkinElementColour(SkinElement.SkinInputControl,
					ElementState.ElementStateDefault,
					ElementComponent.ComponentBackground)), Color.FromArgb(mbApiInterface.Setting_GetSkinElementColour(SkinElement.SkinInputControl,
					ElementState.ElementStateDefault,
					ElementComponent.ComponentForeground)));*/
				menu.SelectSkins();
				menu.Show();
			}
			catch (Exception ex)
			{
				SendToDebugServer(ex.ToString());
			}
		}

		public void LoadAllImages(string packName)
		{
			//SendToDebugServer("Loading " + packName + " into " + persistentpath + "\\" + packName + "\\MDRP-Bridge from " + defaultAssetEndpoint + "/" + packName + "/");
			string fileDir = persistentpath + "\\MDRP-Bridge\\" + packName; //"C:\\Users\\Joey\\Documents\\GitHub\\MDRP-MusicBee-Bridge\\assets\\";
			string assetDir = defaultAssetEndpoint + "//" + packName + "/";

			WebClient wc = new WebClient();
			if (!Directory.Exists(fileDir))
				Directory.CreateDirectory(fileDir);
			if (!File.Exists(fileDir + "\\keyed.png"))
				wc.DownloadFile(assetDir + "keyed.png", fileDir + "\\keyed.png");
			if (!File.Exists(fileDir + "\\paused.png"))
				wc.DownloadFile(assetDir + "paused.png", fileDir + "\\paused.png");
			if (!File.Exists(fileDir + "\\unkeyed.png"))
				wc.DownloadFile(assetDir + "unkeyed.png", fileDir + "\\unkeyed.png");
			if (!File.Exists(fileDir + "\\invalid.png"))
				wc.DownloadFile(assetDir + "invalid.png", fileDir + "\\invalid.png");
			if (!File.Exists(fileDir + "\\offline.png"))
				wc.DownloadFile(assetDir + "offline.png", fileDir + "\\offline.png");

			images[(int)MDRPStatus.Keyed] = Image.FromFile(fileDir + "\\keyed.png"); //Image.FromFile(assetDir + "keyed.png");
			images[(int)MDRPStatus.Paused] = Image.FromFile(fileDir + "\\paused.png");
			images[(int)MDRPStatus.Unkeyed] = Image.FromFile(fileDir + "\\unkeyed.png");
			images[(int)MDRPStatus.KeyedWrong] = Image.FromFile(fileDir + "\\invalid.png");
			images[(int)MDRPStatus.NotRunning] = Image.FromFile(fileDir + "\\offline.png");
		}

		// presence of this function indicates to MusicBee that the dockable panel created above will show menu items when the panel header is clicked
		// return the list of ToolStripMenuItems that will be displayed
		public List<ToolStripItem> GetHeaderMenuItems()
		{
			List<ToolStripItem> list = new List<ToolStripItem>();
			list.Add(new ToolStripMenuItem("A menu item"));
			return list;
		}

		private void panel_Paint(object sender, PaintEventArgs e)
		{
			//e.Graphics.Clear(Color.Black);
			e.Graphics.DrawImage(images[(int)currentStatus], 0, 0, 64, 64);
			//TextRenderer.DrawText(e.Graphics, "hello " + currentStatus, SystemFonts.CaptionFont, new Point(10, 10), Color.White);
		}

		public void Uninstall()
		{
			foreach (Image image in images)
			{
				image.Dispose();
			}
			Directory.Delete(Path.Combine(persistentpath, "MDRP-bridge"), true);
		}

		private void SendToDebugServer(string message)
		{
			try
			{
#if DEBUG
		        if (debugging)
					doRequest(message, 7532, 500).GetResponse().Close();
#else
				File.AppendAllText(Path.Combine(persistentpath, "latest.log"), message + "\n");
#endif
			}
			catch (Exception e)
			{
				debugging = false;
				//suppress this as the server is likely closed
			}
		}
	}
}