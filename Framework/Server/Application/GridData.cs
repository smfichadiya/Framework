﻿namespace Framework.Server.Application
{
    using Framework.Server.Application.Json;
    using Framework.Server.DataAccessLayer;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class GridRowServer
    {
        public string GridName;

        public string Index;

        public Row Row;

        public Row RowNew;

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

        public bool IsClick;
    }

    internal class GridColumnServer
    {
        public bool IsClick;
    }

    internal class GridLoadServer
    {
        public string FieldNameOrderBy;

        public bool IsOrderByDesc;
    }

    public class GridDataServer
    {
        /// <summary>
        /// (GridName, TypeRow)
        /// </summary>
        private Dictionary<string, Type> TypeRowList = new Dictionary<string, Type>();

        private Dictionary<string, GridLoadServer> GridLoadList = new Dictionary<string, GridLoadServer>();

        private GridLoadServer GridLoadGet(string gridName)
        {
            if (!GridLoadList.ContainsKey(gridName))
            {
                GridLoadList[gridName] = new GridLoadServer();
            }
            return GridLoadList[gridName];
        }

        private Dictionary<string, Dictionary<string, GridColumnServer>> ColumnList = new Dictionary<string, Dictionary<string, GridColumnServer>>();

        private GridColumnServer ColumnGet(string gridName, string fieldName)
        {
            if (!ColumnList.ContainsKey(gridName))
            {
                ColumnList[gridName] = new Dictionary<string, GridColumnServer>();
            }
            if (!ColumnList[gridName].ContainsKey(fieldName))
            {
                ColumnList[gridName][fieldName] = new GridColumnServer();
            }
            //
            return ColumnList[gridName][fieldName];
        }

        /// <summary>
        /// (GridName, Index). Original row.
        /// </summary>
        private Dictionary<string, Dictionary<string, GridRowServer>> RowList = new Dictionary<string, Dictionary<string, GridRowServer>>();

        public GridRowServer RowGet(string gridName, string index)
        {
            GridRowServer result = null;
            if (RowList.ContainsKey(gridName))
            {
                if (RowList[gridName].ContainsKey(index))
                {
                    result = RowList[gridName][index];
                }
            }
            return result;
        }

        private void RowSet(GridRowServer gridRowServer)
        {
            string gridName = gridRowServer.GridName;
            string index = gridRowServer.Index;
            if (!RowList.ContainsKey(gridName))
            {
                RowList[gridName] = new Dictionary<string, GridRowServer>();
            }
            RowList[gridName][index] = gridRowServer;
        }

        /// <summary>
        /// (GridName, Index, FieldName, Text).
        /// </summary>
        private Dictionary<string, Dictionary<string, Dictionary<string, GridCellServer>>> CellList = new Dictionary<string, Dictionary<string, Dictionary<string, GridCellServer>>>();

        private GridCellServer CellGet(string gridName, string index, string fieldName)
        {
            GridCellServer result = null;
            if (CellList.ContainsKey(gridName))
            {
                if (CellList[gridName].ContainsKey(index))
                {
                    if (CellList[gridName][index].ContainsKey(fieldName))
                    {
                        result = CellList[gridName][index][fieldName];
                    }
                }
            }
            if (result == null)
            {
                result = new GridCellServer();
                if (!CellList.ContainsKey(gridName))
                {
                    CellList[gridName] = new Dictionary<string, Dictionary<string, GridCellServer>>();
                }
                if (!CellList[gridName].ContainsKey(index))
                {
                    CellList[gridName][index] = new Dictionary<string, GridCellServer>();
                }
                CellList[gridName][index][fieldName] = result;
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
        private string TextGet(string gridName, string index, string fieldName)
        {
            return CellGet(gridName, index, fieldName).Text;
        }

        /// <summary>
        /// Sets user entered text.
        /// </summary>
        /// <param name="text">If null, user has not changed text.</param>
        private void TextSet(string gridName, string index, string fieldName, string text)
        {
            CellGet(gridName, index, fieldName).Text = text;
        }

        /// <summary>
        /// Clear all user modified text for row.
        /// </summary>
        private void TextClear(string gridName, string index)
        {
            if (CellList.ContainsKey(gridName))
            {
                if (CellList[gridName].ContainsKey(index))
                {
                    foreach (GridCellServer gridCellServer in CellList[gridName][index].Values)
                    {
                        gridCellServer.Text = null;
                    }
                }
            }
        }

        /// <summary>
        /// Returns true, if user modified text on row.
        /// </summary>
        private bool IsTextModify(string gridName, string index)
        {
            bool result = false;
            if (CellList.ContainsKey(gridName))
            {
                if (CellList[gridName].ContainsKey(index))
                {
                    foreach (GridCellServer gridCellServer in CellList[gridName][index].Values)
                    {
                        if (gridCellServer.Text != null)
                        {
                            result = true;
                            break;
                        }
                    }
                }
            }
            return result;
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
        /// Delete all errors on row.
        /// </summary>
        private void ErrorCellClear(string gridName, string index)
        {
            if (CellList.ContainsKey(gridName))
            {
                if (CellList[gridName].ContainsKey(index))
                {
                    foreach (string fieldName in CellList[gridName][index].Keys)
                    {
                        CellList[gridName][index][fieldName].Error = null;
                    }
                }
            }
        }

        /// <summary>
        /// Load data from database.
        /// </summary>
        public void LoadDatabase(string gridName, Type typeRow)
        {
            TypeRowList[gridName] = typeRow;
            List<Row> rowList = DataAccessLayer.Util.Select(typeRow, 0, 15);
            LoadRow(gridName, typeRow, rowList);
        }

        /// <summary>
        /// Load data from list.
        /// </summary>
        public void LoadRow(string gridName, Type typeRow, List<Row> rowList)
        {
            if (rowList == null)
            {
                rowList = new List<Row>();
            }
            foreach (Row row in rowList)
            {
                Framework.Util.Assert(row.GetType() == typeRow);
            }
            //
            CellList.Remove(gridName); // Clear user modified text and attached errors.
            RowList[gridName] = new Dictionary<string, GridRowServer>(); // Clear data
            TypeRowList[gridName] = typeRow;
            //
            for (int index = 0; index < rowList.Count; index++)
            {
                RowSet(new GridRowServer() { GridName = gridName, Index = index.ToString(), Row = rowList[index], RowNew = null });
            }
        }

        /// <summary>
        /// Save data to database.
        /// </summary>
        public void SaveDatabase()
        {
            foreach (string gridName in RowList.Keys)
            {
                foreach (string index in RowList[gridName].Keys)
                {
                    ErrorRowSet(gridName, index, null);
                }
            }
            //
            foreach (string gridName in RowList.Keys)
            {
                foreach (string index in RowList[gridName].Keys)
                {
                    var row = RowList[gridName][index];
                    if (row.RowNew != null)
                    {
                        try
                        {
                            DataAccessLayer.Util.Update(row.Row, row.RowNew);
                            row.Row = row.RowNew;
                            TextClear(gridName, index);
                        }
                        catch (Exception exception)
                        {
                            ErrorRowSet(gridName, index, Framework.Util.ExceptionToText(exception));
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
            foreach (string gridName in RowList.Keys)
            {
                foreach (string index in RowList[gridName].Keys)
                {
                    ErrorCellClear(gridName, index);
                }
            }
            //
            foreach (string gridName in RowList.Keys)
            {
                foreach (string index in RowList[gridName].Keys)
                {
                    if (IsTextModify(gridName, index))
                    {
                        var row = RowGet(gridName, index);
                        foreach (string fieldName in CellList[gridName][index].Keys)
                        {
                            if (row.RowNew == null)
                            {
                                row.RowNew = DataAccessLayer.Util.Clone(row.Row);
                            }
                            string text = TextGet(gridName, index, fieldName);
                            if (text != null)
                            {
                                if (text == "")
                                {
                                    text = null;
                                }
                                object value;
                                try
                                {
                                    value = DataAccessLayer.Util.ValueFromText(text, row.RowNew.GetType().GetProperty(fieldName).PropertyType); // Parse text.
                                }
                                catch (Exception exception)
                                {
                                    ErrorCellSet(gridName, index, fieldName, exception.Message);
                                    row.RowNew = null; // Do not save.
                                    break;
                                }
                                row.RowNew.GetType().GetProperty(fieldName).SetValue(row.RowNew, value);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Load GridLoad data from json.
        /// </summary>
        private void LoadJsonGridLoad(ApplicationJson applicationJson)
        {
            GridDataJson gridDataJson = applicationJson.GridDataJson;
            //
            foreach (string gridName in gridDataJson.GridLoadList.Keys)
            {
                GridLoad gridLoad = gridDataJson.GridLoadList[gridName];
                GridLoadServer gridLoadServer = GridLoadGet(gridName);
                gridLoadServer.FieldNameOrderBy = gridLoad.FieldNameOrderBy;
                gridLoadServer.IsOrderByDesc = gridLoad.IsOrderByDesc;
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
        /// Load data from GridDataJson to GridDataServer.
        /// </summary>
        public void LoadJson(ApplicationJson applicationJson, string gridName, Type typeInAssembly)
        {
            LoadJsonGridLoad(applicationJson);
            LoadJsonColumn(applicationJson);
            //
            GridDataJson gridDataJson = applicationJson.GridDataJson;
            //
            string typeRowName = gridDataJson.GridLoadList[gridName].TypeRowName;
            Type typeRow = DataAccessLayer.Util.TypeRowFromName(typeRowName, typeInAssembly);
            TypeRowList[gridName] = typeRow;
            //
            foreach (GridRow row in gridDataJson.RowList[gridName])
            {
                Row resultRow = (Row)Activator.CreateInstance(typeRow);
                GridRowServer gridRowServer = new GridRowServer() { GridName = gridName, Index = row.Index, Row = resultRow, IsSelect = row.IsSelect, IsClick = row.IsClick };
                RowSet(gridRowServer);
                foreach (var column in gridDataJson.ColumnList[gridName])
                {
                    CellGet(gridName, row.Index, column.FieldName).IsSelect = gridDataJson.CellList[gridName][column.FieldName][row.Index].IsSelect;
                    CellGet(gridName, row.Index, column.FieldName).IsClick = gridDataJson.CellList[gridName][column.FieldName][row.Index].IsClick;
                    string text;
                    if (gridDataJson.CellList[gridName][column.FieldName][row.Index].IsO)
                    {
                        text = gridDataJson.CellList[gridName][column.FieldName][row.Index].O; // Original text.
                        string textModify = gridDataJson.CellList[gridName][column.FieldName][row.Index].T; // User modified text.
                        TextSet(gridName, row.Index, column.FieldName, textModify);
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
                    PropertyInfo propertyInfo = typeRow.GetProperty(column.FieldName);
                    object value = DataAccessLayer.Util.ValueFromText(text, propertyInfo.PropertyType);
                    propertyInfo.SetValue(resultRow, value);
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
            foreach (string gridName in gridDataJson.GridLoadList.Keys)
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
            var cellList = Framework.Server.DataAccessLayer.Util.ColumnList(typeRow);
            double widthPercentTotal = 0;
            bool isLast = false;
            for (int i = 0; i < cellList.Count; i++)
            {
                // Text
                string text = cellList[i].FieldNameSql;
                cellList[i].ColumnText(ref text);
                // WidthPercent
                isLast = i == cellList.Count;
                double widthPercentAvg = Math.Round(((double)100 - widthPercentTotal) / ((double)cellList.Count - (double)i), 2);
                double widthPercent = widthPercentAvg;
                cellList[i].ColumnWidthPercent(ref widthPercent);
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
                result.Add(new GridColumn() { FieldName = cellList[i].FieldNameSql, Text = text, WidthPercent = widthPercent });
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
        /// Save GridLoad back to Json.
        /// </summary>
        private void SaveJsonGridLoad(ApplicationJson applicationJson)
        {
            GridDataJson gridDataJson = applicationJson.GridDataJson;
            //
            foreach (string gridName in GridLoadList.Keys)
            {
                GridLoadServer gridLoadServer = GridLoadList[gridName];
                GridLoad gridLoad = gridDataJson.GridLoadList[gridName];
                gridLoad.FieldNameOrderBy = gridLoadServer.FieldNameOrderBy;
                gridLoad.IsOrderByDesc = gridLoadServer.IsOrderByDesc;
            }
        }

        /// <summary>
        /// Copy data from class GridDataServer to class GridDataJson.
        /// </summary>
        public void SaveJson(ApplicationJson applicationJson)
        {
            GridDataJson gridDataJson = applicationJson.GridDataJson;
            //
            if (gridDataJson.GridLoadList == null)
            {
                gridDataJson.GridLoadList = new Dictionary<string, GridLoad>();
            }
            //
            foreach (string gridName in RowList.Keys)
            {
                Type typeRow = TypeRowList[gridName];
                gridDataJson.GridLoadList[gridName] = new GridLoad() { GridName = gridName, TypeRowName = DataAccessLayer.Util.TypeRowToName(typeRow) };
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
                foreach (string index in RowList[gridName].Keys)
                {
                    GridRowServer gridRowServer = RowList[gridName][index];
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
                            object value = propertyInfo.GetValue(gridRowServer.Row);
                            string textJson = DataAccessLayer.Util.ValueToText(value);
                            string text = TextGet(gridName, index, fieldName);
                            GridCellServer gridCellServer = CellGet(gridName, index, fieldName);
                            if (!gridDataJson.CellList[gridName].ContainsKey(fieldName))
                            {
                                gridDataJson.CellList[gridName][fieldName] = new Dictionary<string, GridCell>();
                            }
                            string errorCell = ErrorCellGet(gridName, index, fieldName);
                            GridCell gridCell = new GridCell() { IsSelect = gridCellServer.IsSelect, IsClick = gridCellServer.IsClick, E = errorCell };
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
            SaveJsonGridLoad(applicationJson);
        }
    }
}
