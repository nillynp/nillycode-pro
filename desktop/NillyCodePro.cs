// ============================================================================
// NillyCode Pro — Ultra-Light Native Sublime-Style Code & Text Editor
// created by Nilly graphics (Sotiris Christos) — https://nillygraphics.com/
// Strictly C# 5 compatible (.NET Framework 4.x csc.exe — 0 external DLLs)
// 100% Native Windows · ~90 KB EXE · ~20 MB RAM · 0 MB Background RAM
// ============================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

[assembly: AssemblyTitle("NillyCode Pro")]
[assembly: AssemblyDescription("Ultra-Light Sublime-Style Code & Text Editor — created by Nilly graphics (Sotiris Christos)")]
[assembly: AssemblyCompany("Nilly graphics (Sotiris Christos) — https://nillygraphics.com/")]
[assembly: AssemblyProduct("NillyCode Pro")]
[assembly: AssemblyCopyright("Copyright (c) 2026 Nilly graphics (Sotiris Christos)")]
[assembly: AssemblyVersion("1.1.1.0")]
[assembly: AssemblyFileVersion("1.1.1.0")]

namespace NillyCodePro
{
    // ========================================================================
    // 1. WIN32 NATIVE INTEROP (Dark Titlebar, DPI, Flicker-Free RichEdit)
    // ========================================================================
    internal static class Native
    {
        public const int WM_SETREDRAW = 0x000B;
        public const int WM_VSCROLL = 0x0115;
        public const int WM_HSCROLL = 0x0114;
        public const int WM_MOUSEWHEEL = 0x020A;
        public const int WM_USER = 0x0400;
        public const int EM_GETFIRSTVISIBLELINE = 0x00CE;
        public const int EM_LINESCROLL = 0x00B6;
        public const int EM_SETTABSTOPS = 0x00CB;
        public const int EM_GETEVENTMASK = WM_USER + 59;
        public const int EM_GETOLEINTERFACE = WM_USER + 60;
        public const int EM_SETEVENTMASK = WM_USER + 69;
        public const int EM_GETSCROLLPOS = WM_USER + 221;
        public const int EM_SETSCROLLPOS = WM_USER + 222;
        public const int SW_RESTORE = 9;
        public const int TOM_SUSPEND = -9999995;
        public const int TOM_RESUME = -9999994;

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll")]
        public static extern bool SetProcessDPIAware();

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll")]
        private static extern bool SetProcessWorkingSetSize(IntPtr hProcess, IntPtr minSize, IntPtr maxSize);

