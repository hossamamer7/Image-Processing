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
            DxfDocument dxfDocument = new DxfDocument();

            string fileName = "d://im/plan.PNG";
            Image<Gray, byte> imgo = new Image<Gray, byte>(fileName);
            Image<Gray, byte> img = ~imgo;  //////////////////////////////////

            Image<Gray, byte> imgOut = img.Convert<Gray, byte>()
                .ThresholdBinary(new Gray(0), new Gray(255))
                .Canny(0, 255);

            Image<Gray, byte> im = new Image<Gray, byte>(img.Width, img.Height);
            Image<Gray, byte> imgS = new Image<Gray, byte>(img.Width, img.Height);


            //Find Contours && Lines from contours :
            VectorOfVectorOfPoint contors = new VectorOfVectorOfPoint();
            Mat hier = new Mat();

            VectorOfVectorOfPoint contors_M = new VectorOfVectorOfPoint();
            Mat hier_M = new Mat();

            CvInvoke.FindContours(imgOut, contors, hier, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
            
            List<Vector3> vertices = new List<Vector3>();
            //List<Point> vertices2D = new List<Point>();
            for (int i = 0; i < contors.Size; i++)
            {
                for (int j = 0; j < contors[i].Size; j++)
                {
                    vertices.Add(new Vector3(contors[i][j].X, contors[i][j].Y, 0));
                    //vertices2D.Add(new Point(contors[i][j].X, contors[i][j].Y));
                }
            }

            CvInvoke.DrawContours(imgS, contors, -1, new MCvScalar(255, 255, 255));

            var query = vertices.GroupBy(x => x)   ////Repeated Points
              .Where(g => g.Count() > 1)
              .Select(y => y.Key)
              .ToList();
            var verticesWithoutRepeating = vertices.Distinct().ToList();

            for (int i = 0; i < query.Count; i++)
            {
                System.Drawing.Point p = new System.Drawing.Point((int)query.ElementAt(i).X, (int)query.ElementAt(i).Y);
                CvInvoke.Circle(imgS, p, 0, new MCvScalar(0, 0, 0), 1);
            }

            imgS.Save("d://im/plan121.bmp");

            CvInvoke.FindContours(imgS, contors_M, hier_M, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);


            var maxX = contors_M.ToArrayOfArray().ToList().SelectMany(i => i).Max(p => p.X);
            var minX = contors_M.ToArrayOfArray().ToList().SelectMany(i => i).Min(p => p.X);
            var maxY = contors_M.ToArrayOfArray().ToList().SelectMany(i => i).Max(p => p.Y);
            var minY = contors_M.ToArrayOfArray().ToList().SelectMany(i => i).Min(p => p.Y);

            var contorsList = contors_M.ToArrayOfArray().ToList()
                .Where(z=>z.ToList().Any(p=>p.Y== minY))
                .Where(p=>p.Length>4).ToList();
            

            var unique = vertices.Except(query).ToList();
            #region MyRegion
            //List<Vector3> repeated = new List<Vector3>();



            //for (int a = 0; a < contors.Size; a++)
            //{
            //    for (int i = 0; i < contors[a].Size; i++)
            //    {
            //        Vector3 basePoint = vertices[i];
            //        for (int j = 0; j < contors[a].Size; j++)
            //        {
            //            if (i == j)
            //            {
            //                continue;
            //            }
            //            else
            //            {
            //                if (vertices[i] == vertices[j])
            //                {
            //                    repeated.Add(new Vector3(vertices[j].X, vertices[j].Y, 0));
            //                }
            //            }
            //        }
            //    } 
            //}
            //Stack<Vector3> Points = new Stack<Vector3>();

            //for (int i = 0; i < repeated.Count; i++)
            //{
            //    if (i == repeated.Count - 1)
            //    {
            //        Points.Push(repeated[repeated.Count - 1]);
            //        break;
            //    }
            //    if (repeated[i + 1] != repeated[i])
            //    {
            //        Points.Push(repeated[i]);
            //    }
            //}
            //for (int i = 0; i < Points.Count; i++)
            //{
            //    Console.WriteLine(Points.ElementAt(i).X.ToString() + " , " + Points.ElementAt(i).Y.ToString());
            //} 
            #endregion
            ////for (int i = 0; i < unique.Count; i++)
            ////{
            ////    System.Drawing.Point p = new System.Drawing.Point((int)unique.ElementAt(i).X, (int)unique.ElementAt(i).Y);
            ////    CvInvoke.Circle(im, p, 0, new MCvScalar(255, 255, 255), 1);
            ////}
            foreach (var p in contorsList)
            {
                foreach (var pt in p)
                {
                    CvInvoke.Circle(im, pt, 0, new MCvScalar(255, 255, 255), 1);
                }
            }
            //for (int i = 0; i < query.Count; i++)
            //{
            //    System.Drawing.Point p = new System.Drawing.Point((int)query.ElementAt(i).X, (int)query.ElementAt(i).Y);
            //    CvInvoke.Circle(img, p, 0, new MCvScalar(51, 42, 42), 4);
            //}
            im.Save("d://im/plan123.bmp");

            #region
            
            
            
            
            
            
            
            
            //Image<Gray, byte> imgOut2 = img.Convert<Gray, byte>().ThresholdBinary(new Gray(100), new Gray(255));
            //VectorOfVectorOfPoint contors2 = new VectorOfVectorOfPoint();
            //Mat hier2 = new Mat();
            CvInvoke.FindContours(imgOut, contors, hier, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
            Console.WriteLine(contors.Size);

            int index = 0;
            VectorOfVectorOfPoint contors2 = new VectorOfVectorOfPoint();

            for (int i = 0; i < contors.Size; i++)
            {
                //VectorOfPoint approx = new VectorOfPoint();
                //double perimeter = CvInvoke.ArcLength(contors[i], true);
                //CvInvoke.ApproxPolyDP(contors[i], approx, 0.04 * perimeter, true);
                //if (approx.Size > 4)
                //{
                //    List<Vector3> verticess = new List<Vector3>();
                //    index = i;
                //    for (int j = 0; j < contors[index].Size; j++)
                //    {
                //        verticess.Add(new Vector3(contors[index][j].X, contors[index][j].Y * -1, 0));
                //    }
                //    contors2.Push(approx);
                //    CvInvoke.DrawContours(im, contors2, -1, new MCvScalar(255, 0, 0));
                //    im.Save("d://im/plan107.bmp");

                    //            List<Vector3> ord = Ordering(verticess);
                    //            Vector3 P1 = ord[0];
                    //            Vector3 P2 = ord[ord.Count - 1];
                    //            Line L = new Line(P1, P2);
                    //            string Message = GetLineType(ord);
                    //            verticess.Add(new Vector3(contors2[index][0].X, contors2[index][0].Y * -1, 0));
                    //            Polyline grids = new Polyline(verticess);
                    //            grids.Layer = new Layer("Grids");
                    //            grids.Layer.Color = AciColor.Blue;
                    //            dxfDocument.AddEntity(grids);
                    #region MyRegion
                    //            //if (Message == "Vertical Line")
                    //            //{
                    //            //    VerticalLines.Add(L);
                    //            //}
                    //            //else if (Message == "Horizontal Line")
                    //            //{
                    //            //    HorizontalLines.Add(L);
                    //            //} 
                    #endregion
                    //        }

                    //        if (approx.Size > 3)
                    //        {
                    //            List<Vector3> verticess = new List<Vector3>();
                    //            index = i;
                    //            for (int j = 0; j < contors2[index].Size; j++)
                    //            {
                    //                verticess.Add(new Vector3(contors2[index][j].X, contors2[index][j].Y * -1, 0));
                    //            }
                    //            verticess.Add(new Vector3(contors2[index][0].X, contors2[index][0].Y * -1, 0));
                    //            Polyline Circle = new Polyline(verticess);
                    //            Circle.Layer = new Layer("Bubbles");
                    //            Circle.Layer.Color = AciColor.Red;
                    //            dxfDocument.AddEntity(Circle);
                    //        }
                //}
            }
            #endregion
            //    dxfDocument.DrawingVariables.AcadVer = DxfVersion.AutoCad2010;
            //    dxfDocument.Save("d://dxfs/draftIntersection2.dxf");  /////////////
        }
    }
}

