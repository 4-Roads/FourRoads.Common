using System;
using System.Data;
using FourRoads.Common.Interfaces;
using FourRoads.Common.Sql;
using FourRoads.Common.Web.Tests.Entities;
using FourRoads.Common.Web.Tests.Queries;

namespace FourRoads.Common.Web.Tests.DataHandlers
{
    public class DirectoryDataHandler : EntityDataHandler<Directory>
    {
        public DirectoryDataHandler(IDataHelper dataHelper, IPagedCollectionFactory pagedCollectionFactory, IObjectFactory objectFactory)
            : base( dataHelper, pagedCollectionFactory, objectFactory)
        {
        }

        #region Overrides of EntityDataPopulator<Directory>

        public override void Add(IDBFactory factory, Directory entity)
        {
            using (var connection = factory.CreateConnection())
            {
                using (var command = factory.CreateCommand("fr_Directories_Add", connection))
                {
                    command.Parameters.Add(factory.CreateParameter("@Id", DbType.Int32,ParameterDirection.Output));
                    command.Parameters.Add(factory.CreateParameter("@ApplicationKey", entity.ApplicationKey, DbType.String));
                    command.Parameters.Add(factory.CreateParameter("@ApplicationId", entity.ApplicationId, DbType.Guid));
                    command.Parameters.Add(factory.CreateParameter("@ContainerId", entity.ContainerId, DbType.Guid));
                    command.Parameters.Add(factory.CreateParameter("@Name", DataHelper.MakeSafeValue(entity.Name), DbType.String));
                    command.Parameters.Add(factory.CreateParameter("@Description", DataHelper.MakeSafeValue(entity.Description), DbType.String));
                    command.Parameters.Add(factory.CreateParameter("@CreatorUserID", DataHelper.MakeSafeValue(entity.CreatedByUserId), DbType.Int32));
                    command.Parameters.Add(factory.CreateParameter("@CreatedDate",DataHelper.SafeSqlDateTimeFormat(entity.CreatedDate.ToUniversalTime()), DbType.DateTime));
                    command.Parameters.Add(factory.CreateParameter("@IsEnabled", entity.IsEnabled, DbType.Boolean));

                    connection.Open();
                    command.ExecuteNonQuery();
                    entity.Id = Convert.ToInt32(((IDataParameter) command.Parameters["@Id"]).Value);
                    connection.Close();
                }
            }
        }

        public override void Delete(IDBFactory factory, Directory entity)
        {
            using (var connection = factory.CreateConnection())
            {
                using (var command = factory.CreateCommand("fr_Directories_Delete", connection))
                {
                    command.Parameters.Add(factory.CreateParameter("@Id", entity.Id, DbType.Int32));

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }

        public override IPagedCollection<Directory> Get(IDBFactory factory, IPagedQueryV2 query)
        {
            IPagedCollection<Directory> results;

            using (var connection = factory.CreateConnection())
            {
                using (var command = factory.CreateCommand("fr_Directories_Query", connection))
                {
                    command.Parameters.Add(factory.CreateParameter("@PageIndex", query.PageIndex, DbType.Int32));
                    command.Parameters.Add(factory.CreateParameter("@PageSize", query.PageSize, DbType.Int32));
                    command.Parameters.Add(factory.CreateParameter("@TotalRecords", DbType.Int32, ParameterDirection.Output));

                    // Execute the command
                    connection.Open();

                    using (var dr = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        results = CreateEntityCollection(query, dr, (IDataParameter) command.Parameters["@TotalRecords"]);
                    }

                    connection.Close();
                }
            }
            return results;
        }

        public override void Update(IDBFactory factory, Directory entity)
        {
            using (var connection = factory.CreateConnection())
            {
                using (var command = factory.CreateCommand("fr_Directories_Update", connection))
                {
                    command.Parameters.Add(factory.CreateParameter("@Id", entity.Id, DbType.Int32));
                    command.Parameters.Add(factory.CreateParameter("@ApplicationId", entity.ApplicationId, DbType.Guid));
                    command.Parameters.Add(factory.CreateParameter("@ApplicationKey", entity.ApplicationKey, DbType.String));
                    command.Parameters.Add(factory.CreateParameter("@Name", DataHelper.MakeSafeValue(entity.Name), DbType.String));
                    command.Parameters.Add(factory.CreateParameter("@Description", DataHelper.MakeSafeValue(entity.Description), DbType.String));
                    command.Parameters.Add(factory.CreateParameter("@ContainerId", entity.ContainerId, DbType.Guid));
                    command.Parameters.Add(factory.CreateParameter("@IsIndexed", entity.IsIndexed, DbType.Boolean));
                    command.Parameters.Add(factory.CreateParameter("@IsEnabled", entity.IsEnabled, DbType.Boolean));

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }

        public override void PopulateEntityData(Directory entity, IDataReader dataReader)
        {
            entity.Id = Convert.ToInt32(dataReader["Id"]);
            entity.CreatedByUserId = Convert.ToInt32(dataReader[ "CreatorUserID" ]);
            entity.CreatedDate = Convert.ToDateTime(dataReader[ "CreatedDate" ]).ToLocalTime();
            entity.ApplicationId = new Guid(Convert.ToString(dataReader[ "ApplicationId" ]));
            entity.ContainerId = new Guid(Convert.ToString(dataReader[ "ContainerId" ]));
            entity.IsIndexed = Convert.ToBoolean(dataReader["IsIndexed"]);
            entity.IsEnabled = Convert.ToBoolean(dataReader[ "IsEnabled" ]);
            entity.Name = Convert.ToString(dataReader["Name"]);
            entity.Description = Convert.ToString(dataReader[ "Description" ]);
            entity.ApplicationKey = Convert.ToString(dataReader[ "ApplicationKey" ]);
        }

        #endregion
    }
}