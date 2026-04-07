// 文件2: CodeGenerationContext.cs
// 生成上下文与状态管理

using System.Text;

namespace theArkitectPackage.Editor.CodeGeneration
{
    /// <summary>
    /// 代码生成上下文
    /// 封装单次代码生成的完整状态，包括输出路径、配置选项和内容构建器
    /// </summary>
    public class CodeGenerationContext
    {
        /// <summary>输出文件路径（相对于项目根目录的绝对路径）</summary>
        public string OutputPath { get; set; }
        
        /// <summary>生成选项配置</summary>
        public CodeGenerationOptions Options { get; set; } = new CodeGenerationOptions();
        
        /// <summary>内容构建器（StringBuilder实例，用于累积生成的代码）</summary>
        public StringBuilder ContentBuilder { get; } = new StringBuilder(4096);
        
        /// <summary>当前缩进级别（0表示无缩进，每级增加一个IndentString）</summary>
        public int IndentLevel { get; set; } = 0;
        
        /// <summary>获取当前缩进级别的字符串表示</summary>
        public string CurrentIndent => string.Concat(
            System.Linq.Enumerable.Repeat(Options.IndentString, IndentLevel));
    }
}