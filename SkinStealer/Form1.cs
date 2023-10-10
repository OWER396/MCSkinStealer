using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace SkinStealer
{
    public partial class Form1 : Form
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly string skinsdir = AppDomain.CurrentDomain.BaseDirectory+"\\skins";

        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            if(!Directory.Exists(skinsdir))
            {
                Directory.CreateDirectory(skinsdir);
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            username.Enabled = false;
            button1.Text = "Stealing...";
            //get profile id
            var responseString = await client.GetStringAsync("https://api.mojang.com/users/profiles/minecraft/" + username.Text);
            JObject joResponse = JObject.Parse(responseString);
            string id = joResponse["id"].ToString();

            //get the base64 value and decode it
            responseString = await client.GetStringAsync("https://sessionserver.mojang.com/session/minecraft/profile/" + id);
            joResponse = JObject.Parse(responseString);
            JArray array = (JArray)joResponse["properties"];
            string properties = array[0].ToString();
            joResponse = JObject.Parse(properties);
            string b64 = joResponse["value"].ToString();
            string b64d = Encoding.UTF8.GetString(Convert.FromBase64String(b64));
            joResponse = JObject.Parse(b64d);

            //get the skin url and download it
            JObject ojObject = (JObject)joResponse["textures"];
            joResponse = JObject.Parse(ojObject.ToString());
            ojObject = (JObject)joResponse["SKIN"];
            joResponse = JObject.Parse(ojObject.ToString());
            string skin = joResponse["url"].ToString();
            string filename = skinsdir + "\\"+username.Text+".png";

            if (!File.Exists(filename))
            {
                using (var stream = await client.GetStreamAsync(skin))
                {
                    using (var fileStream = new FileStream(filename, FileMode.CreateNew))
                    {
                        await stream.CopyToAsync(fileStream);
                    }
                }

                MessageBox.Show("Skin successfully stolen!");
            }
            else MessageBox.Show("A file with the same username already exists!");

            button1.Text = "Steal";
            button1.Enabled = true;
            username.Enabled = true;
        }

    }
}
