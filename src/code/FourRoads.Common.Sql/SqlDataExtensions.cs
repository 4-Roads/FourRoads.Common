using System;
using System.Data;

namespace FourRoads.Common.Sql
{
    public enum DbType2
    {
        Text = 129,
        NText = 130
    }

    public static class SqlDataExtensions
    {
        public static SqlDbType ToSqlDbType(this DbType dataType)
        {
            switch (dataType)
            {
                case DbType.AnsiString:
                    return SqlDbType.VarChar;
                case DbType.Binary:
                    return SqlDbType.VarBinary;
                case DbType.Byte:
                    return SqlDbType.TinyInt;
                case DbType.Boolean:
                    return SqlDbType.Bit;
                case DbType.Currency:
                    return SqlDbType.Money;
                case DbType.Date:
                    return SqlDbType.DateTime;
                case DbType.DateTime:
                    return SqlDbType.DateTime;
                case DbType.Time:
                    return SqlDbType.DateTime;
                case DbType.DateTime2:
                    return SqlDbType.DateTime2;
                case DbType.Decimal:
                    return SqlDbType.Decimal;
                case DbType.Double:
                    return SqlDbType.Float;
                case DbType.Guid:
                    return SqlDbType.UniqueIdentifier;
                case DbType.Int16:
                    return SqlDbType.SmallInt;
                case DbType.Int32:
                    return SqlDbType.Int;
                case DbType.Int64:
                    return SqlDbType.BigInt;
                case DbType.Object:
                    return SqlDbType.Variant;
                case DbType.SByte:
                    throw new ArgumentException(string.Format("DbType '{0}' cannot be translated to SqlDbType", dataType));
                case DbType.Single:
                    return SqlDbType.Real;
                case DbType.String:
                    return SqlDbType.NVarChar;
                case DbType.UInt16:
                    throw new ArgumentException(string.Format("DbType '{0}' cannot be translated to SqlDbType", dataType));
                case DbType.UInt32:
                    throw new ArgumentException(string.Format("DbType '{0}' cannot be translated to SqlDbType", dataType));
                case DbType.UInt64:
                    throw new ArgumentException(string.Format("DbType '{0}' cannot be translated to SqlDbType", dataType));
                case DbType.VarNumeric:
                    throw new ArgumentException(string.Format("DbType '{0}' cannot be translated to SqlDbType", dataType));
                case DbType.AnsiStringFixedLength:
                    return SqlDbType.Char;
                case DbType.StringFixedLength:
                    return SqlDbType.NChar;
                case DbType.Xml:
                    return SqlDbType.Xml;
                case (DbType) DbType2.Text:
                    return SqlDbType.Text;
                case (DbType) DbType2.NText:
                    return SqlDbType.NText;
                default:
                    throw new ArgumentException(string.Format("Unkown DbType '{0}' cannot be translated to SqlDbType", dataType));
            }
        }

        public static DbType ToDbType(this SqlDbType dataType)
        {
            switch (dataType)
            {
                case SqlDbType.BigInt:
                    return DbType.Int64;
                case SqlDbType.Binary:
                    return DbType.Binary;
                case SqlDbType.Bit:
                    return DbType.Boolean;
                case SqlDbType.Char:
                    return DbType.AnsiStringFixedLength;
                case SqlDbType.DateTime:
                    return DbType.DateTime;
                case SqlDbType.Decimal:
                    return DbType.Decimal;
                case SqlDbType.Float:
                    return DbType.Double;
                case SqlDbType.Image:
                    return DbType.Binary;
                case SqlDbType.Int:
                    return DbType.Int32;
                case SqlDbType.Money:
                    return DbType.Currency;
                case SqlDbType.NChar:
                    return DbType.StringFixedLength;
                case SqlDbType.NText:
                    return DbType.String;
                case SqlDbType.NVarChar:
                    return DbType.String;
                case SqlDbType.Real:
                    return DbType.Single;
                case SqlDbType.UniqueIdentifier:
                    return DbType.Guid;
                case SqlDbType.SmallDateTime:
                    return DbType.DateTime;
                case SqlDbType.SmallInt:
                    return DbType.Int16;
                case SqlDbType.SmallMoney:
                    return DbType.Currency;
                case SqlDbType.Text:
                    return DbType.AnsiString;
                case SqlDbType.Timestamp:
                    return DbType.Binary;
                case SqlDbType.TinyInt:
                    return DbType.Byte;
                case SqlDbType.VarBinary:
                    return DbType.Binary;
                case SqlDbType.VarChar:
                    return DbType.AnsiString;
                case SqlDbType.Variant:
                    return DbType.Object;
                case SqlDbType.Xml:
                    return DbType.Xml;
                default:
                    throw new ArgumentException(string.Format("Unkown SqlDbType '{0}' cannot be translated to DbType", dataType));
            }
        }

        /// <summary>
        ///     Determines if the current <see cref="IDataRecord" /> instance contains a column with the specified name
        /// </summary>
        /// <param name="dr">The data record</param>
        /// <param name="columnName">The column name</param>
        /// <returns></returns>
        public static bool HasColumn(this IDataRecord dr, string columnName)
        {
            for (var i = 0; i < dr.FieldCount; i++)
            {
                if (dr.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}