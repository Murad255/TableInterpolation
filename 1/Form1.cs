using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using Newtonsoft.Json;
using System.IO;


namespace FourQvestion
{


    public partial class Form1 : Form
    {
        private CSpline _model;

        List<double> Ylist = new List<double>();
        List<double> Xlist = new List<double>();
        List<ExtremPoint> pointList = new List<ExtremPoint>();

        Graphics g;

        const int maxX = 549;
        const int maxY = 369;
        double QestmaxX=0;
        double QestmaxY=0;
        double QestminX=0;
        double QestminY=0;
        public Form1()
        {
            InitializeComponent();
            g = Graph.CreateGraphics();
            //Clear();
        }

        private void Clear()
        {
            g.Clear(Color.White);
        }

        public static int Map(double count, double min, double max, int maxGraf)
        {
            int i= (int)((count - min) / (max - min) * (double)maxGraf);
            if (i > maxGraf) return maxGraf;
            if (i<0 ) return 0;

            return i;
        }
        public static double Map(double count, double minOut, double maxOut, double minIn , double maxIn)
        {
            double i = (count - minOut) / (maxOut - minOut) * (double)maxIn + minIn;
            
            return i;
        }
        private void DrawPoint(double x, double y,int size=1)
        {
            int grafX= Map(x, QestminX, QestmaxX, maxX);
            int grafY = Map(y, QestminY, QestmaxY, maxY);

            g.FillRectangle(Brushes.Black, grafX, maxY-grafY, size, size);
        }
        private void DrawPoint(double x, double y, int size , Brush colors)
        {
            int grafX = Map(x, QestminX, QestmaxX, maxX);
            int grafY = Map(y, QestminY, QestmaxY, maxY);

            g.FillRectangle(colors, grafX, maxY - grafY, size, size);
        }
        private void DrawGrapf(double A, double B, double C)
        {
            double d = (QestmaxX - QestminX) / ((double)maxX);

            for (double x= QestminX;x<= QestmaxX;x+=d)
            {
                double y = x * x * A + x * B + C;
                DrawPoint(x,y);
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            Clear();
            
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            double X ;
            double Y ;
            if (QestmaxX == QestminX)
            {
                 X = e.X;
                 Y = e.Y;
            }
            else
            {
                X = Map(e.X,(double)0, (double)maxX, QestminX,QestmaxX);
                Y = Map(e.Y, (double)maxY, (double)0, QestminY, QestmaxY);
            }

            CoordinateLabel.Text = $"X = {Math.Round(X, 3)};\t Y = {Math.Round(Y, 3)};" ;
        }


        int accuracy = 5;

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                Xlist.Clear();
                Ylist.Clear();
            var points = new CPoint[coordinateGridView.Rows.Count-1];

                for (int i = 0; i < coordinateGridView.Rows.Count - 1; ++i)
                {
                    //Добавляем строку, указывая значения каждой ячейки по имени (можно использовать индекс 0, 1, 2 вместо имен)
                    string tx = coordinateGridView["XColumn", i].Value.ToString();
                    tx = tx.Replace(".", ",");
                    string ty = coordinateGridView["YColumn", i].Value.ToString();
                    ty = ty.Replace(".", ",");

                //интерполяция Лагранжем
                    Xlist.Add(Convert.ToDouble(tx));
                    Ylist.Add(Convert.ToDouble(ty));

                //сплайн
                points[i] = new CPoint(Convert.ToDouble(tx), Convert.ToDouble(ty));

                }
            _model = new CSpline(points);
            _model.GenerateSplines();
            #region находим минимальные и максимальные значения для граффика
            QestmaxX = Xlist[0];
                 QestminX = Xlist[0];
                 QestmaxY = Ylist[0];
                 QestminY = Ylist[0];
                
                foreach (double x in Xlist)
                {
                    if (x > QestmaxX) QestmaxX = x;
                    if (x < QestminX) QestminX = x;
                }
                QestmaxX *= 1.05;
                QestminX *= 0.95;

                foreach (double y in Ylist)
                {
                    if (y > QestmaxY) QestmaxY = y;
                    if (y < QestminY) QestminY = y;
                }
                QestmaxY *= 1.05;
                QestminY *= 0.9;

            #endregion

                double sumX = 0;
                double sumX2 = 0;
                double sumX3 = 0;
                double sumX4 = 0;
                double sumY = 0;
                double sumXY = 0;
                double sumX2Y = 0;

                foreach (double x in Xlist)
                {
                    sumX += x;
                }
                foreach (double x in Xlist)
                {
                    sumX2 += x * x;
                }
                foreach (double x in Xlist)
                {
                    sumX3 += x * x * x;
                }
                foreach (double x in Xlist)
                {
                    sumX4 += x * x * x * x;
                }
                foreach (double y in Ylist)
                {
                    sumY += y;
                }
                for (int n = 0; n < Xlist.Count; n++)
                {
                    sumXY += Xlist[n] * Ylist[n];
                }
                for (int n = 0; n < Xlist.Count; n++)
                {
                    sumX2Y += (Xlist[n] * Xlist[n]) * Ylist[n];
                }
                for (int n = 0; n < Xlist.Count; n++)
                {

                    DrawPoint(Xlist[n], Ylist[n], 6);
                }

                textBox1.Text = "";
                textBox1.Text += $"\u03B2\u2080*{Xlist.Count}+\u03B2\u2081*{Math.Round(sumX, accuracy)}+\u03B2\u2081*{Math.Round(sumX2, accuracy)}= {sumY}\r\n ";
                textBox1.Text += $"\u03B2\u2080*{Math.Round(sumX, accuracy)}+\u03B2\u2081*{Math.Round(sumX2, accuracy)}+\u03B2\u2081*{Math.Round(sumX3, accuracy)}= {Math.Round(sumXY, accuracy)}\r\n ";
                textBox1.Text += $"\u03B2\u2080*{Math.Round(sumX2, accuracy)}+\u03B2\u2081*{Math.Round(sumX3, accuracy)}+\u03B2\u2082*{Math.Round(sumX4, accuracy)}= {Math.Round(sumX2Y, accuracy)}\r\n ";

                double delta = Xlist.Count * sumX2 * sumX4 + 2 * sumX * sumX3 * sumX2 - sumX2 * sumX2 * sumX2
                                - Xlist.Count * sumX3 * sumX3 - sumX * sumX * sumX4;
                textBox1.Text += $"\u0394 is {Math.Round(delta, accuracy)}\r\n ";
                double deltaB0 = sumY * sumX2 * sumX4 + sumX * sumX3 * sumX2Y + sumXY * sumX3 * sumX2
                                  - sumX2Y * sumX2 * sumX2 - sumX3 * sumX3 * sumY - sumXY * sumX * sumX4;
                double B0 = deltaB0 / delta;
                textBox1.Text += $"\u03B2\u2080 = {Math.Round(B0, accuracy)}\r\n ";
                double deltaB1 = Xlist.Count * sumXY * sumX4 + sumY * sumX3 * sumX2 + sumX * sumX2Y * sumX2 - sumX2 * sumXY * sumX2 - sumX2Y * sumX3 * Xlist.Count - sumX * sumY * sumX4;
                double B1 = deltaB1 / delta;
                textBox1.Text += $"\u03B2\u2081 = {Math.Round(B1, accuracy)}\r\n ";
                double deltaB2 = Xlist.Count * sumX2 * sumX2Y + sumXY * sumX * sumX2 + sumX * sumX3 * sumY - sumX2 * sumX2 * sumY - sumX3 * sumXY * Xlist.Count - sumX * sumX * sumX2Y;
                double B2 = deltaB2 / delta;
                textBox1.Text += $"\u03B2\u2082 = {Math.Round(B2, accuracy)}\r\n ";
                double tempSumm = 0;
                for (int i = 0; i < Xlist.Count; i++)
                {
                    tempSumm += (Ylist[i] - B0 - B1 * Xlist[i] - B2 * Xlist[i] * Xlist[i]) * (Ylist[i] - B0 - B1 * Xlist[i] - B2 * Xlist[i] * Xlist[i]);
                }
                double tempSigma = (tempSumm / (Xlist.Count - 1));
                double sigma = Math.Sqrt(tempSigma);
                textBox1.Text += $"\u03A3= {Math.Round(sigma, accuracy)}\r\n ";
                textBox1.Text += $" уравнение кривой:  y={Math.Round(B0, accuracy)}+{Math.Round(B1, accuracy)}*x+{Math.Round(B2, accuracy)}*x^2\r\n ";
                DrawGrapf(B2,B1,B0);
                DrawGrapf(Xlist, Ylist);
                DrawGrapf(_model);
        }
            catch(Exception ex)
            {
                textBox1.Text = ex.Message;
            }
}

