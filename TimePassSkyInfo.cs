using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.TwoDimension;

namespace TimePass
{
    public struct TimePassSkyInfo
    {
        public string id;
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

        public bool IsValid()
        {
            return id != null;
        }

        private TimePassSkyInfo(TimePassInterpolationDataCollection data, float currentTimeOfDay)
        {

            float normalizedHour = currentTimeOfDay % 24 / 24;
            float timeFactorForSnow = 1, timeFactorForRain = 1;
            if (Campaign.Current != null)
            {
                TimePassDefaultSkyParamCalculator.GetSeasonTimeFactorOfCampaignTime(CampaignTime.Now,
                    out timeFactorForSnow,
                    out timeFactorForRain);
            }


            float timeOfDayHundred = currentTimeOfDay * 10;

            data.colorgrade_texture.GetValueForTime(currentTimeOfDay * 10, out color_grade_name, out color_grade_name2, out color_grade_alpha);

            hour = (int)currentTimeOfDay;
            id = "interpolated";
            sun_intesity = data.sun_intesity.GetFloatValue(timeOfDayHundred);
            sky_brightness = Mathf.Max(1f, data.sky_brightness.GetFloatValue(timeOfDayHundred));
            sun_size = data.sun_size.GetFloatValue(timeOfDayHundred);
            sunshafts_strength = data.sun_shafts_intesity.GetFloatValue(timeOfDayHundred);
            sun_color = data.sun_color.GetVec3Value(normalizedHour);
            sun_altitude = data.sun_altitude.GetFloatValue(timeOfDayHundred);
            sun_angle = data.sun_angle.GetFloatValue(timeOfDayHundred);
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
                    sun_altitude = /*(hour > 12 && hour < 21) || (hour >= 0 && hour < 2) ? 180 - float.Parse(value) : */float.Parse(value);
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
            output += "id : " + id + " (" + hour + ")";
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


        public static TimePassSkyInfo Lerp(TimePassSkyInfo fromSkyInfo, TimePassSkyInfo toSkyInfo, float hourProgress)
        {
            // float lerpedSunsize = Mathf.Lerp(fromSkyInfo.sun_size, toSkyInfo.sun_size, hourProgress);
            // float clampedSunSize = lerpedSunsize < 0.05f ? 0.05f : lerpedSunsize;
            TimePassSkyInfo skyInfo = new TimePassSkyInfo
            {
                sky_brightness = Mathf.Lerp(fromSkyInfo.sky_brightness, toSkyInfo.sky_brightness, hourProgress),
                sun_altitude = Mathf.Lerp(fromSkyInfo.sun_altitude, toSkyInfo.sun_altitude, hourProgress),
                sun_angle = Mathf.Lerp(fromSkyInfo.sun_angle, toSkyInfo.sun_angle, hourProgress),
                sun_intesity = Mathf.Lerp(fromSkyInfo.sun_intesity, toSkyInfo.sun_intesity, hourProgress),
                sun_size = fromSkyInfo.sun_size, //Mathf.Lerp(fromSkyInfo.sun_size, toSkyInfo.sun_size, hourProgress),
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
                skybox_texture = fromSkyInfo.skybox_texture
            };

            return skyInfo;
        }
        
        public static TimePassSkyInfo GetOrReadSkyInfo(int currentHour, string atmosphereName)
        {
            TimePassSkyInfo result = new TimePassSkyInfo();
            try
            {
                if (skyInfoCache.TryGetValue(atmosphereName, out result))
                {
                    return result;
                }

                string path = Path.Combine(BasePath.Name, "Modules", atmosphereName.Contains("naval") ? "NavalDLC" : "Native", "Atmospheres", atmosphereName + ".xml");

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

                    result.hour = currentHour;
                    result.id = atmosphereName;
                    skyInfoCache.Add(atmosphereName, result);
                }
            }
            catch (Exception e)
            {
                result.id = null;
                if (TimePassSettings.Instance.EnableDebug)
                {
                    InformationManager.DisplayMessage(new InformationMessage(
                        "GetOrReadSkyInfo(" + atmosphereName + ")exception : " + e.Message
                        , Colors.Red));
                }
            }

            return result;
        }

