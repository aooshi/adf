using System;
using System.Collections;

namespace Adf
{
    /// <summary>
    /// ͨ�÷��ͼ��ϻ���
    /// </summary>
    /// <typeparam name="T">Ԫ������</typeparam>
    public abstract class CollectionGeneric<T> : CollectionBase
    {
        /// <summary>
        /// ��ȡָ����������Ԫ��
        /// </summary>
        /// <param name="index">����</param>
        public T this[int index]
        {
            get
            {
                return (T)base.InnerList[index];
            }
        }

        /// <summary>
        /// ���һ����Ԫ��
        /// </summary>
        /// <param name="item">Ԫ��</param>
        public virtual void Add(T item)
        {
            base.InnerList.Add(item);
        }

        /// <summary>
        /// ���һ����Ԫ��
        /// </summary>
        /// <param name="arritem">Ԫ����</param>
        public virtual void AddRange(T[] arritem)
        {
            base.InnerList.AddRange(arritem);
        }

        /// <summary>
        /// �Ƴ�ָ����Ԫ��
        /// </summary>
        /// <param name="item">Ԫ��</param>
        public virtual void Remove(T item)
        {
            base.InnerList.Remove(item);
        }

        /// <summary>
        /// �Ƴ�ָ����������Ԫ��
        /// </summary>
        /// <param name="index">�Ƴ�ָ����������Ԫ��</param>
        public virtual void Remove(int index)
        {
            base.InnerList.RemoveAt(index);
        }

    }
}
