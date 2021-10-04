using System.ComponentModel;
using System.Windows.Forms;

namespace MusicBeePlugin
{
	partial class Menu
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.MDRPLocationInput = new System.Windows.Forms.TextBox();
			this.MDRPLocationLabel = new System.Windows.Forms.Label();
			this.SaveButton = new System.Windows.Forms.Button();
			this.CloseButton = new System.Windows.Forms.Button();
			this.AutoRun = new System.Windows.Forms.CheckBox();
			this.AutoRunLabel = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.AutoCloseButton = new System.Windows.Forms.CheckBox();
			this.MDRPLocationToolTip = new System.Windows.Forms.ToolTip(this.components);
			this.SuspendLayout();
			// 
			// MDRPLocationInput
			// 
			this.MDRPLocationInput.BackColor = System.Drawing.SystemColors.WindowFrame;
			this.MDRPLocationInput.Location = new System.Drawing.Point(174, 94);
			this.MDRPLocationInput.Name = "MDRPLocationInput";
			this.MDRPLocationInput.Size = new System.Drawing.Size(261, 20);
			this.MDRPLocationInput.TabIndex = 0;
			this.MDRPLocationToolTip.SetToolTip(this.MDRPLocationInput, "Enter the location of MDRP exe. Once the box changes color to green text, the exe" + " is found. This is for automatically starting MDRP.");
			this.MDRPLocationInput.TextChanged += new System.EventHandler(this.VerifyLocation);
			// 
			// MDRPLocationLabel
			// 
			this.MDRPLocationLabel.Location = new System.Drawing.Point(68, 94);
			this.MDRPLocationLabel.Name = "MDRPLocationLabel";
			this.MDRPLocationLabel.Size = new System.Drawing.Size(100, 20);
			this.MDRPLocationLabel.TabIndex = 1;
			this.MDRPLocationLabel.Text = "MDRP Location";
			this.MDRPLocationLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// SaveButton
			// 
			this.SaveButton.Location = new System.Drawing.Point(87, 166);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.Size = new System.Drawing.Size(69, 22);
			this.SaveButton.TabIndex = 2;
			this.SaveButton.Text = "Save";
			this.SaveButton.UseVisualStyleBackColor = true;
			this.SaveButton.Click += new System.EventHandler(this.OnSaveLocation);
			// 
			// CloseButton
			// 
			this.CloseButton.Location = new System.Drawing.Point(16, 166);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = new System.Drawing.Size(65, 22);
			this.CloseButton.TabIndex = 3;
			this.CloseButton.Text = "Close";
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.ButtonClose_Click);
			// 
			// AutoRun
			// 
			this.AutoRun.Location = new System.Drawing.Point(174, 58);
			this.AutoRun.Name = "AutoRun";
			this.AutoRun.Size = new System.Drawing.Size(21, 21);
			this.AutoRun.TabIndex = 4;
			this.MDRPLocationToolTip.SetToolTip(this.AutoRun, "If MB detects that MDRP is not running and this setting is enabled, it will attem" + "pt to start MDRP");
			this.AutoRun.UseVisualStyleBackColor = true;
			// 
			// AutoRunLabel
			// 
			this.AutoRunLabel.Location = new System.Drawing.Point(16, 58);
			this.AutoRunLabel.Name = "AutoRunLabel";
			this.AutoRunLabel.Size = new System.Drawing.Size(152, 21);
			this.AutoRunLabel.TabIndex = 5;
			this.AutoRunLabel.Text = "Automatically Run MDRP";
			this.AutoRunLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(17, 18);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(150, 30);
			this.label1.TabIndex = 6;
			this.label1.Text = "Close MDRP on MB close";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// AutoCloseButton
			// 
			this.AutoCloseButton.Location = new System.Drawing.Point(174, 22);
			this.AutoCloseButton.Name = "AutoCloseButton";
			this.AutoCloseButton.Size = new System.Drawing.Size(21, 24);
			this.AutoCloseButton.TabIndex = 7;
			this.MDRPLocationToolTip.SetToolTip(this.AutoCloseButton, "If MB detects that MDRP is running when MB closes and this setting is enabled, MB" + " will attempt to close MDRP");
			this.AutoCloseButton.UseVisualStyleBackColor = true;
			// 
			// MDRPLocationToolTip
			// 
			this.MDRPLocationToolTip.AutoPopDelay = 5000;
			this.MDRPLocationToolTip.InitialDelay = 1000;
			this.MDRPLocationToolTip.ReshowDelay = 500;
			// 
			// Menu
			// 
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			this.ClientSize = new System.Drawing.Size(487, 221);
			this.ControlBox = false;
			this.Controls.Add(this.AutoCloseButton);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.AutoRunLabel);
			this.Controls.Add(this.AutoRun);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.SaveButton);
			this.Controls.Add(this.MDRPLocationLabel);
			this.Controls.Add(this.MDRPLocationInput);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Menu";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "Settings WIndow";
			this.Load += new System.EventHandler(this.Menu_Load);
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private System.Windows.Forms.CheckBox AutoCloseButton;

		private System.Windows.Forms.Label label1;

		private System.Windows.Forms.CheckBox AutoRun;
		private System.Windows.Forms.Label AutoRunLabel;

		private System.Windows.Forms.Button CloseButton;

		private System.Windows.Forms.Button SaveButton;

		#endregion

		private System.Windows.Forms.TextBox MDRPLocationInput;
		private System.Windows.Forms.Label MDRPLocationLabel;
		private System.Windows.Forms.ToolTip MDRPLocationToolTip;
	}
}