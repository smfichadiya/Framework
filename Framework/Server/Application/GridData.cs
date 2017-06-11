﻿namespace Framework.Server.Application
{
    using Framework.Server.Application.Json;
    using Framework.Server.DataAccessLayer;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Reflection;

    internal class GridRowServer
    {
        public Row Row;

        public Row RowNew;

        /// <summary>
        /// Filter with parsed and valid parameters.
        /// </summary>
        public Row RowFilter; // List<Row> for multiple parameters.

        /// <summary>
        /// Gets or sets error attached to row.
        /// </summary>
        public string Error;

        internal int IsSelect;

        internal bool IsClick;
    }

    internal class GridCellServer
    {
        /// <summary>
        /// Gets or sets user modified text.
        /// </summary>
        public string Text;

        /// <summary>
        /// Gets or sets error attached to cell.
        /// </summary>
        public string Error;

        public bool IsSelect;

        public bool IsModify;

        public bool IsClick;
    }

    internal class GridColumnServer
    {
        public bool IsClick;
    }

    internal class GridQueryServer
    {
        public string FieldNameOrderBy;

        public bool IsOrderByDesc;
    }

    public class GridDataServer
    {

        /// <summary>
        /// Returns user modified text. If null, user has not changed text.
        /// </summary>
        public string CellText(string gridName, string index, string fieldName)
        {
            string result = null;
            GridCellServer cell = CellGet(gridName, index, fieldName);
            if (cell != null)
            {
                return cell.Text;
            }
            return result;
        }

        /// <summary>
        /// Returns list of loaded GridName.
        /// </summary>
        public List<string> GridNameList()
        {
            List<string> result = new List<string>(queryList.Keys);
            return result;
        }

        /// <summary>
        /// Returns column definitions.
        /// </summary>
        public List<Cell> ColumnList(string gridName)
        {
            Type typeRow = TypeRowGet(gridName);
            return DataAccessLayer.Util.ColumnList(typeRow);
        }

        /// <summary>
        /// Returns list of loaded row index.
        /// </summary>
        public List<string> IndexList(string gridName)
        {
            return new List<string>(rowList[gridName].Keys);
        }

        public Type TypeRow(string gridName)
        {
            return TypeRowGet(gridName);
        }

        /// <summary>
        /// (GridName, TypeRow)
        /// </summary>
        private Dictionary<string, Type> typeRowList = new Dictionary<string, Type>();

        private Type TypeRowGet(string gridName)
        {
            Type result;
            typeRowList.TryGetValue(gridName, out result);
            return result;
        }

        private void TypeRowSet(string gridName, Type typeRow)
        {
            typeRowList[gridName] = typeRow;
            if (typeRow == null)
            {
                typeRowList.Remove(gridName);
            }
        }

        /// <summary>
        /// Returns data row.
        /// </summary>
        public Row Row(string gridName, string index)
        {
            Row result = null;
            var row = RowGet(gridName, index);
            if (row != null)
            {
                result = row.Row;
            }
            return result;
        }

        /// <summary>
        /// (GridName, GridQueryServer).
        /// </summary>
        private Dictionary<string, GridQueryServer> queryList = new Dictionary<string, GridQueryServer>();

        private GridQueryServer QueryGet(string gridName)
        {
            if (!queryList.ContainsKey(gridName))
            {
                queryList[gridName] = new GridQueryServer();
            }
            return queryList[gridName];
        }

        /// <summary>
        /// (GridName, FieldName, GridColumnServer).
        /// </summary>
        private Dictionary<string, Dictionary<string, GridColumnServer>> columnList = new Dictionary<string, Dictionary<string, GridColumnServer>>();

        private GridColumnServer ColumnGet(string gridName, string fieldName)
        {
            if (!columnList.ContainsKey(gridName))
            {
                columnList[gridName] = new Dictionary<string, GridColumnServer>();
            }
            if (!columnList[gridName].ContainsKey(fieldName))
            {
                columnList[gridName][fieldName] = new GridColumnServer();
            }
            //
            return columnList[gridName][fieldName];
        }

        /// <summary>
        /// (GridName, Index). Original row as loaded from json.
        /// </summary>
        private Dictionary<string, Dictionary<string, GridRowServer>> rowList = new Dictionary<string, Dictionary<string, GridRowServer>>();

        private GridRowServer RowGet(string gridName, string index)
        {
            GridRowServer result = null;
            if (rowList.ContainsKey(gridName))
            {
                if (rowList[gridName].ContainsKey(index))
                {
                    result = rowList[gridName][index];
                }
            }
            return result;
        }

        private void RowSet(string gridName, string index, GridRowServer gridRowServer)
        {
            if (!rowList.ContainsKey(gridName))
            {
                rowList[gridName] = new Dictionary<string, GridRowServer>();
            }
            rowList[gridName][index] = gridRowServer;
        }

        /// <summary>
        /// (GridName, Index, FieldName, Text).
        /// </summary>
        private Dictionary<string, Dictionary<string, Dictionary<string, GridCellServer>>> cellList = new Dictionary<string, Dictionary<string, Dictionary<string, GridCellServer>>>();

        private GridCellServer CellGet(string gridName, string index, string fieldName)
        {
            GridCellServer result = null;
            if (cellList.ContainsKey(gridName))
            {
                if (cellList[gridName].ContainsKey(index))
                {
                    if (cellList[gridName][index].ContainsKey(fieldName))
                    {
                        result = cellList[gridName][index][fieldName];
                    }
                }
            }
            if (result == null)
            {
                result = new GridCellServer();
                if (!cellList.ContainsKey(gridName))
                {
                    cellList[gridName] = new Dictionary<string, Dictionary<string, GridCellServer>>();
                }
                if (!cellList[gridName].ContainsKey(index))
                {
                    cellList[gridName][index] = new Dictionary<string, GridCellServer>();
                }
                cellList[gridName][index][fieldName] = result;
            }
            return result;
        }

        /// <summary>
        /// Returns error attached to data row.
        /// </summary>
        private string ErrorRowGet(string gridName, string index)
        {
            return RowGet(gridName, index).Error;
        }

        /// <summary>
        /// Set error on data row.
        /// </summary>
        private void ErrorRowSet(string gridName, string index, string text)
        {
            RowGet(gridName, index).Error = text;
        }

        /// <summary>
        /// Gets user entered text.
        /// </summary>
        /// <returns>If null, user has not changed text.</returns>
        private string CellTextGet(string gridName, string index, string fieldName)
        {
            return CellGet(gridName, index, fieldName).Text;
        }

        /// <summary>
        /// Sets user entered text.
        /// </summary>
        /// <param name="text">If null, user has not changed text.</param>
        private void CellTextSet(string gridName, string index, string fieldName, string text)
        {
            CellGet(gridName, index, fieldName).Text = text;
        }

        /// <summary>
        /// Clear all user modified text for row.
        /// </summary>
        private void TextClear(string gridName, string index)
        {
            if (cellList.ContainsKey(gridName))
            {
                if (cellList[gridName].ContainsKey(index))
                {
                    foreach (GridCellServer gridCellServer in cellList[gridName][index].Values)
                    {
                        gridCellServer.Text = null;
                    }
                }
            }
        }

        private string ErrorCellGet(string gridName, string index, string fieldName)
        {
            return CellGet(gridName, index, fieldName).Error;
        }

        private void ErrorCellSet(string gridName, string index, string fieldName, string text)
        {
            CellGet(gridName, index, fieldName).Error = text;
        }

        /// <summary>
        /// Returns true, if data row contains text parse error.
        /// </summary>
        private bool IsErrorRowCell(string gridName, string index)
        {
            bool result = false;
            if (cellList.ContainsKey(gridName))
            {
                if (cellList[gridName].ContainsKey(index))
                {
                    foreach (string fieldName in cellList[gridName][index].Keys)
                    {
                        if (cellList[gridName][index][fieldName].Error != null)
                        {
                            result = true;
                            break;
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Returns true, if text on row has been modified.
        /// </summary>
        private bool IsModifyRowCell(string gridName, string index)
        {
            bool result = false;
            Type typeRow = TypeRowGet(gridName);
            if (cellList.ContainsKey(gridName))
            {
                if (cellList[gridName].ContainsKey(index))
                {
                    foreach (Cell column in DataAccessLayer.Util.ColumnList(typeRow))
                    {
                        if (column.FieldNameSql != null) // Exclude calculated column
                        {
                            string fieldName = column.FieldNameCSharp;
                            if (cellList[gridName][index].ContainsKey(fieldName))
                            {
                                if (cellList[gridName][index][fieldName].IsModify)
                                {
                                    result = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Load data from Sql database.
        /// </summary>
        public void LoadDatabase(string gridName, List<Filter> filterList, string fieldNameOrderBy, bool isOrderByDesc, Type typeRow)
        {
            TypeRowSet(gridName, typeRow);
            List<Row> rowList = DataAccessLayer.Util.Select(typeRow, filterList, fieldNameOrderBy, isOrderByDesc, 0, 15);
            LoadRow(gridName, typeRow, rowList);
        }

        private void LoadDatabase(string gridName, out List<Filter> filterList)
        {
            Type typeRow = TypeRowGet(gridName);
            filterList = new List<Filter>();
            Row row = RowGet(gridName, Util.IndexEnumToString(IndexEnum.Filter)).RowFilter; // Data row with parsed filter values.
            
            foreach (Cell column in DataAccessLayer.Util.ColumnList(typeRow))
            {
                string fieldName = column.FieldNameCSharp;
                string text = CellTextGet(gridName, Util.IndexEnumToString(IndexEnum.Filter), fieldName);
                if (text == "")
                {
                    text = null;
                }
                if (text != null) // Use filter only when text set.
                {
                    if (column.FieldNameSql != null) // Do not filter on calculated column.
                    {
                        object value = row.GetType().GetProperty(fieldName).GetValue(row);
                        FilterOperator filterOperator = FilterOperator.Equal;
                        if (value is string)
                        {
                            filterOperator = FilterOperator.Like;
                        }
                        else
                        {
                            if (text.Contains(">"))
                            {
                                filterOperator = FilterOperator.Greater;
                            }
                            if (text.Contains("<"))
                            {
                                filterOperator = FilterOperator.Greater;
                            }
                        }
                        filterList.Add(new Filter() { FieldName = fieldName, FilterOperator = filterOperator, Value = value });
                    }
                }
            }
        }

        /// <summary>
        /// Load data from database with current grid filter and current sorting.
        /// </summary>
        public void LoadDatabase(string gridName)
        {
            if (!IsErrorRowCell(gridName, Util.IndexEnumToString(IndexEnum.Filter))) // Do not reload data grid if there is text parse error in filter.
            {
                string fieldNameOrderBy = queryList[gridName].FieldNameOrderBy;
                bool isOrderByDesc = queryList[gridName].IsOrderByDesc;
                Type typeRow = TypeRowGet(gridName);
                List<Filter> filterList;
                LoadDatabase(gridName, out filterList);
                LoadDatabase(gridName, filterList, fieldNameOrderBy, isOrderByDesc, typeRow);
            }
        }

        /// <summary>
        /// Load data from list into data grid.
        /// </summary>
        public void LoadRow(string gridName, Type typeRow, List<Row> rowList)
        {
            if (typeRow == null || rowList == null)
            {
                TypeRowSet(gridName, typeRow);
                cellList.Remove(gridName);
                this.rowList.Remove(gridName);
            }
            else
            {
                foreach (Row row in rowList)
                {
                    Framework.Util.Assert(row.GetType() == typeRow);
                }
                //
                Dictionary<string, GridCellServer> cellListFilter = null;
                if (cellList.ContainsKey(gridName))
                {
                    cellList[gridName].TryGetValue(Util.IndexEnumToString(IndexEnum.Filter), out cellListFilter); // Save filter user text.
                }
                cellList.Remove(gridName); // Clear user modified text and attached errors.
                this.rowList[gridName] = new Dictionary<string, GridRowServer>(); // Clear data
                TypeRowSet(gridName, typeRow);
                //
                RowFilterAdd(gridName);
                for (int index = 0; index < rowList.Count; index++)
                {
                    RowSet(gridName, index.ToString(), new GridRowServer() { Row = rowList[index], RowNew = null });
                }
                RowNewAdd(gridName);
                //
                if (cellListFilter != null)
                {
                    cellList[gridName] = new Dictionary<string, Dictionary<string, GridCellServer>>();
                    cellList[gridName][Util.IndexEnumToString(IndexEnum.Filter)] = cellListFilter; // Load back filter user text.
                }
            }
        }

        /// <summary>
        /// Load data from single row into data grid.
        /// </summary>
        public void LoadRow(string gridName, Row row)
        {
            if (row != null)
            {
                List<Row> rowList = new List<Row>();
                rowList.Add(row);
                LoadRow(gridName, row.GetType(), rowList);
            }
        }

        /// <summary>
        /// Add data grid filter row.
        /// </summary>
        private void RowFilterAdd(string gridName)
        {
            RowSet(gridName, Util.IndexEnumToString(IndexEnum.Filter), new GridRowServer() { Row = null, RowNew = null });
        }

        /// <summary>
        /// Add data row of enum New to RowList.
        /// </summary>
        private void RowNewAdd(string gridName)
        {
            // (Index)
            Dictionary<string, GridRowServer> rowListCopy = rowList[gridName];
            rowList[gridName] = new Dictionary<string, GridRowServer>();
            // Filter
            foreach (string index in rowListCopy.Keys)
            {
                if (Util.IndexToIndexEnum(index) == IndexEnum.Filter)
                {
                    RowSet(gridName, index, rowListCopy[index]);
                    break;
                }
            }
            // Index
            int indexInt = 0;
            foreach (string index in rowListCopy.Keys)
            {
                IndexEnum indexEnum = Util.IndexToIndexEnum(index);
                if (indexEnum == IndexEnum.Index || indexEnum == IndexEnum.New)
                {
                    RowSet(gridName, indexInt.ToString(), rowListCopy[index]); // New becomes Index
                    indexInt += 1;
                }
            }
            // New
            RowSet(gridName, Util.IndexEnumToString(IndexEnum.New), new GridRowServer() { Row = null, RowNew = null }); // New row
            // Total
            foreach (string index in rowListCopy.Keys)
            {
                if (Util.IndexToIndexEnum(index) == IndexEnum.Total)
                {
                    RowSet(gridName, index, rowListCopy[index]);
                    break;
                }
            }
        }

        /// <summary>
        /// Save data to sql database.
        /// </summary>
        public void SaveDatabase()
        {
            foreach (string gridName in rowList.Keys.ToArray())
            {
                foreach (string index in rowList[gridName].Keys.ToArray())
                {
                    IndexEnum indexEnum = Util.IndexToIndexEnum(index);
                    if (indexEnum == IndexEnum.Index || indexEnum == IndexEnum.New) // Exclude Filter and Total.
                    {
                        if (!IsErrorRowCell(gridName, index)) // No save if data row has text parse error!
                        {
                            if (IsModifyRowCell(gridName, index)) // Only save row if user modified row on latest request.
                            {
                                var row = rowList[gridName][index];
                                if (row.Row != null && row.RowNew != null) // Database Update
                                {
                                    try
                                    {
                                        DataAccessLayer.Util.Update(row.Row, row.RowNew);
                                        ErrorRowSet(gridName, index, null);
                                        row.Row = row.RowNew;
                                        TextClear(gridName, index);
                                    }
                                    catch (Exception exception)
                                    {
                                        ErrorRowSet(gridName, index, Framework.Util.ExceptionToText(exception));
                                    }
                                }
                                if (row.Row == null && row.RowNew != null) // Database Insert
                                {
                                    try
                                    {
                                        DataAccessLayer.Util.Insert(row.RowNew);
                                        ErrorRowSet(gridName, index, null);
                                        row.Row = row.RowNew;
                                        TextClear(gridName, index);
                                        RowNewAdd(gridName); // Make "New" to "Index" and add "New"
                                    }
                                    catch (Exception exception)
                                    {
                                        ErrorRowSet(gridName, index, Framework.Util.ExceptionToText(exception));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Parse user modified input text. See also method TextSet(); when parse error occurs method ErrorSet(); is called for the field.
        /// </summary>
        public void TextParse()
        {
            foreach (string gridName in rowList.Keys)
            {
                foreach (string index in rowList[gridName].Keys)
                {
                    if (IsModifyRowCell(gridName, index))
                    {
                        Type typeRow = TypeRowGet(gridName);
                        var row = RowGet(gridName, index);
                        if (row.Row != null)
                        {
                            Framework.Util.Assert(row.Row.GetType() == typeRow);
                        }
                        IndexEnum indexEnum = Util.IndexToIndexEnum(index);
                        Row rowWrite;
                        switch (indexEnum)
                        {
                            case IndexEnum.Index:
                                rowWrite = DataAccessLayer.Util.RowClone(row.Row);
                                row.RowNew = rowWrite;
                                break;
                            case IndexEnum.New:
                                rowWrite = DataAccessLayer.Util.RowCreate(typeRow);
                                row.RowNew = rowWrite;
                                break;
                            case IndexEnum.Filter:
                                rowWrite = DataAccessLayer.Util.RowCreate(typeRow);
                                row.RowFilter = rowWrite;
                                break;
                            default:
                                throw new Exception("Enum unknown!");
                        }
                        foreach (string fieldName in cellList[gridName][index].Keys)
                        {
                            string text = CellTextGet(gridName, index, fieldName);
                            if (text != null)
                            {
                                if (text == "")
                                {
                                    text = null;
                                }
                                if (!(text == null && indexEnum == IndexEnum.Filter)) // Do not parse text null for filter.
                                {
                                    object value;
                                    try
                                    {
                                        value = DataAccessLayer.Util.ValueFromText(text, rowWrite.GetType().GetProperty(fieldName).PropertyType); // Parse text.
                                    }
                                    catch (Exception exception)
                                    {
                                        ErrorCellSet(gridName, index, fieldName, exception.Message);
                                        row.RowNew = null; // Do not save.
                                        break;
                                    }
                                    rowWrite.GetType().GetProperty(fieldName).SetValue(rowWrite, value);
                                }
                            }
                            ErrorCellSet(gridName, index, fieldName, null); // Clear error.
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Load Query data from json.
        /// </summary>
        private void LoadJsonQuery(ApplicationJson applicationJson)
        {
            GridDataJson gridDataJson = applicationJson.GridDataJson;
            //
            foreach (string gridName in gridDataJson.GridQueryList.Keys)
            {
                GridQuery gridQuery = gridDataJson.GridQueryList[gridName];
                GridQueryServer gridQueryServer = QueryGet(gridName);
                gridQueryServer.FieldNameOrderBy = gridQuery.FieldNameOrderBy;
                gridQueryServer.IsOrderByDesc = gridQuery.IsOrderByDesc;
            }
        }

        /// <summary>
        /// Load GridColumn data from json.
        /// </summary>
        private void LoadJsonColumn(ApplicationJson applicationJson)
        {
            GridDataJson gridDataJson = applicationJson.GridDataJson;
            //
            foreach (string gridName in gridDataJson.ColumnList.Keys)
            {
                foreach (GridColumn gridColumn in gridDataJson.ColumnList[gridName])
                {
                    GridColumnServer gridColumnServer = ColumnGet(gridName, gridColumn.FieldName);
                    gridColumnServer.IsClick = gridColumn.IsClick;
                }
            }
        }

        /// <summary>
        /// Load data from http json request.
        /// </summary>
        public void LoadJson(ApplicationJson applicationJson, string gridName, Type typeInAssembly)
        {
            LoadJsonQuery(applicationJson);
            LoadJsonColumn(applicationJson);
            //
            GridDataJson gridDataJson = applicationJson.GridDataJson;
            //
            string typeRowName = gridDataJson.GridQueryList[gridName].TypeRowName;
            Type typeRow = DataAccessLayer.Util.TypeRowFromName(typeRowName, typeInAssembly);
            TypeRowSet(gridName, typeRow);
            //
            foreach (GridRow row in gridDataJson.RowList[gridName])
            {
                IndexEnum indexEnum = Util.IndexToIndexEnum(row.Index);
                Row resultRow = null;
                if (indexEnum == IndexEnum.Index)
                {
                    resultRow = (Row)Activator.CreateInstance(typeRow);
                }
                GridRowServer gridRowServer = new GridRowServer() { Row = resultRow, IsSelect = row.IsSelect, IsClick = row.IsClick };
                RowSet(gridName, row.Index, gridRowServer);
                foreach (var column in gridDataJson.ColumnList[gridName])
                {
                    CellGet(gridName, row.Index, column.FieldName).IsSelect = gridDataJson.CellList[gridName][column.FieldName][row.Index].IsSelect;
                    CellGet(gridName, row.Index, column.FieldName).IsClick = gridDataJson.CellList[gridName][column.FieldName][row.Index].IsClick;
                    CellGet(gridName, row.Index, column.FieldName).IsModify = gridDataJson.CellList[gridName][column.FieldName][row.Index].IsModify;
                    string text;
                    if (gridDataJson.CellList[gridName][column.FieldName][row.Index].IsO)
                    {
                        text = gridDataJson.CellList[gridName][column.FieldName][row.Index].O; // Original text.
                        string textModify = gridDataJson.CellList[gridName][column.FieldName][row.Index].T; // User modified text.
                        CellTextSet(gridName, row.Index, column.FieldName, textModify);
                    }
                    else
                    {
                        text = gridDataJson.CellList[gridName][column.FieldName][row.Index].T; // Original text.
                    }
                    // ErrorField
                    string errorFieldText = gridDataJson.CellList[gridName][column.FieldName][row.Index].E;
                    if (errorFieldText != null)
                    {
                        ErrorCellSet(gridName, row.Index, column.FieldName, errorFieldText);
                    }
                    // ErrorRow
                    string errorRowText = row.Error;
                    if (errorRowText != null)
                    {
                        ErrorRowSet(gridName, row.Index, errorRowText);
                    }
                    if (indexEnum == IndexEnum.Index)
                    {
                        PropertyInfo propertyInfo = typeRow.GetProperty(column.FieldName);
                        object value = DataAccessLayer.Util.ValueFromText(text, propertyInfo.PropertyType);
                        propertyInfo.SetValue(resultRow, value);
                    }
                }
            }
        }

        /// <summary>
        /// Load data from GridDataJson to GridDataServer.
        /// </summary>
        public void LoadJson(ApplicationJson applicationJson, Type typeInAssembly)
        {
            GridDataJson gridDataJson = applicationJson.GridDataJson;
            //
            foreach (string gridName in gridDataJson.GridQueryList.Keys)
            {
                LoadJson(applicationJson, gridName, typeInAssembly);
            }
        }

        /// <summary>
        /// Returns row's columns.
        /// </summary>
        private static List<GridColumn> TypeRowToGridColumn(Type typeRow)
        {
            var result = new List<GridColumn>();
            //
            var columnList = Framework.Server.DataAccessLayer.Util.ColumnList(typeRow);
            double widthPercentTotal = 0;
            bool isLast = false;
            for (int i = 0; i < columnList.Count; i++)
            {
                // Text
                string text = columnList[i].FieldNameSql;
                if (text == null)
                {
                    text = columnList[i].FieldNameCSharp; // Calculated column.
                }
                columnList[i].ColumnText(ref text);
                // WidthPercent
                isLast = i == columnList.Count;
                double widthPercentAvg = Math.Round(((double)100 - widthPercentTotal) / ((double)columnList.Count - (double)i), 2);
                double widthPercent = widthPercentAvg;
                columnList[i].ColumnWidthPercent(ref widthPercent);
                widthPercent = Math.Round(widthPercent, 2);
                if (isLast)
                {
                    widthPercent = 100 - widthPercentTotal;
                }
                else
                {
                    if (widthPercentTotal + widthPercent > 100)
                    {
                        widthPercent = widthPercentAvg;
                    }
                }
                widthPercentTotal = widthPercentTotal + widthPercent;
                result.Add(new GridColumn() { FieldName = columnList[i].FieldNameCSharp, Text = text, WidthPercent = widthPercent });
            }
            return result;
        }

        /// <summary>
        /// Save column state to Json.
        /// </summary>
        private void SaveJsonColumn(ApplicationJson applicationJson)
        {
            GridDataJson gridDataJson = applicationJson.GridDataJson;
            //
            foreach (string gridName in gridDataJson.ColumnList.Keys)
            {
                foreach (GridColumn gridColumn in gridDataJson.ColumnList[gridName])
                {
                    GridColumnServer gridColumnServer = ColumnGet(gridName, gridColumn.FieldName);
                    gridColumn.IsClick = gridColumnServer.IsClick;
                }
            }
        }

        /// <summary>
        /// Save Query back to Json.
        /// </summary>
        private void SaveJsonQuery(ApplicationJson applicationJson)
        {
            GridDataJson gridDataJson = applicationJson.GridDataJson;
            //
            foreach (string gridName in queryList.Keys)
            {
                GridQueryServer gridQueryServer = queryList[gridName];
                GridQuery gridQuery = gridDataJson.GridQueryList[gridName];
                gridQuery.FieldNameOrderBy = gridQueryServer.FieldNameOrderBy;
                gridQuery.IsOrderByDesc = gridQueryServer.IsOrderByDesc;
            }
        }

        /// <summary>
        /// Copy data from class GridDataServer to class GridDataJson.
        /// </summary>
        public void SaveJson(ApplicationJson applicationJson)
        {
            GridDataJson gridDataJson = applicationJson.GridDataJson;
            //
            if (gridDataJson.GridQueryList == null)
            {
                gridDataJson.GridQueryList = new Dictionary<string, GridQuery>();
            }
            //
            foreach (string gridName in rowList.Keys)
            {
                Type typeRow = TypeRowGet(gridName);
                gridDataJson.GridQueryList[gridName] = new GridQuery() { GridName = gridName, TypeRowName = DataAccessLayer.Util.TypeRowToName(typeRow) };
                // Row
                if (gridDataJson.RowList == null)
                {
                    gridDataJson.RowList = new Dictionary<string, List<GridRow>>();
                }
                gridDataJson.RowList[gridName] = new List<GridRow>();
                // Column
                if (gridDataJson.ColumnList == null)
                {
                    gridDataJson.ColumnList = new Dictionary<string, List<GridColumn>>();
                }
                gridDataJson.ColumnList[gridName] = TypeRowToGridColumn(typeRow);
                // Cell
                if (gridDataJson.CellList == null)
                {
                    gridDataJson.CellList = new Dictionary<string, Dictionary<string, Dictionary<string, GridCell>>>();
                }
                gridDataJson.CellList[gridName] = new Dictionary<string, Dictionary<string, GridCell>>();
                //
                PropertyInfo[] propertyInfoList = null;
                foreach (string index in rowList[gridName].Keys)
                {
                    GridRowServer gridRowServer = rowList[gridName][index];
                    string errorRow = ErrorRowGet(gridName, index);
                    GridRow gridRow = new GridRow() { Index = index, IsSelect = gridRowServer.IsSelect, IsClick = gridRowServer.IsClick, Error = errorRow };
                    gridDataJson.RowList[gridName].Add(gridRow);
                    if (propertyInfoList == null && typeRow != null)
                    {
                        propertyInfoList = typeRow.GetTypeInfo().GetProperties();
                    }
                    if (propertyInfoList != null)
                    {
                        foreach (PropertyInfo propertyInfo in propertyInfoList)
                        {
                            string fieldName = propertyInfo.Name;
                            object value = null;
                            if (gridRowServer.Row != null)
                            {
                                value = propertyInfo.GetValue(gridRowServer.Row);
                            }
                            string textJson = DataAccessLayer.Util.ValueToText(value);
                            string text = CellTextGet(gridName, index, fieldName);
                            GridCellServer gridCellServer = CellGet(gridName, index, fieldName);
                            if (!gridDataJson.CellList[gridName].ContainsKey(fieldName))
                            {
                                gridDataJson.CellList[gridName][fieldName] = new Dictionary<string, GridCell>();
                            }
                            string errorCell = ErrorCellGet(gridName, index, fieldName);
                            GridCell gridCell = new GridCell() { IsSelect = gridCellServer.IsSelect, IsClick = gridCellServer.IsClick, IsModify = gridCellServer.IsModify, E = errorCell };
                            gridDataJson.CellList[gridName][fieldName][index] = gridCell;
                            if (text == null)
                            {
                                gridCell.T = textJson;
                            }
                            else
                            {
                                gridCell.O = textJson;
                                gridCell.T = text;
                                gridCell.IsO = true;
                            }
                        }
                    }
                }
            }
            SaveJsonColumn(applicationJson);
            SaveJsonQuery(applicationJson);
        }
    }
}
