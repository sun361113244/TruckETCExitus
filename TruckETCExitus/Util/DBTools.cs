//////////////////////////////////////////////////////////////////////////
//Commit:   通用数据库操作类
//Author:   charles
//Date：    2014-07-03
//Version:  1.0
//////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Reflection;

namespace Util
{
    // 数据库类型枚举
    public enum DataBaseType
    {
        SQLServer = 0,
        SQLServerExpress = 1,
        Access = 2,
        MySql = 3,
        Orcale = 4
    }
    public static class DBTools
    {
        public static string DBLocation;                  // 本地数据库实例名或IP

        public static int DBPort;                         // 本地数据库监听端口

        public static DataBaseType DBType;                // 数据库类型

        public static string DBName;                      // 本地数据库名称

        public static string DBUser;                      // 用户名

        public static string DBPwd;                       // 登录密码

        /************************************************************************
          Function:  根据不同的数据库类型,获取不同的数据库连接                                      
          In:                                                           
          Out:  DbConnection,  which is opened    
          Author: charles
          Date: 2014-07-3
        ************************************************************************/
        public static DbConnection GetConnection()
        {
            DbConnection DBConn = null;
            string DbProvider = "";
            string DbConnString = "";

            switch (DBType)
            {
                case DataBaseType.Access:
                    DbConnString = @"Provider=Microsoft.Jet.OLEDB.4.0;";
                    DbConnString += @"Data Source=" + DBLocation;
                    DbProvider = "System.Data.OleDb";
                    break;
                case DataBaseType.MySql:
                    break;
                case DataBaseType.Orcale:
                    DbConnString = string.Format("Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={0}) (PORT={1})))(CONNECT_DATA=(SERVICE_NAME={2})));",
                        DBLocation, DBPort, DBName);
                    DbConnString += string.Format("Persist Security Info=True;User Id={0}; Password={1}", DBUser, DBPwd);
                    DbProvider = "System.Data.OracleClient";
                    break;
                case DataBaseType.SQLServer:
                    DbConnString = @"Persist Security Info=True;Data Source=" + DBLocation + @";";
                    DbConnString += @"Initial Catalog=" + DBName + @";";
                    DbConnString += @"UID=" + DBUser + @";PWD=" + DBPwd;
                    DbProvider = "System.Data.SqlClient";
                    break;
                case DataBaseType.SQLServerExpress:
                    DbConnString = @"Persist Security Info=True;Data Source=" + DBLocation + @"\SQLEXPRESS;";
                    DbConnString += @"Initial Catalog=" + DBName + @";";
                    DbConnString += @"UID=" + DBUser + @";PWD=" + DBPwd;
                    DbProvider = "System.Data.SqlClient";
                    break;
            }
            try
            {
                DbProviderFactory factory = DbProviderFactories.GetFactory(DbProvider);
                DBConn = factory.CreateConnection();
                DBConn.ConnectionString = DbConnString;
                DBConn.Open();
            }
            catch (Exception ex)
            {
                LogTools.WriteSystemErrorLog(MethodBase.GetCurrentMethod().DeclaringType, MethodBase.GetCurrentMethod().Name, ex.Message);
                throw ex;
            }
            return DBConn;
        }
        /************************************************************************
          Function:  根据sql语句获取DataReader                                      
          In:   string sql, DbConnection                                                        
          Out:  DataReader 
          Author: DQS
          Date: 2012-08-28
        ************************************************************************/
        public static System.Data.IDataReader GetDataReader(string sql, System.Data.Common.DbConnection Conn)
        {
            System.Data.IDataReader reader = null;
            if (Conn == null)
            {
                LogTools.WriteSystemErrorLog(MethodBase.GetCurrentMethod().DeclaringType, MethodBase.GetCurrentMethod().Name, "数据库连接为空");
                return null;
            }
            try
            {
                if (Conn.State == System.Data.ConnectionState.Closed)
                {
                    Conn.Open();
                }
                if (Conn.State == ConnectionState.Open)
                {
                    System.Data.IDbCommand cmd = Conn.CreateCommand();
                    cmd.CommandText = sql;
                    reader = cmd.ExecuteReader();
                    return reader;
                }
            }
            catch (Exception ex)
            {
                //LogTools.WriteLog(MethodBase.GetCurrentMethod().DeclaringType, MethodBase.GetCurrentMethod().Name, "sql: " + sql + "错误信息：" + ex.Message);
                throw new Exception("sql: " + sql + "错误信息：" + ex.Message);
            }
            return null;
        }

        /************************************************************************
          Function:  根据sql语句获取DataTable                                      
          In:   string sql, DbConnection                                                        
          Out:  DataTable 
          Author: DQS
          Date: 2012-08-28
        ************************************************************************/
        public static System.Data.DataTable GetDataTable(string sql, System.Data.Common.DbConnection Conn)
        {
            if (Conn == null)
            {
                LogTools.WriteSystemErrorLog(MethodBase.GetCurrentMethod().DeclaringType, MethodBase.GetCurrentMethod().Name, "数据库连接为空");
                return null;
            }
            try
            {
                if (Conn.State == System.Data.ConnectionState.Closed)
                {
                    Conn.Open();
                }
                if (Conn.State == ConnectionState.Open)
                {
                    System.Data.DataTable dt = new System.Data.DataTable();
                    System.Data.IDataReader reader = GetDataReader(sql, Conn);
                    dt.Load(reader);
                    return dt;
                }
            }
            catch (Exception ex)
            {
                LogTools.WriteSystemErrorLog(MethodBase.GetCurrentMethod().DeclaringType, MethodBase.GetCurrentMethod().Name, "sql: " + sql + "错误信息：" + ex.Message);
                throw new Exception("sql: " + sql + "错误信息：" + ex.Message);
            }
            return null;
        }
        /************************************************************************
          Function:  执行sql语句                                      
          In:   string sqlstr, DbConnection                                                        
          Out:  void
          Author: DQS
          Date: 2012-08-28
        ************************************************************************/
        public static void ExecuteSql(string sqlstr, System.Data.Common.DbConnection Conn)
        {
            if (Conn == null)
            {
                LogTools.WriteSystemErrorLog(MethodBase.GetCurrentMethod().DeclaringType, MethodBase.GetCurrentMethod().Name, "数据库连接为空");
                return;
            }
            try
            {
                if (Conn.State == System.Data.ConnectionState.Closed)
                {
                    Conn.Open();
                }
                if (Conn.State == ConnectionState.Open)
                {
                    using (System.Data.IDbCommand cmd = Conn.CreateCommand())
                    {
                        cmd.CommandText = sqlstr;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                LogTools.WriteSystemErrorLog(MethodBase.GetCurrentMethod().DeclaringType, MethodBase.GetCurrentMethod().Name, "sql: " + sqlstr + "错误信息：" + ex.Message);
                throw new Exception("sql: " + sqlstr + "错误信息：" + ex.Message);
            }
        }

    }
}
