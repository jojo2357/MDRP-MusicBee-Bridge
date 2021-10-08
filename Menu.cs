using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MusicBeePlugin
{
    public partial class Menu : Form
    {
	    private readonly Plugin.MusicBeeApiInterface _mbApiInterface;
	    private readonly Plugin.PluginInfo _about;

	    public Menu()
        {
            InitializeComponent();
        }

        public Menu(Plugin.MusicBeeApiInterface mbApiInterface, Plugin.PluginInfo about)
        {
            _mbApiInterface = mbApiInterface;
            _about = about ?? throw new ArgumentNullException(nameof(about));
            InitializeComponent();

            FormClosing += Settings_FormClosing;
            Shown += Settings_OnShown;
            VisibleChanged += OnVisibleChanged;
        }

        private void UpdateAll()
        {
            Settings currentSettings = Plugin.GetCurrentSettings();
            MDRPLocationInput.Text = currentSettings.MDRPLocation;
            AutoRun.Checked = currentSettings.AutoRun;
            AutoCloseButton.Checked = currentSettings.KillOnClose;
            for (int i = 0; i < SkinSelector.Items.Count; i++)
            {
	            if (((SkinSelectorItem)SkinSelector.Items[i]).Text == currentSettings.AssetPackName)
	            {
		            SkinSelector.SelectedItem = SkinSelector.Items[i];
		            break;
	            }
            }
        }

        private void OnVisibleChanged(object sender, EventArgs eventArgs)
        {
            if (Visible) UpdateAll();
        }

        private void Settings_OnShown(object sender, EventArgs eventArgs)
        {
            UpdateAll();
        }

        private void Settings_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.UserClosing) return;
            Hide();
            e.Cancel = true;
        }

        private void ButtonClose_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private void Menu_Load(object sender, EventArgs e)
        {
	        UpdateAll();
        }

        private void OnSaveLocation(object sender, EventArgs e)
        {
	        Plugin.SetSettings(new Settings(MDRPLocationInput.Text, AutoRun.Checked, AutoCloseButton.Checked, ((SkinSelectorItem)SkinSelector.SelectedItem).Text));
	        UpdateAll();
        }

        private void VerifyLocation(object sender, EventArgs e)
        {
	        MDRPLocationInput.SelectionLength = 0;
	        if (File.Exists(Path.Combine(MDRPLocationInput.Text, "MDRP.exe")))
	        {
		        MDRPLocationInput.Text = Path.Combine(MDRPLocationInput.Text, "MDRP.exe");
		        MDRPLocationInput.SelectionStart = MDRPLocationInput.Text.Length;
		        ValidMarker.Visible = true;
	        } else if (File.Exists(Path.Combine(MDRPLocationInput.Text, "MDRP\\bin\\release\\MDRP.exe")))
	        {
		        MDRPLocationInput.Text = Path.Combine(MDRPLocationInput.Text, "MDRP\\bin\\release\\MDRP.exe");
		        MDRPLocationInput.SelectionStart = MDRPLocationInput.Text.Length;
		        ValidMarker.Visible = true;
	        } else if (File.Exists(MDRPLocationInput.Text) && MDRPLocationInput.Text.EndsWith("MDRP.exe"))
	        {
		        MDRPLocationInput.SelectionStart = MDRPLocationInput.Text.Length - 1;
		        ValidMarker.Visible = true;
	        } else
	        {
		        ValidMarker.Visible = false;
	        }
        }

        private void SettingChanged(object sender, EventArgs e)
        {
	        OnSaveLocation(sender, e);
        }

        public void SelectSkins()
        {
	        this.SkinSelector.DataSource = Plugin.getSkins();
        }
    }
}