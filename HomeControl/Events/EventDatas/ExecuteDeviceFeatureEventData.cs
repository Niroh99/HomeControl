namespace HomeControl.Events.EventDatas
{
    public class ExecuteDeviceFeatureEventData : EventData
    {
        public int DeviceId { get; set; }

        public string FeatureName { get; set; }
    }
}