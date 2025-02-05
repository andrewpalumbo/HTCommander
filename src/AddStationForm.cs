﻿using System;
using System.Drawing;
using System.Windows.Forms;
using System.Xml.Linq;

namespace HTCommander
{
    public partial class AddStationForm : Form
    {
        private MainForm parent;

        public AddStationForm(MainForm parent)
        {
            this.parent = parent;
            InitializeComponent();

            stationTypeComboBox.SelectedIndex = 0;
            terminalProtocolComboBox.SelectedIndex = 0;

            // Setup radio channels
            if (parent.radio.Channels != null)
            {
                channelsComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                for (int i = 0; i < parent.radio.Channels.Length; i++)
                {
                    if ((parent.radio.Channels[i] != null) && (!string.IsNullOrEmpty(parent.radio.Channels[i].name_str)))
                    {
                        channelsComboBox.Items.Add(parent.radio.Channels[i].name_str);
                    }
                }
                channelsComboBox.SelectedIndex = 0;
            }
            else
            {
                channelsComboBox.DropDownStyle = ComboBoxStyle.DropDown;
            }

            // Setup APRS routes
            foreach (string aprsRoute in parent.aprsRoutes.Keys) { aprsRouteComboBox.Items.Add(aprsRoute); }
            aprsRouteComboBox.SelectedIndex = 0;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void AddStationForm_Load(object sender, EventArgs e)
        {
            callsignTextBox.Focus();
            UpdateInfo();
            UpdateTabs();
        }

        private void callsignTextBox_TextChanged(object sender, EventArgs e)
        {
            // Uppercase the callsign
            int selectionStart = callsignTextBox.SelectionStart;
            callsignTextBox.Text = callsignTextBox.Text.ToUpper();
            callsignTextBox.SelectionStart = selectionStart;
            Utils.SetPlaceholderText(ax25DestTextBox, callsignTextBox.Text);
            UpdateInfo();
        }

        private void UpdateInfo()
        {
            bool callsignOk = true;
            bool ok = true;

            // Check callsign
            AX25Address addr = AX25Address.GetAddress(callsignTextBox.Text);
            callsignTextBox.BackColor = (addr == null) ? Color.MistyRose : SystemColors.Window;
            if (addr == null) { callsignOk = ok = false; }

            // Check AX.25 address
            if (ax25DestTextBox.Text.Length > 0)
            {
                addr = AX25Address.GetAddress(ax25DestTextBox.Text);
                int i = ax25DestTextBox.Text.IndexOf('-');
                ax25DestTextBox.BackColor = ((addr == null) || (i == -1)) ? Color.MistyRose : SystemColors.Window;
                if ((addr == null) || (i == -1)) { ok = false; }
            }
            else
            {
                ax25DestTextBox.BackColor = SystemColors.Window;
            }

            if ((stationTypeComboBox.SelectedIndex == 2) && (terminalProtocolComboBox.SelectedIndex == 1) && (channelsComboBox.Text.Length == 0)) { ok = false; }

            channelsComboBox.BackColor = (channelsComboBox.Text.Length == 0) ? Color.MistyRose : SystemColors.Window;
            backButton.Enabled = (mainTabControl.SelectedIndex > 0);
            nextButton.Enabled = ((mainTabControl.SelectedIndex < (mainTabControl.TabPages.Count - 1)) && callsignOk) || ok;
            nextButton.Text = (mainTabControl.SelectedIndex < (mainTabControl.TabPages.Count - 1)) ? "Next" : "OK";

            if (terminalProtocolComboBox.SelectedIndex == 1)
            {
                label13.Visible = label14.Visible = true;
                ax25DestTextBox.Visible = true;
            }
            else
            {
                label13.Visible = label14.Visible = false;
                ax25DestTextBox.Visible = false;
            }
        }

        private void UpdateTabs()
        {
            mainTabControl.TabPages.Clear();
            mainTabControl.TabPages.Add(stationTabPage);
            if (stationTypeComboBox.SelectedIndex == 1) { mainTabControl.TabPages.Add(aprsTabPage); }
            if (stationTypeComboBox.SelectedIndex == 2) { mainTabControl.TabPages.Add(terminalTabPage); }
            UpdateInfo();
        }

        private void stationTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateTabs();
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            if (mainTabControl.SelectedIndex < mainTabControl.TabPages.Count - 1)
            {
                mainTabControl.SelectedIndex = mainTabControl.SelectedIndex + 1;
                UpdateInfo();
            }
            else
            {
                DialogResult = DialogResult.OK;
            }
        }

