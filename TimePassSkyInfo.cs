using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.TwoDimension;

namespace TimePass
{
    public struct TimePassSkyInfo
    {
        public int hour;
        public float sun_altitude;
        public float sun_angle;
        public float sun_intesity;
        public float sky_brightness;
        public float sun_size;
        public float sunshafts_strength;
        public Vec3 sun_color;
        public float max_exposure;
        public float min_exposure;
        public float target_exposure;
        public float brightpass_threshold;
        public string color_grade_name;
        public string color_grade_name2;
        public float color_grade_alpha;
        public Vec3 fog_ambient_color;
        public Vec3 fog_color;
        public float fog_density;
        public float fog_falloff;
        public float middle_gray;
        public string skybox_texture;

        private TimePassSkyInfo(TimePassInterpolationDataCollection data, float currentTimeOfDay)
        {

            float normalizedHour = currentTimeOfDay % 24 / 24;
            bool isSunMoon;
            float timeFactorForSnow = 1, timeFactorForRain = 1;
            if (Campaign.Current != null)
            {
                TimePassDefaultSkyParamCalculator.GetSeasonTimeFactorOfCampaignTime(CampaignTime.Now,
                    out timeFactorForSnow,
                    out timeFactorForRain);
            }

            // get sun position from this function is smoother than lerping between sky info
            TimePassDefaultSkyParamCalculator.GetSunPosition(normalizedHour, timeFactorForSnow,
                out sun_altitude, out sun_angle, out isSunMoon);


            float timeOfDayHundred = currentTimeOfDay * 10;

            data.colorgrade_texture.GetValueForTime(currentTimeOfDay * 10, out color_grade_name, out color_grade_name2, out color_grade_alpha);

            hour = (int)currentTimeOfDay;
            sun_intesity = data.sun_intesity.GetFloatValue(timeOfDayHundred);
            sky_brightness = data.sky_brightness.GetFloatValue(timeOfDayHundred);
            sun_size = data.sun_size.GetFloatValue(timeOfDayHundred);
            sunshafts_strength = data.sun_shafts_intesity.GetFloatValue(timeOfDayHundred);
            sun_color = data.sun_color.GetVec3Value(normalizedHour);
            middle_gray = data.middle_grey.GetFloatValue(timeOfDayHundred);
            brightpass_threshold = data.brightpass_exposure.GetFloatValue(timeOfDayHundred);
            fog_ambient_color = data.global_ambient_color.GetVec3Value(normalizedHour);
            fog_color = data.fog_color.GetVec3Value(normalizedHour);
            fog_density = data.fog_density.GetFloatValue(timeOfDayHundred);
            fog_falloff = data.fog_falloff.GetFloatValue(timeOfDayHundred);
            skybox_texture = data.skybox_texture.GetValue(timeOfDayHundred);

            max_exposure = data.max_exposure.GetFloatValue(timeOfDayHundred);
            min_exposure = data.min_exposure.GetFloatValue(timeOfDayHundred);
            target_exposure = data.target_exposure.GetFloatValue(timeOfDayHundred);
        }

        public void SetValue(string varName, string value)
        {
            string[] splitValue = value.Split(',');
            switch (varName)
            {
                case "sun_altitude":
                    // add 90 altitude past mid day time
                    sun_altitude = float.Parse(value) + (hour > 12 ? 90 : 0);
                    break;
                case "sun_angle":
                    sun_angle = float.Parse(value);
                    break;
                case "sun_intesity":
                    sun_intesity = float.Parse(value);
                    break;
                case "sky_brightness":
                    sky_brightness = float.Parse(value);
                    break;
                case "sun_size":
                    sun_size = float.Parse(value);
                    break;
                case "sunshafts_strength":
                    sunshafts_strength = float.Parse(value);
                    break;
                case "sun_color":
                    sun_color = new Vec3(float.Parse(splitValue[0]), float.Parse(splitValue[1]),
                        float.Parse(splitValue[2]));
                    break;
                case "max_exposure":
                    max_exposure = float.Parse(value);
                    break;
                case "min_exposure":
                    min_exposure = float.Parse(value);
                    break;
                case "target_exposure":
                    target_exposure = float.Parse(value);
                    break;
                case "brightpass_threshold":
                    brightpass_threshold = float.Parse(value);
                    break;
                case "fog_ambient_color":
                    fog_ambient_color = new Vec3(float.Parse(splitValue[0]), float.Parse(splitValue[1]),
                        float.Parse(splitValue[2]));
                    break;
                case "fog_color":
                    fog_color = new Vec3(float.Parse(splitValue[0]), float.Parse(splitValue[1]),
                        float.Parse(splitValue[2]));
                    break;
                case "fog_density":
                    fog_density = float.Parse(value);
                    break;
                case "fog_falloff":
                    fog_falloff = float.Parse(value);
                    break;
                case "color_grade_name":
                    color_grade_name = value;
                    break;
                case "middle_gray":
                    middle_gray = float.Parse(value);
                    break;
                case "skybox_texture":
                    skybox_texture = value;
                    break;
                case "skybox_background_texture_name":
                    skybox_texture = value;
                    break;
            }
        }

