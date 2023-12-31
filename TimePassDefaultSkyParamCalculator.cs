using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace TimePass
{
    // helper class to calculate default sky param.
    // implementation copied from DefaultMapWeatherModel,
    // some will adjusted to match preset atmosphere data
    public static class TimePassDefaultSkyParamCalculator
    {
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
                return MBMath.Lerp(minSunAltitude, maxSunAltitude, amount, 1E-05f);
            }
            else
            {
                if (normalizedHour >= 0.9166667f)
                {
                    normalizedHour -= 1f;
                }

                float num = (normalizedHour - -0.08333331f) / 0.16666666f;
                num = ((num < 0f) ? 0f : ((num > 1f) ? 1f : num));
                return MBMath.Lerp(maxSunAltitude, minSunAltitude, num, 1E-05f);
            }
        }
        
        public static Vec3 GetFogColor(float environmentMultiplier, bool sunIsMoon)
        {
            Vec3 vec;
            if (!sunIsMoon)
            {
                vec = new Vec3(1f - (1f - environmentMultiplier) / 7f, 0.75f - environmentMultiplier / 4f,
                    0.55f - environmentMultiplier / 5f, -1f);
            }
            else
            {
                vec = new Vec3(1f - environmentMultiplier * 10f, 0.75f + environmentMultiplier * 1.5f,
                    0.65f + environmentMultiplier * 2f, -1f);
                vec = Vec3.Vec3Max(vec, new Vec3(0.55f, 0.59f, 0.6f, -1f));
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
                    0.9f - (1f - MathF.Pow(environmentMultiplier, 0.3f)) / 2.5f, -1f);
            }
            else
            {
                vec = new Vec3(0.85f - MathF.Pow(environmentMultiplier, 0.4f),
                    0.8f - MathF.Pow(environmentMultiplier, 0.5f), 0.8f - MathF.Pow(environmentMultiplier, 0.8f), -1f);
                vec = Vec3.Vec3Max(vec, new Vec3(0.05f, 0.05f, 0.1f, -1f));
            }

            return vec;
        }

        public static void GetSeasonTimeFactorOfCampaignTime(CampaignTime ct, out float timeFactorForSnow, out float
            timeFactorForRain, bool snapCampaignTimeToWeatherPeriod = true)
        {
            if (snapCampaignTimeToWeatherPeriod)
            {
                ct = CampaignTime.Hours((float)((int)(ct.ToHours / 4.0 / 2.0) * 4 * 2));
            }

            float yearProgress = (float)ct.ToSeasons % 4f;
            timeFactorForSnow = CalculateTimeFactorForSnow(yearProgress);
            timeFactorForRain = CalculateTimeFactorForRain(yearProgress);
        }

        public static float CalculateTimeFactorForSnow(float yearProgress)
        {
            float result = 0f;
            if (yearProgress > 1.5f && (double)yearProgress <= 3.5)
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
            if (yearProgress > 1f && (double)yearProgress <= 2.5)
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
                altitude = MBMath.Lerp(minSunAltitude, maxSunAltitude, amount, 1E-05f);
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
                num = ((num < 0f) ? 0f : ((num > 1f) ? 1f : num));
                altitude = MBMath.Lerp(maxSunAltitude, minSunAltitude, num, 1E-05f);
                angle = 50f * seasonFactor;
            }
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

            num = ((num > 1f) ? (2f - num) : num);
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
        
        private static readonly float minSunAltitude = -10f;
        private static readonly float maxSunAltitude = 190f;
    }
}