// //------------------------------------------------------------------------------
// // <copyright company="4 Roads Ltd">
// //     Copyright (c) 4 Roads Ltd.  All rights reserved.
// // </copyright>
// //------------------------------------------------------------------------------

#region

using System;
using System.Text;
using FourRoads.Common.Interfaces;

#endregion

namespace FourRoads.Common
{
    [Obsolete("QueryBuilder is obsolete. Please use FourRoads.Common.Sql.QueryBuilder instead")]
    public abstract class QueryBuilder
    {
        private ISqlDataChecker _dataChecker = Injector.Get<ISqlDataChecker>();
        private string _dbo;
        private int _settingsID;

        public QueryBuilder(string dbo)
        {
            _dbo = dbo;
        }

        [Obsolete("This constructor is no longer used. please use QueryBuilder(string dbo) constructor instead", false)]
        public QueryBuilder(int settingsID, string dbo)
        {
            _dbo = dbo;
            _settingsID = settingsID;
        }

        public ISqlDataChecker DataChecker
        {
            get { return _dataChecker; }
        }

        public string Dbo
        {
            get { return _dbo; }
        }

        [Obsolete("The SettingID property is obsolete and no longer used")]
        public int SettingsID
        {
            get { return _settingsID; }
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
    }
}