// <copyright file="SqlServerMigrationSqlGenerator.cs" company="Washington University in St. Louis">
// Copyright (c) 2021 Washington University in St. Louis. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
// </copyright>

namespace WUSTL.EMed.EntityFramework.SqlServer.Temporal.SqlGenerators
{
    using System;
    using System.Data.Entity.Migrations.Model;
    using WUSTL.EMed.EntityFramework.SqlServer.Temporal.Operations;

    /// <summary>
    /// Generates SQL server statements for temporal operations.
    /// </summary>
    public class SqlServerMigrationSqlGenerator : System.Data.Entity.SqlServer.SqlServerMigrationSqlGenerator
    {
        /// <inheritdoc/>
        protected override void Generate(MigrationOperation migrationOperation)
        {
            base.Generate(migrationOperation);

            if (migrationOperation is AlterTableDisableTemporalOperation disableOperation)
            {
                var schema = EscapeIdentifier(disableOperation.Schema ?? "dbo");
                var name = EscapeIdentifier(disableOperation.Name);

                using (var writer = Writer())
                {
                    writer.WriteLine($"ALTER TABLE [{schema}].[{name}] SET (SYSTEM_VERSIONING = OFF);");

                    Statement(writer);
                }
            }

            if (migrationOperation is AlterTableEnableTemporalOperation enableOperation)
            {
                var schema = EscapeIdentifier(enableOperation.Schema ?? "dbo");
                var name = EscapeIdentifier(enableOperation.Name);
                var historyName = EscapeIdentifier(enableOperation.HistoryName ?? $"{enableOperation.Name}_History");

                using (var writer = Writer())
                {
                    writer.WriteLine($"ALTER TABLE [{schema}].[{name}] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [{schema}].[{historyName}]));");

                    Statement(writer);
                }
            }

            if (migrationOperation is AlterTableMakeTemporalOperation makeOperation)
            {
                var schema = EscapeIdentifier(makeOperation.Schema ?? "dbo");
                var name = EscapeIdentifier(makeOperation.Name);
                var historyName = EscapeIdentifier(makeOperation.HistoryName ?? $"{makeOperation.Name}_History");
                var startColumnName = EscapeIdentifier(makeOperation.StartColumnName ?? "SysTimeStart");
                var endColumnName = EscapeIdentifier(makeOperation.EndColumnName ?? "SysTimeEnd");

                using (var writer = Writer())
                {
                    writer.WriteLine($"ALTER TABLE [{schema}].[{name}] ADD");
                    writer.WriteLine($"[{startColumnName}] [DATETIME2](7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL DEFAULT SYSUTCDATETIME(),");
                    writer.WriteLine($"[{endColumnName}]   [DATETIME2](7) GENERATED ALWAYS AS ROW END   HIDDEN NOT NULL DEFAULT CAST('9999-12-31 23:59:59.9999999' AS DATETIME2(7)),");
                    writer.WriteLine($"PERIOD FOR SYSTEM_TIME ([{startColumnName}], [{endColumnName}]);");

                    Statement(writer);
                }

                using (var writer = Writer())
                {
                    writer.WriteLine($"ALTER TABLE [{schema}].[{name}] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [{schema}].[{historyName}]));");

                    Statement(writer);
                }
            }

            if (migrationOperation is AlterTableRemoveTemporalOperation removeOperation)
            {
                var schema = EscapeIdentifier(removeOperation.Schema ?? "dbo");
                var name = EscapeIdentifier(removeOperation.Name);
                var historyName = EscapeIdentifier(removeOperation.HistoryName ?? $"{removeOperation.Name}_History");
                var startColumnName = EscapeIdentifier(removeOperation.StartColumnName ?? "SysTimeStart");
                var endColumnName = EscapeIdentifier(removeOperation.EndColumnName ?? "SysTimeEnd");
                var dropHistoryTable = removeOperation.DropHistoryTable;
                var dropTimestampColumns = removeOperation.DropTimestampColumns;

                using (var writer = Writer())
                {
                    writer.WriteLine($"ALTER TABLE [{schema}].[{name}] SET (SYSTEM_VERSIONING = OFF);");

                    Statement(writer);
                }

                using (var writer = Writer())
                {
                    writer.WriteLine($"ALTER TABLE [{schema}].[{name}] DROP PERIOD FOR SYSTEM_TIME;");

                    Statement(writer);
                }

                if (dropHistoryTable)
                {
                    using (var writer = Writer())
                    {
                        writer.WriteLine($"DROP TABLE [{schema}].[{historyName}];");

                        Statement(writer);
                    }
                }

                if (dropTimestampColumns)
                {
                    using (var writer = Writer())
                    {
                        writer.WriteLine(RemoveDefaultConstraintSql(schema, name, startColumnName));

                        StatementBatch(writer.ToString());
                    }

                    using (var writer = Writer())
                    {
                        writer.WriteLine($"ALTER TABLE [{schema}].[{name}] DROP COLUMN [{startColumnName}];");

                        Statement(writer);
                    }

                    using (var writer = Writer())
                    {
                        writer.WriteLine(RemoveDefaultConstraintSql(schema, name, endColumnName));

                        Statement(writer);
                    }

                    using (var writer = Writer())
                    {
                        writer.WriteLine($"ALTER TABLE [{schema}].[{name}] DROP COLUMN [{endColumnName}];");

                        Statement(writer);
                    }
                }
            }
        }

        private static string EscapeIdentifier(string s)
        {
            if (s is null)
            {
                throw new ArgumentNullException(nameof(s));
            }

            return s.Replace("]", "]]");
        }

        private static string RemoveDefaultConstraintSql(string schema, string tableName, string columnName)
        {
            return $@"
DECLARE @sql NVARCHAR(MAX);

SELECT
    @sql = 'ALTER TABLE [' + REPLACE([s].[name], ']', ']]') + '].[' + REPLACE([t].[name], ']', ']]') + '] DROP CONSTRAINT [' + REPLACE([d].[name], ']', ']]') + ']'
FROM
               [sys].[default_constraints] AS [d]
    INNER JOIN [sys].[tables]              AS [t] ON [d].[parent_object_id] = [t].[object_id]
    INNER JOIN [sys].[columns]             AS [c] ON [d].[parent_object_id] = [c].[object_id] AND [d].[parent_column_id] = [c].[column_id]
    INNER JOIN [sys].[schemas]             AS [s] ON [t].[schema_id] = [s].[schema_id]
WHERE
        [d].[type] = 'D' AND [s].[name] = '{schema}' AND [t].[name] = '{tableName}' AND [c].[name] = '{columnName}';

IF (@sql != '')
BEGIN
    EXEC sp_executesql @sql;
END;";
        }
    }
}
