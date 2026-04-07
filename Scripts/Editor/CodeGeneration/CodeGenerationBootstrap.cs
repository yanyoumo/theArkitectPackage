// 文件8: CodeGenerationBootstrap.cs
// 启动器和便捷API（可选的顶层封装）

using System;
using System.Collections.Generic;

namespace theArkitectPackage.Editor.CodeGeneration
{
    /// <summary>
    /// 代码生成启动器
    /// 提供场景化的便捷API，是框架的最上层封装
    /// </summary>
    public static class CodeGenerationBootstrap
    {
        /// <summary>
        /// 生成静态常量配置类
        /// </summary>
        /// <param name="outputPath">输出路径</param>
        /// <param name="namespaceName">命名空间</param>
        /// <param name="className">类名</param>
        /// <param name="fields">字段配置列表</param>
        /// <param name="configure">额外配置委托</param>
        public static void GenerateStaticConfig(
            string outputPath,
            string namespaceName,
            string className,
            List<FieldConfiguration> fields,
            Action<CodeGenerationOptions> configure = null)
        {
            CodeGenerationEngine.QuickGenerate(
                outputPath,
                namespaceName,
                className,
                new StaticDataGenerationStrategy(),
                fields,
                configure
            );
        }

        /// <summary>
        /// 生成缓存系统动作列表
        /// </summary>
        /// <param name="outputPath">输出路径</param>
        /// <param name="namespaceName">命名空间</param>
        /// <param name="className">类名（应为partial）</param>
        /// <param name="entries">缓存条目列表</param>
        /// <param name="configure">额外配置委托</param>
        public static void GenerateCacheActions(
            string outputPath,
            string namespaceName,
            string className,
            List<CacheEntry> entries,
            Action<CodeGenerationOptions> configure = null)
        {
            CodeGenerationEngine.QuickGenerate(
                outputPath,
                namespaceName,
                className,
                new CacheActionListGenerationStrategy(),
                entries,
                opts =>
                {
                    opts.ClassConfig.IsPartial = true;
                    configure?.Invoke(opts);
                }
            );
        }
    }
}