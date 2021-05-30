using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Emgu;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using netDxf;
using netDxf.Entities;
using netDxf.Header;
using netDxf.Tables;
using Point = System.Drawing.Point;
using netDxf.Blocks;

namespace slabDraft
{
    class Program
    {
        static double GetLength(Vector3 p1, Vector3 p2)
        {
            double LEngth = Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
            return LEngth;
        }

        //Function to calculate Mile of a specific line by its start and end points :
        static double GetMile(Vector3 p1, Vector3 p2)
        {
            double Mile = (p1.Y - p2.Y) / (p1.X - p2.X);
            return Mile;
        }
        //Function to handle the length of the line by the CAD units :
        static Vector3 GetPoint(double distance, double mile)
        {
            Vector3 Point = new Vector3(0, 0, 0);
            Point.Z = 0;
            Point.X = Math.Sqrt(distance * distance / (1 + mile * mile));
            Point.Y = Point.X * mile;
            return Point;
        }
        static string GetLineType(List<Vector3> OrderedList)
        {
            if (OrderedList[0].X == OrderedList[OrderedList.Count - 1].X)
            {
                return "Vertical Line";
            }
            else if (OrderedList[0].Y == OrderedList[OrderedList.Count - 1].Y)
            {
                return "Horizontal Line";
            }
            else
            {
                return "Not Valid";
            }
        }

        //Function to return ordered list of points 
        static List<Vector3> Ordering(List<Vector3> vertex)
        {
            List<Vector3> OrderedList = new List<Vector3>();
            OrderedList = (List<Vector3>)vertex.OrderBy(p => p.X).ToList();
            return OrderedList;
        }
        static List<Vector3> Ordering2(List<Vector3> vertex)
        {
            List<Vector3> OrderedList = new List<Vector3>();
            OrderedList = (List<Vector3>)vertex.OrderBy(p => p.Y).ToList();
            return OrderedList;
        }

