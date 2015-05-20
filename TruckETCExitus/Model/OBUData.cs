using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TruckETCExitus.Model
{
    public class OBUData
    {
        private int obuNum;

        public OBUData(int Num)
        {
            obuNum = Num;
        }

        public int ObuNum
        {
            get { return obuNum; }
            set { obuNum = value; }
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
