using UnityEngine;

namespace theArkitectPackage.IDGenerate
{
    /// <summary>
    /// 时间系统ID标记的生成器。
    /// 系统随机数生成依赖Unity的Random系统，需要正确的初始化Random系统。
    /// </summary>
    public class ConsecutiveRandomIDGenerator
    {
        private int lastGeneratedID = 0;

        /// <summary>
        /// 系统采用随机增长生成流程生成随机ID。
        /// </summary>
        /// <returns>生成ID</returns>
        public int GetNext()
        {
            //TODO 要把这个数据的大于等于0的特征等等也要写一下。
            var thisID= lastGeneratedID + Random.Range(5, 100);
            lastGeneratedID = thisID;
            return thisID;
        }
    }
}