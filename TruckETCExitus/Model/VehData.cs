using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace TruckETCExitus.Model
{
    public class VehData
    {
        public String CheckCode;
        public String StationID;
        public int Lane_ID;
        public String License_Plate;
        public String Plate_Color;
        public int Axle_Num;
        public int Axle_Type;
        public int Whole_Weight;
        public int Recheck_WholeWeight;
        public int Whole_Limit;
        public int Whole_Over;
        public int Whole_Overrate;
        public DateTime Check_DT;
        public String Init_StaffName;
        public DateTime Recheck_DT;
        public String Recheck_StarffName;
        public String Staff_ID;
        public int Veh_Speed;
        public int Veh_Length;
        public int Veh_Width;
        public int Veh_Height;
        public int[] AxisWeight;
        public int IsOver;       //1 超限，0不超
        public int Over_Num;
        public int Rated_Load;
        public int Over_Load;
        public int Overload_Num;
        public String Goods_Name;
        public int Goods_Type;
        public int Check_Fee;

        public Image Plate_Image;
        public Image Veh_Image;

        public int MeterVer;

        public VehData(int Axle_Num, int Axle_Type, int Whole_Weight, int Whole_Limit, int Whole_Over, int Whole_Overrate
            , int Veh_Speed, int[] AxisWeight)
        {
            this.Axle_Num = Axle_Num;
            this.Axle_Type = Axle_Type;
            this.Whole_Weight = Whole_Weight;
            this.Whole_Limit = Whole_Limit;
            this.Whole_Over = Whole_Over;
            this.Whole_Overrate = Whole_Overrate;
            this.Veh_Speed = Veh_Speed;
            this.AxisWeight = AxisWeight;
        }
        public VehData()
        {
            this.Axle_Num = 0;
            this.Axle_Type = 0;
            this.Whole_Weight = 0;
            this.Whole_Limit = 0;
            this.Whole_Over = 0;
            this.Whole_Overrate = 0;
            this.Veh_Speed = 0;
        }
        public VehData Clone()
        {
            return new VehData(this.Axle_Num, this.Axle_Type, this.Whole_Weight, this.Whole_Limit, this.Whole_Over,
                this.Whole_Overrate, this.Veh_Speed, this.AxisWeight);
        }

        public void flush()
        {
            this.CheckCode = "";
            this.StationID = "";
            this.Lane_ID = 0;
            this.License_Plate = "";
            this.Plate_Color = "";
            this.Axle_Num = 0;
            this.Axle_Type = 0;
            this.Whole_Limit = 0;
            this.Whole_Over = 0;
            this.Whole_Overrate = 0;
            this.Whole_Weight = 0;
            this.Recheck_WholeWeight = 0;
            this.Check_DT = new DateTime(2000, 1, 1, 1, 1, 1);
            this.Init_StaffName = "";
            this.Recheck_DT = new DateTime(2000, 1, 1, 1, 1, 1);
            this.Recheck_StarffName = "";
            this.Staff_ID = "";
            this.Veh_Height = 0;
            this.Veh_Length = 0;
            this.Veh_Speed = 0;
            this.Veh_Width = 0;
            this.AxisWeight = new int[20];
            this.IsOver = 0;
            this.Over_Num = 0;
            this.Rated_Load = 0;
            this.Over_Load = 0;
            this.Overload_Num = 0;
            this.Goods_Name = "";
            this.Goods_Type = 0;
            this.Check_Fee = 0;

            this.Plate_Image = null;
            this.Veh_Image = null;
        }
        public string toString()
        {
            StringBuilder sb = new StringBuilder(string.Format("Axle_Num={0},Axle_Type={1},Whole_Weight={2},Whole_Limit={3},Whole_Over={4},Whole_Overrate={5},Veh_Speed={6}",
                this.Axle_Num, this.Axle_Type, this.Whole_Weight, this.Whole_Limit, this.Whole_Over, this.Whole_Overrate, this.Veh_Speed));
            for (int i = 0; i < 20; i++)
                sb.Append(string.Format(",AxisWeight[{0}]={1}", i, this.AxisWeight[i]));
            return sb.ToString();
        }
    }
}
