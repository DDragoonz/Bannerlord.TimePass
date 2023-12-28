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
                InformationManager.DisplayMessage(new InformationMessage(
                    "(Fail to load TimePass Settings!) \n exception : " + e.Message
                    , Colors.Red));
                return TimePassSettings.DefaultSettings;
            }
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
            rainSkyTickTimeInterval = 1.0f
        };

        public static readonly string configPath = Path.Combine(Path.Combine(new string[]
        {
            BasePath.Name,
            "Modules",
            "TimePass",
            "TimePassConfig.xml"
        }));


        private static TimePassSettings _instance;
    }
}