// //------------------------------------------------------------------------------
// // <copyright company="4 Roads Ltd">
// //     Copyright (c) 4 Roads Ltd.  All rights reserved.
// // </copyright>
// //------------------------------------------------------------------------------

#region

using System.Text;

#endregion

namespace FourRoads.Common.Sql
{
    public abstract class SqlQueryBuilder
    {
        public SqlQueryBuilder(IDataHelper dataHelper)
        {
            DataHelper = dataHelper;
        }

        public IDataHelper DataHelper { get; }

        public string Dbo { get; set; }

        public string Database { get; set; }

        protected bool AppendString(bool requiresAnd, StringBuilder builder, string str)
        {
            if (requiresAnd)
            {
                builder.Append(" AND ");
            }

            builder.Append(str);

            return true;
        }

        protected string ObjectName(string dbObjectName)
        {
            var prepend = "";

            if (!string.IsNullOrWhiteSpace(Database))
                prepend = $"[{Database}].";

            if (!string.IsNullOrWhiteSpace(Dbo))
                prepend = prepend + $"[{Dbo}].";

            return $"{prepend}[{dbObjectName}]";
        }

        protected abstract string AddQuerySelect();

        protected abstract string AddJoins();

        protected abstract string AddWhereClause();

        protected abstract string AddOrderBy();

        public virtual string BuildSQLQuery()
        {
            var bld = new StringBuilder(300);

            bld.Append(AddQuerySelect());

            bld.Append(AddJoins());

            bld.Append(AddWhereClause());

            bld.Append(AddOrderBy());

            return bld.ToString();
        }
    }
}