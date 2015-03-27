// (c) Microsoft. 
//JSONBuilder used for PowerBI

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization; 

namespace PowerBIExtensionMethods
{ 
    //JSONBuilder used for PowerBI
    internal static class JSONBuilder 
    {
       
        /// <summary>
        /// Creates Json string for SQL tables
        /// </summary>
        /// <param name="tables">DataTable</param>
        /// <param name="dsName">string</param>
        /// <param name="tableName">string</param>
        /// <returns></returns>
        internal static string GetJsonSchema(DataTable tables, string dsName, string tableName)
        {
            StringBuilder jsonSchemaBuilder = new StringBuilder();
            string typeName = string.Empty;

            jsonSchemaBuilder.Append(string.Format("{0}\"name\": \"{1}\",\"tables\": [", "{", dsName));
            jsonSchemaBuilder.Append(String.Format("{0}\"name\": \"{1}\", ", "{", tableName));
            jsonSchemaBuilder.Append("\"columns\": [");
            foreach (DataColumn obj in tables.Columns)
            {
                
                switch (obj.DataType.ToString())
                {
                    case "System.Int32":
                    case "Int64":
                        typeName = "Int64";
                        break;
                    case "System.Double":
                        typeName = "Double";
                        break;
                    case "System.Boolean":
                        typeName = "bool";
                        break;
                    case "System.DateTime":
                        typeName = "DateTime";
                        break;
                    case "System.String":
                        typeName = "string";
                        break;
                    default:
                        typeName = "string";
                        break;
                }

                if (typeName == null)
                        throw new Exception("type not supported");

                    jsonSchemaBuilder.Append(string.Format("{0} \"name\": \"{1}\", \"dataType\": \"{2}\"{3},", "{", obj.Caption, typeName, "}"));
                
            }

            jsonSchemaBuilder.Remove(jsonSchemaBuilder.ToString().Length - 1, 1);
            jsonSchemaBuilder.Append("]}]}");

            return jsonSchemaBuilder.ToString();
        }

        /// <summary>
        /// Creates Json for multiple tables in one dataset.
        /// </summary>
        /// <param name="dtarrTables"></param>
        /// <param name="dsName"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        internal static string GetJsonSchema(DataSet dtarrTables, string dsName)
        {
            StringBuilder jsonSchemaBuilder = new StringBuilder();
            string typeName = string.Empty;

            jsonSchemaBuilder.Append(string.Format("{0}\"name\": \"{1}\",\"tables\": [", "{", dsName));
            foreach (DataTable tables in dtarrTables.Tables)
            {
                jsonSchemaBuilder.Append(String.Format("{0}\"name\": \"{1}\", ", "{", tables.TableName));
                jsonSchemaBuilder.Append("\"columns\": [");
                foreach (DataColumn obj in tables.Columns)
                {

                    switch (obj.DataType.ToString())
                    {
                        case "System.Int32":
                        case "Int64":
                            typeName = "Int64";
                            break;
                        case "System.Double":
                            typeName = "Double";
                            break;
                        case "System.Boolean":
                            typeName = "bool";
                            break;
                        case "System.DateTime":
                            typeName = "DateTime";
                            break;
                        case "System.String":
                            typeName = "string";
                            break;
                        default:
                            typeName = "string";
                            break;
                    }

                    if (typeName == null)
                        throw new Exception("type not supported");

                    jsonSchemaBuilder.Append(string.Format("{0} \"name\": \"{1}\", \"dataType\": \"{2}\"{3},", "{", obj.Caption, typeName, "}"));

                }
                jsonSchemaBuilder.Remove(jsonSchemaBuilder.ToString().Length - 1, 1);
                jsonSchemaBuilder.Append("]},");
            }

            jsonSchemaBuilder.Remove(jsonSchemaBuilder.ToString().Length - 1, 1);
            jsonSchemaBuilder.Append("]}]}");

            return jsonSchemaBuilder.ToString();
        }

        /// <summary>
        /// Converts data table data to Jason format.
        /// </summary>
        /// <param name="dataTable">DataTable</param>
        /// <returns>string, Jason format string</returns>
        internal static string ConvertDataTableTojSonString(DataTable dataTable)
        {
            System.Web.Script.Serialization.JavaScriptSerializer serializer =
                   new System.Web.Script.Serialization.JavaScriptSerializer();

            serializer.MaxJsonLength = Int32.MaxValue;
            List<Dictionary<String, Object>> tableRows = new List<Dictionary<String, Object>>();

            Dictionary<String, Object> row;

            foreach (DataRow dr in dataTable.Rows)
            {
                row = new Dictionary<String, Object>();
                foreach (DataColumn col in dataTable.Columns)
                {
                    row.Add(col.ColumnName, dr[col]);
                }
                tableRows.Add(row);
            }
            return serializer.Serialize(tableRows);
        }

        internal static T ToObject<T>(this string obj, int recursionDepth = 100)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.RecursionLimit = recursionDepth;

            string result = obj.Split(new Char[] { '[' })[1];
            result = (result.EndsWith("}")) ? result = result.Substring(0, result.Length - 1) : result;
            result = String.Format("[{0}", result);

            return serializer.Deserialize<T>(result);
        }

        internal static IEnumerable<Dictionary<string, object>> Datasets(this List<Object> datasets, string name)
        {
            IEnumerable<Dictionary<string, object>> q = from d in (from d in datasets select d as Dictionary<string, object>) where d["name"] as string == name select d;

            return q;
        }

    }

   public class JavaScriptConverter<T> : JavaScriptConverter where T : new()
    {
        private const string _dateFormat = "MM/dd/yyyy HH:mm:ss";

        public override IEnumerable<Type> SupportedTypes
        {
            get
            {
                return new[] { typeof(T) };
            }
        }

        public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            T p = new T();

            var props = typeof(T).GetProperties();

            foreach (string key in dictionary.Keys)
            {
                var prop = props.Where(t => t.Name == key).FirstOrDefault();
                if (prop != null)
                {
                    if (prop.PropertyType == typeof(DateTime))
                    {
                        prop.SetValue(p, DateTime.ParseExact(dictionary[key] as string, _dateFormat, DateTimeFormatInfo.InvariantInfo), null);
                    }
                    else
                    {
                        prop.SetValue(p, dictionary[key], null);
                    }
                }
            }

            return p;
        }

        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            T p = (T)obj;
            IDictionary<string, object> serialized = new Dictionary<string, object>();

            foreach (PropertyInfo pi in typeof(T).GetProperties())
            {
                if (pi.PropertyType == typeof(DateTime))
                {
                    serialized[pi.Name] = ((DateTime)pi.GetValue(p, null)).ToString(_dateFormat);
                }
                else
                {
                    serialized[pi.Name] = pi.GetValue(p, null);
                }

            }


            StringBuilder powerBIJson = new StringBuilder();


            return serialized;
        }

        public static JavaScriptSerializer GetSerializer()
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.RegisterConverters(new[] { new JavaScriptConverter<T>() });

            return serializer;
        }
    }
}