        static void Main(string[] args)
        {
            //Creating a DXF File :
            DxfDocument dxfDocument = new DxfDocument();

            //Reading Information from the image :
            string fileName = "e://NewNew.PNG";
            Image<Gray, byte> imgo = new Image<Gray, byte>(fileName);
            Image<Gray, byte> img = ~imgo;  //////////////////////////////////
            Image<Gray, byte> imgOut = img.Convert<Gray, byte>()
                .ThresholdBinary(new Gray(100), new Gray(255))
                ;
            Image<Gray, byte> im = new Image<Gray, byte>(img.Width, img.Height);
           
            //Find Contours && Lines from contours :
            VectorOfVectorOfPoint contors = new VectorOfVectorOfPoint();
            Mat hier = new Mat();
            CvInvoke.FindContours(imgOut, contors, hier, Emgu.CV.CvEnum.RetrType.Ccomp, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
            CvInvoke.DrawContours(im, contors, -1, new MCvScalar(255, 255, 255));
          
            //Finding The Maximum Value of X,Y in each direction :
            var maxX = contors.ToArrayOfArray().ToList().SelectMany(i => i).Max(p => p.X);
            var minX = contors.ToArrayOfArray().ToList().SelectMany(i => i).Min(p => p.X);
            var maxY = contors.ToArrayOfArray().ToList().SelectMany(i => i).Max(p => p.Y);
            var minY = contors.ToArrayOfArray().ToList().SelectMany(i => i).Min(p => p.Y);
               
            //Find The Contour which contains the maximum value which calculated in the previous step &&Drawing The Bubbles:
           
            var contorsList_Min_Y = contors.ToArrayOfArray().ToList()
                .Where(z => z.ToList().Any(p => p.Y == minY ))
                .ToList();
            for (int i = 0; i < contorsList_Min_Y.Count; i++)
            {
                List<Vector3> Vertex = new List<Vector3>();
                for (int j = 0; j < contorsList_Min_Y[i].Length; j++)
                {
                    Vertex.Add(new Vector3(contorsList_Min_Y[i][j].X, contorsList_Min_Y[i][j].Y * -1, 0));
                    Polyline P = new Polyline(Vertex);
                    P.Layer = new Layer("Bubbles");
                    dxfDocument.AddEntity(P);
                }
            }

            var contorsList_Max_Y = contors.ToArrayOfArray().ToList()
                .Where(z => z.ToList().Any(p => p.Y == maxY ))
                .ToList();
            for (int i = 0; i < contorsList_Max_Y.Count; i++)
            {
                List<Vector3> Vertex = new List<Vector3>();
                for (int j = 0; j < contorsList_Max_Y[i].Length; j++)
                {
                    Vertex.Add(new Vector3(contorsList_Max_Y[i][j].X, contorsList_Max_Y[i][j].Y * -1, 0));
                    Polyline P = new Polyline(Vertex);
                    P.Layer = new Layer("Bubbles");
                    dxfDocument.AddEntity(P);
                }
            }

            var contorsList_Max_X = contors.ToArrayOfArray().ToList()
                .Where(z => z.ToList().Any(p => p.X == maxX))
                .ToList();
            for (int i = 0; i < contorsList_Max_X.Count; i++)
            {
                List<Vector3> Vertex = new List<Vector3>();
                for (int j = 0; j < contorsList_Max_X[i].Length; j++)
                {
                    Vertex.Add(new Vector3(contorsList_Max_X[i][j].X, contorsList_Max_X[i][j].Y * -1, 0));
                    Polyline P = new Polyline(Vertex);
                    P.Layer = new Layer("Bubbles");
                    dxfDocument.AddEntity(P);
                }
            }

            var contorsList_Min_X = contors.ToArrayOfArray().ToList()
                .Where(z => z.ToList().Any(p => p.X == minX))
                .ToList();
            for (int i = 0; i < contorsList_Min_Y.Count; i++)
            {
                List<Vector3> Vertex = new List<Vector3>();
                for (int j = 0; j < contorsList_Min_X[i].Length; j++)
                {
                    Vertex.Add(new Vector3(contorsList_Min_X[i][j].X, contorsList_Min_X[i][j].Y * -1, 0));
                    Polyline P = new Polyline(Vertex);
                    P.Layer = new Layer("Bubbles");
                    dxfDocument.AddEntity(P);
                }
            }

            //Creating an intermmideate image to help : 
            Image<Gray, byte> ims0 = new Image<Gray, byte>(img.Width, img.Height);
            
            //Deleting the contours of the maximum value-->Deleting the Bubbles
            foreach (var p in contorsList_Max_X)
            {
                foreach (var pt in p)
                {
                    CvInvoke.Circle(im, pt, 0, new MCvScalar(0, 0, 0), 7);
                }
            }
            
            foreach (var p in contorsList_Min_X)
            {
                foreach (var pt in p)
                {
                    CvInvoke.Circle(im, pt, 0, new MCvScalar(0, 0, 0), 7);
                }
            }
            
            foreach (var p in contorsList_Max_Y)
            {
                foreach (var pt in p)
                {
                    CvInvoke.Circle(im, pt, 0, new MCvScalar(0, 0, 0), 7);
                }
            }
   
            foreach (var p in contorsList_Min_Y)
            {
                foreach (var pt in p)
                {
                    CvInvoke.Circle(im, pt, 0, new MCvScalar(0, 0, 0), 7);
                }
            }
            //This steps for drawing the text of the bubbles then Deleting them from the image and Drawing them in the dxf
            VectorOfVectorOfPoint contors2 = new VectorOfVectorOfPoint();
            Mat hier2 = new Mat();

            CvInvoke.FindContours(im, contors2, hier2, Emgu.CV.CvEnum.RetrType.Ccomp, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
            CvInvoke.DrawContours(ims0, contors2, -1, new MCvScalar(255, 255, 255));
            
            var maxX2 = contors2.ToArrayOfArray().ToList().SelectMany(i => i).Max(p => p.X);
            var minX2 = contors2.ToArrayOfArray().ToList().SelectMany(i => i).Min(p => p.X);
            var maxY2 = contors2.ToArrayOfArray().ToList().SelectMany(i => i).Max(p => p.Y);
            var minY2 = contors2.ToArrayOfArray().ToList().SelectMany(i => i).Min(p => p.Y);

            var contorsList_Min_Y2 = contors2.ToArrayOfArray().ToList()
               .Where(z => z.ToList().Any(p => p.Y == minY2|| p.Y == minY2+1|| p.Y == minY2+2|| p.Y == minY2+3|| p.Y == minY2+4|| p.Y == minY2+5||
               p.Y == minY2+6|| p.Y == minY2+7|| p.Y == minY2+8
               || p.Y == minY2-1|| p.Y == minY2-2|| p.Y == minY2-3|| p.Y == minY2-4|| p.Y == minY2-5|| p.Y == minY2-6|| p.Y == minY2-7|| p.Y == minY2-8
               ))
               .ToList();

            for (int i = 0; i < contorsList_Min_Y2.Count; i++)
            {
                List<Vector3> Vertex = new List<Vector3>();
                for (int j = 0; j < contorsList_Min_Y2[i].Length; j++)
                {
                    Vertex.Add(new Vector3(contorsList_Min_Y2[i][j].X, contorsList_Min_Y2[i][j].Y * -1, 0));
                    Polyline P = new Polyline(Vertex);
                    P.Layer = new Layer("Bubbles Text");
                    dxfDocument.AddEntity(P);
                }
            }
            var contorsList_Max_Y2 = contors2.ToArrayOfArray().ToList()
                .Where(z => z.ToList().Any(p => p.Y == maxY2|| p.Y == maxY2+1|| p.Y == maxY2+2|| p.Y == maxY2+3|| p.Y == maxY2+4
                || p.Y == maxY2-1|| p.Y == maxY2-2|| p.Y == maxY2-3|| p.Y == maxY2-4 || p.Y == maxY2+5|| p.Y == maxY2+6|| p.Y == maxY2+7|| p.Y == maxY2+8
                || p.Y == maxY2-5|| p.Y == maxY2-6|| p.Y == maxY2-7|| p.Y == maxY2-8
                ))
                .ToList();
            for (int i = 0; i < contorsList_Max_Y2.Count; i++)
            {
                List<Vector3> Vertex = new List<Vector3>();
                for (int j = 0; j < contorsList_Max_Y2[i].Length; j++)
                {
                    Vertex.Add(new Vector3(contorsList_Max_Y2[i][j].X, contorsList_Max_Y2[i][j].Y * -1, 0));
                    Polyline P = new Polyline(Vertex);
                    P.Layer = new Layer("Bubbles Text");
                    dxfDocument.AddEntity(P);
                }
            }
            var contorsList_Max_X2 = contors2.ToArrayOfArray().ToList()
                .Where(z => z.ToList().Any(p => p.X == maxX2|| p.X == maxX2+1|| p.X == maxX2+2|| p.X == maxX2+3|| p.X == maxX2+4
                || p.X == maxX2-1|| p.X == maxX2-2|| p.X == maxX2-3|| p.X == maxX2-4 || p.X == maxX2+5|| p.X == maxX2+6|| p.X == maxX2+7|| p.X == maxX2+8
                || p.X == maxX2-5|| p.X == maxX2-6|| p.X == maxX2-7|| p.X == maxX2-8
                ))
                .ToList();
            for (int i = 0; i < contorsList_Max_X2.Count; i++)
            {
                List<Vector3> Vertex = new List<Vector3>();
                for (int j = 0; j < contorsList_Max_X2[i].Length; j++)
                {
                    Vertex.Add(new Vector3(contorsList_Max_X2[i][j].X, contorsList_Max_X2[i][j].Y * -1, 0));
                    Polyline P = new Polyline(Vertex);
                    P.Layer = new Layer("Bubbles Text");
                    dxfDocument.AddEntity(P);
                }
            }
            var contorsList_Min_X2 = contors2.ToArrayOfArray().ToList()
                .Where(z => z.ToList().Any(p => p.X == minX2|| p.X == minX2+1|| p.X == minX2+2|| p.X == minX2+3|| p.X == minX2+4
                || p.X == minX2-1|| p.X == minX2-2|| p.X == minX2-3|| p.X == minX2-4 || p.X == minX2+5|| p.X == minX2+6|| p.X == minX2+7|| p.X == minX2+8
                || p.X == minX2-5|| p.X == minX2-6|| p.X == minX2-7|| p.X == minX2-8
                ))
                .ToList();
            for (int i = 0; i < contorsList_Min_X2.Count; i++)
            {
                List<Vector3> Vertex = new List<Vector3>();
                for (int j = 0; j < contorsList_Min_X2[i].Length; j++)
                {
                    Vertex.Add(new Vector3(contorsList_Min_X2[i][j].X, contorsList_Min_X2[i][j].Y * -1, 0));
                    Polyline P = new Polyline(Vertex);
                    P.Layer = new Layer("Bubbles Text");
                    dxfDocument.AddEntity(P);
                }
            }
            foreach (var p in contorsList_Max_X2)
            {
                foreach (var pt in p)
                {
                    CvInvoke.Circle(ims0, pt, 7, new MCvScalar(0, 0, 0), 22);
                }
            }
            foreach (var p in contorsList_Min_X2)
            {
                foreach (var pt in p)
                {
                    CvInvoke.Circle(ims0, pt, 7, new MCvScalar(0, 0, 0), 22);
                }
            }
            foreach (var p in contorsList_Max_Y2)
            {
                foreach (var pt in p)
                {
                    CvInvoke.Circle(ims0, pt, 7, new MCvScalar(0, 0, 0), 22);
                }
            }
            foreach (var p in contorsList_Min_Y2)
            {
                foreach (var pt in p)
                {
                    CvInvoke.Circle(ims0, pt, 7, new MCvScalar(0, 0, 0), 22);
                }
            }

            //Drawing Grids :
            VectorOfVectorOfPoint contors3 = new VectorOfVectorOfPoint();
            Mat hier3 = new Mat();

            CvInvoke.FindContours(ims0, contors3, hier3, Emgu.CV.CvEnum.RetrType.Ccomp, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

            var maxX3 = contors3.ToArrayOfArray().ToList().SelectMany(i => i).Max(p => p.X);
            var minX3 = contors3.ToArrayOfArray().ToList().SelectMany(i => i).Min(p => p.X);
            var maxY3 = contors3.ToArrayOfArray().ToList().SelectMany(i => i).Max(p => p.Y);
            var minY3 = contors3.ToArrayOfArray().ToList().SelectMany(i => i).Min(p => p.Y);

            var contorsList_Min_X3 = contors3.ToArrayOfArray().ToList()
                 .Where(z => z.ToList().Any(p => p.X == minX3 || p.X == minX3 + 1 || p.X == minX3 + 2 || p.X == minX3 + 3 || p.X == minX3 + 4
                 || p.X == minX3 - 1 || p.X == minX3 - 2 || p.X == minX3 - 3 || p.X == minX3 - 4 || p.X == minX3 + 5 || p.X == minX3 + 6 || p.X == minX3 + 7 || p.X == minX3 + 8
                 || p.X == minX3 - 5 || p.X == minX3 - 6 || p.X == minX3 - 7 || p.X == minX3 - 8
                 ))
                 .ToList();


            var contorsList_Max_X3 = contors3.ToArrayOfArray().ToList()
               .Where(z => z.ToList().Any(p => p.X == maxX3 || p.X == maxX3 + 1 || p.X == maxX3 + 2 || p.X == maxX3 + 3 || p.X == maxX3 + 4
               || p.X == maxX3 - 1 || p.X == maxX3 - 2 || p.X == maxX3 - 3 || p.X == maxX3 - 4 || p.X == maxX3 + 5 || p.X == maxX3 + 6 || p.X == maxX3 + 7 || p.X == maxX3 + 8
               || p.X == maxX3 - 5 || p.X == maxX3 - 6 || p.X == maxX3 - 7 || p.X == maxX3 - 8
               ))
               .ToList();


            var contorsList_Max_Y3 = contors3.ToArrayOfArray().ToList()
                .Where(z => z.ToList().Any(p => p.Y == maxY3 || p.Y == maxY3 + 1 || p.Y == maxY3 + 2 || p.Y == maxY3 + 3 || p.Y == maxY3 + 4
                || p.Y == maxY3 - 1 || p.Y == maxY3 - 2 || p.Y == maxY3 - 3 || p.Y == maxY3 - 4 || p.Y == maxY3 + 5 || p.Y == maxY3 + 6 || p.Y == maxY3 + 7 || p.Y == maxY3 + 8
                || p.Y == maxY3 - 5 || p.Y == maxY3 - 6 || p.Y == maxY3 - 7 || p.Y == maxY3 - 8
                ))
                .ToList();

            var contorsList_Min_Y3 = contors3.ToArrayOfArray().ToList()
               .Where(z => z.ToList().Any(p => p.Y == minY3 || p.Y == minY3 + 1 || p.Y == minY3 + 2 || p.Y == minY3 + 3 || p.Y == minY3 + 4 || p.Y == minY3 + 5 ||
               p.Y == minY3 + 6 || p.Y == minY3 + 7 || p.Y == minY3 + 8
               || p.Y == minY3 - 1 || p.Y == minY3 - 2 || p.Y == minY3 - 3 || p.Y == minY3 - 4 || p.Y == minY3 - 5 || p.Y == minY3 - 6 || p.Y == minY3 - 7 || p.Y == minY3 - 8
               ))
               .ToList();

            List<Vector3> Start_HZ = new List<Vector3>();
            for(int i = 0; i < contorsList_Min_X3.Count; i++)
            {
                Start_HZ.Add(new Vector3(contorsList_Min_X3[i][0].X, contorsList_Min_X3[i][0].Y * -1, 0));
            }
            Start_HZ = Ordering2(Start_HZ);

            List<Vector3> End_HZ = new List<Vector3>();
            for (int i = 0; i < contorsList_Max_X3.Count; i++)
            {
                End_HZ.Add(new Vector3(contorsList_Max_X3[i][0].X, contorsList_Max_X3[i][0].Y * -1, 0));
            }
            End_HZ = Ordering2(End_HZ);

            for (int i = 0; i < Start_HZ.Count; i++)
            {
                Line L = new Line(Start_HZ[i], End_HZ[i]);
                L.Layer = new Layer("Horizontal Grids");
                L.Linetype = Linetype.Center;
                L.LinetypeScale = 40;
                dxfDocument.AddEntity(L);
            }

            List<Vector3> Start_VL = new List<Vector3>();
            for (int i = 0; i < contorsList_Min_Y3.Count; i++)
            {
                Start_VL.Add(new Vector3(contorsList_Min_Y3[i][0].X, contorsList_Min_Y3[i][0].Y * -1, 0));
            }
            Start_VL = Ordering(Start_VL);

            List<Vector3> End_VL = new List<Vector3>();
            for (int i = 0; i < contorsList_Max_Y3.Count; i++)
            {
                End_VL.Add(new Vector3(contorsList_Max_Y3[i][0].X, contorsList_Max_Y3[i][0].Y * -1, 0));
            }

            End_VL = Ordering(End_VL);
            for (int i = 0; i < Start_VL.Count; i++)
            {
                Line L = new Line(Start_VL[i], End_VL[i]);
                L.Layer = new Layer("Vertical Grids");
                L.Linetype = Linetype.Center;
                L.LinetypeScale = 40;
                dxfDocument.AddEntity(L);
            }
            dxfDocument.DrawingVariables.AcadVer = DxfVersion.AutoCad2010;
            dxfDocument.Save("e://Meda_Trial4.dxf");
        }
    }
}
