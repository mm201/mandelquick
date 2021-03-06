﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace mandelquick
{
    public partial class Form1 : Form
    {
        private Bitmap fractal;

        public Form1()
        {
            InitializeComponent();
            int width = panel1.Width, height = panel1.Height;
            MakeBitmap();
        }

        private void MakeBitmap()
        {
            if (fractal == null || fractal.Width != panel1.Width || fractal.Height != panel1.Height)
            {
                fractal = new Bitmap(panel1.Width, panel1.Height, 
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            }
        }

        private unsafe void btnRender_Click(object sender, EventArgs e)
        {
            MakeBitmap();
            BitmapData fractalPtr = fractal.LockBits(new Rectangle(0, 0, panel1.Width, panel1.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            double width = (double)panel1.Width;
            double height = (double)panel1.Height;

            byte* p = (byte*)fractalPtr.Scan0;
            for (int y = 0; y < panel1.Height; y++)
            {
                uint* left = (uint *)&(p[y * fractalPtr.Stride]);

                double imaginary = ((double)y - height * 0.5d) / width * 4.0d;

                for (int x = 0; x < panel1.Width; x++)
                {
                    double real = ((double)x - width * 0.5d) / width * 4.0d;
                    Complex position = new Complex(real, imaginary);
                    int iterations = JuliaPixel(position, new Complex(-0.4d, 0.6d));

                    if (iterations >= 1024)
                        left[x] = 0xff0000ff;
                    else if (iterations >= 64)
                        left[x] = ((uint)0x00ff0000 - (uint)(Math.Min((iterations - 64) / 4, 255)) * 0x00010000) | 0xff000000;
                    else
                        left[x] = ((uint)0x00ffffff - (uint)(Math.Min(iterations * 4, 255)) * 0x00000101) | 0xff000000;
                }
            }

            fractal.UnlockBits(fractalPtr);

            panel1.Invalidate();
        }

        private int MandelPixel(Complex position)
        {
            int iterations = 0;
            Complex position2 = new Complex(0.0d, 0.0d);
            while (iterations < 512 && position2.MagnitudeSquared < 4.0d)
            {
                position2 *= position2;
                position2 += position;
                iterations++;
            }
            return iterations;
        }

        private int JuliaPixel(Complex position, Complex julia)
        {
            int iterations = 0;
            while (iterations < 1024 && position.MagnitudeSquared < 4.0d)
            {
                position *= position;
                position += julia;
                iterations++;
            }
            return iterations;
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.DrawImage(fractal, 0, 0, panel1.Width, panel1.Height);
        }
    }

    public struct Complex
    {
        double real, imaginary;

        public Complex(double _real, double _imaginary)
        {
            real = _real;
            imaginary = _imaginary;
        }

        public static Complex operator *(Complex a, Complex b)
        {
            return new Complex(a.real * b.real - a.imaginary * b.imaginary, a.real * b.imaginary + a.imaginary * b.real);
        }

        public static Complex operator *(Complex a, double b)
        {
            return new Complex(a.real * b, a.imaginary * b);
        }

        public static Complex operator *(double a, Complex b)
        {
            return new Complex(a * b.real, a * b.imaginary);
        }

        public static Complex operator +(Complex a, Complex b)
        {
            return new Complex(a.real + b.real, a.imaginary + b.imaginary);
        }

        public static Complex operator +(Complex a, double b)
        {
            return new Complex(a.real + b, a.imaginary);
        }

        public static Complex operator +(double a, Complex b)
        {
            return new Complex(a + b.real, b.imaginary);
        }

        public double MagnitudeSquared
        {
            get
            {
                return real * real + imaginary * imaginary;
            }
        }

        public double Magnitude
        {
            get
            {
                return Math.Sqrt(MagnitudeSquared);
            }
        }
    }

    internal static class DpiHelper
    {
        private static float dpi = 0;

        internal static float DPI(Control c, bool force = false)
        {
            if (!force && dpi > 0) return dpi;

            using (System.Drawing.Graphics g = c.CreateGraphics())
            {
                return dpi = g.DpiX;
            }
        }

    }
}
