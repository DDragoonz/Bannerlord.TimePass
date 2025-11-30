using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace TimePass
{
    // helper class to calculate default sky param.
    // implementation copied from DefaultMapWeatherModel,
    // some will adjusted to match preset atmosphere data
    public static class TimePassDefaultSkyParamCalculator
    {

        private static readonly float minSunAltitude = -10f;
        private static readonly float maxSunAltitude = 190f;

        public static float GetFogDensity(float environmentMultiplier, float height, bool sunIsMoon)
        {
            float num = sunIsMoon ? 0.5f : 0.4f;
            float num2 = 1f - environmentMultiplier;
            float num3 = 1f - MBMath.ClampFloat((height - 30f) / 200f, 0f, 0.9f);
            return MathF.Min((0f + num * num2) * num3, 10f);
        }

        public static float CalculateSunAltitude(float normalizedHour)
        {
            if (normalizedHour >= 0.083333336f && normalizedHour < 0.9166667f)
            {
                float amount = (normalizedHour - 0.083333336f) / 0.8333334f;
                return MBMath.Lerp(minSunAltitude, maxSunAltitude, amount);
            }
            if (normalizedHour >= 0.9166667f)
            {
                normalizedHour -= 1f;
            }

            float num = (normalizedHour - -0.08333331f) / 0.16666666f;
            num = num < 0f ? 0f : num > 1f ? 1f : num;
            return MBMath.Lerp(maxSunAltitude, minSunAltitude, num);
        }

        public static Vec3 GetFogColor(float environmentMultiplier, bool sunIsMoon)
        {
            Vec3 vec;
            if (!sunIsMoon)
            {
                vec = new Vec3(1f - (1f - environmentMultiplier) / 7f, 0.75f - environmentMultiplier / 4f,
                    0.55f - environmentMultiplier / 5f);
            }
            else
            {
                vec = new Vec3(1f - environmentMultiplier * 10f, 0.75f + environmentMultiplier * 1.5f,
                    0.65f + environmentMultiplier * 2f);
                vec = Vec3.Vec3Max(vec, new Vec3(0.55f, 0.59f, 0.6f));
            }

            return vec;
        }

        public static float GetSunBrightness(float environmentMultiplier, bool sunIsMoon, bool forceDay = false)
        {
            float num;
            if (!sunIsMoon || forceDay)
            {
                num = MathF.Sin(MathF.Pow((environmentMultiplier - 0.001f) / 0.999f, 1.2f) * 1.5707964f) * 85f;
                // num = MathF.Min(MathF.Max(num, 0.2f), 35f);
            }
            else
            {
                num = 0.2f;
            }

            return num;
        }

        public static float GetSunSize(float envMultiplier)
        {
            return 0.1f + (1f - envMultiplier) / 8f;
        }

        public static Vec3 GetSunColor(float environmentMultiplier, bool sunIsMoon)
        {
            Vec3 vec;
            if (!sunIsMoon)
            {
                vec = new Vec3(1f, 1f - (1f - MathF.Pow(environmentMultiplier, 0.3f)) / 2f,
                    0.9f - (1f - MathF.Pow(environmentMultiplier, 0.3f)) / 2.5f);
            }
            else
            {
                vec = new Vec3(0.85f - MathF.Pow(environmentMultiplier, 0.4f),
                    0.8f - MathF.Pow(environmentMultiplier, 0.5f), 0.8f - MathF.Pow(environmentMultiplier, 0.8f));
                vec = Vec3.Vec3Max(vec, new Vec3(0.05f, 0.05f, 0.1f));
            }

            return vec;
        }

        public static void GetSeasonTimeFactorOfCampaignTime(CampaignTime ct, out float timeFactorForSnow, out float
            timeFactorForRain, bool snapCampaignTimeToWeatherPeriod = true)
        {
            if (snapCampaignTimeToWeatherPeriod)
            {
                ct = CampaignTime.Hours((int)(ct.ToHours / 4.0 / 2.0) * 4 * 2);
            }

            float yearProgress = (float)ct.ToSeasons % 4f;
            timeFactorForSnow = CalculateTimeFactorForSnow(yearProgress);
            timeFactorForRain = CalculateTimeFactorForRain(yearProgress);
        }

        public static float CalculateTimeFactorForSnow(float yearProgress)
        {
            float result = 0f;
            if (yearProgress > 1.5f && yearProgress <= 3.5)
            {
                result = MBMath.Map(yearProgress, 1.5f, 3.5f, 0f, 1f);
            }
            else if (yearProgress <= 1.5f)
            {
                result = MBMath.Map(yearProgress, 0f, 1.5f, 0.75f, 0f);
            }
            else if (yearProgress > 3.5f)
            {
                result = MBMath.Map(yearProgress, 3.5f, 4f, 1f, 0.75f);
            }

            return result;
        }

        public static float CalculateTimeFactorForRain(float yearProgress)
        {
            float result = 0f;
            if (yearProgress > 1f && yearProgress <= 2.5)
            {
                result = MBMath.Map(yearProgress, 1f, 2.5f, 0f, 1f);
            }
            else if (yearProgress <= 1f)
            {
                result = MBMath.Map(yearProgress, 0f, 1f, 1f, 0f);
            }
            else if (yearProgress > 2.5f)
            {
                result = 1f;
            }

            return result;
        }

        public static void GetSunPosition(float hourNorm, float seasonFactor, out float altitude, out float angle, out
            bool sunIsMoon)
        {
            if (hourNorm >= 0.083333336f && hourNorm < 0.9166667f)
            {
                sunIsMoon = false;
                float amount = (hourNorm - 0.083333336f) / 0.8333334f;
                altitude = MBMath.Lerp(minSunAltitude, maxSunAltitude, amount);
                angle = 50f * seasonFactor;
            }
            else
            {
                sunIsMoon = true;
                if (hourNorm >= 0.9166667f)
                {
                    hourNorm -= 1f;
                }

                float num = (hourNorm - -0.08333331f) / 0.16666666f;
                num = num < 0f ? 0f : num > 1f ? 1f : num;
                altitude = MBMath.Lerp(maxSunAltitude, minSunAltitude, num);
                angle = 50f * seasonFactor;
            }
        }
        
        

        public static float GetSunAltitude(float timeOfDay)
        {
            float timeAlpha = timeOfDay - (int)timeOfDay;
            if (timeOfDay > 0 && timeOfDay <= 2.3f) return HermiteEvaluate(90.0f, 180.08f, -11.952f, 0.0f, timeOfDay / 2.3f);
            if (timeOfDay > 2.3f && timeOfDay <= 3.0f) return HermiteEvaluate(0.08f, 1.5f, 0.0f, -0.398f, timeOfDay - 2.3f / 3.0f - 2.3f);
            if (timeOfDay > 3.0f && timeOfDay <= 4.0f) return HermiteEvaluate(1.5f, 4.972f, 0.568f, -0.703f, timeAlpha);
            if (timeOfDay > 4.0f && timeOfDay <= 5.0f) return HermiteEvaluate(4.972f, 9.108f, 0.703f, -0.791f, timeAlpha);
            if (timeOfDay > 5.0f && timeOfDay <= 6.0f) return HermiteEvaluate(9.108f, 13.826f, 0.791f, -0.911f, timeAlpha);
            if (timeOfDay > 6.0f && timeOfDay <= 7.0f) return HermiteEvaluate(13.826f, 20.0f, 0.911f, -1.486f, timeAlpha);
            if (timeOfDay > 7.0f && timeOfDay <= 8.0f) return HermiteEvaluate(20.0f, 32.496f, 1.486f, -1.604f, timeAlpha);
            if (timeOfDay > 8.0f && timeOfDay <= 9.0f) return HermiteEvaluate(32.496f, 46.082f, 1.604f, -1.604f, timeAlpha);
            if (timeOfDay > 9.0f && timeOfDay <= 10.0f) return HermiteEvaluate(46.082f, 59.258f, 1.604f, -1.580f, timeAlpha);
            if (timeOfDay > 10.0f && timeOfDay <= 11.0f) return HermiteEvaluate(59.258f, 72.0f, 1.580f, -1.532f, timeAlpha);
            if (timeOfDay > 11.0f && timeOfDay <= 12.0f) return HermiteEvaluate(72.0f, 82.0f, 1.532f, 0.0f, timeAlpha);
            if (timeOfDay > 12.0f && timeOfDay <= 13.0f) return HermiteEvaluate(82.0f, 180-72.0f, 0.0f, 0.792f, timeAlpha);
            if (timeOfDay > 13.0f && timeOfDay <= 14.0f) return HermiteEvaluate(180-72.0f, 180-59.258f, -0.792f, 1.847f, timeAlpha);
            if (timeOfDay > 14.0f && timeOfDay <= 15.0f) return HermiteEvaluate(180-59.258f, 180-45.895f, -1.847f, 1.979f, timeAlpha);
            if (timeOfDay > 15.0f && timeOfDay <= 16.0f) return HermiteEvaluate(180-45.895f, 180-32.496f, -1.979f, 1.451f, timeAlpha);
            if (timeOfDay > 16.0f && timeOfDay <= 17.0f) return HermiteEvaluate(180-32.496f, 180-20.0f, -1.451f, 2.111f, timeAlpha);
            if (timeOfDay > 17.0f && timeOfDay <= 18.0f) return HermiteEvaluate(180-20.0f, 180-13.826f, -2.111f, 1.055f, timeAlpha);
            if (timeOfDay > 18.0f && timeOfDay <= 19.0f) return HermiteEvaluate(180-13.826f, 180-9.108f, -1.055f, 0.792f, timeAlpha);
            if (timeOfDay > 19.0f && timeOfDay <= 20.0f) return HermiteEvaluate(180-9.108f, 180-4.972f, -0.792f, 0.792f, timeAlpha);
            if (timeOfDay > 20.0f && timeOfDay <= 21.0f) return HermiteEvaluate(180-4.972f, 180-1.5f, -0.792f, 0.463f, timeAlpha);
            if (timeOfDay > 21.0f && timeOfDay <= 21.7f) return HermiteEvaluate(180-1.5f, 180, -0.324f, -0.086f, timeOfDay - 21.0f / 21.7f - 21.0f);
            if (timeOfDay > 21.7f && timeOfDay <= 24.0f) return HermiteEvaluate(0, 90.0f, 0.0f, 0.0f, timeOfDay - 21.7f / 24.0f - 21.7f); 

            return 0;
        }
        

        
        public static float HermiteEvaluate(float value1, float value2, float tangent1, float tangent2, float alpha)
        {
            float t = alpha;
            float t2 = t * t;
            float t3 = t2 * t;

            float h00 = 2f * t3 - 3f * t2 + 1f;
            float h10 = t3 - 2f * t2 + t;
            float h01 = -2f * t3 + 3f * t2;
            float h11 = t3 - t2;

            return h00 * value1 + h10 * tangent1 + h01 * value2 + h11 * tangent2;
        }

        public static float GetEnvironmentMultiplier(float altitude, float angle, bool sunIsMoon)
        {
            float num;
            if (sunIsMoon)
            {
                num = altitude / 180f * 2f;
            }
            else
            {
                num = altitude / 180f * 2f;
            }

            num = num > 1f ? 2f - num : num;
            num = MathF.Pow(num, 0.5f);
            float num2 = 1f - 0.011111111f * angle;
            float num3 = MBMath.ClampFloat(num * num2, 0f, 1f);
            return MBMath.ClampFloat(MathF.Min(MathF.Sin(num3 * num3) * 2f, 1f), 0f, 1f) * 0.999f + 0.001f;
        }

        public static float GetSkyBrightness(float hourNorm, float envMultiplier, bool sunIsMoon)
        {
            float x = (envMultiplier - 0.001f) / 0.999f;
            float num;
            if (!sunIsMoon)
            {
                num = MathF.Sin(MathF.Pow(x, 1.3f) * 1.5707964f) * 80f;
                num -= 1f;
                num = /*MathF.Min(*/MathF.Max(num, 0.055f) /*, 25f)*/;
            }
            else
            {
                num = 0.055f;
            }

            return num;
        }

        public static float GetEnvironmentMultiplier(int currentTime, int initialTime)
        {
            if (initialTime >= 21 || initialTime <= 3)
            {
                return 1;
            }
            switch (currentTime)
            {
                case 0:
                case 23:
                    return 0.0001f;
                case 1:
                case 2:
                case 22:
                    return 0.005f;
                case 3:
                case 21:
                    return 0.01f;
                case 4:
                case 20:
                    return 0.05f;
                case 5:
                case 19:
                    return 0.175f;
                case 6:
                case 18:
                    return 0.2f;
                case 7:
                case 17:
                    return 0.25f;
                case 8:
                case 16:
                    return 0.3f;
                case 9:
                case 15:
                    return 0.5f;
                case 10:
                case 14:
                    return 0.8f;
                case 11:
                case 13:
                    return 1;
            }

            return 1;
        }

    }
}