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
        #region I.Functions Region :
        //Function to return ordered list of points in Vector3 Formatting Type according to X coordinate:
        static List<Vector3> Ordering_VEC3_X(List<Vector3> vertex)
        {
            List<Vector3> OrderedList = new List<Vector3>();
            OrderedList = (List<Vector3>)vertex.OrderBy(p => p.X).ToList();
            return OrderedList;
        }
        //Function to return ordered list of points in Point Formatting Type according to X coordinate:
        static List<Point> Ordering_Points_X(List<Point> vertex)
        {
            List<Point> OrderedList = new List<Point>();
            OrderedList = (List<Point>)vertex.OrderBy(p => p.X).ToList();
            return OrderedList;
        }
        //Function to return ordered list of points in Vector3 Formatting Type according to Y coordinate:
        static List<Vector3> Ordering_VEC3_Y(List<Vector3> vertex)
        {
            List<Vector3> OrderedList = new List<Vector3>();
            OrderedList = (List<Vector3>)vertex.OrderBy(p => p.Y).ToList();
            return OrderedList;
        }
        //Function to return ordered list of points in Point Formatting Type according to Y coordinate:
        static List<Point> Ordering_Points_Y(List<Point> vertex)
        {
            List<Point> OrderedList = new List<Point>();
            OrderedList = (List<Point>)vertex.OrderBy(p => p.Y).ToList();
            return OrderedList;
        }
        #endregion

        static void Main(string[] args)
        {
            #region 1.Creating a DXF File :
            //1.Creating a DXF File :
            DxfDocument dxfDocument = new DxfDocument();
            #endregion

            #region 2.Images Variables :
            //2.Creating Images Variables :
            string Source_Image_Dir = @"e:\ITI-Graduation Project\ImageStr\0.Str_plan.PNG";
            Image<Gray, byte> Source_Image = new Image<Gray, byte>(Source_Image_Dir);
            Image<Gray, byte> Modified_Image = ~Source_Image;
            Image<Gray, byte> Converted_Image = Modified_Image.Convert<Gray, byte>().ThresholdBinary(new Gray(100), new Gray(255));
            Image<Gray, byte> Helping_Image_1 = new Image<Gray, byte>(Modified_Image.Width, Modified_Image.Height);
            Image<Gray, byte> Helping_Image_2 = new Image<Gray, byte>(Modified_Image.Width, Modified_Image.Height);
            Image<Gray, byte> Helping_Image_3 = new Image<Gray, byte>(Modified_Image.Width, Modified_Image.Height);
            #endregion

            #region 3.1st Image Processing :
            //3.First Image Processing Step By Reading All Contours From The Image :
            VectorOfVectorOfPoint All_Contours = new VectorOfVectorOfPoint();
            Mat Hier_LVL1 = new Mat();
            CvInvoke.FindContours(Converted_Image, All_Contours, Hier_LVL1, Emgu.CV.CvEnum.RetrType.Ccomp, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
            CvInvoke.DrawContours(Helping_Image_1, All_Contours, -1, new MCvScalar(255, 255, 255));
            CvInvoke.DrawContours(Helping_Image_3, All_Contours, -1, new MCvScalar(255, 255, 255));
            #endregion

            #region 4.Find Max Values of X,y For Bubbels :
            //4.Finding The Maximum Value of X, Y in each direction To Determine The Bubbles Ending Points :
            var max_X_Bubbles = All_Contours.ToArrayOfArray().ToList().SelectMany(i => i).Max(p => p.X);
            var min_X_Bubbles = All_Contours.ToArrayOfArray().ToList().SelectMany(i => i).Min(p => p.X);
            var max_Y_Bubbles = All_Contours.ToArrayOfArray().ToList().SelectMany(i => i).Max(p => p.Y);
            var min_Y_Bubbles = All_Contours.ToArrayOfArray().ToList().SelectMany(i => i).Min(p => p.Y);
            #endregion

            #region 5.Drawing Bubbels in Dxf and Removing it from Image :
            //5.Find The Contour which contains the maximum value in each direction which are calculated in the previous step &&Drawing The Bubbles in DXF && Removing them from the image:
            //5.1.Drawing of the top bubbles in the dxf file :
            var contorsList_Min_Y_Bubbles = All_Contours.ToArrayOfArray().ToList()
                .Where(z => z.ToList().Any(p => p.Y == min_Y_Bubbles))
                .ToList();
            for (int i = 0; i < contorsList_Min_Y_Bubbles.Count; i++)
            {
                List<Vector3> Vertex_Of_Top_Bubbles = new List<Vector3>();
                for (int j = 0; j < contorsList_Min_Y_Bubbles[i].Length; j++)
                {
                    Vertex_Of_Top_Bubbles.Add(new Vector3(contorsList_Min_Y_Bubbles[i][j].X, contorsList_Min_Y_Bubbles[i][j].Y * -1, 0));
                    Polyline Top_Bubbles = new Polyline(Vertex_Of_Top_Bubbles);
                    Top_Bubbles.Layer = new Layer("Bubbles");
                    dxfDocument.AddEntity(Top_Bubbles);
                }
            }
            dxfDocument.Save(@"e:\ITI-Graduation Project\DXF_Str\1.Top_Bubbles.dxf");

            //5.2.Deleting of the top bubbles from the image :
            VectorOfVectorOfPoint Contors_Of_Top_Bubbles = new VectorOfVectorOfPoint();
            for (int i = 0; i < contorsList_Min_Y_Bubbles.Count; i++)
            {
                VectorOfPoint Intermmideate_Vector = new VectorOfPoint();
                Intermmideate_Vector.Push(contorsList_Min_Y_Bubbles[i]);
                Contors_Of_Top_Bubbles.Push(Intermmideate_Vector);
            }
            CvInvoke.DrawContours(Helping_Image_1, Contors_Of_Top_Bubbles, -1, new MCvScalar(0, 0,0));
            Helping_Image_1.Save(@"e:\ITI-Graduation Project\ImageStr\1.Removing_Top_Bubbles.bmp");

            //5.3.Drawing of the bottom bubbles in the dxf file :
            var contorsList_Max_Y_Bubbles = All_Contours.ToArrayOfArray().ToList()
                .Where(z => z.ToList().Any(p => p.Y == max_Y_Bubbles))
                .ToList();
            for (int i = 0; i < contorsList_Max_Y_Bubbles.Count; i++)
            {
                List<Vector3> Vertex_Of_Bottom_Bubbles = new List<Vector3>();
                for (int j = 0; j < contorsList_Max_Y_Bubbles[i].Length; j++)
                {
                    Vertex_Of_Bottom_Bubbles.Add(new Vector3(contorsList_Max_Y_Bubbles[i][j].X, contorsList_Max_Y_Bubbles[i][j].Y * -1, 0));
                    Polyline Bottom_Bubbles = new Polyline(Vertex_Of_Bottom_Bubbles);
                    Bottom_Bubbles.Layer = new Layer("Bubbles");
                    dxfDocument.AddEntity(Bottom_Bubbles);
                }
            }
            dxfDocument.Save(@"e:\ITI-Graduation Project\DXF_Str\2.Bottom_Bubbles.dxf");

            //5.4.Deleting of the bottom bubbles from the image :
            VectorOfVectorOfPoint Contors_Of_Bottom_Bubbles = new VectorOfVectorOfPoint();
            for (int i = 0; i < contorsList_Max_Y_Bubbles.Count; i++)
            {
                VectorOfPoint Intermmideate_Vector = new VectorOfPoint();
                Intermmideate_Vector.Push(contorsList_Max_Y_Bubbles[i]);
                Contors_Of_Bottom_Bubbles.Push(Intermmideate_Vector);
            }
            CvInvoke.DrawContours(Helping_Image_1, Contors_Of_Bottom_Bubbles, -1, new MCvScalar(0, 0, 0));
            Helping_Image_1.Save(@"e:\ITI-Graduation Project\ImageStr\2.Removing_Bottom_Bubbles.bmp");

            //5.5.Drawing of the Right Bubbles in the DXF :
            var contorsList_Max_X_Bubbles = All_Contours.ToArrayOfArray().ToList()
                .Where(z => z.ToList().Any(p => p.X == max_X_Bubbles))
                .ToList();
            for (int i = 0; i < contorsList_Max_X_Bubbles.Count; i++)
            {
                List<Vector3> Vertex_Of_Right_Bubbles = new List<Vector3>();
                for (int j = 0; j < contorsList_Max_X_Bubbles[i].Length; j++)
                {
                    Vertex_Of_Right_Bubbles.Add(new Vector3(contorsList_Max_X_Bubbles[i][j].X, contorsList_Max_X_Bubbles[i][j].Y * -1, 0));
                    Polyline Right_Bubbles = new Polyline(Vertex_Of_Right_Bubbles);
                    Right_Bubbles.Layer = new Layer("Bubbles");
                    dxfDocument.AddEntity(Right_Bubbles);
                }
            }
            dxfDocument.Save(@"e:\ITI-Graduation Project\DXF_Str\3.Right_Bubbles.dxf");

            //5.6.Deleting of The Right bottoms from the image :
            VectorOfVectorOfPoint Contors_Of_Right_Bubbles = new VectorOfVectorOfPoint();
            for (int i = 0; i< contorsList_Max_X_Bubbles.Count; i++)
            {
                VectorOfPoint Intermmideate_Vector = new VectorOfPoint();
                Intermmideate_Vector.Push(contorsList_Max_X_Bubbles[i]);
                Contors_Of_Right_Bubbles.Push(Intermmideate_Vector);
            }
            CvInvoke.DrawContours(Helping_Image_1, Contors_Of_Right_Bubbles, -1, new MCvScalar(0, 0, 0));
            Helping_Image_1.Save(@"e:\ITI-Graduation Project\ImageStr\3.Removing_Right_Bubbles.bmp");

            //5.7.Drawing of The Left Bubbles in the DXF File :
            var contorsList_Min_X_Bubbles = All_Contours.ToArrayOfArray().ToList()
                .Where(z => z.ToList().Any(p => p.X == min_X_Bubbles))
                .ToList();
            for (int i = 0; i < contorsList_Min_X_Bubbles.Count; i++)
            {
                List<Vector3> Vertex_of_Left_Bubbles = new List<Vector3>();
                for (int j = 0; j < contorsList_Min_X_Bubbles[i].Length; j++)
                {
                    Vertex_of_Left_Bubbles.Add(new Vector3(contorsList_Min_X_Bubbles[i][j].X, contorsList_Min_X_Bubbles[i][j].Y * -1, 0));
                    Polyline Left_Bubbles = new Polyline(Vertex_of_Left_Bubbles);
                    Left_Bubbles.Layer = new Layer("Bubbles");
                    dxfDocument.AddEntity(Left_Bubbles);
                }
            }
            dxfDocument.Save(@"e:\ITI-Graduation Project\DXF_Str\4.Left_Bubbles.dxf");

            //5.8.Deleting The Left Bubbles from the image :
            VectorOfVectorOfPoint Contors_Of_Left_Bubbles = new VectorOfVectorOfPoint();
            for (int i = 0; i < contorsList_Min_X_Bubbles.Count; i++)
            {
                VectorOfPoint Intermmideate_Vector = new VectorOfPoint();
                Intermmideate_Vector.Push(contorsList_Min_X_Bubbles[i]);
                Contors_Of_Left_Bubbles.Push(Intermmideate_Vector);
            }
            CvInvoke.DrawContours(Helping_Image_1, Contors_Of_Left_Bubbles, -1, new MCvScalar(0, 0, 0));
            Helping_Image_1.Save(@"e:\ITI-Graduation Project\ImageStr\4.Removing_Left_Bubbles.bmp");
            #endregion

            #region 6.2nd Image Processing :
            ////6.Second Image Processing Step By Reading All Contours Without The Bubbles From The Image:
            VectorOfVectorOfPoint All_Contours_Without_Bubbles = new VectorOfVectorOfPoint();
            Mat Hier_LVL2 = new Mat();
            CvInvoke.FindContours(Helping_Image_1, All_Contours_Without_Bubbles, Hier_LVL2, Emgu.CV.CvEnum.RetrType.Ccomp, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
            CvInvoke.DrawContours(Helping_Image_2, All_Contours_Without_Bubbles, -1, new MCvScalar(255, 255, 255));

            ////6.1.Finding The Maximum Value of X, Y in each direction To Determine The Inside-Bubbles Text Ending Points :
            var max_X_Text = All_Contours_Without_Bubbles.ToArrayOfArray().ToList().SelectMany(i => i).Max(p => p.X);
            var min_X_Text = All_Contours_Without_Bubbles.ToArrayOfArray().ToList().SelectMany(i => i).Min(p => p.X);
            var max_Y_Text = All_Contours_Without_Bubbles.ToArrayOfArray().ToList().SelectMany(i => i).Max(p => p.Y);
            var min_Y_Text = All_Contours_Without_Bubbles.ToArrayOfArray().ToList().SelectMany(i => i).Min(p => p.Y);
            #endregion

            #region 7.Writing The Inside-Bubbles Text
            //7.Writing The Inside-Bubbles Text for The four sides in the dxf file then Deleting them from the image :
            //7.1.Writing The nside-Bubbles Text for Top Bubbles In The DXF File :
            var contorsList_Min_Y_Text = All_Contours_Without_Bubbles.ToArrayOfArray().ToList()
               .Where(z => z.ToList().Any(p => p.Y == min_Y_Text || p.Y == min_Y_Text + 1 || p.Y == min_Y_Text + 2 || p.Y == min_Y_Text + 3 || p.Y == min_Y_Text + 4 ||
               p.Y == min_Y_Text + 5 || p.Y == min_Y_Text + 6 || p.Y == min_Y_Text + 7 || p.Y == min_Y_Text + 8 || p.Y == min_Y_Text - 1 || p.Y == min_Y_Text - 2
               || p.Y == min_Y_Text - 3 || p.Y == min_Y_Text - 4 || p.Y == min_Y_Text - 5 || p.Y == min_Y_Text - 6 || p.Y == min_Y_Text - 7 || p.Y == min_Y_Text - 8
               || p.Y == min_Y_Text + 9 || p.Y == min_Y_Text + 10 || p.Y == min_Y_Text + 11 || p.Y == min_Y_Text + 12 || p.Y == min_Y_Text + 13 || p.Y == min_Y_Text + 14
               || p.Y == min_Y_Text + 15 || p.Y == min_Y_Text - 9 || p.Y == min_Y_Text - 10 || p.Y == min_Y_Text - 11 || p.Y == min_Y_Text - 12 || p.Y == min_Y_Text - 13
               || p.Y == min_Y_Text - 14 || p.Y == min_Y_Text - 15
               )).ToList();

            for (int i = 0; i < contorsList_Min_Y_Text.Count; i++)
            {
                List<Vector3> Vertex_Of_Top_Text = new List<Vector3>();
                for (int j = 0; j < contorsList_Min_Y_Text[i].Length; j++)
                {
                    Vertex_Of_Top_Text.Add(new Vector3(contorsList_Min_Y_Text[i][j].X, contorsList_Min_Y_Text[i][j].Y * -1, 0));
                    Polyline Top_Text = new Polyline(Vertex_Of_Top_Text);
                    Top_Text.Layer = new Layer("Bubbles Text");
                    dxfDocument.AddEntity(Top_Text);
                }
            }
            dxfDocument.Save(@"e:\ITI-Graduation Project\DXF_Str\5.Top_Text.dxf");

            ////7.2.Deleting the Top Text from The Image :
            VectorOfVectorOfPoint Contors_Of_Top_Text = new VectorOfVectorOfPoint();
            for (int i = 0; i < contorsList_Min_Y_Text.Count; i++)
            {
                VectorOfPoint Intermmediate_Vector = new VectorOfPoint();
                Intermmediate_Vector.Push(contorsList_Min_Y_Text[i]);
                Contors_Of_Top_Text.Push(Intermmediate_Vector);
            }
            CvInvoke.DrawContours(Helping_Image_2, Contors_Of_Top_Text, -1, new MCvScalar(0, 0, 0));
            Helping_Image_2.Save(@"e:\ITI-Graduation Project\ImageStr\5.Removing_Top_Text.bmp");

            ////7.3.Writing The Inside-Bubbles Text for Bottom Bubbles In The DXF File :
            var contorsList_Max_Y_Text = All_Contours_Without_Bubbles.ToArrayOfArray().ToList()
                .Where(z => z.ToList().Any(p => p.Y == max_Y_Text || p.Y == max_Y_Text + 1 || p.Y == max_Y_Text + 2 || p.Y == max_Y_Text + 3 || p.Y == max_Y_Text + 4
                || p.Y == max_Y_Text - 1 || p.Y == max_Y_Text - 2 || p.Y == max_Y_Text - 3 || p.Y == max_Y_Text - 4 || p.Y == max_Y_Text + 5 || p.Y == max_Y_Text + 6
                || p.Y == max_Y_Text + 7 || p.Y == max_Y_Text + 8 || p.Y == max_Y_Text - 5 || p.Y == max_Y_Text - 6 || p.Y == max_Y_Text - 7 || p.Y == max_Y_Text - 8
                || p.Y == max_Y_Text + 9 || p.Y == max_Y_Text + 10 || p.Y == max_Y_Text + 11 || p.Y == max_Y_Text + 12 || p.Y == max_Y_Text + 13 || p.Y == max_Y_Text + 14
                || p.Y == max_Y_Text + 15 || p.Y == max_Y_Text - 9 || p.Y == max_Y_Text - 10 || p.Y == max_Y_Text - 11 || p.Y == max_Y_Text - 12 || p.Y == max_Y_Text - 13
                || p.Y == max_Y_Text - 14 || p.Y == max_Y_Text - 15))
                .ToList();

            for (int i = 0; i < contorsList_Max_Y_Text.Count; i++)
            {
                List<Vector3> Vertex_of_Bottom_Text = new List<Vector3>();
                for (int j = 0; j < contorsList_Max_Y_Text[i].Length; j++)
                {
                    Vertex_of_Bottom_Text.Add(new Vector3(contorsList_Max_Y_Text[i][j].X, contorsList_Max_Y_Text[i][j].Y * -1, 0));
                    Polyline Bottom_Text = new Polyline(Vertex_of_Bottom_Text);
                    Bottom_Text.Layer = new Layer("Bubbles Text");
                    dxfDocument.AddEntity(Bottom_Text);
                }
            }
            dxfDocument.Save(@"e:\ITI-Graduation Project\DXF_Str\6.Bottom_Text.dxf");

            ////7.4.Deleting the Bottom Text From The Image :
            VectorOfVectorOfPoint Contors_Of_Bottom_Text = new VectorOfVectorOfPoint();
            for (int i = 0; i < contorsList_Max_Y_Text.Count; i++)
            {
                VectorOfPoint Intermmediate_Vector = new VectorOfPoint();
                Intermmediate_Vector.Push(contorsList_Max_Y_Text[i]);
                Contors_Of_Bottom_Text.Push(Intermmediate_Vector);
            }
            CvInvoke.DrawContours(Helping_Image_2, Contors_Of_Bottom_Text, -1, new MCvScalar(0, 0, 0));
            Helping_Image_2.Save(@"e:\ITI-Graduation Project\ImageStr\6.Removing_Bottom_Text.bmp");

            ////7.5.Writing The Inside-Bubbles Text for Right Bubbles In The DXF File :
            var contorsList_Max_X_Text = All_Contours_Without_Bubbles.ToArrayOfArray().ToList()
                .Where(z => z.ToList().Any(p => p.X == max_X_Text || p.X == max_X_Text + 1 || p.X == max_X_Text + 2 || p.X == max_X_Text + 3 || p.X == max_X_Text + 4
                || p.X == max_X_Text - 1 || p.X == max_X_Text - 2 || p.X == max_X_Text - 3 || p.X == max_X_Text - 4 || p.X == max_X_Text + 5 || p.X == max_X_Text + 6
                || p.X == max_X_Text + 7 || p.X == max_X_Text + 8 || p.X == max_X_Text - 5 || p.X == max_X_Text - 6 || p.X == max_X_Text - 7 || p.X == max_X_Text - 8
                || p.X == max_X_Text + 9 || p.X == max_X_Text + 10 || p.X == max_X_Text + 11 || p.X == max_X_Text + 12 || p.X == max_X_Text + 13 || p.X == max_X_Text + 14
                || p.X == max_X_Text + 15 || p.X == max_X_Text - 9 || p.X == max_X_Text - 10 || p.X == max_X_Text - 11 || p.X == max_X_Text - 12 || p.X == max_X_Text - 13
                || p.X == max_X_Text - 14 || p.X == max_X_Text - 15
                )).ToList();

            for (int i = 0; i < contorsList_Max_X_Text.Count; i++)
            {
                List<Vector3> Vertex_Of_Right_Text = new List<Vector3>();
                for (int j = 0; j < contorsList_Max_X_Text[i].Length; j++)
                {
                    Vertex_Of_Right_Text.Add(new Vector3(contorsList_Max_X_Text[i][j].X, contorsList_Max_X_Text[i][j].Y * -1, 0));
                    Polyline Right_Text = new Polyline(Vertex_Of_Right_Text);
                    Right_Text.Layer = new Layer("Bubbles Text");
                    dxfDocument.AddEntity(Right_Text);
                }
            }
            dxfDocument.Save(@"e:\ITI-Graduation Project\DXF_Str\7.Right_Text.dxf");

            ////7.6.Deleting The right Text from the image :
            VectorOfVectorOfPoint Contors_Of_Right_Text = new VectorOfVectorOfPoint();
            for (int i = 0; i < contorsList_Max_X_Text.Count; i++)
            {
                VectorOfPoint Intermmediate_Vector = new VectorOfPoint();
                Intermmediate_Vector.Push(contorsList_Max_X_Text[i]);
                Contors_Of_Right_Text.Push(Intermmediate_Vector);
            }
            CvInvoke.DrawContours(Helping_Image_2, Contors_Of_Right_Text, -1, new MCvScalar(0, 0, 0));
            Helping_Image_2.Save(@"e:\ITI-Graduation Project\ImageStr\7.Removing_Right_Text.bmp");

            ////7.7.Writing The Inside-Bubbles Text for Left Bubbles In The DXF File :
            var contorsList_Min_X_Text = All_Contours_Without_Bubbles.ToArrayOfArray().ToList()
                .Where(z => z.ToList().Any(p => p.X == min_X_Text || p.X == min_X_Text + 1 || p.X == min_X_Text + 2 || p.X == min_X_Text + 3 || p.X == min_X_Text + 4
                || p.X == min_X_Text - 1 || p.X == min_X_Text - 2 || p.X == min_X_Text - 3 || p.X == min_X_Text - 4 || p.X == min_X_Text + 5 || p.X == min_X_Text + 6 ||
                p.X == min_X_Text + 7 || p.X == min_X_Text + 8 || p.X == min_X_Text - 5 || p.X == min_X_Text - 6 || p.X == min_X_Text - 7 || p.X == min_X_Text - 8
                || p.X == min_X_Text + 9 || p.X == min_X_Text + 10 || p.X == min_X_Text + 11 || p.X == min_X_Text + 12 || p.X == min_X_Text + 13 || p.X == min_X_Text + 14
                || p.X == min_X_Text + 15 || p.X == min_X_Text - 9 || p.X == min_X_Text - 10 || p.X == min_X_Text - 11 || p.X == min_X_Text - 12 || p.X == min_X_Text - 13
                || p.X == min_X_Text - 14 || p.X == min_X_Text - 15)).ToList();

            for (int i = 0; i < contorsList_Min_X_Text.Count; i++)
            {
                List<Vector3> Vertex_Of_Left_Text = new List<Vector3>();
                for (int j = 0; j < contorsList_Min_X_Text[i].Length; j++)
                {
                    Vertex_Of_Left_Text.Add(new Vector3(contorsList_Min_X_Text[i][j].X, contorsList_Min_X_Text[i][j].Y * -1, 0));
                    Polyline Left_Text = new Polyline(Vertex_Of_Left_Text);
                    Left_Text.Layer = new Layer("Bubbles Text");
                    dxfDocument.AddEntity(Left_Text);
                }
            }
            dxfDocument.Save(@"e:\ITI-Graduation Project\DXF_Str\8.Left_Text.dxf");

            ////7.8.Deleting The left Text from the image :
            VectorOfVectorOfPoint Contors_Of_Left_Text = new VectorOfVectorOfPoint();
            for (int i = 0; i < contorsList_Min_X_Text.Count; i++)
            {
                VectorOfPoint Intermmideate_Vector = new VectorOfPoint();
                Intermmideate_Vector.Push(contorsList_Min_X_Text[i]);
                Contors_Of_Left_Text.Push(Intermmideate_Vector);
            }
            CvInvoke.DrawContours(Helping_Image_2, Contors_Of_Left_Text, -1, new MCvScalar(0, 0, 0));
            Helping_Image_2.Save(@"e:\ITI-Graduation Project\ImageStr\8.Removing_Left_Text.bmp");
            #endregion

            #region 8.3rd Image Processing :
            //8.Third Image Processing Step By Reading All Contours Without The Bubbles && The Bubbles Inside Text From The Image:
            VectorOfVectorOfPoint Contours_After_Removing_Bubbles_Text = new VectorOfVectorOfPoint();
            Mat hier_Lvl3 = new Mat();
            CvInvoke.FindContours(Helping_Image_2, Contours_After_Removing_Bubbles_Text, hier_Lvl3, Emgu.CV.CvEnum.RetrType.Ccomp, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

            ////8.1.Finding The Maximum Value of X, Y in each direction To Determine The Inside-Bubbles Useless Dimensions Ending Points :
            var max_X_Dim = Contours_After_Removing_Bubbles_Text.ToArrayOfArray().ToList().SelectMany(i => i).Max(p => p.X);
            var min_X_Dim = Contours_After_Removing_Bubbles_Text.ToArrayOfArray().ToList().SelectMany(i => i).Min(p => p.X);
            var max_Y_Dim = Contours_After_Removing_Bubbles_Text.ToArrayOfArray().ToList().SelectMany(i => i).Max(p => p.Y);
            var min_Y_Dim = Contours_After_Removing_Bubbles_Text.ToArrayOfArray().ToList().SelectMany(i => i).Min(p => p.Y);
            #endregion

            #region 9.Removing Useless Dimensions :
            //9.Removing Useless Dimensions From The Image :
            //9.1.Removing The Left Side Dimensions :
            var contorsList_Min_X_Dim = Contours_After_Removing_Bubbles_Text.ToArrayOfArray().ToList()
                 .Where(z => z.ToList().Any(p => p.X == min_X_Dim || p.X == min_X_Dim + 1 || p.X == min_X_Dim + 2 || p.X == min_X_Dim + 3 || p.X == min_X_Dim + 4
                 || p.X == min_X_Dim - 1 || p.X == min_X_Dim - 2 || p.X == min_X_Dim - 3 || p.X == min_X_Dim - 4 || p.X == min_X_Dim + 5 || p.X == min_X_Dim + 6 ||
                 p.X == min_X_Dim + 7 || p.X == min_X_Dim + 8 || p.X == min_X_Dim - 5 || p.X == min_X_Dim - 6 || p.X == min_X_Dim - 7 || p.X == min_X_Dim - 8 ||
                 p.X == min_X_Dim - 9 || p.X == min_X_Dim + 9 || p.X == min_X_Dim + 10
                 )).ToList();

            VectorOfVectorOfPoint Contors_Of_LeftSide_Dimensions = new VectorOfVectorOfPoint();
            for (int i = 0; i < contorsList_Min_X_Dim.Count; i++)
            {
                VectorOfPoint Intermmideate_Vector = new VectorOfPoint();
                Intermmideate_Vector.Push(contorsList_Min_X_Dim[i]);
                Contors_Of_LeftSide_Dimensions.Push(Intermmideate_Vector);
            }
            CvInvoke.DrawContours(Helping_Image_2, Contors_Of_LeftSide_Dimensions, -1, new MCvScalar(0, 0, 0));
            Helping_Image_2.Save(@"e:\ITI-Graduation Project\ImageStr\9.Removing_LeftSide_Dimensions.bmp");

            ////9.2.Removing The Right Side Dimensions :
            var contorsList_Max_X_Dim = Contours_After_Removing_Bubbles_Text.ToArrayOfArray().ToList()
               .Where(z => z.ToList().Any(p => p.X == max_X_Dim || p.X == max_X_Dim + 1 || p.X == max_X_Dim + 2 || p.X == max_X_Dim + 3 || p.X == max_X_Dim + 4
               || p.X == max_X_Dim - 1 || p.X == max_X_Dim - 2 || p.X == max_X_Dim - 3 || p.X == max_X_Dim - 4 || p.X == max_X_Dim + 5 || p.X == max_X_Dim + 6 ||
                p.X == max_X_Dim + 7 || p.X == max_X_Dim + 8 || p.X == max_X_Dim - 5 || p.X == max_X_Dim - 6 || p.X == max_X_Dim - 7 || p.X == max_X_Dim - 8 ||
                p.X == max_X_Dim - 9 || p.X == max_X_Dim - 10 || p.X == max_X_Dim + 9 || p.X == max_X_Dim + 10
               )).ToList();

            VectorOfVectorOfPoint Contors_Of_RightSide_Dimensions = new VectorOfVectorOfPoint();
            for (int i = 0; i < contorsList_Max_X_Dim.Count; i++)
            {
                VectorOfPoint Intermmideate_Vector = new VectorOfPoint();
                Intermmideate_Vector.Push(contorsList_Max_X_Dim[i]);
                Contors_Of_RightSide_Dimensions.Push(Intermmideate_Vector);
            }
            CvInvoke.DrawContours(Helping_Image_2, Contors_Of_RightSide_Dimensions, -1, new MCvScalar(0, 0, 0));
            Helping_Image_2.Save(@"e:\ITI-Graduation Project\ImageStr\10.Removing_RightSide_Dimensions.bmp");

            ////9.3.Removing The Bottom Side Dimensions :
            var contorsList_Max_Y_Dim = Contours_After_Removing_Bubbles_Text.ToArrayOfArray().ToList()
                .Where(z => z.ToList().Any(p => p.Y == max_Y_Dim || p.Y == max_Y_Dim + 1 || p.Y == max_Y_Dim + 2 || p.Y == max_Y_Dim + 3
                || p.Y == max_Y_Dim - 1 || p.Y == max_Y_Dim - 2 || p.Y == max_Y_Dim - 3 || p.Y == max_Y_Dim - 4 || p.Y == max_Y_Dim + 4
                ))
                .ToList();
            VectorOfVectorOfPoint Contors_Of_BottomSide_Dimensions = new VectorOfVectorOfPoint();
            for (int i = 0; i < contorsList_Max_Y_Dim.Count; i++)
            {
                VectorOfPoint Intermmideate_Vector = new VectorOfPoint();
                Intermmideate_Vector.Push(contorsList_Max_Y_Dim[i]);
                Contors_Of_BottomSide_Dimensions.Push(Intermmideate_Vector);
            }
            CvInvoke.DrawContours(Helping_Image_2, Contors_Of_BottomSide_Dimensions, -1, new MCvScalar(0, 0, 0));
            Helping_Image_2.Save(@"e:\ITI-Graduation Project\ImageStr\11.Removing_BottomSide_Dimensions.bmp");

            ////9.4.Removing The Top Side Dimensions :
            var contorsList_Min_Y_Dim = Contours_After_Removing_Bubbles_Text.ToArrayOfArray().ToList()
               .Where(z => z.ToList().Any(p => p.Y == min_Y_Dim || p.Y == min_Y_Dim + 1 || p.Y == min_Y_Dim - 1 || p.Y == min_Y_Dim - 2 || p.Y == min_Y_Dim + 2 || p.Y == min_Y_Dim - 3
               || p.Y == min_Y_Dim + 3
               ))
               .ToList();
            VectorOfVectorOfPoint Contors_Of_TopSide_Dimensions = new VectorOfVectorOfPoint();
            for (int i = 0; i < contorsList_Min_Y_Dim.Count; i++)
            {
                VectorOfPoint Intermmideate_Vector = new VectorOfPoint();
                Intermmideate_Vector.Push(contorsList_Min_Y_Dim[i]);
                Contors_Of_TopSide_Dimensions.Push(Intermmideate_Vector);
            }
            CvInvoke.DrawContours(Helping_Image_2, Contors_Of_TopSide_Dimensions, -1, new MCvScalar(0, 0, 0));
            Helping_Image_2.Save(@"e:\ITI-Graduation Project\ImageStr\12.Removing_TopSide_Dimensions.bmp");
            #endregion

            #region 10.Deleting Bubbels From Helping_Image2 To have a higher accuracy:
            //10.1.Deleting Bubbels From The Source Image Using Small Black Circles To Obtain Higher Accuracy in Getting Grids :
            //10.1.1.Deleting Bubbels :-
            foreach (var p in contorsList_Max_X_Bubbles)
            {
                foreach (var pt in p)
                {
                    CvInvoke.Circle(Helping_Image_3, pt, 0, new MCvScalar(0, 0, 0), 7);
                }
            }

            foreach (var p in contorsList_Min_X_Bubbles)
            {
                foreach (var pt in p)
                {
                    CvInvoke.Circle(Helping_Image_3, pt, 0, new MCvScalar(0, 0, 0), 7);
                }
            }

            foreach (var p in contorsList_Max_Y_Bubbles)
            {
                foreach (var pt in p)
                {
                    CvInvoke.Circle(Helping_Image_3, pt, 0, new MCvScalar(0, 0, 0), 7);
                }
            }

            foreach (var p in contorsList_Min_Y_Bubbles)
            {
                foreach (var pt in p)
                {
                    CvInvoke.Circle(Helping_Image_3, pt, 0, new MCvScalar(0, 0, 0), 7);
                }
            }
            Helping_Image_3.Save(@"e:\ITI-Graduation Project\ImageStr\13.Grids_1.bmp");

            //10.1.2.Deleting Inside Bubbels Text By The Same Method We Used before but saving On Helping_Image3 :
            CvInvoke.DrawContours(Helping_Image_3, Contors_Of_Left_Text, -1, new MCvScalar(0, 0, 0));
            CvInvoke.DrawContours(Helping_Image_3, Contors_Of_Right_Text, -1, new MCvScalar(0, 0, 0));
            CvInvoke.DrawContours(Helping_Image_3, Contors_Of_Bottom_Text, -1, new MCvScalar(0, 0, 0));
            CvInvoke.DrawContours(Helping_Image_3, Contors_Of_Top_Text, -1, new MCvScalar(0, 0, 0));
            Helping_Image_3.Save(@"e:\ITI-Graduation Project\ImageStr\13.Grids_2.bmp");
            #endregion

            #region 11.4th Image Processing :
            //11.Fourth Image Processing Step By Reading All Contours Without The Bubbles && The Bubbles Inside Text && Useless Dimensions From The Image:
            VectorOfVectorOfPoint Contours_To_Get_Grids = new VectorOfVectorOfPoint();
            Mat hier_Lvl4 = new Mat();
            CvInvoke.FindContours(Helping_Image_3, Contours_To_Get_Grids, hier_Lvl4, Emgu.CV.CvEnum.RetrType.Ccomp, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

            ////11.1.Finding The Maximum Value of X, Y in each direction To Determine The Grids Ending Points :
            var max_X_Grids = Contours_To_Get_Grids.ToArrayOfArray().ToList().SelectMany(i => i).Max(p => p.X);
            var min_X_Grids = Contours_To_Get_Grids.ToArrayOfArray().ToList().SelectMany(i => i).Min(p => p.X);
            var max_Y_Grids = Contours_To_Get_Grids.ToArrayOfArray().ToList().SelectMany(i => i).Max(p => p.Y);
            var min_Y_Grids = Contours_To_Get_Grids.ToArrayOfArray().ToList().SelectMany(i => i).Min(p => p.Y);
            #endregion

            #region 12.Drawing Grids in The DXF File:
            //12.Drawing The Grids :-
            //12.1.Getting The Ending Points for each Grid in the four directions :-
            var contorsList_Min_X_Grids = Contours_To_Get_Grids.ToArrayOfArray().ToList()
                 .Where(z => z.ToList().Any(p => p.X == min_X_Grids || p.X == min_X_Grids + 1 || p.X == min_X_Grids + 2 || p.X == min_X_Grids + 3 || p.X == min_X_Grids + 4
                 || p.X == min_X_Grids - 1 || p.X == min_X_Grids - 2 || p.X == min_X_Grids - 3 || p.X == min_X_Grids - 4 || p.X == min_X_Grids + 5 || p.X == min_X_Grids + 6 ||
                 p.X == min_X_Grids + 7 || p.X == min_X_Grids + 8 || p.X == min_X_Grids - 5 || p.X == min_X_Grids - 6 || p.X == min_X_Grids - 7 || p.X == min_X_Grids - 8
                 ))
                 .ToList();


            var contorsList_Max_X_Grids = Contours_To_Get_Grids.ToArrayOfArray().ToList()
               .Where(z => z.ToList().Any(p => p.X == max_X_Grids || p.X == max_X_Grids + 1 || p.X == max_X_Grids + 2 || p.X == max_X_Grids + 3 || p.X == max_X_Grids + 4
               || p.X == max_X_Grids - 1 || p.X == max_X_Grids - 2 || p.X == max_X_Grids - 3 || p.X == max_X_Grids - 4 || p.X == max_X_Grids + 5 || p.X == max_X_Grids + 6
               || p.X == max_X_Grids + 7 || p.X == max_X_Grids + 8 || p.X == max_X_Grids - 5 || p.X == max_X_Grids - 6 || p.X == max_X_Grids - 7 || p.X == max_X_Grids - 8
               ))
               .ToList();


            var contorsList_Max_Y_Grids = Contours_To_Get_Grids.ToArrayOfArray().ToList()
                .Where(z => z.ToList().Any(p => p.Y == max_Y_Grids || p.Y == max_Y_Grids + 1 || p.Y == max_Y_Grids + 2 || p.Y == max_Y_Grids + 3 || p.Y == max_Y_Grids + 4
                || p.Y == max_Y_Grids - 1 || p.Y == max_Y_Grids - 2 || p.Y == max_Y_Grids - 3 || p.Y == max_Y_Grids - 4 || p.Y == max_Y_Grids + 5 || p.Y == max_Y_Grids + 6
                || p.Y == max_Y_Grids + 7 || p.Y == max_Y_Grids + 8 || p.Y == max_Y_Grids - 5 || p.Y == max_Y_Grids - 6 || p.Y == max_Y_Grids - 7 || p.Y == max_Y_Grids - 8
                ))
                .ToList();

            var contorsList_Min_Y_Grids = Contours_To_Get_Grids.ToArrayOfArray().ToList()
               .Where(z => z.ToList().Any(p => p.Y == min_Y_Grids || p.Y == min_Y_Grids + 1 || p.Y == min_Y_Grids + 2 || p.Y == min_Y_Grids + 3 || p.Y == min_Y_Grids + 4
               || p.Y == min_Y_Grids + 5 || p.Y == min_Y_Grids + 6 || p.Y == min_Y_Grids + 7 || p.Y == min_Y_Grids + 8 || p.Y == min_Y_Grids - 1 || p.Y == min_Y_Grids - 2
               || p.Y == min_Y_Grids - 3 || p.Y == min_Y_Grids - 4 || p.Y == min_Y_Grids - 5 || p.Y == min_Y_Grids - 6 || p.Y == min_Y_Grids - 7 || p.Y == min_Y_Grids - 8
               ))
               .ToList();

            ////12.2.Getting The List Of Starting Points for each Horizontal Grids :
            List<Vector3> Horizontal_Grids_Starting_Points = new List<Vector3>();
            for (int i = 0; i < contorsList_Min_X_Grids.Count; i++)
            {
                Horizontal_Grids_Starting_Points.Add(new Vector3(contorsList_Min_X_Grids[i][0].X, contorsList_Min_X_Grids[i][0].Y * -1, 0));
            }
            Horizontal_Grids_Starting_Points = Ordering_VEC3_Y(Horizontal_Grids_Starting_Points); //Ordering The List According to Y To Match with the ending points in the next steps  

            ////12.2.Getting The List Of Ending Points for each Vertical Grids :
            List<Vector3> Horizontal_Grids_Ending_Points = new List<Vector3>();
            for (int i = 0; i < contorsList_Max_X_Grids.Count; i++)
            {
                Horizontal_Grids_Ending_Points.Add(new Vector3(contorsList_Max_X_Grids[i][0].X, contorsList_Max_X_Grids[i][0].Y * -1, 0));
            }
            Horizontal_Grids_Ending_Points = Ordering_VEC3_Y(Horizontal_Grids_Ending_Points); //Ordering The List According to Y To Match with the starting points in the previous steps  

            ////12.3.Drawing The Horizontal Grids in the DXF :
            for (int i = 0; i < Horizontal_Grids_Starting_Points.Count; i++)
            {
                Line Horizontal_Grid = new Line(Horizontal_Grids_Starting_Points[i], Horizontal_Grids_Ending_Points[i]);
                Horizontal_Grid.Layer = new Layer("Horizontal Grids");
                Horizontal_Grid.Linetype = Linetype.Center;
                Horizontal_Grid.LinetypeScale = 40;
                dxfDocument.AddEntity(Horizontal_Grid);
            }
            dxfDocument.Save(@"e:\ITI-Graduation Project\DXF_Str\9.Horizontal_Grids.dxf");

            ////12.4.Getting The List Of Starting Points for each Vertical Grids :
            List<Vector3> Vertical_Grids_Starting_Points = new List<Vector3>();
            for (int i = 0; i < contorsList_Min_Y_Grids.Count; i++)
            {
                Vertical_Grids_Starting_Points.Add(new Vector3(contorsList_Min_Y_Grids[i][0].X, contorsList_Min_Y_Grids[i][0].Y * -1, 0));
            }
            Vertical_Grids_Starting_Points = Ordering_VEC3_X(Vertical_Grids_Starting_Points);//Ordering The List According to X To Match with the ending points in the next steps 

            ////12.5.Getting The List Of Ending Points for each Vertical Grids :
            List<Vector3> Vertical_Grids_Ending_Points = new List<Vector3>();
            for (int i = 0; i < contorsList_Max_Y_Grids.Count; i++)
            {
                Vertical_Grids_Ending_Points.Add(new Vector3(contorsList_Max_Y_Grids[i][0].X, contorsList_Max_Y_Grids[i][0].Y * -1, 0));
            }
            Vertical_Grids_Ending_Points = Ordering_VEC3_X(Vertical_Grids_Ending_Points); //Ordering The List According to X To Match with the starting points in the previous steps 

            ////12.6.Drawing The Vertical Grids in the DXF :
            for (int i = 0; i < Vertical_Grids_Starting_Points.Count; i++)
            {
                Line Vertical_Grid = new Line(Vertical_Grids_Starting_Points[i], Vertical_Grids_Ending_Points[i]);
                Vertical_Grid.Layer = new Layer("Vertical Grids");
                Vertical_Grid.Linetype = Linetype.Center;
                Vertical_Grid.LinetypeScale = 40;
                dxfDocument.AddEntity(Vertical_Grid);
            }
            dxfDocument.Save(@"e:\ITI-Graduation Project\DXF_Str\10.Vertical_Grids.dxf");
            #endregion

            #region 13.Deleting Grids from The Image:
            //13.Deleting Contours From The Image To be able to Draw the last part of the image :
            //13.1.Getting The List Of Starting Points for each Horizontal Grids (as points not vector3) but the same logic before except *-1 :
            List<Point> Horizontal_Grids_Starting_Points_IMG = new List<Point>();
            for (int i = 0; i < contorsList_Min_X_Grids.Count; i++)
            {
                Horizontal_Grids_Starting_Points_IMG.Add(new Point(contorsList_Min_X_Grids[i][0].X, contorsList_Min_X_Grids[i][0].Y));
            }
            Horizontal_Grids_Starting_Points_IMG = Ordering_Points_Y(Horizontal_Grids_Starting_Points_IMG); //Ordering The List According to Y To Match with the ending points in the next steps

            ////13.2.Getting The List Of Ending Points for each Horizontal Grids (as points not vector3) but the same logic before except *-1 :
            List<Point> Horizontal_Grids_Ending_Points_IMG = new List<Point>();
            for (int i = 0; i < contorsList_Max_X_Grids.Count; i++)
            {
                Horizontal_Grids_Ending_Points_IMG.Add(new Point(contorsList_Max_X_Grids[i][0].X, contorsList_Max_X_Grids[i][0].Y));
            }
            Horizontal_Grids_Ending_Points_IMG = Ordering_Points_Y(Horizontal_Grids_Ending_Points_IMG);//Ordering The List According to Y To Match with the starting points in the previous steps

            ////13.3.Deleting The Horizontal Grids From The Image :
            for (int i = 0; i < Horizontal_Grids_Starting_Points_IMG.Count; i++)
            {
                CvInvoke.Line(Helping_Image_2, Horizontal_Grids_Starting_Points_IMG[i], Horizontal_Grids_Ending_Points_IMG[i], new MCvScalar(0, 0, 0), 1);
            }
            Helping_Image_2.Save(@"e:\ITI-Graduation Project\ImageStr\14.With_No_HZ_Grids.bmp");

            ////13.4.Getting The List Of Starting Points for each Vertical Grids (as points not vector3) but the same logic before except *-1 :
            List<Point> Vertical_Grids_Starting_Points_IMG = new List<Point>();
            for (int i = 0; i < contorsList_Min_Y_Grids.Count; i++)
            {
                Vertical_Grids_Starting_Points_IMG.Add(new Point(contorsList_Min_Y_Grids[i][0].X, contorsList_Min_Y_Grids[i][0].Y));
            }
            Vertical_Grids_Starting_Points_IMG = Ordering_Points_X(Vertical_Grids_Starting_Points_IMG); //Ordering The List According to X To Match with the starting points in the next steps 

            ////13.5.Getting The List Of Ending Points for each Vertical Grids (as points not vector3) but the same logic before except *-1 :
            List<Point> Vertical_Grids_Ending_Points_IMG = new List<Point>();
            for (int i = 0; i < contorsList_Max_Y_Grids.Count; i++)
            {
                Vertical_Grids_Ending_Points_IMG.Add(new Point(contorsList_Max_Y_Grids[i][0].X, contorsList_Max_Y_Grids[i][0].Y));
            }
            Vertical_Grids_Ending_Points_IMG = Ordering_Points_X(Vertical_Grids_Ending_Points_IMG); //Ordering The List According to X To Match with the starting points in the next steps

            for (int i = 0; i < Vertical_Grids_Starting_Points_IMG.Count; i++)
            {
                CvInvoke.Line(Helping_Image_2, Vertical_Grids_Starting_Points_IMG[i], Vertical_Grids_Ending_Points_IMG[i], new MCvScalar(0, 0, 0), 1);
            }
            Helping_Image_2.Save(@"e:\ITI-Graduation Project\ImageStr\14.With_No_VL_Grids.bmp");
            #endregion

            #region 14.5th Image Processing:
            ////14.Fifth Image Processing Step By Reading All Contours Without The Bubbles && The Bubbles Inside Text && Useless Dimensions From The Image:
            VectorOfVectorOfPoint Inside_Part_Contours = new VectorOfVectorOfPoint();
            Mat hier_LVL4 = new Mat();
            CvInvoke.FindContours(Helping_Image_2, Inside_Part_Contours, hier_LVL4, Emgu.CV.CvEnum.RetrType.Ccomp, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
            #endregion

            #region 15.Drawing The Inside Part in the DXF File:
            //15.Drawing The Inside Part in the DXF File :
            for (int i = 0; i < Inside_Part_Contours.Size; i++)
            {
                List<Vector3> Inside_Vertices = new List<Vector3>();
                for (int j = 0; j < Inside_Part_Contours[i].Size; j++)
                {
                    Inside_Vertices.Add(new Vector3(Inside_Part_Contours[i][j].X, Inside_Part_Contours[i][j].Y * -1, 0));
                }
                Polyline Inside = new Polyline(Inside_Vertices);
                Inside.Layer = new Layer("Inside Drawing Elements");
                dxfDocument.AddEntity(Inside);
            }
            #endregion

            #region 16.Saving Part:
            //16.Saving The Final File :
            dxfDocument.DrawingVariables.AcadVer = DxfVersion.AutoCad2010;
            dxfDocument.Save(@"e:\ITI-Graduation Project\DXF_Str\11.Struc_Plan.dxf");
            #endregion
        }
    }
}
