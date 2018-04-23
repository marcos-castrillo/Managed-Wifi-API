using NativeWifi;
using System;
using System.Text;
using System.Windows.Forms;

namespace WifiExample
{
    class Program
    {
        /// Converts a 802.11 SSID to a string.
        
        //Get speed, incompleted
        //static double GetStringForSpeed(Wlan.WlanRateSet speed)
        //{
        //    ushort[] Rates = new ushort[3];
        //    //Rates=speed.Rates;
        //    //int WlanRateIndex = Rates[0];
        //    return speed.GetRateInMbps(1);
        //}

        [STAThread]
        static void Main( string[] args )
        {
                Application.EnableVisualStyles();
             Application.Run(new Form1()); 
                //// Retrieves XML configurations of existing profiles.
                //// This can assist you in constructing your own XML configuration
                //// (that is, it will give you an example to follow).
                //foreach ( Wlan.WlanProfileInfo profileInfo in wlanIface.GetProfiles() )
                //{
                //    string name = profileInfo.profileName; // this is typically the network's SSID
                //    string xml = wlanIface.GetProfileXml( profileInfo.profileName );
                //}

                //// Connects to a known network with WEP security
                //string profileName = "Cheesecake"; // this is also the SSID
                //string mac = "52544131303235572D454137443638";
                //string key = "hello";
                //string profileXml = string.Format("<?xml version=\"1.0\"?><WLANProfile xmlns=\"http://www.microsoft.com/networking/WLAN/profile/v1\"><name>{0}</name><SSIDConfig><SSID><hex>{1}</hex><name>{0}</name></SSID></SSIDConfig><connectionType>ESS</connectionType><MSM><security><authEncryption><authentication>open</authentication><encryption>WEP</encryption><useOneX>false</useOneX></authEncryption><sharedKey><keyType>networkKey</keyType><protected>false</protected><keyMaterial>{2}</keyMaterial></sharedKey><keyIndex>0</keyIndex></security></MSM></WLANProfile>", profileName, mac, key);
                //wlanIface.SetProfile( Wlan.WlanProfileFlags.AllUser, profileXml, true );
                //wlanIface.Connect( Wlan.WlanConnectionMode.Profile, Wlan.Dot11BssType.Any, profileName );
            }
        }
    }

