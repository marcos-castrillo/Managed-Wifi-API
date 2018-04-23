using System;
using System.Xml.Linq;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections;
using System.IO;
using NativeWifi;
using System.Linq;

namespace WifiExample
{

    public partial class Form1 : Form
    {
    
        public Form1()
        {
            this.BackColor = ColorTranslator.FromHtml("#adf4e7");
   
            scanWifi(out string[,] availableNetworksArray, out string[,] savedNetworksArray, out string[,] connectedNetworksArray, out string interfaceName);
            //Array.Sort(qualityArray, ssidArray, revComparer); //Sort by % of qualities. It just sorts the ssids, not the rest of the data
            IComparer revComparer = new ReverseComparer();
            InitializeComponent();

            //Define strings of data
            string[] headersArray = new string[5] { "SSID", "Strength", "Security", "Threshold", "Priority" };
            int nAvailableNetworks = availableNetworksArray.GetLength(0)-1;
            int nSavedNetworks = savedNetworksArray.GetLength(0)-1;
            int nConnectedNetworks = connectedNetworksArray.GetLength(0)-1;
            
            int nColumns = headersArray.Length;

            bool activeForm = false;
            bool activeCombobox = false;
            int widthSSID = 250;
            int widthStandar = 90;
            int heightHeader = 50;
            int heightStandar = 50;
            int showingLimit = 13;

            int coincidences = savedNetworksArray.GetLength(0) - 1;

            string path = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            var directory = System.IO.Path.GetDirectoryName(path);
            var localDirectory = new Uri(directory).LocalPath;


            //ELEMENTS:--------------------------


         
            
            bool refresh = false;
            
            
            createKnownNetworksPanel(Panel1);
            createDetectedNetworksPanel(Panel2);

            Panel1.Hide();
            Panel2.Hide();
            refreshForm(refresh);

            System.Windows.Forms.Timer timerRefresh = new System.Windows.Forms.Timer();
            timerRefresh.Interval = 5000; // specify interval time as you want
            timerRefresh.Tick += new EventHandler(timerRefresh_Tick);
            timerRefresh.Start();
            void timerRefresh_Tick(object sender, EventArgs e)
            {
                refreshForm(refresh);
            }

            System.Windows.Forms.Timer timerChange = new System.Windows.Forms.Timer();
            timerChange.Interval = 10000; // specify interval time as you want
            timerChange.Tick += new EventHandler(timerChange_Tick);
            timerChange.Start();
            void timerChange_Tick(object sender, EventArgs e)
            {
                changeForm(refresh);
            }


            //Methods
            void scanWifi(out string[,] theavailableNetworksArray, out string[,] thesavedNetworksArray, out string[,] theconnectedNetworksArray, out string theinterfaceName)
            {
                theavailableNetworksArray = new string[0, 0];
                thesavedNetworksArray = new string[0, 0];
                theconnectedNetworksArray = new string[0, 0];
                int theavailableNetworks = 0;
                int thesavedNetworks = 0;
                int theconnectedNetworks = 0;
                int others = 0;
                string connectedSSID = "adsadsdadadasd";
                WlanClient client = new WlanClient();
                bool intNameSaved = false;
                theinterfaceName = "";
                
                foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
                {

                    if (intNameSaved == false)
                        theinterfaceName = wlanIface.InterfaceName;
                    intNameSaved = true;
                    Wlan.WlanAvailableNetwork[] networks = wlanIface.GetAvailableNetworkList(0);
                    string[] savedSSIDarray = new string[100];//TODO
                    int totalNetworksNumber = networks.Length;

                    bool checkSavedSSID(string ssid)
                    {
                        for (int i = 0; i < savedSSIDarray.Length; i++)
                            if (ssid == savedSSIDarray[i])
                                return false;
                        return true;
                    }
                    foreach (Wlan.WlanAvailableNetwork network in networks)
                    {

                        if (network.flags.ToString() == "Connected, HasProfile")
                        {

                            theconnectedNetworks++;
                            connectedSSID = GetStringForSSID(network.dot11Ssid);
                        }
                        else if (network.flags == Wlan.WlanAvailableNetworkFlags.HasProfile && GetStringForSSID(network.dot11Ssid) != connectedSSID)
                        {
                            thesavedNetworks++;

                            savedSSIDarray[thesavedNetworks] = GetStringForSSID(network.dot11Ssid);
                        }
                        else if (network.flags == 0 && GetStringForSSID(network.dot11Ssid) != connectedSSID && checkSavedSSID(GetStringForSSID(network.dot11Ssid)))
                        {


                            theavailableNetworks++;
                        }
                        else
                            others++;
                    }

                    theavailableNetworksArray = new string[theavailableNetworks + 1, 5];

                    thesavedNetworksArray = new string[thesavedNetworks + 1, 5];
                    theconnectedNetworksArray = new string[theconnectedNetworks + 1, 5];
                    headersArray = new string[5] { "Wi-Fi", "Strength", "Security", "Threshold", "Priority" };

                    for (int i = 0; i < 5; i++)
                    {
                        theavailableNetworksArray[0, i] = headersArray[i];
                        thesavedNetworksArray[0, i] = headersArray[i];
                        theconnectedNetworksArray[0, i] = headersArray[i];
                    }

                    uint[] quaArray = new uint[1];
                    Wlan.Dot11CipherAlgorithm[] secArray = new Wlan.Dot11CipherAlgorithm[1];
                    string[] secAux = new string[1];

                    int countAvailable = 1;
                    int countSaved = 1;
                    int countConnected = 1;
                    int y = 0;

                    string GetStringForSSID(Wlan.Dot11Ssid ssid)
                    {
                        return Encoding.ASCII.GetString(ssid.SSID, 0, (int)ssid.SSIDLength);
                    }
                    foreach (Wlan.WlanAvailableNetwork network in networks)
                    {

                        void fillArray(string[,] networksArray, int nextRow)
                        {
                            y = 0;
                            networksArray[nextRow, 0] = GetStringForSSID(network.dot11Ssid);

                            quaArray[y] = network.wlanSignalQuality;
                            networksArray[nextRow, 1] = quaArray[y].ToString();

                            secArray[y] = network.dot11DefaultCipherAlgorithm;
                            string secType = secArray[y].ToString();
                            if (secType == "None")
                                secAux[y] = "Open";
                            else if (secType == "WEP40")
                                secAux[y] = "WEP";
                            else if (secType == "TKIP")
                                secAux[y] = "WPA";
                            else if (secType == "CCMP")
                                secAux[y] = "WPA2";
                            else if (secType == "WEP104")
                                secAux[y] = "WEP";
                            else if (secType == "WPA_UseGroup")
                                secAux[y] = "WPA";
                            else if (secType == "RSN_UseGroup")
                                secAux[y] = "WPA2";
                            else if (secType == "WEP")
                                secAux[y] = "WEP";
                            else
                                secAux[y] = "Unknown";
                            networksArray[nextRow, 2] = secAux[y];

                            y++;


                        }

                        if (network.flags.ToString() == "Connected, HasProfile")
                        {

                            fillArray(connectedNetworksArray, countConnected);

                            countConnected++;
                        }
                        else if (network.flags == Wlan.WlanAvailableNetworkFlags.HasProfile && GetStringForSSID(network.dot11Ssid) != connectedSSID)
                        {

                            fillArray(savedNetworksArray, countSaved);
                            countSaved++;
                        }
                        else if (network.flags == 0 && GetStringForSSID(network.dot11Ssid) != connectedSSID && checkSavedSSID(GetStringForSSID(network.dot11Ssid)))
                        {
                            fillArray(availableNetworksArray, countAvailable);
                            countAvailable++;
                        }

                        y++;
                    }
                }
            
            }


            
            void refreshForm(bool refreshBit)
            {
                if (activeForm==false&&activeCombobox==false) {
                    
                    if (refreshBit == false)
                    {

                        scanWifi(out availableNetworksArray, out savedNetworksArray, out connectedNetworksArray, out interfaceName);
                        nAvailableNetworks = availableNetworksArray.GetLength(0);
                        nSavedNetworks = savedNetworksArray.GetLength(0);
                        nConnectedNetworks = connectedNetworksArray.GetLength(0);
                        coincidences = savedNetworksArray.GetLength(0) - 1;
                        createKnownNetworksPanel(Panel3);
                        createDetectedNetworksPanel(Panel4);

                    }
                    else {

                        scanWifi(out availableNetworksArray, out savedNetworksArray, out connectedNetworksArray, out interfaceName);
                        nAvailableNetworks = availableNetworksArray.GetLength(0);
                        nSavedNetworks = savedNetworksArray.GetLength(0);
                        nConnectedNetworks = connectedNetworksArray.GetLength(0);
                        coincidences = savedNetworksArray.GetLength(0) - 1;
                        createKnownNetworksPanel(Panel1);
                        createDetectedNetworksPanel(Panel2);


                    }
                }
            }
            void changeForm(bool refreshBit)
            {

                    if (refreshBit == false)
                            {
                                Panel3.Show();
                                Panel4.Show();
                                Panel1.Hide();
                                Panel2.Hide();
                                refreshBit = true;
                            }
                            else
                            {
                                Panel1.Show();
                                Panel2.Show();
                                Panel3.Hide();
                                Panel4.Hide();
                                refreshBit = false;
                            }

                
            }

                void createKnownNetworksPanel(TableLayoutPanel panel)
            {
                Controls.Add(new TableLayoutPanel { Dock = DockStyle.Fill });

               
                
                panel.Controls.Clear();
                panel.ColumnStyles.Clear();
                panel.RowStyles.Clear();

                panel.ColumnCount = nColumns;
                panel.RowCount = coincidences + 1;
                panel.BorderStyle = 0;
                panel.BackColor=ColorTranslator.FromHtml("#FFFFFF");
                
                for (int i = 0; i < coincidences + 1; i++)
                {
                    for (int j = 0; j < nColumns; j++)
                    {
                        if (i == 0)
                        {
                            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                            TextBox titleTextBox = new TextBox();
                            titleTextBox.Width = panel.Width;
                            titleTextBox.Location = new Point(0, titleTextBox.Location.Y);
                            titleTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                            titleTextBox.Text = headersArray[j];
                            titleTextBox.Size = new System.Drawing.Size(widthStandar, heightHeader);

                            titleTextBox.Font = new Font(new FontFamily(System.Drawing.Text.GenericFontFamilies.Monospace), 13, FontStyle.Bold);
                        
                            titleTextBox.TextAlign = HorizontalAlignment.Center;
                            titleTextBox.Dock = DockStyle.Fill;
                            titleTextBox.ReadOnly = true;
                            titleTextBox.BorderStyle = 0;
                            
                            titleTextBox.ForeColor = ColorTranslator.FromHtml("#000000");
                            titleTextBox.BackColor = panel.BackColor;

                            titleTextBox.TabStop = false;

                            panel.Controls.Add(titleTextBox, j, i);
                        }
                        
                        else if (j == 0)
                        {
                            panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                            Button ssidButton = new Button();
                            ssidButton.Text = savedNetworksArray[i, j];
                            ssidButton.Font = new Font(new FontFamily(System.Drawing.Text.GenericFontFamilies.Monospace), 11);
                            ssidButton.ForeColor = Color.DarkBlue;
                            ssidButton.BackColor = Color.White;
                            ssidButton.Size = new System.Drawing.Size(widthSSID, heightStandar);
                            ssidButton.Height = 20;
                            ssidButton.TabStop = false;
                            ssidButton.TextAlign = ContentAlignment.MiddleCenter;
                            ssidButton.Click += new System.EventHandler(ssidButtonNewNetwork_Click);
                            panel.Controls.Add(ssidButton, j, i);
                            
                        }

                        else if (j == 1)
                        {
                            TextBox qualityTextBox = new TextBox();
                            qualityTextBox.Text = savedNetworksArray[i, j] + "%";
                            qualityTextBox.Size = new System.Drawing.Size(widthStandar, heightStandar);
                            qualityTextBox.Font = new Font(new FontFamily(System.Drawing.Text.GenericFontFamilies.Monospace), 12);
                            qualityTextBox.TextAlign = HorizontalAlignment.Center;
                            qualityTextBox.ReadOnly = true;
                            qualityTextBox.BorderStyle = 0;
                            qualityTextBox.ForeColor = ColorTranslator.FromHtml("#000000");
                            qualityTextBox.BackColor = panel.BackColor;
                            qualityTextBox.TabStop = false;

                            if (savedNetworksArray[i, j] != null)
                            {
                                Label imgLabel = new Label();
                                int percentage = Int32.Parse(savedNetworksArray[i, j]);
                                if (percentage > 60)
                                    imgLabel.Image = Image.FromFile(localDirectory + @"\storage\WiFiSignal100.png");
                                else if (percentage < 60 && percentage > 25)
                                    imgLabel.Image = Image.FromFile(localDirectory + @"\storage\WiFiSignal66.png");
                                else if (percentage < 25 && percentage > 5)
                                    imgLabel.Image = Image.FromFile(localDirectory + @"\storage\WiFiSignal33.png");
                                else
                                    imgLabel.Image = Image.FromFile(localDirectory + @"\storage\WiFiSignal0.png");
                                imgLabel.AutoSize = false;
                                imgLabel.Size = imgLabel.Image.Size;
                                imgLabel.ImageAlign = ContentAlignment.MiddleCenter;
                                imgLabel.Text = "";
                                imgLabel.BackColor = Color.Transparent;
                                imgLabel.Parent = qualityTextBox;
                                imgLabel.Location = new Point(65, 1);
                            }
                            panel.Controls.Add(qualityTextBox, j, i);
                        }
                        else if (j == 2)
                        {
                            TextBox securityTextBox = new TextBox();
                            securityTextBox.Text = savedNetworksArray[i, j];
                            securityTextBox.Size = new System.Drawing.Size(widthStandar, heightStandar);
                            securityTextBox.Font = new Font(new FontFamily(System.Drawing.Text.GenericFontFamilies.Monospace), 12);
                            securityTextBox.TextAlign = HorizontalAlignment.Center;
                            panel.Controls.Add(securityTextBox, j, i);
                            securityTextBox.ReadOnly = true;
                            securityTextBox.BorderStyle = 0;
                            securityTextBox.ForeColor = ColorTranslator.FromHtml("#000000");
                            securityTextBox.BackColor = panel.BackColor;
                            securityTextBox.TabStop = false;
                        }
                        else if (j == 3)
                        {
                            NumericUpDown thresholdUpDown = new NumericUpDown();
                           
                            String ssidName = savedNetworksArray[i, 0];
                            string xmlName = @localDirectory + "/thresholds.xml";
                            XDocument doc;
                            try
                            {
                                // Use the file
                                doc = XDocument.Load(xmlName);
                                try
                                {
                                   thresholdUpDown.Value = decimal.Parse(doc.Root.Element(ssidName).Value);
                                }
                                catch (Exception)
                                {
                                }
                            }
                            catch (FileNotFoundException)
                            {

                            }
                            thresholdUpDown.Size = new System.Drawing.Size(widthStandar, heightStandar);
                            thresholdUpDown.Font = new Font(new FontFamily(System.Drawing.Text.GenericFontFamilies.Monospace), 10);
                            thresholdUpDown.TextAlign = HorizontalAlignment.Center;
                            thresholdUpDown.ValueChanged += (sender, e) => { activeCombobox = false; };
                            thresholdUpDown.ValueChanged += new System.EventHandler(thresholdTextBox_ValueChanged);
                            thresholdUpDown.GotFocus += (sender, e) => { activeCombobox = true; };
                            thresholdUpDown.LostFocus += (sender, e) => { activeCombobox = false; };
                            thresholdUpDown.Maximum = 100;
                            thresholdUpDown.Minimum = 0;



                            panel.Controls.Add(thresholdUpDown, j, i);

                        }
                        else if (j == 4)
                        {
                            ComboBox priorityComboBox = new ComboBox();
                            priorityComboBox.Location = new System.Drawing.Point(128, 48);
                            priorityComboBox.Name = "ComboBox1";
                            priorityComboBox.Size = new System.Drawing.Size(90, 20);
                            priorityComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                            string[] priorities = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
                            priorityComboBox.Items.AddRange(priorities);
                            priorityComboBox.SelectedIndexChanged += (sender, e) => { activeCombobox = false; };
                            priorityComboBox.SelectedIndexChanged += (sender2, e2) => priorityComboBox_SelectedIndexChanged(sender2, e2, interfaceName);
                            priorityComboBox.GotFocus += (sender, e) => { activeCombobox = true; };
                            priorityComboBox.LostFocus += (sender, e) => { activeCombobox = false; };

                            String ssidName = savedNetworksArray[i, 0];
                            string xmlName = @localDirectory + "/priorities.xml";

                            XDocument doc;
                            try
                            {
                                // Use the file
                                doc = XDocument.Load(xmlName);
                                try
                                {
                                    priorityComboBox.Text = doc.Root.Element(ssidName).Value;
                                }
                                catch (Exception)
                                {
                                }
                            }
                            catch (FileNotFoundException)
                            {

                            }

                            panel.Controls.Add(priorityComboBox, j, i);
                        }

                    }




                }
                
                if (connectedNetworksArray.GetLength(0) > 1)
                {
                    
                    foreach (Control c in panel.Controls)
                    {
                        if(panel.GetRow(c)>0)
                            panel.SetRow(c, panel.GetRow(c) + 1);
                    }

                    for (int j = 0; j < nColumns; j++)
                    {
                       
                        if (j == 0)
                        {
                            panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                            Button ssidButton = new Button();
                            ssidButton.Text = connectedNetworksArray[1, j];
                            ssidButton.Font = new Font(new FontFamily(System.Drawing.Text.GenericFontFamilies.Monospace), 11, FontStyle.Bold);
                            ssidButton.ForeColor = Color.Blue;
                            ssidButton.BackColor = Color.White;
                            ssidButton.Size = new System.Drawing.Size(widthSSID, heightStandar);
                            ssidButton.TabStop = false;
                            ssidButton.TextAlign = ContentAlignment.MiddleCenter;
                            ssidButton.Height = 20;
                            ssidButton.Click += new System.EventHandler(ssidButtonNewNetwork_Click);
                            panel.Controls.Add(ssidButton, j, 1);

                        }

                        else if (j == 1)
                        {
                            TextBox qualityTextBox = new TextBox();
                            qualityTextBox.Text = connectedNetworksArray[1, j] + "%";
                            qualityTextBox.Size = new System.Drawing.Size(widthStandar, heightStandar);
                            qualityTextBox.Font = new Font(new FontFamily(System.Drawing.Text.GenericFontFamilies.Monospace), 12);
                            qualityTextBox.TextAlign = HorizontalAlignment.Center;
                            qualityTextBox.ReadOnly = true;
                            qualityTextBox.BorderStyle = 0;
                            qualityTextBox.BackColor = panel.BackColor;
                            qualityTextBox.TabStop = false;

                            if (connectedNetworksArray[1, j] != null)
                            {
                                Label imgLabel = new Label();
                                int percentage = Int32.Parse(connectedNetworksArray[1, j]);
                                if (percentage > 200 / 3)
                                    imgLabel.Image = Image.FromFile(localDirectory + @"\storage\WiFiSignal100.png");
                                else if (percentage < 200 / 3 && percentage > 100 / 3)
                                    imgLabel.Image = Image.FromFile(localDirectory + @"\storage\WiFiSignal66.png");
                                else if (percentage < 100 / 3 && percentage > 5)
                                    imgLabel.Image = Image.FromFile(localDirectory + @"\storage\WiFiSignal33.png");
                                else
                                    imgLabel.Image = Image.FromFile(localDirectory + @"\storage\WiFiSignal0.png");
                                imgLabel.AutoSize = false;
                                imgLabel.Size = imgLabel.Image.Size;
                                imgLabel.ImageAlign = ContentAlignment.MiddleCenter;
                                imgLabel.Text = "";
                                imgLabel.BackColor = Color.Transparent;
                                imgLabel.Parent = qualityTextBox;
                                imgLabel.Location = new Point(65, 1);
                            }
                            panel.Controls.Add(qualityTextBox, j, 1);
                        }
                        else if (j == 2)
                        {
                            TextBox securityTextBox = new TextBox();
                            securityTextBox.Text = connectedNetworksArray[1, j];
                            securityTextBox.Size = new System.Drawing.Size(widthStandar, heightStandar);
                            securityTextBox.Font = new Font(new FontFamily(System.Drawing.Text.GenericFontFamilies.Monospace), 12);
                            securityTextBox.TextAlign = HorizontalAlignment.Center;
                            panel.Controls.Add(securityTextBox, j, 1);
                            securityTextBox.ReadOnly = true;
                            securityTextBox.BorderStyle = 0;
                            securityTextBox.BackColor = panel.BackColor;
                            securityTextBox.TabStop = false;
                        }
                        else if (j == 3)
                        {
                            NumericUpDown thresholdUpDown = new NumericUpDown();
                           
                          
                            String ssidName = connectedNetworksArray[1, 0];
                            string xmlName = @localDirectory + "/thresholds.xml";

                            XDocument doc;
                            try
                            {
                                // Use the file
                                doc = XDocument.Load(xmlName);
                                try
                                {
                                    thresholdUpDown.Value = Convert.ToDecimal(doc.Root.Element(ssidName).Value);
                                }
                                catch (Exception)
                                {
                                }
                            }
                            catch (FileNotFoundException)
                            {

                            }
                            thresholdUpDown.Size = new System.Drawing.Size(widthStandar, heightStandar);
                            thresholdUpDown.Font = new Font(new FontFamily(System.Drawing.Text.GenericFontFamilies.Monospace), 10);
                            thresholdUpDown.TextAlign = HorizontalAlignment.Center;
                            thresholdUpDown.ValueChanged += (sender, e) => { activeCombobox = false; };
                            thresholdUpDown.ValueChanged += new System.EventHandler(thresholdTextBox_ValueChanged);
                            thresholdUpDown.GotFocus += (sender, e) => { activeCombobox = true; };
                            thresholdUpDown.LostFocus += (sender, e) => { activeCombobox = false; };
                            thresholdUpDown.Maximum = 100;
                            thresholdUpDown.Minimum = 0;


                            panel.Controls.Add(thresholdUpDown, j, 1);

                        }
                        else if (j == 4)
                        {
                            ComboBox priorityComboBox = new ComboBox();
                            priorityComboBox.Location = new System.Drawing.Point(128, 48);
                            priorityComboBox.Name = "ComboBox1";
                            priorityComboBox.Size = new System.Drawing.Size(90, 20);
                            priorityComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                            string[] priorities = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
                            priorityComboBox.Items.AddRange(priorities);
                            priorityComboBox.SelectedIndexChanged += (sender2, e2) => priorityComboBox_SelectedIndexChanged(sender2, e2, interfaceName);
                            priorityComboBox.SelectedIndexChanged += (sender, e) => { activeCombobox = false; };
                            priorityComboBox.GotFocus += (sender, e) => { activeCombobox = true; };
                            priorityComboBox.LostFocus += (sender, e) => { activeCombobox = false;  };
                            String ssidName = connectedNetworksArray[1, 0];
                            string xmlName = @localDirectory + "/priorities.xml";

                            XDocument doc;
                            try
                            {
                                // Use the file
                                doc = XDocument.Load(xmlName);
                                try
                                {
                                    priorityComboBox.Text = doc.Root.Element(ssidName).Value;
                                }
                                catch (Exception)
                                {
                                }
                            }
                            catch (FileNotFoundException)
                            {

                            }

                            panel.Controls.Add(priorityComboBox, j, 1);
                        }

                    }

                }
                panel.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
                panel.Size = new System.Drawing.Size(4 * widthStandar + widthSSID,heightStandar * coincidences);
                panel.AutoSize = true;
                if (coincidences > showingLimit)
                {
                    panel.AutoSize = false;
                    panel.AutoScroll = true;
                    panel.Size = new System.Drawing.Size(4 * widthStandar + widthSSID + widthStandar / 2, heightHeader + heightStandar * coincidences);
                }
                if (coincidences == 0&&connectedNetworksArray.GetLength(0)==1)
                {
                    panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                    Label noNetworkLabel = new Label();
                    Label emptyLabel = new Label();
                    noNetworkLabel.Text = "No saved networks";
                    noNetworkLabel.Size = new System.Drawing.Size(widthSSID, heightHeader);
                    noNetworkLabel.Font = new Font(new FontFamily(System.Drawing.Text.GenericFontFamilies.Monospace), 15, FontStyle.Bold);
                    noNetworkLabel.TextAlign = ContentAlignment.MiddleRight;

                    panel.Controls.Add(noNetworkLabel, 1, 1);
                    panel.SetColumnSpan(noNetworkLabel, 3);

                }
                panel.Show();
            }



            void createDetectedNetworksPanel(TableLayoutPanel panel)
            {
                Controls.Add(new TableLayoutPanel { Dock = DockStyle.Fill });
               
                panel.Controls.Clear();

                panel.ColumnStyles.Clear();
                panel.RowStyles.Clear();

                panel.ColumnCount = nColumns - 2;
                panel.RowCount = nAvailableNetworks;
                panel.BackColor = ColorTranslator.FromHtml("#FFFFFF");
                for (int i = 0; i < nAvailableNetworks; i++)
                {
                    for (int j = 0; j < nColumns - 2; j++)
                    {

                        if (i == 0)
                        {
                            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                            TextBox titleTextBox = new TextBox();
                            titleTextBox.Text = headersArray[j];
                            titleTextBox.Size = new System.Drawing.Size(widthStandar, heightHeader);
                            titleTextBox.Font = new Font(new FontFamily(System.Drawing.Text.GenericFontFamilies.Monospace), 13, FontStyle.Bold);
                            titleTextBox.TextAlign = HorizontalAlignment.Center;
                            titleTextBox.Dock = DockStyle.Fill;
                            titleTextBox.ReadOnly = true;
                            titleTextBox.BorderStyle = 0;
                            titleTextBox.ForeColor = ColorTranslator.FromHtml("#000000");
                            titleTextBox.BackColor = panel.BackColor;
                            titleTextBox.TabStop = false;
                            panel.Controls.Add(titleTextBox, j, i);
                        }

                        else if (j == 0)
                        {
                            panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                            Button ssidButton = new Button();
                            ssidButton.Text = availableNetworksArray[i, j];
                            ssidButton.Size = new System.Drawing.Size(widthSSID, heightStandar);
                            ssidButton.Font = new Font(new FontFamily(System.Drawing.Text.GenericFontFamilies.Monospace), 11);
                            ssidButton.TextAlign = ContentAlignment.MiddleCenter;
                            ssidButton.ForeColor = ColorTranslator.FromHtml("#000000");
                            ssidButton.BackColor = panel.BackColor;
                            ssidButton.TabStop = false;
                            ssidButton.Height = 20;
                            ssidButton.Click += new System.EventHandler(ssidButtonNewNetwork_Click);
                            panel.Controls.Add(ssidButton, j, i);
                        }

                        else if (j == 1)
                        {
                            TextBox qualityTextBox = new TextBox();
                            qualityTextBox.Text = availableNetworksArray[i, j] + "%";
                            qualityTextBox.Size = new System.Drawing.Size(widthStandar, heightStandar);
                            qualityTextBox.Font = new Font(new FontFamily(System.Drawing.Text.GenericFontFamilies.Monospace), 12);
                            qualityTextBox.TextAlign = HorizontalAlignment.Center;
                            qualityTextBox.ReadOnly = true;
                            qualityTextBox.BorderStyle = 0;
                            qualityTextBox.ForeColor = ColorTranslator.FromHtml("#000000");
                            qualityTextBox.BackColor = panel.BackColor;
                            qualityTextBox.TabStop = false;
                            Label imgLabel = new Label();
                            if (availableNetworksArray[i, j] != null)
                            {
                                int percentage = Int32.Parse(availableNetworksArray[i, j]);
                                if (percentage > 200 / 3)
                                    imgLabel.Image = Image.FromFile(localDirectory + @"\storage\WiFiSignal100.png");
                                else if (percentage < 200 / 3 && percentage > 100 / 3)
                                    imgLabel.Image = Image.FromFile(localDirectory + @"\storage\WiFiSignal66.png");
                                else if (percentage < 100 / 3 && percentage > 5)
                                    imgLabel.Image = Image.FromFile(localDirectory + @"\storage\WiFiSignal33.png");
                                else
                                    imgLabel.Image = Image.FromFile(localDirectory + @"\storage\WiFiSignal0.png");
                                imgLabel.AutoSize = false;
                                imgLabel.Size = imgLabel.Image.Size;
                                imgLabel.ImageAlign = ContentAlignment.MiddleCenter;
                                imgLabel.Text = "";
                                imgLabel.BackColor = Color.Transparent;
                                imgLabel.Parent = qualityTextBox;
                                imgLabel.Location = new Point(65, 1);
                            }
                            panel.Controls.Add(qualityTextBox, j, i);

                        }
                        else if (j == 2)
                        {
                            TextBox securityTextBox = new TextBox();
                            securityTextBox.Text = availableNetworksArray[i, j];
                            securityTextBox.Size = new System.Drawing.Size(widthStandar, heightStandar);
                            securityTextBox.Font = new Font(new FontFamily(System.Drawing.Text.GenericFontFamilies.Monospace), 12);
                            securityTextBox.TextAlign = HorizontalAlignment.Center;
                            securityTextBox.ReadOnly = true;
                            securityTextBox.BorderStyle = 0;
                            securityTextBox.ForeColor = ColorTranslator.FromHtml("#000000");
                            securityTextBox.BackColor = panel.BackColor;
                            securityTextBox.TabStop = false;
                            panel.Controls.Add(securityTextBox, j, i);
                        }
                    }


                }
                panel.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
                panel.Size = new System.Drawing.Size(widthSSID + 2 * widthStandar, heightStandar * coincidences);
                panel.AutoSize = true;
                if (nAvailableNetworks > showingLimit)
                {
                    panel.AutoSize = false;
                    panel.AutoScroll = true;
                    panel.Size = new System.Drawing.Size(widthSSID + 2 * widthStandar + widthStandar / 2, showingLimit * heightStandar);
                }
                if (nAvailableNetworks==1)
                {
                    panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                    Label noNetworkLabel = new Label();
                    Label emptyLabel = new Label();
                    noNetworkLabel.Text = "No new networks";
                    noNetworkLabel.Size = new System.Drawing.Size(widthSSID, heightHeader);
                    noNetworkLabel.Font = new Font(new FontFamily(System.Drawing.Text.GenericFontFamilies.Monospace), 15, FontStyle.Bold);
                    noNetworkLabel.TextAlign = ContentAlignment.MiddleRight;
                    panel.Controls.Add(noNetworkLabel, 0, 1);
                    panel.SetColumnSpan(noNetworkLabel, 3);

                }
                panel.Show();
            }



            //CLICKS

            void ssidButtonNewNetwork_Click(object sender, System.EventArgs e)
            {
                WlanClient client = new WlanClient();
                Button clickedButton = (Button)sender;

                foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
                {
                    Form form = clickedButton.FindForm();
                    Control percentageControl = form.GetNextControl(clickedButton, true);
                    Control securityControl = form.GetNextControl(percentageControl, true);
                    securityControl = form.GetNextControl(securityControl, true);
                    Control thresholdControl = form.GetNextControl(securityControl, true);


                    string xmlName = @localDirectory + "/thresholds.xml";

                    XDocument doc;
                    String threshold = "";
                    try
                    {
                        // Use the file
                        doc = XDocument.Load(xmlName);
                        try
                        {
                            threshold = doc.Root.Element(clickedButton.Text).Value;
                        }
                        catch (Exception)
                        {

                        }
                    }
                    catch (FileNotFoundException)
                    { }


                    string perc = percentageControl.Text.Substring(0, percentageControl.Text.Length - 1);


                    if (threshold != "" && Convert.ToInt32(perc) < Convert.ToInt32(threshold))
                    {
                        Control nextcontrol = form.GetNextControl(clickedButton, true);
                        nextcontrol = form.GetNextControl(nextcontrol, true);
                        nextcontrol = form.GetNextControl(nextcontrol, true);
                        nextcontrol = form.GetNextControl(nextcontrol, true);
                        nextcontrol.BackColor = Color.Red;
                        DateTime _desired = DateTime.Now.AddSeconds(1);
                        while (DateTime.Now < _desired)
                        {
                            System.Windows.Forms.Application.DoEvents();
                        }
                        nextcontrol.BackColor = Color.White;
                    }
                    else if (clickedButton.ForeColor == Color.Blue)
                    {
                        activeForm = true;
                        Prompt.ShowDeleteDialog("", "Forget " + clickedButton.Text + "?", clickedButton.Text);
                        activeForm = false;
                        refreshForm(refresh);
                        
                    }
                    else if (clickedButton.ForeColor == Color.DarkBlue)
                    {
                        activeForm = true;
                        Prompt.ShowKeyDeleteDialog("", "Choose an option", clickedButton.Text);
                        activeForm = false;
                        refreshForm(refresh);
                        
                    }




                    else
                    {
                        string securityText = securityControl.Text;
                        string profileName = clickedButton.Text;

                        if (securityText == "Open")//add threshold
                        {
                            xmlName = @localDirectory + "/open.xml";

                            //xml format
                            XNamespace empNM = "http://www.microsoft.com/networking/WLAN/profile/v1";
                            doc = new XDocument(
                                new XElement(empNM + "WLANProfile",
                                               new XElement("name", "{0}"),
                                                   new XElement("SSIDConfig",
                                                        new XElement("SSID",
                                                            new XElement("name", "{0}")),
                                                        new XElement("nonBroadcast", "false")),
                                                   new XElement("connectionType", "ESS"),
                                                   new XElement("connectionMode", "manual"),
                                                   new XElement("MSM",
                                                        new XElement("security",
                                                            new XElement("authEncryption",
                                                                 new XElement("authentication", "open"),
                                                                 new XElement("encryption", "none"),
                                                                 new XElement("useOneX", "false"))))));

                            //delete all the xmlns="" in the code
                            foreach (var node in doc.Root.Descendants()
                                    .Where(n => n.Name.NamespaceName == ""))
                            {
                                node.Attributes("xmlns").Remove();
                                node.Name = node.Parent.Name.Namespace + node.Name.LocalName;
                            }

                            doc.Save(xmlName);


                            string xmlcontents = XDocument.Load(xmlName).ToString();
                            //Add xml declaration at the beginning
                            string profileXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" + string.Format(xmlcontents, profileName);
                            wlanIface.SetProfile(Wlan.WlanProfileFlags.AllUser, profileXml, true);
                            wlanIface.Connect(Wlan.WlanConnectionMode.Profile, Wlan.Dot11BssType.Any, profileName);
                            File.Delete(xmlName);
                        }
                        else if (securityText == "WEP")
                        {
                            xmlName = @localDirectory + "/wep.xml";
                            activeForm = true;
                            string key = Prompt.ShowKeyDialog("", "Insert key");
                            activeForm = false;

                            if (key != "")
                            {

                                //xml format
                                XNamespace empNM = "http://www.microsoft.com/networking/WLAN/profile/v1";
                                doc = new XDocument(
                                    new XElement(empNM + "WLANProfile",
                                                   new XElement("name", "{0}"),
                                                       new XElement("SSIDConfig",
                                                            new XElement("SSID",
                                                                new XElement("name", "{0}"))),
                                                       new XElement("connectionType", "ESS"),
                                                       new XElement("connectionMode", "manual"),
                                                       new XElement("MSM",
                                                            new XElement("security",
                                                                new XElement("authEncryption",
                                                                     new XElement("authentication", "open"),
                                                                     new XElement("encryption", "WEP"),
                                                                     new XElement("useOneX", "false")),
                                                                 new XElement("sharedKey",
                                                                     new XElement("keyType", "networkKey"),
                                                                     new XElement("protected", "false"),
                                                                     new XElement("keyMaterial", "{1}"))))));

                                //delete all the xmlns="" in the code
                                foreach (var node in doc.Root.Descendants()
                                        .Where(n => n.Name.NamespaceName == ""))
                                {
                                    node.Attributes("xmlns").Remove();
                                    node.Name = node.Parent.Name.Namespace + node.Name.LocalName;
                                }

                                doc.Save(xmlName);


                                string xmlcontents = XDocument.Load(xmlName).ToString();
                                //Add xml declaration at the beginning
                                string profileXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" + string.Format(xmlcontents, profileName, key);

                                wlanIface.SetProfile(Wlan.WlanProfileFlags.AllUser, profileXml, true);
                                wlanIface.Connect(Wlan.WlanConnectionMode.Profile, Wlan.Dot11BssType.Any, profileName);
                                File.Delete(xmlName);

                                Process cmd = new Process();
                                cmd.StartInfo.FileName = "cmd.exe";
                                cmd.StartInfo.RedirectStandardInput = true;
                                cmd.StartInfo.RedirectStandardOutput = true;
                                cmd.StartInfo.CreateNoWindow = true;
                                cmd.StartInfo.UseShellExecute = false;
                                cmd.Start();

                                string command = String.Format(@"netsh wlan set profileparameter name = ""{0}"" connectionmode = manual", profileName);
                                cmd.StandardInput.WriteLine(command);
                                cmd.StandardInput.Flush();
                                cmd.StandardInput.Close();
                                cmd.WaitForExit();
                                Console.WriteLine(cmd.StandardOutput.ReadToEnd());

                                refreshForm(refresh);
                            }
                        }
                        else if (securityText == "WPA")
                        {
                            xmlName = @localDirectory + "/wep.xml";
                            activeForm = true;
                            string key = Prompt.ShowKeyDialog("", "Insert key");
                            activeForm = false;


                            if (key != "" && key.Length >= 8)
                            {

                                //xml format
                                XNamespace empNM = "http://www.microsoft.com/networking/WLAN/profile/v1";
                                doc = new XDocument(
                                    new XElement(empNM + "WLANProfile",
                                                   new XElement("name", "{0}"),
                                                       new XElement("SSIDConfig",
                                                            new XElement("SSID",
                                                                new XElement("name", "{0}"))),
                                                       new XElement("connectionType", "ESS"),
                                                       new XElement("connectionMode", "auto"),
                                                       new XElement("autoSwitch", "false"),
                                                       new XElement("MSM",
                                                            new XElement("security",
                                                                new XElement("authEncryption",
                                                                     new XElement("authentication", "WPAPSK"),
                                                                     new XElement("encryption", "TKIP"),
                                                                     new XElement("useOneX", "false")),
                                                                 new XElement("sharedKey",
                                                                     new XElement("keyType", "passPhrase"),
                                                                     new XElement("protected", "false"),
                                                                     new XElement("keyMaterial", "{1}"))))));

                                //delete all the xmlns="" in the code
                                foreach (var node in doc.Root.Descendants()
                                        .Where(n => n.Name.NamespaceName == ""))
                                {
                                    node.Attributes("xmlns").Remove();
                                    node.Name = node.Parent.Name.Namespace + node.Name.LocalName;
                                }

                                doc.Save(xmlName);


                                string xmlcontents = XDocument.Load(xmlName).ToString();
                                //Add xml declaration at the beginning
                                string profileXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" + string.Format(xmlcontents, profileName, key);

                                wlanIface.SetProfile(Wlan.WlanProfileFlags.AllUser, profileXml, true);
                                wlanIface.Connect(Wlan.WlanConnectionMode.Profile, Wlan.Dot11BssType.Any, profileName);
                                File.Delete(xmlName);

                                Process cmd = new Process();
                                cmd.StartInfo.FileName = "cmd.exe";
                                cmd.StartInfo.RedirectStandardInput = true;
                                cmd.StartInfo.RedirectStandardOutput = true;
                                cmd.StartInfo.CreateNoWindow = true;
                                cmd.StartInfo.UseShellExecute = false;
                                cmd.Start();

                                string command = String.Format(@"netsh wlan set profileparameter name = ""{0}"" connectionmode = manual", profileName);
                                cmd.StandardInput.WriteLine(command);
                                cmd.StandardInput.Flush();
                                cmd.StandardInput.Close();
                                cmd.WaitForExit();
                                Console.WriteLine(cmd.StandardOutput.ReadToEnd());

                                refreshForm(refresh);
                            }
                        }
                        else if (securityText == "WPA2")
                        {
                            xmlName = @localDirectory + "/wpa.xml";
                            activeForm = true;
                            string key = Prompt.ShowKeyDialog("", "Insert key");
                            activeForm = false;

                            if (key != "" && key.Length >= 8)
                            {

                                //xml format
                                XNamespace empNM = "http://www.microsoft.com/networking/WLAN/profile/v1";
                                doc = new XDocument(
                                    new XElement(empNM + "WLANProfile",
                                                   new XElement("name", "{0}"),
                                                       new XElement("SSIDConfig",
                                                            new XElement("SSID",
                                                                new XElement("name", "{0}"))),
                                                       new XElement("connectionType", "ESS"),
                                                       new XElement("connectionMode", "auto"),
                                                       new XElement("autoSwitch", "false"),
                                                       new XElement("MSM",
                                                            new XElement("security",
                                                                new XElement("authEncryption",
                                                                     new XElement("authentication", "WPA2PSK"),
                                                                     new XElement("encryption", "AES"),
                                                                     new XElement("useOneX", "false")),
                                                                 new XElement("sharedKey",
                                                                     new XElement("keyType", "passPhrase"),
                                                                     new XElement("protected", "false"),
                                                                     new XElement("keyMaterial", "{1}"))))));

                                //delete all the xmlns="" in the code
                                foreach (var node in doc.Root.Descendants()
                                        .Where(n => n.Name.NamespaceName == ""))
                                {
                                    node.Attributes("xmlns").Remove();
                                    node.Name = node.Parent.Name.Namespace + node.Name.LocalName;
                                }

                                doc.Save(xmlName);


                                string xmlcontents = XDocument.Load(xmlName).ToString();
                                //Add xml declaration at the beginning
                                string profileXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" + string.Format(xmlcontents, profileName, key);
                                Console.Write(profileXml);
                                wlanIface.SetProfile(Wlan.WlanProfileFlags.AllUser, profileXml, true);
                                wlanIface.Connect(Wlan.WlanConnectionMode.Profile, Wlan.Dot11BssType.Any, profileName);
                                File.Delete(xmlName);

                                Process cmd = new Process();
                                cmd.StartInfo.FileName = "cmd.exe";
                                cmd.StartInfo.RedirectStandardInput = true;
                                cmd.StartInfo.RedirectStandardOutput = true;
                                cmd.StartInfo.CreateNoWindow = true;
                                cmd.StartInfo.UseShellExecute = false;
                                cmd.Start();

                                string command = String.Format(@"netsh wlan set profileparameter name = ""{0}"" connectionmode = manual", profileName);
                                cmd.StandardInput.WriteLine(command);
                                cmd.StandardInput.Flush();
                                cmd.StandardInput.Close();
                                cmd.WaitForExit();
                                Console.WriteLine(cmd.StandardOutput.ReadToEnd());

                                refreshForm(refresh);
                            }
                        }
                        else if (securityText == "Unknown") { }
                        else { }

                    }


                }
            }


        }

