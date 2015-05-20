using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TruckETCExitus.Model
{
    public class Location
    {
        #region 属性

        private OBUData obuData;

        private int sendFrameType;       

        private int pOstate;

        private int xOBUCoord;

        private int yOBUCoord;

        public OBUData ObuData
        {
            get { return obuData; }
            set { obuData = value; }
        }

        public int SendFrameType
        {
            get { return sendFrameType; }
            set { sendFrameType = value; }
        }

        public int POstate
        {
            get { return pOstate; }
            set { pOstate = value; }
        }        

        public int XOBUCoord
        {
            get { return xOBUCoord; }
            set { xOBUCoord = value; }
        }

        public int YOBUCoord
        {
            get { return yOBUCoord; }
            set { yOBUCoord = value; }
        }

        #endregion

        #region 构造函数

        public Location(OBUData obudata, int sendFrameType, int postate, int xOBUCoord, int yOBUCoord)
        {
            this.obuData = obudata;
            this.sendFrameType = sendFrameType;
            this.pOstate = postate;
            this.xOBUCoord = xOBUCoord;
            this.yOBUCoord = yOBUCoord;
        }

        #endregion

    }
}