        private void backButton_Click(object sender, EventArgs e)
        {
            mainTabControl.SelectedIndex = mainTabControl.SelectedIndex - 1;
            UpdateInfo();
        }

        private void channelsComboBox_TextChanged(object sender, EventArgs e)
        {
            UpdateInfo();
        }

        private void terminalProtocolComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateInfo();
        }

        // Method to serialize the values into a DialogData object.
        public StationInfoClass SerializeToObject()
        {
            return new StationInfoClass
            {
                Callsign = callsignTextBox.Text,
                Name = nameTextBox.Text,
                Description = desciptionTextBox.Text,
                StationType = (StationInfoClass.StationTypes)stationTypeComboBox.SelectedIndex,
                APRSRoute = aprsRouteComboBox.Text,
                TerminalProtocol = (StationInfoClass.TerminalProtocols)terminalProtocolComboBox.SelectedIndex,
                Channel = channelsComboBox.Text,
                AX25Destination = ax25DestTextBox.Text
            };
        }

        public void FixStationType(StationInfoClass.StationTypes stationType)
        {
            stationTypeComboBox.SelectedIndex = (int)stationType;
            if (stationType == StationInfoClass.StationTypes.Generic) { Text = "Voice Station"; }
            if (stationType == StationInfoClass.StationTypes.APRS) { Text = "APRS Station"; }
            if (stationType == StationInfoClass.StationTypes.Terminal) { Text = "Terminal Station"; }
            stationTypeComboBox.Visible = false;
            typeOfStationLabel.Visible = false;
            stationTypeLabel.Visible = false;
        }

        // Method to populate the controls from a DialogData object.
        public void DeserializeFromObject(StationInfoClass data)
        {
            if (data == null) return;
            callsignTextBox.Text = data.Callsign;
            this.Text = (this.Text + " - " + callsignTextBox.Text);
            nameTextBox.Text = data.Name;
            desciptionTextBox.Text = data.Description;
            stationTypeComboBox.SelectedIndex = (int)data.StationType;
            aprsRouteComboBox.Text = data.APRSRoute;
            terminalProtocolComboBox.SelectedIndex = (int)data.TerminalProtocol;
            channelsComboBox.Text = data.Channel;
            ax25DestTextBox.Text = data.AX25Destination;
            callsignTextBox.Enabled = false;
            stationTypeComboBox.Visible = false;
            typeOfStationLabel.Visible = false;
            stationTypeLabel.Visible = false;
            if (data.StationType == StationInfoClass.StationTypes.Generic) { Text = "Voice Station - " + data.Callsign; }
            if (data.StationType == StationInfoClass.StationTypes.APRS) { Text = "APRS Station - " + data.Callsign; }
            if (data.StationType == StationInfoClass.StationTypes.Terminal) { Text = "Terminal Station - " + data.Callsign; }
        }

        private void ax25DestTextBox_TextChanged(object sender, EventArgs e)
        {
            // Uppercase the callsign
            int selectionStart = ax25DestTextBox.SelectionStart;
            ax25DestTextBox.Text = ax25DestTextBox.Text.ToUpper();
            ax25DestTextBox.SelectionStart = selectionStart;
            UpdateInfo();
        }

        private void ax25DestTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow letters, numbers, and the dash (-)
            if (!char.IsLetterOrDigit(e.KeyChar) && e.KeyChar != '-' && e.KeyChar != (char)Keys.Back)
            {
                e.Handled = true; // Block the input
            }
        }

        private void callsignTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow letters, numbers, and the dash (-)
            if (!char.IsLetterOrDigit(e.KeyChar) && e.KeyChar != '-' && e.KeyChar != (char)Keys.Back)
            {
                e.Handled = true; // Block the input
            }
        }

        private void channelsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateInfo();
        }
    }
}
