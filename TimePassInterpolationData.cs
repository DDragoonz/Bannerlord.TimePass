using System;
using System.Collections.Generic;
using TaleWorlds.Library;

namespace TimePass
{
    public class TimePassInterpolationData
    {
        private List<KeyValuePair<float,string>> data = new List<KeyValuePair<float, string>>();

        public bool GetValueForTime(float time, out string valueA, out string valueB, out float alpha)
        {
            alpha = 1;
            
            for(int i = 0; i<data.Count; i++)
            {
                
                if (i + 1 >= data.Count)
                {
                    valueA = data[i].Value;
                    valueB = data[0].Value;
                    alpha = 1;
                    return true;
                    break;
                }
                
                float currentTime = data[i].Key;
                float nextTime = data[i + 1].Key;
                float expectedTime = time;

                if (expectedTime >= currentTime && expectedTime < nextTime)
                {
                    valueA = data[i].Value;
                    valueB = data[i + 1].Value;
                    alpha = (time - currentTime) / (nextTime - currentTime);

                    return true;
                }
                
            }
            valueA = valueB = "";
            return false;
        }

        public string GetValue(float time)
        {
            string valueA, valueB;
            float alpha;
            if (GetValueForTime(time, out valueA, out valueB, out alpha))
            {
                return valueA;
            }

            return "";
        }

        public float GetFloatValue(float time,out float alpha)
        {
            string valueA, valueB;
            if (GetValueForTime(time,out valueA,out valueB, out alpha))
            {
                try
                {
                    float fValueA = float.Parse(valueA);
                    float fValueB = float.Parse(valueB);
                    return MathF.Lerp(fValueA, fValueB, alpha);
                }
                catch (Exception e)
                {
                    // parse failure
                }
                

            }
            alpha = 0;
            return 0;

        }

        public float GetFloatValue(float time)
        {
            float alpha;
            return GetFloatValue(time, out alpha);
        }

        public Vec3 GetVec3Value(float time, out float alpha)
        {
            
            string valueA, valueB;
            if (GetValueForTime(time,out valueA,out valueB, out alpha))
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
            alpha = 0;
            return Vec3.Zero;
            
        }

        public Vec3 GetVec3Value(float time)
        {
            float alpha;
            return GetVec3Value(time, out alpha);
        }

        public void AddData(float key, string value)
        {
            // if (data == null) data = new List<KeyValuePair<float, string>>();
            
            for (int i = 0; i < data.Count; i++)
            {
                if (key == data[i].Key)
                {
                    return;
                }

                if (key < data[i].Key)
                {
                    data.Insert(i, new KeyValuePair<float, string>(key,value));
                    return;
                }
            }
            data.Add(new KeyValuePair<float, string>(key,value));
        }
        
    }

}