using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;
using System.Windows.Input;
using AForge.Imaging.Filters;



struct point{
   public int x;
   public int y;
}
namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        bool canny = false;
        int sch = 0;
        LinkedList<point> car = new LinkedList<point>();
        LinkedList<point> listOfPoints = new LinkedList<point>();
        LinkedList<poligons> listPoligon = new LinkedList<poligons>();
        LinkedList<poligons> afterDecode = new LinkedList<poligons>();
        LinkedList<poligons> rezult;
        double brightRift;
        double maxBright;
        double minBright;
        int[,] contour;
        int contourC;
        int[,] rezCanny;
        string[ ]  codeIm = new string[65536];        
        poligons curr = new poligons();
        double[,] PIX_BR;
        byte[,] COMPRESSED;
        int maxFragmentation;
        Undo undoClass = new Undo(null, null);
        Boolean pic = false, pic2 = false;
        Bitmap field = new Bitmap(256,256);
        int count = 0;
        int rezultPoligons = 1;
        int curElement = 0;
        int blocked = 0;
        int nextFree;
        int[] rezMass = new int[10000000];
        Graphics g;
        public Image Crop(Image image, Rectangle selection)
        {
            Bitmap bmp = image as Bitmap;

            // Check if it is a bitmap:
            if (bmp == null)
                throw new ArgumentException("No valid bitmap");

            // Crop the image:
            Bitmap cropBmp = bmp.Clone(selection, bmp.PixelFormat);

            // Release the resources:
            image.Dispose();

            return cropBmp;
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            if (ofd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            PIX_BR = new double[256, 256];
            int m = 0, n = 0;
            Image imIn = Image.FromFile(ofd.FileName);
            Bitmap inBmp = new Bitmap(imIn);
            Bitmap bmpCrop = new Bitmap(256, 256);
            for (int i = 0; i < bmpCrop.Width; i++)
                for (int j = 0; j < bmpCrop.Height; j++)
                    bmpCrop.SetPixel(i, j, inBmp.GetPixel(i, j));
            // Assign the cursor in the Stream to the Form's Cursor property.
            Image im = bmpCrop;
            //im = Crop(im, new Rectangle(0, 0, 256, 256));
            pictureBox1.Image = im;
            Bitmap bmp = new Bitmap(im);
            //Color pixel;
            PixelFormat pxf = PixelFormat.Format24bppRgb;




            // Получаем данные картинки.
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            //Блокируем набор данных изображения в памяти
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, pxf);

            // Получаем адрес первой линии.
            IntPtr ptr = bmpData.Scan0;
            //На 3 умножаем - поскольку RGB цвет кодируется 3-мя байтами

            int numBytes = bmpData.Stride * bmp.Height;
            int widthBytes = bmpData.Stride;
            byte[] rgbValues = new byte[numBytes];

            // Копируем значения в массив.
            Marshal.Copy(ptr, rgbValues, 0, numBytes);

            // Перебираем пиксели по 3 байта на каждый и меняем значения
            for (int counter = 0; counter < rgbValues.Length; counter += 3)
            {
                byte color_b = 0;
                double value = rgbValues[counter] * 0.3 + rgbValues[counter + 1] * 0.59 + rgbValues[counter + 2] * 0.11;
                color_b = Convert.ToByte(value);
                if (m == 256)
                {
                    n++;
                    m = 0;
                }
                PIX_BR[m, n] = value;
                m++;


                rgbValues[counter] = color_b;
                rgbValues[counter + 1] = color_b;
                rgbValues[counter + 2] = color_b;

            }
            // Копируем набор данных обратно в изображение
            Marshal.Copy(rgbValues, 0, ptr, numBytes);

            // Разблокируем набор данных изображения в памяти.
            bmp.UnlockBits(bmpData);
            pictureBox1.Image = bmp;
            pic = true;
            count++;
            undoClass.add(pictureBox1.Image, pictureBox2.Image, true, false);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            {
                if (pictureBox2.Image != null)
                {
                    SaveFileDialog save = new SaveFileDialog();
                    Stream str;
                    save.Filter = "Image files (*.jpeg)| *.jpeg | Bitmap files (*.bmp)| *.bmp | Images (*.png)| *.png | Gif files (*.gif)| *.gif ";
                    save.RestoreDirectory = true;
                    if (save.ShowDialog() == DialogResult.OK)
                    {
                        if ((str = save.OpenFile()) != null)
                        {
                            MessageBox.Show(save.FileName);
                            try
                            {
                                switch (save.FilterIndex)
                                {
                                    case 0: pictureBox2.Image.Save(str, ImageFormat.Jpeg); break;
                                    case 1: pictureBox2.Image.Save(str, ImageFormat.Bmp); break;
                                    case 2: pictureBox2.Image.Save(str, ImageFormat.Png); break;
                                    case 3: pictureBox2.Image.Save(str, ImageFormat.Gif); break;
                                    default: MessageBox.Show("Неправильный формат файла"); break;
                                }
                            }
                            catch (Exception sd)
                            {
                                MessageBox.Show(sd.Message);
                            }
                            str.Close();
                        }
                    }
                }
                else MessageBox.Show("Нечего сохранять");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if ((pic) && (count > 0))
            {
                pictureBox2.Image = null;
                pictureBox3.Image = null;
                if (pic && pic2)
                {
                    undoClass.add(pictureBox1.Image, pictureBox2.Image, true, true);
                }
                else
                {
                    undoClass.add(pictureBox1.Image, true, false);
                }

                count++;
                pic = true;
                pic2 = false;
            }
            else
            {
                MessageBox.Show("Error!");
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (Undo.i >= 1)
            {
                Boolean pic1 = undoClass.getLast().pic1;
                Boolean pic2 = undoClass.getLast().pic2;
                undoClass.Limg.RemoveLast();
                Undo.i--;
                count--;
                if (pic1 && pic2)
                {
                    pictureBox1.Image = undoClass.getLast().im1;
                    pictureBox2.Image = undoClass.getLast().im2;
                }
                else
                {
                    if (pic1)
                    {
                        pictureBox1.Image = undoClass.getLast().im1;
                    }
                    if (pic2)
                    {
                        pictureBox2.Image = undoClass.getLast().im2;
                    }
                }

            }
            else
                MessageBox.Show("Ошибка !!!", "Fatal error");

        }






        public void readBRight()
        {
            rezult = new LinkedList<poligons>();
            for (int i = 0; i < 10000000; i++)
                rezMass[i] = -1;
            g = pictureBox3.CreateGraphics();
            try
            {
                brightRift = Convert.ToDouble(textBox1.Text);
                maxFragmentation = Convert.ToInt32(textBox2.Text);
            } catch(System.FormatException e)
            {
                brightRift = 30;
                maxFragmentation = 8;
            }

            Image im = pictureBox1.Image;
            Bitmap image = new Bitmap(im);
            curr.horizontalSide = 256;
            curr.verticalSide = 256;
            curr.x1 = 0;
            curr.x2 = 255;
            curr.y1 = 0;
            curr.y2 = 255;
            listPoligon.AddLast(curr);
            rezMass[0] = 1;
            rezMass[1] = 0;
            curElement = 2;
            nextFree = 4;
            rezultPoligons = 1;
            while (listPoligon.Count != 0)
                rekursFragmentation();


            return;
        }




        public void rekursFragmentation()
        {
            if (listPoligon.Count != 0)
            {
                maxBright = PIX_BR[listPoligon.First().x1, listPoligon.First().y1];
                minBright = PIX_BR[listPoligon.First().x1, listPoligon.First().y1];
                for (int i = listPoligon.First().x1; i <= listPoligon.First().x2; i++)
                {
                    for (int j = listPoligon.First().y1; j <= listPoligon.First().y2; j++)
                    {
                        if (PIX_BR[i, j] > maxBright)
                            maxBright = PIX_BR[i, j];
                        if (PIX_BR[i, j] < minBright)
                            minBright = PIX_BR[i, j];
                    }
                }
                //MessageBox.Show(Convert.ToString(maxBright - minBright));
                if ((maxBright - minBright > brightRift) && (maxFragmentation <= listPoligon.First().verticalSide / 2))
                {
                    if  ((listPoligon.First().verticalSide != 256)  || (listPoligon.First().horizontalSide != 256))
                    {
                        while (rezMass[curElement] != -1)
                            curElement++;
                        rezMass[curElement] = -nextFree;
                        rezMass[nextFree] = curElement;
                        nextFree += 3;
                    }
                    //MessageBox.Show(Convert.ToString(maxBright - minBright));
                    if (listPoligon.First().horizontalSide < listPoligon.First().verticalSide)
                    {
                        
                        g.DrawLine(new Pen(Brushes.DarkMagenta, 1), new Point(listPoligon.First().x1, (listPoligon.First().y1 + listPoligon.First().y2 )/ 2 ), new Point(listPoligon.First().x2, (listPoligon.First().y1 + listPoligon.First().y2 ) / 2));
                        curr.horizontalSide = listPoligon.First().horizontalSide;
                        curr.verticalSide = listPoligon.First().verticalSide / 2;
                        curr.x1 = listPoligon.First().x1;
                        curr.x2 = listPoligon.First().x2;
                        curr.y1 = listPoligon.First().y1;
                        curr.y2 = listPoligon.First().y2 - listPoligon.First().verticalSide / 2;
                        listPoligon.AddLast(curr);
                        curr.horizontalSide = listPoligon.First().horizontalSide;
                        curr.verticalSide = listPoligon.First().verticalSide / 2;
                        curr.x1 = listPoligon.First().x1;
                        curr.x2 = listPoligon.First().x2;
                        curr.y1 = listPoligon.First().y2 - listPoligon.First().verticalSide / 2 + 1;
                        curr.y2 = listPoligon.First().y2;
                        listPoligon.AddLast(curr);
                        listPoligon.RemoveFirst();
                    }
                    else
                    {
                        g.DrawLine(new Pen(Brushes.DarkMagenta, 1), new Point((listPoligon.First().x1 + listPoligon.First().x2 ) / 2, listPoligon.First().y1), new Point((listPoligon.First().x1 + listPoligon.First().x2) / 2, listPoligon.First().y2));
                        curr.horizontalSide = listPoligon.First().horizontalSide / 2;
                        curr.verticalSide = listPoligon.First().verticalSide;
                        curr.x1 = listPoligon.First().x1;
                        curr.x2 = listPoligon.First().x2 - listPoligon.First().horizontalSide / 2;
                        curr.y1 = listPoligon.First().y1;
                        curr.y2 = listPoligon.First().y2;
                        listPoligon.AddLast(curr);
                        curr.horizontalSide = listPoligon.First().horizontalSide / 2;
                        curr.verticalSide = listPoligon.First().verticalSide;
                        curr.x1 = listPoligon.First().x2 - listPoligon.First().horizontalSide / 2 + 1;
                        curr.x2 = listPoligon.First().x2;
                        curr.y1 = listPoligon.First().y1;
                        curr.y2 = listPoligon.First().y2;
                        listPoligon.AddLast(curr);
                        listPoligon.RemoveFirst();
                        //rekursFragmentation();
                    }


                }
                else
                {
                    while (rezMass[curElement] != -1)
                        curElement++;
                    rezMass[curElement] = rezultPoligons;
                    rezult.AddLast(listPoligon.First());
                    listPoligon.RemoveFirst();
                    rezultPoligons++;
       
                    //rekursFragmentation();



                }
            }
            else
            {
                return;

            }
            
        

        }
        public void compressedImage()
        {
            //MessageBox.Show(Convert.ToString(rezult.Count));
            double sko = 0;
            COMPRESSED = new byte[256, 256];
            poligons it;
            double st1, st2, st3, st4, sumSt;
            label4.Text = Convert.ToString((double)(65536d/(rezult.Count*10d)));
            while (rezult.Count != 0)
            {

                it = rezult.First();
                rezult.RemoveFirst();
                for (int i = it.y1; i <= it.y2; i++)
                {
                    for (int j = it.x1; j <= it.x2; j++)
                    {
                        st1 = Math.Sqrt(Math.Pow((it.x1 - j), 2) + Math.Pow((it.y1 - i), 2));
                        st2 = Math.Sqrt(Math.Pow((it.x2 - j), 2) + Math.Pow((it.y1 - i), 2));
                        st3 = Math.Sqrt(Math.Pow((it.x1 - j), 2) + Math.Pow((it.y2 - i), 2));
                        st4 = Math.Sqrt(Math.Pow((it.x2 - j), 2) + Math.Pow((it.y2 - i), 2));
                        if ((st1 == 0) || (st2 == 0) || (st3 == 0) || (st4 == 0))
                        {
                            if (st1 == 0)
                            {
                                st1 = 1;
                                st2 = 0;
                                st3 = 0;
                                st4 = 0;
                            }
                            if (st2 == 0)
                            {
                                st1 = 0;
                                st2 = 1;
                                st3 = 0;
                                st4 = 0;
                            }
                            if (st3 == 0)
                            {
                                st1 = 0;
                                st2 = 0;
                                st3 = 1;
                                st4 = 0;
                            }
                            if (st4 == 0)
                            {
                                st1 = 0;
                                st2 = 0;
                                st3 = 0;
                                st4 = 1;
                            }
                        }
                        else
                        {
                            sumSt = st1 + st2 + st3 + st4;
                            st1 = sumSt / st1;
                            st2 = sumSt / st2;
                            st3 = sumSt / st3;
                            st4 = sumSt / st4;
                            sumSt = st1 + st2 + st3 + st4;
                            st1 = st1 / sumSt;
                            st2 = st2 / sumSt;
                            st3 = st3 / sumSt;
                            st4 = st4 / sumSt;
                        }
                                COMPRESSED[j, i] = Convert.ToByte(st1 * PIX_BR[it.x1, it.y1] + st2 * PIX_BR[it.x2, it.y1] + st3 * PIX_BR[it.x1, it.y2] + st4 * PIX_BR[it.x2, it.y2]);
                        
                        
                    }
                }

            }
            if (canny)
            {
                for (int k = 0; k < 256; k++)
                {
                    for (int k1 = 0; k1 < 256; k1++)
                    {
                        if (rezCanny[k1, k] != 0)
                        {
                            COMPRESSED[k1, k] = (byte)rezCanny[k1, k];
                        }
                    }
                }

            }
            int m = 0, n = 0;
                        PixelFormat pxf = PixelFormat.Format24bppRgb;
                        Bitmap bmp = new Bitmap(pictureBox1.Image);
                        Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                        BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, pxf);
                        IntPtr ptr = bmpData.Scan0;
                        int numBytes = bmpData.Stride * bmp.Height;
                        int widthBytes = bmpData.Stride;
                        byte[] rgbValues = new byte[numBytes];
                        Marshal.Copy(ptr, rgbValues, 0, numBytes);
                        for (int counter = 0; counter < rgbValues.Length; counter += 3)
                        {
                            byte color_b = 0;
                            if (m == 256)
                            {
                                n++;
                                m = 0;
                            }
                            byte value = COMPRESSED[m, n];
                            m++;
                            color_b = value;

                            rgbValues[counter] = color_b;
                            rgbValues[counter + 1] = color_b;
                            rgbValues[counter + 2] = color_b;

                        }
                        Marshal.Copy(rgbValues, 0, ptr, numBytes);
                        bmp.UnlockBits(bmpData);
                        pictureBox2.Image = bmp;
            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < 256; j++)
                {
                    sko += Math.Abs(PIX_BR[j, i] - COMPRESSED[j, i]);
                }
            }
            label6.Text = Convert.ToString(Math.Sqrt((sko / (65536))/ 65535));





        }
        private void SobelEdgeDetect(Bitmap original)
        {
            Bitmap b = original;
            int m = 0, n = 0;
            int[,] sobel = new int[256, 256];
            int width = b.Width;
            int height = b.Height;
            int[,] gx = new int[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            int[,] gy = new int[,] { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } };

            int limit = 64;
            try
            {
                limit = Convert.ToInt16(textBox3.Text);
            }
            catch(Exception e)
            {
                limit = 64;
            }

            int new_rx = 0, new_ry = 0;
            for (int i = 1; i < b.Width - 1; i++)
            {
                for (int j = 1; j < b.Height - 1; j++)
                {

                    new_rx = 0;
                    new_ry = 0;

                    for (int wi = -1; wi < 2; wi++)
                    {
                        for (int hw = -1; hw < 2; hw++)
                        {
                            new_rx += gx[wi + 1, hw + 1] * (int)PIX_BR[i + hw, j + wi]; ;
                            new_ry += gy[wi + 1, hw + 1] * (int)PIX_BR[i + hw, j + wi]; ;
                        }
                    }
                    if (new_rx * new_rx + new_ry * new_ry > limit*128)
                        sobel[i, j] = 255;
                    else
                        sobel[i, j] = 0;
                }
            }
            PixelFormat pxf = PixelFormat.Format24bppRgb;
            Bitmap bmp = new Bitmap(pictureBox1.Image);
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, pxf);
            IntPtr ptr = bmpData.Scan0;
            int numBytes = bmpData.Stride * bmp.Height;
            int widthBytes = bmpData.Stride;
            byte[] rgbValues = new byte[numBytes];
            Marshal.Copy(ptr, rgbValues, 0, numBytes);
            for (int counter = 0; counter < rgbValues.Length; counter += 3)
            {
                byte color_b = 0;
                if (m == 256)
                {
                    n++;
                    m = 0;
                }
                byte value = (byte)sobel[m, n];
                m++;
                color_b = value;

                rgbValues[counter] = color_b;
                rgbValues[counter + 1] = color_b;
                rgbValues[counter + 2] = color_b;

            }
            Marshal.Copy(rgbValues, 0, ptr, numBytes);
            bmp.UnlockBits(bmpData);
            pictureBox2.Image = bmp;
            undoClass.add(pictureBox2.Image, false, true);
            count++;
            pic2 = true;
            return;

        }

        private void button8_Click(object sender, EventArgs e)
        {
            int limit = 0;
            try
            {
                limit = Convert.ToInt16(textBox3.Text);
            }
            catch (Exception er)
            {
                limit = 150;
            }
            Bitmap roberts = pictureBox1.Image as Bitmap;
            int m = 256, n = 256;
            int[,] Rob = new int[m, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    int Gx = 0, Gy = 0, G;
                    int zhx = 0, zhy = 0, zhxx = 0, zhyy = 0;
                    if ((j == m - 1) | (i == n - 1))
                    {
                        if ((j == m - 1) & (i == n - 1))
                        {
                            zhx = (byte)PIX_BR[j, i];
                            zhxx = 0;
                            zhy = 0;
                            zhyy = 0;
                        }
                        else
                        {
                            if (j == m - 1)
                            {
                                zhx = (byte)PIX_BR[j, i];
                                zhxx = 0;
                                zhy = 0;
                                zhyy = (byte)PIX_BR[j, i + 1];

                            }
                            if (i == n - 1)
                            {
                                zhx = (byte)PIX_BR[j, i];
                                zhxx = 0;
                                zhy = (byte)PIX_BR[j + 1, i];
                                zhyy = 0;
                            }
                        }
                    }
                    else
                    {
                        zhx = (byte)PIX_BR[j, i];
                        zhxx = (byte)PIX_BR[j + 1, i + 1];
                        zhy = (byte)PIX_BR[j + 1, i];
                        zhyy = (byte)PIX_BR[j, i + 1];

                    }
                    Gx = (zhxx - zhx);
                    Gy = (zhy - zhyy);
                    double GG = Gx * Gx + Gy * Gy;
                    double GGG = Math.Sqrt(GG);
                    G = Convert.ToInt32(GGG);
                    if (G > 255)
                    {
                        G = 255;
                    }               
                        Rob[j, i] = G;
                }
            }
            for (int i = 0; i < 256; i++)
                for (int j = 0; j < 256; j++)
                {
                    Rob[j, i] = Rob[j, i] > limit/4 ? 255 : 0;
                }
            m = 0;
            n = 0;
            PixelFormat pxf = PixelFormat.Format24bppRgb;
            Bitmap bmp = new Bitmap(pictureBox1.Image);
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, pxf);
            IntPtr ptr = bmpData.Scan0;
            int numBytes = bmpData.Stride * bmp.Height;
            int widthBytes = bmpData.Stride;
            byte[] rgbValues = new byte[numBytes];
            Marshal.Copy(ptr, rgbValues, 0, numBytes);
            for (int counter = 0; counter < rgbValues.Length; counter += 3)
            {
                byte color_b = 0;
                if (m == 256)
                {
                    n++;
                    m = 0;
                }
                byte value = (byte)Rob[m, n];
                m++;
                color_b = value;

                rgbValues[counter] = color_b;
                rgbValues[counter + 1] = color_b;
                rgbValues[counter + 2] = color_b;

            }
            Marshal.Copy(rgbValues, 0, ptr, numBytes);
            bmp.UnlockBits(bmpData);
            pictureBox2.Image = bmp;
            undoClass.add(pictureBox2.Image, false, true);
            count++;
            pic2 = true;
        }
        

        private void button9_Click(object sender, EventArgs e)
        {
            Bitmap sobel = pictureBox1.Image as Bitmap;
            SobelEdgeDetect(sobel);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            int limit = 0;
            try
            {
                limit = Convert.ToInt16(textBox3.Text);
            }
            catch (Exception er)
            {
                limit = 150;
            }
            int m = 0, n = 0;
            int[,] previtta = new int[256, 256];
            int[,] gx = new int[,] { { -1, 0, 1 }, { -1, 0, 1 }, { -1, 0, 1 } };
            int[,] gy = new int[,] { { 1, 1, 1 }, { 0, 0, 0 }, { -1, -1, -1 } };

            

            int new_rx = 0, new_ry = 0;
            for (int i = 1; i < 255; i++)
            {
                for (int j = 1; j < 255; j++)
                {

                    new_rx = 0;
                    new_ry = 0;

                    for (int wi = -1; wi < 2; wi++)
                    {
                        for (int hw = -1; hw < 2; hw++)
                        {
                            new_rx += gx[wi + 1, hw + 1] * (int)PIX_BR[i + hw, j + wi]; ;
                            new_ry += gy[wi + 1, hw + 1] * (int)PIX_BR[i + hw, j + wi]; ;
                        }
                    }
                    if (new_rx * new_rx + new_ry * new_ry > limit * 48)
                        previtta[i, j] = 255;
                    else
                        previtta[i, j] = 0;
                }
            }
            PixelFormat pxf = PixelFormat.Format24bppRgb;
            Bitmap bmp = new Bitmap(pictureBox1.Image);
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, pxf);
            IntPtr ptr = bmpData.Scan0;
            int numBytes = bmpData.Stride * bmp.Height;
            int widthBytes = bmpData.Stride;
            byte[] rgbValues = new byte[numBytes];
            Marshal.Copy(ptr, rgbValues, 0, numBytes);
            for (int counter = 0; counter < rgbValues.Length; counter += 3)
            {
                byte color_b = 0;
                if (m == 256)
                {
                    n++;
                    m = 0;
                }
                byte value = (byte)previtta[m, n];
                m++;
                color_b = value;

                rgbValues[counter] = color_b;
                rgbValues[counter + 1] = color_b;
                rgbValues[counter + 2] = color_b;
            }
            Marshal.Copy(rgbValues, 0, ptr, numBytes);
            bmp.UnlockBits(bmpData);
            pictureBox2.Image = bmp;
            undoClass.add(pictureBox2.Image, false, true);
            count++;
            pic2 = true;
            return;
        }
        /*public void cannyFilter()
        {
            double[,] gradient = new double[252, 252];
            double[,] blur = new double[256, 256];
            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < 256; j++)
                {
                    blur[j,i] = PIX_BR[j,i];
                }
            }
            for (int i = 2; i < 254; i++)
            {
                for (int j = 2; j < 254; j++)
                {
                    blur[j, i] = 15 * PIX_BR[j, i];
                    blur[j, i] += 4 * (PIX_BR[j - 2, i - 2] + PIX_BR[j - 2, i + 2] + PIX_BR[j + 2, i - 2] + PIX_BR[j + 2, i + 2]);
                    blur[j, i] += 5 * (PIX_BR[j - 2, i ] + PIX_BR[j + 2, i ] + PIX_BR[j, i - 2] + PIX_BR[j, i + 2]);
                    blur[j, i] += 4 * (PIX_BR[j - 2, i - 1] + PIX_BR[j - 2, i + 1] + PIX_BR[j + 2, i - 1] + PIX_BR[j + 2, i + 1] + PIX_BR[j - 1, i - 2] + PIX_BR[j - 1, i + 2] + PIX_BR[j + 1, i - 2] + PIX_BR[j + 1, i + 2]);
                    blur[j, i] += 9 * (PIX_BR[j - 1, i - 1] + PIX_BR[j - 1, i + 1] + PIX_BR[j + 1, i - 1] + PIX_BR[j + 1, i + 1]);
                    blur[j, i] += 12 * (PIX_BR[j - 1, i] + PIX_BR[j + 1, i] + PIX_BR[j, i - 1] + PIX_BR[j, i + 1]) + PIX_BR[j, i];
                    blur[j, i] /= 159;
                    
                }
            }
            int m = 0, n = 0;
            int[,] sobelX = new int[256, 256];
            int[,] sobelY = new int[256, 256];
            double[,] sobel = new double[256, 256];
            double[,] sobeli = new double[256, 256];
            int[,] gx = new int[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            int[,] gy = new int[,] { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } };

            int limit = 128 * 32;

            int new_rx = 0, new_ry = 0;
            for (int i = 1; i < 255; i++)
            {
                for (int j = 1; j < 255; j++)
                {

                    new_rx = 0;
                    new_ry = 0;

                    for (int wi = -1; wi < 2; wi++)
                    {
                        for (int hw = -1; hw < 2; hw++)
                        {
                            new_rx += gx[wi + 1, hw + 1] * (int)PIX_BR[i + hw, j + wi]; 
                            new_ry += gy[wi + 1, hw + 1] * (int)PIX_BR[i + hw, j + wi]; 
                        }
                    }
                    sobelX[i, j] = new_rx;
                    sobelY[i, j] = new_ry;
                    sobeli[i, j] = Math.Sqrt(new_rx * new_rx + new_ry * new_ry);
                }
            }
            for (int i = 0; i < 252; i++)
            {
                for (int j = 0; j < 252; j++)
                { 
                    if ((sobelY[i, j] == 0) || (sobelX[i, j] == 0))
                    {
                        if ((sobelY[i, j] == 0) && (sobelX[i, j] == 0))
                        {
                            gradient[i, j] = 0;
                        }
                        else
                            gradient[i, j] = sobelY[i, j] == 0 ? 0 : 100;
                    }
                    else
                    {
                        gradient[i, j] = sobelY[i, j] / sobelX[i, j];

                    }
                    
                }
            }
            for (int i = 1; i < 251; i ++ )
                for ( int j = 1; j < 251; j++)
                {
                    if (Math.Abs(sobeli[i, j]) < 0.414)
                    {
                        if ((gradient[i, j] > gradient[i, j + 1]) && (gradient[i, j] > gradient[i, j - 1]))
                        {
                            sobel[i, j] = 255;
                        }
                    }
                    if (((gradient[i, j]) >= 0.414) && ((gradient[i, j]) < 2.414))
                    {
                        if ((gradient[i, j] > gradient[i + 1, j - 1]) && (gradient[i, j] > gradient[i - 1, j + 1]))
                        {
                            sobel[i, j] = 255;
                        }
                    }
                    if (Math.Abs(gradient[i, j]) >=  2.414)
                    {
                        if ((gradient[i, j] > gradient[i + 1, j]) && (gradient[i, j] > gradient[i - 1, j]))
                        {
                            sobel[i, j] = 255;
                        }
                    }
                    if (((gradient[i, j]) <= 0.414) && ((gradient[i, j]) > 2.414))
                    {
                        if ((gradient[i, j] > gradient[i + 1, j + 1]) && (gradient[i, j] > gradient[i - 1, j - 1]))
                        {
                            sobel[i, j] = 255;
                        }
                    }
                }
            PixelFormat pxf = PixelFormat.Format24bppRgb;
            Bitmap bmp = new Bitmap(pictureBox1.Image);
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, pxf);

            // Получаем адрес первой линии.
            IntPtr ptr = bmpData.Scan0;

            // Задаём массив из Byte и помещаем в него надор данных.
            // int numBytes = bmp.Width * bmp.Height * 3; 
            //На 3 умножаем - поскольку RGB цвет кодируется 3-мя байтами
            //Либо используем вместо Width - Stride
            int numBytes = bmpData.Stride * bmp.Height;
            int widthBytes = bmpData.Stride;
            byte[] rgbValues = new byte[numBytes];

            // Копируем значения в массив.
            Marshal.Copy(ptr, rgbValues, 0, numBytes);

            // Перебираем пиксели по 3 байта на каждый и меняем значения
            for (int counter = 0; counter < rgbValues.Length; counter += 3)
            {

                //int value = rgbValues[counter] + rgbValues[counter + 1] + rgbValues[counter + 2];
                byte color_b = 0;


                if (m == 256)
                {
                    n++;
                    m = 0;
                }
                byte value = (byte)sobel[m, n];
                m++;
                color_b = value;

                rgbValues[counter] = color_b;
                rgbValues[counter + 1] = color_b;
                rgbValues[counter + 2] = color_b;

            }
            // Копируем набор данных обратно в изображение
            Marshal.Copy(rgbValues, 0, ptr, numBytes);

            // Разблокируем набор данных изображения в памяти.
            bmp.UnlockBits(bmpData);
            pictureBox2.Image = bmp;
            undoClass.add(pictureBox2.Image, false, true);
            count++;
            pic2 = true;
            return;

        }*/

        private void button10_Click(object sender, EventArgs e)
        {
            contour = new int[65536, 2];
            contourC = 0;
            int limith = 0, m =0, n =0;
            int limitl = 0;
            int sizeOfGausMask = 5;
            rezCanny = new int[256, 256];
            try
            {
                sizeOfGausMask = Convert.ToInt16(textBox5.Text);
            }
            catch (Exception er)
            {
                sizeOfGausMask = 5;
            }
            try
            {
                limith = Convert.ToInt16(textBox3.Text);
            }
            catch (Exception er)
            {
                limith = 100;
            }
            try
            {
                limitl = Convert.ToInt16(textBox4.Text);
            }
            catch (Exception er)
            {
                limitl = 80;
            }
            Canny obj = new Canny(pictureBox1.Image as Bitmap, limith / 2, limitl / 2, sizeOfGausMask, 1);
                
            pictureBox2.Image = obj.DisplayImage(obj.EdgeMap);
            PixelFormat pxf = PixelFormat.Format24bppRgb;
            Bitmap bmp = new Bitmap(pictureBox2.Image);
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, pxf);
            IntPtr ptr = bmpData.Scan0;
            int numBytes = bmpData.Stride * bmp.Height;
            int widthBytes = bmpData.Stride;
            byte[] rgbValues = new byte[numBytes];
            Marshal.Copy(ptr, rgbValues, 0, numBytes);
            for (int counter = 0; counter < rgbValues.Length; counter += 3)
            {
                if (rgbValues[counter] > 128)
                {
                    rezCanny[m, n] = (int)PIX_BR[m, n];

                    contour[contourC, 0] = m;
                    contour[contourC, 1] = n;
                    contourC ++;

                }
                    
            
                else
                    rezCanny[m, n] = 0;
                m++;
                if (m == 256)
                {
                    n++;
                    m = 0;
                }  


            }
            Marshal.Copy(rgbValues, 0, ptr, numBytes);
            bmp.UnlockBits(bmpData);
            pictureBox2.Image = bmp;
            undoClass.add(pictureBox2.Image, false, true);
            count++;
            canny = true;
            pic2 = true;
            
            return;
        }

       

        private void button9_MouseHover(object sender, EventArgs e)
        {
            ToolTip t = new ToolTip();
            t.SetToolTip(button9, "Рекомендуемые пределы задания порога: 50 - 500");
        }

        private void button8_MouseLeave(object sender, EventArgs e)
        {
            ToolTip t = new ToolTip();
            t.SetToolTip(button8, "Рекомендуемые пределы задания порога: 50 - 350");
        }

        private void button7_MouseHover(object sender, EventArgs e)
        {
            ToolTip t = new ToolTip();
            t.SetToolTip(button7,"Рекомендуемые пределы задания порога: 50 - 500");
        }

        private void button10_MouseHover(object sender, EventArgs e)
        {
            ToolTip t = new ToolTip();
            t.SetToolTip(button10,"Рекомендуемые пределы задания верхнего/нижнего порога: 50 - 400\nВозможные значения размера маски Гаусса: 3, 5, 7, 9\n(чем больше значение, тем больше размытие)");

        }

        private void button11_Click(object sender, EventArgs e)
        {
            int sumBr = 0, averageValue, curBr = 0;
            int[] massOfBright = new int[65536];
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FileStream file2 = new FileStream(@ofd.FileName, FileMode.Create);
                StreamWriter writer2 = new StreamWriter(file2);
                while (rezMass[curElement] != -1)
                    curElement++;
                for (int i = 0; i < curElement; i++)
                    writer2.WriteLine(Convert.ToString(rezMass[i]));
                writer2.WriteLine("stop");
                for (int i = 0; i < 256; i++)
                    for (int j = 0; j < 256; j++)
                    {
                        writer2.WriteLine(Convert.ToString(COMPRESSED[j,i]));
                    }
                writer2.WriteLine("stop!!!");
                writer2.Close();
            }
            

        }

        private void button12_Click(object sender, EventArgs e)
        {
            LinkedList<poligons> decodeIm = new LinkedList<poligons>();
            OpenFileDialog ofd = new OpenFileDialog();
            poligons pol = new poligons();
            int[,] brightOfDec = new int[256, 256];
            int counteri = 1;
            pol.x1 = 0;
            pol.x2 = 127;
            pol.y1 = 0;
            pol.y2 = 255;
            pol.horizontalSide = 128;
            pol.verticalSide = 256;
            afterDecode.AddLast(pol);
            pol.x1 = 128;
            pol.x2 = 255;
            pol.y1 = 0;
            pol.y2 = 255;
            pol.horizontalSide = 128;
            pol.verticalSide = 256;
            afterDecode.AddLast(pol);
            g = pictureBox3.CreateGraphics();
            String curStr = "";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                g.DrawLine(new Pen(Brushes.DarkMagenta, 1), new Point(127, 0), new Point(127, 256));
                StreamReader reader = new StreamReader(new FileStream(ofd.FileName, FileMode.OpenOrCreate));
                reader.ReadLine();
                reader.ReadLine();
                while ((curStr != "stop"))
                {
                    curStr = reader.ReadLine();
                    if (curStr == "stop")
                        break;
                    if (counteri++ % 3 == 0)
                    {
                        continue;
                    }
                    if (Convert.ToInt32(curStr) < 0)
                    {
                        if (afterDecode.First().horizontalSide < afterDecode.First().verticalSide)
                        {
                           
                            g.DrawLine(new Pen(Brushes.DarkMagenta, 1), new Point(afterDecode.First().x1, (afterDecode.First().y1 + afterDecode.First().y2) / 2), new Point(afterDecode.First().x2, (afterDecode.First().y1 + afterDecode.First().y2) / 2));
                            pol.horizontalSide = afterDecode.First().horizontalSide;
                            pol.verticalSide = afterDecode.First().verticalSide / 2;
                            pol.x1 = afterDecode.First().x1;
                            pol.x2 = afterDecode.First().x2;
                            pol.y1 = afterDecode.First().y1;
                            pol.y2 = afterDecode.First().y2 - afterDecode.First().verticalSide / 2;
                            afterDecode.AddLast(pol);
                            pol.horizontalSide = afterDecode.First().horizontalSide;
                            pol.verticalSide = afterDecode.First().verticalSide / 2;
                            pol.x1 = afterDecode.First().x1;
                            pol.x2 = afterDecode.First().x2;
                            pol.y1 = afterDecode.First().y2 - afterDecode.First().verticalSide / 2 + 1;
                            pol.y2 = afterDecode.First().y2;
                            afterDecode.AddLast(pol);
                            afterDecode.RemoveFirst();
                        }
                        else
                        {
                            g.DrawLine(new Pen(Brushes.DarkMagenta, 1), new Point((afterDecode.First().x1 + afterDecode.First().x2) / 2, afterDecode.First().y1), new Point((afterDecode.First().x1 + afterDecode.First().x2) / 2, afterDecode.First().y2));
                            pol.horizontalSide = afterDecode.First().horizontalSide / 2;
                            pol.verticalSide = afterDecode.First().verticalSide;
                            pol.x1 = afterDecode.First().x1;
                            pol.x2 = afterDecode.First().x2 - afterDecode.First().horizontalSide / 2;
                            pol.y1 = afterDecode.First().y1;
                            pol.y2 = afterDecode.First().y2;
                            afterDecode.AddLast(pol);
                            pol.horizontalSide = afterDecode.First().horizontalSide / 2;
                            pol.verticalSide = afterDecode.First().verticalSide;
                            pol.x1 = afterDecode.First().x2 - afterDecode.First().horizontalSide / 2 + 1;
                            pol.x2 = afterDecode.First().x2;
                            pol.y1 = afterDecode.First().y1;
                            pol.y2 = afterDecode.First().y2;
                            afterDecode.AddLast(pol);
                            afterDecode.RemoveFirst();
                        }


                    }
                    else
                    {
                        decodeIm.AddLast(afterDecode.First());
                        afterDecode.RemoveFirst();
                    }
                }
                String sign = "";
                for (int i = 0; i < 256; i++)
                    for (int j = 0; j < 256; j++)
                    {
                        sign = reader.ReadLine();
                        brightOfDec[j, i] = Convert.ToInt16(sign);
                    }
                PixelFormat pxf = PixelFormat.Format24bppRgb;
                Bitmap bmp = new Bitmap(256, 256);
                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, pxf);
                IntPtr ptr = bmpData.Scan0;
                int numBytes = bmpData.Stride * bmp.Height;
                int widthBytes = bmpData.Stride;
                int m = 0, n = 0; 
                byte[] rgbValues = new byte[numBytes];
                Marshal.Copy(ptr, rgbValues, 0, numBytes);
                for (int counter = 0; counter < rgbValues.Length; counter += 3)
                {
                    byte color_b = 0;
                    if (m == 256)
                    {
                        n++;
                        m = 0;
                    }
                    byte value = (byte)brightOfDec[m, n];
                    m++;
                    color_b = value;

                    rgbValues[counter] = color_b;
                    rgbValues[counter + 1] = color_b;
                    rgbValues[counter + 2] = color_b;

                }
                Marshal.Copy(rgbValues, 0, ptr, numBytes);
                bmp.UnlockBits(bmpData);
                pictureBox2.Image = bmp;
                return;
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Point p = pictureBox2.PointToClient(System.Windows.Forms.Cursor.Position);
            MessageBox.Show(p.ToString());
            point curr;
            g = pictureBox3.CreateGraphics();
            g.DrawLine(new Pen(Brushes.Aquamarine, 1), new Point(p.X - 1, p.Y), new Point(p.X + 1, p.Y));
            g.DrawLine(new Pen(Brushes.Aquamarine, 1), new Point(p.X, p.Y - 1), new Point(p.X, p.Y + 1));
            curr.x = p.X;
            curr.y = p.Y;
            listOfPoints.AddLast(curr);
            
        }

        private void button13_Click(object sender, EventArgs e)
        {
            listOfPoints.Clear();
            g.Clear(Color.White);
        }

        private void button14_Click(object sender, EventArgs e)
        {
            MessageBox.Show(Convert.ToString(contourC));
            MessageBox.Show(Convert.ToString(listOfPoints.Count));
            LinkedList<point> dubl = new LinkedList<point>();
            point curP;
            point curP1 = new point();
            int[,] copyCont = new int[contourC, 2];
            for (int i = 0; i < listOfPoints.Count; i++)
            {
                curP = listOfPoints.First();
                listOfPoints.RemoveFirst();
                listOfPoints.AddLast(curP);  
                dubl.AddLast(curP);
            }
            for (int i = 0; i < contourC; i++)
            {
                copyCont[i, 0] = contour[i, 0];
                copyCont[i, 1] = contour[i, 1];
            }
            while (dubl.Count != 0)
            {
                curP = dubl.First();
                dubl.RemoveFirst();
                    for (int k = 0; k < contourC; k++)
                    {
                        if (copyCont[k, 0] != -1)
                        {
                            curP1.x = copyCont[k, 0];
                            curP1.y = copyCont[k, 1];
                            if ((Math.Abs(curP.x - copyCont[k, 0]) == 1 || curP.x == copyCont[k, 0]) && (Math.Abs(curP.y - copyCont[k, 1]) == 1 || curP.y == copyCont[k, 1]))
                            {
                                car.AddLast(curP1);
                                dubl.AddLast(curP1);
                                copyCont[k, 0] = -1;
                            }
                        }
                    }
                
            }
            for (int i =0; i < 256; i++)
                for (int j =0; j < 256; j++)
                {
                    rezCanny[j, i] = 255;
                }
            while (car.Count != 0)
            {
                point c = car.First();
                car.RemoveFirst();
                rezCanny[c.x, c.y] = 0;
            }
            int m = 0, n = 0;
            PixelFormat pxf = PixelFormat.Format24bppRgb;
            Bitmap bmp = new Bitmap(pictureBox1.Image);
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, pxf);
            IntPtr ptr = bmpData.Scan0;
            int numBytes = bmpData.Stride * bmp.Height;
            int widthBytes = bmpData
                .Stride;
            byte[] rgbValues = new byte[numBytes];
            Marshal.Copy(ptr, rgbValues, 0, numBytes);
            for (int counter = 0; counter < rgbValues.Length; counter += 3)
            {
                byte color_b = 0;
                if (m == 256)
                {
                    n++;
                    m = 0;
                }
                byte value = (byte)rezCanny[m, n];
                m++;
                color_b = value;
                rgbValues[counter] = color_b;
                rgbValues[counter + 1] = color_b;
                rgbValues[counter + 2] = color_b;
            }
            Marshal.Copy(rgbValues, 0, ptr, numBytes);
            bmp.UnlockBits(bmpData);
            pictureBox3.Image = bmp;
            undoClass.add(pictureBox2.Image, false, true);
            count++;
            pic2 = true;
            return;
        }

        private Bitmap paintClosedObject(int[,] bounds)
        {
            //int startEndBounds 
            return null;
        }

        private void button2_MouseHover(object sender, EventArgs e)
        {
            ToolTip t = new ToolTip();
            t.SetToolTip(button2, "Рекомендуемые пределы задания порога яркости: 1 - 150\nРекомендуемые значения порога разбиения: 1, 2, 4, 8, 16");
        }

        private void btn_aCanny_Click(object sender, EventArgs e)
        {
            contour = new int[65536, 2];
            contourC = 0;
            int limith = 0, m = 0, n = 0;
            int limitl = 0;
            int sizeOfGausMask = 5;
            rezCanny = new int[256, 256];
            try
            {
                sizeOfGausMask = Convert.ToInt16(textBox5.Text);
            }
            catch (Exception er)
            {
                sizeOfGausMask = 5;
            }
            try
            {
                limith = Convert.ToInt16(textBox3.Text);
            }
            catch (Exception er)
            {
                limith = 100;
            }
            try
            {
                limitl = Convert.ToInt16(textBox4.Text);
            }
            catch (Exception er)
            {
                limitl = 80;
            }

            var detector = new CannyEdgeDetector((byte)limitl, (byte)limith, sizeOfGausMask);

            var grayScaleImage = new GrayscaleY().Apply((Bitmap)pictureBox1.Image);

            pictureBox2.Image = detector.Apply(grayScaleImage);

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button16_Click(object sender, EventArgs e)
        {
            var qualityOfSegmentationForm = new EdgeDetectionEvaluationForm(new CriteriaEvaluationResult[] {new CriteriaEvaluationResult("Pratt", 0, 100, 99)});
            qualityOfSegmentationForm.Show(this);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if ((pic == true) && (count > 0))
            {
                readBRight();
                rekursFragmentation();
                compressedImage();
                undoClass.add(pictureBox2.Image, false, true);
                count++;
                pic2 = true;
                
            }
            else
            {
                MessageBox.Show("Error!- Image is not opened", "Error");
            }
        }
        }
    }