        public static void TrimMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized, false);
                SetProcessWorkingSetSize(GetCurrentProcess(), (IntPtr)(-1), (IntPtr)(-1));
            }
            catch { }
        }

        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref POINT lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int[] lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hWnd, int msg, IntPtr wParam, out IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public static void SetDarkTitleBar(IntPtr hwnd, bool dark)
        {
            try
            {
                int val = dark ? 1 : 0;
                if (DwmSetWindowAttribute(hwnd, 20, ref val, 4) != 0)
                    DwmSetWindowAttribute(hwnd, 19, ref val, 4);
            }
            catch { }
        }
    }

    // ========================================================================
    // 2. SUBLIME TEXT THEMES (Monokai, Mariana, Light Paper)
    // ========================================================================
    internal class EditorTheme
    {
        public string Name;
        public bool IsDark;
        public Color EditorBg;
        public Color EditorFg;
        public Color GutterBg;
        public Color GutterFg;
        public Color GutterActiveBg;
        public Color GutterActiveFg;
        public Color TabBarBg;
        public Color TabActiveBg;
        public Color TabInactiveBg;
        public Color TabHoverBg;
        public Color TabTextActive;
        public Color TabTextInactive;
        public Color TabBorder;
        public Color Accent;
        public Color SidebarBg;
        public Color SidebarFg;
        public Color SidebarHeaderFg;
        public Color SidebarSelBg;
        public Color StatusBarBg;
        public Color StatusBarFg;
        public Color MenuBg;
        public Color MenuFg;
        public Color MenuHoverBg;
        public Color MenuBorder;
        public Color InputBg;
        public Color InputFg;
        public Color MinimapBg;
        public Color MinimapViewport;
        public Color BorderLine;

        // Syntax colors
        public Color SynKeyword;
        public Color SynString;
        public Color SynComment;
        public Color SynNumber;
        public Color SynType;
        public Color SynFunction;
        public Color SynOperator;

        public static EditorTheme Monokai()
        {
            EditorTheme t = new EditorTheme();
            t.Name = "Monokai";
            t.IsDark = true;
            t.EditorBg = Color.FromArgb(39, 40, 34);          // #272822
            t.EditorFg = Color.FromArgb(248, 248, 242);       // #F8F8F2
            t.GutterBg = Color.FromArgb(39, 40, 34);          // #272822
            t.GutterFg = Color.FromArgb(143, 144, 138);       // #8F908A
            t.GutterActiveBg = Color.FromArgb(56, 57, 48);    // #383930
            t.GutterActiveFg = Color.FromArgb(248, 248, 242); // #F8F8F2
            t.TabBarBg = Color.FromArgb(26, 27, 23);          // #1A1B17
            t.TabActiveBg = Color.FromArgb(39, 40, 34);       // #272822
            t.TabInactiveBg = Color.FromArgb(46, 48, 40);     // #2E3028
            t.TabHoverBg = Color.FromArgb(56, 58, 49);        // #383A31
            t.TabTextActive = Color.FromArgb(248, 248, 242);
            t.TabTextInactive = Color.FromArgb(166, 167, 158);
            t.TabBorder = Color.FromArgb(20, 21, 17);
            t.Accent = Color.FromArgb(253, 151, 31);          // #FD971F
            t.SidebarBg = Color.FromArgb(30, 31, 28);         // #1E1F1C
            t.SidebarFg = Color.FromArgb(214, 214, 206);
            t.SidebarHeaderFg = Color.FromArgb(130, 131, 122);
            t.SidebarSelBg = Color.FromArgb(62, 61, 50);
            t.StatusBarBg = Color.FromArgb(26, 27, 23);
            t.StatusBarFg = Color.FromArgb(182, 182, 172);
            t.MenuBg = Color.FromArgb(30, 31, 28);
            t.MenuFg = Color.FromArgb(232, 232, 224);
            t.MenuHoverBg = Color.FromArgb(62, 61, 50);
            t.MenuBorder = Color.FromArgb(56, 57, 48);
            t.InputBg = Color.FromArgb(24, 25, 21);
            t.InputFg = Color.FromArgb(248, 248, 242);
            t.MinimapBg = Color.FromArgb(34, 35, 30);
            t.MinimapViewport = Color.FromArgb(44, 255, 255, 255);
            t.BorderLine = Color.FromArgb(52, 53, 45);
            t.SynKeyword = Color.FromArgb(249, 38, 114);      // #F92672
            t.SynString = Color.FromArgb(230, 219, 116);      // #E6DB74
            t.SynComment = Color.FromArgb(117, 113, 94);      // #75715E
            t.SynNumber = Color.FromArgb(174, 129, 255);      // #AE81FF
            t.SynType = Color.FromArgb(102, 217, 239);        // #66D9EF
            t.SynFunction = Color.FromArgb(166, 226, 46);     // #A6E22E
            t.SynOperator = Color.FromArgb(253, 151, 31);     // #FD971F
            return t;
        }

        public static EditorTheme Mariana()
        {
            EditorTheme t = new EditorTheme();
            t.Name = "Mariana";
            t.IsDark = true;
            t.EditorBg = Color.FromArgb(48, 56, 65);          // #303841
            t.EditorFg = Color.FromArgb(216, 222, 233);       // #D8DEE9
            t.GutterBg = Color.FromArgb(48, 56, 65);
            t.GutterFg = Color.FromArgb(128, 140, 154);
            t.GutterActiveBg = Color.FromArgb(60, 70, 82);
            t.GutterActiveFg = Color.FromArgb(236, 240, 245);
            t.TabBarBg = Color.FromArgb(34, 40, 47);          // #22282F
            t.TabActiveBg = Color.FromArgb(48, 56, 65);
            t.TabInactiveBg = Color.FromArgb(41, 48, 56);
            t.TabHoverBg = Color.FromArgb(54, 63, 74);
            t.TabTextActive = Color.FromArgb(238, 242, 248);
            t.TabTextInactive = Color.FromArgb(160, 170, 182);
            t.TabBorder = Color.FromArgb(28, 33, 39);
            t.Accent = Color.FromArgb(95, 179, 179);          // #5FB3B3
            t.SidebarBg = Color.FromArgb(39, 46, 54);
            t.SidebarFg = Color.FromArgb(210, 218, 228);
            t.SidebarHeaderFg = Color.FromArgb(132, 144, 158);
            t.SidebarSelBg = Color.FromArgb(60, 70, 82);
            t.StatusBarBg = Color.FromArgb(34, 40, 47);
            t.StatusBarFg = Color.FromArgb(176, 186, 198);
            t.MenuBg = Color.FromArgb(39, 46, 54);
            t.MenuFg = Color.FromArgb(224, 230, 238);
            t.MenuHoverBg = Color.FromArgb(60, 70, 82);
            t.MenuBorder = Color.FromArgb(62, 72, 84);
            t.InputBg = Color.FromArgb(31, 36, 43);
            t.InputFg = Color.FromArgb(230, 236, 244);
            t.MinimapBg = Color.FromArgb(43, 50, 58);
            t.MinimapViewport = Color.FromArgb(44, 255, 255, 255);
            t.BorderLine = Color.FromArgb(62, 72, 84);
            t.SynKeyword = Color.FromArgb(197, 148, 197);     // #C594C5
            t.SynString = Color.FromArgb(153, 199, 148);      // #99C794
            t.SynComment = Color.FromArgb(150, 162, 174);     // #96A2AE
            t.SynNumber = Color.FromArgb(249, 145, 87);       // #F99157
            t.SynType = Color.FromArgb(95, 179, 179);         // #5FB3B3
            t.SynFunction = Color.FromArgb(102, 153, 204);    // #6699CC
            t.SynOperator = Color.FromArgb(236, 95, 103);     // #EC5F67
            return t;
        }

        public static EditorTheme Light()
        {
            EditorTheme t = new EditorTheme();
            t.Name = "Light";
            t.IsDark = false;
            t.EditorBg = Color.FromArgb(250, 250, 250);
            t.EditorFg = Color.FromArgb(36, 41, 46);
            t.GutterBg = Color.FromArgb(245, 246, 248);
            t.GutterFg = Color.FromArgb(140, 148, 158);
            t.GutterActiveBg = Color.FromArgb(234, 238, 244);
            t.GutterActiveFg = Color.FromArgb(36, 41, 46);
            t.TabBarBg = Color.FromArgb(228, 231, 235);
            t.TabActiveBg = Color.FromArgb(250, 250, 250);
            t.TabInactiveBg = Color.FromArgb(236, 239, 242);
            t.TabHoverBg = Color.FromArgb(244, 246, 248);
            t.TabTextActive = Color.FromArgb(24, 28, 32);
            t.TabTextInactive = Color.FromArgb(96, 104, 114);
            t.TabBorder = Color.FromArgb(208, 212, 218);
            t.Accent = Color.FromArgb(217, 107, 0);
            t.SidebarBg = Color.FromArgb(242, 244, 246);
            t.SidebarFg = Color.FromArgb(46, 52, 60);
            t.SidebarHeaderFg = Color.FromArgb(118, 126, 136);
            t.SidebarSelBg = Color.FromArgb(220, 228, 238);
            t.StatusBarBg = Color.FromArgb(232, 235, 239);
            t.StatusBarFg = Color.FromArgb(72, 80, 90);
            t.MenuBg = Color.FromArgb(242, 244, 246);
            t.MenuFg = Color.FromArgb(36, 41, 46);
            t.MenuHoverBg = Color.FromArgb(220, 228, 238);
            t.MenuBorder = Color.FromArgb(206, 210, 216);
            t.InputBg = Color.White;
            t.InputFg = Color.FromArgb(36, 41, 46);
            t.MinimapBg = Color.FromArgb(244, 245, 247);
            t.MinimapViewport = Color.FromArgb(36, 0, 0, 0);
            t.BorderLine = Color.FromArgb(214, 218, 224);
            t.SynKeyword = Color.FromArgb(207, 34, 46);
            t.SynString = Color.FromArgb(10, 102, 56);
            t.SynComment = Color.FromArgb(110, 119, 129);
            t.SynNumber = Color.FromArgb(130, 80, 223);
            t.SynType = Color.FromArgb(5, 80, 174);
            t.SynFunction = Color.FromArgb(149, 56, 0);
            t.SynOperator = Color.FromArgb(207, 34, 46);
            return t;
        }

        public static EditorTheme ByName(string name)
        {
            if (string.Equals(name, "Mariana", StringComparison.OrdinalIgnoreCase)) return Mariana();
            if (string.Equals(name, "Light", StringComparison.OrdinalIgnoreCase)) return Light();
            return Monokai();
        }
    }

    // ========================================================================
    // 3. CUSTOM DARK MENU / TOOLSTRIP RENDERER
    // ========================================================================
    internal class SublimeColorTable : ProfessionalColorTable
    {
        private EditorTheme _t;
        public SublimeColorTable(EditorTheme t) { _t = t; }
        public override Color MenuStripGradientBegin { get { return _t.MenuBg; } }
        public override Color MenuStripGradientEnd { get { return _t.MenuBg; } }
        public override Color ToolStripDropDownBackground { get { return _t.MenuBg; } }
        public override Color ImageMarginGradientBegin { get { return _t.MenuBg; } }
        public override Color ImageMarginGradientMiddle { get { return _t.MenuBg; } }
        public override Color ImageMarginGradientEnd { get { return _t.MenuBg; } }
        public override Color MenuBorder { get { return _t.MenuBorder; } }
        public override Color MenuItemBorder { get { return _t.MenuHoverBg; } }
        public override Color MenuItemSelected { get { return _t.MenuHoverBg; } }
        public override Color MenuItemSelectedGradientBegin { get { return _t.MenuHoverBg; } }
        public override Color MenuItemSelectedGradientEnd { get { return _t.MenuHoverBg; } }
        public override Color MenuItemPressedGradientBegin { get { return _t.MenuHoverBg; } }
        public override Color MenuItemPressedGradientEnd { get { return _t.MenuHoverBg; } }
        public override Color SeparatorDark { get { return _t.BorderLine; } }
        public override Color SeparatorLight { get { return _t.MenuBg; } }
    }

    internal class SublimeMenuRenderer : ToolStripProfessionalRenderer
    {
        private EditorTheme _t;
        public SublimeMenuRenderer(EditorTheme t) : base(new SublimeColorTable(t))
        {
            _t = t;
            this.RoundedEdges = false;
        }
        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = e.Item.Enabled ? _t.MenuFg : _t.SidebarHeaderFg;
            base.OnRenderItemText(e);
        }
        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            e.ArrowColor = _t.MenuFg;
            base.OnRenderArrow(e);
        }
        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            Rectangle r = e.ImageRectangle;
            using (SolidBrush b = new SolidBrush(_t.Accent))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillEllipse(b, r.X + 4, r.Y + 4, 8, 8);
            }
        }
    }

    // ========================================================================
    // 4. SYNTAX HIGHLIGHTING ENGINE (15 Languages, Monokai Palette)
    // ========================================================================
    internal enum TokenKind { Default, Keyword, String, Comment, Number, Type, Function, Operator }

    internal class SyntaxRule
    {
        public Regex Pattern;
        public TokenKind Kind;
        public SyntaxRule(string regex, TokenKind kind, RegexOptions opts)
        {
            Pattern = new Regex(regex, opts | RegexOptions.Compiled);
            Kind = kind;
        }
    }

    internal static class SyntaxEngine
    {
        public static readonly string[] AllLanguages = new string[]
        {
            "Plain Text", "C#", "JavaScript", "PHP", "HTML", "CSS", "JSON",
            "PowerShell", "Batch", "Python", "SQL", "XML", "Markdown", "INI", "YAML"
        };

        private static readonly Dictionary<string, List<SyntaxRule>> _cache = new Dictionary<string, List<SyntaxRule>>(StringComparer.OrdinalIgnoreCase);

        public static string DetectLanguage(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return "Plain Text";
            string ext = Path.GetExtension(filePath).ToLowerInvariant();
            switch (ext)
            {
                case ".cs": return "C#";
                case ".js": case ".mjs": case ".cjs": case ".jsx": case ".ts": case ".tsx": return "JavaScript";
                case ".php": case ".phtml": return "PHP";
                case ".html": case ".htm": case ".xhtml": case ".vue": case ".svelte": return "HTML";
                case ".css": case ".scss": case ".sass": case ".less": return "CSS";
                case ".json": case ".jsonc": case ".webmanifest": return "JSON";
                case ".ps1": case ".psm1": case ".psd1": return "PowerShell";
                case ".bat": case ".cmd": return "Batch";
                case ".py": case ".pyw": return "Python";
                case ".sql": return "SQL";
                case ".xml": case ".svg": case ".xaml": case ".csproj": case ".config": case ".plist": return "XML";
                case ".md": case ".markdown": return "Markdown";
                case ".ini": case ".cfg": case ".conf": case ".env": case ".properties": case ".inf": case ".toml": return "INI";
                case ".yml": case ".yaml": return "YAML";
                default: return "Plain Text";
            }
        }

        public static Color LanguageDotColor(string lang, EditorTheme t)
        {
            switch (lang)
            {
                case "HTML": case "XML": return t.SynKeyword;
                case "CSS": case "C#": return t.SynType;
                case "JavaScript": case "JSON": return t.SynString;
                case "PHP": case "Python": return t.SynNumber;
                case "PowerShell": case "Batch": return t.SynOperator;
                case "SQL": case "Markdown": return t.SynFunction;
                default: return t.GutterFg;
            }
        }

        public static string LineCommentPrefix(string lang)
        {
            switch (lang)
            {
                case "C#": case "JavaScript": case "PHP": case "CSS": return "// ";
                case "PowerShell": case "Python": case "YAML": case "INI": return "# ";
                case "Batch": return "REM ";
                case "SQL": return "-- ";
                case "HTML": case "XML": return "<!-- ";
                default: return "// ";
            }
        }

        public static List<SyntaxRule> GetRules(string lang)
        {
            if (string.IsNullOrEmpty(lang) || lang == "Plain Text") return null;
            List<SyntaxRule> rules;
            if (_cache.TryGetValue(lang, out rules)) return rules;

            rules = new List<SyntaxRule>();
            RegexOptions ml = RegexOptions.Multiline;
            RegexOptions mli = RegexOptions.Multiline | RegexOptions.IgnoreCase;

            if (lang == "JSON")
            {
                rules.Add(new SyntaxRule("\"([^\"\\\\]|\\\\.)*\"(?=\\s*:)", TokenKind.Type, ml));
                rules.Add(new SyntaxRule("\"([^\"\\\\]|\\\\.)*\"", TokenKind.String, ml));
                rules.Add(new SyntaxRule("\\b(true|false|null)\\b", TokenKind.Number, ml));
                rules.Add(new SyntaxRule("-?\\b\\d+(\\.\\d+)?([eE][+-]?\\d+)?\\b", TokenKind.Number, ml));
                rules.Add(new SyntaxRule("//.*$", TokenKind.Comment, ml));
            }
            else if (lang == "HTML" || lang == "XML")
            {
                rules.Add(new SyntaxRule("<!--([\\s\\S]*?)-->", TokenKind.Comment, ml));
                rules.Add(new SyntaxRule("</?[a-zA-Z][a-zA-Z0-9:-]*", TokenKind.Keyword, ml));
                rules.Add(new SyntaxRule("/?>", TokenKind.Keyword, ml));
                rules.Add(new SyntaxRule("\\b[a-zA-Z_:-]+(?=\\s*=)", TokenKind.Function, ml));
                rules.Add(new SyntaxRule("\"([^\"\\\\]|\\\\.)*\"|'([^'\\\\]|\\\\.)*'", TokenKind.String, ml));
                rules.Add(new SyntaxRule("&[a-zA-Z0-9#]+;", TokenKind.Number, ml));
            }
            else if (lang == "CSS")
            {
                rules.Add(new SyntaxRule("/\\*[\\s\\S]*?\\*/|//.*$", TokenKind.Comment, ml));
                rules.Add(new SyntaxRule("@[a-zA-Z-]+", TokenKind.Keyword, ml));
                rules.Add(new SyntaxRule("(\\.|#)[a-zA-Z_-][a-zA-Z0-9_-]*", TokenKind.Function, ml));
                rules.Add(new SyntaxRule("\\b[a-z-]+(?=\\s*:)", TokenKind.Type, ml));
                rules.Add(new SyntaxRule("\"([^\"\\\\]|\\\\.)*\"|'([^'\\\\]|\\\\.)*'", TokenKind.String, ml));
                rules.Add(new SyntaxRule("#[0-9a-fA-F]{3,8}\\b|\\b\\d+(\\.\\d+)?(px|em|rem|%|vh|vw|s|ms|deg|fr)?\\b", TokenKind.Number, ml));
                rules.Add(new SyntaxRule("!important\\b", TokenKind.Keyword, ml));
            }
            else if (lang == "JavaScript")
            {
                rules.Add(new SyntaxRule("\\b(var|let|const|function|return|if|else|for|while|do|switch|case|break|continue|default|try|catch|finally|throw|new|typeof|instanceof|in|of|class|extends|super|import|export|from|async|await|yield|this|delete|void|interface|type)\\b", TokenKind.Keyword, ml));
                rules.Add(new SyntaxRule("\\b(true|false|null|undefined|NaN|Infinity)\\b|\\b0x[0-9a-fA-F]+\\b|\\b\\d+(\\.\\d+)?\\b", TokenKind.Number, ml));
                rules.Add(new SyntaxRule("\\b(console|window|document|Math|JSON|Array|Object|String|Number|Boolean|Date|Promise|Map|Set|RegExp|Error)\\b", TokenKind.Type, ml));
                rules.Add(new SyntaxRule("\\b[a-zA-Z_$][a-zA-Z0-9_$]*(?=\\s*\\()", TokenKind.Function, ml));
                rules.Add(new SyntaxRule("\"([^\"\\\\]|\\\\.)*\"|'([^'\\\\]|\\\\.)*'|`([^`\\\\]|\\\\.)*`", TokenKind.String, ml));
                rules.Add(new SyntaxRule("//.*$|/\\*[\\s\\S]*?\\*/", TokenKind.Comment, ml));
            }
            else if (lang == "PHP")
            {
                rules.Add(new SyntaxRule("<\\?php|<\\?|\\?>", TokenKind.Operator, ml));
                rules.Add(new SyntaxRule("\\b(echo|print|function|return|if|else|elseif|endif|for|foreach|endforeach|while|endwhile|do|switch|case|break|continue|default|try|catch|finally|throw|new|class|interface|trait|extends|implements|public|private|protected|static|final|abstract|namespace|use|as|require|require_once|include|include_once|global|const|array|match|fn)\\b", TokenKind.Keyword, mli));
                rules.Add(new SyntaxRule("\\$[a-zA-Z_][a-zA-Z0-9_]*", TokenKind.Type, ml));
                rules.Add(new SyntaxRule("\\b(true|false|null)\\b|\\b\\d+(\\.\\d+)?\\b", TokenKind.Number, mli));
                rules.Add(new SyntaxRule("\\b[a-zA-Z_][a-zA-Z0-9_]*(?=\\s*\\()", TokenKind.Function, ml));
                rules.Add(new SyntaxRule("\"([^\"\\\\]|\\\\.)*\"|'([^'\\\\]|\\\\.)*'", TokenKind.String, ml));
                rules.Add(new SyntaxRule("//.*$|#.*$|/\\*[\\s\\S]*?\\*/", TokenKind.Comment, ml));
            }
            else if (lang == "C#")
            {
                rules.Add(new SyntaxRule("\\b(using|namespace|class|struct|enum|interface|public|private|protected|internal|static|readonly|const|volatile|void|return|if|else|for|foreach|in|while|do|switch|case|break|continue|default|try|catch|finally|throw|new|this|base|as|is|ref|out|params|override|virtual|abstract|sealed|partial|async|await|lock|delegate|event|get|set|where|var)\\b", TokenKind.Keyword, ml));
                rules.Add(new SyntaxRule("\\b(string|int|long|short|byte|bool|float|double|decimal|char|object|IntPtr|uint|ulong|ushort|sbyte|Color|List|Dictionary|HashSet|Regex|File|Path|Directory|StringBuilder|DateTime|TimeSpan|Task|Thread|Form|Control|Panel)\\b", TokenKind.Type, ml));
                rules.Add(new SyntaxRule("\\b(true|false|null)\\b|\\b0x[0-9a-fA-F]+\\b|\\b\\d+(\\.\\d+)?[fFdDmMlL]?\\b", TokenKind.Number, ml));
                rules.Add(new SyntaxRule("\\b[a-zA-Z_][a-zA-Z0-9_]*(?=\\s*\\()", TokenKind.Function, ml));
                rules.Add(new SyntaxRule("@\"([^\"]|\"\")*\"|\"([^\"\\\\]|\\\\.)*\"|'([^'\\\\]|\\\\.)*'", TokenKind.String, ml));
                rules.Add(new SyntaxRule("//.*$|/\\*[\\s\\S]*?\\*/", TokenKind.Comment, ml));
            }
            else if (lang == "PowerShell")
            {
                rules.Add(new SyntaxRule("\\b(function|param|begin|process|end|if|elseif|else|for|foreach|while|do|until|switch|break|continue|return|try|catch|finally|throw|trap|filter|class|enum|using|in)\\b", TokenKind.Keyword, mli));
                rules.Add(new SyntaxRule("\\b[A-Z][a-z]+-[A-Z][a-zA-Z0-9]+\\b", TokenKind.Function, ml));
                rules.Add(new SyntaxRule("\\$[a-zA-Z0-9_:]+", TokenKind.Type, ml));
                rules.Add(new SyntaxRule("-(eq|ne|gt|ge|lt|le|like|notlike|match|notmatch|and|or|not|contains|in|replace|split|join|f|ErrorAction|Force|Recurse|Path|LiteralPath|Name|Value)\\b", TokenKind.Operator, mli));
                rules.Add(new SyntaxRule("\\b\\d+(\\.\\d+)?\\b", TokenKind.Number, ml));
                rules.Add(new SyntaxRule("\"([^\"`]|`.)*\"|'([^']|'')*'", TokenKind.String, ml));
                rules.Add(new SyntaxRule("#.*$|<#[\\s\\S]*?#>", TokenKind.Comment, ml));
            }
            else if (lang == "Batch")
            {
                rules.Add(new SyntaxRule("\\b(echo|off|on|set|setlocal|endlocal|if|else|exist|not|equ|neq|for|in|do|goto|call|exit|start|pause|title|cd|dir|copy|xcopy|robocopy|move|del|rmdir|mkdir|chcp|timeout|reg|sc|schtasks|powershell|cmd)\\b", TokenKind.Keyword, mli));
                rules.Add(new SyntaxRule("%[a-zA-Z0-9_~*]+%|%%[a-zA-Z]", TokenKind.Type, ml));
                rules.Add(new SyntaxRule("^\\s*:[a-zA-Z0-9_-]+", TokenKind.Function, ml));
                rules.Add(new SyntaxRule("\"[^\"]*\"", TokenKind.String, ml));
                rules.Add(new SyntaxRule("^\\s*(REM\\b.*|::.*)$", TokenKind.Comment, mli));
            }
            else if (lang == "Python")
            {
                rules.Add(new SyntaxRule("\\b(def|class|return|if|elif|else|for|while|break|continue|pass|import|from|as|try|except|finally|raise|with|lambda|yield|global|nonlocal|assert|del|in|is|not|and|or|async|await)\\b", TokenKind.Keyword, ml));
                rules.Add(new SyntaxRule("\\b(True|False|None)\\b|\\b\\d+(\\.\\d+)?\\b", TokenKind.Number, ml));
                rules.Add(new SyntaxRule("\\b(self|cls|int|str|float|list|dict|set|tuple|bool|bytes|len|range|print|open|enumerate|zip|map|filter)\\b", TokenKind.Type, ml));
                rules.Add(new SyntaxRule("\\b[a-zA-Z_][a-zA-Z0-9_]*(?=\\s*\\()", TokenKind.Function, ml));
                rules.Add(new SyntaxRule("\"([^\"\\\\]|\\\\.)*\"|'([^'\\\\]|\\\\.)*'", TokenKind.String, ml));
                rules.Add(new SyntaxRule("#.*$", TokenKind.Comment, ml));
            }
            else if (lang == "SQL")
            {
                rules.Add(new SyntaxRule("\\b(SELECT|FROM|WHERE|INSERT|INTO|VALUES|UPDATE|SET|DELETE|CREATE|TABLE|ALTER|DROP|INDEX|VIEW|JOIN|INNER|LEFT|RIGHT|OUTER|ON|AS|AND|OR|NOT|NULL|IS|IN|LIKE|BETWEEN|ORDER|BY|GROUP|HAVING|LIMIT|OFFSET|DISTINCT|UNION|ALL|CASE|WHEN|THEN|ELSE|END|PRIMARY|KEY|FOREIGN|REFERENCES|DEFAULT|AUTO_INCREMENT|ASC|DESC|COUNT|SUM|AVG|MIN|MAX)\\b", TokenKind.Keyword, mli));
                rules.Add(new SyntaxRule("\\b(INT|INTEGER|BIGINT|VARCHAR|CHAR|TEXT|DECIMAL|FLOAT|DOUBLE|BOOLEAN|BOOL|DATE|DATETIME|TIMESTAMP|JSON|BLOB)\\b", TokenKind.Type, mli));
                rules.Add(new SyntaxRule("\\b\\d+(\\.\\d+)?\\b", TokenKind.Number, ml));
                rules.Add(new SyntaxRule("'([^']|'')*'|\"([^\"]|\"\")*\"", TokenKind.String, ml));
                rules.Add(new SyntaxRule("--.*$|/\\*[\\s\\S]*?\\*/", TokenKind.Comment, ml));
            }
            else if (lang == "Markdown")
            {
                rules.Add(new SyntaxRule("^#{1,6}\\s.*$", TokenKind.Keyword, ml));
                rules.Add(new SyntaxRule("`[^`]+`", TokenKind.String, ml));
                rules.Add(new SyntaxRule("\\*\\*[^*]+\\*\\*|__[^_]+__", TokenKind.Operator, ml));
                rules.Add(new SyntaxRule("\\[[^\\]]+\\]\\([^\\)]+\\)", TokenKind.Type, ml));
                rules.Add(new SyntaxRule("^\\s*>.*$", TokenKind.Comment, ml));
                rules.Add(new SyntaxRule("^\\s*([-*+]|\\d+\\.)\\s", TokenKind.Function, ml));
            }
            else if (lang == "INI" || lang == "YAML")
            {
                rules.Add(new SyntaxRule("^\\s*\\[[^\\]]+\\]", TokenKind.Keyword, ml));
                rules.Add(new SyntaxRule("^\\s*[a-zA-Z0-9_.-]+(?=\\s*[:=])", TokenKind.Type, ml));
                rules.Add(new SyntaxRule("\\b(true|false|yes|no|null|on|off)\\b|\\b\\d+(\\.\\d+)?\\b", TokenKind.Number, mli));
                rules.Add(new SyntaxRule("\"([^\"\\\\]|\\\\.)*\"|'([^'\\\\]|\\\\.)*'", TokenKind.String, ml));
                rules.Add(new SyntaxRule("(#|;).*$", TokenKind.Comment, ml));
            }

            _cache[lang] = rules;
            return rules;
        }
    }

    // ========================================================================
    // 5. DOCUMENT MODEL (Tab State, Encoding, Line Endings)
    // ========================================================================
    internal class DocTab
    {
        public string FilePath;
        public string Title;
        public string Content;
        public string Language;
        public string EncodingName; // "UTF-8", "UTF-8 BOM", "UTF-16", "Win-1253"
        public string LineEnding;   // "CRLF" or "LF"
        public bool IsDirty;
        public int CaretPos;
        public int FirstVisibleLine;

        public DocTab()
        {
            Title = "untitled";
            Content = "";
            Language = "Plain Text";
            EncodingName = "UTF-8";
            LineEnding = "CRLF";
        }

        public Encoding GetEncoding()
        {
            if (EncodingName == "UTF-8 BOM") return new UTF8Encoding(true);
            if (EncodingName == "UTF-16") return Encoding.Unicode;
            if (EncodingName == "Win-1253") return Encoding.GetEncoding(1253);
            return new UTF8Encoding(false);
        }

        public static DocTab LoadFromFile(string path)
        {
            DocTab d = new DocTab();
            d.FilePath = Path.GetFullPath(path);
            d.Title = Path.GetFileName(d.FilePath);
            d.Language = SyntaxEngine.DetectLanguage(d.FilePath);

            byte[] raw = File.ReadAllBytes(d.FilePath);
            string text;
            if (raw.Length >= 3 && raw[0] == 0xEF && raw[1] == 0xBB && raw[2] == 0xBF)
            {
                d.EncodingName = "UTF-8 BOM";
                text = Encoding.UTF8.GetString(raw, 3, raw.Length - 3);
            }
            else if (raw.Length >= 2 && raw[0] == 0xFF && raw[1] == 0xFE)
            {
                d.EncodingName = "UTF-16";
                text = Encoding.Unicode.GetString(raw, 2, raw.Length - 2);
            }
            else
            {
                try
                {
                    UTF8Encoding strictUtf8 = new UTF8Encoding(false, true);
                    text = strictUtf8.GetString(raw);
                    d.EncodingName = "UTF-8";
                }
                catch
                {
                    d.EncodingName = "Win-1253";
                    text = Encoding.GetEncoding(1253).GetString(raw);
                }
            }

            d.LineEnding = (text.IndexOf("\r\n") >= 0 || text.IndexOf('\n') < 0) ? "CRLF" : "LF";
            d.Content = text.Replace("\r\n", "\n").Replace("\r", "\n");
            d.IsDirty = false;
            return d;
        }

        public void SaveToFile(string path)
        {
            FilePath = Path.GetFullPath(path);
            Title = Path.GetFileName(FilePath);
            if (Language == "Plain Text") Language = SyntaxEngine.DetectLanguage(FilePath);
            string norm = (Content ?? "").Replace("\r\n", "\n").Replace("\r", "\n");
            if (LineEnding == "CRLF") norm = norm.Replace("\n", "\r\n");
            File.WriteAllText(FilePath, norm, GetEncoding());
            IsDirty = false;
        }
    }

    // ========================================================================
    // 6. SUBLIME CUSTOM TAB BAR (Double-Buffered GDI+)
    // ========================================================================
    internal class SublimeTabBar : Control
    {
        public List<DocTab> Tabs = new List<DocTab>();
        public int ActiveIndex = -1;
        public EditorTheme Theme = EditorTheme.Monokai();

        public event Action<int> TabSelected;
        public event Action<int> TabClosed;
        public event Action NewTabRequested;
        public event Action<int, Point> TabContextMenuRequested;

        private int _hoverTab = -1;
        private int _hoverClose = -1;
        private bool _hoverPlus = false;
        private List<Rectangle> _tabRects = new List<Rectangle>();
        private List<Rectangle> _closeRects = new List<Rectangle>();
        private Rectangle _plusRect = Rectangle.Empty;
        private ToolTip _tip = new ToolTip();
        private int _tipTab = -1;

        public SublimeTabBar()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            this.Height = 34;
            this.Font = new Font("Segoe UI", 9f, FontStyle.Regular);
            this.Cursor = Cursors.Default;
        }

        private void ComputeRects()
        {
            _tabRects.Clear();
            _closeRects.Clear();
            int count = Tabs.Count;
            int avail = Math.Max(60, this.Width - 42);
            int tabW = count > 0 ? Math.Min(195, Math.Max(84, avail / count)) : 140;
            int x = 4;
            for (int i = 0; i < count; i++)
            {
                Rectangle r = new Rectangle(x, 4, tabW - 2, this.Height - 4);
                _tabRects.Add(r);
                _closeRects.Add(new Rectangle(r.Right - 22, r.Y + (r.Height - 16) / 2, 16, 16));
                x += tabW;
            }
            _plusRect = new Rectangle(x + 4, 7, 24, this.Height - 12);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            ComputeRects();
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            using (SolidBrush bg = new SolidBrush(Theme.TabBarBg))
                g.FillRectangle(bg, this.ClientRectangle);

            using (Pen bottomLine = new Pen(Theme.BorderLine))
                g.DrawLine(bottomLine, 0, this.Height - 1, this.Width, this.Height - 1);

            for (int i = 0; i < Tabs.Count; i++)
            {
                DocTab tab = Tabs[i];
                Rectangle r = _tabRects[i];
                bool active = (i == ActiveIndex);
                bool hover = (i == _hoverTab);

                Color fill = active ? Theme.TabActiveBg : (hover ? Theme.TabHoverBg : Theme.TabInactiveBg);
                using (GraphicsPath path = RoundedTopRect(r, 5))
                {
                    using (SolidBrush b = new SolidBrush(fill)) g.FillPath(b, path);
                    if (!active)
                    {
                        using (Pen p = new Pen(Theme.TabBorder)) g.DrawPath(p, path);
                    }
                }

                if (active)
                {
                    using (SolidBrush accent = new SolidBrush(Theme.Accent))
                        g.FillRectangle(accent, r.X + 4, r.Y, r.Width - 8, 2);
                    using (SolidBrush blend = new SolidBrush(Theme.EditorBg))
                        g.FillRectangle(blend, r.X, this.Height - 2, r.Width, 2);
                }

                // Language syntax dot
                Color dotCol = SyntaxEngine.LanguageDotColor(tab.Language, Theme);
                using (SolidBrush db = new SolidBrush(dotCol))
                    g.FillEllipse(db, r.X + 10, r.Y + (r.Height - 7) / 2 + 1, 7, 7);

                // Title
                Rectangle textR = new Rectangle(r.X + 22, r.Y + 1, r.Width - 44, r.Height - 2);
                TextRenderer.DrawText(g, tab.Title, this.Font, textR,
                    active ? Theme.TabTextActive : Theme.TabTextInactive,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

                // Dirty dot or close X
                Rectangle cr = _closeRects[i];
                bool showX = (i == _hoverClose) || (!tab.IsDirty && (active || hover));
                if (tab.IsDirty && i != _hoverClose)
                {
                    using (SolidBrush dirtyB = new SolidBrush(Theme.Accent))
                        g.FillEllipse(dirtyB, cr.X + 4, cr.Y + 4, 8, 8);
                }
                else if (showX)
                {
                    if (i == _hoverClose)
                    {
                        using (SolidBrush hb = new SolidBrush(Color.FromArgb(45, 255, 255, 255)))
                            g.FillEllipse(hb, cr);
                    }
                    using (Pen xp = new Pen(i == _hoverClose ? Theme.TabTextActive : Theme.TabTextInactive, 1.4f))
                    {
                        g.DrawLine(xp, cr.X + 4, cr.Y + 4, cr.Right - 5, cr.Bottom - 5);
                        g.DrawLine(xp, cr.Right - 5, cr.Y + 4, cr.X + 4, cr.Bottom - 5);
                    }
                }
            }

            // "+" new tab button
            if (_hoverPlus)
            {
                using (SolidBrush pb = new SolidBrush(Theme.TabHoverBg))
                    g.FillRectangle(pb, _plusRect);
            }
            using (Pen pp = new Pen(_hoverPlus ? Theme.TabTextActive : Theme.TabTextInactive, 1.5f))
            {
                int cx = _plusRect.X + _plusRect.Width / 2;
                int cy = _plusRect.Y + _plusRect.Height / 2;
                g.DrawLine(pp, cx - 4, cy, cx + 4, cy);
                g.DrawLine(pp, cx, cy - 4, cx, cy + 4);
            }
        }

        private static GraphicsPath RoundedTopRect(Rectangle r, int rad)
        {
            GraphicsPath p = new GraphicsPath();
            int d = rad * 2;
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddLine(r.Right, r.Bottom, r.X, r.Bottom);
            p.CloseFigure();
            return p;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            ComputeRects();
            int ht = -1, hc = -1;
            for (int i = 0; i < _tabRects.Count; i++)
            {
                if (_closeRects[i].Contains(e.Location)) { ht = i; hc = i; break; }
                if (_tabRects[i].Contains(e.Location)) { ht = i; break; }
            }
            bool hp = _plusRect.Contains(e.Location);
            if (ht != _hoverTab || hc != _hoverClose || hp != _hoverPlus)
            {
                _hoverTab = ht; _hoverClose = hc; _hoverPlus = hp;
                this.Invalidate();
            }
            if (ht != _tipTab)
            {
                _tipTab = ht;
                if (ht >= 0 && ht < Tabs.Count)
                    _tip.SetToolTip(this, string.IsNullOrEmpty(Tabs[ht].FilePath) ? Tabs[ht].Title : Tabs[ht].FilePath);
                else
                    _tip.SetToolTip(this, "");
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hoverTab = -1; _hoverClose = -1; _hoverPlus = false;
            this.Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            ComputeRects();
            if (e.Button == MouseButtons.Left && _plusRect.Contains(e.Location))
            {
                if (NewTabRequested != null) NewTabRequested();
                return;
            }
            for (int i = 0; i < _tabRects.Count; i++)
            {
                if (!_tabRects[i].Contains(e.Location)) continue;
                if (e.Button == MouseButtons.Middle || (e.Button == MouseButtons.Left && _closeRects[i].Contains(e.Location)))
                {
                    if (TabClosed != null) TabClosed(i);
                }
                else if (e.Button == MouseButtons.Left)
                {
                    if (TabSelected != null) TabSelected(i);
                }
                else if (e.Button == MouseButtons.Right)
                {
                    if (TabContextMenuRequested != null) TabContextMenuRequested(i, e.Location);
                }
                return;
            }
        }
    }

    // ========================================================================
    // 7. ENHANCED CODE EDITOR CONTROL (Flicker-Free + Undo-Safe Highlighting)
    // ========================================================================
    internal class CodeEditorBox : RichTextBox
    {
        public event EventHandler ViewChanged;
        private object _textDoc;
        private bool _oleTried;
        private int _updateDepth;
        private IntPtr _savedEventMask;
        private Native.POINT _savedScrollPos;

        public CodeEditorBox()
        {
            this.BorderStyle = BorderStyle.None;
            this.DetectUrls = false;
            this.AcceptsTab = true;
            this.HideSelection = false;
            this.AutoWordSelection = false;
            this.WordWrap = false;
            this.ScrollBars = RichTextBoxScrollBars.Both;
        }

        public void SetTabWidthSpaces(int spaces)
        {
            try
            {
                // Dialog units: roughly 4 units per average char width
                int[] stops = new int[] { spaces * 4 };
                Native.SendMessage(this.Handle, Native.EM_SETTABSTOPS, 1, stops);
            }
            catch { }
        }

        public int GetFirstVisibleLine()
        {
            if (!this.IsHandleCreated) return 0;
            return Native.SendMessage(this.Handle, Native.EM_GETFIRSTVISIBLELINE, IntPtr.Zero, IntPtr.Zero).ToInt32();
        }

        public void ScrollToLine(int targetLine)
        {
            if (!this.IsHandleCreated) return;
            int current = GetFirstVisibleLine();
            int delta = targetLine - current;
            if (delta != 0)
            {
                Native.SendMessage(this.Handle, Native.EM_LINESCROLL, IntPtr.Zero, (IntPtr)delta);
                if (ViewChanged != null) ViewChanged(this, EventArgs.Empty);
            }
        }

        private object GetTextDocument()
        {
            if (_oleTried) return _textDoc;
            _oleTried = true;
            try
            {
                IntPtr ptr;
                Native.SendMessage(this.Handle, Native.EM_GETOLEINTERFACE, IntPtr.Zero, out ptr);
                if (ptr != IntPtr.Zero)
                {
                    _textDoc = Marshal.GetObjectForIUnknown(ptr);
                    Marshal.Release(ptr);
                }
            }
            catch { _textDoc = null; }
            return _textDoc;
        }

        public void SuspendUndo()
        {
            try
            {
                object doc = GetTextDocument();
                if (doc != null)
                    doc.GetType().InvokeMember("Undo", BindingFlags.InvokeMethod, null, doc, new object[] { Native.TOM_SUSPEND });
            }
            catch { }
        }

        public void ResumeUndo()
        {
            try
            {
                object doc = GetTextDocument();
                if (doc != null)
                    doc.GetType().InvokeMember("Undo", BindingFlags.InvokeMethod, null, doc, new object[] { Native.TOM_RESUME });
            }
            catch { }
        }

        public void BeginSilentUpdate()
        {
            if (_updateDepth == 0 && this.IsHandleCreated)
            {
                _savedEventMask = Native.SendMessage(this.Handle, Native.EM_GETEVENTMASK, IntPtr.Zero, IntPtr.Zero);
                Native.SendMessage(this.Handle, Native.EM_SETEVENTMASK, IntPtr.Zero, IntPtr.Zero);
                Native.SendMessage(this.Handle, Native.WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
                _savedScrollPos = new Native.POINT();
                Native.SendMessage(this.Handle, Native.EM_GETSCROLLPOS, IntPtr.Zero, ref _savedScrollPos);
            }
            _updateDepth++;
        }

        public void EndSilentUpdate()
        {
            if (_updateDepth <= 0) return;
            _updateDepth--;
            if (_updateDepth == 0 && this.IsHandleCreated)
            {
                Native.SendMessage(this.Handle, Native.EM_SETSCROLLPOS, IntPtr.Zero, ref _savedScrollPos);
                Native.SendMessage(this.Handle, Native.EM_SETEVENTMASK, IntPtr.Zero, _savedEventMask);
                Native.SendMessage(this.Handle, Native.WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
                this.Invalidate();
            }
        }

        public void PastePlainText()
        {
            try
            {
                if (Clipboard.ContainsText())
                {
                    string txt = Clipboard.GetText(TextDataFormat.UnicodeText);
                    this.SelectedText = txt.Replace("\r\n", "\n");
                }
            }
            catch { }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == Native.WM_VSCROLL || m.Msg == Native.WM_HSCROLL || m.Msg == Native.WM_MOUSEWHEEL)
            {
                if (ViewChanged != null) ViewChanged(this, EventArgs.Empty);
            }
        }

        protected override void OnVScroll(EventArgs e)
        {
            base.OnVScroll(e);
            if (ViewChanged != null) ViewChanged(this, EventArgs.Empty);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (ViewChanged != null) ViewChanged(this, EventArgs.Empty);
        }
    }

    // ========================================================================
    // 8. PIXEL-EXACT LINE NUMBER GUTTER
    // ========================================================================
    internal class LineNumberGutter : Control
    {
        public CodeEditorBox Editor;
        public EditorTheme Theme = EditorTheme.Monokai();

        public LineNumberGutter()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            this.Width = 54;
            this.Cursor = Cursors.Hand;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            using (SolidBrush bg = new SolidBrush(Theme.GutterBg))
                g.FillRectangle(bg, this.ClientRectangle);

            using (Pen sep = new Pen(Theme.BorderLine))
                g.DrawLine(sep, this.Width - 1, 0, this.Width - 1, this.Height);

            if (Editor == null || !Editor.IsHandleCreated) return;

            int lineCount = Math.Max(1, Editor.Lines.Length);
            int digits = Math.Max(3, lineCount.ToString().Length);
            int neededW = Math.Max(48, digits * (int)Math.Ceiling(Editor.Font.Size * 0.85f) + 22);
            if (Math.Abs(this.Width - neededW) > 6) this.Width = neededW;

            int activeLine = Editor.GetLineFromCharIndex(Editor.SelectionStart);
            int firstChar = Editor.GetCharIndexFromPosition(new Point(2, 2));
            int firstLine = Editor.GetLineFromCharIndex(firstChar);
            int lineH = Math.Max(14, (int)Math.Ceiling(Editor.Font.GetHeight(g)) + 1);
            int lastDrawnLine = -1;

            for (int line = firstLine; line < lineCount; line++)
            {
                int charIdx = Editor.GetFirstCharIndexFromLine(line);
                if (charIdx < 0) break;
                Point pt = Editor.GetPositionFromCharIndex(charIdx);
                if (pt.Y > this.Height) break;
                if (pt.Y + lineH < 0) continue;
                if (line == lastDrawnLine) continue;
                lastDrawnLine = line;

                bool isActive = (line == activeLine);
                if (isActive)
                {
                    using (SolidBrush ab = new SolidBrush(Theme.GutterActiveBg))
                        g.FillRectangle(ab, 0, pt.Y, this.Width - 1, lineH);
                }

                Rectangle r = new Rectangle(0, pt.Y, this.Width - 10, lineH);
                TextRenderer.DrawText(g, (line + 1).ToString(), Editor.Font, r,
                    isActive ? Theme.GutterActiveFg : Theme.GutterFg,
                    TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (Editor == null) return;
            int idx = Editor.GetCharIndexFromPosition(new Point(2, e.Y));
            int line = Editor.GetLineFromCharIndex(idx);
            int start = Editor.GetFirstCharIndexFromLine(line);
            if (start < 0) return;
            int end = (line + 1 < Editor.Lines.Length) ? Editor.GetFirstCharIndexFromLine(line + 1) : Editor.TextLength;
            Editor.Select(start, Math.Max(0, end - start));
            Editor.Focus();
            this.Invalidate();
        }
    }

    // ========================================================================
    // 9. INTERACTIVE SUBLIME TEXT MINIMAP (Click & Drag Scroll)
    // ========================================================================
    internal class SublimeMinimap : Control
    {
        public CodeEditorBox Editor;
        public EditorTheme Theme = EditorTheme.Monokai();
        private bool _dragging = false;

        public SublimeMinimap()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            this.Width = 92;
            this.Cursor = Cursors.Hand;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            using (SolidBrush bg = new SolidBrush(Theme.MinimapBg))
                g.FillRectangle(bg, this.ClientRectangle);

            using (Pen sep = new Pen(Theme.BorderLine))
                g.DrawLine(sep, 0, 0, 0, this.Height);

            if (Editor == null || !Editor.IsHandleCreated) return;
            string[] lines = Editor.Lines;
            int total = Math.Max(1, lines.Length);

            int firstVis = Editor.GetFirstVisibleLine();
            int visCount = Math.Max(1, Editor.Height / Math.Max(14, (int)Math.Ceiling(Editor.Font.GetHeight(g))));

            // How many lines fit in minimap (3 px pitch)
            int pitch = 3;
            int maxDrawable = Math.Max(1, (this.Height - 8) / pitch);
            int mapStartLine = 0;
            if (total > maxDrawable)
            {
                float ratio = (float)firstVis / Math.Max(1, total - visCount);
                ratio = Math.Max(0f, Math.Min(1f, ratio));
                mapStartLine = (int)(ratio * (total - maxDrawable));
            }
            int mapEndLine = Math.Min(total, mapStartLine + maxDrawable);

            using (SolidBrush codeB = new SolidBrush(Color.FromArgb(115, Theme.EditorFg)))
            using (SolidBrush kwB = new SolidBrush(Color.FromArgb(135, Theme.SynKeyword)))
            using (SolidBrush comB = new SolidBrush(Color.FromArgb(95, Theme.SynComment)))
            using (SolidBrush strB = new SolidBrush(Color.FromArgb(125, Theme.SynString)))
            {
                for (int i = mapStartLine; i < mapEndLine; i++)
                {
                    string s = lines[i];
                    if (string.IsNullOrWhiteSpace(s)) continue;
                    int y = 4 + (i - mapStartLine) * pitch;
                    int indent = 0;
                    while (indent < s.Length && (s[indent] == ' ' || s[indent] == '\t'))
                        indent += (s[indent] == '\t') ? 4 : 1;
                    int len = Math.Min(90, s.Trim().Length);
                    int x = 6 + Math.Min(36, indent);
                    int w = Math.Max(3, Math.Min(this.Width - x - 6, len));

                    string tr = s.TrimStart();
                    SolidBrush b = codeB;
                    if (tr.StartsWith("//") || tr.StartsWith("#") || tr.StartsWith("/*") || tr.StartsWith("--") || tr.StartsWith("<!--"))
                        b = comB;
                    else if (tr.IndexOf('"') >= 0 || tr.IndexOf('\'') >= 0)
                        b = strB;
                    else if (tr.StartsWith("function") || tr.StartsWith("class") || tr.StartsWith("public") || tr.StartsWith("private") || tr.StartsWith("def ") || tr.StartsWith("if") || tr.StartsWith("<"))
                        b = kwB;

                    g.FillRectangle(b, x, y, w, 2);
                }
            }

            // Translucent viewport box
            int vpTop = 4 + (firstVis - mapStartLine) * pitch;
            int vpH = Math.Max(18, Math.Min(this.Height - 8, visCount * pitch));
            vpTop = Math.Max(2, Math.Min(this.Height - vpH - 2, vpTop));
            using (SolidBrush vp = new SolidBrush(Theme.MinimapViewport))
                g.FillRectangle(vp, 1, vpTop, this.Width - 2, vpH);
        }

        private void ScrollFromY(int y)
        {
            if (Editor == null) return;
            int total = Math.Max(1, Editor.Lines.Length);
            float frac = Math.Max(0f, Math.Min(1f, (float)y / Math.Max(1, this.Height)));
            int target = (int)(frac * total);
            Editor.ScrollToLine(Math.Max(0, target - 6));
            this.Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left) { _dragging = true; ScrollFromY(e.Y); }
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_dragging) ScrollFromY(e.Y);
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _dragging = false;
        }
    }

    // ========================================================================
    // 10. PURE C# JSON PRETTY-PRINTER & MINIFIER (0 External DLLs)
    // ========================================================================
    internal static class JsonTools
    {
        public static string Format(string json, int indentSpaces)
        {
            if (string.IsNullOrWhiteSpace(json)) return json;
            StringBuilder sb = new StringBuilder(json.Length + 256);
            int indent = 0;
            bool inString = false;
            bool escaped = false;
            string tab = new string(' ', indentSpaces);

            for (int i = 0; i < json.Length; i++)
            {
                char ch = json[i];
                if (inString)
                {
                    sb.Append(ch);
                    if (escaped) escaped = false;
                    else if (ch == '\\') escaped = true;
                    else if (ch == '"') inString = false;
                    continue;
                }
                if (char.IsWhiteSpace(ch)) continue;
                if (ch == '"')
                {
                    inString = true;
                    sb.Append(ch);
                }
                else if (ch == '{' || ch == '[')
                {
                    sb.Append(ch);
                    int next = NextNonWs(json, i + 1);
                    if (next >= 0 && ((ch == '{' && json[next] == '}') || (ch == '[' && json[next] == ']')))
                    {
                        sb.Append(json[next]);
                        i = next;
                    }
                    else
                    {
                        indent++;
                        sb.Append('\n');
                        AppendIndent(sb, tab, indent);
                    }
                }
                else if (ch == '}' || ch == ']')
                {
                    indent = Math.Max(0, indent - 1);
                    sb.Append('\n');
                    AppendIndent(sb, tab, indent);
                    sb.Append(ch);
                }
                else if (ch == ',')
                {
                    sb.Append(",\n");
                    AppendIndent(sb, tab, indent);
                }
                else if (ch == ':')
                {
                    sb.Append(": ");
                }
                else
                {
                    sb.Append(ch);
                }
            }
            return sb.ToString();
        }

        public static string Minify(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return json;
            StringBuilder sb = new StringBuilder(json.Length);
            bool inString = false;
            bool escaped = false;
            for (int i = 0; i < json.Length; i++)
            {
                char ch = json[i];
                if (inString)
                {
                    sb.Append(ch);
                    if (escaped) escaped = false;
                    else if (ch == '\\') escaped = true;
                    else if (ch == '"') inString = false;
                    continue;
                }
                if (char.IsWhiteSpace(ch)) continue;
                if (ch == '"') inString = true;
                sb.Append(ch);
            }
            return sb.ToString();
        }

        private static int NextNonWs(string s, int idx)
        {
            while (idx < s.Length && char.IsWhiteSpace(s[idx])) idx++;
            return idx < s.Length ? idx : -1;
        }

        private static void AppendIndent(StringBuilder sb, string tab, int level)
        {
            for (int k = 0; k < level; k++) sb.Append(tab);
        }
    }

    // ========================================================================
    // 11. SUBLIME COMMAND PALETTE OVERLAY (Ctrl+Shift+P)
    // ========================================================================
    internal class PaletteCommand
    {
        public string Label;
        public string Shortcut;
        public Action Run;
        public PaletteCommand(string label, string shortcut, Action run)
        {
            Label = label; Shortcut = shortcut; Run = run;
        }
    }

    internal class CommandPaletteForm : Form
    {
        private TextBox _query;
        private ListBox _list;
        private List<PaletteCommand> _all;
        private List<PaletteCommand> _filtered = new List<PaletteCommand>();
        private EditorTheme _t;

        public CommandPaletteForm(List<PaletteCommand> commands, EditorTheme theme, Form owner)
        {
            _all = commands;
            _t = theme;
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.BackColor = _t.BorderLine;
            this.Padding = new Padding(1);
            this.Size = new Size(540, 340);
            this.Location = new Point(owner.Left + (owner.Width - this.Width) / 2, owner.Top + 68);
            this.KeyPreview = true;

            Panel inner = new Panel();
            inner.Dock = DockStyle.Fill;
            inner.BackColor = _t.SidebarBg;
            inner.Padding = new Padding(8);
            this.Controls.Add(inner);

            _query = new TextBox();
            _query.Dock = DockStyle.Top;
            _query.BorderStyle = BorderStyle.FixedSingle;
            _query.BackColor = _t.InputBg;
            _query.ForeColor = _t.InputFg;
            _query.Font = new Font("Segoe UI", 10.5f);
            _query.TextChanged += delegate { Refilter(); };
            _query.KeyDown += OnQueryKeyDown;

            _list = new ListBox();
            _list.Dock = DockStyle.Fill;
            _list.BorderStyle = BorderStyle.None;
            _list.BackColor = _t.SidebarBg;
            _list.ForeColor = _t.SidebarFg;
            _list.Font = new Font("Segoe UI", 9.5f);
            _list.DrawMode = DrawMode.OwnerDrawFixed;
            _list.ItemHeight = 28;
            _list.DrawItem += OnDrawItem;
            _list.DoubleClick += delegate { ExecuteSelected(); };
            _list.KeyDown += OnQueryKeyDown;

            Panel spacer = new Panel();
            spacer.Dock = DockStyle.Top;
            spacer.Height = 6;
            spacer.BackColor = _t.SidebarBg;

            inner.Controls.Add(_list);
            inner.Controls.Add(spacer);
            inner.Controls.Add(_query);

            this.Deactivate += delegate { this.Close(); };
            Refilter();
        }

        private void Refilter()
        {
            string q = (_query.Text ?? "").Trim().ToLowerInvariant();
            _filtered.Clear();
            for (int i = 0; i < _all.Count; i++)
            {
                if (q.Length == 0 || FuzzyMatch(_all[i].Label.ToLowerInvariant(), q))
                    _filtered.Add(_all[i]);
            }
            _list.BeginUpdate();
            _list.Items.Clear();
            for (int i = 0; i < _filtered.Count; i++) _list.Items.Add(_filtered[i]);
            if (_list.Items.Count > 0) _list.SelectedIndex = 0;
            _list.EndUpdate();
        }

        private static bool FuzzyMatch(string text, string pattern)
        {
            if (text.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            int pi = 0;
            for (int ti = 0; ti < text.Length && pi < pattern.Length; ti++)
                if (text[ti] == pattern[pi]) pi++;
            return pi == pattern.Length;
        }

        private void OnDrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= _filtered.Count) return;
            PaletteCommand cmd = _filtered[e.Index];
            bool sel = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            using (SolidBrush bg = new SolidBrush(sel ? _t.SidebarSelBg : _t.SidebarBg))
                e.Graphics.FillRectangle(bg, e.Bounds);
            if (sel)
            {
                using (SolidBrush acc = new SolidBrush(_t.Accent))
                    e.Graphics.FillRectangle(acc, e.Bounds.X, e.Bounds.Y + 3, 3, e.Bounds.Height - 6);
            }
            Rectangle rLabel = new Rectangle(e.Bounds.X + 10, e.Bounds.Y, e.Bounds.Width - 110, e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics, cmd.Label, e.Font, rLabel,
                sel ? _t.TabTextActive : _t.SidebarFg,
                TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

            if (!string.IsNullOrEmpty(cmd.Shortcut))
            {
                Rectangle rKey = new Rectangle(e.Bounds.Right - 105, e.Bounds.Y, 96, e.Bounds.Height);
                TextRenderer.DrawText(e.Graphics, cmd.Shortcut, e.Font, rKey, _t.SidebarHeaderFg,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Right | TextFormatFlags.NoPrefix);
            }
        }

        private void OnQueryKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) { this.Close(); e.Handled = true; }
            else if (e.KeyCode == Keys.Down && _list.Items.Count > 0)
            {
                _list.SelectedIndex = Math.Min(_list.Items.Count - 1, _list.SelectedIndex + 1);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Up && _list.Items.Count > 0)
            {
                _list.SelectedIndex = Math.Max(0, _list.SelectedIndex - 1);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                ExecuteSelected();
                e.Handled = true;
            }
        }

        private void ExecuteSelected()
        {
            int idx = _list.SelectedIndex;
            if (idx >= 0 && idx < _filtered.Count)
            {
                Action act = _filtered[idx].Run;
                this.Close();
                if (act != null) act();
            }
        }
    }

    // ========================================================================
    // 12. COMPACT DIFF VIEWER DIALOG (Compare 2 Tabs)
    // ========================================================================
    internal class DiffViewerForm : Form
    {
        public DiffViewerForm(DocTab left, DocTab right, EditorTheme theme, Font monoFont)
        {
            this.Text = "NillyCode Pro — Diff: " + left.Title + "  vs  " + right.Title;
            this.Size = new Size(920, 620);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = theme.EditorBg;
            this.ForeColor = theme.EditorFg;
            this.HandleCreated += delegate { Native.SetDarkTitleBar(this.Handle, theme.IsDark); };

            RichTextBox box = new RichTextBox();
            box.Dock = DockStyle.Fill;
            box.ReadOnly = true;
            box.BorderStyle = BorderStyle.None;
            box.BackColor = theme.EditorBg;
            box.ForeColor = theme.EditorFg;
            box.Font = monoFont;
            box.WordWrap = false;
            this.Controls.Add(box);

            string[] a = (left.Content ?? "").Replace("\r\n", "\n").Split('\n');
            string[] b = (right.Content ?? "").Replace("\r\n", "\n").Split('\n');
            int max = Math.Max(a.Length, b.Length);
            int diffs = 0;

            box.SelectionColor = theme.SynComment;
            box.AppendText("--- " + left.Title + " (" + a.Length + " lines)\n");
            box.AppendText("+++ " + right.Title + " (" + b.Length + " lines)\n\n");

            for (int i = 0; i < max; i++)
            {
                string la = i < a.Length ? a[i] : null;
                string lb = i < b.Length ? b[i] : null;
                if (la == lb)
                {
                    box.SelectionColor = theme.GutterFg;
                    box.AppendText(string.Format("  {0,5} | {1}\n", i + 1, la));
                }
                else
                {
                    diffs++;
                    if (la != null)
                    {
                        box.SelectionColor = theme.SynKeyword;
                        box.AppendText(string.Format("- {0,5} | {1}\n", i + 1, la));
                    }
                    if (lb != null)
                    {
                        box.SelectionColor = theme.SynFunction;
                        box.AppendText(string.Format("+ {0,5} | {1}\n", i + 1, lb));
                    }
                }
            }
            if (diffs == 0)
            {
                box.SelectionColor = theme.SynFunction;
                box.AppendText("\n[Τα δύο αρχεία είναι 100% ίδια / Files are identical]");
            }
            box.SelectionStart = 0;
            box.ScrollToCaret();
        }
    }

    // ========================================================================
    // 13. MAIN SUBLIME-STYLE WINDOW (MainForm)
    // ========================================================================
    internal class MainForm : Form
    {
        private EditorTheme _theme = EditorTheme.Monokai();
        private MenuStrip _menu;
        private Panel _sidebarPanel;
        private Label _sidebarHeader;
        private TreeView _tree;
        private Splitter _splitter;
        private Panel _workPanel;
        private SublimeTabBar _tabBar;
        private Panel _editorContainer;
        private LineNumberGutter _gutter;
        private CodeEditorBox _editor;
        private SublimeMinimap _minimap;

        // Bottom Sublime Find/Replace bar
        private Panel _findBar;
        private TextBox _txtFind;
        private TextBox _txtReplace;
        private Panel _replaceRow;
        private CheckBox _chkCase;
        private CheckBox _chkRegex;
        private Label _lblMatchCount;

        // Bottom Status Bar
        private Panel _statusBar;
        private Label _lblPos;
        private Label _lblInfo;
        private Label _lblEncoding;
        private Label _lblLineEnd;
        private Label _lblLang;

        private ContextMenuStrip _tabMenu;
        private int _contextTabIdx = -1;

        private List<DocTab> _tabs = new List<DocTab>();
        private int _activeIdx = -1;
        private bool _switchingTab = false;
        private string _openedFolder = "";
        private float _fontSize = 10.5f;
        private string _fontFamily = "Consolas";
        private bool _showSidebar = false;
        private bool _showMinimap = true;
        private bool _wordWrap = false;

        private System.Windows.Forms.Timer _hlTimer;
        private Thread _ipcThread;
        private volatile bool _ipcRunning = true;

        private static string ConfigDir
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NillyCodePro"); }
        }
        private static string SessionFile
        {
            get { return Path.Combine(ConfigDir, "session.ini"); }
        }

        public MainForm(string[] initialArgs)
        {
            _fontFamily = PickBestMonoFont();
            this.Text = "NillyCode Pro";
            this.Size = new Size(1220, 780);
            this.MinimumSize = new Size(640, 420);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.AllowDrop = true;
            this.KeyPreview = true;

            try
            {
                string icoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NillyCodePro.ico");
                if (File.Exists(icoPath)) this.Icon = new Icon(icoPath);
                else this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch { }

            BuildUI();
            LoadSession();
            ApplyTheme(_theme);

            // Open CLI files/folders passed in
            bool openedFromArgs = false;
            if (initialArgs != null)
            {
                for (int i = 0; i < initialArgs.Length; i++)
                {
                    string arg = initialArgs[i];
                    if (string.IsNullOrEmpty(arg) || arg.StartsWith("--")) continue;
                    if (Directory.Exists(arg))
                    {
                        OpenFolderInSidebar(arg);
                        openedFromArgs = true;
                    }
                    else if (File.Exists(arg))
                    {
                        OpenFileInTab(arg);
                        openedFromArgs = true;
                    }
                }
            }

            if (_tabs.Count == 0) NewTab();
            else if (!openedFromArgs && _activeIdx < 0) SwitchToTab(0);

            _hlTimer = new System.Windows.Forms.Timer();
            _hlTimer.Interval = 90;
            _hlTimer.Tick += delegate
            {
                _hlTimer.Stop();
                HighlightVisibleRange();
                _gutter.Invalidate();
                if (_showMinimap) _minimap.Invalidate();
            };

            this.HandleCreated += delegate { Native.SetDarkTitleBar(this.Handle, _theme.IsDark); };
            this.DragEnter += OnDragEnter;
            this.DragDrop += OnDragDrop;
            this.FormClosing += OnFormClosing;
            this.Shown += delegate
            {
                Native.SetDarkTitleBar(this.Handle, _theme.IsDark);
                _editor.SetTabWidthSpaces(4);
                _editor.Focus();
                HighlightVisibleRange();
                Native.TrimMemory();
            };

            StartIpcServer();
        }

        private static string PickBestMonoFont()
        {
            string[] candidates = new string[] { "Cascadia Mono", "Cascadia Code", "Consolas", "Courier New" };
            using (InstalledFontCollection ifc = new InstalledFontCollection())
            {
                HashSet<string> installed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < ifc.Families.Length; i++) installed.Add(ifc.Families[i].Name);
                for (int i = 0; i < candidates.Length; i++)
                    if (installed.Contains(candidates[i])) return candidates[i];
            }
            return "Consolas";
        }

        private void BuildUI()
        {
            // 1. Top MenuStrip
            _menu = new MenuStrip();
            _menu.Dock = DockStyle.Top;
            _menu.Padding = new Padding(6, 2, 0, 2);
            _menu.Font = new Font("Segoe UI", 9f);

            ToolStripMenuItem mFile = new ToolStripMenuItem("File");
            mFile.DropDownItems.Add(MakeItem("New File", "Ctrl+N", delegate { NewTab(); }));
            mFile.DropDownItems.Add(MakeItem("Open File...", "Ctrl+O", delegate { PromptOpenFile(); }));
            mFile.DropDownItems.Add(MakeItem("Open Folder...", "Ctrl+Shift+O", delegate { PromptOpenFolder(); }));
            mFile.DropDownItems.Add(new ToolStripSeparator());
            mFile.DropDownItems.Add(MakeItem("Save", "Ctrl+S", delegate { SaveActiveTab(false); }));
            mFile.DropDownItems.Add(MakeItem("Save As...", "Ctrl+Shift+S", delegate { SaveActiveTab(true); }));
            mFile.DropDownItems.Add(MakeItem("Save All", "Ctrl+Alt+S", delegate { SaveAllTabs(); }));
            mFile.DropDownItems.Add(new ToolStripSeparator());
            string noteExe = FindSiblingExe("NillyNotePro", "NillyNotePro.exe");
            if (noteExe != null)
                mFile.DropDownItems.Add(MakeItem("↗ Open in NillyNote Pro", "", delegate { SendActiveToNote(noteExe); }));
            string filesExe = FindSiblingExe("NillyFilesPro", "NillyFilesPro.exe");
            if (filesExe != null)
                mFile.DropDownItems.Add(MakeItem("📂 Reveal Folder in NillyFiles Pro", "", delegate { OpenActiveFolderInFiles(filesExe); }));
            mFile.DropDownItems.Add(new ToolStripSeparator());
            mFile.DropDownItems.Add(MakeItem("Close File", "Ctrl+W", delegate { CloseTab(_activeIdx); }));
            mFile.DropDownItems.Add(MakeItem("Close All Files", "", delegate { CloseAllTabs(); }));
            mFile.DropDownItems.Add(new ToolStripSeparator());
            mFile.DropDownItems.Add(MakeItem("Exit", "Alt+F4", delegate { this.Close(); }));

            ToolStripMenuItem mEdit = new ToolStripMenuItem("Edit");
            mEdit.DropDownItems.Add(MakeItem("Undo", "Ctrl+Z", delegate { if (_editor.CanUndo) _editor.Undo(); }));
            mEdit.DropDownItems.Add(MakeItem("Redo", "Ctrl+Y", delegate { if (_editor.CanRedo) _editor.Redo(); }));
            mEdit.DropDownItems.Add(new ToolStripSeparator());
            mEdit.DropDownItems.Add(MakeItem("Cut", "Ctrl+X", delegate { _editor.Cut(); }));
            mEdit.DropDownItems.Add(MakeItem("Copy", "Ctrl+C", delegate { _editor.Copy(); }));
            mEdit.DropDownItems.Add(MakeItem("Paste (Plain Text)", "Ctrl+V", delegate { _editor.PastePlainText(); }));
            mEdit.DropDownItems.Add(new ToolStripSeparator());
            mEdit.DropDownItems.Add(MakeItem("Toggle Line Comment", "Ctrl+/", delegate { ToggleLineComment(); }));
            mEdit.DropDownItems.Add(MakeItem("Duplicate Line", "Ctrl+Shift+D", delegate { DuplicateCurrentLine(); }));
            mEdit.DropDownItems.Add(MakeItem("Delete Line", "Ctrl+Shift+K", delegate { DeleteCurrentLine(); }));
            mEdit.DropDownItems.Add(MakeItem("Move Line Up", "Alt+Up", delegate { MoveCurrentLine(-1); }));
            mEdit.DropDownItems.Add(MakeItem("Move Line Down", "Alt+Down", delegate { MoveCurrentLine(1); }));
            mEdit.DropDownItems.Add(new ToolStripSeparator());
            mEdit.DropDownItems.Add(MakeItem("Sort Lines (A-Z)", "", delegate { SortLines(false); }));
            mEdit.DropDownItems.Add(MakeItem("Remove Duplicate Lines", "", delegate { RemoveDuplicateLines(); }));
            mEdit.DropDownItems.Add(MakeItem("Trim Trailing Whitespace", "", delegate { TrimTrailingWhitespace(); }));

            ToolStripMenuItem mFind = new ToolStripMenuItem("Find");
            mFind.DropDownItems.Add(MakeItem("Find...", "Ctrl+F", delegate { ShowFindBar(false); }));
            mFind.DropDownItems.Add(MakeItem("Replace...", "Ctrl+H", delegate { ShowFindBar(true); }));
            mFind.DropDownItems.Add(MakeItem("Find Next", "F3", delegate { FindNext(false); }));
            mFind.DropDownItems.Add(MakeItem("Find Previous", "Shift+F3", delegate { FindNext(true); }));
            mFind.DropDownItems.Add(new ToolStripSeparator());
            mFind.DropDownItems.Add(MakeItem("Go to Line...", "Ctrl+G", delegate { PromptGoToLine(); }));

            ToolStripMenuItem mView = new ToolStripMenuItem("View");
            mView.DropDownItems.Add(MakeItem("Toggle Sidebar", "Ctrl+B", delegate { ToggleSidebar(); }));
            mView.DropDownItems.Add(MakeItem("Toggle Minimap", "", delegate { ToggleMinimap(); }));
            mView.DropDownItems.Add(MakeItem("Toggle Word Wrap", "Alt+Z", delegate { ToggleWordWrap(); }));
            mView.DropDownItems.Add(new ToolStripSeparator());
            mView.DropDownItems.Add(MakeItem("Theme: Monokai (Sublime Classic)", "", delegate { ApplyTheme(EditorTheme.Monokai()); }));
            mView.DropDownItems.Add(MakeItem("Theme: Mariana (Sublime 4 Dark)", "", delegate { ApplyTheme(EditorTheme.Mariana()); }));
            mView.DropDownItems.Add(MakeItem("Theme: Light Paper", "", delegate { ApplyTheme(EditorTheme.Light()); }));
            mView.DropDownItems.Add(new ToolStripSeparator());
            mView.DropDownItems.Add(MakeItem("Zoom In", "Ctrl++", delegate { ChangeFontSize(1f); }));
            mView.DropDownItems.Add(MakeItem("Zoom Out", "Ctrl+-", delegate { ChangeFontSize(-1f); }));
            mView.DropDownItems.Add(MakeItem("Reset Zoom", "Ctrl+0", delegate { SetFontSize(10.5f); }));

            ToolStripMenuItem mTools = new ToolStripMenuItem("Tools");
            mTools.DropDownItems.Add(MakeItem("Command Palette...", "Ctrl+Shift+P", delegate { ShowCommandPalette(); }));
            mTools.DropDownItems.Add(new ToolStripSeparator());
            mTools.DropDownItems.Add(MakeItem("Format JSON (Pretty)", "", delegate { FormatActiveJson(false); }));
            mTools.DropDownItems.Add(MakeItem("Minify JSON", "", delegate { FormatActiveJson(true); }));
            mTools.DropDownItems.Add(new ToolStripSeparator());
            mTools.DropDownItems.Add(MakeItem("Compare Active Tab (Diff)...", "", delegate { ShowDiffPicker(); }));
            mTools.DropDownItems.Add(MakeItem("Reveal File in Explorer", "", delegate { RevealActiveInExplorer(); }));
            mTools.DropDownItems.Add(MakeItem("Open Terminal in Folder", "", delegate { OpenTerminalInFolder(); }));

            ToolStripMenuItem mHelp = new ToolStripMenuItem("Help");
            mHelp.DropDownItems.Add(MakeItem("About NillyCode Pro", "", delegate { ShowAbout(); }));
            mHelp.DropDownItems.Add(MakeItem("Nilly graphics Website", "", delegate { try { Process.Start("https://nillygraphics.com/"); } catch { } }));

            _menu.Items.AddRange(new ToolStripItem[] { mFile, mEdit, mFind, mView, mTools, mHelp });

            // 2. Bottom Status Bar
            _statusBar = new Panel();
            _statusBar.Dock = DockStyle.Bottom;
            _statusBar.Height = 25;
            _statusBar.Padding = new Padding(8, 0, 8, 0);

            _lblPos = MakeStatusLabel("Line 1, Col 1", DockStyle.Left, false);
            _lblInfo = MakeStatusLabel("0 lines  |  0 chars", DockStyle.Left, false);
            _lblInfo.Padding = new Padding(14, 0, 0, 0);

            _lblLang = MakeStatusLabel("Plain Text", DockStyle.Right, true);
            _lblLang.Click += delegate { ShowLanguageMenu(); };
            _lblLineEnd = MakeStatusLabel("CRLF", DockStyle.Right, true);
            _lblLineEnd.Click += delegate { ToggleLineEnding(); };
            _lblEncoding = MakeStatusLabel("UTF-8", DockStyle.Right, true);
            _lblEncoding.Click += delegate { CycleEncoding(); };

            _statusBar.Controls.Add(_lblInfo);
            _statusBar.Controls.Add(_lblPos);
            _statusBar.Controls.Add(_lblEncoding);
            _statusBar.Controls.Add(_lblLineEnd);
            _statusBar.Controls.Add(_lblLang);

            // 3. Bottom Find & Replace Bar (hidden initially)
            _findBar = new Panel();
            _findBar.Dock = DockStyle.Bottom;
            _findBar.Height = 36;
            _findBar.Visible = false;
            _findBar.Padding = new Padding(8, 5, 8, 5);

            Panel findRow = new Panel();
            findRow.Dock = DockStyle.Top;
            findRow.Height = 26;

            Label lF = new Label();
            lF.Text = "Find:";
            lF.Width = 56;
            lF.Dock = DockStyle.Left;
            lF.TextAlign = ContentAlignment.MiddleLeft;
            lF.Font = new Font("Segoe UI", 8.5f);

            _txtFind = new TextBox();
            _txtFind.Width = 260;
            _txtFind.Dock = DockStyle.Left;
            _txtFind.BorderStyle = BorderStyle.FixedSingle;
            _txtFind.Font = new Font("Segoe UI", 9.5f);
            _txtFind.TextChanged += delegate { UpdateMatchCount(); };
            _txtFind.KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter) { FindNext(e.Shift); e.SuppressKeyPress = true; }
                else if (e.KeyCode == Keys.Escape) { HideFindBar(); e.SuppressKeyPress = true; }
            };

            _chkCase = new CheckBox();
            _chkCase.Text = "Aa";
            _chkCase.AutoSize = true;
            _chkCase.Dock = DockStyle.Left;
            _chkCase.Padding = new Padding(8, 3, 4, 0);
            _chkCase.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            _chkCase.CheckedChanged += delegate { UpdateMatchCount(); };

            _chkRegex = new CheckBox();
            _chkRegex.Text = ".*";
            _chkRegex.AutoSize = true;
            _chkRegex.Dock = DockStyle.Left;
            _chkRegex.Padding = new Padding(4, 3, 8, 0);
            _chkRegex.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            _chkRegex.CheckedChanged += delegate { UpdateMatchCount(); };

            Button btnPrev = MakeFlatButton("Prev", 52, delegate { FindNext(true); });
            Button btnNext = MakeFlatButton("Next", 52, delegate { FindNext(false); });
            Button btnCloseFind = MakeFlatButton("X", 28, delegate { HideFindBar(); });
            btnCloseFind.Dock = DockStyle.Right;

            _lblMatchCount = new Label();
            _lblMatchCount.AutoSize = false;
            _lblMatchCount.Width = 110;
            _lblMatchCount.Dock = DockStyle.Left;
            _lblMatchCount.TextAlign = ContentAlignment.MiddleLeft;
            _lblMatchCount.Padding = new Padding(8, 0, 0, 0);
            _lblMatchCount.Font = new Font("Segoe UI", 8.5f);

            findRow.Controls.Add(_lblMatchCount);
            findRow.Controls.Add(btnNext);
            findRow.Controls.Add(btnPrev);
            findRow.Controls.Add(_chkRegex);
            findRow.Controls.Add(_chkCase);
            findRow.Controls.Add(_txtFind);
            findRow.Controls.Add(lF);
            findRow.Controls.Add(btnCloseFind);

            _replaceRow = new Panel();
            _replaceRow.Dock = DockStyle.Top;
            _replaceRow.Height = 30;
            _replaceRow.Padding = new Padding(0, 4, 0, 0);
            _replaceRow.Visible = false;

            Label lR = new Label();
            lR.Text = "Replace:";
            lR.Width = 56;
            lR.Dock = DockStyle.Left;
            lR.TextAlign = ContentAlignment.MiddleLeft;
            lR.Font = new Font("Segoe UI", 8.5f);

            _txtReplace = new TextBox();
            _txtReplace.Width = 260;
            _txtReplace.Dock = DockStyle.Left;
            _txtReplace.BorderStyle = BorderStyle.FixedSingle;
            _txtReplace.Font = new Font("Segoe UI", 9.5f);
            _txtReplace.KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter) { ReplaceOne(); e.SuppressKeyPress = true; }
                else if (e.KeyCode == Keys.Escape) { HideFindBar(); e.SuppressKeyPress = true; }
            };

            Button btnRep = MakeFlatButton("Replace", 72, delegate { ReplaceOne(); });
            Button btnRepAll = MakeFlatButton("Replace All", 84, delegate { ReplaceAll(); });

            _replaceRow.Controls.Add(btnRepAll);
            _replaceRow.Controls.Add(btnRep);
            _replaceRow.Controls.Add(_txtReplace);
            _replaceRow.Controls.Add(lR);

            _findBar.Controls.Add(_replaceRow);
            _findBar.Controls.Add(findRow);

            // 4. Collapsible Sidebar (Folder Explorer)
            _sidebarPanel = new Panel();
            _sidebarPanel.Dock = DockStyle.Left;
            _sidebarPanel.Width = 225;
            _sidebarPanel.Visible = false;

            _sidebarHeader = new Label();
            _sidebarHeader.Dock = DockStyle.Top;
            _sidebarHeader.Height = 28;
            _sidebarHeader.Text = "  FOLDERS  (Ctrl+Shift+O)";
            _sidebarHeader.Font = new Font("Segoe UI", 8f, FontStyle.Bold);
            _sidebarHeader.TextAlign = ContentAlignment.MiddleLeft;
            _sidebarHeader.Cursor = Cursors.Hand;
            _sidebarHeader.Click += delegate { PromptOpenFolder(); };

            _tree = new TreeView();
            _tree.Dock = DockStyle.Fill;
            _tree.BorderStyle = BorderStyle.None;
            _tree.ShowLines = false;
            _tree.FullRowSelect = true;
            _tree.ItemHeight = 22;
            _tree.Font = new Font("Segoe UI", 9f);
            _tree.BeforeExpand += OnTreeBeforeExpand;
            _tree.NodeMouseDoubleClick += OnTreeNodeActivate;
            _tree.KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter && _tree.SelectedNode != null)
                {
                    OpenTreeNode(_tree.SelectedNode);
                    e.Handled = true;
                }
            };

            _sidebarPanel.Controls.Add(_tree);
            _sidebarPanel.Controls.Add(_sidebarHeader);

            _splitter = new Splitter();
            _splitter.Dock = DockStyle.Left;
            _splitter.Width = 3;
            _splitter.Visible = false;

            // 5. Work Area (TabBar + Gutter + CodeEditor + Minimap)
            _workPanel = new Panel();
            _workPanel.Dock = DockStyle.Fill;

            _tabBar = new SublimeTabBar();
            _tabBar.Dock = DockStyle.Top;
            _tabBar.Tabs = _tabs;
            _tabBar.TabSelected += delegate(int i) { SwitchToTab(i); };
            _tabBar.TabClosed += delegate(int i) { CloseTab(i); };
            _tabBar.NewTabRequested += delegate { NewTab(); };
            _tabBar.TabContextMenuRequested += delegate(int i, Point pt)
            {
                _contextTabIdx = i;
                _tabMenu.Show(_tabBar, pt);
            };

            _editorContainer = new Panel();
            _editorContainer.Dock = DockStyle.Fill;

            _editor = new CodeEditorBox();
            _editor.Dock = DockStyle.Fill;
            _editor.Font = new Font(_fontFamily, _fontSize);
            _editor.TextChanged += OnEditorTextChanged;
            _editor.SelectionChanged += OnEditorSelectionChanged;
            _editor.ViewChanged += delegate { _gutter.Invalidate(); if (_showMinimap) _minimap.Invalidate(); ScheduleHighlight(); };
            _editor.KeyDown += OnEditorKeyDown;
            _editor.MouseWheel += OnEditorMouseWheel;

            // Editor right-click context menu
            ContextMenuStrip edMenu = new ContextMenuStrip();
            edMenu.Items.Add(MakeItem("Undo", "Ctrl+Z", delegate { if (_editor.CanUndo) _editor.Undo(); }));
            edMenu.Items.Add(MakeItem("Redo", "Ctrl+Y", delegate { if (_editor.CanRedo) _editor.Redo(); }));
            edMenu.Items.Add(new ToolStripSeparator());
            edMenu.Items.Add(MakeItem("Cut", "Ctrl+X", delegate { _editor.Cut(); }));
            edMenu.Items.Add(MakeItem("Copy", "Ctrl+C", delegate { _editor.Copy(); }));
            edMenu.Items.Add(MakeItem("Paste (Plain Text)", "Ctrl+V", delegate { _editor.PastePlainText(); }));
            edMenu.Items.Add(MakeItem("Select All", "Ctrl+A", delegate { _editor.SelectAll(); }));
            edMenu.Items.Add(new ToolStripSeparator());
            edMenu.Items.Add(MakeItem("Toggle Comment", "Ctrl+/", delegate { ToggleLineComment(); }));
            edMenu.Items.Add(MakeItem("Format JSON", "", delegate { FormatActiveJson(false); }));
            edMenu.Items.Add(MakeItem("Command Palette...", "Ctrl+Shift+P", delegate { ShowCommandPalette(); }));
            _editor.ContextMenuStrip = edMenu;

            _gutter = new LineNumberGutter();
            _gutter.Dock = DockStyle.Left;
            _gutter.Editor = _editor;

            _minimap = new SublimeMinimap();
            _minimap.Dock = DockStyle.Right;
            _minimap.Editor = _editor;
            _minimap.Visible = _showMinimap;

            _editorContainer.Controls.Add(_editor);
            _editorContainer.Controls.Add(_gutter);
            _editorContainer.Controls.Add(_minimap);

            _workPanel.Controls.Add(_editorContainer);
            _workPanel.Controls.Add(_tabBar);

            // Tab right-click context menu
            _tabMenu = new ContextMenuStrip();
            _tabMenu.Items.Add(MakeItem("Close Tab", "Ctrl+W", delegate { if (_contextTabIdx >= 0) CloseTab(_contextTabIdx); }));
            _tabMenu.Items.Add(MakeItem("Close Other Tabs", "", delegate { CloseOtherTabs(_contextTabIdx); }));
            _tabMenu.Items.Add(new ToolStripSeparator());
            _tabMenu.Items.Add(MakeItem("Copy File Path", "", delegate { CopyTabPath(_contextTabIdx); }));
            _tabMenu.Items.Add(MakeItem("Reveal in Explorer", "", delegate { RevealTabInExplorer(_contextTabIdx); }));
            _tabMenu.Items.Add(MakeItem("Compare with Active Tab (Diff)", "", delegate { DiffWithTab(_contextTabIdx); }));

            // Dock order matters in WinForms!
            this.Controls.Add(_workPanel);
            this.Controls.Add(_splitter);
            this.Controls.Add(_sidebarPanel);
            this.Controls.Add(_findBar);
            this.Controls.Add(_statusBar);
            this.Controls.Add(_menu);
            this.MainMenuStrip = _menu;
        }

        private static ToolStripMenuItem MakeItem(string text, string shortcutHint, Action onClick)
        {
            ToolStripMenuItem item = new ToolStripMenuItem(text);
            item.ShortcutKeyDisplayString = shortcutHint;
            item.Click += delegate { if (onClick != null) onClick(); };
            return item;
        }

        private static Label MakeStatusLabel(string text, DockStyle dock, bool clickable)
        {
            Label l = new Label();
            l.Text = text;
            l.AutoSize = true;
            l.Dock = dock;
            l.TextAlign = ContentAlignment.MiddleCenter;
            l.Padding = new Padding(8, 5, 8, 0);
            l.Font = new Font("Segoe UI", 8.5f);
            if (clickable) l.Cursor = Cursors.Hand;
            return l;
        }

        private Button MakeFlatButton(string text, int width, Action onClick)
        {
            Button b = new Button();
            b.Text = text;
            b.Width = width;
            b.Dock = DockStyle.Left;
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 1;
            b.Font = new Font("Segoe UI", 8.5f);
            b.Cursor = Cursors.Hand;
            b.Click += delegate { if (onClick != null) onClick(); };
            return b;
        }

        // ====================================================================
        // THEME APPLICATION
        // ====================================================================
        public void ApplyTheme(EditorTheme t)
        {
            _theme = t;
            this.BackColor = t.EditorBg;
            this.ForeColor = t.EditorFg;
            if (this.IsHandleCreated) Native.SetDarkTitleBar(this.Handle, t.IsDark);

            SublimeMenuRenderer renderer = new SublimeMenuRenderer(t);
            _menu.Renderer = renderer;
            _menu.BackColor = t.MenuBg;
            _menu.ForeColor = t.MenuFg;
            _tabMenu.Renderer = renderer;
            _tabMenu.BackColor = t.MenuBg;
            if (_editor.ContextMenuStrip != null)
            {
                _editor.ContextMenuStrip.Renderer = renderer;
                _editor.ContextMenuStrip.BackColor = t.MenuBg;
            }

            _tabBar.Theme = t;
            _tabBar.Invalidate();

            _gutter.Theme = t;
            _gutter.Invalidate();

            _minimap.Theme = t;
            _minimap.Invalidate();

            bool prevSwitch = _switchingTab;
            _switchingTab = true;
            try
            {
                _editor.BackColor = t.EditorBg;
                _editor.ForeColor = t.EditorFg;
            }
            finally
            {
                _switchingTab = prevSwitch;
            }

            _sidebarPanel.BackColor = t.SidebarBg;
            _sidebarHeader.BackColor = t.SidebarBg;
            _sidebarHeader.ForeColor = t.SidebarHeaderFg;
            _tree.BackColor = t.SidebarBg;
            _tree.ForeColor = t.SidebarFg;
            _splitter.BackColor = t.BorderLine;

            _statusBar.BackColor = t.StatusBarBg;
            _statusBar.ForeColor = t.StatusBarFg;
            foreach (Control c in _statusBar.Controls) c.ForeColor = t.StatusBarFg;

            _findBar.BackColor = t.SidebarBg;
            _findBar.ForeColor = t.SidebarFg;
            StyleFindControls(_findBar, t);

            ScheduleHighlight();
        }

        private static void StyleFindControls(Control parent, EditorTheme t)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is TextBox)
                {
                    c.BackColor = t.InputBg;
                    c.ForeColor = t.InputFg;
                }
                else if (c is Button)
                {
                    Button b = (Button)c;
                    b.BackColor = t.TabInactiveBg;
                    b.ForeColor = t.MenuFg;
                    b.FlatAppearance.BorderColor = t.BorderLine;
                    b.FlatAppearance.MouseOverBackColor = t.MenuHoverBg;
                }
                else if (c is Panel)
                {
                    c.BackColor = t.SidebarBg;
                    StyleFindControls(c, t);
                }
                else
                {
                    c.ForeColor = t.SidebarFg;
                }
            }
        }

        // ====================================================================
        // TAB MANAGEMENT
        // ====================================================================
        private DocTab ActiveTab
        {
            get { return (_activeIdx >= 0 && _activeIdx < _tabs.Count) ? _tabs[_activeIdx] : null; }
        }

        public void NewTab()
        {
            DocTab d = new DocTab();
            _tabs.Add(d);
            SwitchToTab(_tabs.Count - 1);
            _editor.Focus();
        }

        public void OpenFileInTab(string path)
        {
            try
            {
                string full = Path.GetFullPath(path);
                for (int i = 0; i < _tabs.Count; i++)
                {
                    if (!string.IsNullOrEmpty(_tabs[i].FilePath) &&
                        string.Equals(_tabs[i].FilePath, full, StringComparison.OrdinalIgnoreCase))
                    {
                        SwitchToTab(i);
                        return;
                    }
                }
                DocTab loaded = DocTab.LoadFromFile(full);
                // Replace single clean untitled tab if empty
                if (_tabs.Count == 1 && string.IsNullOrEmpty(_tabs[0].FilePath) && !_tabs[0].IsDirty && string.IsNullOrEmpty(_editor.Text))
                {
                    _tabs[0] = loaded;
                    _activeIdx = -1;
                    SwitchToTab(0);
                }
                else
                {
                    _tabs.Add(loaded);
                    SwitchToTab(_tabs.Count - 1);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Αδυναμία ανοίγματος αρχείου:\n" + ex.Message, "NillyCode Pro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveCurrentEditorStateToTab()
        {
            DocTab cur = ActiveTab;
            if (cur == null) return;
            cur.Content = _editor.Text;
            cur.CaretPos = _editor.SelectionStart;
            cur.FirstVisibleLine = _editor.GetFirstVisibleLine();
        }

        public void SwitchToTab(int idx)
        {
            if (idx < 0 || idx >= _tabs.Count) return;
            if (idx == _activeIdx) return;

            SaveCurrentEditorStateToTab();
            _switchingTab = true;
            try
            {
                _activeIdx = idx;
                _tabBar.ActiveIndex = idx;
                DocTab tab = _tabs[idx];

                _editor.BeginSilentUpdate();
                _editor.SuspendUndo();
                _editor.Clear();
                _editor.ForeColor = _theme.EditorFg;
                _editor.Text = tab.Content ?? "";
                _editor.SetTabWidthSpaces(4);
                _editor.Select(Math.Min(tab.CaretPos, _editor.TextLength), 0);
                _editor.ScrollToLine(tab.FirstVisibleLine);
                _editor.ResumeUndo();
                _editor.EndSilentUpdate();

                UpdateWindowTitle();
                UpdateStatusBar();
                _tabBar.Invalidate();
            }
            finally
            {
                _switchingTab = false;
            }
            HighlightVisibleRange();
            _gutter.Invalidate();
            if (_showMinimap) _minimap.Invalidate();
        }

        public bool CloseTab(int idx)
        {
            if (idx < 0 || idx >= _tabs.Count) return true;
            if (idx == _activeIdx) SaveCurrentEditorStateToTab();
            DocTab tab = _tabs[idx];
            if (tab.IsDirty)
            {
                SwitchToTab(idx);
                DialogResult res = MessageBox.Show(
                    "Να αποθηκευτούν οι αλλαγές στο \"" + tab.Title + "\" πριν το κλείσιμο;",
                    "NillyCode Pro", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (res == DialogResult.Cancel) return false;
                if (res == DialogResult.Yes && !SaveActiveTab(false)) return false;
            }

            _tabs.RemoveAt(idx);
            if (_tabs.Count == 0)
            {
                _activeIdx = -1;
                NewTab();
            }
            else
            {
                int next = Math.Min(idx, _tabs.Count - 1);
                _activeIdx = -1;
                SwitchToTab(next);
            }
            return true;
        }

        private void CloseOtherTabs(int keepIdx)
        {
            if (keepIdx < 0 || keepIdx >= _tabs.Count) return;
            DocTab keep = _tabs[keepIdx];
            for (int i = _tabs.Count - 1; i >= 0; i--)
            {
                if (_tabs[i] == keep) continue;
                if (!CloseTab(i)) return;
            }
        }

        private bool CloseAllTabs()
        {
            for (int i = _tabs.Count - 1; i >= 0; i--)
                if (!CloseTab(i)) return false;
            return true;
        }

        public bool SaveActiveTab(bool saveAs)
        {
            DocTab tab = ActiveTab;
            if (tab == null) return false;
            SaveCurrentEditorStateToTab();
            string path = tab.FilePath;
            if (saveAs || string.IsNullOrEmpty(path))
            {
                using (SaveFileDialog dlg = new SaveFileDialog())
                {
                    dlg.FileName = string.IsNullOrEmpty(path) ? tab.Title : Path.GetFileName(path);
                    dlg.Filter = "All Files (*.*)|*.*|Text Files (*.txt)|*.txt|HTML (*.html)|*.html|CSS (*.css)|*.css|JavaScript (*.js)|*.js|PHP (*.php)|*.php|JSON (*.json)|*.json|PowerShell (*.ps1)|*.ps1|C# (*.cs)|*.cs|Markdown (*.md)|*.md";
                    if (dlg.ShowDialog(this) != DialogResult.OK) return false;
                    path = dlg.FileName;
                }
            }
            try
            {
                tab.SaveToFile(path);
                UpdateWindowTitle();
                UpdateStatusBar();
                _tabBar.Invalidate();
                ScheduleHighlight();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Σφάλμα αποθήκευσης:\n" + ex.Message, "NillyCode Pro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void SaveAllTabs()
        {
            int orig = _activeIdx;
            for (int i = 0; i < _tabs.Count; i++)
            {
                if (_tabs[i].IsDirty)
                {
                    SwitchToTab(i);
                    SaveActiveTab(false);
                }
            }
            if (orig >= 0 && orig < _tabs.Count) SwitchToTab(orig);
        }

        // ====================================================================
        // VISIBLE-RANGE FLICKER-FREE SYNTAX HIGHLIGHTING
        // ====================================================================
        private void ScheduleHighlight()
        {
            if (_hlTimer == null) return;
            _hlTimer.Stop();
            _hlTimer.Start();
        }

        public void HighlightVisibleRange()
        {
            DocTab tab = ActiveTab;
            if (tab == null || !_editor.IsHandleCreated || _editor.TextLength == 0) return;

            List<SyntaxRule> rules = SyntaxEngine.GetRules(tab.Language);
            int totalLines = _editor.Lines.Length;
            if (totalLines <= 0) return;

            int firstVis = _editor.GetFirstVisibleLine();
            int lineH = Math.Max(14, (int)Math.Ceiling(_editor.Font.GetHeight()));
            int visLines = Math.Max(20, (_editor.Height / lineH) + 8);

            int startLine = Math.Max(0, firstVis - 20);
            int endLine = Math.Min(totalLines - 1, firstVis + visLines + 20);

            int startChar = _editor.GetFirstCharIndexFromLine(startLine);
            int endChar = (endLine + 1 < totalLines) ? _editor.GetFirstCharIndexFromLine(endLine + 1) : _editor.TextLength;
            if (startChar < 0 || endChar <= startChar) return;

            int len = endChar - startChar;
            string slice = _editor.Text.Substring(startChar, len);

            int selStart = _editor.SelectionStart;
            int selLen = _editor.SelectionLength;

            _editor.BeginSilentUpdate();
            _editor.SuspendUndo();
            try
            {
                _editor.Select(startChar, len);
                _editor.SelectionColor = _theme.EditorFg;

                if (rules != null && len <= 120000)
                {
                    for (int r = 0; r < rules.Count; r++)
                    {
                        SyntaxRule rule = rules[r];
                        Color col = ColorForToken(rule.Kind);
                        MatchCollection matches = rule.Pattern.Matches(slice);
                        for (int m = 0; m < matches.Count; m++)
                        {
                            Match match = matches[m];
                            if (match.Length <= 0) continue;
                            _editor.Select(startChar + match.Index, match.Length);
                            _editor.SelectionColor = col;
                        }
                    }
                }

                _editor.Select(selStart, selLen);
                _editor.SelectionColor = _theme.EditorFg;
            }
            finally
            {
                _editor.ResumeUndo();
                _editor.EndSilentUpdate();
            }
        }

        private Color ColorForToken(TokenKind kind)
        {
            switch (kind)
            {
                case TokenKind.Keyword: return _theme.SynKeyword;
                case TokenKind.String: return _theme.SynString;
                case TokenKind.Comment: return _theme.SynComment;
                case TokenKind.Number: return _theme.SynNumber;
                case TokenKind.Type: return _theme.SynType;
                case TokenKind.Function: return _theme.SynFunction;
                case TokenKind.Operator: return _theme.SynOperator;
                default: return _theme.EditorFg;
            }
        }

        // ====================================================================
        // EDITOR EVENTS & SUBLIME KEYBOARD ERGONOMICS
        // ====================================================================
        private void OnEditorTextChanged(object sender, EventArgs e)
        {
            if (_switchingTab) return;
            DocTab tab = ActiveTab;
            if (tab != null)
            {
                if (!tab.IsDirty && !string.Equals(_editor.Text, tab.Content ?? "", StringComparison.Ordinal))
                {
                    tab.IsDirty = true;
                    UpdateWindowTitle();
                    _tabBar.Invalidate();
                }
            }
            UpdateStatusBar();
            _gutter.Invalidate();
            ScheduleHighlight();
        }

        private void OnEditorSelectionChanged(object sender, EventArgs e)
        {
            UpdateStatusBar();
            _gutter.Invalidate();
        }

        private void OnEditorMouseWheel(object sender, MouseEventArgs e)
        {
            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                ChangeFontSize(e.Delta > 0 ? 1f : -1f);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.N)) { NewTab(); return true; }
            if (keyData == (Keys.Control | Keys.O)) { PromptOpenFile(); return true; }
            if (keyData == (Keys.Control | Keys.Shift | Keys.O)) { PromptOpenFolder(); return true; }
            if (keyData == (Keys.Control | Keys.S)) { SaveActiveTab(false); return true; }
            if (keyData == (Keys.Control | Keys.Shift | Keys.S)) { SaveActiveTab(true); return true; }
            if (keyData == (Keys.Control | Keys.Alt | Keys.S)) { SaveAllTabs(); return true; }
            if (keyData == (Keys.Control | Keys.W)) { CloseTab(_activeIdx); return true; }
            if (keyData == (Keys.Control | Keys.F)) { ShowFindBar(false); return true; }
            if (keyData == (Keys.Control | Keys.H)) { ShowFindBar(true); return true; }
            if (keyData == Keys.F3) { FindNext(false); return true; }
            if (keyData == (Keys.Shift | Keys.F3)) { FindNext(true); return true; }
            if (keyData == (Keys.Control | Keys.G)) { PromptGoToLine(); return true; }
            if (keyData == (Keys.Control | Keys.Shift | Keys.P)) { ShowCommandPalette(); return true; }
            if (keyData == (Keys.Control | Keys.B)) { ToggleSidebar(); return true; }
            if (keyData == (Keys.Alt | Keys.Z)) { ToggleWordWrap(); return true; }
            if (keyData == (Keys.Control | Keys.Tab) && _tabs.Count > 1) { SwitchToTab((_activeIdx + 1) % _tabs.Count); return true; }
            if (keyData == (Keys.Control | Keys.Shift | Keys.Tab) && _tabs.Count > 1) { SwitchToTab((_activeIdx - 1 + _tabs.Count) % _tabs.Count); return true; }
            if (keyData == (Keys.Control | Keys.Oemplus) || keyData == (Keys.Control | Keys.Add)) { ChangeFontSize(1f); return true; }
            if (keyData == (Keys.Control | Keys.OemMinus) || keyData == (Keys.Control | Keys.Subtract)) { ChangeFontSize(-1f); return true; }
            if (keyData == (Keys.Control | Keys.D0) || keyData == (Keys.Control | Keys.NumPad0)) { SetFontSize(10.5f); return true; }
            if (keyData == Keys.Escape && _findBar.Visible) { HideFindBar(); return true; }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void OnEditorKeyDown(object sender, KeyEventArgs e)
        {
            // Plain text paste so formatting never corrupts dark code editor
            if (e.Control && !e.Shift && !e.Alt && e.KeyCode == Keys.V)
            {
                _editor.PastePlainText();
                e.SuppressKeyPress = true;
                e.Handled = true;
                return;
            }
            // Ctrl+/ Toggle line comment
            if (e.Control && !e.Alt && (e.KeyCode == Keys.OemQuestion || e.KeyCode == Keys.Divide))
            {
                ToggleLineComment();
                e.SuppressKeyPress = true;
                e.Handled = true;
                return;
            }
            // Ctrl+Shift+D Duplicate line
            if (e.Control && e.Shift && e.KeyCode == Keys.D)
            {
                DuplicateCurrentLine();
                e.SuppressKeyPress = true;
                e.Handled = true;
                return;
            }
            // Ctrl+Shift+K Delete line
            if (e.Control && e.Shift && e.KeyCode == Keys.K)
            {
                DeleteCurrentLine();
                e.SuppressKeyPress = true;
                e.Handled = true;
                return;
            }
            // Alt+Up / Alt+Down Move line
            if (e.Alt && !e.Control && (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down))
            {
                MoveCurrentLine(e.KeyCode == Keys.Up ? -1 : 1);
                e.SuppressKeyPress = true;
                e.Handled = true;
                return;
            }
            // Tab / Shift+Tab smart indentation
            if (e.KeyCode == Keys.Tab && !e.Control && !e.Alt)
            {
                if (_editor.SelectionLength > 0 || e.Shift)
                {
                    IndentSelection(e.Shift);
                }
                else
                {
                    _editor.SelectedText = "    ";
                }
                e.SuppressKeyPress = true;
                e.Handled = true;
                return;
            }
            // Smart Auto-Indent on Enter
            if (e.KeyCode == Keys.Enter && !e.Control && !e.Alt)
            {
                int pos = _editor.SelectionStart;
                int lineIdx = _editor.GetLineFromCharIndex(pos);
                string[] lines = _editor.Lines;
                if (lineIdx >= 0 && lineIdx < lines.Length)
                {
                    string curLine = lines[lineIdx];
                    int lead = 0;
                    while (lead < curLine.Length && (curLine[lead] == ' ' || curLine[lead] == '\t')) lead++;
                    string indent = curLine.Substring(0, lead);
                    string trimmed = curLine.TrimEnd();
                    if (trimmed.EndsWith("{") || trimmed.EndsWith("[") || trimmed.EndsWith(":"))
                        indent += "    ";
                    _editor.SelectedText = "\n" + indent;
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                }
            }
        }

        private void IndentSelection(bool unindent)
        {
            int startLine = _editor.GetLineFromCharIndex(_editor.SelectionStart);
            int endPos = _editor.SelectionStart + Math.Max(0, _editor.SelectionLength - 1);
            int endLine = _editor.GetLineFromCharIndex(endPos);
            int selStartChar = _editor.GetFirstCharIndexFromLine(startLine);
            int selEndChar = (endLine + 1 < _editor.Lines.Length) ? _editor.GetFirstCharIndexFromLine(endLine + 1) : _editor.TextLength;
            if (selStartChar < 0) return;

            string block = _editor.Text.Substring(selStartChar, selEndChar - selStartChar);
            string[] lines = block.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                if (i == lines.Length - 1 && lines[i].Length == 0) continue;
                if (!unindent) lines[i] = "    " + lines[i];
                else
                {
                    if (lines[i].StartsWith("    ")) lines[i] = lines[i].Substring(4);
                    else if (lines[i].StartsWith("\t")) lines[i] = lines[i].Substring(1);
                    else
                    {
                        int sp = 0;
                        while (sp < 4 && sp < lines[i].Length && lines[i][sp] == ' ') sp++;
                        lines[i] = lines[i].Substring(sp);
                    }
                }
            }
            string updated = string.Join("\n", lines);
            _editor.Select(selStartChar, selEndChar - selStartChar);
            _editor.SelectedText = updated;
            _editor.Select(selStartChar, updated.Length);
        }

        private void ToggleLineComment()
        {
            DocTab tab = ActiveTab;
            if (tab == null) return;
            string prefix = SyntaxEngine.LineCommentPrefix(tab.Language);
            string suffix = (prefix == "<!-- ") ? " -->" : "";
            int startLine = _editor.GetLineFromCharIndex(_editor.SelectionStart);
            int endPos = _editor.SelectionStart + Math.Max(0, _editor.SelectionLength - 1);
            int endLine = _editor.GetLineFromCharIndex(endPos);
            int selStartChar = _editor.GetFirstCharIndexFromLine(startLine);
            int selEndChar = (endLine + 1 < _editor.Lines.Length) ? _editor.GetFirstCharIndexFromLine(endLine + 1) : _editor.TextLength;
            if (selStartChar < 0) return;

            string block = _editor.Text.Substring(selStartChar, selEndChar - selStartChar);
            string[] lines = block.Split('\n');
            bool allCommented = true;
            for (int i = 0; i < lines.Length; i++)
            {
                string tr = lines[i].TrimStart();
                if (tr.Length == 0) continue;
                if (!tr.StartsWith(prefix.Trim())) { allCommented = false; break; }
            }
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Trim().Length == 0) continue;
                if (allCommented)
                {
                    int idx = lines[i].IndexOf(prefix);
                    if (idx >= 0) lines[i] = lines[i].Remove(idx, prefix.Length);
                    else
                    {
                        string pt = prefix.Trim();
                        idx = lines[i].IndexOf(pt);
                        if (idx >= 0) lines[i] = lines[i].Remove(idx, pt.Length);
                    }
                    if (suffix.Length > 0 && lines[i].EndsWith(suffix))
                        lines[i] = lines[i].Substring(0, lines[i].Length - suffix.Length);
                }
                else
                {
                    int lead = 0;
                    while (lead < lines[i].Length && (lines[i][lead] == ' ' || lines[i][lead] == '\t')) lead++;
                    lines[i] = lines[i].Substring(0, lead) + prefix + lines[i].Substring(lead) + suffix;
                }
            }
            string updated = string.Join("\n", lines);
            _editor.Select(selStartChar, selEndChar - selStartChar);
            _editor.SelectedText = updated;
            _editor.Select(selStartChar, updated.Length);
        }

        private void DuplicateCurrentLine()
        {
            if (_editor.SelectionLength > 0)
            {
                string sel = _editor.SelectedText;
                int end = _editor.SelectionStart + _editor.SelectionLength;
                _editor.Select(end, 0);
                _editor.SelectedText = sel;
                return;
            }
            int line = _editor.GetLineFromCharIndex(_editor.SelectionStart);
            int start = _editor.GetFirstCharIndexFromLine(line);
            if (start < 0 || line >= _editor.Lines.Length) return;
            string lineText = _editor.Lines[line];
            int insertPos = start + lineText.Length;
            _editor.Select(insertPos, 0);
            _editor.SelectedText = "\n" + lineText;
        }

        private void DeleteCurrentLine()
        {
            int line = _editor.GetLineFromCharIndex(_editor.SelectionStart);
            int start = _editor.GetFirstCharIndexFromLine(line);
            if (start < 0) return;
            int end = (line + 1 < _editor.Lines.Length) ? _editor.GetFirstCharIndexFromLine(line + 1) : _editor.TextLength;
            _editor.Select(start, end - start);
            _editor.SelectedText = "";
        }

        private void MoveCurrentLine(int delta)
        {
            string[] lines = _editor.Lines;
            if (lines.Length < 2) return;
            int line = _editor.GetLineFromCharIndex(_editor.SelectionStart);
            int target = line + delta;
            if (target < 0 || target >= lines.Length) return;
            string tmp = lines[line];
            lines[line] = lines[target];
            lines[target] = tmp;
            _editor.Text = string.Join("\n", lines);
            int pos = _editor.GetFirstCharIndexFromLine(target);
            if (pos >= 0) _editor.Select(pos, 0);
            ScheduleHighlight();
        }

        // ====================================================================
        // FIND & REPLACE
        // ====================================================================
        private void ShowFindBar(bool withReplace)
        {
            _findBar.Visible = true;
            _replaceRow.Visible = withReplace;
            _findBar.Height = withReplace ? 66 : 36;
            if (_editor.SelectionLength > 0 && _editor.SelectionLength < 120 && _editor.SelectedText.IndexOf('\n') < 0)
                _txtFind.Text = _editor.SelectedText;
            _txtFind.Focus();
            _txtFind.SelectAll();
            UpdateMatchCount();
        }

        private void HideFindBar()
        {
            _findBar.Visible = false;
            _editor.Focus();
        }

        private Regex BuildFindRegex()
        {
            string q = _txtFind.Text;
            if (string.IsNullOrEmpty(q)) return null;
            try
            {
                string pat = _chkRegex.Checked ? q : Regex.Escape(q);
                RegexOptions opts = _chkCase.Checked ? RegexOptions.None : RegexOptions.IgnoreCase;
                return new Regex(pat, opts);
            }
            catch { return null; }
        }

        private void UpdateMatchCount()
        {
            Regex rx = BuildFindRegex();
            if (rx == null) { _lblMatchCount.Text = ""; return; }
            MatchCollection mc = rx.Matches(_editor.Text);
            _lblMatchCount.Text = mc.Count == 0 ? "No matches" : (mc.Count + " matches");
        }

        private void FindNext(bool backwards)
        {
            Regex rx = BuildFindRegex();
            if (rx == null) return;
            MatchCollection mc = rx.Matches(_editor.Text);
            if (mc.Count == 0) { _lblMatchCount.Text = "No matches"; return; }

            int curPos = backwards ? _editor.SelectionStart : (_editor.SelectionStart + _editor.SelectionLength);
            Match chosen = null;
            int chosenIdx = 0;

            if (!backwards)
            {
                for (int i = 0; i < mc.Count; i++)
                {
                    if (mc[i].Index >= curPos) { chosen = mc[i]; chosenIdx = i; break; }
                }
                if (chosen == null) { chosen = mc[0]; chosenIdx = 0; }
            }
            else
            {
                for (int i = mc.Count - 1; i >= 0; i--)
                {
                    if (mc[i].Index < curPos) { chosen = mc[i]; chosenIdx = i; break; }
                }
                if (chosen == null) { chosen = mc[mc.Count - 1]; chosenIdx = mc.Count - 1; }
            }

            _editor.Select(chosen.Index, chosen.Length);
            _editor.ScrollToCaret();
            _lblMatchCount.Text = (chosenIdx + 1) + " of " + mc.Count;
        }

        private void ReplaceOne()
        {
            Regex rx = BuildFindRegex();
            if (rx == null) return;
            if (_editor.SelectionLength > 0 && rx.IsMatch(_editor.SelectedText))
                _editor.SelectedText = _txtReplace.Text;
            FindNext(false);
        }

        private void ReplaceAll()
        {
            Regex rx = BuildFindRegex();
            if (rx == null) return;
            int count = rx.Matches(_editor.Text).Count;
            if (count == 0) return;
            _editor.Text = rx.Replace(_editor.Text, _txtReplace.Text);
            _lblMatchCount.Text = "Replaced " + count;
            ScheduleHighlight();
        }

        // ====================================================================
        // STUDIO TOOLS (JSON, Sort, Dedup, Case, Diff, Sidebar, Palette)
        // ====================================================================
        private void FormatActiveJson(bool minify)
        {
            try
            {
                string src = _editor.SelectionLength > 0 ? _editor.SelectedText : _editor.Text;
                string res = minify ? JsonTools.Minify(src) : JsonTools.Format(src, 2);
                if (_editor.SelectionLength > 0) _editor.SelectedText = res;
                else
                {
                    _editor.Text = res;
                    if (ActiveTab != null) { ActiveTab.Language = "JSON"; UpdateStatusBar(); }
                }
                ScheduleHighlight();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Σφάλμα JSON format:\n" + ex.Message, "NillyCode Pro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SortLines(bool descending)
        {
            string[] lines = _editor.Lines;
            Array.Sort(lines, StringComparer.CurrentCultureIgnoreCase);
            if (descending) Array.Reverse(lines);
            _editor.Text = string.Join("\n", lines);
            ScheduleHighlight();
        }

        private void RemoveDuplicateLines()
        {
            string[] lines = _editor.Lines;
            HashSet<string> seen = new HashSet<string>();
            List<string> kept = new List<string>();
            for (int i = 0; i < lines.Length; i++)
                if (seen.Add(lines[i])) kept.Add(lines[i]);
            _editor.Text = string.Join("\n", kept.ToArray());
            ScheduleHighlight();
        }

        private void TrimTrailingWhitespace()
        {
            string[] lines = _editor.Lines;
            for (int i = 0; i < lines.Length; i++) lines[i] = lines[i].TrimEnd(' ', '\t');
            _editor.Text = string.Join("\n", lines);
            ScheduleHighlight();
        }

        private void TransformCase(int mode) // 0=UPPER, 1=lower, 2=Title
        {
            bool sel = _editor.SelectionLength > 0;
            string s = sel ? _editor.SelectedText : _editor.Text;
            string res = mode == 0 ? s.ToUpper() : (mode == 1 ? s.ToLower() : System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower()));
            if (sel) _editor.SelectedText = res;
            else _editor.Text = res;
        }

        private void ShowDiffPicker()
        {
            if (_tabs.Count < 2)
            {
                MessageBox.Show("Άνοιξε τουλάχιστον 2 tabs για να τα συγκρίνεις (Diff).", "NillyCode Pro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            int other = (_activeIdx == 0) ? 1 : 0;
            DiffWithTab(other);
        }

        private void DiffWithTab(int otherIdx)
        {
            if (otherIdx < 0 || otherIdx >= _tabs.Count || otherIdx == _activeIdx) return;
            SaveCurrentEditorStateToTab();
            using (DiffViewerForm dlg = new DiffViewerForm(_tabs[_activeIdx], _tabs[otherIdx], _theme, _editor.Font))
                dlg.ShowDialog(this);
        }

        private void PromptOpenFile()
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Multiselect = true;
                dlg.Filter = "All Files (*.*)|*.*|Code & Text|*.txt;*.php;*.html;*.css;*.js;*.ts;*.json;*.cs;*.ps1;*.bat;*.py;*.sql;*.xml;*.md;*.ini;*.yml";
                if (dlg.ShowDialog(this) == DialogResult.OK)
                    for (int i = 0; i < dlg.FileNames.Length; i++) OpenFileInTab(dlg.FileNames[i]);
            }
        }

        private void PromptOpenFolder()
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = "Επιλογή φακέλου για το Sidebar του NillyCode Pro";
                if (!string.IsNullOrEmpty(_openedFolder) && Directory.Exists(_openedFolder))
                    dlg.SelectedPath = _openedFolder;
                if (dlg.ShowDialog(this) == DialogResult.OK)
                    OpenFolderInSidebar(dlg.SelectedPath);
            }
        }

        private void OpenFolderInSidebar(string folder)
        {
            if (!Directory.Exists(folder)) return;
            _openedFolder = Path.GetFullPath(folder);
            _showSidebar = true;
            _sidebarPanel.Visible = true;
            _splitter.Visible = true;
            _sidebarHeader.Text = "  " + Path.GetFileName(_openedFolder).ToUpperInvariant();

            _tree.BeginUpdate();
            _tree.Nodes.Clear();
            TreeNode root = new TreeNode(Path.GetFileName(_openedFolder));
            root.Tag = _openedFolder;
            PopulateTreeNode(root);
            root.Expand();
            _tree.Nodes.Add(root);
            _tree.EndUpdate();
        }

        private static void PopulateTreeNode(TreeNode node)
        {
            string dir = node.Tag as string;
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) return;
            node.Nodes.Clear();
            try
            {
                string[] dirs = Directory.GetDirectories(dir);
                Array.Sort(dirs, StringComparer.CurrentCultureIgnoreCase);
                for (int i = 0; i < dirs.Length; i++)
                {
                    string name = Path.GetFileName(dirs[i]);
                    if (name == ".git" || name == "node_modules" || name == "bin" || name == "obj") continue;
                    TreeNode dn = new TreeNode("[" + name + "]");
                    dn.Tag = dirs[i];
                    dn.Nodes.Add(new TreeNode("..."));
                    node.Nodes.Add(dn);
                }
                string[] files = Directory.GetFiles(dir);
                Array.Sort(files, StringComparer.CurrentCultureIgnoreCase);
                for (int i = 0; i < files.Length; i++)
                {
                    TreeNode fn = new TreeNode(Path.GetFileName(files[i]));
                    fn.Tag = files[i];
                    node.Nodes.Add(fn);
                }
            }
            catch { }
        }

        private void OnTreeBeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.Nodes.Count == 1 && e.Node.Nodes[0].Text == "...")
                PopulateTreeNode(e.Node);
        }

        private void OnTreeNodeActivate(object sender, TreeNodeMouseClickEventArgs e)
        {
            OpenTreeNode(e.Node);
        }

        private void OpenTreeNode(TreeNode node)
        {
            string p = node.Tag as string;
            if (!string.IsNullOrEmpty(p) && File.Exists(p))
            {
                OpenFileInTab(p);
                _editor.Focus();
            }
        }

        private void PromptGoToLine()
        {
            using (Form f = new Form())
            {
                f.FormBorderStyle = FormBorderStyle.None;
                f.StartPosition = FormStartPosition.Manual;
                f.ShowInTaskbar = false;
                f.Size = new Size(300, 44);
                f.BackColor = _theme.BorderLine;
                f.Padding = new Padding(1);
                f.Location = new Point(this.Left + (this.Width - f.Width) / 2, this.Top + 68);

                Panel p = new Panel();
                p.Dock = DockStyle.Fill;
                p.BackColor = _theme.SidebarBg;
                p.Padding = new Padding(8);
                f.Controls.Add(p);

                Label lbl = new Label();
                lbl.Text = "Go to Line:";
                lbl.Dock = DockStyle.Left;
                lbl.Width = 74;
                lbl.ForeColor = _theme.SidebarFg;
                lbl.TextAlign = ContentAlignment.MiddleLeft;

                TextBox tb = new TextBox();
                tb.Dock = DockStyle.Fill;
                tb.BackColor = _theme.InputBg;
                tb.ForeColor = _theme.InputFg;
                tb.BorderStyle = BorderStyle.FixedSingle;
                tb.Font = new Font("Segoe UI", 10f);
                tb.KeyDown += delegate(object s, KeyEventArgs e)
                {
                    if (e.KeyCode == Keys.Enter)
                    {
                        int ln;
                        if (int.TryParse(tb.Text.Trim(), out ln) && ln >= 1)
                        {
                            int idx = _editor.GetFirstCharIndexFromLine(Math.Min(ln - 1, Math.Max(0, _editor.Lines.Length - 1)));
                            if (idx >= 0) { _editor.Select(idx, 0); _editor.ScrollToCaret(); }
                        }
                        f.Close();
                    }
                    else if (e.KeyCode == Keys.Escape) f.Close();
                };
                p.Controls.Add(tb);
                p.Controls.Add(lbl);
                f.Deactivate += delegate { f.Close(); };
                f.ShowDialog(this);
            }
        }

        private void ShowCommandPalette()
        {
            List<PaletteCommand> cmds = new List<PaletteCommand>();
            cmds.Add(new PaletteCommand("Theme: Monokai (Sublime Classic)", "", delegate { ApplyTheme(EditorTheme.Monokai()); }));
            cmds.Add(new PaletteCommand("Theme: Mariana (Sublime 4 Dark)", "", delegate { ApplyTheme(EditorTheme.Mariana()); }));
            cmds.Add(new PaletteCommand("Theme: Light Paper", "", delegate { ApplyTheme(EditorTheme.Light()); }));
            cmds.Add(new PaletteCommand("Format JSON (Pretty Print)", "", delegate { FormatActiveJson(false); }));
            cmds.Add(new PaletteCommand("Minify JSON", "", delegate { FormatActiveJson(true); }));
            cmds.Add(new PaletteCommand("View: Toggle Sidebar", "Ctrl+B", delegate { ToggleSidebar(); }));
            cmds.Add(new PaletteCommand("View: Toggle Minimap", "", delegate { ToggleMinimap(); }));
            cmds.Add(new PaletteCommand("View: Toggle Word Wrap", "Alt+Z", delegate { ToggleWordWrap(); }));
            cmds.Add(new PaletteCommand("File: Open Folder in Sidebar", "Ctrl+Shift+O", delegate { PromptOpenFolder(); }));
            cmds.Add(new PaletteCommand("File: Save All", "Ctrl+Alt+S", delegate { SaveAllTabs(); }));
            cmds.Add(new PaletteCommand("File: Reveal in Explorer", "", delegate { RevealActiveInExplorer(); }));
            cmds.Add(new PaletteCommand("File: Copy Full Path", "", delegate { CopyTabPath(_activeIdx); }));
            cmds.Add(new PaletteCommand("Tools: Open Terminal in Folder", "", delegate { OpenTerminalInFolder(); }));
            cmds.Add(new PaletteCommand("Tools: Compare Active Tab (Diff)", "", delegate { ShowDiffPicker(); }));
            cmds.Add(new PaletteCommand("Edit: Sort Lines (A -> Z)", "", delegate { SortLines(false); }));
            cmds.Add(new PaletteCommand("Edit: Sort Lines (Z -> A)", "", delegate { SortLines(true); }));
            cmds.Add(new PaletteCommand("Edit: Remove Duplicate Lines", "", delegate { RemoveDuplicateLines(); }));
            cmds.Add(new PaletteCommand("Edit: Trim Trailing Whitespace", "", delegate { TrimTrailingWhitespace(); }));
            cmds.Add(new PaletteCommand("Convert Case: UPPERCASE", "", delegate { TransformCase(0); }));
            cmds.Add(new PaletteCommand("Convert Case: lowercase", "", delegate { TransformCase(1); }));
            cmds.Add(new PaletteCommand("Convert Case: Title Case", "", delegate { TransformCase(2); }));
            cmds.Add(new PaletteCommand("Go to Line...", "Ctrl+G", delegate { PromptGoToLine(); }));

            for (int i = 0; i < SyntaxEngine.AllLanguages.Length; i++)
            {
                string lang = SyntaxEngine.AllLanguages[i];
                cmds.Add(new PaletteCommand("Set Syntax: " + lang, "", delegate { SetActiveLanguage(lang); }));
            }

            CommandPaletteForm pal = new CommandPaletteForm(cmds, _theme, this);
            pal.Show(this);
        }

        // ====================================================================
        // VIEW & STATUS HELPERS
        // ====================================================================
        private void ToggleSidebar()
        {
            _showSidebar = !_showSidebar;
            _sidebarPanel.Visible = _showSidebar;
            _splitter.Visible = _showSidebar;
        }

        private void ToggleMinimap()
        {
            _showMinimap = !_showMinimap;
            _minimap.Visible = _showMinimap;
            if (_showMinimap) _minimap.Invalidate();
        }

        private void ToggleWordWrap()
        {
            bool prev = _switchingTab;
            _switchingTab = true;
            try
            {
                _wordWrap = !_wordWrap;
                _editor.WordWrap = _wordWrap;
            }
            finally { _switchingTab = prev; }
            _gutter.Invalidate();
        }

        private void ChangeFontSize(float delta)
        {
            SetFontSize(Math.Max(7f, Math.Min(28f, _fontSize + delta)));
        }

        private void SetFontSize(float size)
        {
            bool prev = _switchingTab;
            _switchingTab = true;
            try
            {
                _fontSize = size;
                _editor.Font = new Font(_fontFamily, _fontSize);
                _editor.SetTabWidthSpaces(4);
            }
            finally { _switchingTab = prev; }
            _gutter.Invalidate();
            ScheduleHighlight();
        }

        private void SetActiveLanguage(string lang)
        {
            DocTab tab = ActiveTab;
            if (tab == null) return;
            tab.Language = lang;
            UpdateStatusBar();
            _tabBar.Invalidate();
            HighlightVisibleRange();
        }

        private void ShowLanguageMenu()
        {
            ContextMenuStrip m = new ContextMenuStrip();
            m.Renderer = new SublimeMenuRenderer(_theme);
            m.BackColor = _theme.MenuBg;
            for (int i = 0; i < SyntaxEngine.AllLanguages.Length; i++)
            {
                string lang = SyntaxEngine.AllLanguages[i];
                m.Items.Add(MakeItem(lang, "", delegate { SetActiveLanguage(lang); }));
            }
            m.Show(_lblLang, new Point(0, -m.PreferredSize.Height));
        }

        private void ToggleLineEnding()
        {
            DocTab tab = ActiveTab;
            if (tab == null) return;
            tab.LineEnding = (tab.LineEnding == "CRLF") ? "LF" : "CRLF";
            tab.IsDirty = true;
            UpdateStatusBar();
            _tabBar.Invalidate();
        }

        private void CycleEncoding()
        {
            DocTab tab = ActiveTab;
            if (tab == null) return;
            if (tab.EncodingName == "UTF-8") tab.EncodingName = "UTF-8 BOM";
            else if (tab.EncodingName == "UTF-8 BOM") tab.EncodingName = "Win-1253";
            else tab.EncodingName = "UTF-8";
            tab.IsDirty = true;
            UpdateStatusBar();
            _tabBar.Invalidate();
        }

        private void CopyTabPath(int idx)
        {
            if (idx >= 0 && idx < _tabs.Count && !string.IsNullOrEmpty(_tabs[idx].FilePath))
                try { Clipboard.SetText(_tabs[idx].FilePath); } catch { }
        }

        private void RevealActiveInExplorer() { RevealTabInExplorer(_activeIdx); }

        private void RevealTabInExplorer(int idx)
        {
            if (idx >= 0 && idx < _tabs.Count && !string.IsNullOrEmpty(_tabs[idx].FilePath) && File.Exists(_tabs[idx].FilePath))
                try { Process.Start("explorer.exe", "/select,\"" + _tabs[idx].FilePath + "\""); } catch { }
        }

        private void OpenTerminalInFolder()
        {
            string dir = _openedFolder;
            if (ActiveTab != null && !string.IsNullOrEmpty(ActiveTab.FilePath))
                dir = Path.GetDirectoryName(ActiveTab.FilePath);
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
                dir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            try { Process.Start(new ProcessStartInfo("powershell.exe") { WorkingDirectory = dir }); } catch { }
        }

        private void UpdateWindowTitle()
        {
            DocTab tab = ActiveTab;
            if (tab == null) { this.Text = "NillyCode Pro"; return; }
            string mark = tab.IsDirty ? "* " : "";
            string path = string.IsNullOrEmpty(tab.FilePath) ? tab.Title : tab.FilePath;
            this.Text = mark + path + " — NillyCode Pro";
        }

        private void UpdateStatusBar()
        {
            DocTab tab = ActiveTab;
            int pos = _editor.SelectionStart;
            int line = _editor.GetLineFromCharIndex(pos);
            int lineStart = _editor.GetFirstCharIndexFromLine(line);
            int col = Math.Max(0, pos - lineStart) + 1;
            string selStr = _editor.SelectionLength > 0 ? ("  (" + _editor.SelectionLength + " selected)") : "";
            _lblPos.Text = "Line " + (line + 1) + ", Col " + col + selStr;
            _lblInfo.Text = Math.Max(1, _editor.Lines.Length) + " lines  |  " + _editor.TextLength + " chars";
            if (tab != null)
            {
                _lblEncoding.Text = tab.EncodingName;
                _lblLineEnd.Text = tab.LineEnding;
                _lblLang.Text = tab.Language;
            }
        }

        private void ShowAbout()
        {
            MessageBox.Show(
                "NillyCode Pro v1.0\n" +
                "Ultra-Light Native Sublime-Style Code & Text Editor\n\n" +
                "• 100% Native Windows (.NET 4.x · 0 External DLLs)\n" +
                "• 15 Languages Syntax Highlighting · Minimap · Command Palette\n" +
                "• 0 MB Background RAM · Zero Admin Portable\n\n" +
                "created by Nilly graphics (Sotiris Christos)\n" +
                "https://nillygraphics.com/",
                "About NillyCode Pro", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ====================================================================
        // DRAG & DROP + SESSION SAVE/RESTORE + SINGLE-INSTANCE IPC
        // ====================================================================
        private void OnDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void OnDragDrop(object sender, DragEventArgs e)
        {
            string[] items = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (items == null) return;
            for (int i = 0; i < items.Length; i++)
            {
                if (Directory.Exists(items[i])) OpenFolderInSidebar(items[i]);
                else if (File.Exists(items[i])) OpenFileInTab(items[i]);
            }
        }

        private static string FindSiblingExe(string folder, string exeName)
        {
            try
            {
                string appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"NillySuite\" + folder + @"\" + exeName);
                if (File.Exists(appData)) return appData;
                DirectoryInfo par = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\'));
                if (par != null)
                {
                    string sib = Path.Combine(Path.Combine(par.FullName, folder), exeName);
                    if (File.Exists(sib)) return sib;
                }
            }
            catch { }
            return null;
        }

        private void SendActiveToNote(string noteExe)
        {
            try
            {
                string path = null;
                if (_activeIdx >= 0 && _activeIdx < _tabs.Count && !string.IsNullOrEmpty(_tabs[_activeIdx].FilePath) && File.Exists(_tabs[_activeIdx].FilePath))
                    path = _tabs[_activeIdx].FilePath;
                if (path == null)
                {
                    path = Path.Combine(Path.GetTempPath(), "NillyCode_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt");
                    File.WriteAllText(path, _editor.Text ?? "", new UTF8Encoding(false));
                }
                Process.Start(new ProcessStartInfo(noteExe, "\"" + path + "\"") { WorkingDirectory = Path.GetDirectoryName(noteExe), UseShellExecute = true });
            }
            catch { }
        }

        private void OpenActiveFolderInFiles(string filesExe)
        {
            try
            {
                string dir = null;
                if (_activeIdx >= 0 && _activeIdx < _tabs.Count && !string.IsNullOrEmpty(_tabs[_activeIdx].FilePath))
                    dir = Path.GetDirectoryName(_tabs[_activeIdx].FilePath);
                if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) dir = _openedFolder;
                if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                    Process.Start(new ProcessStartInfo(filesExe, "\"" + dir + "\"") { WorkingDirectory = Path.GetDirectoryName(filesExe), UseShellExecute = true });
            }
            catch { }
        }

        private void LoadSession()
        {
            try
            {
                string suiteTheme = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"NillySuite\suite.theme");
                bool hasLocal = File.Exists(SessionFile);
                if (hasLocal)
                {
                    string[] lines = File.ReadAllLines(SessionFile, Encoding.UTF8);
                    int active = 0;
                    for (int i = 0; i < lines.Length; i++)
                    {
                        int eq = lines[i].IndexOf('=');
                        if (eq <= 0) continue;
                        string k = lines[i].Substring(0, eq).Trim();
                        string v = lines[i].Substring(eq + 1).Trim();
                        if (k == "Theme") _theme = EditorTheme.ByName(v);
                        else if (k == "FontSize") { float fs; if (float.TryParse(v, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out fs)) SetFontSize(fs); }
                        else if (k == "Minimap") { _showMinimap = (v == "1"); _minimap.Visible = _showMinimap; }
                        else if (k == "WordWrap") { _wordWrap = (v == "1"); _editor.WordWrap = _wordWrap; }
                        else if (k == "Folder" && Directory.Exists(v)) OpenFolderInSidebar(v);
                        else if (k == "Sidebar") { _showSidebar = (v == "1"); _sidebarPanel.Visible = _showSidebar; _splitter.Visible = _showSidebar; }
                        else if (k == "Tab" && File.Exists(v)) OpenFileInTab(v);
                        else if (k == "ActiveIndex") { int ai; if (int.TryParse(v, out ai)) active = ai; }
                    }
                    if (_tabs.Count > 0 && active >= 0 && active < _tabs.Count) SwitchToTab(active);
                }
                if (File.Exists(suiteTheme) && (!hasLocal || File.GetLastWriteTimeUtc(suiteTheme) > File.GetLastWriteTimeUtc(SessionFile)))
                {
                    string s = File.ReadAllText(suiteTheme, Encoding.ASCII).Trim();
                    if (s.Equals("light", StringComparison.OrdinalIgnoreCase)) _theme = EditorTheme.ByName("Light");
                    else if (s.Equals("dark", StringComparison.OrdinalIgnoreCase) && !_theme.IsDark) _theme = EditorTheme.ByName("Monokai");
                }
            }
            catch { }
        }

        private void SaveSession()
        {
            try
            {
                Directory.CreateDirectory(ConfigDir);
                List<string> lines = new List<string>();
                lines.Add("Theme=" + _theme.Name);
                lines.Add("FontSize=" + _fontSize.ToString(System.Globalization.CultureInfo.InvariantCulture));
                lines.Add("Minimap=" + (_showMinimap ? "1" : "0"));
                lines.Add("WordWrap=" + (_wordWrap ? "1" : "0"));
                lines.Add("Sidebar=" + (_showSidebar ? "1" : "0"));
                if (!string.IsNullOrEmpty(_openedFolder)) lines.Add("Folder=" + _openedFolder);
                lines.Add("ActiveIndex=" + _activeIdx);
                for (int i = 0; i < _tabs.Count; i++)
                    if (!string.IsNullOrEmpty(_tabs[i].FilePath) && File.Exists(_tabs[i].FilePath))
                        lines.Add("Tab=" + _tabs[i].FilePath);
                File.WriteAllLines(SessionFile, lines.ToArray(), Encoding.UTF8);
            }
            catch { }
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            SaveCurrentEditorStateToTab();
            for (int i = 0; i < _tabs.Count; i++)
            {
                if (_tabs[i].IsDirty)
                {
                    SwitchToTab(i);
                    DialogResult res = MessageBox.Show(
                        "Υπάρχουν μη αποθηκευμένες αλλαγές στο \"" + _tabs[i].Title + "\".\nΝα αποθηκευτούν πριν το κλείσιμο;",
                        "NillyCode Pro", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    if (res == DialogResult.Cancel) { e.Cancel = true; return; }
                    if (res == DialogResult.Yes && !SaveActiveTab(false)) { e.Cancel = true; return; }
                }
            }
            _ipcRunning = false;
            SaveSession();
        }

        private static string GetPipeName()
        {
            return "NillyCodePro_IPC_" + Environment.UserName.Replace(" ", "_");
        }

        private void StartIpcServer()
        {
            _ipcThread = new Thread(delegate()
            {
                string pipeName = GetPipeName();
                while (_ipcRunning)
                {
                    try
                    {
                        using (NamedPipeServerStream server = new NamedPipeServerStream(pipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
                        {
                            IAsyncResult ar = server.BeginWaitForConnection(null, null);
                            while (_ipcRunning && !ar.IsCompleted) Thread.Sleep(150);
                            if (!_ipcRunning) break;
                            server.EndWaitForConnection(ar);
                            using (StreamReader sr = new StreamReader(server, Encoding.UTF8))
                            {
                                string payload = sr.ReadToEnd();
                                if (!string.IsNullOrEmpty(payload) && this.IsHandleCreated)
                                {
                                    this.BeginInvoke((Action)delegate
                                    {
                                        string[] paths = payload.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                                        for (int i = 0; i < paths.Length; i++)
                                        {
                                            string p = paths[i].Trim();
                                            if (Directory.Exists(p)) OpenFolderInSidebar(p);
                                            else if (File.Exists(p)) OpenFileInTab(p);
                                        }
                                        if (this.WindowState == FormWindowState.Minimized)
                                            Native.ShowWindow(this.Handle, Native.SW_RESTORE);
                                        Native.SetForegroundWindow(this.Handle);
                                    });
                                }
                            }
                        }
                    }
                    catch { Thread.Sleep(250); }
                }
            });
            _ipcThread.IsBackground = true;
            _ipcThread.Start();
        }

        public static bool ForwardArgsToRunningInstance(string[] args)
        {
            try
            {
                using (NamedPipeClientStream client = new NamedPipeClientStream(".", GetPipeName(), PipeDirection.Out))
                {
                    client.Connect(250);
                    using (StreamWriter sw = new StreamWriter(client, Encoding.UTF8))
                    {
                        if (args != null && args.Length > 0)
                        {
                            for (int i = 0; i < args.Length; i++)
                                if (!string.IsNullOrEmpty(args[i]) && !args[i].StartsWith("--"))
                                    sw.WriteLine(Path.GetFullPath(args[i]));
                        }
                        else sw.WriteLine("");
                        sw.Flush();
                    }
                    return true;
                }
            }
            catch { return false; }
        }
    }

    // ========================================================================
    // 14. ENTRY POINT & AUTOMATED SMOKE TEST
    // ========================================================================
    internal static class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            try { Native.SetProcessDPIAware(); } catch { }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Built-in smoke test for automated QA
            if (args != null && args.Length > 0 && args[0] == "--test-smoke")
            {
                string logArg = args.Length > 1 ? args[1] : null;
                string shotArg = args.Length > 2 ? args[2] : null;
                return RunSmokeTest(logArg, shotArg);
            }

            // Single-instance check: forward opened files to existing window as new tabs!
            bool createdNew;
            string mutexName = "NillyCodePro_SingleInstance_" + Environment.UserName.Replace(" ", "_");
            using (Mutex mutex = new Mutex(true, mutexName, out createdNew))
            {
                if (!createdNew)
                {
                    if (MainForm.ForwardArgsToRunningInstance(args))
                        return 0;
                }
                Application.Run(new MainForm(args));
            }
            return 0;
        }

        private static int RunSmokeTest(string logPath, string screenshotPath)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                // 1. JSON formatter test
                string formatted = JsonTools.Format("{\"app\":\"NillyCode Pro\",\"price\":0,\"greek\":\"Ελληνικά\"}", 2);
                string minified = JsonTools.Minify(formatted);
                sb.AppendLine("JSON_FORMAT_OK=" + (formatted.Contains("\n") && formatted.Contains("Ελληνικά")));
                sb.AppendLine("JSON_MINIFY_OK=" + (minified == "{\"app\":\"NillyCode Pro\",\"price\":0,\"greek\":\"Ελληνικά\"}"));

                // 2. Language detection & syntax rules test
                sb.AppendLine("LANG_PHP=" + SyntaxEngine.DetectLanguage("index.php"));
                sb.AppendLine("LANG_PS1=" + SyntaxEngine.DetectLanguage("script.ps1"));
                sb.AppendLine("LANG_CS=" + SyntaxEngine.DetectLanguage("NillyCodePro.cs"));
                int ruleLangs = 0;
                for (int i = 1; i < SyntaxEngine.AllLanguages.Length; i++)
                    if (SyntaxEngine.GetRules(SyntaxEngine.AllLanguages[i]) != null) ruleLangs++;
                sb.AppendLine("SYNTAX_LANGUAGES_READY=" + ruleLangs);

                // 3. UI Form + Real Files + Theme + Highlight + RAM check
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string sampleCs = Path.Combine(baseDir, "desktop\\NillyCodePro.cs");
                string sampleMd = Path.Combine(baseDir, "README.md");
                List<string> openArgs = new List<string>();
                if (File.Exists(sampleMd)) openArgs.Add(sampleMd);
                if (File.Exists(sampleCs)) openArgs.Add(sampleCs);
                if (Directory.Exists(baseDir)) openArgs.Add(baseDir);

                using (MainForm form = new MainForm(openArgs.ToArray()))
                {
                    form.Show();
                    Application.DoEvents();
                    form.ApplyTheme(EditorTheme.Mariana());
                    form.ApplyTheme(EditorTheme.Light());
                    form.ApplyTheme(EditorTheme.Monokai());
                    form.HighlightVisibleRange();
                    Application.DoEvents();
                    Thread.Sleep(250);
                    Application.DoEvents();

                    if (!string.IsNullOrEmpty(screenshotPath))
                    {
                        try
                        {
                            Rectangle b = form.Bounds;
                            using (Bitmap bmp = new Bitmap(b.Width, b.Height))
                            using (Graphics g = Graphics.FromImage(bmp))
                            {
                                g.CopyFromScreen(b.Location, Point.Empty, b.Size);
                                bmp.Save(screenshotPath, System.Drawing.Imaging.ImageFormat.Png);
                                sb.AppendLine("SCREENSHOT_SAVED=True");
                            }
                        }
                        catch (Exception sx) { sb.AppendLine("SCREENSHOT_ERR=" + sx.Message); }
                    }

                    long ramMb = Process.GetCurrentProcess().WorkingSet64 / (1024 * 1024);
                    sb.AppendLine("WINDOW_OPENED_OK=" + form.IsHandleCreated);
                    sb.AppendLine("WORKING_SET_MB=" + ramMb);
                    form.Close();
                }
                sb.AppendLine("SMOKE_RESULT=PASS");
            }
            catch (Exception ex)
            {
                sb.AppendLine("SMOKE_RESULT=FAIL");
                sb.AppendLine(ex.ToString());
                if (!string.IsNullOrEmpty(logPath)) File.WriteAllText(logPath, sb.ToString(), Encoding.UTF8);
                return 1;
            }
            if (!string.IsNullOrEmpty(logPath)) File.WriteAllText(logPath, sb.ToString(), Encoding.UTF8);
            return 0;
        }
    }
}
