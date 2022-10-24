using Newtonsoft.Json.Linq;

namespace HFPS.Systems
{
    public interface IJsonListener
    {
        void OnJsonChanged(JObject root);
    }
}