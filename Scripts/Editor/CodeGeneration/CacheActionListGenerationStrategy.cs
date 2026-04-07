// 文件5: CacheActionListGenerationStrategy.cs
// 缓存系统专用生成策略

using System;
using System.Collections.Generic;
using System.Linq;

namespace theArkitectPackage.Editor.CodeGeneration
{
    /// <summary>
    /// 缓存动作列表生成策略
    /// 专门为CacheSystem生成ReadCacheFileActions属性，生成Lambda表达式列表
    /// </summary>
    public class CacheActionListGenerationStrategy : ICodeGenerationStrategy
    {
        /// <summary>
        /// 生成属性：private static List&lt;Action&gt; ReadCacheFileActions => new() { ... };
        /// </summary>
        public void Generate(CodeGenerationContext context, object data)
        {
            var entries = (List<CacheEntry>)data;
            var sb = context.ContentBuilder;
            string baseIndent = context.CurrentIndent;

            // 属性定义头（使用C# 9+的target-typed new表达式）
            sb.AppendLine($"{baseIndent}private static List<Action> ReadCacheFileActions => new()");
            sb.AppendLine($"{baseIndent}{{");
            
            context.IndentLevel++;
            
            // 生成每个缓存条目的Lambda
            foreach (var entry in entries)
            {
                string line = string.Format(
                    "{0}() => ReadOneCache<{1}>(\"{2}\", \"{1}\"),",
                    context.CurrentIndent,
                    entry.TypeName,
                    entry.FileName
                );
                sb.AppendLine(line);
            }
            
            context.IndentLevel--;
            
            sb.AppendLine($"{baseIndent}}};");
        }

        /// <summary>
        /// 验证缓存条目数据
        /// </summary>
        public void Validate(object data)
        {
            var entries = (List<CacheEntry>)data;
            
            if (entries == null || entries.Count == 0)
                throw new ArgumentException("缓存条目列表不能为空");

            var seenFiles = new HashSet<string>();
            
            foreach (var entry in entries)
            {
                if (string.IsNullOrWhiteSpace(entry.FileName))
                    throw new ArgumentException("缓存条目文件名不能为空");
                
                if (!CodeGenerationUtils.IsValidIdentifier(entry.TypeName))
                    throw new ArgumentException($"无效的类型标识符: {entry.TypeName}");
                
                if (!seenFiles.Add(entry.FileName))
                    throw new ArgumentException($"重复的缓存文件名: {entry.FileName}");
            }
        }
    }
}