﻿

namespace Anycmd.Engine.Host.Edi.MemorySets
{
    using Engine.Edi;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    /// <summary>
    /// 状态码上下文
    /// </summary>
    internal sealed class StateCodes : IStateCodes, IMemorySet
    {
        public static readonly StateCodes Empty = new StateCodes(EmptyAcDomain.SingleInstance);

        private static readonly List<StateCode> List = new List<StateCode>();
        private static bool _initialized = false;
        private static readonly object Locker = new object();
        private readonly Guid _id = Guid.NewGuid();
        private readonly IAcDomain _host;

        public StateCodes(IAcDomain host)
        {
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }
            this._host = host;
        }

        public Guid Id
        {
            get { return _id; }
        }

        /// <summary>
        /// 
        /// </summary>
        internal static void Refresh()
        {
            if (_initialized)
            {
                _initialized = false;
            }
        }

        private void Init()
        {
            if (_initialized) return;
            lock (Locker)
            {
                if (_initialized) return;
                _host.MessageDispatcher.DispatchMessage(new MemorySetInitingEvent(this));
                List.Clear();
                var stateCodeEnumType = typeof(Status);
                var members = stateCodeEnumType.GetFields();
                var list = new List<StateCode>();
                var ok = new StateCode((int)Status.Ok, "Ok", "成功");
                list.Add(ok);

                // 通过反射构建状态码对象
                foreach (var item in members)
                {
                    if (item.DeclaringType == stateCodeEnumType)
                    {
                        var value = (Status)item.GetValue(Status.Ok);
                        if (value != Status.Ok)
                        {
                            var attrs = item.GetCustomAttributes(typeof(DescriptionAttribute), inherit: true);
                            var description = attrs.Length > 0 ? ((DescriptionAttribute)attrs[0]).Description : item.Name;
                            var reasonPhrase = value.ToString();
                            var entity = new StateCode((int)value, reasonPhrase, description);
                            list.Add(entity);
                        }
                    }
                }
                foreach (var item in list.OrderBy(a => a.Code))
                {
                    List.Add(item);
                }
                _initialized = true;
                _host.MessageDispatcher.DispatchMessage(new MemorySetInitializedEvent(this));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerator<StateCode> GetEnumerator()
        {
            if (!_initialized)
            {
                Init();
            }
            return List.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (!_initialized)
            {
                Init();
            }
            return List.GetEnumerator();
        }
    }
}