//вторая версия : более простая(пи.си. для нубов), но не очень эффективная
/*using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        Undo undoClass = new Undo(Image.FromFile("C:\\Users\\Антон\\Desktop\\images — копия.jpg"), Image.FromFile("C:\\Users\\Антон\\Desktop\\images — копия — копия.jpg"));
        Boolean pic = false, pic2 = false;
        int count = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                count++;
                // Assign the cursor in the Stream to the Form's Cursor property.
                pictureBox1.Image = Image.FromFile(ofd.FileName);
                pic = true;
                undoClass.add(pictureBox1.Image, pictureBox2.Image);
            }
            MessageBox.Show("Данная программа перекрашивает разные оттенки серого\n                                              в синий цвет.");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if ((pic != false) && (pic2 != false))
            {
                Bitmap bmpSave = (Bitmap)pictureBox2.Image;
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.DefaultExt = "bmp";
                sfd.Filter = "Image files (*.bmp)|*.bmp|All files (*.*)|*.*";
                if (sfd.ShowDialog() == DialogResult.OK)

                    bmpSave.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            else
            {
                MessageBox.Show("Error!- Image is not hadled or not opened", "Error");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if ((pic) && (count > 0))
            {
                pictureBox1.Image = Image.FromFile("C:\\Users\\Антон\\Desktop\\images — копия.jpg");
                pictureBox2.Image = Image.FromFile("C:\\Users\\Антон\\Desktop\\images — копия — копия.jpg");
                undoClass.add(pictureBox1.Image, pictureBox2.Image);              
                pic = false;
                pic2 = false;
            }
            else
            {
                MessageBox.Show("Error!");
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (Undo.i >= 1)
            {

                undoClass.Limg.RemoveLast();
                Undo.i--;
                pictureBox1.Image = undoClass.getLast().im1;
                pictureBox2.Image = undoClass.getLast().im2;
                

            }
            else
                MessageBox.Show("Назад?- Серьёзно?- Назад в будущее чтоли ???!!!", "Fatal error");

        }


        // реакция на нажатие на кнопку run
        private void button2_Click(object sender, EventArgs e)
        {
            if (pic == true)
            {
                Image im = pictureBox1.Image;
                Bitmap picbit = new Bitmap(im);
                int width = im.Width;
                int height = im.Height;
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        Color pixel = picbit.GetPixel(j, i);
                        if ((Math.Abs(pixel.R - pixel.G) < 20) && ((Math.Abs(pixel.R - pixel.B) < 20)) && ((Math.Abs(pixel.G - pixel.B) < 20)) || (pixel.R < 20) && (pixel.G < 20) && (pixel.B < 20))
                        {

                            picbit.SetPixel(j, i, Color.Blue);
                        }

                    }
                }
                pictureBox2.Image = picbit;
                undoClass.add(pictureBox1.Image, pictureBox2.Image);
                count++;


                pic2 = true;
            }
            else
            {
                MessageBox.Show("Error!- Image is not opened", "Error");
            }
        }
    }
}*/