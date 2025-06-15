using System;
using System.Collections;
using System.Formats.Tar;
using System.IO;
using System.Linq;

namespace fmtownscolorize
{
    public partial class Form1 : Form
    {
        int[] fileData;

        public Form1()
        {
            InitializeComponent();
            fileData = new int[1];
        }

        int getPictureType(string strFile, out int height, out int width)
        {
            height = 32;
            width = 32;

            long length = new System.IO.FileInfo(strFile).Length;
            try
            {
                Image tifImage = Image.FromFile(strFile);

                height = tifImage.Height;
                width = tifImage.Width;

                if (length % 0x200 == 0)
                {
                    return 1;
                }
            }
            catch (Exception ex)
            {
                height = 32;
                width = 32;
                return 1;
            }
            byte[] data = File.ReadAllBytes(strFile);
            for(int index = 0x400; index < data.Length; index++)
            {
                int tempval = data[index];
                int val1 = (tempval >> 4) & 0xF;
                int val2 = tempval & 0xF;

                if(val1 > 7 || val2 > 7)
                {
                    return 0;
                }
            }

            return 1;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            string[] strFiles = Directory.GetFiles("G:\\source\\fmtowns\\testimage", "*.tif");

            foreach (string strFile in strFiles)
            {
                string strOutFile = strFile;
                strOutFile = strOutFile.Replace("TIF", "png");

                try
                {
                    int height;
                    int width;
                    int picType = getPictureType(strFile, out height, out width);

                    

                    if(picType == 0)
                    {
                        Image tifImage = Image.FromFile(strFile);
                        LoadFile(strFile, strOutFile, width, height);
                    }
                    else if(picType == 1)
                    {
                        CreateFile(strFile, strOutFile, width, height);
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }

        private void expandData(byte[] data)
        {
            fileData = new int[data.Length * 2];
            int tempCount = 0;
            for (int index = 0; index < data.Length; index++)
            {
                int val1 = (data[index] >> 4) & 0xF;
                int val2 = data[index] & 0xF;
                fileData[tempCount] = val2;
                fileData[tempCount + 1] = val1;
                tempCount += 2;
            }
        }

        private void CreateFile(string strFile, string strOutFile, int width, int height)
        {
            byte[] data = File.ReadAllBytes(strFile);
            int numimages = (data.Length - 0x200) / ((width / 2) * height);
            expandData(data);
            int startPos = 0x400;
            Bitmap bmp = new Bitmap(width * numimages, height);

            for (int indexY = 1; indexY < numimages + 1; ++indexY)
            {
                int tempCount = 0;
                Color blah;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int tempval = fileData[startPos + tempCount];
                        tempCount++;

                        switch (tempval)
                        {
                            case 0:
                                if(numimages == 1)
                                {
                                    blah = Color.Black;
                                }
                                else
                                {
                                    blah = Color.Gray;
                                }
                                    
                                break;
                            case 1:
                                blah = Color.Blue;
                                break;
                            case 2:
                                blah = Color.Red;
                                break;
                            case 3:
                                blah = Color.Black;
                                break;
                            case 4:
                                blah = Color.Lime;
                                break;
                            case 5:
                                blah = Color.Cyan;
                                break;
                            case 6:
                                blah = Color.Yellow;
                                break;
                            case 7:
                                blah = Color.White;
                                break;
                            default:
                                //tempval *= 16;
                                tempval = 0;
                                blah = Color.FromArgb(tempval, tempval, tempval);
                                break;
                        }

                        bmp.SetPixel(x + ((indexY - 1) * width), y, blah);
                    }
                }
                startPos += 0x400;
            }
            bmp.Save(strOutFile, System.Drawing.Imaging.ImageFormat.Png);
        }

        private void LoadFile(string strFile, string strOutFile, int width, int height)
        {
            byte[] data = File.ReadAllBytes(strFile);
            short[] shortArray = new short[(int)Math.Ceiling((data.Length - 0x200) / 2.0)]; // Create short array of appropriate size
            Buffer.BlockCopy(data, 0x200, shortArray, 0, (data.Length - 0x200));
            List<short> list = new List<short>();
            for (int index = 0; index < shortArray.Length; index++)
            {

                if (!list.Contains(shortArray[index]))
                {
                    list.Add(shortArray[index]);
                }
            }
            Bitmap bmp = new Bitmap(width, height);
            Color blah;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    short curValue = shortArray[y * width + x];

                    int g = (curValue >> 10) & 0x1F;
                    int r = (curValue >> 5) & 0x1F;
                    int b = (curValue >> 0) & 0x1F;

                    r = (int)(255.0f * (r / 31.0f));
                    g = (int)(255.0f * (g / 31.0f));
                    b = (int)(255.0f * (b / 31.0f));
                    blah = Color.FromArgb(r, g, b);

                    bmp.SetPixel(x, y, blah);
                }
            }

            bmp.Save(strOutFile, System.Drawing.Imaging.ImageFormat.Png);
        }
    }
}
