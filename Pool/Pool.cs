using System;
using UnityEngine;
using System.Reflection;
using System.Collections.Concurrent;

public abstract class Pool<T>
{
    private protected T Original;
    private protected ConcurrentStack<T> Objects = new ConcurrentStack<T>();

    private protected abstract T CreateObject();

    public T Rent()
    {
        T result;

        if (Objects.TryPop(out result))
            return result;

        return CreateObject();
    }

    public void Return(T item) => Objects.Push(item);

    public void Return(T[] items) => Objects.PushRange(items);
}

public class PoolObject : Pool<GameObject>
{
    public PoolObject(GameObject original, int length)
    {
        Original = original;
        GameObject[] collection = new GameObject[length];

        for (int i = 0; i < length; i++)
            collection[i] = CreateObject();

        Objects = new ConcurrentStack<GameObject>(collection);
    }

    private protected override GameObject CreateObject() 
        => UnityEngine.Object.Instantiate(Original,Original.transform.parent);

}

public class PoolComponent<T> : Pool<T> where T : Component
{
    public PoolComponent(T original, int length)
    {
        Original = original;
        T[] collection = new T[length];

        type = Original.GetType();
        fieldsInfo = type.GetFields();
        fieldValues = new object[fieldsInfo.Length];

        for (int i = 0; i < fieldsInfo.Length; i++)
            fieldValues[i] = fieldsInfo[i].GetValue(Original);

        for (int i = 0; i < length; i++)
            collection[i] = CreateObject();

        Objects = new ConcurrentStack<T>(collection);
    }

    private Type type;
    private FieldInfo[] fieldsInfo;
    private object[] fieldValues;

    private protected override T CreateObject()
    {
        T copy = (T)Original.gameObject.AddComponent(type);

        for (int i = 0; i < fieldsInfo.Length; i++)
            fieldsInfo[i].SetValue(copy, fieldValues[i]);

        return copy;
    }

}
