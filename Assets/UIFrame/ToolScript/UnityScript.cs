using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UnityScript
{
    public class ObjectPool<T> where T : new()
    {
        private readonly Stack<T> m_Stack = new Stack<T>();

        private readonly UnityAction<T> m_ActionOnGet;

        private readonly UnityAction<T> m_ActionOnRelease;

        public int countAll {
            get;
            private set;
        }

        public int countActive {
            get {
                return this.countAll - this.countInactive;
            }
        }

        public int countInactive {
            get {
                return this.m_Stack.Count;
            }
        }

        public ObjectPool(UnityAction<T> actionOnGet, UnityAction<T> actionOnRelease)
        {
            this.m_ActionOnGet = actionOnGet;
            this.m_ActionOnRelease = actionOnRelease;
        }

        public T Get()
        {
            T t;
            if (this.m_Stack.Count == 0) {
                t = System.Activator.CreateInstance<T>();
                this.countAll++;
            } else {
                t = this.m_Stack.Pop();
            }
            if (this.m_ActionOnGet != null) {
                this.m_ActionOnGet(t);
            }
            return t;
        }

        public void Release(T element)
        {
            if (this.m_Stack.Count > 0 && object.ReferenceEquals(this.m_Stack.Peek(), element)) {
                Debug.LogError("Internal error. Trying to destroy object that is already released to pool.");
            }
            if (this.m_ActionOnRelease != null) {
                this.m_ActionOnRelease(element);
            }
            this.m_Stack.Push(element);
        }
    }

    public static class ListPool<T>
    {
        private static readonly ObjectPool<List<T>> s_ListPool = new ObjectPool<List<T>>(null, delegate (List<T> l) {
            l.Clear();
        });

        public static List<T> Get()
        {
            return ListPool<T>.s_ListPool.Get();
        }

        public static void Release(List<T> toRelease)
        {
            ListPool<T>.s_ListPool.Release(toRelease);
        }
    }
}