// <copyright file="IDbMigrationExtensions.cs" company="Washington University in St. Louis">
// Copyright (c) 2021 Washington University in St. Louis. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
// </copyright>

namespace WUSTL.EMed.EntityFramework.SqlServer.Temporal
{
    using System;
    using System.Data.Entity.Migrations.Infrastructure;
    using WUSTL.EMed.EntityFramework.SqlServer.Temporal.Operations;

    /// <summary>
    /// A collection of <see cref="IDbMigration"/> extension methods.
    /// </summary>
    public static class IDbMigrationExtensions
    {
        /// <summary>
        /// Alters a table to disable system versioning.
        /// </summary>
        /// <param name="migration">An <see cref="IDbMigration"/> instance.</param>
        /// <param name="name">The table name.</param>
        /// <param name="schema">The schema that contains the table, or null to use the default schema.</param>
        public static void AlterTableDisableTemporal(this IDbMigration migration, string name, string schema = "dbo")
        {
            if (migration is null)
            {
                throw new ArgumentNullException(nameof(migration));
            }

            migration.AddOperation(new AlterTableDisableTemporalOperation(name, schema));
        }

        /// <summary>
        /// Alters a table to enable system versioning.
        /// </summary>
        /// <param name="migration">An <see cref="IDbMigration"/> instance.</param>
        /// <param name="name">The table name.</param>
        /// <param name="schema">The schema that contains the table, or null to use the default schema.</param>
        /// <param name="historyName">The history table name.</param>
        public static void AlterTableEnableTemporal(this IDbMigration migration, string name, string schema = "dbo", string historyName = null)
        {
            if (migration is null)
            {
                throw new ArgumentNullException(nameof(migration));
            }

            migration.AddOperation(new AlterTableEnableTemporalOperation(name, schema, historyName));
        }

        /// <summary>
        /// Alters a table to make it system versioned (temporal).
        /// </summary>
        /// <param name="migration">An <see cref="IDbMigration"/> instance.</param>
        /// <param name="name">The table name.</param>
        /// <param name="schema">The schema that contains the table, or null to use the default schema.</param>
        /// <param name="historyName">The history table name.</param>
        /// <param name="startColumnName">The name of the temporal start timestamp column.</param>
        /// <param name="endColumnName">The name of the temporal end timestamp column.</param>
        public static void AlterTableMakeTemporal(this IDbMigration migration, string name, string schema = "dbo", string historyName = null, string startColumnName = "SysTimeStart", string endColumnName = "SysTimeEnd")
        {
            if (migration is null)
            {
                throw new ArgumentNullException(nameof(migration));
            }

            migration.AddOperation(new AlterTableMakeTemporalOperation(name, schema, historyName, startColumnName, endColumnName));
        }

        /// <summary>
        /// Alters a table to remove system versioning (temporal).
        /// </summary>
        /// <param name="migration">An <see cref="IDbMigration"/> instance.</param>
        /// <param name="name">The table name.</param>
        /// <param name="schema">The schema that contains the table, or null to use the default schema.</param>
        /// <param name="historyName">The history table name.</param>
        /// <param name="startColumnName">The name of the temporal start timestamp column.</param>
        /// <param name="endColumnName">The name of the temporal end timestamp column.</param>
        /// <param name="dropHistoryTable">True if the history table should be dropped; otherwise, false.</param>
        /// <param name="dropTimestampColumns">True if the timestamp columns should be dropped; otherwise, false.</param>
        public static void AlterTableRemoveTemporal(this IDbMigration migration, string name, string schema = "dbo", string historyName = null, string startColumnName = "SysTimeStart", string endColumnName = "SysTimeEnd", bool dropHistoryTable = true, bool dropTimestampColumns = true)
        {
            if (migration is null)
            {
                throw new ArgumentNullException(nameof(migration));
            }

            migration.AddOperation(new AlterTableRemoveTemporalOperation(name, schema, historyName, startColumnName, endColumnName, dropHistoryTable, dropTimestampColumns));
        }
    }
}
