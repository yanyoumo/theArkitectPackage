// 文件3: ICodeGenerationStrategy.cs
// 策略接口定义

namespace theArkitectPackage.Editor.CodeGeneration
{
    /// <summary>
    /// 代码生成策略接口
    /// 实现此接口以创建自定义代码生成逻辑，通过策略模式支持任意代码结构生成
    /// </summary>
    public interface ICodeGenerationStrategy
    {
        /// <summary>
        /// 生成代码内容到上下文构建器
        /// </summary>
        /// <param name="context">生成上下文，包含构建器和缩进状态</param>
        /// <param name="data">策略特定的数据对象，需在实现中转换为目标类型</param>
        void Generate(CodeGenerationContext context, object data);
        
        /// <summary>
        /// 验证输入数据的有效性和完整性
        /// </summary>
        /// <param name="data">待验证的数据对象</param>
        /// <exception cref="System.ArgumentException">数据无效时抛出</exception>
        void Validate(object data);
    }
}