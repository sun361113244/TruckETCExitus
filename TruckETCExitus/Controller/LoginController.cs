using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using TruckETCExitus.Etc;
using Util;

namespace TruckETCExitus.Controller
{
    public class LoginController
    {
        private static DbConnection SQLDBConn;
        public static DbConnection GetSQLServerConnection()
        {
            if(SQLDBConn == null)
            {
                DBTools.DBLocation = LocalDBParams.DBLocation;
                DBTools.DBName = LocalDBParams.DBName;
                DBTools.DBPort = LocalDBParams.DBPort;
                DBTools.DBUser = LocalDBParams.DBUser;
                DBTools.DBPwd = LocalDBParams.DBPwd;
                DBTools.DBType = LocalDBParams.DBType;

                SQLDBConn = DBTools.GetConnection();
                return SQLDBConn;
            }
            else
            {
                return SQLDBConn;
            }
        }
    }
}
