using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Jose
{
    public class DataContractMapper : IJsonMapper
    {
        public string Serialize(object obj)
        {
            var stream = new MemoryStream();
            if (obj is IDictionary<string, object>)
            {
                WriteDictionary(obj, stream);
            }
            else
            {
                var serializer = new DataContractJsonSerializer(obj.GetType());
                serializer.WriteObject(stream, obj);
            }
            stream.Position = 0;
            var jsonStringReader = new StreamReader(stream,Encoding.UTF8);
            var json = jsonStringReader.ReadToEnd();
            return json;
        }

        private static void WriteDictionary(object obj, MemoryStream stream)
        {
            var dic = (IDictionary<string, object>) obj;
            var xmlDictionaryWriter = JsonReaderWriterFactory.CreateJsonWriter(stream);
            xmlDictionaryWriter.WriteStartDocument();
            WriteJsonObj(xmlDictionaryWriter,"root", dic);
            xmlDictionaryWriter.WriteEndDocument();
            xmlDictionaryWriter.Flush();
        }

        private static void WriteJsonObj(
            XmlDictionaryWriter xmlDictionaryWriter, 
            string fieldName, 
            IDictionary<string, object> dic)
        {
            xmlDictionaryWriter.WriteStartElement(fieldName);
            xmlDictionaryWriter.WriteAttributeString("type", "object");
            foreach (var fieldValue in dic)
            {
                WriteElement(xmlDictionaryWriter, fieldValue.Key, fieldValue.Value);
            }
            xmlDictionaryWriter.WriteEndElement();
        }

        private static void WriteElement(XmlDictionaryWriter xmlDictionaryWriter, string key, object value)
        {
            if (value is IDictionary<string,object>)
            {
                WriteJsonObj(xmlDictionaryWriter, key, (IDictionary<string, object>)value);
            }
            else if (value is int)
            {
                xmlDictionaryWriter.WriteStartElement(key);
                xmlDictionaryWriter.WriteAttributeString("type","number");
                xmlDictionaryWriter.WriteString(value.ToString());
                xmlDictionaryWriter.WriteEndElement();
            }
            else
            {
                xmlDictionaryWriter.WriteElementString(key,value.ToString());
            }
        }

        public T Parse<T>(string json)
        {
            if (typeof(T) == typeof(IDictionary<string, object>) || typeof(T) == typeof(Dictionary<string, object>))
            {
                object result = ParseJsonArgumentsObjects(json);
                return (T)result;
            }
            return RawParse<T>(json);
        }

        private static T RawParse<T>(string json)
        {
            var serializer = new DataContractJsonSerializer(typeof (T));
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            T result = (T) serializer.ReadObject(stream);
            return result;
        }
        public static Dictionary<string, object> ParseJsonArgumentsObjects(string jsonParam)
        {

            XmlReader reader =
                JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(jsonParam), new XmlDictionaryReaderQuotas());
            XElement root = XElement.Load(reader);

            return XmlStructureToMap(root);
        }

        private static Dictionary<string, object> XmlStructureToMap(XElement elementToConvert)
        {
            return elementToConvert.Elements().ToDictionary(e => e.Name.LocalName, XmlValueToJson, StringComparer.OrdinalIgnoreCase);
        }

        private static object XmlValueToJson(XElement xElement)
        {
            if (xElement.HasElements)
            {
                return XmlStructureToMap(xElement);
            }
            else
            {
                var value = xElement.Value;
                long asNumber;
                if (long.TryParse(value, out asNumber))
                {
                    if (asNumber < int.MaxValue)
                    {
                        return (int) asNumber;
                    }
                    return asNumber;;
                }
                return xElement.Value;
            }
        }
    }
}