        private void panel1_MouseClick(object sender, MouseEventArgs e)
        {
            g.FillRectangle(Brushes.Black, e.X, e.Y, 3, 3);
        }

        private void coordinateGridView_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            labelN.Text = $"n = {coordinateGridView.Rows.Count - 1}";
        }


        private void DrawGrapf( List<double>  xValues, List<double> yValues)
        {
            double d = (QestmaxX - QestminX) / ((double)maxX);

            for (double x = QestminX; x <= QestmaxX; x += d)
            {
                double y = InterpolateLagrangePolynomial(x, xValues.ToArray(), yValues.ToArray(), xValues.Count);
                DrawPoint(x, y, 1, Brushes.Red);
            }
        }

        private void DrawGrapf(CSpline spline)
        {

            double d = (QestmaxX - QestminX) / ((double)maxX);

            for (double x = QestminX; x <= QestmaxX; x += d)
            {
                DrawPoint(x, spline.GetY(x), 1, Brushes.Blue);
            }
        }

        static double InterpolateLagrangePolynomial(double x, double[] xValues, double[] yValues, int size)
            {
                double lagrangePol = 0;

                for (int i = 0; i < size; i++)
                {
                    double basicsPol = 1;
                    for (int j = 0; j < size; j++)
                    {
                        if (j != i)
                        {
                            basicsPol *= (x - xValues[j]) / (xValues[i] - xValues[j]);
                        }
                    }
                    lagrangePol += basicsPol * yValues[i];
                }

                return lagrangePol;
        }



        private void button1_Click(object sender, EventArgs e)
        {
            Clear();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {

        }
    }
}
