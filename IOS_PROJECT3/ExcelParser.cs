using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Threading.Tasks;

namespace IOS_PROJECT3
{
    public class ExcelParser
    {
        public List<string> ReadColumn(string path, int column)
        {

            OleDbConnection conn = new OleDbConnection();
            //conn.ConnectionString = conn.ConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +path
            // +";Extended Properties='Excel 12.0 Xml;HDR=NO;IMEX=1;MAXSCANROWS=0'";
            conn.ConnectionString = String.Format(@"Provider=Microsoft.ACE.OLEDB.12.0;Excel 12.0 Xml;HDR=No;Data Source={0}", path);
            conn.Open();
            var dtSchema = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
            var Sheet1 = dtSchema.Rows[0].Field<string>("TABLE_NAME");
            var comm = new OleDbCommand();
            comm.Connection = conn;
            comm.CommandText = String.Format("Select * from [{0}]", Sheet1);
            OleDbDataAdapter adapter = new OleDbDataAdapter(comm.CommandText, conn);
            DataTable dsXLS = new DataTable();
            adapter.Fill(dsXLS);
            List<string> result = new List<string>();

            var foo = dsXLS.Rows;
            for (int i = 0; i < foo.Count; i++)
            {
                result.Add(foo[i].ItemArray[column].ToString());
            }
            conn.Close();
            return result;
        }
    }
}
