// <copyright file="AlterTableMakeTemporalOperation.cs" company="Washington University in St. Louis">
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
    /// An operation to make a table system versioned (temporal).
    /// </summary>
    public class AlterTableMakeTemporalOperation : SqlOperation
    {
        private string schema;
        private string name;
        private string historyName;
        private string startColumnName;
        private string endColumnName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AlterTableMakeTemporalOperation"/> class.
        /// </summary>
        /// <param name="name">The table name.</param>
        /// <param name="schema">The schema that contains the table, or null to use the default schema.</param>
        /// <param name="historyName">The history table name.</param>
        /// <param name="startColumnName">The name of the temporal start timestamp column.</param>
        /// <param name="endColumnName">The name of the temporal end timestamp column.</param>
        public AlterTableMakeTemporalOperation(string name, string schema, string historyName, string startColumnName, string endColumnName)
            : base(";", null)
        {
            Name = name;
            Schema = schema;
            HistoryName = historyName;
            StartColumnName = startColumnName;
            EndColumnName = endColumnName;
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

        /// <inheritdoc/>
        public override bool IsDestructiveChange => false;

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

                var sqlBuilder = new StringBuilder();
                sqlBuilder.AppendLine($@"ALTER TABLE [{schema}].[{name}] ADD");
                sqlBuilder.AppendLine($"[{startColumnName}] [DATETIME2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL DEFAULT SYSUTCDATETIME(),");
                sqlBuilder.AppendLine($"[{endColumnName}]   [DATETIME2](7) GENERATED ALWAYS AS ROW END   HIDDEN NOT NULL DEFAULT CAST('9999-12-31 23:59:59.9999999' AS DATETIME2(7)),");
                sqlBuilder.AppendLine($"PERIOD FOR SYSTEM_TIME ([{startColumnName}], [{endColumnName}]);");
                sqlBuilder.AppendLine($"ALTER TABLE [{schema}].[{name}] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [{schema}].[{historyName}]));");

                return sqlBuilder.ToString();
            }
        }
    }
}
