using BitMiracle.LibTiff.Classic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

namespace MakeTiff
{
    class Program
    {
        static void Main(string[] args)
        {
            //test1();
            //creatingTiff();
            //WriteTiledTiff("bigtiff.tiff", BitmapSourceFromBrush(new RadialGradientBrush(Colors.Aqua, Colors.Red), 256));
            //WriteScanline();
            //WriteTile();


            
            string path = Console.ReadLine();
            List<string> dirs = new List<string>(System.IO.Directory.GetDirectories(path));
            List<string> pics = new List<string>(System.IO.Directory.GetFiles(dirs[0], "*.jpg"));
            dirs.Sort();
            pics.Sort();

            string[,] files = new string[dirs.Count, pics.Count];
            for (int row = 0; row < dirs.Count; row++) {
                for (int col = 0; col < pics.Count; col++) {
                    files[row, col] = System.IO.Path.GetFullPath(dirs[row] + "/" + System.IO.Path.GetFileName(pics[col]));
                }
            }
            UnionIMG(files, 256, 256, "OUT.TIF");
            
            
            Console.WriteLine("完成");
            Console.ReadKey();
        }


        /// <summary>
        /// 合并数据
        /// </summary>
        /// <param name="files">要合并的文件列表</param>
        /// <param name="tilewidth">瓦片宽度 必须是16的倍数</param>
        /// <param name="tileheight">瓦片高度 必须是16的倍数</param>
        /// <param name="outfile">输出文件</param>
        private static void UnionIMG(string[,] files, int tilewidth, int tileheight, string outfile)
        {
            using (Tiff tif = Tiff.Open(outfile, "w")) {
                // 
                int width = files.GetLength(1) * tilewidth;         // 整个图片大小
                int height = files.GetLength(0) * tileheight;       // 整个图片大小

                tif.SetField(TiffTag.IMAGEWIDTH, width);
                tif.SetField(TiffTag.IMAGELENGTH, height);
                tif.SetField(TiffTag.COMPRESSION, Compression.NONE);
                tif.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB);
                tif.SetField(TiffTag.SUBFILETYPE, 0);
                tif.SetField(TiffTag.BITSPERSAMPLE, 8);
                tif.SetField(TiffTag.SAMPLESPERPIXEL, 3);
                tif.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                tif.SetField(TiffTag.TILEWIDTH, tilewidth);
                tif.SetField(TiffTag.TILELENGTH, tileheight);

                int index = 0;
                for (int row = 0; row < files.GetLength(0); row++) {
                    for (int col = 0; col < files.GetLength(1); col++) {
                        // 打开图片
                        byte[] pixels = null;
                        using (Bitmap bmp = new Bitmap(files[row, col])) {
                            pixels = GetImageRasterBytes(bmp, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                            bmp.Clone();
                        }
                        tif.WriteEncodedTile(index++, pixels, pixels.Length);

                        if (index % 100 == 0) Console.WriteLine("{0} / {1}", index, files.Length);
                    }
                }
                tif.WriteDirectory();
            }
        }


        /// <summary>
        /// 按行写数据
        /// </summary>
        private static void WriteScanline()
        {
            using (Tiff tif = Tiff.Open("cool.tif", "w")) {
                // 
                int width = 10000;
                int height = 10000;

                tif.SetField(TiffTag.IMAGEWIDTH, width);
                tif.SetField(TiffTag.IMAGELENGTH, height);
                tif.SetField(TiffTag.COMPRESSION, Compression.NONE);
                tif.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB);
                tif.SetField(TiffTag.SUBFILETYPE, 0);
                tif.SetField(TiffTag.BITSPERSAMPLE, 8);
                tif.SetField(TiffTag.SAMPLESPERPIXEL, 3);
                tif.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);

