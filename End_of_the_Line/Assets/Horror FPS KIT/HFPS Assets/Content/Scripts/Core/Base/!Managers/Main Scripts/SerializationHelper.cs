namespace ThunderWire.Helpers
{
    public class SerializationHelper : Singleton<SerializationHelper>
    {
        public SerializationSettings serializationSettings;

        public static SerializationSettings Settings
        {
            get
            {
                return Instance.serializationSettings;
            }
        }
    }
}