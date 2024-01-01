using TaleWorlds.Library;

namespace TimePass
{
    public class TimePassVM : ViewModel
    {
        [DataSourceProperty]
        public string TimePassTimeOfDayText
        {
            get
            {
                float currentTimeOfDay = TimePassSkyInfo.GetCurrentTimeOfDay();
                int hour = (int)currentTimeOfDay;
                float hourFraction = currentTimeOfDay - hour;
                int minute = (int)(60 * hourFraction);
                string AM_PM = "" ;
                if (!TimePassSettings.Instance.use24HourFormat)
                {
                    if (hour > 12)
                    {
                        hour %= 12;
                        AM_PM = "PM";
                    }
                    else
                    {
                        AM_PM = "AM";
                    }
                }
                return string.Format("{0}:{1} {2}", hour.ToString("00"),minute.ToString("00"),AM_PM);
            }
            set
            {
            }
        }
    }
}