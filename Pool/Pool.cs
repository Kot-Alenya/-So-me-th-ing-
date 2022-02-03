using System;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

public abstract class Pool<T>
{
    private protected T Original;
    private protected Stack<T> Objects = new Stack<T>();
    //DOTS in the future

    private protected abstract T CreateObject();

    public T Rent()
    {
        if(Objects.Count < 1)
            return CreateObject();

        return Objects.Pop(); 
    }

    public void Return(T item) => Objects.Push(item);
}

public class GameObjectPool : Pool<GameObject>
{
    public GameObjectPool(GameObject original, int length)
    {
        Original = original;
        GameObject[] collection = new GameObject[length];

        for (int i = 0; i < length; i++)
            collection[i] = CreateObject();

        Objects = new Stack<GameObject>(collection);
    }

    private protected override GameObject CreateObject() 
        => UnityEngine.Object.Instantiate(Original,Original.transform.parent);

}

public class ComponentPool<T> : Pool<T> where T : Component
{
    public ComponentPool(T original, int length)
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

        Objects = new Stack<T>(collection);
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