        void savePriority(string ssid, string priority)
        {
            string path = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            var directory = System.IO.Path.GetDirectoryName(path);
            var localDirectory = new Uri(directory).LocalPath;
            string xmlName = @localDirectory + "/priorities.xml";

            XDocument doc;
            try
            {
                // Use the file
                doc = XDocument.Load(xmlName);
                try
                {
                    doc.Elements().Elements(ssid).Remove();
                    doc.Root.Add(new XElement(ssid, priority));
                }
                catch (Exception)
                {
                    doc.Root.Add(new XElement(ssid, priority));
                }
            }
            catch (FileNotFoundException)
            {
                // File does not exist
                doc = new XDocument(
                    new XElement("Priorities",
                                   new XElement(ssid, priority)));
            }

            doc.Save(xmlName);
        }
        void priorityComboBox_SelectedIndexChanged(object sender,System.EventArgs e,String theinterfaceName)
        {
            ComboBox priorityComboBox = (ComboBox)sender;
            Form form = priorityComboBox.FindForm();
            Control thresholdControl = form.GetNextControl(priorityComboBox, false);
            Control ssidControl = form.GetNextControl(thresholdControl, false);
            ssidControl = form.GetNextControl(ssidControl, false);
            ssidControl = form.GetNextControl(ssidControl, false);
            ssidControl = form.GetNextControl(ssidControl, false);
            string nameSSID = "";

            if (ssidControl != null)
                nameSSID = ssidControl.Text;
            
            string priorityNumber = priorityComboBox.Text;
            
            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();
            
            string command = String.Format(@"netsh wlan set profileorder name = ""{0}"" interface=""{1}"" priority={2}",nameSSID, theinterfaceName, priorityNumber);
            cmd.StandardInput.WriteLine(command);
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            cmd.WaitForExit();
            Console.WriteLine(cmd.StandardOutput.ReadToEnd());
            savePriority(ssidControl.Text, priorityComboBox.Text);

        }
        void saveThreshold(string ssid, int threshold)
        {
            string path = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            var directory = System.IO.Path.GetDirectoryName(path);
            var localDirectory = new Uri(directory).LocalPath;
            string xmlName = @localDirectory + "/thresholds.xml";
            
            XDocument doc;
            try
            {
                // Use the file
                doc = XDocument.Load(xmlName);
                try
                {
                    doc.Elements().Elements(ssid).Remove();
                    doc.Root.Add(new XElement(ssid, threshold));
                }
                catch (Exception)
                {

                    doc.Root.Add(new XElement(ssid, threshold));
                }
            }
            catch (FileNotFoundException)
            {
                // File does not exist
                doc = new XDocument(
                    new XElement("Thresholds",
                                   new XElement(ssid, threshold)));
            }

            doc.Save(xmlName);
        }
        void thresholdTextBox_ValueChanged(object sender, System.EventArgs e)
        {
            NumericUpDown thresholdUpDown = (NumericUpDown)sender;

            Form form = thresholdUpDown.FindForm();
            InitializeComponent();
            Control ssidControl = form.GetNextControl(thresholdUpDown, false);
            ssidControl = form.GetNextControl(ssidControl, false);
            ssidControl = form.GetNextControl(ssidControl, false);
            ssidControl = form.GetNextControl(ssidControl, false);
            int threshold = Convert.ToInt16(thresholdUpDown.Text);
            saveThreshold(ssidControl.Text, threshold);

        }
        public class ReverseComparer : IComparer
        {
            // Call CaseInsensitiveComparer.Compare with the parameters reversed.
            public int Compare(Object x, Object y)
            {
                return (new CaseInsensitiveComparer()).Compare(y, x);
            }

        }

