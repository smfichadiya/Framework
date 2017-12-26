﻿namespace Framework.Application
{
    using Framework.Component;
    using Framework.DataAccessLayer;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Dynamic.Core;

    /// <summary>
    /// Process OrderBy click.
    /// </summary>
    internal class ProcessGridOrderBy : Process
    {
        protected internal override void Run(App app)
        {
            AppJson appJson = app.AppJson;
            // Detect OrderBy click
            foreach (string gridName in appJson.GridDataJson.ColumnList.Keys.ToArray())
            {
                foreach (GridColumn gridColumn in appJson.GridDataJson.ColumnList[gridName])
                {
                    if (gridColumn.IsClick)
                    {
                        GridQuery gridQuery = appJson.GridDataJson.GridQueryList[gridName];
                        if (gridQuery.FieldNameOrderBy == gridColumn.FieldName)
                        {
                            gridQuery.IsOrderByDesc = !gridQuery.IsOrderByDesc;
                        }
                        else
                        {
                            gridQuery.FieldNameOrderBy = gridColumn.FieldName;
                            gridQuery.IsOrderByDesc = false;
                        }
                        app.GridData.TextParse(isFilterParse: true);
                        app.GridData.LoadDatabaseReload(GridName.FromJson(gridName));
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Set OrderBy up or down arrow.
    /// </summary>
    internal class ProcessGridOrderByText : Process
    {
        protected internal override void Run(App app)
        {
            AppJson appJson = app.AppJson;
            //
            foreach (string gridName in appJson.GridDataJson.ColumnList.Keys)
            {
                GridQuery gridQuery = appJson.GridDataJson.GridQueryList[gridName];
                foreach (GridColumn gridColumn in appJson.GridDataJson.ColumnList[gridName])
                {
                    gridColumn.IsClick = false;
                    if (gridColumn.FieldName == gridQuery.FieldNameOrderBy)
                    {
                        if (gridQuery.IsOrderByDesc)
                        {
                            gridColumn.Text = "▼" + gridColumn.Text;
                        }
                        else
                        {
                            gridColumn.Text = "▲" + gridColumn.Text;
                        }
                    }
                }
            }
        }
    }

    internal class ProcessTextParse : Process
    {
        protected internal override void Run(App app)
        {
            app.GridData.TextParse();
        }
    }

    /// <summary>
    /// Process data grid filter.
    /// </summary>
    internal class ProcessGridFilter : Process
    {
        protected internal override void Run(App app)
        {
            AppJson appJson = app.AppJson;
            //
            List<string> gridNameList = new List<string>(); // Grids to reload after filter changed.
            foreach (string gridName in appJson.GridDataJson.ColumnList.Keys)
            {
                foreach (GridRow gridRow in appJson.GridDataJson.RowList[gridName])
                {
                    if (new Index(gridRow.Index).Enum == IndexEnum.Filter)
                    {
                        foreach (GridColumn gridColumn in appJson.GridDataJson.ColumnList[gridName])
                        {
                            GridCell gridCell = appJson.GridDataJson.CellList[gridName][gridColumn.FieldName][gridRow.Index];
                            if (gridCell.IsModify)
                            {
                                if (!gridNameList.Contains(gridName))
                                {
                                    gridNameList.Add(gridName);
                                }
                            }
                        }
                    }
                }
            }
            //
            foreach (string gridName in gridNameList) // Grids with filter changed
            {
                GridData gridData = app.GridData;
                gridData.LoadDatabaseReload(GridName.FromJson(gridName));
            }
        }
    }

    /// <summary>
    /// Grid row or cell is clicked. Set IsSelect.
    /// </summary>
    internal class ProcessGridIsClick : Process
    {
        private void ProcessGridSelectRowClear(AppJson appJson, string gridName)
        {
            foreach (GridRow gridRow in appJson.GridDataJson.RowList[gridName])
            {
                gridRow.IsSelectSet(false);
            }
        }

        private void ProcessGridSelectCell(AppJson appJson, string gridName, Index index, string fieldName)
        {
            GridDataJson gridDataJson = appJson.GridDataJson;
            //
            gridDataJson.SelectGridNamePrevious = gridDataJson.SelectGridName;
            gridDataJson.SelectIndexPrevious = gridDataJson.SelectIndex;
            gridDataJson.SelectFieldNamePrevious = gridDataJson.SelectFieldName;
            //
            gridDataJson.SelectGridName = gridName;
            gridDataJson.SelectIndex = index.Value;
            gridDataJson.SelectFieldName = fieldName;
        }

        protected internal override void Run(App app)
        {
            GridDataJson gridDataJson = app.AppJson.GridDataJson;
            foreach (GridQuery gridQuery in gridDataJson.GridQueryList.Values)
            {
                string gridName = gridQuery.GridName;
                foreach (GridRow gridRow in gridDataJson.RowList[gridName])
                {
                    bool cellIsClick = false;
                    foreach (var gridColumn in gridDataJson.ColumnList[gridName])
                    {
                        GridCell gridCell = gridDataJson.CellList[gridName][gridColumn.FieldName][gridRow.Index];
                        if (gridCell.IsClick == true)
                        {
                            cellIsClick = true;
                            ProcessGridSelectCell(app.AppJson, gridName, new Index(gridRow.Index), gridColumn.FieldName);
                        }
                    }
                    if (gridRow.IsClick || cellIsClick)
                    {
                        gridRow.IsClick = true; // If cell is clicked, row is also clicked.
                        ProcessGridSelectRowClear(app.AppJson, gridName);
                        gridRow.IsSelectSet(true);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Select first row of grid, if no row is yet selected.
    /// </summary>
    internal class ProcessGridRowSelectFirst : Process
    {
        protected internal override void Run(App app)
        {
            GridData gridData = app.GridData;
            foreach (GridName gridName in gridData.GridNameList())
            {
                Index index = gridData.RowSelectedIndex(gridName);
                if (index == null)
                {
                    Index indexFirst = gridData.IndexList(gridName).Where(item => item.Enum == IndexEnum.Index).FirstOrDefault();
                    if (indexFirst == null)
                    {
                        indexFirst = gridData.IndexList(gridName).Where(item => item.Enum == IndexEnum.New).FirstOrDefault();
                    }
                    Row rowSelect = gridData.RowSelect(gridName, indexFirst);
                    ProcessGridIsClickMasterDetail.MasterDetailIsClick(app, gridName, rowSelect);
                }
            }
        }
    }

    internal class ProcessGridIsClickMasterDetail : Process
    {
        internal static void MasterDetailIsClick(App app, GridName gridNameMaster, Row rowMaster)
        {
            GridData gridData = app.GridData;
            foreach (GridName gridName in gridData.GridNameList())
            {
                Type typeRow = gridData.TypeRow(gridName);
                Row rowTable = UtilDataAccessLayer.RowCreate(typeRow); // RowTable is the API. No data in record!
                bool isReload = false;
                rowTable.MasterIsClick(app, gridNameMaster, rowMaster, ref isReload);
                if (isReload)
                {
                    gridData.LoadDatabaseReload(gridName);
                }
            }
        }

        protected internal override void Run(App app)
        {
            GridDataJson gridDataJson = app.AppJson.GridDataJson;
            foreach (GridQuery gridQuery in gridDataJson.GridQueryList.Values)
            {
                string gridName = gridQuery.GridName;
                foreach (GridRow gridRow in gridDataJson.RowList[gridName])
                {
                    if (gridRow.IsClick)
                    {
                        Index gridRowIndex = new Index(gridRow.Index);
                        if (gridRowIndex.Enum == IndexEnum.Index || gridRowIndex.Enum == IndexEnum.New)
                        {
                            GridData gridData = app.GridData;
                            var row = gridData.Row(GridName.FromJson(gridName), gridRowIndex);
                            MasterDetailIsClick(app, GridName.FromJson(gridName), row);
                            break;
                        }
                    }
                }
            }
        }
     }

    /// <summary>
    /// Save GridData back to json.
    /// </summary>
    internal class ProcessGridSaveJson : Process
    {
        protected internal override void Run(App app)
        {
            app.GridData.SaveJson();
        }
    }

    /// <summary>
    /// Set row and cell IsClick to false
    /// </summary>
    internal class ProcessGridIsClickFalse : Process
    {
        protected internal override void Run(App app)
        {
            GridDataJson gridDataJson = app.AppJson.GridDataJson;
            foreach (GridQuery gridQuery in gridDataJson.GridQueryList.Values)
            {
                string gridName = gridQuery.GridName;
                foreach (GridRow gridRow in gridDataJson.RowList[gridName])
                {
                    gridRow.IsClick = false;
                    foreach (var gridColumn in gridDataJson.ColumnList[gridName])
                    {
                        GridCell gridCell = gridDataJson.CellList[gridName][gridColumn.FieldName][gridRow.Index];
                        gridCell.IsClick = false;
                    }
                }
            }
            //
            gridDataJson.SelectGridNamePrevious = null;
            gridDataJson.SelectIndexPrevious = null;
            gridDataJson.SelectFieldNamePrevious = null;
        }
    }

    internal class ProcessGridCellIsModifyFalse : Process
    {
        protected internal override void Run(App app)
        {
            GridDataJson gridDataJson = app.AppJson.GridDataJson;
            //
            foreach (string gridName in gridDataJson.RowList.Keys)
            {
                foreach (GridRow gridRow in gridDataJson.RowList[gridName])
                {
                    foreach (var gridColumn in gridDataJson.ColumnList[gridName])
                    {
                        GridCell gridCell = gridDataJson.CellList[gridName][gridColumn.FieldName][gridRow.Index];
                        if (gridCell.IsModify)
                        {
                            gridCell.IsModify = false;
                        }
                    }
                }
            }
        }
    }

    internal class ProcessGridLookupIsClick : Process
    {
        protected internal override void Run(App app)
        {
            Row rowLookup = null;
            GridDataJson gridDataJson = app.AppJson.GridDataJson;
            foreach (string gridName in gridDataJson.RowList.Keys)
            {
                if (gridName == "Lookup")
                {
                    foreach (GridRow gridRow in gridDataJson.RowList[gridName])
                    {
                        if (gridRow.IsClick)
                        {
                            Index gridRowIndex = new Index(gridRow.Index);
                            if (gridRowIndex.Enum == IndexEnum.Index)
                            {
                                GridData gridData = app.GridData;
                                rowLookup = gridData.Row(new GridName("Lookup"), gridRowIndex);
                            }
                        }
                    }
                }
            }
            //
            if (rowLookup != null)
            {
                GridData gridData = app.GridData;
                // Row and cell, on which lookup is open.
                GridName gridName = GridName.FromJson(gridDataJson.SelectGridNamePrevious);
                Index index = new Index(gridDataJson.SelectIndexPrevious);
                string fieldName = gridDataJson.SelectFieldNamePrevious;
                // Lookup
                GridName gridNameLookup = GridName.FromJson(gridDataJson.SelectGridName);
                Index indexLookup = new Index(gridDataJson.SelectIndex);
                string fieldNameLookup = gridDataJson.SelectFieldName;
                // Set IsModify
                gridData.CellIsModifySet(gridName, index, fieldName);
                var row = gridData.Row(gridName, index);
                Cell cell = UtilDataAccessLayer.CellList(row.GetType(), row).Where(item => item.FieldNameCSharp == fieldName).First();
                // Cell of lookup which user clicked.
                Cell cellLookup = UtilDataAccessLayer.CellList(rowLookup.GetType(), rowLookup).Where(item => item.FieldNameCSharp == fieldNameLookup).First();
                string text = app.GridData.CellGet(gridNameLookup, indexLookup, fieldNameLookup).Text;
                cell.CellLookupIsClick(app, gridName, index, cell.FieldNameCSharp, rowLookup, cellLookup.FieldNameCSharp, text);
                //
                app.GridData.SelectGridName = GridName.ToJson(gridName);
                app.GridData.SelectIndex = index;
                app.GridData.SelectFieldName = fieldName;
            }
        }
    }

    /// <summary>
    /// Open Lookup grid.
    /// </summary>
    internal class ProcessGridLookup : Process
    {
        /// <summary>
        /// Returns true, if cell has been clicked or text has been entered.
        /// </summary>
        private bool IsLookupOpen(App app, out GridName gridName, out Index index, out string fieldName)
        {
            bool result = false;
            gridName = null;
            index = null;
            fieldName = null;
            //
            GridDataJson gridDataJson = app.AppJson.GridDataJson;
            foreach (string gridNameItem in gridDataJson.RowList.Keys)
            {
                foreach (GridRow gridRow in gridDataJson.RowList[gridNameItem])
                {
                    foreach (var gridColumn in gridDataJson.ColumnList[gridNameItem])
                    {
                        GridCell gridCell = gridDataJson.CellList[gridNameItem][gridColumn.FieldName][gridRow.Index];
                        if (gridCell.IsClick || gridCell.IsModify)
                        {
                            result = true;
                            gridName = GridName.FromJson(gridNameItem);
                            index = new Index(gridRow.Index);
                            fieldName = gridColumn.FieldName;
                            break;
                        }
                    }
                }
            }
            return result;
        }

        protected internal override void Run(App app)
        {
            GridName gridName;
            Index index;
            string fieldName;
            bool isLookupOpen = IsLookupOpen(app, out gridName, out index, out fieldName);
            //
            GridData gridData = app.GridData;
            if (isLookupOpen)
            {
                Row row = gridData.Row(gridName, index);
                GridCellInternal gridCellInternal = gridData.CellGet(gridName, index, fieldName);
                //
                Type typeRow = gridData.TypeRow(gridName);
                Cell cell = UtilDataAccessLayer.CellList(typeRow, row).Where(item => item.FieldNameCSharp == fieldName).Single();
                List<Row> rowList = null;
                IQueryable query;
                cell.CellLookup(app, gridName, index, fieldName, out query);
                if (query != null)
                {
                    typeRow = query.ElementType;
                    UtilFramework.Assert(UtilFramework.IsSubclassOf(typeRow, typeof(Row))); // Query needs to return Row list! Define Row type in memory namespace.
                    rowList = query.Take(10).Cast<Row>().ToList();
                }
                bool isLoadRow = gridData.LoadRow(new GridNameTypeRow(typeRow, UtilApplication.GridNameLookup), rowList);
                //
                if (isLoadRow)
                {
                    gridData.LookupOpen(gridName, index, fieldName);
                }
                else
                {
                    gridData.LookupClose();
                }
            }
            else
            {
                gridData.LookupClose();
            }
        }
    }

    internal class ProcessGridFocus : Process
    {
        private void FocusClear(App app)
        {
            GridDataJson gridDataJson = app.AppJson.GridDataJson;
            foreach (string gridNameItem in gridDataJson.RowList.Keys)
            {
                foreach (GridRow gridRow in gridDataJson.RowList[gridNameItem])
                {
                    foreach (var gridColumn in gridDataJson.ColumnList[gridNameItem])
                    {
                        GridCell gridCell = gridDataJson.CellList[gridNameItem][gridColumn.FieldName][gridRow.Index];
                        gridCell.FocusId = null;
                        gridCell.FocusIdRequest = null;
                    }
                }
            }
            //
            app.GridData.CellAll((GridCellInternal gridCellInternal) => { gridCellInternal.FocusId = null; gridCellInternal.FocusIdRequest = null; });
        }

        protected internal override void Run(App app)
        {
            GridDataJson gridDataJson = app.AppJson.GridDataJson;
            foreach (string gridNameItem in gridDataJson.RowList.Keys)
            {
                foreach (GridRow gridRow in gridDataJson.RowList[gridNameItem])
                {
                    foreach (GridColumn gridColumn in gridDataJson.ColumnList[gridNameItem])
                    {
                        GridCell gridCell = gridDataJson.CellList[gridNameItem][gridColumn.FieldName][gridRow.Index];
                        if (gridCell.FocusIdRequest != null && gridCell.IsSelect)
                        {
                            int? focusIdRequest = gridCell.FocusIdRequest;
                            FocusClear(app);
                            gridCell.FocusId = focusIdRequest;
                            app.GridData.CellGet(GridName.FromJson(gridNameItem), new Index(gridRow.Index), gridColumn.FieldName).FocusId = focusIdRequest; 
                            break;
                        }
                    }
                }
            }
        }
    }


    /// <summary>
    /// Set IsSelect on selected GridCell, or to null, if cell does not exist anymore.
    /// </summary>
    internal class ProcessGridCellIsSelect : Process
    {
        private void IsSelect(GridDataJson gridDataJson)
        {
            foreach (string gridName in gridDataJson.RowList.Keys)
            {
                foreach (GridRow gridRow in gridDataJson.RowList[gridName])
                {
                    foreach (var gridColumn in gridDataJson.ColumnList[gridName])
                    {
                        GridCell gridCell = gridDataJson.CellList[gridName][gridColumn.FieldName][gridRow.Index];
                        bool isSelect = gridDataJson.SelectGridName == gridName && gridDataJson.SelectFieldName == gridColumn.FieldName && gridDataJson.SelectIndex == gridRow.Index;
                        gridCell.IsSelect = isSelect;
                    }
                }
            }
        }

        protected internal override void Run(App app)
        {
            GridDataJson gridDataJson = app.AppJson.GridDataJson;
            bool isExist = false; // Selected field exists
            if (gridDataJson.SelectFieldName != null)
            {
                if (gridDataJson.RowList[gridDataJson.SelectGridName].Exists(item => item.Index == gridDataJson.SelectIndex)) // Selected row exists
                {
                    if (gridDataJson.ColumnList[gridDataJson.SelectGridName].Exists(item => item.FieldName == gridDataJson.SelectFieldName)) // Selected column exists
                    {
                        isExist = true;
                    }
                }
            }
            if (isExist == false)
            {
                if (app.AppJson.GridDataJson != null)
                {
                    app.AppJson.GridDataJson.SelectFieldName = null;
                    app.AppJson.GridDataJson.SelectGridName = null;
                    app.AppJson.GridDataJson.SelectIndex = null;
                }
            }
            //
            IsSelect(gridDataJson);
        }
    }

    internal class ProcessGridFieldWithLabelIndex : Process
    {
        protected internal override void Run(App app)
        {
            foreach (GridFieldWithLabel gridFieldWithLabel in app.AppJson.ListAll().OfType<GridFieldWithLabel>())
            {
                gridFieldWithLabel.Index = app.GridData.RowSelectedIndex(GridName.FromJson(gridFieldWithLabel.GridName))?.Value; // Set index to selected row.
            }
        }
    }

    internal class ProcessGridSaveDatabase : Process
    {
        protected internal override void Run(App app)
        {
            app.GridData.SaveDatabase();
        }
    }

    /// <summary>
    /// Cell rendered as button is clicked.
    /// </summary>
    internal class ProcessGridCellButtonIsClick : Process
    {
        protected internal override void Run(App app)
        {
            GridDataJson gridDataJson = app.AppJson.GridDataJson;
            //
            string gridNameClick = null;
            string indexClick = null;
            string fieldNameClick = null;
            foreach (string gridName in gridDataJson.RowList.Keys)
            {
                foreach (GridRow gridRow in gridDataJson.RowList[gridName])
                {
                    foreach (var gridColumn in gridDataJson.ColumnList[gridName])
                    {
                        GridCell gridCell = gridDataJson.CellList[gridName][gridColumn.FieldName][gridRow.Index];
                        if (gridCell.IsModify && gridCell.CellEnum == GridCellEnum.Button)
                        {
                            gridNameClick = gridName;
                            indexClick = gridRow.Index;
                            fieldNameClick = gridColumn.FieldName;
                            break;
                        }
                    }
                }
            }
            //
            if (gridNameClick != null)
            {
                Row row = app.GridData.Row(GridName.FromJson(gridNameClick), new Index(indexClick));
                Type typeRow = app.GridData.TypeRow(GridName.FromJson(gridNameClick));
                Cell cell = UtilDataAccessLayer.CellList(typeRow, row).Where(item => item.FieldNameCSharp == fieldNameClick).Single();
                bool isReload = false;
                bool isException = false;
                try
                {
                    cell.CellButtonIsClick(app, GridName.FromJson(gridNameClick), new Index(indexClick), row, fieldNameClick, ref isReload);
                }
                catch (Exception exception)
                {
                    isException = true;
                    app.GridData.ErrorRowSet(GridName.FromJson(gridNameClick), new Index(indexClick), UtilFramework.ExceptionToText(exception));
                }
                if (isReload && isException == false)
                {
                    app.GridData.LoadDatabaseReload(GridName.FromJson(gridNameClick));
                }
            }
        }
    }
}