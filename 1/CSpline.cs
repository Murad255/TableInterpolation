using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FourQvestion
{
    class CPoint
    {
        public double X { get; }
        public double Y { get; }

        public double Df { get; set; }
        public double Ddf { get; set; }

        public CPoint(double x, double y)
        {
            X = x;
            Y = y;
        }
    }


    class CSplineSubinterval
    {
        public double A { get; }
        public double B { get; }
        public double C { get; }
        public double D { get; }

        private readonly CPoint _p1;
        private readonly CPoint _p2;

        public CSplineSubinterval(CPoint p1, CPoint p2, double df, double ddf)
        {
            _p1 = p1;
            _p2 = p2;

            B = ddf;
            C = df;
            D = p1.Y;
            A = (_p2.Y - B * Math.Pow(_p2.X - _p1.X, 2) - C * (_p2.X - _p1.X) - D) / Math.Pow(_p2.X - _p1.X, 3);
        }

        public double F(double x)
        {
            return A * Math.Pow(x - _p1.X, 3) + B * Math.Pow(x - _p1.X, 2) + C * (x - _p1.X) + D;
        }

        public double Df(double x)
        {
            return 3 * A * Math.Pow(x - _p1.X, 2) + 2 * B * (x - _p1.X) + C;
        }

        public double Ddf(double x)
        {
            return 6 * A * (x - _p1.X) + 2 * B;
        }

        public bool IncludInInterval(double x)
        {

            if( (_p2.X > _p1.X)&& (x >= _p1.X && x <= _p2.X)) return true; 
            else if ((_p2.X < _p1.X) &&(x <= _p1.X && x >= _p2.X)) return true; 
            return false;
        }
    }


    class CSpline
    {
        private readonly CPoint[] _points;
        private readonly CSplineSubinterval[] _splines;

        public double Df1
        {
            get { return _points[0].Df; }
            set { _points[0].Df = value; }
        }
        public double Ddf1
        {
            get { return _points[0].Ddf; }
            set { _points[0].Ddf = value; }
        }
        public double Dfn
        {
            get { return _points[_points.Length - 1].Df; }
            set { _points[_points.Length - 1].Df = value; }
        }
        public double Ddfn
        {
            get { return _points[_points.Length - 1].Ddf; }
            set { _points[_points.Length - 1].Ddf = value; }
        }

        public CSpline(CPoint[] points)
        {
            _points = points;
            _splines = new CSplineSubinterval[points.Length - 1];
        }

        public void GenerateSplines()
        {
            double xx = _points[1].X - _points[0].X;
            double yy = _points[1].Y - _points[0].Y;

            Df1 = Math.Asin(yy / Math.Sqrt(xx * xx + yy * yy));
            // if ((_points[_points.Length - 1].Y - _points[_points.Length - 2].Y) != 0)
            double xx2 = _points[_points.Length - 1].X - _points[_points.Length - 2].X;
            double yy2 = _points[_points.Length - 1].Y - _points[_points.Length - 2].Y;


            Dfn = Math.Asin(yy2 / Math.Sqrt(xx2 * xx2 + yy2 * yy2));
            const double x1 = 0;
            var y1 = BuildSplines(x1);
            const double x2 = 10;
            var y2 = BuildSplines(x2);

            _points[0].Ddf = -y1 * (x2 - x1) / (y2 - y1);

            BuildSplines(_points[0].Ddf);

            _points[_points.Length - 1].Ddf = _splines[_splines.Length - 1].Ddf(_points[_points.Length - 1].X);
        }

        private double BuildSplines(double ddf1)
        {
            double df = _points[0].Df, ddf = ddf1;
            for (var i = 0; i < _splines.Length; i++)
            {
                _splines[i] = new CSplineSubinterval(_points[i], _points[i + 1], df, ddf);

                df = _splines[i].Df(_points[i + 1].X);
                ddf = _splines[i].Ddf(_points[i + 1].X);

                if (i < _splines.Length - 1)
                {
                    _points[i + 1].Df = df;
                    _points[i + 1].Ddf = ddf;
                }
            }
            return df - Dfn;
        }

        public double GetY(double x)
        {
            foreach(CSplineSubinterval point in _splines)
            {
                if (point.IncludInInterval(x)) return point.F(x);
            }
            return 0;
        }
    }
}
