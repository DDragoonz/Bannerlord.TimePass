using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using TaleWorlds.Library;

namespace TimePass
{
    public class TimePassSettings
    {
        //settings
        public bool enableDebug;
        public bool disableSkyUpdate;
        public float realSecondToWorldSecondRatio;
        public float skyTickTimeInterval;
        public float rainSkyTickTimeInterval;
        
        // UI settings
        public bool displayTime;
        public bool use24HourFormat;

        // advanced settings
        public bool usePerCultureAtmosphereSettings;
        public bool updateColorGrade;
        public bool updateSkyBrightness;
        public bool updateSunColorAndIntensity;
        public bool updateSunShaft;
        public bool updateSunSize;
        public bool updateExposure;
        public bool updateBrightpassThreshold;
        public bool updateMiddleGray;
        public bool updateAmbientColor;
        public bool updateFog;
        

        public static TimePassSettings LoadSettings()
        {
            TimePassSettings result;
            try
            {

                using (XmlReader xmlReader = XmlReader.Create(configPath))
                {
                    xmlReader.MoveToContent();
                    result = (TimePassSettings)new XmlSerializer(typeof(TimePassSettings), new XmlRootAttribute
                    {
                        ElementName = xmlReader.Name
                    }).Deserialize(xmlReader);
                }

                return result;
            }
            catch (Exception e)
            {
                if (e is DirectoryNotFoundException || e is FileNotFoundException)
                {
                    try
                    {
                        using (XmlReader xmlReader = XmlReader.Create(steamWorkShopConfigPath))
                        {
                            xmlReader.MoveToContent();
                            result = (TimePassSettings)new XmlSerializer(typeof(TimePassSettings), new XmlRootAttribute
                            {
                                ElementName = xmlReader.Name
                            }).Deserialize(xmlReader);
                        }

                        return result;
                    }
                    catch (Exception e2)
                    {
                        InformationManager.DisplayMessage(new InformationMessage(
                            "(Fail to load TimePass Settings!) \n exception : " + e2.Message
                            , Colors.Red));
                        
                    }
                }
                else
                {
                    InformationManager.DisplayMessage(new InformationMessage(
                        "(Fail to load TimePass Settings!) \n exception : " + e.Message
                        , Colors.Red));
                }
                
            }
            
            return TimePassSettings.DefaultSettings;
        }

        public static TimePassSettings Instance
        {
            get
            {
                bool flag = TimePassSettings._instance == null;
                if (flag)
                {
                    TimePassSettings._instance = TimePassSettings.DefaultSettings;
                }

                return TimePassSettings._instance;
            }
            set { TimePassSettings._instance = (value ?? TimePassSettings.DefaultSettings); }
        }

        public static TimePassSettings DefaultSettings = new TimePassSettings()
        {
            enableDebug = false,
            disableSkyUpdate = false,
            realSecondToWorldSecondRatio = 60,
            skyTickTimeInterval = 0.1f,
            rainSkyTickTimeInterval = 1.0f,
            displayTime = true,
            use24HourFormat = false,
            usePerCultureAtmosphereSettings = true, 
            updateColorGrade = true,
            updateSkyBrightness = true,
            updateSunColorAndIntensity = true,
            updateSunShaft = true,
            updateSunSize = false,
            updateExposure = true,
            updateBrightpassThreshold = true,
            updateMiddleGray = true,
            updateAmbientColor = false,
            updateFog = true,
            
        };

        public static readonly string configPath = Path.Combine(Path.Combine(new string[]
        {
            BasePath.Name,
            "Modules",
            "TimePass",
            "TimePassConfig.xml"
        }));

        public static readonly string steamWorkShopConfigPath = "../../../../workshop/content/261550/3129438112/TimePassConfig.xml"; 


        private static TimePassSettings _instance;
    }
}