using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Text;

namespace AR_Clothing_GUI
{
    public partial class Form1 : Form
    {
        string[] tShirtImages = { "blackTshirt.PNG", "whiteTshirt.PNG","blueTshirt.PNG","greenTshirt.PNG" ,"redTshirt.PNG" };
        string[] hoodieImages = { "blueHoodie.PNG", "yellowHoodie.PNG", "greenHoodie.PNG" };
        string[] pantsImages = { "biegePants.PNG", "brownPants.PNG" };
        string[] longSleeveImages = { "blackLongSleeve.PNG", "whiteLongSleeve.PNG" };

        int currentColorIndex = 0;
        string[] currentImages;
        int tShirtColorIndex = 0;
        int hoodieColorIndex = 0;
        int pantsColorIndex = 0;
        int longSleeveColorIndex = 0;
        public Form1()
        {
            InitializeComponent();

            radioButton1.CheckedChanged += new EventHandler(radioButton_CheckedChanged);
            radioButton2.CheckedChanged += new EventHandler(radioButton_CheckedChanged);
            radioButton3.CheckedChanged += new EventHandler(radioButton_CheckedChanged);
            radioButton4.CheckedChanged += new EventHandler(radioButton_CheckedChanged);

            currentImages = tShirtImages;
            DisplayCurrentImage();
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
        }
        private void SendClothingSelection(string data)
        {
            try
            {   
                // tcp client used for the server connection(sends data for the clothes)
                using (TcpClient client = new TcpClient("127.0.0.1", 12347))
                {
                    NetworkStream stream = client.GetStream();
                    byte[] message = Encoding.ASCII.GetBytes(data);
                    stream.Write(message, 0, message.Length);
                    stream.Flush();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error sending data: " + ex.Message);
            }
        }
        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
        private void button2_Click(object sender, EventArgs e) => ChangeImage(-1);
        private void button1_Click(object sender, EventArgs e) => ChangeImage(1);
        private void ChangeImage(int direction)

        {

            currentColorIndex = (currentColorIndex + direction + currentImages.Length) % currentImages.Length;

            SaveCurrentColorIndex();

            string selectedColor = currentImages[currentColorIndex];

            string data = GetSelectedClothingType() + ";" + selectedColor;


            DisplayCurrentImage();

            SendClothingSelection(data);

            SendCommandToTuiO("NEXT"); // Send a command to indicate the next item

        }
        private void SendCommandToTuiO(string command)

        {

            try

            {

                using (TcpClient client = new TcpClient("127.0.0.1", 12347)) // Change to the new port

                {

                    NetworkStream stream = client.GetStream();

                    byte[] message = Encoding.ASCII.GetBytes(command);

                    stream.Write(message, 0, message.Length);

                    stream.Flush();

                }

            }

            catch (Exception ex)

            {

                MessageBox.Show("Error sending command to TUIO: " + ex.Message);

            }

        }
        private string GetSelectedClothingType()
        {
            if (radioButton1.Checked)
                return "TShirt";
            else if (radioButton2.Checked)
                return "Hoodie";
            else if (radioButton3.Checked)
                return "Pants";
            else if (radioButton4.Checked)
                return "LongSleeve";

            return "";
        }
        private void UpdateCurrentImages(RadioButton rb)
        {
            switch (rb.Text)
            {
                case "T-Shirts":
                    currentImages = tShirtImages;
                    currentColorIndex = tShirtColorIndex;
                    break;
                case "Hoodies":
                    currentImages = hoodieImages;
                    currentColorIndex = hoodieColorIndex;
                    break;
                case "Pants":
                    currentImages = pantsImages;
                    currentColorIndex = pantsColorIndex;
                    break;
                case "Long Sleeves":
                    currentImages = longSleeveImages;
                    currentColorIndex = longSleeveColorIndex;
                    break;
                default:
                    currentImages = tShirtImages; 
                    currentColorIndex = 0;
                    break;
            }
        }
        private void radioButton_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb.Checked)
            {
                UpdateCurrentImages(rb); 
                string selectedColor = currentImages[currentColorIndex];
                string data = GetSelectedClothingType() + ";" + selectedColor;

                DisplayCurrentImage(); 
                SendClothingSelection(data); 
            }
        }
        private void SaveCurrentColorIndex()
        {
           
            if (radioButton1.Checked)
                tShirtColorIndex = currentColorIndex;
            else if (radioButton2.Checked)
                hoodieColorIndex = currentColorIndex;
            else if (radioButton3.Checked)
                pantsColorIndex = currentColorIndex;
            else if (radioButton4.Checked)
                longSleeveColorIndex = currentColorIndex;
        }
        private void DisplayCurrentImage()
        {
            if (System.IO.File.Exists(currentImages[currentColorIndex]))
            {
                pictureBox1.ImageLocation = currentImages[currentColorIndex];
            }
            else
            {
                MessageBox.Show("Image not found: " + currentImages[currentColorIndex]);
            }
        }   
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
