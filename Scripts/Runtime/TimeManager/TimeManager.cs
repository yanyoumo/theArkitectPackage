using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace theArkitectPackage.TimeManager
{
    /// <summary>
    /// 系统预定义时间变速。
    /// </summary>
    public enum TimeSpeed
    {
        Paused = 0,
        Normal = 1,
        Double = 2,
        Five = 5,
        Ten = 10,
    }
    
    /// <summary>
    /// 节拍器子系统预定义时间周期的枚举类。
    /// </summary>
    public enum MetronomePeriod
    {
        TenthSeconds = -2,
        HalfSeconds = -1,
        OneSecond = 1,
        FiveSeconds = 5,
        TenSeconds = 10,
        ThirtySeconds = 30,
        OneMinute = 60,
        FiveMinutes = 300,
        FifteenMinutes = 900,
        HalfAHour = 1800,
    }

    /// <summary>
    /// 时间系统的本体。
    /// </summary>
    /// <remarks>
    /// 本类型应当在Unity中，作为一个组件添加至某个管理GameObject上面。<br/>
    /// 本类型在Awake时，会将自己单例化。并且会自动打开DontDestroyOnLoad标记。<br/>
    /// 系统依赖Unity核心时间系统，如果系统上层系统出现问题（例如系统过于卡顿），本系统可能会出现无法预期的表现。<br/>
    /// </remarks>
    public class TimeManager : MonoBehaviour
    {
        private class TimeManagerCallback
        {
            public float Time;
            public Action<float> ActualCallback;
        }
        
        private static TimeManager _instance;
        
        /// <summary>
        /// 系统单例引用。
        /// </summary>
        public static TimeManager Instance => _instance;
        
        /// <summary>
        /// 当前时间速度。
        /// </summary>
        public float CurrentTimeSpeed { get; private set; } = 1.0f;
        /// <summary>
        /// 暂停时，系统缓存的时间速度。
        /// </summary>
        /// <remarks>
        /// 本数据在系统没有暂停时为1.0。
        /// </remarks>
        public float CachedTimeSpeed { get; private set; } = 1.0f;
        /// <summary>
        /// 当前时间。
        /// </summary>
        public float CurrentTime { get; private set; } = 0.0f;

        /// <summary>
        /// 当前系统是否暂停。
        /// </summary>
        public bool IsTimePaused => CurrentTimeSpeed == 0.0f;

        private Dictionary<string, float> _timeMarksDic;
        
        private Dictionary<string, TimeManagerCallback> _alarmList;
        private List<string> _alarmIDFromNearestToFurthest;
        
        private Dictionary<string, TimeManagerCallback> _metronomeTickList;
        private Dictionary<string, float> _metronomeTickLastTickedList;
        
        /// <summary>
        /// 重置当前系统速度。
        /// </summary>
        /// <param name="resetSpeed">是否同时重设当前变速</param>
        public void ResetTime(bool resetSpeed = false)
        {
            //TODO ResetTime的时候其他内部变量重置与否。
            CurrentTime = 0.0f;
            if (resetSpeed)
            {
                SetCurrentSpeed(1.0f);
            }
        }
        
        /// <summary>
        /// 设置新的系统时间速度。
        /// </summary>
        /// <param name="newSpeed">新的系统时间速度枚举。设置为Paused时和系统暂停表现相同。 </param>
        public void SetCurrentSpeed(TimeSpeed newSpeed)
        {
            SetCurrentSpeed((float)newSpeed);
        }
        
        /// <summary>
        /// 设置新的系统时间速度，直接设置新速度的重载。
        /// </summary>
        /// <remarks>
        /// <list type="table">
        /// <item> <term><paramref name="newSpeed"/>设置小于0.0时</term> <description>函数调用无效。</description> </item>
        /// <item> <term><paramref name="newSpeed"/>设置为0.0时</term> <description>和系统暂停表现相同。</description> </item>
        /// <item> <term><paramref name="newSpeed"/>设置大于0.0时</term> <description>系统正常变速，并且会打断暂停。</description> </item>
        /// </list>
        /// </remarks>
        /// <param name="newSpeed">新的系统时间速度。</param>
        public void SetCurrentSpeed(float newSpeed)
        {
            if (newSpeed == 0.0f)
            {
                SetPause(true);
            }
            else
            {
                SetSpeedWithUnPause(newSpeed, false);
            }
        }

        /// <summary>
        /// 暂停或继续时间。
        /// </summary>
        /// <remarks>
        /// 系统只会在首次调用本函数暂停时，暂存当前的变速。
        /// 系统已经暂停时，再次使用本函数的暂停功能没有任何效果，也不会影响首次暂停时记录的变速倍率。
        /// 继续时间时，可以决定是否恢复为之前的变速。如果继续时不恢复，系统变速将会设为1.0。
        /// </remarks>
        /// <param name="pause">需要暂停或者继续系统。</param>
        /// <param name="resumeSpeed">继续时是否恢复之前的变速，默认为不恢复。此变量在<paramref name="pause"/>为true时无效。</param>
        public void SetPause(bool pause, bool resumeSpeed = false)
        {
            if (pause)
            {
                PauseGame();
            }
            else
            {
                SetSpeedWithUnPause(1.0f, resumeSpeed);
            }
        }
        
        private void PauseGame()
        {
            CachedTimeSpeed = CurrentTimeSpeed;
            CurrentTimeSpeed = 0.0f;
        }

        private void SetSpeedWithUnPause(float newSpeed, bool resumeSpeed)
        {
            //ResumeSpeed 优先级高，内部使用还可以。
            CurrentTimeSpeed = resumeSpeed ? CachedTimeSpeed : newSpeed;
            CachedTimeSpeed = 1.0f;
        }

        /// <summary>
        /// 于当前时间添加时间标记。
        /// </summary>
        /// <remarks>
        /// 时间标记ID不能重复，如果重复，第二次及以后的添加将失败。
        /// </remarks>
        /// <param name="markerID">记录的时间标记ID。</param>
        /// <returns>是否添加成功。</returns>
        public bool AddTimeMarkNow(string markerID)
        {
            if (_timeMarksDic.ContainsKey(markerID))
            {
                return false;
            }
            _timeMarksDic.Add(markerID, CurrentTime);
            return true;
        }
        
        /// <summary>
        /// 根据时间标记ID清除对应标记。
        /// </summary>
        /// <remarks>
        /// 如果目标时间标记ID不存在，那么就会移除失败。
        /// </remarks>
        /// <param name="markerID">记录的时间标记ID。</param>
        /// <returns>是否移除成功。</returns>
        public bool RemoveTimeMarkByID(string markerID)
        {
            if (_timeMarksDic.ContainsKey(markerID))
            {
                _timeMarksDic.Remove(markerID);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 清除全部时间标记。
        /// </summary>
        public void RemoveAllTimeMark()
        {
            _timeMarksDic = new Dictionary<string, float>();
        }
        
        /// <summary>
        /// 查看记录时间标记和当前时间的差距。
        /// </summary>
        /// <remarks>
        /// 如果目标时间标记ID不存在，那么就会查询失败，返回NaN。
        /// </remarks>
        /// <param name="markerID">记录的时间标记ID。</param>
        /// <returns>现在对于记录的时间标记已经过了多久。</returns>
        public float CheckTimeMark(string markerID)
        {
            return _timeMarksDic.TryGetValue(markerID, out var mark) ? CurrentTime - mark : float.NaN;
        }
        
        /// <summary>
        /// 查看记录时间和当前时间的差距,任意时间查询。
        /// </summary>
        /// <param name="recordedTime">记录的时间。</param>
        /// <returns>现在对于记录时间已经经过了多久。</returns>
        public float CheckTime(float recordedTime)
        {
            return CurrentTime - recordedTime;
        }
        
        #region 闹钟相关函数

        /// <summary>
        /// 注册闹钟。
        /// </summary>
        /// <remarks>
        /// 如果注册的ID重复，调用无效，返回失败。
        /// 如果需要注册的时间小于等于当先时间，调用无效，返回失败。<br/>
        /// 如果设为<paramref name="relative"/>设为true，而且<paramref name="time"/>小于等于0.0时，调用无效，返回失败。
        /// </remarks>
        /// <param name="alarmID">注册闹钟的ID。</param>
        /// <param name="time">需要闹钟响动的时间。</param>
        /// <param name="alarmCallBack">闹钟响动的回调，回调变量为闹钟响动的具体时间。</param>
        /// <param name="relative"><paramref name="time"/>为绝对值还是相对值，默认为绝对值。</param>
        /// <returns>闹钟添加是否成功。</returns>
        public bool RegisterAlarm(string alarmID,float time, Action<float> alarmCallBack, bool relative = false)
        {
            var actualTime = time;
            if (relative)
            {
                if (time <= 0.0f)
                {
                    return false;
                }
                actualTime += CurrentTime;
            }
            else
            {
                if (time <= CurrentTime)
                {
                    return false;
                }
                actualTime = time;
            }

            if (_alarmList.ContainsKey(alarmID))
            {
                return false;
            }

            _alarmList[alarmID] = new TimeManagerCallback
            {
                ActualCallback = alarmCallBack,
                Time = actualTime,
            };

            _alarmIDFromNearestToFurthest.Add(alarmID);
            _alarmIDFromNearestToFurthest = 
                _alarmIDFromNearestToFurthest.OrderBy(
                    id => _alarmList[id].Time).ToList();//添加的时候保证这个列表是时间升序排列。
            return true;
        }

        /// <summary>
        /// 注销闹钟回调。
        /// </summary>
        /// <param name="ID">注销闹钟目标ID。</param>
        /// <returns>是否注销成功，如果目标ID不存在，则返回失败。</returns>
        public bool DeregisterAlarmByID(string ID)
        {
            if (_alarmList.ContainsKey(ID))
            {
                _alarmList.Remove(ID);
                _alarmIDFromNearestToFurthest.Remove(ID);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 注销全部闹钟回调。
        /// </summary>
        public void DeregisterAllAlarms()
        {
            _alarmList = new Dictionary<string, TimeManagerCallback>();
            _alarmIDFromNearestToFurthest = new List<string>();
        }
        
        #endregion

        #region 节拍器部分

        /// <summary>
        /// 注册节拍器函数。
        /// </summary>
        /// <remarks>
        /// 如果注册的ID重复，调用无效，返回失败。
        /// 现在节拍器的表现是从注册时开始计时，每达到预期的周期，就会调用一次回调。
        /// 注册即立刻开始计时，但是不会立刻调用CallBack。
        /// </remarks>
        /// <param name="metronomeID">注册节拍器的ID。</param>
        /// <param name="metronomeCallBack">节拍器打拍的回调,回调变量为节拍器打拍的具体时间。</param>
        /// <param name="period">节拍器打拍的间隔枚举。</param>
        /// <returns>节拍器添加成功与否。</returns>
        public bool RegisterMetronomeTick(string metronomeID, Action<float> metronomeCallBack, MetronomePeriod period)
        {
            var duration = period switch
            {
                MetronomePeriod.TenthSeconds => 0.1f,
                MetronomePeriod.HalfSeconds => 0.5f,
                _ => (float)period
            };
            return RegisterMetronomeTick(metronomeID, metronomeCallBack, duration);
        }

        /// <summary>
        /// 注册节拍器函数。这个是直接设置间隔的重载。
        /// </summary>
        /// <remarks>
        /// 如果注册的ID重复，调用无效，返回失败。
        /// 现在节拍器的表现是从注册时开始计时，每达到预期的周期，就会调用一次回调。
        /// 注册即立刻开始计时，但是不会立刻调用CallBack。
        /// 并且，请注意，如果因为加速过高，导致多个节拍调用挤进一帧内的，系统将仅回调一次。但是保证不会丢。
        /// </remarks>
        /// <param name="metronomeID">注册节拍器的ID。</param>
        /// <param name="metronomeCallBack">节拍器打拍的回调,回调变量为节拍器打拍的具体时间。</param>
        /// <param name="duration">节拍器打拍的间隔。如果这个值小于等于0，则调用无效，返回失败。</param>
        /// <returns>节拍器添加成功与否。</returns>
        public bool RegisterMetronomeTick(string metronomeID,Action<float> metronomeCallBack,float duration)
        {
            if (duration <= 0.0f)
            {
                return false;
            }
            
            if (_metronomeTickList.ContainsKey(metronomeID))
            {
                return false;
            }

            _metronomeTickList[metronomeID] = new TimeManagerCallback
            {
                ActualCallback = metronomeCallBack,
                Time = duration,
            };
            _metronomeTickLastTickedList[metronomeID] = CurrentTime;
            
            return true;
        }
        
        /// <summary>
        /// 注销节拍器回调。
        /// </summary>
        /// <param name="metronomeID">目标注销的节拍器回调ID。</param>
        /// <returns>是否注销成功，如果目标ID不存在，则返回失败。</returns>
        public bool DeregisterMetronome(string metronomeID)
        {
            if (_metronomeTickList.ContainsKey(metronomeID))
            {
                _metronomeTickList.Remove(metronomeID);
                _metronomeTickLastTickedList.Remove(metronomeID);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 注销全部节拍器回调。
        /// </summary>
        public void DeregisterAllMetronomes()
        {
            _metronomeTickList = new Dictionary<string, TimeManagerCallback>();
            _metronomeTickLastTickedList = new Dictionary<string, float>();
        }

        #endregion
        
        private void CheckMetronome()
        {
            foreach (var (key, value) in _metronomeTickList)
            {
                var shouldTick = CurrentTime - _metronomeTickLastTickedList[key] > value.Time;
                if (shouldTick)
                {
                    _metronomeTickLastTickedList[key] = CurrentTime;
                    value.ActualCallback?.Invoke(CurrentTime);
                }
            }
        }
        
        private void CheckFirstAlarm()
        {
            if (!_alarmIDFromNearestToFurthest.Any())
            {
                return;
            }
            var firstAlarmID = _alarmIDFromNearestToFurthest[0];
            if (CurrentTime >= _alarmList[firstAlarmID].Time)
            {
                _alarmList[firstAlarmID].ActualCallback?.Invoke(CurrentTime);
                _alarmList.Remove(firstAlarmID);
                _alarmIDFromNearestToFurthest.RemoveAt(0);
                CheckFirstAlarm();
            }
        }

        private void Update()
        {
            CurrentTime += Time.deltaTime * CurrentTimeSpeed;
            if (_alarmList is { Count: > 0 })
            {
                CheckFirstAlarm();
            }
            if (_metronomeTickList is {Count:>0})
            {
                CheckMetronome();
            }
        }
        
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            
            DeregisterAllAlarms();
            DeregisterAllMetronomes();
            RemoveAllTimeMark();
        }
    }
}