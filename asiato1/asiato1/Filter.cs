﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace asiato1
{
    class Filter
    {
        public  Bitmap Apply(Bitmap source, int size)        //初期 : int size=3
        {
            // ビットマップ画像から全てのピクセルを抜き出す
            PixelManipulator s = PixelManipulator.LoadBitmap(source);
            PixelManipulator d = s.Clone();

            // 範囲チェック
            if (size < 3)
            {
                size = 3;
            }
            if (size > 15)
            {
                size = 15;
            }
            size--;
            size /= 2;

            // カーネルを作成する
            float[] kernel = _CreateGaussianKernel(size);

            // 全てのピクセルを巡回する
            s.EachPixel((x, y) =>
            {
                byte r = _GaussianBlur(kernel, s.RangeR(x, y, size));
                byte g = _GaussianBlur(kernel, s.RangeG(x, y, size));
                byte b = _GaussianBlur(kernel, s.RangeB(x, y, size));
                d.SetPixel(x, y, r, g, b);
            });

            // 新しいビットマップ画像を作成して、ピクセルをセットする
            return d.CreateBitmap();
        }

        // カーネルを作成する
        private static float[] _CreateGaussianKernel(int size)
        {
            float sigma = 0.215f * ((size - 1f) * 0.5f - 1f) + 0.81f;
            float[] kernel = new float[(size * 2 + 1) * (size * 2 + 1)];
            float total = 0;
            int count = 0;
            for (int y = -size; y <= size; y++)
            {
                for (int x = -size; x <= size; x++)
                {
                    kernel[count] = _GaussianF(x, y, sigma);
                    total += kernel[count];
                    count++;
                }
            }
            return kernel;
        }

        // ガウス分布のメソッド
        private static float _GaussianF(float x, float y, float sigma)
        {
            float pi = (float)Math.PI;
            float sigma2 = sigma * sigma;
            return (1f / (2f * pi * sigma2)) * (float)Math.Exp(-(x * x + y * y) / (2f * sigma2));
        }

        // ガウシアンフィルタを適用する
        private static byte _GaussianBlur(float[] kernel, byte[,] pixels)
        {
            float color = 0;
            int size = pixels.GetLength(0);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float c = pixels[x, y];
                    c *= kernel[x + y * size];
                    color += c;
                }
            }
            return (byte)color;
        }





        // ピクセル操作クラス (今のところ 24 bit RGB 固定)
        internal class PixelManipulator
        {
            // 画像の幅
            public int width;

            // 画像の高さ
            public int height;

            // スキャンラインの幅
            public int stride;

            // ピクセルのバイトサイズ (24 bit RGB の場合は 3)
            public int pixelSize;

            // 画像データを展開したバイト配列
            public byte[] bytes;

            // ピクセルフォーマット
            public PixelFormat pixelFormat;

            // コンストラクタ
            public PixelManipulator()
            {
                pixelFormat = PixelFormat.Format24bppRgb;
            }

            // ピクセルをセットする
            public void SetPixel(int x, int y, byte r, byte g, byte b)
            {
                if (_IsValidPosition(x, y) == false)
                {
                    return;
                }
                int i = _GetIndex(x, y);
                bytes[i + 2] = r;
                bytes[i + 1] = g;
                bytes[i + 0] = b;
            }

            // R の値を取得する
            public byte R(int x, int y, byte defaultValue = 0)
            {
                if (_IsValidPosition(x, y) == false)
                {
                    return defaultValue;
                }
                int i = _GetIndex(x, y);
                return bytes[i + 2];
            }

            // R の値を設定する
            public void SetR(int x, int y, byte value)
            {
                if (_IsValidPosition(x, y) == false)
                {
                    return;
                }
                int i = _GetIndex(x, y);
                bytes[i + 2] = value;
            }

            // G の値を取得する
            public byte G(int x, int y, byte defaultValue = 0)
            {
                if (_IsValidPosition(x, y) == false)
                {
                    return defaultValue;
                }
                int i = _GetIndex(x, y);
                return bytes[i + 1];
            }

            // G の値を設定する
            public void SetG(int x, int y, byte value)
            {
                if (_IsValidPosition(x, y) == false)
                {
                    return;
                }
                int i = _GetIndex(x, y);
                bytes[i + 1] = value;
            }

            // B の値を取得する
            public byte B(int x, int y, byte defaultValue = 0)
            {
                if (_IsValidPosition(x, y) == false)
                {
                    return defaultValue;
                }
                int i = _GetIndex(x, y);
                return bytes[i];
            }

            // B の値を設定する
            public void SetB(int x, int y, byte value)
            {
                if (_IsValidPosition(x, y) == false)
                {
                    return;
                }
                int i = _GetIndex(x, y);
                bytes[i] = value;
            }

            // 全てのピクセルを巡回する
            public void EachPixel(Action<int, int> action)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        action(x, y);
                    }
                }
            }

            // (x, y) を中心に、周囲 size ピクセル分の R 値の配列を取得する
            public byte[,] RangeR(int x, int y, int size)
            {
                return _Range(R, x, y, size);
            }

            // (x, y) を中心に、周囲 size ピクセル分の G 値の配列を取得する
            public byte[,] RangeG(int x, int y, int size)
            {
                return _Range(G, x, y, size);
            }

            // (x, y) を中心に、周囲 size ピクセル分の B 値の配列を取得する
            public byte[,] RangeB(int x, int y, int size)
            {
                return _Range(B, x, y, size);
            }

            // PixelManipulator をコピーする
            public PixelManipulator Clone(bool copyBytes = false)
            {
                PixelManipulator result = new PixelManipulator();
                result.width = width;
                result.height = height;
                result.stride = stride;
                result.pixelSize = pixelSize;
                result.bytes = new byte[bytes.Length];
                if (copyBytes)
                {
                    Array.Copy(bytes, result.bytes, bytes.Length);
                }
                return result;
            }

            // ビットマップ画像を作成する
            public Bitmap CreateBitmap()
            {
                Bitmap bitmap = new Bitmap(width, height, pixelFormat);
                Rectangle rect = new Rectangle(0, 0, width, height);
                BitmapData data = bitmap.LockBits(rect, ImageLockMode.WriteOnly, pixelFormat);
                Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);
                bitmap.UnlockBits(data);
                return bitmap;
            }

            // 指定された座標が正常な範囲に収まっているかチェックする
            private bool _IsValidPosition(int x, int y)
            {
                if (x < 0 || x >= width || y < 0 || y >= height)
                {
                    return false;
                }
                return true;
            }

            // bytes 変数の中の、指定された座標のインデックス値を取得する
            private int _GetIndex(int x, int y)
            {
                return (x + y * stride) * pixelSize;
            }

            // (x, y) を中心に、周囲 size ピクセル分のピクセルを取得する
            private byte[,] _Range(Func<int, int, byte, byte> func, int x, int y, int size)
            {
                int count = size * 2 + 1;
                byte[,] pixels = new byte[count, count];

                byte center = func(x, y, 0);
                for (int y2 = -size; y2 <= size; y2++)
                {
                    for (int x2 = -size; x2 <= size; x2++)
                    {
                        pixels[x2 + size, y2 + size] = func(x + x2, y + y2, center);
                    }
                }
                return pixels;
            }

            // Bitmap オブジェクトから PixelManipulator を作成する
            public static PixelManipulator LoadBitmap(Bitmap bitmap)
            {
                if (bitmap == null)
                {
                    return null;
                }

                PixelManipulator result = new PixelManipulator();
                result.width = bitmap.Width;
                result.height = bitmap.Height;
                result.stride = _GetScanLineSize(bitmap, result.pixelFormat);
                result.pixelSize = Image.GetPixelFormatSize(result.pixelFormat) / 8;
                result.bytes = _GetPixels(bitmap, result.pixelFormat);
                return result;
            }

            // 横の長さを計る (width よりも大きいサイズになることがある)
            private static int _GetScanLineSize(Bitmap bitmap, PixelFormat pixelFormat)
            {
                int pixelSize = Image.GetPixelFormatSize(pixelFormat) / 8;
                Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, pixelFormat);
                int stride = data.Stride;
                bitmap.UnlockBits(data);
                return stride / pixelSize;
            }

            // ビットマップから全てのピクセルをコピーする
            private static byte[] _GetPixels(Bitmap bitmap, PixelFormat pixelFormat)
            {
                Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, pixelFormat);
                byte[] bytes = new byte[data.Stride * bitmap.Height];
                Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
                bitmap.UnlockBits(data);
                return bytes;
            }
        }
    }
}

