// //------------------------------------------------------------------------------
// // <copyright company="Four Roads LLC">
// //     Copyright (c) Four Roads LLC.  All rights reserved.
// // </copyright>
// //------------------------------------------------------------------------------

#region

using System;
using System.Text;
using FourRoads.Common.Interfaces;

#endregion

namespace FourRoads.Common.Sql
{
    public abstract class SqlQueryBuilder
    {
        private ISqlDataHelper _dataHelper = Injector.Get<ISqlDataHelper>();
        private string _dbo;

        public SqlQueryBuilder(string dbo)
        {
            _dbo = dbo;
        }

        public ISqlDataHelper DataHelper
        {
            get { return _dataHelper; }
        }

        public string Dbo
        {
            get { return _dbo; }
        }

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
            return string.Format("[{0}].[{1}]", Dbo, dbObjectName); 
        }

        protected abstract string AddQuerySelect();

        protected abstract string AddJoins();

        protected abstract string AddWhereClause();

        protected abstract string AddOrderBy();

        public virtual string BuildSQLQuery()
        {
            StringBuilder bld = new StringBuilder(300);

            bld.Append(AddQuerySelect());

            bld.Append(AddJoins());

            bld.Append(AddWhereClause());

            bld.Append(AddOrderBy());

            return bld.ToString();
        }

        protected virtual string AddQueryCountSelect()
        {
            return null;
        }

        public virtual string BuildSQLCountQuery()
        {
            StringBuilder bld = new StringBuilder(300);

            string queryCount = AddQueryCountSelect();
            if (queryCount == null)
                throw new NotImplementedException(
                    "You must implement AddQueryCountSelect before calling BuildSQLCountQuery");

            bld.Append(queryCount);

            bld.Append(AddJoins());

            bld.Append(AddWhereClause());

            return bld.ToString();
        }
    }
}