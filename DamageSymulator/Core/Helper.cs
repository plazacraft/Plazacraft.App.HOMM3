using System.IO;
using System.Xml.Serialization;

namespace Plazacraft.HOMM3.DamageSymulator.Core
{

    public static class Helper
    {

        public static T DeserializeXML<T>(TextReader value)
            where T : class
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            T data = null;
            data = (T)serializer.Deserialize(value);
            return data;
        }

        public static T DeserializeXML<T>(string value)
            where T: class
        {
            using (TextReader sr = new StringReader(value))
            {
                return DeserializeXML<T>(sr);
            }
        }


    }
}