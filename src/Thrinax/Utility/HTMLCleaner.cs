using HtmlAgilityPack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Thrinax.Utility
{
    public class HTMLCleaner
    {
        /// <summary>
        /// ȥ����ǩ�������еȸ�ʽ�ַ�
        /// </summary>
        /// <param name="HTML">����ϴ��HTML</param>
        /// <param name="CleanMedia">�Ƿ���ϴ��Img��ǩ����Ƶ��ǩ</param>
        /// <returns>��ϴ�������ı�</returns>
        public static string CleanHTML(string HTML, bool CleanMedia)
        {
            if (string.IsNullOrEmpty(HTML)) return null;

            //ȥ��HTML��ǩ
            HTML = StripHtml(HTML, CleanMedia);
            HTML = RestoreReplacement(HTML);
            HTML = CleanSpaces(HTML);

            return HTML;
        }

        #region ������ϴ��ʽ

        /// <summary>
        /// ��media�ֶ��л�ȡת��ý�����ϴ��ʽ����TOP20�ܽ�Ĺ���
        /// </summary>
        /// <param name="MediaName">����ת��ý����ֶ�</param>
        /// <returns></returns>
        public static string CleanMediaName(string MediaName)
        {
            MediaName = TextCleaner.FullClean(MediaName);
            if (MediaName.Contains("��Դ")) MediaName = MediaName.Substring(MediaName.IndexOf("��Դ") + 3);
            while (!string.IsNullOrWhiteSpace(MediaName) && MediaName.StartsWith(" ")) MediaName = MediaName.Substring(1);
            if (MediaName.Contains(" ")) MediaName = MediaName.Substring(0, MediaName.IndexOf(' '));
            MediaName = MediaName.Replace(")", "").Replace("��", "");
            if (!string.IsNullOrWhiteSpace(MediaName))
                return MediaName;
            else return null;
        }

        /// <summary>
        /// ��media�ֶ��л�ȡ���ߵ���ϴ��ʽ����TOP20�ܽ�Ĺ���
        /// </summary>
        /// <param name="Author">�������ߵ��ֶ�</param>
        /// <returns></returns>
        public static string CleanAuthor(string Author)
        {
            Author = TextCleaner.FullClean(Author);
            if (Author.Contains("����")) Author = Author.Substring(Author.IndexOf("����") + 3);
            if (Author.Contains("��Դ")) Author = Author.Substring(0, Author.IndexOf("��Դ"));
            if (Author.Contains("����ʱ��")) Author = Author.Substring(0, Author.IndexOf("����ʱ��"));

            if (!string.IsNullOrWhiteSpace(Author))
                return Author;
            else return null;
        }


        /// <summary>
        /// ���������ݵ���ϴ������TOP20�ܽ���ģ���Ϊ�㷺���õĹ���
        /// </summary>
        /// <param name="nodes">ͨ��ItemContentXPathѡ����nodes</param>
        /// <param name="Url">��url������FormatHtml�������������¸�ʽ</param>
        /// <param name="Format">�Ƿ�����FormatHtml���������¸�ʽ�������������ں��ڻ���ϴ��p��br�ȱ�ǩ</param>
        /// <returns></returns>
        public static string CleanContent(HtmlNodeCollection nodes, string Url, bool Format = true)
        {
            string Content = string.Empty;
            foreach (HtmlNode cnode in nodes)
            {
                string temp = HtmlFormattor.FormatHtml(cnode.InnerHtml, Url);
                temp = CleanContent_CleanEditor(temp);
                temp = CleanContent_CleanA(temp);
                if (!Format) temp = TextCleaner.FullClean(temp);
                Content += temp;
            }
            return Content;
        }
        public static string CleanContent(List<HtmlNode> nodes, string Url)
        {
            string Content = string.Empty;
            foreach (HtmlNode cnode in nodes)
            {
                string temp = HtmlFormattor.FormatHtml(cnode.InnerHtml, Url);
                Content += temp;
            }
            return Content;
        }

        /// <summary>
        /// ���������ݵ���ϴ������ϴ�����༭��XXX������ֶ�
        /// </summary>
        /// <param name="Content">��������</param>
        /// <returns></returns>
        internal static string CleanContent_CleanEditor(string Content, string Title = "��")
        {
            if (string.IsNullOrWhiteSpace(Content)) return Content;
            string resultContent = string.Empty;
            string temp = Content;
            string[] WordstoClean = @"�༭ ���� ���� ���� ���� ���� ���ں�".Split();
            if (Title.Length > 5)
            {
                temp = temp.Substring(0, Math.Min(Title.Length * 5, temp.Length));
                if (temp.Contains(Title))
                    Content = Content.Substring(0, Math.Min(Title.Length * 5, temp.Length)).Replace(Title, "") + Content.Substring(Math.Min(Title.Length * 5, temp.Length));
            }
            temp = Content;
            while (Content.Length > 0)
            {
                char[] leftb = new char[1];
                leftb[0] = '('; 
                //leftb[1] = '��';
                char[] rightb = new char[1]; 
                rightb[0] = ')';
                //rightb[1] = '��';
                int indexleftb = Content.IndexOfAny(leftb);
                int indexrightb = Content.IndexOfAny(rightb);

                //����Ҳ�������һ�ߵ�������ֱ�ӽ����˳�
                if (Math.Min(indexleftb, indexrightb) < 0)
                { resultContent = resultContent + Content; Content = string.Empty; continue; }

                //����ܲ�������ȳ����������Ż�������������һ�����������Ź���
                if (indexrightb < indexleftb + 2)
                { resultContent = resultContent + Content.Substring(0, indexrightb + 1); Content = Content.Substring(indexrightb + 1); continue; }

                if (indexrightb > indexleftb + 1)
                {
                    try
                    {
                        string mid = Content.Substring(indexleftb + 1, indexrightb - indexleftb - 1); bool toclean = false;

                        foreach (string test in WordstoClean)
                            if (mid.Contains(test))
                                toclean = true;
                        if (toclean)
                        {
                            resultContent = indexleftb > 0 ? resultContent + Content.Substring(0, indexleftb - 1) : resultContent;
                            Content = Content.Substring(indexrightb + 1);
                        }
                        else
                        {
                            resultContent = resultContent + Content.Substring(0, indexrightb + 1);
                            Content = Content.Substring(indexrightb + 1);
                        }
                    }
                    catch { }
                }
            }

            if (Content.Length < 50)
                foreach (string text in WordstoClean)
                    if (Content.Contains(text))
                        Content = string.Empty;

            resultContent = resultContent + Content;

            return resultContent;
        }

        /// <summary>
        /// ���������ݵ���ϴ����ϴ����������ҳ�����a��ǩ
        /// </summary>
        /// <param name="Content">��������</param>
        /// <returns></returns>
        internal static string CleanContent_CleanA(string Content)
        {
            if (string.IsNullOrWhiteSpace(Content)) return Content;
            string resultContent = string.Empty;
            string[] WordstoClean = @"�༭ ���� ���� ���� ���� ���� ���� ���ں� ��ӡ ��ҳ ����".Split();

            //������ϴ�����岿��
            while (Content.Length > 0)
            {
                int aleft = Content.IndexOf("<a");
                int aright = Content.IndexOf("</a>");

                if (Math.Min(aleft, aright) < 0)//���ֻ��һ����������û���Ǿ�����֮��ȫ���ӵ����ȥ
                { resultContent = resultContent + Content; Content = string.Empty; continue; }

                if (aright < aleft + 4)
                {//������a��ǩ�ǿյ��ǾͲ�������
                    resultContent = resultContent + Content.Substring(0, aright + 4);
                    Content = Content.Substring(aright + 4);
                    continue;
                }

                if (aright > aleft + 3)
                {
                    try
                    {
                        string mid = Content.Substring(aleft + 3, aright - aleft - 3); bool toclean = false;
                        foreach (string stopword in WordstoClean)
                            if (mid.Contains(stopword))
                            { toclean = true; break; }//����Ƿ���Ҫ�������a��ǩ
                        if (toclean)
                        {
                            resultContent = resultContent + Content.Substring(0, aleft);
                            Content = Content.Substring(aright + 4);
                            continue;
                        }
                        else
                        {
                            resultContent = resultContent + Content.Substring(0, aright + 4);
                            Content = Content.Substring(aright + 4);
                            continue;
                        }
                    }
                    catch { }
                }
            }
            return resultContent;
        }

        #endregion ������ϴ��ʽ

        /// <summary>
        /// ȥ�������еȸ�ʽ�ַ�
        /// </summary>
        /// <param name="HTML"></param>
        /// <returns></returns>
        public static string CleanSpaces(string HTML)
        {
            //ȥ��\n \t \r &nbsp; �ϲ������Ŀո�
            HTML = nRegex.Replace(HTML,"");
            HTML = trimRegex.Replace(HTML, " ");
            HTML = wRegex.Replace(HTML,  " ");
            HTML = sRegex.Replace(HTML, " ");

            return HTML.Trim();
        }
        private static Regex nRegex = new Regex(@"\n", RegexOptions.Compiled);
        private static Regex trimRegex = new Regex(@"\t|\r|&nbsp;|&nbsp", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static Regex wRegex = new Regex(@"&\w+;", RegexOptions.Compiled);
        private static Regex sRegex = new Regex(@"\s+", RegexOptions.Compiled);
        private static Regex nbspRegex = new Regex(@"&nbsp;", RegexOptions.Compiled);
        private static Regex gtRegex = new Regex(@"&gt;", RegexOptions.Compiled);
        private static Regex ltRegex = new Regex(@"&lt;", RegexOptions.Compiled);
        private static Regex ampRegex = new Regex(@"&amp;", RegexOptions.Compiled);
        private static Regex quotRegex = new Regex(@"&quot;", RegexOptions.Compiled);

        public static string RestoreReplacement(string HTML)
        {
            //�����д�������ԭ
            HTML = nbspRegex.Replace(HTML, " ");
            HTML = gtRegex.Replace(HTML, ">");
            HTML = ltRegex.Replace(HTML,  "<");
            HTML = ampRegex.Replace(HTML,  "&");
            HTML = quotRegex.Replace(HTML, "\"");

            return HTML;
        }

        private static Regex removeImgRegex = new Regex(@"<(?:.|\n)+?>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static Regex retainImgRegex = new Regex(@"<(?!\s*(img|embed|/embed|iframe|/iframe))(?:.|\n)+?>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        /// <summary>
        /// ��ȥ��HTML��ǩ
        /// </summary>
        /// <remarks>
        /// Stripping HTML
        /// http://www.4guysfromrolla.com/webtech/042501-1.shtml
        ///
        /// Using regex to find tags without a trailing slash
        /// http://concepts.waetech.com/unclosed_tags/index.cfm
        ///
        /// http://msdn.microsoft.com/library/en-us/script56/html/js56jsgrpregexpsyntax.asp
        /// </remarks>
        public static string StripHtml(string HTML, bool CleanImg)
        {
            string styleless = StripScriptAndStyle(HTML);

            //Strips the HTML tags from the Html
            
            Regex objRegExp = 
                CleanImg ? removeImgRegex : retainImgRegex;

            //Replace all HTML tag matches with the empty string
            string strOutput = objRegExp.Replace(styleless, string.Empty);

            //Replace all < and > with &lt; and &gt;
            //strOutput = strOutput.Replace("<", "&lt;");
            //strOutput = strOutput.Replace(">", "&gt;");

            objRegExp = null;
            return strOutput;
        }
        private static Regex scriptsRegex = new Regex(@"<script[^>.]*>[\s\S]*?</script>", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ExplicitCapture | RegexOptions.Compiled);
        private static Regex stylesRegex = new Regex(@"<style[^>.]*>[\s\S]*?</style>", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ExplicitCapture | RegexOptions.Compiled);
        private static Regex inlineJavascriptRegex = new Regex(@"javascript:.*?""", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ExplicitCapture | RegexOptions.Compiled);

        /// <summary>
        /// ȥ���ű�����ʽ
        /// </summary>
        /// <param name="HTML"></param>
        /// <returns></returns>
        public static string StripScriptAndStyle(string HTML)
        {
            //Stripts the <script> tags from the Html
            string cleanedText = scriptsRegex.Replace(HTML, string.Empty);

            //Stripts the <style> tags from the Html
            cleanedText = stylesRegex.Replace(cleanedText, string.Empty);
            cleanedText = inlineJavascriptRegex.Replace(cleanedText, string.Empty);
            return cleanedText;
        }
        private static ConcurrentDictionary<string,Regex> removeTagRegDictionary =new ConcurrentDictionary<string, Regex>();
        private static ConcurrentDictionary<string, Regex> removeInnerRegDictionary = new ConcurrentDictionary<string, Regex>(); 
        /// <summary>
        /// ȥ��ָ����Html��ǩ
        /// </summary>
        /// <param name="HTML"></param>
        /// <param name="Tag"></param>
        /// <param name="RemoveInnerHTML">�Ƿ�ȥ���ڲ�����</param>
        /// <returns></returns>
        public static string StripHtmlTag(string HTML, string Tag, bool RemoveInnerHTML)
        {
            if (string.IsNullOrEmpty(Tag) || string.IsNullOrEmpty(HTML)) return HTML;
            Regex replacereg;
            if (RemoveInnerHTML)
            {
                if (!removeInnerRegDictionary.ContainsKey(Tag))
                {
                    string regexStr = string.Format(@"<\s*{0}[^>]*?>[\s\S]*?</\s*{0}\s*>", Tag);
                    Regex regex = new Regex(regexStr,RegexOptions.IgnoreCase|RegexOptions.Compiled);
                    removeInnerRegDictionary.TryAdd(Tag,regex);
                }
                replacereg = removeInnerRegDictionary[Tag];
            }
            else
            {
                if (!removeTagRegDictionary.ContainsKey(Tag))
                {
                    string regexStr = string.Format(@"<(?:/)?\s*{0}[^>]*?>", Tag);
                    Regex regex = new Regex(regexStr,RegexOptions.IgnoreCase|RegexOptions.Compiled);
                    removeTagRegDictionary.TryAdd(Tag, regex);
                }
                replacereg = removeTagRegDictionary[Tag];
            }


            return replacereg.Replace(HTML, string.Empty);
        }

        /// <summary>
        /// ���Html��ǩ�е�����
        /// </summary>
        /// <param name="HTML"></param>
        /// <param name="Tag"></param>
        /// <returns></returns>
        public static string StripHtmlProperty(string HTML, string Tag)
        {
            if (string.IsNullOrEmpty(Tag) || string.IsNullOrEmpty(HTML)) return HTML;
            Regex replacereg;

            if (!removeInnerRegDictionary.ContainsKey(Tag))
            {
                string regexStr = string.Format(@"\s*{0}\s*=""[^""]*?""\s*", Tag);
                Regex regex = new Regex(regexStr, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                removeInnerRegDictionary.TryAdd(Tag, regex);
            }
            replacereg = removeInnerRegDictionary[Tag];

            return replacereg.Replace(HTML, " ");
        }

        /// <summary>
        /// ��ȡHTML��Title��ǩ����
        /// </summary>
        /// <param name="HTML"></param>
        /// <returns></returns>
        public static string GetTitle(string HTML)
        {
            HtmlDocument Document = new HtmlDocument();
            Document.LoadHtml(HTML);

            HtmlNode TitleNode = Document.DocumentNode.SelectSingleNode("//title");
            if (TitleNode != null)
                return TitleNode.InnerText;
            else
                return string.Empty;
        }

        /// <summary>
        /// ȥ��ǰ�ͺ�����ͬ�Ӵ�
        /// </summary>
        /// <param name="Str">����ϴ�ַ���</param>
        /// <param name="CompareStr">���Ƚ��ַ���</param>
        /// <returns></returns>
        public static string StringCompareClean(string Str, string CompareStr)
        {
            if (string.IsNullOrEmpty(Str) || string.IsNullOrEmpty(CompareStr)) return Str;
            
            string sStr = Str.ToLower();
            CompareStr = CompareStr.ToLower();

            int Start = 0;
            int End = 0; 

            //��ǰ��ʼ����ͬ�ַ��ĸ���
            try
            {
                while (Start < sStr.Length & Start < CompareStr.Length)
                    if (sStr[Start] == CompareStr[Start])
                        Start++;
                    else
                        break;
            }
            catch (Exception ex)
            {
                //Console.WriteLine(string.Format("Str={0} CompareStr={1} Start={2} ex={3}", Str, CompareStr, Start, ex.Message));
            }

            //����ϴ�ַ����ǱȽ��ַ���ǰ׺������ϴ������ԭ�����
            if (Start == sStr.Length)
            {
                return Str;
            }
            else
            {
                //�Ӻ�ʼ����ͬ�ַ��ĸ���
                while (End < sStr.Length & End < CompareStr.Length & sStr[sStr.Length - End - 1] == CompareStr[CompareStr.Length - End - 1])
                    End++;

                return Str.Substring(Start, Str.Length - Start - End);
            }
        }

        /// <summary>
        /// ��HTML���������CompareHTML��ͬ�Ĳ��֣����PlainText
        /// </summary>
        /// <param name="HTML"></param>
        /// <param name="CompareHTML"></param>
        /// <param name="Title"></param>
        /// <returns></returns>
        public static void DOMCompareClean(string OriHTML, string CompareHTML, bool CleanText, ref string Title, ref string HTML)
        {
            ///����HtmlDocument����
            HtmlDocument Document = new HtmlDocument();
            Document.LoadHtml(OriHTML);
            HtmlDocument CompareDocument = new HtmlDocument();
            CompareDocument.LoadHtml(CompareHTML);

            ///�����ͬ�ڵ�
            DOMCompareClean(Document.DocumentNode, CompareDocument.DocumentNode);

            ///����ʣ��Ҷ�ӽڵ���ı�
            StringBuilder Builder = new StringBuilder(500);
            if (CleanText)
                AppendLeavesText(Builder, Document.DocumentNode);
            else
                AppendLeavesHTML(Builder, Document.DocumentNode, (int)Math.Truncate(TextLengh(Document.DocumentNode)*0.6));
            HTML = Builder.ToString();

            ///����
            Title = StringCompareClean( GetTitle(HTML), GetTitle(CompareHTML));
            if (CleanText)
                Title = CleanHTML(Title, true);

        }

        /// <summary>
        /// �ݹ麯������һ���ڵ����һ���ڵ�Ƚϣ�ɾ����ͬ���ӽڵ㣨����DOMCompareClean��
        /// </summary>
        private static void DOMCompareClean(HtmlNode Node, HtmlNode CompareNode)
        {
            ///����HTML��ÿһ���ӽڵ�
            for (int SubNode = 0; Node.HasChildNodes && SubNode < Node.ChildNodes.Count;)
            {
                ///����Ƿ�ɾ���˽ڵ�
                bool CutIt = false;
               
                ///����JS��CSS�ͱ�עֱ��ɾ��
                if (string.Compare(Node.ChildNodes[SubNode].Name, "style", true) == 0
                    || string.Compare(Node.ChildNodes[SubNode].Name, "script", true) == 0
                    || string.Compare(Node.ChildNodes[SubNode].Name, "meta", true) == 0
                    || Node.ChildNodes[SubNode].NodeType == HtmlNodeType.Comment)
                    ///ɾ���˽ڵ�
                    CutIt = true;
                else
                    ///����CompareHTML��ÿһ���ӽڵ�
                    for (int SubCompareNode = 0; CompareNode.HasChildNodes && SubCompareNode < CompareNode.ChildNodes.Count; SubCompareNode++)
                        ///��ÿһ��ͬ����ͬId����Ԫ�أ������бȽ�
                        ///�������ƻ�Id�����ڵ����
                        ///���������±��������Ԫ�أ�id���Ƚϴ󣩣�����id�Ĳ�һ�£���Ȼ���бȽ�
                        if (string.Compare(Node.ChildNodes[SubNode].Name, CompareNode.ChildNodes[SubCompareNode].Name, true) == 0
                            && (Node.ChildNodes[SubNode].Id != null && Node.ChildNodes[SubNode].Id.Length > 6
                                || string.Compare(Node.ChildNodes[SubNode].Id, CompareNode.ChildNodes[SubCompareNode].Id, true) == 0))
                        {
                            switch (Node.ChildNodes[SubNode].NodeType)
                            {
                                ///�ı��ڵ�ֱ�ӱȽϲ��ݹ�
                                case HtmlNodeType.Comment:
                                case HtmlNodeType.Text:
                                    string CleanInnerText = CleanSpaces(Node.ChildNodes[SubNode].InnerText);
                                    ///����ı���ͬ����ɾ��
                                    if (string.IsNullOrEmpty(CleanInnerText)
                                        || string.Compare(CleanInnerText, CleanSpaces(CompareNode.ChildNodes[SubCompareNode].InnerText), true) == 0)
                                        ///ɾ���˽ڵ�
                                        CutIt = true;
                                    break;
                                ///���ı��ڵ���Ҫ�ݹ�
                                default:
                                    ///���û���ӽڵ㣬�ձ�ǩ,��ɾ��
                                    if (!Node.ChildNodes[SubNode].HasChildNodes)
                                        CutIt = true;
                                    ///����ݹ�
                                    else
                                    {
                                        DOMCompareClean(Node.ChildNodes[SubNode], CompareNode.ChildNodes[SubCompareNode]);
                                        ///��������ӽڵ㶼cutted��ĸ�ڵ�ҲӦcutted
                                        if (!Node.ChildNodes[SubNode].HasChildNodes || Node.ChildNodes[SubNode].ChildNodes.Count == 0)
                                            CutIt = true;
                                    }
                                    break;

                            }
                            ///����Ѿ�ɾ��������Ҫ�ټ��CompareHTML�������ڵ�
                            if (CutIt) break;
                        }

                ///ɾ������ڵ�
                if (CutIt)
                {
                    Node.ChildNodes[SubNode].RemoveAllChildren();
                    Node.RemoveChild(Node.ChildNodes[SubNode]);
                }
                else
                    SubNode++;
            }
        }

        /// <summary>
        /// �ݹ麯�������ýڵ��µ�Ҷ�ӽڵ���ı���ӵ�StringBuilder������DOMCompareClean��
        /// </summary>
        /// <param name="Builder"></param>
        /// <param name="Node"></param>
        private static void AppendLeavesText(StringBuilder Builder, HtmlNode Node)
        {
            if (Node.HasChildNodes)
            {
                foreach (HtmlNode SubNode in Node.ChildNodes)
                    AppendLeavesText(Builder, SubNode);
            }
            else
                Builder.Append(CleanSpaces(Node.InnerText)).Append(" ");
        }

        /// <summary>
        /// �ݹ麯�����ҵ�����ָ������������Ч�ı�����Զ�ڵ㣬����HTML��ӵ�StringBuilder������DOMCompareClean��
        /// </summary>
        /// <param name="Builder"></param>
        /// <param name="Node"></param>
        /// <returns>�Ƿ��ҵ�����Ҫ��Ľڵ㲢����</returns>
        private static bool AppendLeavesHTML(StringBuilder Builder, HtmlNode Node, int Length)
        {
            if (TextLengh(Node) < Length) return false;

            if (Node.HasChildNodes)
            {
                foreach (HtmlNode SubNode in Node.ChildNodes)
                    if (AppendLeavesHTML(Builder, SubNode, Length))
                        return true;
            }

            Builder.Append((string) Node.InnerHtml);
            return true;
        }

        /// <summary>
        /// �ݹ麯��������ڵ��µ���Ч�ı����ȣ�����AppendLeavesHTML��
        /// </summary>
        /// <param name="Node"></param>
        /// <returns></returns>
        private static int TextLengh(HtmlNode Node)
        {
            int Length = 0;
            if (Node.HasChildNodes)
            {
                foreach (HtmlNode SubNode in Node.ChildNodes)
                    Length += TextLengh(SubNode);
            }
            else
                Length = Node.InnerText.Length;
            
            return Length;
        }

        /// <summary>
        /// ���Url�����Ƿ���ȷ��Ч
        /// </summary>
        /// <param name="Url"></param>
        /// <returns></returns>
        public static bool isUrlGood(string Url)
        {
            return !(Url.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase) || Url.StartsWith("vbscript:", StringComparison.OrdinalIgnoreCase)
                || Url.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase) || Url.StartsWith("#")
                || Url.StartsWith("thunder:", StringComparison.OrdinalIgnoreCase)
                || Url.StartsWith("file:", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// �������ӣ�ȥ��# js ê��ȵ�
        /// </summary>
        /// <param name="Url"></param>
        /// <returns></returns>
        public static string CleanUrl(string Url)
        {
            if (string.IsNullOrEmpty(Url)) return null;

            Url = Url.Trim();
            if (!isUrlGood(Url)) return null;

            //ȥ������ê����
            Url = Regex.Replace(Url, @"#\w+", string.Empty);

            //ȥ��ĩβ��"/"
            if (Url.Length > 1)
                Url.TrimEnd('/');
            return Url;
        }

        /// <summary>
        /// HtmlAgilityPack��InnerText���Ի����Script�ȣ�Ҫ���������������InnerText
        /// </summary>
        /// <param name="Node"></param>
        /// <returns></returns>
        public static string GetCleanInnerText(HtmlNode Node)
        {
            try
            {
                foreach (var script in Node.Descendants("script"))
                    script.Remove();
                foreach (var style in Node.Descendants("style"))
                    style.Remove();
            }
            catch { }
            HtmlNodeCollection comments = Node.SelectNodes("//comment()");
            if (comments != null)
                foreach (var comment in comments)
                    comment.Remove();

            return Node.InnerText;
        }
    }
}
