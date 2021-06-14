// <copyright file="AlterTableRemoveTemporalOperation.cs" company="Washington University in St. Louis">
// Copyright (c) 2021 Washington University in St. Louis. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
// </copyright>

namespace WUSTL.EMed.EntityFramework.SqlServer.Temporal.Operations
{
    using System;
    using System.Data.Entity.Migrations.Model;
    using System.Text;
    using WUSTL.EMed.EntityFramework.SqlServer.Temporal.Utils;

    /// <summary>
    /// An operation to remove system versioning (temporal) froma table.
    /// </summary>
    public class AlterTableRemoveTemporalOperation : SqlOperation
    {
        private string schema;
        private string name;
        private string historyName;
        private string startColumnName;
        private string endColumnName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AlterTableRemoveTemporalOperation"/> class.
        /// </summary>
        /// <param name="name">The table name.</param>
        /// <param name="schema">The schema that contains the table, or null to use the default schema.</param>
        /// <param name="historyName">The history table name.</param>
        /// <param name="startColumnName">The name of the temporal start timestamp column.</param>
        /// <param name="endColumnName">The name of the temporal end timestamp column.</param>
        /// <param name="dropHistoryTable">True if the history table should be dropped; otherwise, false.</param>
        /// <param name="dropTimestampColumns">True if the timestamp columns should be dropped; otherwise, false.</param>
        public AlterTableRemoveTemporalOperation(string name, string schema, string historyName, string startColumnName, string endColumnName, bool dropHistoryTable, bool dropTimestampColumns)
            : base(";", null)
        {
            Name = name;
            Schema = schema;
            HistoryName = historyName;
            StartColumnName = startColumnName;
            EndColumnName = endColumnName;
            DropHistoryTable = dropHistoryTable;
            DropTimestampColumns = dropTimestampColumns;
        }

        /// <summary>
        /// Gets or sets the schema containing the table.
        /// </summary>
        public string Schema
        {
            get => schema;
            set => schema = string.IsNullOrEmpty(value) ? null : value;
        }

        /// <summary>
        /// Gets or sets the name of the table.
        /// </summary>
        public string Name
        {
            get => name;
            set => name = string.IsNullOrEmpty(value) ? throw new ArgumentNullException(nameof(value)) : value;
        }

        /// <summary>
        /// Gets or sets the name of the history table.
        /// </summary>
        public string HistoryName
        {
            get => historyName;
            set => historyName = string.IsNullOrEmpty(value) ? null : value;
        }

        /// <summary>
        /// Gets or sets the name of the temporal start timestamp column.
        /// </summary>
        public string StartColumnName
        {
            get => startColumnName;
            set => startColumnName = string.IsNullOrEmpty(value) ? null : value;
        }

        /// <summary>
        /// Gets or sets the name of the temporal end timestamp column.
        /// </summary>
        public string EndColumnName
        {
            get => endColumnName;
            set => endColumnName = string.IsNullOrEmpty(value) ? null : value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the history table should be dropped; default is true.
        /// </summary>
        public bool DropHistoryTable { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the temporal timestamp columns should be dropped; default is true.
        /// </summary>
        public bool DropTimestampColumns { get; set; } = true;

        /// <inheritdoc/>
        public override bool IsDestructiveChange => true;

        /// <inheritdoc/>
        public override string Sql
        {
            get
            {
                var schema = SqlUtils.EscapeIdentifier(this.schema ?? "dbo");
                var name = SqlUtils.EscapeIdentifier(this.name);
                var historyName = SqlUtils.EscapeIdentifier(this.historyName ?? $"{this.name}_History");
                var startColumnName = SqlUtils.EscapeIdentifier(this.startColumnName ?? "SysTimeStart");
                var endColumnName = SqlUtils.EscapeIdentifier(this.endColumnName ?? "SysTimeEnd");
                var dropHistoryTable = DropHistoryTable;
                var dropTimestampColumns = DropTimestampColumns;

                var sqlBuilder = new StringBuilder();
                sqlBuilder.AppendLine($"ALTER TABLE [{schema}].[{name}] SET (SYSTEM_VERSIONING = OFF);");
                sqlBuilder.AppendLine($"ALTER TABLE [{schema}].[{name}] DROP PERIOD FOR SYSTEM_TIME;");

                if (dropHistoryTable)
                {
                    sqlBuilder.AppendLine($"DROP TABLE [{schema}].[{historyName}];");
                }

                if (dropTimestampColumns)
                {
                    sqlBuilder.AppendLine(RemoveDefaultConstraintSql(schema, name, startColumnName));
                    sqlBuilder.AppendLine($"ALTER TABLE [{schema}].[{name}] DROP COLUMN [{startColumnName}];");
                    sqlBuilder.AppendLine(RemoveDefaultConstraintSql(schema, name, endColumnName));
                    sqlBuilder.AppendLine($"ALTER TABLE [{schema}].[{name}] DROP COLUMN [{endColumnName}];");
                }

                return sqlBuilder.ToString();
            }
        }

        private static string RemoveDefaultConstraintSql(string schema, string name, string columnName)
        {
            var sqlBuilder = new StringBuilder();
            sqlBuilder.AppendLine($"EXECUTE(N'"); // Wrap the whole thing in an execute to avoid variable collisions.
            sqlBuilder.AppendLine($"    DECLARE @var NVARCHAR(128)");
            sqlBuilder.AppendLine($"    SELECT @var = REPLACE([name], '']'', '']]'')");
            sqlBuilder.AppendLine($"    FROM [sys].[default_constraints]");
            sqlBuilder.AppendLine($"    WHERE [parent_object_id] = object_id(N''[{schema}].[{name}]'')");
            sqlBuilder.AppendLine($"    AND REPLACE(col_name([parent_object_id], [parent_column_id]), '']'', '']]'') = ''{columnName}'';"); // We need this replace because currently we're passing in an already escaped name.
            sqlBuilder.AppendLine($"    IF @var IS NOT NULL");
            sqlBuilder.AppendLine($"        EXECUTE(N''ALTER TABLE [{schema}].[{name}] DROP CONSTRAINT ['' + @var + '']'')");
            sqlBuilder.Append($"');");

            return sqlBuilder.ToString();
        }
    }
}