        public override string ToString()
        {
            string output = "";
            output += "time : " + TimeOfDay;
            if (TimePassSettings.Instance.UpdateColorGrade) output += "\n color_grade_name : " + color_grade_name;
            if (TimePassSettings.Instance.UpdateSkybox) output += "\n skybox_texture : " + skybox_texture;
            if (TimePassSettings.Instance.UpdateSkyBrightness) output += "\n sky_brightness : " + sky_brightness;
            output += "\n sun_altitude : " + sun_altitude;
            if (TimePassSettings.Instance.UpdateSunColorAndIntensity)
            {
                output += "\n sun_color : " + sun_color;
                output += "\n sun_intesity : " + sun_intesity;
            }
            if (TimePassSettings.Instance.UpdateSunShaft) output += "\n sunshafts_strength : " + sunshafts_strength;
            if (TimePassSettings.Instance.UpdateExposure)
            {
                output += "\n max_exposure : " + max_exposure;
                output += "\n min_exposure : " + min_exposure;
                output += "\n target_exposure : " + target_exposure;
            }
            if (TimePassSettings.Instance.UpdateMiddleGray) output += "\n middle_gray : " + middle_gray;
            if (TimePassSettings.Instance.UpdateBrightpassThreshold) output += "\n brightpass_threshold : " + brightpass_threshold;
            if (TimePassSettings.Instance.UpdateFog) output += "\n fog_color : " + fog_color;
            if (TimePassSettings.Instance.UpdateAmbientColor) output += "\n fog_ambient_color : " + fog_ambient_color;
            return output;

        }

        public static void Init(float timeOfDay, bool isBadWeather, string interpAtmosphere = null)
        {
            TimeOfDay = timeOfDay;

            if (interpAtmosphere != null)
            {
                TimePassInterpolationDataCollection data = TimePassInterpolationDataCollection.GetInterpolationData(interpAtmosphere);
                if (data != null)
                {
                    initialSkyInfo = new TimePassSkyInfo(data, timeOfDay);
                    return;
                }

            }

            int atmosphereHour = ClampAtmosphereHour((int)timeOfDay);
            string atmosphereName = GetAtmosphereName(atmosphereHour, isBadWeather);
            initialSkyInfo = GetOrReadSkyInfo((int)timeOfDay, atmosphereName);

        }


