// 文件7: CodeGenerationEngine.cs
// 核心生成引擎

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace theArkitectPackage.Editor.CodeGeneration
{
    /// <summary>
    /// 代码生成引擎
    /// 框架的核心协调者，负责编排生成流程、管理文件写入和Unity资源刷新
    /// </summary>
    public static class CodeGenerationEngine
    {
        #region 公共API

        /// <summary>
        /// 执行完整的代码生成流程（完整控制）
        /// </summary>
        /// <typeparam name="TData">策略特定的数据类型</typeparam>
        /// <param name="context">生成上下文（包含路径和配置）</param>
        /// <param name="strategy">生成策略实例</param>
        /// <param name="data">策略数据</param>
        public static void Generate<TData>(
            CodeGenerationContext context, 
            ICodeGenerationStrategy strategy, 
            TData data)
        {
            // 1. 验证
            ValidateContext(context);
            strategy.Validate(data);

            // 2. 构建内容
            BuildContent(context, strategy, data);

            // 3. 写入文件
            WriteFileAtomically(context.OutputPath, context.ContentBuilder.ToString());

            // 4. 刷新Unity
            AssetDatabase.ImportAsset(context.OutputPath, ImportAssetOptions.ImportRecursive);
            
            Debug.Log($"[CodeGen] 成功生成: {context.OutputPath}");
        }

        /// <summary>
        /// 快速生成方法（便捷API，使用默认配置）
        /// </summary>
        public static void QuickGenerate<TData>(
            string outputPath,
            string namespaceName,
            string className,
            ICodeGenerationStrategy strategy,
            TData data,
            Action<CodeGenerationOptions> configure = null)
        {
            var options = new CodeGenerationOptions
            {
                NamespaceConfig = new NamespaceConfiguration { Name = namespaceName },
                ClassConfig = new ClassConfiguration { Name = className }
            };
            
            configure?.Invoke(options);

            var context = new CodeGenerationContext
            {
                OutputPath = outputPath,
                Options = options
            };

            Generate(context, strategy, data);
        }

        #endregion

        #region 构建流程

        private static void BuildContent(
            CodeGenerationContext context, 
            ICodeGenerationStrategy strategy, 
            object data)
        {
            // 文件头
            BuildFileHeader(context);

            // 命名空间（内部会调用类构建）
            BuildNamespace(context, () => 
            {
                BuildClass(context, () =>
                {
                    // 设置类内部缩进并执行策略
                    context.IndentLevel = 2;
                    strategy.Generate(context, data);
                });
            });
        }

        private static void BuildFileHeader(CodeGenerationContext context)
        {
            var sb = context.ContentBuilder;
            var opts = context.Options;

            sb.AppendLine(opts.HeaderComment);
            
            if (opts.IncludeTimestamp)
            {
                sb.AppendLine($"// 生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            }
            
            sb.AppendLine("// 工具: theArkitectPackage.Editor.CodeGeneration");
            sb.AppendLine();

            // 标准using
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            
            // 额外using
            foreach (var u in opts.AdditionalUsings)
            {
                sb.AppendLine($"using {u};");
            }
            
            sb.AppendLine();
        }

        private static void BuildNamespace(CodeGenerationContext context, Action innerBuilder)
        {
            var sb = context.ContentBuilder;
            var ns = context.Options.NamespaceConfig;

            if (ns.UseFileScopedNamespace)
            {
                // C# 10+ 文件作用域命名空间（无大括号）
                sb.AppendLine($"namespace {ns.Name};");
                sb.AppendLine();
                innerBuilder();
            }
            else
            {
                // 传统块级命名空间
                sb.AppendLine($"namespace {ns.Name}");
                sb.AppendLine("{");
                context.IndentLevel = 1;
                innerBuilder();
                sb.AppendLine("}");
            }
        }

        private static void BuildClass(CodeGenerationContext context, Action innerBuilder)
        {
            var sb = context.ContentBuilder;
            var cls = context.Options.ClassConfig;
            string indent = context.CurrentIndent;

            // XML文档注释
            if (!string.IsNullOrEmpty(cls.XmlDocumentation))
            {
                sb.AppendLine($"{indent}/// <summary>");
                sb.AppendLine($"{indent}/// {cls.XmlDocumentation}");
                sb.AppendLine($"{indent}/// </summary>");
            }

            // 特性
            foreach (var attr in cls.Attributes)
            {
                sb.AppendLine($"{indent}[{attr}]");
            }

            // 修饰符
            var mods = new List<string>();
            mods.Add(GetAccessModifierString(cls.AccessModifier));
            if (cls.IsStatic) mods.Add("static");
            if (cls.IsPartial) mods.Add("partial");

            // 类声明
            sb.Append($"{indent}{string.Join(" ", mods)} class {cls.Name}");

            // 继承
            if (cls.Inheritances.Count > 0)
            {
                sb.Append($" : {string.Join(", ", cls.Inheritances)}");
            }

            sb.AppendLine();
            sb.AppendLine($"{indent}{{");
            
            innerBuilder();
            
            sb.AppendLine($"{indent}}}");
        }

        #endregion

        #region 工具方法

        private static void ValidateContext(CodeGenerationContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            
            if (string.IsNullOrWhiteSpace(context.OutputPath))
                throw new ArgumentException("输出路径不能为空", nameof(context.OutputPath));
            
            if (context.Options?.NamespaceConfig == null)
                throw new ArgumentException("命名空间配置不能为空");
            
            if (context.Options?.ClassConfig == null)
                throw new ArgumentException("类配置不能为空");
        }

        private static void WriteFileAtomically(string path, string content)
        {
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string tempPath = path + ".tmp";
            
            try
            {
                File.WriteAllText(tempPath, content, Encoding.UTF8);
                
                if (File.Exists(path))
                    File.Replace(tempPath, path, null);
                else
                    File.Move(tempPath, path);
            }
            catch
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
                throw;
            }
        }

        private static string GetAccessModifierString(AccessModifier mod) => mod switch
        {
            AccessModifier.Public => "public",
            AccessModifier.Internal => "internal",
            AccessModifier.Protected => "protected",
            AccessModifier.Private => "private",
            AccessModifier.ProtectedInternal => "protected internal",
            AccessModifier.PrivateProtected => "private protected",
            _ => "public"
        };

        #endregion
    }
}