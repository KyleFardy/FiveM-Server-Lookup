using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FiveM_Server_Lookup
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public static string resolved_ip_address { get; set; }
        public static string itags { get; set; }
        public static string dynamic { get; set; }
        public static string players { get; set; }
        public static string information { get; set; }
        public static dynamic dynamic_parsed { get; set; }
        public static dynamic players_parsed { get; set; }
        public static dynamic information_parsed { get; set; }
        public static string[] enpoints = { "dynamic.json", "players.json", "info.json" };
        public void clear()
        {
            dataGridView1.Rows.Clear();
            listBoxControl1.Items.Clear();
            listBoxControl2.Items.Clear();
            banner_detail.ImageLocation = null;
            banner_detail.Image = null;
            logo.ImageLocation = null;
            logo.Image = null;
            connect_image.ImageLocation = null;
            connect_image.Image = null;
            groupControl5.Text = "SELECTED PLAYER IDENTIFIERS";
            groupControl3.Text = "SERVER INFORMATION";
            resolved_ip_address = null;
            itags = null;
            dynamic = null;
            players = null;
            information = null;
            dynamic_parsed = null;
            players_parsed = null;
            information_parsed = null;
            resolved_ip_address = null;
            clients_online_count.Text = "Clients Online : <b>0</b>";
            tags.Text = "Tags : <b>None</b>";
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!string.IsNullOrEmpty((information_parsed.vars.DiscordLink).ToString()))
            {
                Process.Start((information_parsed.vars.DiscordLink).ToString());
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!string.IsNullOrEmpty((information_parsed.vars.WebsiteLink).ToString()))
            {
                Process.Start((information_parsed.vars.WebsiteLink).ToString());
            }
        }

        public Image load_image(string base64) => (Bitmap)new ImageConverter().ConvertFrom(Convert.FromBase64String(base64));
        public static bool valid_http_url(string url)
        {
            Regex r = new Regex(@"^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return r.IsMatch(url);
        }
        public bool valid_ip_address(string ip)
        {
            string[] iparr = ip.Split(':');
            string[] arr = iparr[0].Split('.');
            if (arr.Length != 4)
                return false;
            byte obyte = 0;
            foreach (string str in arr)
                if (!byte.TryParse(str, out obyte))
                    return false;
            return true;
        }
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            listBoxControl2.Items.Clear();
            if (dataGridView1.SelectedCells.Count > 0)
            {
                players_parsed = JsonConvert.DeserializeObject(players);
                foreach (var player in players_parsed)
                {
                    foreach (var identifier in player.identifiers)
                    {
                        if (player.name == dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells["PLAYER_NAME"].Value.ToString())
                        {
                            groupControl5.Text = player.name + "'s IDENTIFIERS";
                            listBoxControl2.Items.Add(identifier);
                        }
                        else
                        {
                            groupControl5.Text = "SELECTED PLAYER IDENTIFIERS";
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please Refresh The List To Select A Player!", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            clear();
            try
            {
                if (string.IsNullOrEmpty(connect_url.Text))
                {
                    MessageBox.Show("Please Enter A Connection URL!", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    if (valid_http_url(connect_url.Text))
                    {
                        WebClient MyClient = new WebClient();
                        MyClient.Headers.Add("Content-Type", "application/json");
                        MyClient.Headers.Add("User-Agent", "DIMS /0.1 +" + connect_url.Text);
                        MyClient.UploadString(connect_url.Text, connect_url.Text);
                        WebHeaderCollection myWebHeaderCollection = MyClient.ResponseHeaders;
                        if (string.IsNullOrEmpty(myWebHeaderCollection.Get("x-citizenfx-url")))
                        {
                            MessageBox.Show("We Cant Seem To Check This Server!", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            if (valid_ip_address(myWebHeaderCollection.Get("x-citizenfx-url").Replace("https://", "").Replace("http://", "").Replace("/", "")))
                            {
                                resolved_ip_address = myWebHeaderCollection.Get("x-citizenfx-url");
                                dynamic = MyClient.DownloadString(string.Format("{0}{1}", resolved_ip_address, enpoints[0]));
                                players = MyClient.DownloadString(string.Format("{0}{1}", resolved_ip_address, enpoints[1]));
                                information = MyClient.DownloadString(string.Format("{0}{1}", resolved_ip_address, enpoints[2]));
                                dynamic_parsed = JsonConvert.DeserializeObject(dynamic);
                                players_parsed = JsonConvert.DeserializeObject(players);
                                information_parsed = JsonConvert.DeserializeObject(information);
                                foreach (var player in players_parsed)
                                {
                                    dataGridView1.Rows.Add(player.id, player.name, player.endpoint, player.ping + "ms");
                                }
                                foreach (var resource in information_parsed.resources)
                                {
                                    listBoxControl1.Items.Add(resource);
                                }
                                connect_image.ImageLocation = information_parsed.vars.banner_connecting;
                                banner_detail.ImageLocation = information_parsed.vars.banner_detail;
                                logo.Image = load_image((information_parsed.icon).ToString());
                                groupControl3.Text = dynamic_parsed.hostname;
                                clients_online_count.Text = "Clients Online : <b>" + dynamic_parsed.clients + "</b>";
                                tags.Text = "Tags : <b>" + information_parsed.vars.tags + "</b>";
                            }
                            else
                            {
                                MessageBox.Show("Failed To Parse The Servers IP Address!", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please Enter A Valid Connection URL!, Ex : https://cfx.re/join/z4opbd", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