        public static TimePassSkyInfo Lerp(TimePassSkyInfo fromSkyInfo, TimePassSkyInfo toSkyInfo, float hourProgress)
        {
            float lerpedSunsize = Mathf.Lerp(fromSkyInfo.sun_size, toSkyInfo.sun_size, hourProgress);
            float clampedSunSize = lerpedSunsize < 0.05f ? 0.05f : lerpedSunsize;
            TimePassSkyInfo skyInfo = new TimePassSkyInfo
            {
                sky_brightness = Mathf.Lerp(fromSkyInfo.sky_brightness, toSkyInfo.sky_brightness, hourProgress),
                sun_altitude = Mathf.Lerp(fromSkyInfo.sun_altitude, toSkyInfo.sun_altitude,
                    hourProgress),
                sun_angle = Mathf.Lerp(fromSkyInfo.sun_angle, toSkyInfo.sun_angle, hourProgress),
                sun_intesity = Mathf.Lerp(fromSkyInfo.sun_intesity, toSkyInfo.sun_intesity, hourProgress),
                sun_size = Mathf.Lerp(fromSkyInfo.sun_size, toSkyInfo.sun_size, hourProgress),
                sunshafts_strength = Mathf.Lerp(fromSkyInfo.sunshafts_strength, toSkyInfo.sunshafts_strength,
                    hourProgress),
                sun_color = Vec3.Lerp(fromSkyInfo.sun_color, toSkyInfo.sun_color, hourProgress),
                fog_ambient_color = Vec3.Lerp(fromSkyInfo.fog_ambient_color, toSkyInfo.fog_ambient_color, hourProgress),
                max_exposure = Mathf.Lerp(fromSkyInfo.max_exposure, toSkyInfo.max_exposure, hourProgress),
                min_exposure = Mathf.Lerp(fromSkyInfo.min_exposure, toSkyInfo.min_exposure, hourProgress),
                target_exposure = Mathf.Lerp(fromSkyInfo.target_exposure, toSkyInfo.target_exposure, hourProgress),
                brightpass_threshold = Mathf.Lerp(fromSkyInfo.brightpass_threshold, toSkyInfo.brightpass_threshold,
                    hourProgress),
                color_grade_name = fromSkyInfo.color_grade_name,
                color_grade_name2 = toSkyInfo.color_grade_name,
                color_grade_alpha = hourProgress,
                fog_color = Vec3.Lerp(fromSkyInfo.fog_color, toSkyInfo.fog_color, hourProgress),
                fog_density = Mathf.Lerp(fromSkyInfo.fog_density, toSkyInfo.fog_density, hourProgress),
                fog_falloff = Mathf.Lerp(fromSkyInfo.fog_falloff, toSkyInfo.fog_falloff, hourProgress),
                middle_gray = Mathf.Lerp(fromSkyInfo.middle_gray, toSkyInfo.middle_gray, hourProgress),
                skybox_texture = toSkyInfo.skybox_texture

            };

            return skyInfo;
        }

        public static string GetAtmosphereName(int hour, bool isBadWeather)
        {
            hour = ClampAtmosphereHour(hour);
            string suffix = "_00_SemiCloudy";

            // only use overcast atmosphere when hour not 1.
            // there are TOD_01_00_HeavyRain, but it's unfortunately it could be too dark to be playable.
            if (isBadWeather && hour != 1)
            {
                suffix = "_00_Overcast";
            }

            return "TOD_" + hour.ToString("00") + suffix;

        }

        // this is used to get atmosphere file
        public static int ClampAtmosphereHour(int hour)
        {
            if (hour > 24)
            {
                hour %= 24;
            }

            if (hour > 12)
            {
                hour = 12 - (hour - 12);
            }

            if (hour == 0)
            {
                hour = 1;
            }

            return hour;
        }

        public static int GetUnclampedAtmosphereHour(int currentHour, bool isBadWeather)
        {

            if (isBadWeather)
            {
                if (currentHour < 6) return 1;
                if (currentHour < 8) return 6;
                if (currentHour < 12) return 8;
                if (currentHour < 16) return 12;
                if (currentHour < 18) return 16;
                if (currentHour < 23) return 18;
            }

            return currentHour;
        }

        // get next hour where atmosphere will change
        public static int GetUnclampedNextAtmosphereHour(int currentHour, bool isBadWeather)
        {
            if (currentHour == 23)
            {
                return 1;
            }

            if (isBadWeather)
            {
                if (currentHour < 1) return 1;
                if (currentHour < 6) return 6;
                if (currentHour < 8) return 8;
                if (currentHour < 12) return 12;
                if (currentHour < 16) return 16;
                if (currentHour < 18) return 18;
                if (currentHour < 23) return 23;
            }

            return currentHour + 1;

        }


        public static TimePassSkyInfo GetOrReadSkyInfo(int currentHour, string atmosphereName)
        {
            TimePassSkyInfo result = new TimePassSkyInfo();
            result.hour = currentHour;
            try
            {
                if (skyInfoCache.TryGetValue(atmosphereName, out result))
                {
                    return result;
                }

                string path = Path.Combine(BasePath.Name, "Modules", "Native", "Atmospheres", atmosphereName + ".xml");

                using (XmlReader xmlReader = XmlReader.Create(path))
                {
                    xmlReader.MoveToContent();

                    string[] sections = { "values", "global_ambient", "fog", "sun", "postfx" };
                    foreach (string section in sections)
                    {
                        xmlReader.ReadToDescendant(section);
                        while (xmlReader.Read())
                        {

                            if (section == "global_ambient")
                            {
                                xmlReader.MoveToAttribute("fog_ambient_color");
                                result.SetValue("fog_ambient_color", xmlReader.Value);
                            }
                            else if (xmlReader.MoveToAttribute("name"))
                            {
                                string varName = xmlReader.Value;
                                xmlReader.MoveToAttribute("value");
                                string value = xmlReader.Value;
                                result.SetValue(varName, value);
                            }
                        }
                    }


                    skyInfoCache.Add(atmosphereName, result);
                }
            }
            catch (Exception e)
            {
                if (TimePassSettings.Instance.EnableDebug)
                {
                    InformationManager.DisplayMessage(new InformationMessage(
                        "GetOrReadSkyInfo(" + atmosphereName + ")exception : " + e.Message
                        , Colors.Red));
                }
            }

            return result;
        }

