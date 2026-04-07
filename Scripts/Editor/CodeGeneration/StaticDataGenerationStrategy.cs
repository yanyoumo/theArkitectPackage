// 文件4: StaticDataGenerationStrategy.cs
// 静态数据字段生成策略

using System;
using System.Collections.Generic;

namespace theArkitectPackage.Editor.CodeGeneration
{
    /// <summary>
    /// 静态数据生成策略
    /// 生成public static readonly/const字段，适用于配置常量、游戏数值等场景
    /// </summary>
    public class StaticDataGenerationStrategy : ICodeGenerationStrategy
    {
        /// <summary>
        /// 执行生成：将FieldConfiguration列表转换为C#字段代码
        /// </summary>
        public void Generate(CodeGenerationContext context, object data)
        {
            var fields = (List<FieldConfiguration>)data;
            
            foreach (var field in fields)
            {
                GenerateSingleField(context, field);
            }
        }

        /// <summary>
        /// 验证所有字段配置的有效性
        /// </summary>
        public void Validate(object data)
        {
            var fields = (List<FieldConfiguration>)data;
            
            foreach (var field in fields)
            {
                if (!CodeGenerationUtils.IsValidIdentifier(field.Name))
                    throw new ArgumentException($"无效的字段标识符: {field.Name}");
                
                if (string.IsNullOrWhiteSpace(field.TypeName))
                    throw new ArgumentException($"字段 {field.Name} 未指定类型");
                
                // 验证const规则：只有值类型且编译期常量才能用const
                if (field.IsConst && !IsValidConstType(field.TypeName))
                {
                    throw new ArgumentException(
                        $"字段 {field.Name} 类型 {field.TypeName} 不能声明为const");
                }
            }
        }

        private void GenerateSingleField(CodeGenerationContext context, FieldConfiguration field)
        {
            var sb = context.ContentBuilder;
            string indent = context.CurrentIndent;

            // XML文档注释
            if (!string.IsNullOrEmpty(field.XmlDocumentation))
            {
                sb.AppendLine($"{indent}/// <summary>");
                sb.AppendLine($"{indent}/// {field.XmlDocumentation}");
                sb.AppendLine($"{indent}/// </summary>");
            }

            // 特性
            foreach (var attr in field.Attributes)
            {
                sb.AppendLine($"{indent}[{attr}]");
            }

            // 修饰符构建
            var modifiers = new List<string>();
            modifiers.Add(CodeGenerationUtils.GetAccessModifierString(field.AccessModifier));
            
            if (field.IsConst)
            {
                modifiers.Add("const");
            }
            else
            {
                if (field.IsStatic) modifiers.Add("static");
                if (field.IsReadonly) modifiers.Add("readonly");
            }

            // 类型和名称
            sb.Append($"{indent}{string.Join(" ", modifiers)} {field.TypeName} {field.Name}");
            
            // 初始值
            if (!string.IsNullOrEmpty(field.InitialValue))
            {
                sb.Append($" = {field.InitialValue}");
            }
            
            sb.AppendLine(";");
            sb.AppendLine(); // 字段间空行提升可读性
        }

        private bool IsValidConstType(string typeName)
        {
            // 简化的const类型检查（实际应更完善）
            var validTypes = new HashSet<string> 
            { 
                "bool", "byte", "sbyte", "char", "decimal", 
                "double", "float", "int", "uint", "long", "ulong",
                "short", "ushort", "string" 
            };
            return validTypes.Contains(typeName);
        }
    }
}