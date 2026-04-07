// 文件6: CodeGenerationUtils.cs
// 工具方法集合

using System.Collections.Generic;
using System.Text;

namespace theArkitectPackage.Editor.CodeGeneration
{
    /// <summary>
    /// 代码生成工具类
    /// 提供标识符验证、字符串转义等通用功能
    /// </summary>
    public static class CodeGenerationUtils
    {
        // C#关键字集合（简化版，实际应包含所有关键字和上下文关键字）
        private static readonly HashSet<string> CSharpKeywords = new HashSet<string>
        {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch",
            "char", "checked", "class", "const", "continue", "decimal", "default",
            "delegate", "do", "double", "else", "enum", "event", "explicit", "extern",
            "false", "finally", "fixed", "float", "for", "foreach", "goto", "if",
            "implicit", "in", "int", "interface", "internal", "is", "lock", "long",
            "namespace", "new", "null", "object", "operator", "out", "override",
            "params", "private", "protected", "public", "readonly", "ref", "return",
            "sbyte", "sealed", "short", "sizeof", "stackalloc", "static", "string",
            "struct", "switch", "this", "throw", "true", "try", "typeof", "uint",
            "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void",
            "volatile", "while"
        };

        /// <summary>
        /// 验证字符串是否为合法的C#标识符
        /// </summary>
        /// <param name="identifier">待验证的标识符</param>
        /// <returns>是否合法</returns>
        public static bool IsValidIdentifier(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
                return false;

            // 首字符必须是字母或下划线
            char first = identifier[0];
            if (!char.IsLetter(first) && first != '_')
                return false;

            // 不能是关键字
            if (CSharpKeywords.Contains(identifier))
                return false;

            // 后续字符必须是字母、数字或下划线
            for (int i = 1; i < identifier.Length; i++)
            {
                char c = identifier[i];
                if (!char.IsLetterOrDigit(c) && c != '_')
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 将字符串转义为合法的C#字符串字面量
        /// </summary>
        /// <param name="input">原始字符串</param>
        /// <returns>转义后的字符串（不包含引号）</returns>
        public static string EscapeStringLiteral(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var sb = new StringBuilder(input.Length + 10);
            
            foreach (char c in input)
            {
                switch (c)
                {
                    case '\\': sb.Append("\\\\"); break;
                    case '\"': sb.Append("\\\""); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    case '\0': sb.Append("\\0"); break;
                    case '\a': sb.Append("\\a"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    case '\v': sb.Append("\\v"); break;
                    default:
                        if (char.IsControl(c))
                            sb.Append($"\\u{(int)c:X4}");
                        else
                            sb.Append(c);
                        break;
                }
            }
            
            return sb.ToString();
        }

        /// <summary>
        /// 获取访问修饰符的字符串表示
        /// </summary>
        public static string GetAccessModifierString(AccessModifier modifier)
        {
            return modifier switch
            {
                AccessModifier.Public => "public",
                AccessModifier.Internal => "internal",
                AccessModifier.Protected => "protected",
                AccessModifier.Private => "private",
                AccessModifier.ProtectedInternal => "protected internal",
                AccessModifier.PrivateProtected => "private protected",
                _ => "public"
            };
        }

        /// <summary>
        /// 获取类型的默认初始值字符串表示
        /// </summary>
        public static string GetDefaultValueString(string typeName)
        {
            return typeName?.ToLower() switch
            {
                "int" or "long" or "short" or "byte" or "sbyte" => "0",
                "uint" or "ulong" or "ushort" => "0",
                "float" => "0f",
                "double" => "0d",
                "decimal" => "0m",
                "bool" => "false",
                "string" => "\"\"",
                "char" => "'\\0'",
                _ => "default"
            };
        }
    }
}