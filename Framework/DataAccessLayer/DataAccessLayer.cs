﻿namespace Framework.DataAccessLayer
{
    using Framework.Application;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Base class for every database row.
    /// </summary>
    public class Row
    {
        protected virtual void IsReadOnly(ref bool result)
        {

        }

        /// <summary>
        /// Update data row on database.
        /// </summary>
        /// <param name="row">Old data row</param>
        /// <param name="rowNew">New data row. Set properties for example to read back updated content from db.</param>
        protected virtual internal void Update(App app, GridName gridName, Index index, Row row, Row rowNew)
        {
            UtilFramework.Assert(this == rowNew);
            if (app.GridData.IsModifyRowCell(gridName, index, true)) // No update on database, if only calculated column has been modified.
            {
                UtilDataAccessLayer.Update(row, this);
            }
        }

        /// <summary>
        /// Override this method for example to save data to underlying database tables from sql view.
        /// </summary>
        protected virtual internal void Insert(App app, GridName gridName, Index index, Row rowNew)
        {
            UtilFramework.Assert(rowNew == this);
            if (app.GridData.IsModifyRowCell(gridName, index, true)) // No insert on database, if only calculated column has been modified.
            {
                UtilDataAccessLayer.Insert(this);
            }
        }

        /// <summary>
        /// Override this method to filter detail grid when master row has been clicked.
        /// </summary>
        /// <param name="gridNameMaster">Master gridName.</param>
        /// <param name="rowMaster">Clicked master grid row.</param>
        /// <param name="isReload">If true, this grid (detail) gets reloaded. Override also method Row.Where(); to filter detail grid.</param>
        protected virtual internal void MasterIsClick(App app, GridName gridNameMaster, Row rowMaster, ref bool isReload)
        {

        }

        protected virtual internal IQueryable Query(App app, GridName gridName)
        {
            return UtilDataAccessLayer.Query(GetType());
        }
    }

    /// <summary>
    /// Base class for every database field.
    /// </summary>
    public class Cell
    {
        /// <summary>
        /// Constructor for column.
        /// </summary>
        internal void Constructor(string tableNameSql, string fieldNameSql, string fieldNameCSharp, Type typeRow, Type typeField, PropertyInfo propertyInfo)
        {
            this.TableNameSql = tableNameSql;
            this.FieldNameSql = fieldNameSql;
            this.FieldNameCSharp = fieldNameCSharp;
            this.TypeRow = typeRow;
            this.TypeField = typeField;
            this.PropertyInfo = propertyInfo;
        }

        /// <summary>
        /// Constructor for column and cell. Switch between column and cell mode. (Column mode: row = null; Cell mode: row != null).
        /// </summary>
        internal Cell Constructor(object row)
        {
            this.Row = row;
            return this;
        }

        /// <summary>
        /// Gets sql TableName.
        /// </summary>
        public string TableNameSql { get; private set; }


        /// <summary>
        /// Gets sql FieldName. If null, then it's a calculated column.
        /// </summary>
        public string FieldNameSql { get; private set; }

        /// <summary>
        /// Gets CSharp FieldName.
        /// </summary>
        public string FieldNameCSharp { get; private set; }

        /// <summary>
        /// Gets TypeRow.
        /// </summary>
        public Type TypeRow { get; private set; }

        /// <summary>
        /// Gets TypeField.
        /// </summary>
        public Type TypeField { get; private set; }

        internal PropertyInfo PropertyInfo { get; private set; }

        /// <summary>
        /// Gets Row. Null, if column.
        /// </summary>
        public object Row { get; private set; }

        protected virtual internal void InfoColumn(App app, GridNameTypeRow gridName, InfoColumn result)
        {

        }

        protected virtual internal void InfoCell(App app, GridName gridName, Index index, InfoCell result)
        {

        }

        /// <summary>
        /// Parse user entered text.
        /// </summary>
        protected virtual internal void CellTextParse(App app, GridName gridName, Index index, string fieldName, string text)
        {
            object value = UtilDataAccessLayer.RowValueFromText(text, Row.GetType().GetProperty(fieldName).PropertyType); // Default parse text.
            Row.GetType().GetProperty(fieldName).SetValue(Row, value);
        }

        /// <summary>
        /// Override for custom formatting like adding units of measurement. Called after method UtilDataAccessLayer.RowValueToText(); Inverse function is CellValueFromText.
        /// </summary>
        protected virtual internal void CellRowValueToText(App app, GridName gridName, Index index, ref string result)
        {

        }

        /// <summary>
        /// Override to parse custom formating like value with units of measurement. Called before user entered text is parsed with method UtilDataAccessLayer.ValueFromText(); Inverse function is CellValueToText.
        /// </summary>
        protected virtual internal void CellRowValueFromText(App app, GridName gridName, Index index, ref string result)
        {
            
        }

        protected virtual internal void ColumnWidthPercent(ref double widthPercent)
        {

        }

        /// <summary>
        /// Values user can select from lookup list.
        /// </summary>
        /// <param name="query">Database query or in-memeory list.</param>
        protected virtual internal void CellLookup(App app, GridName gridName, Index index, string fieldName, out IQueryable query)
        {
            query = null;
        }

        /// <summary>
        /// Override to handle clicked Lookup row.
        /// </summary>
        /// <param name="gridName">Grid with open lookup.</param>
        /// <param name="index">Row with open lookup.</param>
        /// <param name="rowLookup">LoowUp row which has been clicked.</param>
        /// <param name="fieldNameLookup">Field which has been clicked.</param>
        protected virtual internal void CellLookupIsClick(App app, GridName gridName, Index index, string fieldName, Row rowLookup, string fieldNameLookup, string text)
        {
            CellTextParse(app, gridName, index, fieldName, text);
        }

        /// <summary>
        /// Override this method to handle button click event. For example delete button.
        /// </summary>
        protected virtual internal void CellButtonIsClick(App app, GridName gridName, Index index, Row row, string fieldName, ref bool isReload)
        {

        }

        /// <summary>
        /// Gets or sets Value. Throws exception if cell is in column mode.
        /// </summary>
        public object Value
        {
            get
            {
                if (Row == null) // Column mode!
                {
                    return null;
                }
                return PropertyInfo.GetValue(Row);
            }
            set
            {
                UtilFramework.Assert(Row != null, "Column mode!");
                PropertyInfo.SetValue(Row, value);
            }
        }
    }

    /// <summary>
    /// Base class for every database field.
    /// </summary>
    public class Cell<TRow> : Cell
    {
        public new TRow Row
        {
            get
            {
                return (TRow)base.Row;
            }
        }
    }

    /// <summary>
    /// Sql schema name and table name.
    /// </summary>
    public class SqlTableAttribute : Attribute
    {
        public SqlTableAttribute(string sqlSchemaName, string sqlTableName)
        {
            this.SqlSchemaName = sqlSchemaName;
            this.SqlTableName = sqlTableName;
        }

        public readonly string SqlSchemaName;

        public readonly string SqlTableName;
    }

    /// <summary>
    /// Sql column name.
    /// </summary>
    public class SqlColumnAttribute : Attribute
    {
        public SqlColumnAttribute(string sqlColumnName, Type typeCell)
        {
            this.SqlColumnName = sqlColumnName;
            this.TypeCell = typeCell;
        }

        public readonly string SqlColumnName;

        public readonly Type TypeCell;
    }
}
