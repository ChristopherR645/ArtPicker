using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Unsplasharp;


namespace ArtPicker
{
    public partial class artFinder : Form
    {
        int secs = 0;
        int totalTime = 0;
        int i = 0;
        string currentPictureURL;
        FormWindowState LastWindowState = FormWindowState.Minimized;
        Image retrieved;
        WebClient wc = new WebClient();
        

        public artFinder()
        {
            InitializeComponent();
            

        }

        private void addButton_Click(object sender, EventArgs e)
        {
            addWordToList();
        }

        private void wordBar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)//allows enter to add word to list
            {
                addButton_Click(this, new EventArgs());
                e.Handled = true;
                e.SuppressKeyPress = true; //gets rid of the ding from hitting enter
            }
        }

        private void addWordToList()
        {
            if (!string.IsNullOrWhiteSpace(wordBar.Text))
            {
                wordList.Items.Add(wordBar.Text);
                wordBar.Text = "";
                wordBar.PlaceholderText = "";
            }
            else MessageBox.Show("Add words", "Error");
        }


        private void startButton_Click(object sender, EventArgs e)
        {   //If wordlist is not empty
            if (!(wordList.Items.Count == 0) && timerInput.Value + timerInputSeconds.Value > 0)
            {
                disableProperties();
                displayImage(i);
                timerStart();

            }
            else MessageBox.Show("Add words/time", "Error");

        }

        private void disableProperties()//disables features 
        {
            timerInput.Enabled = false; wordBar.Enabled = false; addButton.Enabled = false; startButton.Enabled = false; deleteButton.Enabled = false; deleteButton.Visible = false;
            label1.Enabled = false; label2.Enabled = false; wordList.Enabled = false; timerInputSeconds.Enabled = false; skipButton.Enabled = true; skipButton.Visible = true;
            linkLabel1.Visible = true;
        }

        private void enableProperties()//enables features 
        {
            timerInput.Enabled = true; wordBar.Enabled = true; addButton.Enabled = true; startButton.Enabled = true; deleteButton.Enabled = true; deleteButton.Visible = true;
            label1.Enabled = true; label2.Enabled = true; wordList.Enabled = true; timerInputSeconds.Enabled = true; skipButton.Enabled = false; skipButton.Visible = false;
            linkLabel1.Visible = false;
        }
        private void timerStart()
        {
            secs = 0;
            totalTime = (int)timerInput.Value * 60 + (int)timerInputSeconds.Value;
            timerDisplay.Visible = true; wordDisplay.Visible = true;
            timerDisplay.Text = totalTime.ToString();
            timer1.Start();

        }


        private void timer1_Tick(object sender, EventArgs e)
        {

            secs++;
            timerDisplay.Text = (totalTime - secs).ToString();
            if (totalTime - secs <= 0)
            {
                timer1.Stop();
                if (i < wordList.Items.Count - 1)
                {
                    i++;
                    displayImage(i);
                    timerStart();
                }
                else
                {
                    enableProperties();
                    reset();
                }
            }
        }
        
        private bool timerRunning()
        {
            if (totalTime - secs <= 0)
            {
                return false;
            }
            else return true;
        }

        private async void testButton_Click(object sender, EventArgs e)
        {
            double test = 614.0 / 2821.0;
            MessageBox.Show(test.ToString());


        }

        
        private void displayImage(int i)
        {
            ArrayList wordListArray = wordListToArray(wordList);
            searchUnsplash((string)wordListArray[i]);
            wordDisplay.Text = "Displaying word: " + wordListArray[i].ToString();
            
        }

        private ArrayList wordListToArray(ListBox wordList)
        {
            ArrayList tempArray = new ArrayList();
            foreach(object item in wordList.Items)
            {
                tempArray.Add(item);
            }
            return tempArray;
        }

        //Retrieves random image from Unsplash using given query and resizes it to fit the picture box 
        private async void searchUnsplash(string query)
        {
            try
            {
                var client = new UnsplasharpClient("S4_WnRvhDNt93pJAgwvNzRk62xhfAr9F99VYlDTcatM");
                var photosFound = await client.SearchPhotos(query);
                int max = photosFound.Count;
                Random random = new Random();
                int rnd = random.Next(max);
                string temp = photosFound[rnd].Urls.Raw; currentPictureURL = photosFound[rnd].Urls.Raw; //+ "&w="+ pictureBox1.Size.Width +"&h=" + pictureBox1.Size.Width;   //+ "&w=540&h=540"

                var request = WebRequest.Create(temp);

                using (var response = request.GetResponse())
                using (var stream = response.GetResponseStream())
                {
                    retrieved = Bitmap.FromStream(stream);
                    
                    if (retrieved.Height >= pictureBox1.Height)
                    {
                        double x = ((double)retrieved.Height / (double)pictureBox1.Height);
                        

                        Image resizedImage =(Image) ResizeImage(retrieved,(int)(retrieved.Width / x), (int)(retrieved.Height / x));
                        pictureBox1.Image = resizedImage;
                    }
                    else if (retrieved.Width >= pictureBox1.Width)
                    {
                        double y = ((double)retrieved.Width / (double)pictureBox1.Width);
                        Image resizedImage = (Image)ResizeImage(retrieved, (int)(retrieved.Width / y), (int)(retrieved.Height / y));
                        pictureBox1.Image = resizedImage;
                    }
                    else
                    {
                        pictureBox1.Image = retrieved;
                    }

                    linkLabel1.LinkVisited = false;

                }
            }catch(Exception e)
            {
                MessageBox.Show(e.Message);
                MessageBox.Show("No image with query: " + query);
                timer1.Stop();
                i++;
                displayImage(i);
                timerStart();
            }
            
        }

        private void reset()
        {
            secs = 0;
            totalTime = 0;
            i = 0;
            pictureBox1.Image = null;
            wordList.Items.Clear();
            timerDisplay.Visible = false;
            wordDisplay.Visible = false;
            wordDisplay.Text = "";
            linkLabel1.Visible = false;  linkLabel1.LinkVisited = false;

        }

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);
            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            for (int i = wordList.SelectedIndices.Count - 1; i >= 0; i--)
            {
                wordList.Items.RemoveAt(wordList.SelectedIndices[i]);
            }
        }

        private void skipButton_Click(object sender, EventArgs e)
        {
            secs = totalTime;

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel1.LinkVisited = true;
            Process.Start(new ProcessStartInfo(currentPictureURL) { UseShellExecute = true });
        }

        private void artFinder_Resize(object sender, EventArgs e)
        {
            try
            {
                // When window state changes
                if (WindowState != LastWindowState)
                {
                    LastWindowState = WindowState;


                    if (WindowState == FormWindowState.Maximized)
                    {
                        if (retrieved.Height >= pictureBox1.Height)
                        {
                            double x = ((double)retrieved.Height / (double)pictureBox1.Height);


                            Image resizedImage = (Image)ResizeImage(retrieved, (int)(retrieved.Width / x), (int)(retrieved.Height / x));
                            pictureBox1.Image = resizedImage;
                        }
                        else if (retrieved.Width >= pictureBox1.Width)
                        {
                            double y = ((double)retrieved.Width / (double)pictureBox1.Width);
                            Image resizedImage = (Image)ResizeImage(retrieved, (int)(retrieved.Width / y), (int)(retrieved.Height / y));
                            pictureBox1.Image = resizedImage;
                        }
                        else
                        {
                            pictureBox1.Image = retrieved;
                        }
                        // Maximized!
                    }
                    if (WindowState == FormWindowState.Normal)
                    {
                        if (retrieved.Height >= pictureBox1.Height)
                        {
                            double x = ((double)retrieved.Height / (double)pictureBox1.Height);


                            Image resizedImage = (Image)ResizeImage(retrieved, (int)(retrieved.Width / x), (int)(retrieved.Height / x));
                            pictureBox1.Image = resizedImage;
                        }
                        else if (retrieved.Width >= pictureBox1.Width)
                        {
                            double y = ((double)retrieved.Width / (double)pictureBox1.Width);
                            Image resizedImage = (Image)ResizeImage(retrieved, (int)(retrieved.Width / y), (int)(retrieved.Height / y));
                            pictureBox1.Image = resizedImage;
                        }
                        else
                        {
                            pictureBox1.Image = retrieved;
                        }
                        // Restored!
                    }
                }
            }
            catch
            {

            }
        }
    }
}
