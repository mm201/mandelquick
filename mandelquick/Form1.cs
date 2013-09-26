using System;
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
            fractal = new Bitmap(640, 480, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        }

        private unsafe void btnRender_Click(object sender, EventArgs e)
        {
            BitmapData fractalPtr = fractal.LockBits(new Rectangle(0, 0, 640, 480), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            byte* p = (byte*)fractalPtr.Scan0;
            for (int y = 0; y < 480; y++)
            {
                uint* left = (uint *)&(p[y * fractalPtr.Stride]);

                double imaginary = (double)(y - 240) / 160.0d;

                for (int x = 0; x < 640; x++)
                {
                    double real = (double)(x - 320) / 160.0d;
                    Complex position = new Complex(real, imaginary);
                    int iterations = MandelPixel(position); //JuliaPixel(position, new Complex(-0.4d, 0.6d));

                    if (iterations >= 256)
                        left[x] = 0xff0000ff;
                    else
                        left[x] = ((uint) 0x00ffffff - (uint)(Math.Min(iterations * 4, 255)) * 0x00000101) | 0xff000000;
                }
            }

            fractal.UnlockBits(fractalPtr);

            panel1.Invalidate();
        }

        private int MandelPixel(Complex position)
        {
            int iterations = 0;
            Complex position2 = new Complex(0.0d, 0.0d);
            while (iterations < 256 && position2.MagnitudeSquared < 16.0d)
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
            while (iterations < 256 && position.MagnitudeSquared < 16.0d)
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
            g.DrawImage(fractal, 0, 0);
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
}