        public static class Prompt
        {
            public static string ShowKeyDialog(string text, string caption)
            {

                Form prompt = new Form()
                {
                    Width = 300,
                    Height = 100,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    Text = caption,
                    StartPosition = FormStartPosition.CenterScreen
                };
                Label textLabel = new Label() { Left = 100, Top = 20, Text = text };
                TextBox textBox = new TextBox() { Left = 50, Top = 10, Width = 150 };
                textBox.PasswordChar = '*';
                Button confirmationButton = new Button() { Text = "Ok", Left = 100, Width = 50, Top = 35, DialogResult = DialogResult.OK };
                confirmationButton.Click += (sender, e) => { prompt.Close(); };


                prompt.Controls.Add(textBox);
                prompt.Controls.Add(confirmationButton);
                prompt.Controls.Add(textLabel);
                prompt.AcceptButton = confirmationButton;

                return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
            }

            public static string ShowDeleteDialog(string text, string caption, string ssid)
            {

                Form promptDelete = new Form()
                {
                    Width = 300,
                    Height = 80,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    Text = caption,
                    StartPosition = FormStartPosition.CenterScreen
                };
                Label textLabel = new Label() { Left = 100, Top = 20, Text = text };

                Button confirmationButton = new Button() { Text = "Forget", Left = 100, Width = 50, Top = 10, DialogResult = DialogResult.OK };
                confirmationButton.Click += (sender, e) =>
                {
                    WlanClient client = new WlanClient();
                    foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
                    {
                        wlanIface.DeleteProfile(ssid);
                    }
                    promptDelete.Close();
                };



                promptDelete.Controls.Add(confirmationButton);
                promptDelete.Controls.Add(textLabel);
                promptDelete.AcceptButton = confirmationButton;

                return promptDelete.ShowDialog() == DialogResult.OK ? textLabel.Text : "";
            }





