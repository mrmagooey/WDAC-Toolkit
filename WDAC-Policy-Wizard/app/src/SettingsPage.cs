﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// jogeurte 11/19

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Xml; 


namespace WDAC_Wizard
{
    public partial class SettingsPage : UserControl
    {
        private Dictionary<string, bool> SettingsDict;
        private MainWindow _MainWindow;
        private Logger _Log; 

        public SettingsPage(MainWindow pMainWindow)
        {
            InitializeComponent();
            ShowVersionNumber();

            this._MainWindow = pMainWindow;
            this._Log = pMainWindow.Log; 
            this.SettingsDict = new Dictionary<string, bool>(); 
        }

        //
        // Summary:
        //     Method to set the current build version at the bottom of the About section of the settings page. 
        //      
        // Returns:
        //     None.
        private void ShowVersionNumber()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

            this.appVersion_Label.Text = String.Format("App Version {0}.22", versionInfo.FileVersion);
        }

        //
        // Summary:
        //     Method maps the checkbox that is toggled to the corresponding Preference.Setting 
        //     Sets the UI elements and modifies the Setting. Settings.Default are saved after each mod.
        //      
        // Returns:
        //     None.
        private void SettingCheckBox_Click(object sender, EventArgs e)
        {
            // Map the checkbox [really a picturebox] to the setting
            // Names of the checkbox are always <setting_name>_CheckBox
            int CHAR_OFFSET = 9;
            PictureBox checkBox = ((PictureBox)sender);
            string settingName = checkBox.Name.Substring(0, checkBox.Name.Length - CHAR_OFFSET);

            // Prompt user for additional confirmation before disabling telemetry
            if(settingName == "allowTelemetry" && checkBox.Tag.ToString() == "Checked")
            {
                DialogResult res = MessageBox.Show("Turning this off will not allow developers to improve this tool or help" +
                    " debug your case." + Environment.NewLine + "Are you sure you want to disable this?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (res == DialogResult.No)
                    return;
            }

            if (checkBox.Tag.ToString() == "Checked")
            {
                // Set setting --> false and checkbox --> unchecked
                checkBox.BackgroundImage = Properties.Resources.check_box_unchecked;
                checkBox.Tag = "Unchecked";
                Properties.Settings.Default[settingName] = false;
            }
            else
            {
                // Set setting --> true and checkbox --> checked
                checkBox.BackgroundImage = Properties.Resources.check_box_checked;
                checkBox.Tag = "Checked";
                Properties.Settings.Default[settingName] = true;
            }

            // Save settings and show settings update to user
            Properties.Settings.Default.Save();
            this.Update_Label.Visible = true; 

            Timer settingsUpdateNotificationTimer = new Timer();
            settingsUpdateNotificationTimer.Interval = (1500); // 1.5 secs
            settingsUpdateNotificationTimer.Tick += new EventHandler(SettingUpdateTimer_Tick);
            settingsUpdateNotificationTimer.Start();
            
        }


        //
        // Summary:
        //     Method to launch the terms of use page 
        //      
        // Returns:
        //     None.
        private void terms_Label_Click(object sender, EventArgs e)
        {
            // Launch the terms of use page

        }

        private void privacy_Label_Click(object sender, EventArgs e)
        {
            // Launch the privacy agreement page

        }

        private void resetButton_Click(object sender, EventArgs e)
        {
            // Prompt user for additional confirmation
            DialogResult res = MessageBox.Show("Are you sure you want to reset settings to their original state?",
                "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (res == DialogResult.Yes)
            {
                // Read the exe config file
                XmlDocument doc = new XmlDocument();
                doc.Load("WDAC Wizard.exe.config"); // Reading from the xml config file
                XmlNodeList settingsNodes = doc.GetElementsByTagName("setting");
                const int START = 15; 
                foreach(XmlNode settingNode in settingsNodes)
                {
                    int stop = settingNode.OuterXml.IndexOf("serialize") - 2;
                    string settingName = settingNode.OuterXml.Substring(START, stop - START);
                    string settingVal = settingNode.InnerText;
                    this.SettingsDict[settingName] = settingVal=="True";

                    this._Log.AddInfoMsg(String.Format("Parsed {0} = {1}", settingName, settingVal)); 
                }

                SetSettingsValues(this.SettingsDict);
            }
        }


        private void SettingsPage_Load(object sender, EventArgs e)
        {
            // On load, configure UI to match the app settings
            this.SettingsDict.Add("useEnvVars", (bool)Properties.Settings.Default["useEnvVars"]);
            this.SettingsDict.Add("useDefaultStrings", (bool)Properties.Settings.Default["useDefaultStrings"]);
            this.SettingsDict.Add("allowTelemetry", (bool)Properties.Settings.Default["allowTelemetry"]);
            this.SettingsDict.Add("createMultiPolicyByDefault", (bool)Properties.Settings.Default["createMultiPolicyByDefault"]); 

            this._Log.AddInfoMsg("Successfully read in the following Default Settings: ");
            foreach (var key in this.SettingsDict.Keys)
                this._Log.AddInfoMsg(String.Format("{0}: {1}", key, this.SettingsDict[key].ToString()));

            SetSettingsValues(this.SettingsDict); 
        }

        private void SetSettingsValues(Dictionary<string, bool> settingDict)
        {
           
            // if the setting is set to false, otherwise keep the default UI state of checked
            foreach (var settingName in settingDict.Keys)
            {
                string checkBoxName = settingName + "_CheckBox"; 
                if(!settingDict[settingName]) //False case
                {
                    this.Controls.Find(checkBoxName, true).FirstOrDefault().Tag = "Unchecked";
                    this.Controls.Find(checkBoxName, true).FirstOrDefault().BackgroundImage = Properties.Resources.check_box_unchecked; 
                }
                else
                {
                    this.Controls.Find(checkBoxName, true).FirstOrDefault().Tag = "Checked";
                    this.Controls.Find(checkBoxName, true).FirstOrDefault().BackgroundImage = Properties.Resources.check_box_checked;
                }

                this._Log.AddInfoMsg(String.Format("Setting {0} set to {1}", settingName, settingDict[settingName])); 
            }

            
        }

        private void SettingUpdateTimer_Tick(object sender, EventArgs e)
        {
            this.Update_Label.Visible = false; 
        }

        private void SettingCheckBox_Hover(object sender, EventArgs e)
        {
            // Change the background color when mouse is hovering above checkbox
            PictureBox checkBox = ((PictureBox)sender);
            checkBox.BackColor = Color.FromArgb(190, 230, 253);
        }


        /// <summary>
        /// Sets the back color to white for the checkbox picturebox when the user is no longer hovering the mouse over a checkbox.  
        /// </summary>
        /// <param name="sender">Sender is the picturebox control </param>
        private void SettingCheckBox_Leave(object sender, EventArgs e)
        {
            PictureBox checkBox = ((PictureBox)sender);
            checkBox.BackColor = Color.White;
        }
    }
}
