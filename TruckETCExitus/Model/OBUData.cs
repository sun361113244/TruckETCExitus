using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TruckETCExitus.Model
{
    public class OBUData
    {
        private int obuNum;

        private UInt64 userCardNo = 0;
        

        public OBUData(int Num)
        {
            obuNum = Num;
        }

        public OBUData(int Num , UInt64 userCardNo)
        {
            this.obuNum = Num;
            this.userCardNo = userCardNo;
        }

        public int ObuNum
        {
            get { return obuNum; }
            set { obuNum = value; }
        }
        public UInt64 UserCardNo
        {
            get { return userCardNo; }
            set { userCardNo = value; }
        }

        public OBUData Clone()
        {
            return new OBUData(this.obuNum);
        }

        public override bool Equals(Object elem)
        {
            return this.obuNum == ((OBUData)elem).ObuNum;
        }
    }
}
