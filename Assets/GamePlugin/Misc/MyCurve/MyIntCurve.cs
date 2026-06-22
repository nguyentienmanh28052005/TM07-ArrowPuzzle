// Assets/Editor/MyIntCurve.cs  (put anywhere under Assets, doesn't need Editor folder)

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class MyIntKeyframe
{
    public int time;
    public int value;

    public MyIntKeyframe()
    {
    }

    public MyIntKeyframe(int t, int v)
    {
        time = t;
        value = v;
    }
}

[Serializable][JsonConverter(typeof(MyIntCurveConverter))]
public class MyIntCurve
{
    public List<int> values = new() { 0, 4 };

    [JsonIgnore]
    public int Length => values.Count;

    public int Evaluate(int t)
    {
        if (values == null || values.Count == 0) return 0;
        if (values.Count == 1) return values[0];

        // clamp t về [0, Length-1]
        t = Mathf.Clamp(t, 0, values.Count - 1);

        return values[t];
    }
}

public class MyIntCurveConverter : JsonConverter<MyIntCurve>
{
    public override void WriteJson(JsonWriter writer, MyIntCurve value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value.values); // chỉ ghi ra list số
    }

    public override MyIntCurve ReadJson(JsonReader reader, Type objectType, MyIntCurve existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var list = serializer.Deserialize<List<int>>(reader);
        return new MyIntCurve { values = list ?? new List<int>() };
    }
}