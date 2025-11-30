using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using TaleWorlds.Library;

namespace TimePass
{
    public class TimePassInterpolationDataCollection
    {
        
        // these variables were ordered based on atmosphere interpolation data. 

        #region interpolation_element

        public TimePassInterpolationData fog_density = new TimePassInterpolationData();
        public TimePassInterpolationData fog_falloff = new TimePassInterpolationData();
        public TimePassInterpolationData brightpass_exposure = new TimePassInterpolationData();
        public TimePassInterpolationData max_exposure = new TimePassInterpolationData();
        public TimePassInterpolationData middle_grey = new TimePassInterpolationData();
        public TimePassInterpolationData min_exposure = new TimePassInterpolationData();
        public TimePassInterpolationData target_exposure = new TimePassInterpolationData();
        public TimePassInterpolationData sky_brightness = new TimePassInterpolationData();
        public TimePassInterpolationData skybox_rotation = new TimePassInterpolationData();
        public TimePassInterpolationData sun_intesity = new TimePassInterpolationData();
        public TimePassInterpolationData sun_shafts_intesity = new TimePassInterpolationData();
        public TimePassInterpolationData sun_size = new TimePassInterpolationData();
        public TimePassInterpolationData sun_altitude = new TimePassInterpolationData();
        public TimePassInterpolationData sun_angle = new TimePassInterpolationData();

        #endregion

        #region individual_element

        public TimePassInterpolationData sun_color = new TimePassInterpolationData();
        public TimePassInterpolationData fog_color = new TimePassInterpolationData();
        public TimePassInterpolationData fog_ambient_color = new TimePassInterpolationData();
        public TimePassInterpolationData global_ambient_color = new TimePassInterpolationData();
        public TimePassInterpolationData skybox_texture = new TimePassInterpolationData();
        public TimePassInterpolationData colorgrade_texture = new TimePassInterpolationData();

        #endregion

        public bool GetInterpolationDataByName(string name, out TimePassInterpolationData data)
        {
            switch (name)
            {
                case "fog_density":
                    data = fog_density;
                    break;
                case "fog_falloff":
                    data = fog_falloff;
                    break;
                case "brightpass_exposure":
                    data = brightpass_exposure;
                    break;
                case "max_exposure":
                    data = max_exposure;
                    break;
                case "middle_grey":
                    data = middle_grey;
                    break;
                case "min_exposure":
                    data = min_exposure;
                    break;
                case "target_exposure":
                    data = target_exposure;
                    break;
                case "sky_brightness":
                    data = sky_brightness;
                    break;
                case "sun_intesity":
                    data = sun_intesity;
                    break;
                case "sun_size":
                    data = sun_size;
                    break;
                case "sun_shafts_intesity":
                    data = sun_shafts_intesity;
                    break;
                case "sun_altitude":
                    data = sun_altitude;
                    break;
                case "sun_angle":
                    data = sun_angle;
                    break;
                case "sun_color":
                    data = sun_color;
                    break;
                case "fog_color":
                    data = fog_color;
                    break;
                case "fog_ambient_color":
                    data = fog_ambient_color;
                    break;
                case "global_ambient_color":
                    data = global_ambient_color;
                    break;
                case "skybox_texture":
                    data = skybox_texture;
                    break;
                case "skybox_rotation":
                    data = skybox_rotation;
                    break;
                case "colorgrade_texture":
                    data = colorgrade_texture;
                    break;
                default:
                    data = new TimePassInterpolationData();
                    return false;
            }

            return true;
        }

        public static TimePassInterpolationDataCollection GetInterpolationData(string name)
        {
            TimePassInterpolationDataCollection result;
            if (InterpolationDataCache.TryGetValue(name, out result))
            {
                return result;
            }

            result = new TimePassInterpolationDataCollection();

            try
            {
                string path = Path.Combine(BasePath.Name, "Modules", "Native", "Atmospheres", "Interpolated", name + ".xml");

                XmlReaderSettings settings = new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment };
                using (XmlReader xmlReader = XmlReader.Create(path, settings))
                {
                    xmlReader.MoveToContent(); // navigate to <atmo>
                    xmlReader.ReadToFollowing("interpolation_element");

                    do
                    {
                        TimePassInterpolationData data;
                        string interpolation_name = xmlReader.GetAttribute("name");
                        string[] attributeSplit = interpolation_name.Split('#');
                        List<string> ignoredName = new List<string> { "skybox_texture", "colorgrade_texture" };
                        if (attributeSplit.Length >= 2 &&
                            !ignoredName.Contains(attributeSplit[1]) && // we don't want skybox_texture here, we want skybox_texture in later section
                            result.GetInterpolationDataByName(attributeSplit[1], out data))
                        {
                            xmlReader.ReadToFollowing("key_frame");
                            do
                            {
                                // move to "frame"
                                if (!xmlReader.MoveToFirstAttribute())
                                {
                                    continue;
                                }
                                
                                // get the value of "frame" this should be 0, 10, 20... etc
                                float key = float.Parse(xmlReader.Value);
                                // move to next attribute ("value")
                                if (!xmlReader.MoveToNextAttribute())
                                {
                                    continue;
                                }

                                string sValue = xmlReader.Value;
                                string sTangent = "";
                                if (xmlReader.MoveToNextAttribute())
                                {
                                    sTangent = xmlReader.Value;
                                }
                                
                                data.AddData(key, sValue, sTangent);
                                xmlReader.MoveToElement();
                            } while (xmlReader.ReadToNextSibling("key_frame"));

                        }
                    } while (xmlReader.ReadToNextSibling("interpolation_element")); // read <interpolation_element>

                    string[] sections =
                    {
                        "sun_color", "fog_color", "fog_ambient_color", "global_ambient_color", "skybox_texture",
                        "colorgrade_texture"
                    };
                    foreach (string section in sections)
                    {


                        TimePassInterpolationData data;
                        if (!result.GetInterpolationDataByName(section, out data))
                        {
                            continue;
                        }

                        while (!xmlReader.IsStartElement(section))
                        {
                            xmlReader.Read();
                        }

                        xmlReader.ReadToFollowing("key");
                        do
                        {
                            if (!xmlReader.MoveToFirstAttribute())
                            {
                                continue;
                            }

                            float key = float.Parse(xmlReader.Value);
                            if (!xmlReader.MoveToNextAttribute())
                            {
                                continue;
                            }

                            data.AddData(key, xmlReader.Value);
                            xmlReader.MoveToElement();
                        } while (xmlReader.ReadToNextSibling("key"));
                        
                    }
                    
                    InterpolationDataCache.Add(name, result);
                    return result;
                }

            }
            catch (Exception e)
            {
                if (TimePassSettings.Instance.EnableDebug)
                {
                    InformationManager.DisplayMessage(new InformationMessage(
                        "GetInterpolationData(" + name + ")exception : " + e.Message
                        , Colors.Red));
                }
            }

            return null;
        }

        private static readonly Dictionary<string, TimePassInterpolationDataCollection> InterpolationDataCache = new Dictionary<string, TimePassInterpolationDataCollection>();

    }
}