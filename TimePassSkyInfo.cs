using System;
using System.Collections.Generic;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.TwoDimension;

namespace TimePass
{
    public struct TimePassSkyInfo
    {
        public int hour;
        public float skybox_rotation;
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
        public Vec3 fog_ambient_color;


        public void SetValue(string varName, string value)
        {
            switch (varName)
            {
                case "skybox_rotation":
                    skybox_rotation = float.Parse(value);
                    break;
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
                    string[] splitValue = value.Split(',');
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
                    string[] splitValue2 = value.Split(',');
                    fog_ambient_color = new Vec3(float.Parse(splitValue2[0]), float.Parse(splitValue2[1]),
                        float.Parse(splitValue2[2]));
                    break;
                default:
                    break;
            }
        }

        public override string ToString()
        {
            float time = 0;
            if (Campaign.Current != null)
            {
                time = CampaignTime.Now.CurrentHourInDay;
            }
            else if (Mission.Current.Scene != null)
            {
                time = Mission.Current.Scene.TimeOfDay;
            }

            return "time : " + time
                             + "\n skybox_rotation : " + skybox_rotation
                             + "\n sky_brightness : " + sky_brightness
                             + "\n sun_altitude : " + sun_altitude
                             + "\n sun_angle : " + sun_angle
                             + "\n sun_intesity : " + sun_intesity
                             + "\n sun_size : " + sun_size
                             + "\n sun_color : " + sun_color
                             + "\n sunshafts_strength : " + sunshafts_strength
                             + "\n max_exposure : " + max_exposure
                             + "\n min_exposure : " + min_exposure
                             + "\n target_exposure : " + target_exposure
                             + "\n brightpass_threshold : " + brightpass_threshold
                             + "\n fog_ambient_color : " + fog_ambient_color;
        }

        public static TimePassSkyInfo Lerp(TimePassSkyInfo fromSkyInfo, TimePassSkyInfo toSkyInfo, float hourProgress)
        {
            TimePassSkyInfo skyInfo = new TimePassSkyInfo
            {
                skybox_rotation = MBMath.Lerp(fromSkyInfo.skybox_rotation, toSkyInfo.skybox_rotation, hourProgress),
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
                    hourProgress)
            };

            return skyInfo;
        }

        public static string GetAtmosphereName(int currentHour, float rainDensity)
        {
            if (currentHour > 24)
            {
                currentHour %= 24;
            }

            if (currentHour > 12)
            {
                currentHour = 12 - (currentHour - 12);
            }

            // skipping TOD_12_00_SemiCloudy because it somehow has different sun position than the other, causing wrong shadow position
            if (currentHour == 12)
            {
                currentHour = 11;
            }

            if (currentHour == 0)
            {
                currentHour = 1;
            }

            // TODO : handle rain atmosphere
            return "TOD_" + currentHour.ToString("00") + "_00_SemiCloudy";
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

                string path = System.IO.Path.Combine(new string[]
                {
                    BasePath.Name,
                    "Modules",
                    "Native",
                    "Atmospheres",
                    atmosphereName + ".xml"
                });

                using (XmlReader xmlReader = XmlReader.Create(path))
                {
                    xmlReader.MoveToContent();

                    xmlReader.ReadToDescendant("global_ambient");
                    xmlReader.MoveToAttribute("fog_ambient_color");
                    result.SetValue("fog_ambient_color", xmlReader.Value);

                    string[] sections = { "fog", "sun", "postfx" };
                    foreach (string section in sections)
                    {
                        xmlReader.ReadToDescendant(section);
                        while (xmlReader.Read())
                        {
                            if (xmlReader.MoveToAttribute("name"))
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
                if (TimePassSettings.Instance.enableDebug)
                    InformationManager.DisplayMessage(new InformationMessage(
                        "GetOrReadSkyInfo(" + atmosphereName + ")exception : " + e.Message
                        , Colors.Red));
            }

            return result;
        }

        private static Dictionary<string, TimePassSkyInfo> skyInfoCache = new Dictionary<string, TimePassSkyInfo>();
    }
}