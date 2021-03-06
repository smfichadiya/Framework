﻿namespace Framework.Cli.Generate
{
    using Framework.DataAccessLayer;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using static Framework.Cli.AppCli;

    internal static class NamingConventionIntegrate
    {
        public static bool TableNameIsIntegrate(string tableNameCSharp)
        {
            return tableNameCSharp.EndsWith("Integrate");
        }
    }

    internal class GenerateCSharpIntegrate
    {
        /// <summary>
        /// Generate CSharp namespace for every database schema.
        /// </summary>
        /// <param name="isFrameworkDb">If true, generate CSharp code for Framework library (internal use only) otherwise generate code for Application.</param>
        /// <param name="isApplication">If false, generate CSharp code for cli. If true, generate code for Application or Framework.</param>
        private static void GenerateCSharpSchemaName(List<GenerateIntegrateItem> integrateList, bool isFrameworkDb, bool isApplication, StringBuilder result)
        {
            integrateList = integrateList.Where(item => item.IsFrameworkDb == isFrameworkDb && item.IsApplication == isApplication).ToList();
            var schemaNameCSharpList = integrateList.GroupBy(item => item.SchemaNameCSharp, (key, group) => key);
            bool isFirst = true;
            foreach (string schemaNameCSharp in schemaNameCSharpList)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    result.AppendLine();
                }
                result.AppendLine(string.Format("namespace DatabaseIntegrate.{0}", schemaNameCSharp));
                result.AppendLine(string.Format("{{"));
                result.AppendLine(string.Format("    using System;")); // Used for method Guid.Parse();
                result.AppendLine(string.Format("    using System.Collections.Generic;"));
                result.AppendLine(string.Format("    using System.Globalization;")); // Used for property CultureInfo.InvariantCulture;
                if (isApplication)
                {
                    // See also class IdNameEnumAttribute
                    result.AppendLine(string.Format("    using System.Linq;"));
                    result.AppendLine(string.Format("    using Framework.DataAccessLayer;"));
                    result.AppendLine(string.Format("    using Framework.DataAccessLayer.Integrate;"));
                }
                result.AppendLine(string.Format("    using Database.{0};", schemaNameCSharp));
                result.AppendLine();
                GenerateCSharpTableNameClass(integrateList.Where(item => item.SchemaNameCSharp == schemaNameCSharp).ToList(), isFrameworkDb, isApplication, result);
                result.AppendLine(string.Format("}}"));
            }
        }

        /// <summary>
        /// Generate static CSharp class for every database table.
        /// </summary>
        private static void GenerateCSharpTableNameClass(List<GenerateIntegrateItem> integrateList, bool isFrameworkDb, bool isApplication, StringBuilder result)
        {
            bool isFirst = true;
            foreach (var integrate in integrateList)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    result.AppendLine();
                }

                // Use one "Database" and "DatabaseIntegrate" namespace for Framework and Application
                string classNameExtension = ""; // "Table"; // See also method CommandDeployDbIntegrateInternal();

                // Framework, Application
                if (isFrameworkDb)
                {
                    classNameExtension += "Framework";
                }
                else
                {
                    classNameExtension += "Application";
                }

                // Cli, Application
                if (isApplication)
                {
                    classNameExtension += "";
                }
                else
                {
                    classNameExtension += "Cli";
                }

                result.AppendLine(string.Format("    public static class {0}{1}", integrate.TableNameCSharp, classNameExtension));
                result.AppendLine(string.Format("    {{"));
                if (isApplication)
                {
                    GenerateCSharpNameEnum(integrate, result);
                }
                result.AppendLine(string.Format("        public static List<{0}> RowList", integrate.TableNameCSharp));
                result.AppendLine(string.Format("        {{"));
                result.AppendLine(string.Format("            get"));
                result.AppendLine(string.Format("            {{"));
                result.AppendLine(string.Format("                var result = new List<{0}>", integrate.TableNameCSharp));
                result.AppendLine(string.Format("                {{", integrate.TableNameCSharp));
                GenerateCSharpRowIntegrate(integrate, result);
                result.AppendLine(string.Format("                }};", integrate.TableNameCSharp));
                result.AppendLine(string.Format("                return result;"));
                result.AppendLine(string.Format("            }}"));
                result.AppendLine(string.Format("        }}"));
                result.AppendLine(string.Format("    }}"));
            }
        }

        private static void GenerateCSharpNameEnum(GenerateIntegrateItem integrate, StringBuilder result)
        {
            var fieldList = UtilDalType.TypeRowToFieldList(integrate.TypeRow);
            var fieldId = fieldList.SingleOrDefault(item => item.FieldNameCSharp == "Id"); // See also FieldIntegrate.IsKey
            var fieldIdName = fieldList.SingleOrDefault(item => item.FieldNameCSharp == "IdName"); // See also FieldIntegrate.IsKey
            if (fieldIdName != null) 
            {
                result.Append(string.Format("        public enum IdNameEnum {{ [IdNameEnum(null)]None = 0"));
                List<string> nameExceptList = new List<string>();
                int count = 0;
                foreach (Row row in integrate.RowList)
                {
                    count += 1;
                    string idName = (string)fieldIdName.PropertyInfo.GetValue(row);
                    string nameCSharp = UtilGenerate.NameCSharp(idName, nameExceptList);
                    result.Append(string.Format(", [IdNameEnum(\"{0}\")]{1} = {2}", idName, nameCSharp, count));
                }
                result.AppendLine(string.Format(" }}"));
                result.AppendLine();
                result.AppendLine(string.Format("        public static {0} Row(this IdNameEnum value)", integrate.TableNameCSharp));
                result.AppendLine(string.Format("        {{"));
                result.AppendLine(string.Format("            return RowList.Where(item => item.IdName == IdNameEnumAttribute.IdNameFromEnum(value)).SingleOrDefault();"));
                result.AppendLine(string.Format("        }}"));
                result.AppendLine();
                result.AppendLine(string.Format("        public static IdNameEnum IdName(string value)"));
                result.AppendLine(string.Format("        {{"));
                result.AppendLine(string.Format("            return IdNameEnumAttribute.IdNameToEnum<IdNameEnum>(value);"));
                result.AppendLine(string.Format("        }}"));
                result.AppendLine();
                result.AppendLine(string.Format("        public static string IdName(this IdNameEnum value)"));
                result.AppendLine(string.Format("        {{"));
                result.AppendLine(string.Format("            return IdNameEnumAttribute.IdNameFromEnum(value);"));
                result.AppendLine(string.Format("        }}"));
                result.AppendLine();
            }
        }

        private static void GenerateCSharpRowIntegrate(GenerateIntegrateItem integrateItem, StringBuilder result)
        {
            var fieldNameIdCSharpReferenceList = integrateItem.Owner.ResultReference.Where(item => item.TypeRowIntegrate == integrateItem.TypeRow).Select(item => item.FieldNameIdCSharp).ToList();
            var fieldList = UtilDalType.TypeRowToFieldList(integrateItem.TypeRow);
            foreach (Row row in integrateItem.RowList)
            {
                result.Append(string.Format("                    new {0} {{", integrateItem.TableNameCSharp));
                bool isFirst = true;
                foreach (var field in fieldList)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        result.Append(", ");
                    }
                    object value = field.PropertyInfo.GetValue(row);
                    if (fieldNameIdCSharpReferenceList.Contains(field.FieldNameCSharp) || (fieldNameIdCSharpReferenceList.Count() > 0 && field.FieldNameCSharp == "Id"))
                    {
                        UtilFramework.Assert(value == null || value.GetType() == typeof(int));

                        // Unlike IdName, Id can change from database to database.
                        value = 0;
                    }
                    GenerateCSharpRowIntegrateField(field, value, result);
                }
                result.Append(" },");
                result.AppendLine();
            }
        }

        /// <summary>
        /// Generate CSharp property with value.
        /// </summary>
        private static void GenerateCSharpRowIntegrateField(UtilDalType.Field field, object value, StringBuilder result)
        {
            string fieldNameCSharp = field.FieldNameCSharp;
            FrameworkType frameworkType = UtilDalType.FrameworkTypeFromEnum(field.FrameworkTypeEnum);
            string valueCSharp;
            if (value is string)
            {
                valueCSharp = UtilCli.EscapeCSharpString(value.ToString());
            }
            else
            {
                valueCSharp = frameworkType.ValueToCSharp(value);
            }
            result.Append(string.Format("{0} = {1}", fieldNameCSharp, valueCSharp));
        }

        /// <summary>
        /// Generate CSharp code.
        /// </summary>
        /// <param name="isApplication">If false, generate code for cli. If true, generate code for Application.</param>
        public void Run(out string cSharp, bool isFrameworkDb, bool isApplication, List<GenerateIntegrateItem> integrateList)
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine("// Do not modify this file. It's generated by Framework.Cli.");
            result.AppendLine();
            GenerateCSharpSchemaName(integrateList, isFrameworkDb, isApplication, result);
            cSharp = result.ToString();
        }
    }
}
