using System;
using System.Collections.Generic;
using TaleWorlds.Library;

namespace TimePass
{
    public class TimePassInterpolationData
    {
        private struct InterpolationData
        {
            public readonly float key;
            public readonly string sValue;
            public List<float> tangents;

            public InterpolationData(float key, string value)
            {
                this.key = key;
                sValue = value;
                tangents = new List<float>();
            }
            
            public InterpolationData(float key, string value, float tangent)
            {
                this.key = key;
                sValue = value;
                tangents = new List<float> { tangent };
            }

            public float GetTangents(bool inTangent)
            {
                if (tangents.Count == 0)
                {
                    return 0;
                }
                return inTangent ? tangents[0] : tangents[tangents.Count - 1];
            }
        }

        private readonly List<InterpolationData> data = new List<InterpolationData>();

        public bool GetValueForTime(float time, out string valueA, out string valueB, out float alpha, out float tangentA, out float tangentB)
        {
            alpha = 1;

            for (int i = 0; i < data.Count; i++)
            {
                if (i + 1 >= data.Count)
                {
                    valueA = data[i].sValue;
                    valueB = data[0].sValue;
                    alpha = 1;
                    tangentA = data[i].GetTangents(false);
                    tangentB = data[0].GetTangents(true);
                    return true;
                }

                float currentTime = data[i].key;
                float nextTime = data[i + 1].key;
                float expectedTime = time;

                if (expectedTime >= currentTime && expectedTime < nextTime)
                {
                    valueA = data[i].sValue;
                    valueB = data[i + 1].sValue;
                    alpha = (time - currentTime) / (nextTime - currentTime);
                    tangentA = data[i].GetTangents(false);
                    tangentB = data[i + 1].GetTangents(true);
                    return true;
                }
            }

            valueA = valueB = "";
            tangentA = tangentB = 0;
            return false;
        }

        public bool GetValueForTime(float time, out string valueA, out string valueB, out float alpha)
        {
            return GetValueForTime(time, out valueA, out valueB, out alpha, out _, out _);
        }

        public string GetValue(float time)
        {
            return GetValueForTime(time, out string valueA, out string _, out float _, out float _, out float _) ? valueA : "";
        }

        public float GetFloatValue(float time)
        {
            if (GetValueForTime(time, out string valueA, out string valueB, out float alpha, out float tangentA, out float tangentB))
            {
                float.TryParse(valueA, out float fValueA);
                float.TryParse(valueB, out float fValueB);
                return TimePassDefaultSkyParamCalculator.HermiteEvaluate(fValueA, fValueB, tangentA, tangentB, alpha);
            }

            return 0;
        }

        public Vec3 GetVec3Value(float time)
        {
            if (GetValueForTime(time, out string valueA, out string valueB, out float alpha))
            {
                try
                {
                    string[] splitValueA = valueA.Split(',');
                    Vec3 v3ValueA = new Vec3(float.Parse(splitValueA[0]), float.Parse(splitValueA[1]),
                        float.Parse(splitValueA[2]));
                    string[] splitValueB = valueB.Split(',');
                    Vec3 v3ValueB = new Vec3(float.Parse(splitValueB[0]), float.Parse(splitValueB[1]),
                        float.Parse(splitValueB[2]));

                    return Vec3.Lerp(v3ValueA, v3ValueB, alpha);
                }
                catch (Exception e)
                {
                    //parse failure
                }
            }
            return Vec3.Zero;
        }

        public void AddData(float key, string value)
        {
            for (int i = 0; i < data.Count; i++)
            {
                if (Math.Abs(key - data[i].key) < 0.001f)
                {
                    return;
                }

                if (key < data[i].key)
                {
                    data.Insert(i, new InterpolationData(key, value));
                    return;
                }
            }

            data.Add(new InterpolationData(key, value));
        }

        public void AddData(float key, string value, string sTangent)
        {
            // if (data == null) data = new List<KeyValuePair<float, string>>();
            
            float.TryParse(sTangent, out float tangent);

            for (int i = 0; i < data.Count; i++)
            {
                if (Math.Abs(key - data[i].key) < 0.001f)
                {
                    data[i].tangents.Add(tangent);
                    return;
                }

                if (key < data[i].key)
                {
                    data.Insert(i, new InterpolationData(key,value));
                    return;
                }

            }

            data.Add(new InterpolationData(key, value, tangent));
        }
        
        
    }
}