        // 
        public bool UpdateSkyInfo(float timeOfDay, bool isBadWeather, string interpAtmosphere, bool isNaval)
        {
            int currentHour = (int)timeOfDay;
            if (currentHour == hour)
            {
                return false; // do not update if same hour
            }

            if (hour == currentHour)
            {
                return false;
            }
            
            string presetAtmosphereName = GetPresetAtmosphereName(id, currentHour, isBadWeather, isNaval);
            if (TimePassSettings.Instance.IncludeCultureInterpolatedAtmosphere && interpAtmosphere != null && (presetAtmosphereName == null || MBRandom.RandomFloat < 0.5f))
            {
                TimePassInterpolationDataCollection interpolatedAtmosphere = TimePassInterpolationDataCollection.GetInterpolationData(interpAtmosphere);
                if (interpolatedAtmosphere != null)
                {
                    this = new TimePassSkyInfo(interpolatedAtmosphere, timeOfDay);
                    return true;
                }
            }
            
            // if resulting same id, only update the hour
            if (presetAtmosphereName == id || presetAtmosphereName == null)
            {
                hour = currentHour;
                return false;
            }

            this = GetOrReadSkyInfo(currentHour, presetAtmosphereName);
            hour = currentHour;
            return IsValid();

        }

        private static string GetPresetAtmosphereName(string currentAtmosphere, int timeOfDay, bool isBadWeather, bool isNaval)
        {
            List<string> nameCandidates = new List<string>();
            
            if (TimePassSettings.Instance.IncludeOldPresetAtmosphere && (!isNaval || TimePassSettings.Instance.IncludeDefaultPresetAtmosphereInNaval))
            {
                switch (timeOfDay)
                {
                    case 0:
                    case 1:
                    case 23:
                        
                        if (isBadWeather) nameCandidates.Add("TOD_01_00_HeavyRain");
                        else nameCandidates.Add("TOD_01_00_SemiCloudy"); 
                        break;
                    case 2:
                    case 22:
                        nameCandidates.Add("TOD_02_00_SemiCloudy");
                        break;
                    case 3:
                    case 21:
                        nameCandidates.Add("TOD_03_00_SemiCloudy");
                        break;
                    case 4:
                    case 20:
                        nameCandidates.Add("TOD_04_00_SemiCloudy");
                        break;
                    case 5:
                    case 19:
                        nameCandidates.Add("TOD_05_00_SemiCloudy");
                        break;
                    case 6:
                    case 18:
                        if (isBadWeather) nameCandidates.Add("TOD_06_00_Overcast");
                        else
                        {
                            nameCandidates.Add("TOD_06_00_Foggy");
                            nameCandidates.Add("TOD_06_00_SemiCloudy");
                        }
                        break;
                    case 7:
                    case 17:
                        nameCandidates.Add("TOD_07_00_SemiCloudy");
                        break;
                    case 8:
                    case 16:
                        nameCandidates.Add("TOD_08_00_Overcast");
                        nameCandidates.Add("TOD_08_00_SemiCloudy");
                        break;
                    case 9:
                    case 15:
                        nameCandidates.Add("TOD_09_00_SemiCloudy");
                        break;
                    case 10:
                    case 14:
                        nameCandidates.Add("TOD_10_00_SemiCloudy");
                        break;
                    case 11:
                    case 13:
                        nameCandidates.Add("TOD_11_00_SemiCloudy");
                        break;
                    case 12:
                        
                        if (isBadWeather) nameCandidates.Add("TOD_12_00_Overcast");
                        else nameCandidates.Add("TOD_12_00_SemiCloudy");
                        
                        break;
                }
            }
            if (TimePassSettings.Instance.IncludeNewPresetAtmosphere && (!isNaval || TimePassSettings.Instance.IncludeDefaultPresetAtmosphereInNaval))
            {
                switch (timeOfDay)
                {
                    case 4:
                        nameCandidates.Add("TOD_photo_04_00_dawn");
                        break;
                    case 5:
                        nameCandidates.Add("TOD_photo_05_00_sunset");
                        nameCandidates.Add("TOD_photo_05_00_sunset2");
                        break;
                    case 6:
                        nameCandidates.Add("TOD_photo_06_00_sunset");
                        nameCandidates.Add("TOD_photo_06_00_Cloudy");
                        nameCandidates.Add("TOD_photo_06_00_Foggy");
                        break;
                    case 7:
                    case 17:
                        if (isBadWeather) nameCandidates.Add("TOD_photo_07_00_Overcast");
                        else
                        {
                            nameCandidates.Add("TOD_photo_07_00_sunset2");
                            nameCandidates.Add("TOD_photo_07_00_Cloudy");
                        }
                        break;
                    case 8:
                    case 16:
                        if (isBadWeather) nameCandidates.Add("TOD_photo_08_00_rain_storm");
                        else
                        {
                            nameCandidates.Add("TOD_photo_08_00_Cloudy");
                            nameCandidates.Add("TOD_photo_08_00_Cloudy2");
                            nameCandidates.Add("TOD_photo_08_00_Overcast");
                        }
                        break;
                    case 9:
                    case 15:
                        if (isBadWeather) nameCandidates.Add("TOD_photo_09_00_Overcast");
                        else {
                            nameCandidates.Add("TOD_photo_09_00_Cloudy");
                            nameCandidates.Add("TOD_photo_09_00_Cloudy2");
                        }
                        
                        break;
                    case 10:
                    case 14:
                        
                        if (isBadWeather) nameCandidates.Add("TOD_photo_10_00_rain_storm");
                        else
                        {
                            nameCandidates.Add("TOD_photo_10_00_Cloudy");
                            nameCandidates.Add("TOD_photo_10_00_SemiCloudy");
                            nameCandidates.Add("TOD_photo_10_00_semi_cloudy");
                        }
                        break;
                    case 11:
                    case 12:
                    case 13:
                        if (isBadWeather) nameCandidates.Add("TOD_photo_11_00_overcast");
                        else
                        {
                            nameCandidates.Add("TOD_photo_11_00_Cloudy"); 
                            nameCandidates.Add("TOD_photo_11_00_SemiCloudy");
                        }
                        
                        break;
                    case 18:
                    case 19:
                        nameCandidates.Add("TOD_photo_06_00_Cloudy");
                        nameCandidates.Add("TOD_photo_06_00_Foggy");
                        break;
                    case 3:
                    case 21:
                        nameCandidates.Add("TOD_03_00_SemiCloudy");
                        break;
                    case 20:
                        nameCandidates.Add("TOD_04_00_SemiCloudy");
                        break;
                    case 22:
                    case 2:
                        nameCandidates.Add("TOD_02_00_SemiCloudy");
                        break;
                    case 23:
                    case 0:
                    case 1:
                        nameCandidates.Add("TOD_photo_04_00_night");
                        break;
                }
            }
            
            string path = Path.Combine(BasePath.Name, "Modules", "NavalDLC", "Atmospheres");
            if (Directory.Exists(path) && (isNaval || TimePassSettings.Instance.IncludeNavalPresetAtmosphereInDefault))
            {
                switch (timeOfDay)
                {
                    case 0:
                    case 1:
                    case 23:
                        nameCandidates.Add("TOD_naval_01_00_SemiCloudy");
                        break;
                    case 2:
                        nameCandidates.Add("TOD_naval_02_00_SemiCloudy");
                        break;
                    case 22:
                    case 21:
                        nameCandidates.Add("TOD_naval_02_30_night");
                        break;
                    case 3:
                        if (isBadWeather) nameCandidates.Add("TOD_naval_08_00_rain_storm");
                        else nameCandidates.Add("TOD_naval_03_00_sunset");
                        break;
                    case 4:
                        if (isBadWeather) nameCandidates.Add("TOD_naval_08_00_rain_storm");
                        else nameCandidates.Add("TOD_naval_04_00_Cloudy");
                        break;
                    case 20:
                        if (isBadWeather) nameCandidates.Add("TOD_naval_08_00_rain_storm");
                        else nameCandidates.Add("TOD_naval_04_30_Cloudy");
                        break;
                    case 5:
                        if (isBadWeather) nameCandidates.Add("TOD_naval_08_00_rain_storm");
                        else
                        {
                            nameCandidates.Add("TOD_naval_05_00_sunset");
                            nameCandidates.Add("TOD_naval_05_30_sunset");
                        }

                        break;
                    case 19:
                        if (isBadWeather) nameCandidates.Add("TOD_naval_08_00_rain_storm");
                        else nameCandidates.Add("TOD_naval_05_30_Cloudy");
                        break;
                    case 6:
                        if (isBadWeather) nameCandidates.Add("TOD_naval_08_00_rain_storm");
                        else
                        {
                            nameCandidates.Add("TOD_naval_06_00_sunset");
                            nameCandidates.Add("TOD_naval_06_30_sunset");
                        }

                        break;
                    case 18:
                        if (isBadWeather) nameCandidates.Add("TOD_naval_08_00_rain_storm");
                        else
                        {
                            nameCandidates.Add("TOD_naval_06_00_Cloudy");
                            nameCandidates.Add("TOD_naval_06_00_Foggy");
                        }
                        break;
                    case 7:
                        if (isBadWeather) nameCandidates.Add("TOD_naval_08_00_rain_storm");
                        else nameCandidates.Add("TOD_naval_07_00_Cloudy");
                        break;
                    case 17:
                        if (isBadWeather) nameCandidates.Add("TOD_naval_08_00_rain_storm");
                        else nameCandidates.Add("TOD_naval_07_00_sunset2");
                        break;
                    case 8:
                        if (isBadWeather) nameCandidates.Add("TOD_naval_08_00_rain_storm");
                        else
                        {
                            nameCandidates.Add("TOD_naval_08_00_Cloudy2");
                            nameCandidates.Add("TOD_naval_08_00_Overcast");   
                        }
                        break;
                    case 16:
                        if (isBadWeather) nameCandidates.Add("TOD_naval_08_00_rain_storm");
                        else nameCandidates.Add("TOD_naval_08_00_Cloudy");
                        break;
                    case 9:
                        if (isBadWeather) nameCandidates.Add("TOD_naval_09_00_Overcast");
                        else nameCandidates.Add("TOD_naval_09_00_Cloudy2"); 
                        break;
                    case 15:
                        if (isBadWeather) nameCandidates.Add("TOD_naval_09_00_Overcast");
                        else nameCandidates.Add("TOD_naval_09_00_Cloudy");
                        break;
                    case 10:
                        if (isBadWeather) nameCandidates.Add("TOD_naval_10_00_rain_storm");
                        else nameCandidates.Add("TOD_naval_10_00_SemiCloudy");
                        break;
                    case 14:
                        if (isBadWeather) nameCandidates.Add("TOD_naval_10_00_rain_storm");
                        else nameCandidates.Add("TOD_naval_10_00_Cloudy");
                        break;
                    case 11:
                        if (isBadWeather) nameCandidates.Add("TOD_naval_11_00_Overcast");
                        else nameCandidates.Add("TOD_naval_11_00_Cloudy");
                        break;
                    case 13:
                        if (isBadWeather) nameCandidates.Add("TOD_naval_11_00_Overcast");
                        else nameCandidates.Add("TOD_naval_11_00_SemiCloudy");
                        break;
                    case 12:
                        if (isBadWeather) nameCandidates.Add("TOD_naval_11_00_Overcast");
                        else nameCandidates.Add("TOD_naval_12_00_SemiCloudy");
                        break;
                }
                
            }

            nameCandidates.Remove(currentAtmosphere);
            return nameCandidates.Count == 0 ? null : nameCandidates.GetRandomElement();
        }

        private static readonly Dictionary<string, TimePassSkyInfo> skyInfoCache = new Dictionary<string, TimePassSkyInfo>();

    }
}