                long index = 0;
                for (int line = 0; line < height; line++) {
                    byte[] raster = new byte[width * 3];
                    for (int n = 0; n < width * 3; n += 3) {
                        raster[n] = (byte)(index++ % 256);
                        raster[n + 1] = (byte)(index / 256);
                        raster[n + 2] = (byte)(index / (256 * 256));
                    }
                    bool b = tif.WriteScanline(raster, line, 0);
                    if (line % 10000 == 0) Console.WriteLine(line);
                }
            }
        }

        /// <summary>
        /// 按块写数据
        /// </summary>
        private static void WriteTile()
        {
            using (Tiff tif = Tiff.Open("cool.tif", "w")) {
                // 
                int width = 10000;        // 整个图片大小
                int height = 10000;       // 整个图片大小
                int tilewidth = 255;     // 瓦片必须是16的倍数
                int tileheight = 255;    // 瓦片必须是16的倍数

                tif.SetField(TiffTag.IMAGEWIDTH, width);
                tif.SetField(TiffTag.IMAGELENGTH, height);
                tif.SetField(TiffTag.COMPRESSION, Compression.NONE);
                tif.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB);
                tif.SetField(TiffTag.SUBFILETYPE, 0);
                tif.SetField(TiffTag.BITSPERSAMPLE, 8);
                tif.SetField(TiffTag.SAMPLESPERPIXEL, 3);
                tif.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);

                tif.SetField(TiffTag.TILEWIDTH, tilewidth);
                tif.SetField(TiffTag.TILELENGTH, tileheight);

                byte[] pixels = new byte[tilewidth * tileheight * 3];
                for (int i = 0; i < pixels.Length; i++) {
                    pixels[i] = (byte)(i % 256);
                }

                int count = ((width + tilewidth - 1) / tilewidth) * ((height + tileheight - 1) / tileheight);

                for (int i = 0; i < count; i++) {

                    int c = new Random(i).Next(255);
                    for (int n = 0; n < pixels.Length; n++) {
                        pixels[n] = (byte)c;
                    }

                    tif.WriteEncodedTile(i, pixels, pixels.Length);

                    if (i % 1000 == 0) Console.WriteLine("{0}/{1}", i, count);
                }

                tif.WriteDirectory();
            }
        }

        private static bool creatingTiff()
        {
            //using (Tiff tif = Tiff.Open("cool.tif", "w")) {
            //    int width = 10000;
            //    int height = 10000;

            //    tif.SetField(TiffTag.IMAGEWIDTH, width);
            //    tif.SetField(TiffTag.IMAGELENGTH, height);
            //    tif.SetField(TiffTag.COMPRESSION, Compression.NONE);
            //    tif.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB);
            //    tif.SetField(TiffTag.SUBFILETYPE, 0);
            //    //tif.SetField(TiffTag.ROWSPERSTRIP, 1);      // 高度
            //    //tif.SetField(TiffTag.ORIENTATION, BitMiracle.LibTiff.Classic.Orientation.TOPLEFT);
            //    //tif.SetField(TiffTag.XRESOLUTION, 72);
            //    //tif.SetField(TiffTag.YRESOLUTION, 72);
            //    //tif.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.INCH);
            //    tif.SetField(TiffTag.BITSPERSAMPLE, 8);
            //    tif.SetField(TiffTag.SAMPLESPERPIXEL, 3);
            //    //tif.SetField(TiffTag.JPEGIFOFFSET, 768);
            //    tif.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);



            //    long index = 0;
            //    for (int line = 0; line < height; line++) {
            //        byte[] raster = new byte[width * 3];
            //        for (int n = 0; n < width * 3; n += 3) {
            //            raster[n] = (byte)(index++ % 256);
            //            raster[n + 1] = (byte)(index++ % 256);
            //            raster[n + 2] = (byte)(index++ % 256);
            //        }
            //        bool b = tif.WriteScanline(raster, line, 0);

            //        if (line % 10000 == 0) Console.WriteLine(line);
            //    }
            //    //byte[] raster = new byte[] { 96, 128, 254 };
            //    //bool b = tif.WriteScanline(raster, 0, 0);
            //}
            //return true;


            using (Bitmap bmp = new Bitmap("test.jpg"))
            {
                using (Tiff tif = Tiff.Open("BitmapTo24BitColorTiff.tif", "w"))
                {
                    byte[] raster = GetImageRasterBytes(bmp, System.Drawing.Imaging.PixelFormat.Format24bppRgb);


                    //tif.SetField(TiffTag.SUBFILETYPE, 1, TiffType.LONG); 
                    //tif.SetField(TiffTag.THRESHHOLDING, 1, TiffType.SHORT); 
                    //tif.SetField(TiffTag.DOCUMENTNAME, 1, TiffType.ASCII); 
                    //tif.SetField(TiffTag.IMAGEDESCRIPTION, 1, TiffType.ASCII); 
                    //tif.SetField(TiffTag.MAKE, 1, TiffType.ASCII); 
                    //tif.SetField(TiffTag.MODEL, 1, TiffType.ASCII); 
                    //tif.SetField(TiffTag.MINSAMPLEVALUE, 1, TiffType.SHORT); 
                    //tif.SetField(TiffTag.MAXSAMPLEVALUE, 1, TiffType.SHORT); 
                    //tif.SetField(TiffTag.XRESOLUTION, 1, TiffType.RATIONAL); 
                    //tif.SetField(TiffTag.YRESOLUTION, 1, TiffType.RATIONAL); 
                    //tif.SetField(TiffTag.PAGENAME, 1, TiffType.ASCII); 
                    //tif.SetField(TiffTag.XPOSITION, 1, TiffType.RATIONAL); 
                    //tif.SetField(TiffTag.YPOSITION, 1, TiffType.RATIONAL); 
                    //tif.SetField(TiffTag.RESOLUTIONUNIT, 1, TiffType.SHORT); 
                    //tif.SetField(TiffTag.SOFTWARE, 1, TiffType.ASCII); 
                    //tif.SetField(TiffTag.DATETIME, 1, TiffType.ASCII); 
                    //tif.SetField(TiffTag.ARTIST, 1, TiffType.ASCII); 
                    //tif.SetField(TiffTag.HOSTCOMPUTER, 1, TiffType.ASCII); 
                    //tif.SetField(TiffTag.WHITEPOINT, -1, TiffType.RATIONAL);
                    //tif.SetField(TiffTag.PRIMARYCHROMATICITIES, -1, TiffType.RATIONAL);
                    //tif.SetField(TiffTag.HALFTONEHINTS, 2, TiffType.SHORT); 
                    //tif.SetField(TiffTag.INKSET, 1, TiffType.SHORT); 
                    //tif.SetField(TiffTag.DOTRANGE, 2, TiffType.SHORT); 
                    //tif.SetField(TiffTag.TARGETPRINTER, 1, TiffType.ASCII); 
                    //tif.SetField(TiffTag.SAMPLEFORMAT, 1, TiffType.SHORT); 
                    //tif.SetField(TiffTag.YCBCRCOEFFICIENTS, -1, TiffType.RATIONAL); 
                    //tif.SetField(TiffTag.YCBCRSUBSAMPLING, 2, TiffType.SHORT); 
                    //tif.SetField(TiffTag.YCBCRPOSITIONING, 1, TiffType.SHORT); 
                    ////tif.SetField(TiffTag.REFERENCEBLACKWHITE, -1, TiffType.RATIONAL); 
                    ////tif.SetField(TiffTag.EXTRASAMPLES, -1, TiffType.SHORT); 
                    //tif.SetField(TiffTag.SMINSAMPLEVALUE, 1, TiffType.DOUBLE); 
                    //tif.SetField(TiffTag.SMAXSAMPLEVALUE, 1, TiffType.DOUBLE); 
                    //tif.SetField(TiffTag.STONITS, 1, TiffType.DOUBLE); 



                    tif.SetField(TiffTag.IMAGEWIDTH, bmp.Width);
                    tif.SetField(TiffTag.IMAGELENGTH, bmp.Height);
                    

                    tif.SetField(TiffTag.COMPRESSION, Compression.NONE);
                    tif.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB);
                    //tif.SetField(TiffTag.SUBFILETYPE, 0);
                    //tif.SetField(TiffTag.ROWSPERSTRIP, bmp.Height);
                    //tif.SetField(TiffTag.ORIENTATION, BitMiracle.LibTiff.Classic.Orientation.TOPLEFT);
                    //tif.SetField(TiffTag.XRESOLUTION, bmp.HorizontalResolution);
                    //tif.SetField(TiffTag.YRESOLUTION, bmp.VerticalResolution);
                    //tif.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.INCH);
                    tif.SetField(TiffTag.BITSPERSAMPLE, 8);
                    tif.SetField(TiffTag.SAMPLESPERPIXEL, 3);
                    //tif.SetField(TiffTag.JPEGIFOFFSET, 768);
                    tif.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);


                    long stride = raster.Length / bmp.Height;
                    for (int i = 0, offset = 0; i < bmp.Height; i++) {
                        bool b = tif.WriteScanline(raster, offset, i, 0);
                        //Console.WriteLine("write succes: " + b);
                        offset += bmp.Width * 3;
                    }

                    //for (int i = 0; i < 4; i++) {
                    //    tif.WriteEncodedTile(i, raster, 4);
                    //}
                    
                }
                System.Diagnostics.Process.Start("BitmapTo24BitColorTiff.tif");
                return true;
            }
        }

        #region 读取图片信息
        /// <summary>
        /// 读取图片的RGB信息
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        private static byte[] GetImageRasterBytes(Bitmap bmp, PixelFormat format)
        {
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            byte[] bits = null;
            try {
                // Lock the managed memory
                BitmapData bmpdata = bmp.LockBits(rect, ImageLockMode.ReadWrite, format);
                // Declare an array to hold the bytes of the bitmap.
                bits = new byte[bmpdata.Stride * bmpdata.Height];
                // Copy the values into the array.
                System.Runtime.InteropServices.Marshal.Copy(bmpdata.Scan0, bits, 0, bits.Length);
                // Release managed memory
                bmp.UnlockBits(bmpdata);
                // 重新排列顺序
                ConvertSamples(bits);
                return bits;
            }
            catch { return null; }
        }
        /// <summary>
        /// BGR排列给RGB排列
        /// </summary>
        /// <param name="data"></param>
        private static void ConvertSamples(byte[] data)
        {
            const int samplesPerPixel = 3;      // 每个像素长度 RGB 所以3个字节
            for (int i = 0; i < data.Length; i += samplesPerPixel)
            {
                byte temp = data[i + 2];
                data[i + 2] = data[i];
                data[i] = temp;
            }
        }
        #endregion


        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        private static void ConvertSamples(byte[] data, int width, int height)
        {
            int stride = data.Length / height;
            const int samplesPerPixel = 3;

            for (int y = 0; y < height; y++) {
                int offset = stride * y;
                int strideEnd = offset + width * samplesPerPixel;

                for (int i = offset; i < strideEnd; i += samplesPerPixel) {
                    byte temp = data[i + 2];
                    data[i + 2] = data[i];
                    data[i] = temp;
                }
            }
        }














        //public static BitmapSource BitmapSourceFromBrush(Brush drawingBrush, int size = 32, int dpi = 96)
        //{
        //    // RenderTargetBitmap = builds a bitmap rendering of a visual
        //    var pixelFormat = PixelFormats.Pbgra32;
        //    RenderTargetBitmap rtb = new RenderTargetBitmap(size, size, dpi, dpi, pixelFormat);

        //    // Drawing visual allows us to compose graphic drawing parts into a visual to render
        //    var drawingVisual = new DrawingVisual();
        //    using (DrawingContext context = drawingVisual.RenderOpen())
        //    {
        //        // Declaring drawing a rectangle using the input brush to fill up the visual
        //        context.DrawRectangle(drawingBrush, null, new Rect(0, 0, size, size));
        //    }

        //    // Actually rendering the bitmap
        //    rtb.Render(drawingVisual);
        //    return rtb;
        //}

        //public static void WriteTiledTiff(string fileName, BitmapSource tile)
        //{
        //    const int PIXEL_WIDTH = 48000;
        //    const int PIXEL_HEIGHT = 48000;

        //    int iTile_Width = tile.PixelWidth;
        //    int iTile_Height = tile.PixelHeight;

        //    using (Tiff tiff = Tiff.Open(fileName, "w"))
        //    {
        //        tiff.SetField(TiffTag.IMAGEWIDTH, PIXEL_WIDTH);
        //        tiff.SetField(TiffTag.IMAGELENGTH, PIXEL_HEIGHT);
        //        tiff.SetField(TiffTag.COMPRESSION, Compression.NONE);
        //        tiff.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB);

        //        tiff.SetField(TiffTag.ROWSPERSTRIP, PIXEL_HEIGHT);

        //        tiff.SetField(TiffTag.XRESOLUTION, 96);
        //        tiff.SetField(TiffTag.YRESOLUTION, 96);

        //        tiff.SetField(TiffTag.BITSPERSAMPLE, 8);
        //        tiff.SetField(TiffTag.SAMPLESPERPIXEL, 3);

        //        tiff.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);

        //        tiff.SetField(TiffTag.TILEWIDTH, iTile_Width);
        //        tiff.SetField(TiffTag.TILELENGTH, iTile_Height);
        //        Bitmap b;
                
        //        int tileC = 0;
        //        for (int row = 0; row < PIXEL_HEIGHT; row += iTile_Height)
        //        {
        //            for (int col = 0; col < PIXEL_WIDTH; col += iTile_Width)
        //            {
        //                if (tile.Format != PixelFormats.Rgb24) tile = new FormatConvertedBitmap(tile, PixelFormats.Rgb24, null, 0);
        //                int stride = tile.PixelWidth * ((tile.Format.BitsPerPixel + 7) / 8);

        //                byte[] pixels = new byte[tile.PixelHeight * stride];
        //                tile.CopyPixels(pixels, stride, 0);

        //                tiff.WriteEncodedTile(tileC++, pixels, pixels.Length);
        //            }
        //        }

        //        tiff.WriteDirectory();
        //    }
        //}



    }
}
