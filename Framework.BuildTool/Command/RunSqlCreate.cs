﻿namespace Framework.BuildTool
{
    using Framework.Application;
    using Framework.BuildTool.DataAccessLayer;
    using Framework.Server;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Text;

    public class CommandRunSqlCreate : Command
    {
        public CommandRunSqlCreate(AppBuildTool appBuildTool) 
            : base("runSqlCreate", "Run sql create scripts and update FrameworkApplicationType (CSharp meta)")
        {
            this.AppBuildTool = appBuildTool;
            this.OptionDrop = OptionAdd("-d|--drop", "Run sql drop scripts");
        }

        public readonly AppBuildTool AppBuildTool;

        public readonly Option OptionDrop;

        /// <summary>
        /// Returns SqlCreate, SqlDrop folder name.
        /// </summary>
        /// <param name="isFrameworkDb">Framework or Application.</param>
        /// <param name="isDrop">Add SqlCreate or SqlDrop to folder name.</param>
        /// <param name="isName">Return folder name as name without drive information and SqlCreate folder.</param>
        private string FolderName(bool isFrameworkDb, bool isDrop, bool isName)
        {
            string result = null;
            if (isName == false)
            {
                result += UtilFramework.FolderName;
            }
            if (isFrameworkDb == false)
            {
                result += "BuildTool/";
            }
            else
            {
                result += "Submodule/Framework.BuildTool/";
            }
            if (isName == false)
            {
                if (isDrop == false)
                {
                    result += "SqlCreate/";
                }
                else
                {
                    result += "SqlDrop/";
                }
            }
            return result;
        }

        /// <summary>
        /// Same file in SqlCreate and SqlDrop folder have the same name.
        /// </summary>
        private string FileNameToName(string fileName, bool isFrameworkDb, bool isDrop)
        {
            string folderNameFind = FolderName(isFrameworkDb, isDrop, false);
            string folderNameReplace = FolderName(isFrameworkDb, false, true);
            UtilFramework.Assert(fileName.StartsWith(folderNameFind));
            string result = fileName.Replace(folderNameFind, folderNameReplace);
            return result;
        }

        /// <summary>
        /// Populate NameList.
        /// </summary>
        private void NameList(bool isFrameworkDb, bool isDrop, ref string[] nameList)
        {
            string folderName = FolderName(isFrameworkDb, isDrop, false);
            var fileNameList = UtilFramework.FileNameList(folderName);
            var result = fileNameList.Select(item => FileNameToName(item, isFrameworkDb, isDrop)).ToArray();
            nameList = nameList.Union(result).Distinct().ToArray();
        }

        /// <summary>
        /// Returns true, if script is marked as IsRun on database.
        /// </summary>
        /// <param name="fileName">SqlCreate or SqlDrop script.</param>
        private bool IsRunGet(string fileName, bool isFrameworkDb, bool isDrop)
        {
            string name = FileNameToName(fileName, isFrameworkDb, isDrop);
            var rowList = UtilBuildTool.SqlRead("SELECT * FROM FrameworkScript WHERE Name = @Name", true, new SqlParameter("Name", System.Data.SqlDbType.NVarChar) { Value = name });
            bool result = (bool)rowList.Single()["IsRun"];
            return result;
        }

        /// <summary>
        /// Set IsRun flag on database table FrameworkScript.
        /// </summary>
        private void IsRunSet(string fileName, bool isFrameworkDb, bool isDrop, bool value)
        {
            string name = FileNameToName(fileName, isFrameworkDb, isDrop);
            UtilBuildTool.SqlCommand(
                "UPDATE FrameworkScript SET IsRun = @IsRun WHERE Name = @Name",
                true,
                new SqlParameter("IsRun", System.Data.SqlDbType.Bit) { Value = value }, 
                new SqlParameter("Name", System.Data.SqlDbType.NVarChar) { Value = name }
            );
        }

        /// <summary>
        /// Execute SqlCreate, SqlDrop scripts.
        /// </summary>
        private void ScriptExecute(bool isFrameworkDb, bool isDrop)
        {
            string folderName = FolderName(isFrameworkDb, isDrop, false);
            var fileNameList = UtilFramework.FileNameList(folderName, "*.sql").OrderBy(item => item);
            foreach (string fileName in fileNameList)
            {
                bool isRun = IsRunGet(fileName, isFrameworkDb, isDrop);
                UtilFramework.Log(string.Format("### Start RunSql={0}; OptionDrop={1}; IsRun={2};", fileName, OptionDrop.IsOn, isRun));
                if (isRun == isDrop)
                {
                    string text = UtilFramework.FileRead(fileName);
                    var sqlList = text.Split(new string[] { "\r\nGO", "\nGO", "GO\r\n", "GO\n" }, StringSplitOptions.RemoveEmptyEntries);
                    UtilBuildTool.SqlCommand(sqlList.ToList(), isFrameworkDb);
                    IsRunSet(fileName, isFrameworkDb, isDrop, !isDrop);
                }
                UtilFramework.Log(string.Format("### Exit RunSql={0}; OptionDrop={1}; IsRun={2};", fileName, OptionDrop.IsOn, isRun));
            }
        }

        /// <summary>
        /// Insert new scripts and mark missing ones as not exist.
        /// </summary>
        private void Upsert(string[] nameList)
        {
            UtilBuildTool.SqlCommand("UPDATE FrameworkScript SET IsExist = 0", true);
            //
            string sqlUpsert = @"
            MERGE INTO FrameworkScript AS Target
            USING ({0}) AS Source
	            ON NOT EXISTS(
                    SELECT Source.Name
                    EXCEPT
                    SELECT Target.Name)
            WHEN MATCHED THEN
	            UPDATE SET Target.IsExist = 1
            WHEN NOT MATCHED BY TARGET THEN
	            INSERT (Name, IsExist, IsRun)
	            VALUES (Source.Name, 1, 0);
            ";
            StringBuilder sqlSelect = new StringBuilder();
            bool isFirst = true;
            foreach (string name in nameList)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    sqlSelect.Append(" UNION ALL\r\n");
                }
                sqlSelect.Append(string.Format("(SELECT '{0}' AS Name)", name));
            }
            sqlUpsert = string.Format(sqlUpsert, sqlSelect.ToString());
            UtilBuildTool.SqlCommand(sqlUpsert, true);
        }

        private void RunSqlCreate()
        {
            // Create table FrameworkScript
            string fileNameScript = UtilFramework.FolderName + "Submodule/Framework.BuildTool/Sql/Script.sql";
            string sql = UtilGenerate.FileLoad(fileNameScript);
            UtilBuildTool.SqlCommand(sql, true);
            //
            string[] nameList = new string[] { };
            NameList(false, false, ref nameList);
            NameList(false, true, ref nameList);
            NameList(true, false, ref nameList);
            NameList(true, true, ref nameList);
            //
            Upsert(nameList);
            //
            ScriptExecute(true, OptionDrop.IsOn);
            ScriptExecute(false, OptionDrop.IsOn);
        }

        public override void Run()
        {
            RunSqlCreate();
            if (OptionDrop.IsOn == false)
            {
                new CommandRunSqlMeta(AppBuildTool).Run();
            }
        }
    }
}
