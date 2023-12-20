using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace theArkitectPackage.TimeManager
{
    public sealed class TimeManager : MonoBehaviour
    {
        private static TimeManager _instance;
        public static TimeManager Instance => _instance;

        public float CurrentSpeed { get; private set; } = 1.0f;
        public float CurrentTime { get; private set; } = 0.0f;

        public bool IsTimePaused => CurrentSpeed == 0.0f;

        private List<(float,Action<float>)> AlarmList;
        
        /// <summary>
        /// 设置新的系统时间速度
        /// </summary>
        /// <param name="newSpeed">新的系统时间速度</param>
        public void SetCurrentSpeed(float newSpeed)
        {
            CurrentSpeed = newSpeed;
        }

        /// <summary>
        /// 注册闹钟时间
        /// </summary>
        /// <param name="time">需要闹钟响动的时间</param>
        /// <param name="alarmCallBack">闹钟响动的回调,回调变量为闹钟响动的具体时间</param>
        /// <param name="relative">时间参数是否是基于现在游戏时间的绝对值还是相对值，默认为非</param>
        public void RegisterAlarm(float time, Action<float> alarmCallBack, bool relative = false)
        {
            var actualTime = time;
            if (relative)
            {
                actualTime += CurrentTime;
            }
            AlarmList.Add(new ValueTuple<float, Action<float>>(actualTime, alarmCallBack));
            AlarmList = AlarmList.OrderBy(v => v.Item1).ToList();//添加的时候保证这个列表是时间升序排列。
        }

        private void CheckFirstAlarm()
        {
            if (CurrentTime >= AlarmList[0].Item1)
            {
                AlarmList[0].Item2?.Invoke(CurrentTime);
                AlarmList.RemoveAt(0);
                CheckFirstAlarm();
            }
        }
        
        private void Update()
        {
            CurrentTime += Time.deltaTime * CurrentSpeed;
            if (AlarmList is { Count: > 0 })
            {
                CheckFirstAlarm();
            }
        }

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                _instance = this;
            }
        }

        private void Start()
        {
            AlarmList = new List<(float, Action<float>)>();
        }
    }
}