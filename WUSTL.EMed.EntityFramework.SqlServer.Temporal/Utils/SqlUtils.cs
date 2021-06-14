// <copyright file="SqlUtils.cs" company="Washington University in St. Louis">
// Copyright (c) 2021 Washington University in St. Louis. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
// </copyright>

namespace WUSTL.EMed.EntityFramework.SqlServer.Temporal.Utils
{
    using System;

    /// <summary>
    /// A static class of sql utility methods.
    /// </summary>
    internal static class SqlUtils
    {
        /// <summary>
        /// Escapes a string for use in a sql server quoted identifier name.
        /// </summary>
        /// <param name="s">The string to escape.</param>
        /// <returns>The given string where "]" has been replaced with "]]".</returns>
        internal static string EscapeIdentifier(string s)
        {
            if (s is null)
            {
                throw new ArgumentNullException(nameof(s));
            }

            return s.Replace("]", "]]");
        }

        /// <summary>
        /// Escapes a string for use in a sql server string literal.
        /// </summary>
        /// <param name="s">The string to escape.</param>
        /// <returns>The given string where "'" has been replaced with "''".</returns>
        internal static string EscapeString(string s)
        {
            if (s is null)
            {
                throw new ArgumentNullException(nameof(s));
            }

            return s.Replace("'", "''");
        }
    }
}
