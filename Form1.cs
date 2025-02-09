﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace yazlap2_1
{
    public partial class PuzzleForm : Form
    {
        Image img;
        List<string> highScoreNames = new List<string>();
        List<float> highScores = new List<float>();
        float score = 0;
        int count = 0;

        string path = @"C:\Users\NECOO\Desktop\puzzle\1.jpg";
        public PuzzleForm()
        {
            InitializeComponent();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            LoadHignScores();
            LoadImage();
            UpdateScore();
        }

        public void UpdateScore()
        {
            Text = "Puzzle (Score = " + score + ")";
        }
        public void SaveScore(string name)
        {
            bool saved = false;
            for (int i = 0; i < highScores.Count; i++)
            {
                if (score > highScores[i])
                {
                    saved = true;
                    highScores.Insert(i, score);
                    highScoreNames.Insert(i, name);
                    break;
                }
            }
            if (!saved)
            {
                highScores.Add(score);
                highScoreNames.Add(name);
            }
            using (TextWriter t = File.CreateText("scores.txt"))
            {
                for (int i = 0; i < highScores.Count; i++)
                {
                    t.WriteLine(highScoreNames[i] + ":" + highScores[i]);
                }
            }
            LoadHignScores();
        }

        public void LoadHignScores()
        {
            listBox1.Items.Clear();
            if (!File.Exists("scores.txt"))
                return;
            int i = 1;
            foreach (var line in File.ReadAllLines("scores.txt"))
            {
                var vars = line.Split(new char[] { ':' });
                if (vars.Length == 2)
                {
                    if (float.TryParse(vars[1], out float score))
                    {
                        highScoreNames.Add(vars[0]);
                        highScores.Add(score);
                        listBox1.Items.Add("#" + (i++) + " | " + score + " | " + vars[0]);
                    }
                }
            }
        }
        public float GetFactor(int w, int h)
        {
            float W = panel1.Width - offset.X * 2;
            float H = panel1.Height - offset.Y * 2;
            if (w / (float)h > W / H)
            {
                return W / (4f * w);
            }
            else
            {
                return H / (4f * h);
            }
        }
        Point offset = new Point(10, 10);
        void LoadImage()
        {
            panel1.Enabled = true;
            count = 0;
            score = 0;
            panel1.Controls.Clear();
            Image[,] img_correct;

            img = Image.FromFile(path);

            Image[,] img_parts = new Image[4, 4];
            int ext_x = img.Width % 4;
            int ext_y = img.Height % 4;
            int btn_w = img.Width / 4;
            int btn_h = img.Height / 4;


            float factor = GetFactor(btn_w, btn_h);

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    img_parts[i, j] = new Bitmap(btn_w, btn_h);
                    var graphics = Graphics.FromImage(img_parts[i, j]);
                    graphics.DrawImage(img, new Rectangle(0, 0, btn_w, btn_h), new Rectangle(i * btn_w, j * btn_h, btn_w, btn_h), GraphicsUnit.Pixel);
                    graphics.Dispose();

                }
            }

            img_correct = (Image[,])img_parts.Clone();
            Shuffle(img_parts);

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    PuzzleButton btn = new PuzzleButton()
                    {
                        Name = "btn" + i + "-" + j,
                        Location = new Point(offset.X + (int)(i * btn_w * factor), offset.Y + (int)(j * btn_h * factor)),
                        Size = new Size((int)(btn_w * factor), (int)(btn_h * factor)),
                        BackgroundImage = img_parts[i, j],
                        BackgroundImageLayout = ImageLayout.Stretch,
                        CorrectImage = img_correct[i, j],
                        FlatStyle = FlatStyle.Flat
                    };
                    btn.FlatAppearance.BorderSize = 2;
                    btn.FlatAppearance.BorderColor = Color.Wheat;
                    if (CheckValid(btn))
                    {
                        btn.Enabled = false;
                        btn.FlatAppearance.BorderColor = Color.Blue;
                        //puan
                        score += 6.25f;
                        //doğru sayısı
                        count++;
                        if (count == 16)
                        {
                            score = 100;
                            new Form2(this).Show();
                            this.Enabled = false;
                            MessageBox.Show("İnanılmazz");
                        }
                    }

                    UpdateScore();
                    btn.MouseClick += Btn_MouseClick;
                    panel1.Controls.Add(btn);
                }
            }
            if (count == 0)
            {
                panel1.Enabled = false;
                MessageBox.Show("Hiçbir resim doğru konuma yerleşmedi. Karıştırmayı tekrar dene ya da yeni bir resim seç :)");
            }
        }
        PuzzleButton lastSelection;
        private void Btn_MouseClick(object sender, MouseEventArgs e)
        {

            if (lastSelection == null)
            {
                lastSelection = (PuzzleButton)sender;
                return;
            }
            //exchange
            Image temp = lastSelection.BackgroundImage;
            lastSelection.BackgroundImage = ((PuzzleButton)sender).BackgroundImage;
            ((PuzzleButton)sender).BackgroundImage = temp;
            //score
            bool c1 = CheckValid((PuzzleButton)sender);
            if (c1)
            {
                ((PuzzleButton)sender).Enabled = false;
                ((PuzzleButton)sender).FlatAppearance.BorderColor = Color.Blue;
                count++;
            }
            bool c2 = CheckValid(lastSelection);
            if (c2)
            {
                lastSelection.Enabled = false;
                lastSelection.FlatAppearance.BorderColor = Color.Blue;
                count++;
            }
            if (c1 && c2)
                score += 12.5f;
            else if (c1 || c2)
                score += 6.25f;
            else
            {
                score -= 4;
                if (score < 0)
                    score = 0;
            }

            UpdateScore();
            lastSelection = null;
            if (count == 16)
            {
                new Form2(this).Show();
                this.Enabled = false;
            }

        }

        public static void Shuffle(Image[,] images)
        {
            Random random = new Random();
            int lengthRow = images.GetLength(1);

            for (int i = 0; i < images.Length; i++)
            {
                int i0 = i / lengthRow;
                int i1 = i % lengthRow;

                int j = random.Next(i + 1);
                int j0 = j / lengthRow;
                int j1 = j % lengthRow;

                Image temp = images[i0, i1];
                images[i0, i1] = images[j0, j1];
                images[j0, j1] = temp;
            }
        }

        public bool CheckValid(PuzzleButton button)
        {
            var correct = (Bitmap)button.CorrectImage;

            for (int i = 0; i < correct.Width; i++)
            {
                for (int j = 0; j < correct.Height; j++)
                {
                    if (correct.GetPixel(i, j) != ((Bitmap)button.BackgroundImage).GetPixel(i, j))
                        return false;
                }
            }
            return true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            LoadImage();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                path = openFileDialog1.FileName;
                textBox1.Text = path;
                LoadImage();
            }

        }
    }
}