            public static string ShowKeyDeleteDialog(string text, string caption, string ssid)
            {

                Form prompt = new Form()
                {
                    Width = 300,
                    Height = 100,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    Text = caption,
                    StartPosition = FormStartPosition.CenterScreen
                };

                Button confirmationButton = new Button() { Text = "Connect", Left = 100, Width = 70, Top = 20, DialogResult = DialogResult.OK };
                confirmationButton.Click += (sender, e) => {
                   
                    Process cmd = new Process();
                    cmd.StartInfo.FileName = "cmd.exe";
                    cmd.StartInfo.RedirectStandardInput = true;
                    cmd.StartInfo.RedirectStandardOutput = true;
                    cmd.StartInfo.CreateNoWindow = true;
                    cmd.StartInfo.UseShellExecute = false;
                    cmd.Start();

                    string command = String.Format(@"netsh wlan connect name=""{0}""", ssid);
                    cmd.StandardInput.WriteLine(command);
                    cmd.StandardInput.Flush();
                    cmd.StandardInput.Close();
                    cmd.WaitForExit();
                    Console.WriteLine(cmd.StandardOutput.ReadToEnd());
                    prompt.Close();
                    
                };
                Button forgetButton = new Button() { Text = "Forget", Left = 200, Width = 50, Top = 20, DialogResult = DialogResult.OK };
                forgetButton.Click += (sender, e) =>
                {
                    WlanClient client = new WlanClient();
                    foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
                    {
                        wlanIface.DeleteProfile(ssid);
                    }
                    prompt.Close();
                };

                prompt.Controls.Add(confirmationButton);
                prompt.Controls.Add(forgetButton);
                prompt.AcceptButton = confirmationButton;

                return prompt.ShowDialog() == DialogResult.OK ? confirmationButton.Text : "";
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}


