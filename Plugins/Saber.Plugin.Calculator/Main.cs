using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using YAMP;

namespace Saber.Plugin.Caculator
{

    //https://github.com/FlorianRappl/YAMP

    public class Main : IPlugin, IPluginI18n
    {
        private static Regex regValidExpressChar = new Regex(
                        @"^(" +
                        @"ceil|floor|exp|pi|e|max|min|det|abs|log|ln|sqrt|" +
                        @"sin|cos|tan|arcsin|arccos|arctan|" +
                        @"eigval|eigvec|eig|sum|polar|plot|round|sort|real|zeta|" +
                        @"bin2dec|hex2dec|oct2dec|" +
                        @"==|~=|&&|\|\||" +
                        @"[ei]|[0-9]|[\+\-\*\/\^\., ""]|[\(\)\|\!\[\]]" +
                        @")+$", RegexOptions.Compiled);
        private static Regex regBrackets = new Regex(@"[\(\)\[\]]", RegexOptions.Compiled);


        private static Parser parser;


        private PluginInitContext context { get; set; }

        private static PluginMetadata metadata;

        static Main()
        {
            parser = new Parser();
            parser.InteractiveMode = false;
            parser.UseScripting = false;


            metadata = new PluginMetadata();
            metadata.ID = "CEA0FDFC6D3B4085823D60DC76F28855";
            metadata.Name = "Calculator";
            metadata.ActionKeyword = "*";
            metadata.IcoPath = "Images\\calculator.png";
        }

        public List<Result> Query(Query query)
        {
            if (query.Search.Length <= 2          // don't affect when user only input "e" or "i" keyword
                || !regValidExpressChar.IsMatch(query.Search)
                || !IsBracketComplete(query.Search)) return new List<Result>();

            try
            {
                var result = parser.Parse(query.Search);
                result.Run();
                if (result.Output != null && !string.IsNullOrEmpty(result.Result))
                {
                    return new List<Result>
                    { new Result
                    { 
                        Title = result.Result, 
                        IcoPath = "Images/calculator.png", 
                        Score = 300,
                        SubTitle = "Copy this number to the clipboard", 
                        Action = c =>
                        {
                            try
                            {
                                Clipboard.SetText(result.Result);
                                return true;
                            }
                            catch (ExternalException e)
                            {
                                MessageBox.Show("Copy failed, please try later");
                                return false;
                            }
                        }
                    } };
                }
            }
            catch
            {}

            return new List<Result>();
        }

        private bool IsBracketComplete(string query)
        {
            var matchs = regBrackets.Matches(query);
            var leftBracketCount = 0;
            foreach (Match match in matchs)
            {
                if (match.Value == "(" || match.Value == "[")
                {
                    leftBracketCount++;
                }
                else
                {
                    leftBracketCount--;
                }
            }

            return leftBracketCount == 0;
        }

        public void Init(PluginInitContext context)
        {
            this.context = context;
        }

        public string GetTranslatedPluginTitle()
        {
            return "计算器";
        }

        public string GetTranslatedPluginDescription()
        {
            return "为Saber提供数学计算能力。(试着在Saber输入 5*3-2)";
        }

        public PluginMetadata Metadata()
        {
            return metadata;
        }
    }
}
