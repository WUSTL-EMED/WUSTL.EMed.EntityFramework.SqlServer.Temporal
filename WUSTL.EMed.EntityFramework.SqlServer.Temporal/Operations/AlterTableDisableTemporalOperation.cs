// <copyright file="AlterTableDisableTemporalOperation.cs" company="Washington University in St. Louis">
// Copyright (c) 2021 Washington University in St. Louis. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
// </copyright>

namespace WUSTL.EMed.EntityFramework.SqlServer.Temporal.Operations
{
    using System;
    using System.Data.Entity.Migrations.Model;
    using WUSTL.EMed.EntityFramework.SqlServer.Temporal.Utils;

    /// <summary>
    /// An operation to disable system versioning on a table (temporal).
    /// </summary>
    public class AlterTableDisableTemporalOperation : SqlOperation
    {
        private string schema;
        private string name;

        /// <summary>
        /// Initializes a new instance of the <see cref="AlterTableDisableTemporalOperation"/> class.
        /// </summary>
        /// <param name="name">The table name.</param>
        /// <param name="schema">The schema that contains the table, or null to use the default schema.</param>
        public AlterTableDisableTemporalOperation(string name, string schema = "dbo")
            : base(";", null)
        {
            Name = name;
            Schema = schema;
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

        /// <inheritdoc/>
        public override bool IsDestructiveChange => false;

        /// <inheritdoc/>
        public override string Sql
        {
            get
            {
                var schema = SqlUtils.EscapeIdentifier(this.schema ?? "dbo");
                var name = SqlUtils.EscapeIdentifier(this.name);

                return $"ALTER TABLE [{schema}].[{name}] SET (SYSTEM_VERSIONING = OFF);";
            }
        }
    }
}