        public static TimePassSkyInfo GetTargetSkyInfo(float timeOfDay, bool isBadWeather, string interpAtmosphere = null)
        {

            if (TimePassSettings.Instance.UsePerCultureAtmosphereSettings && interpAtmosphere != null)
            {
                TimePassInterpolationDataCollection data =
                    TimePassInterpolationDataCollection.GetInterpolationData(interpAtmosphere);

                if (data != null)
                {
                    return new TimePassSkyInfo(data, timeOfDay);
                }
            }

            // if there is no interpolation data, interpolate using preset TOD_Atmosphere
            float normalizedHour = timeOfDay % 24 / 24;
            bool isSunMoon;
            float timeFactorForSnow = 1, timeFactorForRain = 1;
            float sun_altitude, sun_angle;
            if (Campaign.Current != null)
            {
                TimePassDefaultSkyParamCalculator.GetSeasonTimeFactorOfCampaignTime(CampaignTime.Now,
                    out timeFactorForSnow,
                    out timeFactorForRain);
            }

            // get sun position from this function is smoother than lerping between sky info
            TimePassDefaultSkyParamCalculator.GetSunPosition(normalizedHour, timeFactorForSnow,
                out sun_altitude, out sun_angle, out isSunMoon);


            int currentHour = (int)timeOfDay;
            int unclampedAtmosphereHour = GetUnclampedAtmosphereHour(currentHour, isBadWeather);
            int atmosphereHour = ClampAtmosphereHour(unclampedAtmosphereHour);

            // update whole atmosphere once in an hour
            string atmosphereName = GetAtmosphereName(atmosphereHour, isBadWeather);
            TimePassSkyInfo skyInfo = GetOrReadSkyInfo(currentHour, atmosphereName);


            // lerp sun position every tick
            int unclampedNextAtmosphereHour = GetUnclampedNextAtmosphereHour(currentHour, isBadWeather);
            int nextAtmosphereHour = ClampAtmosphereHour(unclampedNextAtmosphereHour);
            int timeDiff = Math.Abs(unclampedNextAtmosphereHour - unclampedAtmosphereHour);
            float hourProgress = TimeOfDay - unclampedAtmosphereHour;
            // if atmosphere changed
            if (timeDiff > 0)
            {
                hourProgress /= timeDiff;
            }

            string nextAtmosphereName = GetAtmosphereName(nextAtmosphereHour, isBadWeather);
            TimePassSkyInfo nextSkyInfo = GetOrReadSkyInfo(unclampedNextAtmosphereHour, nextAtmosphereName);
            TimePassSkyInfo result = Lerp(skyInfo, nextSkyInfo, hourProgress);
            result.sun_altitude = sun_altitude;
            result.sun_angle = sun_angle;

            return result;

        }

        public void ClampTo(TimePassSkyInfo timePassSkyInfo)
        {
            float minSkyBrightness = Mathf.Min(timePassSkyInfo.sky_brightness, TimePassSettings.Instance.MinimumSkyBrightness);

            sky_brightness = Mathf.Max(minSkyBrightness, sky_brightness);
            min_exposure = Mathf.Min(timePassSkyInfo.min_exposure, min_exposure);
            target_exposure = Mathf.Min(timePassSkyInfo.target_exposure, target_exposure);
        }

        public void ClampToInitialSkyInfo()
        {
            ClampTo(initialSkyInfo);
        }


        private static readonly Dictionary<string, TimePassSkyInfo> skyInfoCache = new Dictionary<string, TimePassSkyInfo>();
        private static TimePassSkyInfo initialSkyInfo;

        public static float TimeOfDay;



    }
}