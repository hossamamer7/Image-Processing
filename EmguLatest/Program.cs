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
        static List<Vector2> Ordering(List<Vector2> vertex)
        {
            List<Vector2> OrderedList = new List<Vector2>();
            OrderedList = (List<Vector2>)vertex.OrderBy(p => p.X).ToList();
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
            DxfDocument dxfDocument = new DxfDocument();

            string fileName = "e://new.jpg";
            Image<Gray, byte> imgo = new Image<Gray, byte>(fileName);
            Image<Gray, byte> img = ~imgo;  //////////////////////////////////

            Image<Gray, byte> imgOut = img.Convert<Gray, byte>()
                .ThresholdBinary(new Gray(100), new Gray(255))
                ;

            Image<Gray, byte> im = new Image<Gray, byte>(img.Width, img.Height);

            //Find Contours && Lines from contours :
            VectorOfVectorOfPoint contors = new VectorOfVectorOfPoint();
            Mat hier = new Mat();

            CvInvoke.FindContours(imgOut, contors, hier, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
            CvInvoke.DrawContours(im, contors, -1, new MCvScalar(255, 255, 255));

            List<Vector3> vertices = new List<Vector3>();

            for (int i = 0; i < contors.Size; i++)
            {
                for (int j = 0; j < contors[i].Size; j++)
                {
                    vertices.Add(new Vector3(contors[i][j].X, contors[i][j].Y, 0));
                }
            }

            var query = vertices.GroupBy(x => x)   ////Repeated Points
           .Where(g => g.Count() > 1)
           .Select(y => y.Key)
           .ToList();

            var unique = vertices.Except(query).ToList();
            
           
            im.Save("e://planDraft5.bmp");

            var maxX = contors.ToArrayOfArray().ToList().SelectMany(i => i).Max(p => p.X);
            var minX = contors.ToArrayOfArray().ToList().SelectMany(i => i).Min(p => p.X);
            var maxY = contors.ToArrayOfArray().ToList().SelectMany(i => i).Max(p => p.Y);
            var minY = contors.ToArrayOfArray().ToList().SelectMany(i => i).Min(p => p.Y);

            var contorsList_Min_Y = contors.ToArrayOfArray().ToList()
                .Where(z => z.ToList().Any(p => p.Y == minY))
                .Where(p => p.Length > 4).ToList();

            var contorsList_Max_Y = contors.ToArrayOfArray().ToList()
                .Where(z => z.ToList().Any(p => p.Y == maxY))
                .Where(p => p.Length > 4).ToList();

            var contorsList_Max_X = contors.ToArrayOfArray().ToList()
                .Where(z => z.ToList().Any(p => p.X == maxX))
                .Where(p => p.Length > 4).ToList();

            var contorsList_Min_X = contors.ToArrayOfArray().ToList()
                .Where(z => z.ToList().Any(p => p.X == minX))
                .Where(p => p.Length > 4).ToList();
            Image<Gray, byte> ims = new Image<Gray, byte>(img.Width, img.Height);
           
            foreach (var p in contorsList_Max_X)
            {
                foreach(var pt in p)
                {
                    CvInvoke.Circle(ims, pt, 0, new MCvScalar(255, 255, 255), 1);
                }
            }

            ims.Save("e://planDraft13.bmp");

            //List<Point> Starts_HZ = new List<Point>();
            //List<Point> Ends_HZ = new List<Point>();

            //foreach (var p in contorsList_Max_X)
            //{
            //    Point P1 = p.OrderBy(p0 => p0.X).First();
            //    Starts_HZ.Add(P1);

            //}
            //for (int i = 0; i < Starts_HZ.Count; i++)
            //{
            //    Console.WriteLine(Starts_HZ[i].X.ToString() + " , " + Starts_HZ[i].Y.ToString());
            //}
            //Console.WriteLine("///////////////////////////////////////////");
            //foreach (var p in contorsList_Min_X)
            //{
            //    Point P1 = p.OrderBy(p0 => p0.X).Last();
            //    Ends_HZ.Add(P1);
            //}
            //for (int i = 0; i < Ends_HZ.Count; i++)
            //{
            //    Console.WriteLine(Ends_HZ[i].X.ToString() + " , " + Ends_HZ[i].Y.ToString());
            //}
            //List<Point> Ends_Mod = new List<Point>();
            //for (int i = 0; i < Starts_HZ.Count; i++)
            //{
            //    for (int j = 0; j < Ends_HZ.Count; j++)
            //    {
            //        if (Starts_HZ[i].Y == Ends_HZ[j].Y)
            //        {
            //            Ends_Mod.Add(Ends_HZ[j]);
            //        }
            //    }
            //}

            //List<Vector2> Start_HZ_Points = new List<Vector2>();
            //List<Vector2> End_HZ_Points = new List<Vector2>();

            //for (int i = 0; i < Starts_HZ.Count; i++)
            //{
            //    Start_HZ_Points.Add(new Vector2(Starts_HZ[i].X, Starts_HZ[i].Y * -1));
            //}

            //for (int i = 0; i < Ends_Mod.Count; i++)
            //{
            //    End_HZ_Points.Add(new Vector2(Ends_Mod[i].X, Ends_Mod[i].Y * -1));
            //}

            //for (int i = 0; i < Start_HZ_Points.Count; i++)
            //{
            //    Line L = new Line(Start_HZ_Points[i], End_HZ_Points[i]);
            //    L.Linetype = Linetype.Center;
            //    L.Layer = new Layer("Grids");
            //    L.Layer.Color = AciColor.Red;
            //    dxfDocument.AddEntity(L);
            //}

            //List<Point> Starts_VL = new List<Point>();
            //List<Point> Ends_VL = new List<Point>();
            //List<Point> Starts_VL2 = new List<Point>();
            //List<Point> Starts_VL3 = new List<Point>();

            //foreach (var p in contorsList_Max_Y)
            //{
            //    Point P1 = p.OrderBy(p0 => p0.Y).First();
            //    Starts_VL.Add(P1);
            //}
            //for (int i = 0; i < Starts_VL.Count / 2; i++)
            //{
            //    Starts_VL2.Add(Starts_VL[i]);
            //}
            //Starts_VL3 = Starts_VL.Except(Starts_VL2).ToList();


            //foreach (var p in contorsList_Min_Y)
            //{
            //    Point P1 = p.OrderBy(p0 => p0.Y).Last();
            //    Ends_VL.Add(P1);
            //}

            //List<Vector2> Start_VL_Points = new List<Vector2>();
            //List<Vector2> End_VL_Points = new List<Vector2>();

            //for (int i = 0; i < Starts_VL3.Count; i++)
            //{
            //    Start_VL_Points.Add(new Vector2(Starts_VL3[i].X, Starts_VL3[i].Y * -1));
            //}

            //for (int i = 0; i < Ends_VL.Count; i++)
            //{
            //    End_VL_Points.Add(new Vector2(Ends_VL[i].X, Ends_VL[i].Y * -1));
            //}

            //for (int i = 0; i < Start_VL_Points.Count; i++)
            //{
            //    Line L = new Line(Start_VL_Points[i], End_VL_Points[i]);
            //    L.Linetype = Linetype.Center;
            //    L.Layer = new Layer("Grids");
            //    L.Layer.Color = AciColor.Red;
            //    dxfDocument.AddEntity(L);
            //}

            //List<Vector2> Centers1 = new List<Vector2>();
            //List<Vector2> Centers2 = new List<Vector2>();
            //List<Vector2> Centers3 = new List<Vector2>();
            //List<Vector2> Centers4 = new List<Vector2>();

            //for (int i = 0; i < Start_HZ_Points.Count; i++)
            //{
            //    Centers1.Add(new Vector2(Start_HZ_Points[i].X + 5, Start_HZ_Points[i].Y));
            //}

            //for (int i = 0; i < Start_HZ_Points.Count; i++)
            //{
            //    Circle C = new Circle(Centers1[i], 5);
            //    C.Layer = new Layer("Bubbles");
            //    C.Layer.Color = AciColor.Red;
            //    dxfDocument.AddEntity(C);
            //}

            //for (int i = 0; i < End_HZ_Points.Count; i++)
            //{
            //    Centers2.Add(new Vector2(End_HZ_Points[i].X - 5, End_HZ_Points[i].Y));
            //}

            //for (int i = 0; i < End_HZ_Points.Count; i++)
            //{
            //    Circle C = new Circle(Centers2[i], 5);
            //    C.Layer = new Layer("Bubbles");
            //    C.Layer.Color = AciColor.Red;
            //    dxfDocument.AddEntity(C);
            //}

            //for (int i = 0; i < Start_VL_Points.Count; i++)
            //{
            //    Centers3.Add(new Vector2(Start_VL_Points[i].X, Start_VL_Points[i].Y - 5));
            //}

            //for (int i = 0; i < Start_VL_Points.Count; i++)
            //{
            //    Circle C = new Circle(Centers3[i], 5);
            //    C.Layer = new Layer("Bubbles");
            //    C.Layer.Color = AciColor.Red;
            //    dxfDocument.AddEntity(C);
            //}

            //for (int i = 0; i < End_VL_Points.Count; i++)
            //{
            //    Centers4.Add(new Vector2(End_VL_Points[i].X, End_VL_Points[i].Y + 5));
            //}

            //for (int i = 0; i < End_VL_Points.Count; i++)
            //{
            //    Circle C = new Circle(Centers4[i], 5);
            //    C.Layer = new Layer("Bubbles");
            //    C.Layer.Color = AciColor.Red;
            //    dxfDocument.AddEntity(C);
            //}

            //dxfDocument.DrawingVariables.AcadVer = DxfVersion.AutoCad2010;
            //dxfDocument.Save("e://Grids_2.dxf");

        }
    }
}
