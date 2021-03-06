﻿namespace Framework.Test
{
    using Database.dbo;
    using Framework.DataAccessLayer;
    using Framework.Json;
    using Framework.Json.Bootstrap;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class UnitTest
    {
        public static void Run()
        {
            {
                UtilFramework.CamelCase camelCase = new UtilFramework.CamelCase("AbcDef");
                UtilFramework.Assert(camelCase.TextList[0] == "Abc");
                UtilFramework.Assert(camelCase.TextList[1] == "Def");
            }
            {
                UtilFramework.CamelCase camelCase = new UtilFramework.CamelCase("abcDef");
                UtilFramework.Assert(camelCase.TextList[0] == "abc");
                UtilFramework.Assert(camelCase.TextList[1] == "Def");
            }
            {
                UtilFramework.CamelCase camelCase = new UtilFramework.CamelCase("AbcDefCSharp");
                UtilFramework.Assert(camelCase.TextList[0] == "Abc");
                UtilFramework.Assert(camelCase.TextList[1] == "Def");
                UtilFramework.Assert(camelCase.TextList[2] == "CSharp");
                UtilFramework.Assert(camelCase.EndsWith("DefCSharp"));
                UtilFramework.Assert(camelCase.EndsWith("cDefCSharp") == false);
                UtilFramework.Assert(camelCase.EndsWith("AbcDefCSharp"));
                UtilFramework.Assert(camelCase.EndsWith("AbcDefCSharpCar") == false);
                UtilFramework.Assert(camelCase.EndsWith("CarAbcDefCSharp") == false);
                UtilFramework.Assert(camelCase.EndsWith("") == true);
            }
            {
                UtilFramework.CamelCase camelCase = new UtilFramework.CamelCase("AbcDefCSharp");
                UtilFramework.Assert(camelCase.StartsWith("Abc"));
                UtilFramework.Assert(camelCase.StartsWith("AbcDef"));
                UtilFramework.Assert(camelCase.StartsWith("AbcDefCSharp"));
                UtilFramework.Assert(camelCase.StartsWith("AbcDefCShar") == false);
                UtilFramework.Assert(camelCase.StartsWith("AbcDefCSharpLk") == false);
                UtilFramework.Assert(camelCase.StartsWith("LkAbcDefCSharp") == false);
                UtilFramework.Assert(camelCase.StartsWith(""));
            }
            {
                UtilFramework.CamelCase camelCase = new UtilFramework.CamelCase("ImageFileId");
                UtilFramework.Assert(camelCase.EndsWith("FileId"));
            }
            { 
                UtilFramework.CamelCase camelCase = new UtilFramework.CamelCase("ImagEFileId");
                UtilFramework.Assert(camelCase.EndsWith("FileId") == false);
            }
            {
                var source = new AppMain();

                // Serialize, deserialize
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                var dest = (AppMain)UtilJson.Deserialize(jsonSession);
            }
            {
                var source = new MyApp();
                source.Div = new Div(source);
                source.Div.ComponentRemove();

                // Serialize, deserialize
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                var dest = (MyApp)UtilJson.Deserialize(jsonSession);
            }
            {
                var source = new MyApp();
                source.Row = new BootstrapRow(source);
                source.Col = new BootstrapCol((BootstrapRow)source.Row);

                // Serialize, deserialize
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                var dest = (MyApp)UtilJson.Deserialize(jsonSession);

                UtilFramework.Assert(!jsonSession.Contains("PropertyReadOnly"));
            }
            {
                var source = new MyApp();

                // Serialize, deserialize
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                var dest = (MyApp)UtilJson.Deserialize(jsonSession);

                UtilFramework.Assert(!jsonSession.Contains("PropertyReadOnly"));
            }
            {
                var source = new MyApp(); 

                // Serialize, deserialize
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                var dest = (MyApp)UtilJson.Deserialize(jsonSession);

                UtilFramework.Assert(!jsonSession.Contains("PropertyReadOnly"));
            }
            {
                var source = new MyApp();
                var myGrid = new MyGrid(source) { Text = "K7", IsHide = true };
                source.MyCell = new MyCell { MyGridBoth = myGrid, MyText = "7755" };

                // Serialize, deserialize
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                var dest = (MyApp)UtilJson.Deserialize(jsonSession);

                UtilFramework.Assert(UtilFramework.FindCount(jsonClient, "K7") == 1);
                UtilFramework.Assert(UtilFramework.FindCount(jsonSession, "K7") == 1); // Ensure session stores reference
                UtilFramework.Assert(dest.List[0] == dest.MyCell.MyGridBoth);
            }
            {
                var source = new MyApp();
                var myGrid = new MyGrid(source) { Text = "K7", IsHide = true };
                var myGrid2 = new MyGrid(source) { Text = "K8", IsHide = true };
                source.MyCell = new MyCell { MyGrid = myGrid, MyGrid2 = myGrid2, MyText = "7755" };

                // Serialize, deserialize
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                var dest = (MyApp)UtilJson.Deserialize(jsonSession);

                UtilFramework.Assert(jsonClient.Contains("K7"));
                UtilFramework.Assert(!jsonClient.Contains("K8"));
                UtilFramework.Assert(dest.List[1] == dest.MyCell.MyGrid2);
            }
            RunComponentJson();
            {
                A source = new A();
                source.MyEnum = MyEnum.Left;

                // Serialize, deserialize
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                A dest = (A)UtilJson.Deserialize(jsonSession);

                UtilFramework.Assert(dest.MyEnum == MyEnum.Left);
                UtilFramework.Assert(dest.MyEnumNullable == null);
            }
            {
                A source = new A();
                source.MyEnumNullable = MyEnum.None;

                // Serialize, deserialize
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                A dest = (A)UtilJson.Deserialize(jsonSession);
                UtilFramework.Assert(dest.MyEnumNullable == MyEnum.None);
            }
            {
                A source = new A();

                // Serialize, deserialize
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                A dest = (A)UtilJson.Deserialize(jsonSession);

                UtilFramework.Assert(!jsonSession.Contains(nameof(A.MyEnumList)));
                UtilFramework.Assert(source.MyEnumList == null);
                UtilFramework.Assert(dest.MyEnumList != null);
                UtilFramework.Assert(dest.MyEnumList.Count == 0);
            }
            {
                A source = new A();
                source.MyEnumList = new List<MyEnum>();
                source.MyEnumList.Add(MyEnum.None);
                source.MyEnumList.Add(MyEnum.Left);
                source.MyEnumList.Add(MyEnum.Right);

                // Serialize, deserialize
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                A dest = (A)UtilJson.Deserialize(jsonSession);

                UtilFramework.Assert(jsonSession.Contains(nameof(A.MyEnumList)));
                UtilFramework.Assert(dest.MyEnumList[0] == MyEnum.None);
                UtilFramework.Assert(dest.MyEnumList[1] == MyEnum.Left);
                UtilFramework.Assert(dest.MyEnumList[2] == MyEnum.Right);
            }
            {
                A source = new A();
                source.MyEnumNullableList = new List<MyEnum?>();
                source.MyEnumNullableList.Add(MyEnum.None);
                source.MyEnumNullableList.Add(MyEnum.Left);
                source.MyEnumNullableList.Add(null);
                source.MyEnumNullableList.Add(MyEnum.Right);

                // Serialize, deserialize
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                A dest = (A)UtilJson.Deserialize(jsonSession);

                UtilFramework.Assert(jsonSession.Contains(nameof(A.MyEnumNullableList)));
                UtilFramework.Assert(dest.MyEnumNullableList[0] == MyEnum.None);
                UtilFramework.Assert(dest.MyEnumNullableList[1] == MyEnum.Left);
                UtilFramework.Assert(dest.MyEnumNullableList[2] == null);
                UtilFramework.Assert(dest.MyEnumNullableList[3] == MyEnum.Right);
            }
            {
                A source = new A();
                source.IntNullableList = new List<int?>();
                source.IntNullableList.Add(0);
                source.IntNullableList.Add(1);
                source.IntNullableList.Add(null);
                source.IntNullableList.Add(2);

                // Serialize, deserialize
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                A dest = (A)UtilJson.Deserialize(jsonSession);

                UtilFramework.Assert(jsonSession.Contains(nameof(A.IntNullableList)));
                UtilFramework.Assert(dest.IntNullableList[0] == 0);
                UtilFramework.Assert(dest.IntNullableList[1] == 1);
                UtilFramework.Assert(dest.IntNullableList[2] == null);
                UtilFramework.Assert(dest.IntNullableList[3] == 2);
            }
            {
                A source = new A();
                source.IntList = new List<int>();
                source.IntList.Add(0);
                source.IntList.Add(1);
                source.IntList.Add(2);

                // Serialize, deserialize
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                A dest = (A)UtilJson.Deserialize(jsonSession);

                UtilFramework.Assert(jsonSession.Contains(nameof(A.IntList)));
                UtilFramework.Assert(dest.IntList[0] == 0);
                UtilFramework.Assert(dest.IntList[1] == 1);
                UtilFramework.Assert(dest.IntList[2] == 2);
            }
            {
                A source = new A();
                source.V = 33;

                // Serialize, deserialize
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                A dest = (A)UtilJson.Deserialize(jsonSession);

                UtilFramework.Assert((int)dest.V == 33);
            }
            {
                A source = new A();
                source.V = "Hello";

                // Serialize, deserialize
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                A dest = (A)UtilJson.Deserialize(jsonSession);

                UtilFramework.Assert((string)dest.V == "Hello");
            }
            {
                var date = DateTime.Now;
                A source = new A();
                source.Row = new FrameworkDeployDb { Id = 22, FileName = @"C:\Temp\Readme.txt", Date = date };

                // Serialize, deserialize
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                A dest = (A)UtilJson.Deserialize(jsonSession);

                UtilFramework.Assert(dest.Row.Id == 22);
                UtilFramework.Assert(dest.Row.FileName == @"C:\Temp\Readme.txt");
                UtilFramework.Assert(dest.Row.Date ==date);
            }
            {
                A source = new A();
                source.V = MyEnum.None; // TODO Serialize enum on property of type object.

                // Serialize, deserialize
                // string json = UtilJson.Serialize(source);
                // A dest = (A)UtilJson.Deserialize(json);
            }
        }

        private static void RunComponentJson()
        {
            // Reference to self
            {
                MyComponent source = new MyComponent(null);
                source.Component = source;
                UtilJson.Serialize(source, out string json, out string jsonClient);
                MyComponent dest = (MyComponent)UtilJson.Deserialize(json);
                UtilFramework.Assert(dest.Component == dest);
            }
            // ComponentJson reference to ComponentJson do not send to client
            {
                MyComponent source = new MyComponent(null);
                source.HtmlAbc = new Html(source) { TextHtml = "JK" };
                source.MyTextSession = "SessionValueX";
                source.MyTextClient = "ClientValueX";
                source.MyIgnore = "IgnoreX";
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                MyComponent dest = (MyComponent)UtilJson.Deserialize(jsonSession);
                UtilFramework.Assert(!jsonClient.Contains("HtmlAbc")); // Do not send property name of ComponentJson reference to client

                UtilFramework.Assert(jsonSession.Contains("SessionValueX"));
                UtilFramework.Assert(!jsonClient.Contains("SessionValueX"));

                UtilFramework.Assert(!jsonSession.Contains("ClientValueX"));
                UtilFramework.Assert(jsonClient.Contains("ClientValueX"));

                UtilFramework.Assert(!jsonSession.Contains("IgnoreX"));
                UtilFramework.Assert(!jsonClient.Contains("IgnoreX"));

                UtilFramework.Assert(!jsonSession.Contains("Owner"));
                UtilFramework.Assert(!jsonClient.Contains("Owner"));
            }
            // ComponentJson.IsHide
            {
                MyComponent source = new MyComponent(null);
                new Html(source) { TextHtml = "X11" };
                new Html(source) { TextHtml = "X12", IsHide = true };
                new Html(source) { TextHtml = "X13" };
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                MyComponent dest = (MyComponent)UtilJson.Deserialize(jsonSession);
                UtilFramework.Assert(dest.List.Count == 3);
                UtilFramework.Assert(jsonClient.Contains("X11"));
                UtilFramework.Assert(!jsonClient.Contains("X12"));
                UtilFramework.Assert(jsonClient.Contains("X13"));
            }
            // ComponentJson.IsHide (Dto to ComponentJson
            {
                My source = new My();
                source.MyComponent = new MyComponent(null) { Id = 789, IsHide = true };
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                My dest = (My)UtilJson.Deserialize(jsonSession);
                UtilFramework.Assert(!jsonClient.Contains("789"));
            }
            // ComponentJson.IsHide
            {
                MyComponent source = new MyComponent(null);
                source.Html = new Html(source) { TextHtml = "My123", IsHide = true };
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                MyComponent dest = (MyComponent)UtilJson.Deserialize(jsonSession);
                UtilFramework.Assert(jsonSession.Contains("My123"));
                UtilFramework.Assert(!jsonClient.Contains("My123"));
            }
            // ComponentJson.IsHide (Root)
            {
                MyComponent source = new MyComponent(null);
                source.IsHide = true;
                source.Html = new Html(source) { TextHtml = "My123", IsHide = true };
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                MyComponent dest = (MyComponent)UtilJson.Deserialize(jsonSession);
                UtilFramework.Assert(jsonSession.Contains("My123"));
                UtilFramework.Assert(jsonClient == "");
            }
            // Reference to Row
            {
                MyComponent source = new MyComponent(null);
                source.MyRow = new MyRow { Text = "My123", DateTime = DateTime.Now };
                source.MyRowList = new List<Row>();
                source.MyRowList.Add(new MyRow { Text = "My1234", DateTime = DateTime.Now });
                source.MyRowList.Add(new MyRow { Text = "My12356", DateTime = DateTime.Now });
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                MyComponent dest = (MyComponent)UtilJson.Deserialize(jsonSession);
                UtilFramework.Assert(!jsonClient.Contains("My123"));
            }
            // Reference to Row
            {
                MyComponent source = new MyComponent(null);
                source.MyRowList = new List<Row>();
                source.MyRowList.Add(new MyRow { Text = "My1234", DateTime = DateTime.Now });
                source.MyRowList.Add(new MyRow { Text = "My12356", DateTime = DateTime.Now });
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                MyComponent dest = (MyComponent)UtilJson.Deserialize(jsonSession);
                UtilFramework.Assert(!jsonClient.Contains("My123"));
            }
            // Field of object type with Row value
            {
                MyComponent source = new MyComponent(null);
                source.V = new MyRow() { Text = "Hello" };
                try
                {
                    UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                }
                catch (Exception exception)
                {
                    UtilFramework.Assert(exception.Message == "Can not send data row to client!"); // V is object declaration therefore no Row detection.
                }
            }
            // Reference to removed ComponentJson
            {
                MyComponent source = new MyComponent(null);
                var html = new Html(source) { TextHtml = "My" };
                source.Html = html;
                html.ComponentRemove();
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                MyComponent dest = (MyComponent)UtilJson.Deserialize(jsonSession);
                UtilFramework.Assert(dest.Html == null);
            }
            // ComponentJson reference in list
            {
                MyComponent source = new MyComponent(null);
                var html = new Html(source) { TextHtml = "My" };
                source.HtmlList = new List<Html>();
                source.HtmlList.Add(html);
                // Serialize, deserialize
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                try
                {
                    var dest = (MyComponent)UtilJson.Deserialize(jsonSession);
                }
                catch (Exception exception)
                {
                    UtilFramework.Assert(exception.Message == "Reference to ComponentJson in List not supported!");
                }
            }
            {
                MyComponent source = new MyComponent(null);
                new MyComponent(source);
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                var dest = (MyComponent)UtilJson.Deserialize(jsonSession);
                UtilFramework.Assert(dest.List.Count == 1);
            }
            {
                MyComponent source = new MyComponent(null);
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                var dest = (MyComponent)UtilJson.Deserialize(jsonSession);
                UtilFramework.Assert(dest.Index == null);
            }
            {
                MyComponent source = new MyComponent(null);
                source.Index = 0;
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                var dest = (MyComponent)UtilJson.Deserialize(jsonSession);
                UtilFramework.Assert(dest.Index == 0);
            }
            {
                MyComponent source = new MyComponent(null);
                source.Index = -1;
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                var dest = (MyComponent)UtilJson.Deserialize(jsonSession);
                UtilFramework.Assert(dest.Index == -1);
            }
            {
                My source = new My();
                var myComponent1 = new MyComponent(null);
                Html html1 = new Html(myComponent1) { TextHtml = "A" };
                myComponent1.Dto = new Dto { Css = "A", Html = html1 };
                var myComponent2 = new MyComponent(null);
                Html html2 = new Html(myComponent2) { TextHtml = "B" };
                myComponent2.Dto = new Dto { Css = "B", Html = html2 };
                source.List.Add(myComponent1);

                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                var dest = (My)UtilJson.Deserialize(jsonSession);
                dest.List[0].Dto.Html.TextHtml = "abc";
                UtilFramework.Assert(((Html)dest.List[0].List[0]).TextHtml == "abc");

                source.List.Add(myComponent2);
                try
                {
                    UtilJson.Serialize(source, out jsonSession, out jsonClient);
                }
                catch (Exception exception)
                {
                    UtilFramework.Assert(exception.Message == "JsonClient can only have one ComponentJson graph!");
                }
            }
            {
                My source = new My();
                var myComponent1 = new MyComponent(null);
                Html html1 = new Html(myComponent1) { TextHtml = "A" };
                myComponent1.Dto = new Dto { Css = "A", Html = html1 };
                var myComponent2 = new MyComponent(null);
                Html html2 = new Html(myComponent2) { TextHtml = "B" };
                myComponent2.Dto = new Dto { Css = "B", Html = html2 }; 
                var myComponent3 = new MyComponent(myComponent1);
                source.List.Add(myComponent3); // Reference not to root!
                try
                {
                    UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                }
                catch (Exception exception)
                {
                    UtilFramework.Assert(exception.Message == "Referenced ComponentJson not root!");
                }
                source.List.Remove(myComponent3);
                source.List.Add(myComponent1);
                source.List.Add(myComponent2);
                try
                {
                    UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                }
                catch (Exception exception)
                {
                    UtilFramework.Assert(exception.Message == "JsonClient can only have one ComponentJson graph!");
                }
            }
            {
                My source = new My();
                var myComponent1 = new MyComponent(null);
                Html html1 = new Html(myComponent1) { TextHtml = "A" };
                myComponent1.Dto = new Dto { Css = "A", Html = html1 };
                var myComponent2 = new MyComponent(null);
                Html html2 = new Html(myComponent2) { TextHtml = "B" };
                myComponent2.Dto = new Dto { Css = "B", Html = html1 }; // Reference to object in different graph
                source.List.Add(myComponent2);
                source.List.Add(myComponent1);
                try
                {
                    UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                }
                catch (Exception exception)
                {
                    UtilFramework.Assert(exception.Message == "Referenced ComponentJson not in same object graph!");
                }
            }
            {
                var source = new MyComponent(null);

                source.Html = new Html(source) { TextHtml = "Hello" };

                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);

                UtilFramework.Assert(!jsonSession.Contains("Owner"));

                var dest = (MyComponent)UtilJson.Deserialize(jsonSession);

                var htmlOne = dest.Html;
                var htmlTwo = dest.List.OfType<Html>().First();

                htmlOne.TextHtml = "K";
                UtilFramework.Assert(htmlOne.TextHtml == htmlTwo.TextHtml);
            }
            // Referenced ComponentJson not in same graph
            {
                var source = new MyComponent(null);
                source.Html = new Html(null);

                try
                {
                    UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                }
                catch (Exception exception)
                {
                    UtilFramework.Assert(exception.Message == "Referenced ComponentJson not in same object graph!");
                }
            }
        }
    }

    public class MyApp : ComponentJson
    {
        public MyApp() 
            : base(null, nameof(MyApp))
        {

        }

        public Div Div;

        public BootstrapCol Col;
        
        public DivContainer Row;

        public MyCell MyCell;

        public int PropertyReadOnly => 9;
    }

    public class MyCell
    {
        public string MyText;

        [Serialize(SerializeEnum.Client)]
        public MyGrid MyGrid;

        public MyGrid MyGrid2;

        [Serialize(SerializeEnum.Both)]
        public MyGrid MyGridBoth;
    }

    public class MyGrid : ComponentJson
    {
        public MyGrid(ComponentJson owner) 
            : base(owner, nameof(MyGrid))
        {

        }

        public string Text;
    }

    public class My
    {
        public List<MyComponent> List = new List<MyComponent>();

        public MyComponent MyComponent;
    }

    public class MyComponent : ComponentJson
    {
        public MyComponent(ComponentJson owner) 
            : base(owner, nameof(MyComponent))
        {

        }

        public Html Html;

        public Html HtmlAbc;

        [Serialize(SerializeEnum.Session)]
        public string MyTextSession;

        [Serialize(SerializeEnum.Client)]
        public string MyTextClient;

        [Serialize(SerializeEnum.None)]
        public string MyIgnore;

        public Dto Dto;

        public int? Index;

        public List<Html> HtmlList;

        public MyRow MyRow;

        public List<Row> MyRowList;

        public object V;

        public MyComponent Component;
    }

    public class Dto
    {
        public string Css;

        public Html Html;
    }

    public class MyRow : Row
    {
        public string Text { get; set; }

        public DateTime DateTime { get; set; }
    }

    public enum MyEnum { None = 0, Left = 1, Right = 2 }

    public class A
    {
        public MyEnum MyEnum;

        public MyEnum? MyEnumNullable;

        public List<MyEnum> MyEnumList;

        public List<MyEnum?> MyEnumNullableList;

        public List<int> IntList;

        public List<int?> IntNullableList;

        public object V;

        public FrameworkDeployDb Row;
    }

    public class AppMain : AppJson
    {
        public AppMain()
        {
            this.Row = new BootstrapRow(this);
            this.Col = new BootstrapCol(Row);
        }

        public BootstrapRow Row;

        public BootstrapCol Col;
    }
}