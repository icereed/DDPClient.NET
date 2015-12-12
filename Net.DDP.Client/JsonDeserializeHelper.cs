using System.Collections.Generic;
using System.Globalization;
using System.Dynamic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Net.DDP.Client
{
    internal class JsonDeserializeHelper : IDeserializer
    {
        private readonly IDataSubscriber _subscriber;

        public JsonDeserializeHelper(IDataSubscriber subscriber)
        {
            _subscriber = subscriber;
        }

        public void Deserialize(string item)
        {
            JObject jObj = JObject.Parse(item);

            if (jObj[DDPClient.DdpPropsError] != null || jObj[DDPClient.DdpPropsMessage] != null && jObj[DDPClient.DdpPropsMessage].ToString() == "error")
                HandleError(jObj[DDPClient.DdpPropsError] ?? jObj);
            else if (jObj[DDPClient.DdpPropsSession] != null)
                HandleConnected(jObj);
            else if (jObj[DDPClient.DdpPropsMessage] != null)
                HandleSubResult(jObj);
            else if (jObj[DDPClient.DdpPropsResult] != null)
                HandleMethodResult(jObj);
        }

        private void HandleConnected(JObject jObj)
        {
            dynamic entity = new ExpandoObject();
            entity.Session = jObj[DDPClient.DdpPropsSession].ToString();
            entity.Type = DDPType.Connected;

            _subscriber.DataReceived(entity);
        }

        private void HandleError(JToken jError)
        {
            dynamic entity = new ExpandoObject();
            entity.Type = DDPType.Error;
            entity.Error = jError["reason"].ToString();

            if (jError["error"] != null)
                entity.Code = jError["error"];
            if (jError["message"] != null)
                entity.Message = jError["message"];
        }

        private void HandleMethodResult(JObject jObj)
        {
            dynamic entity = new ExpandoObject();
            entity.Type = DDPType.MethodResult;
            entity.RequestingId = jObj["id"].ToString();
            entity.Result = jObj[DDPClient.DdpPropsResult].ToString();
            _subscriber.DataReceived(entity);
        }

        private void HandleSubResult(JObject jObj)
        {
            dynamic entity = new ExpandoObject();

            switch (jObj[DDPClient.DdpPropsMessage].ToString())
            {
                case DDPClient.DdpMessageTypeAdded:
                    entity = GetMessageData(jObj);

                    entity.Type = DDPType.Added;
                    break;
                case DDPClient.DdpMessageTypeChanged:
                    entity = GetMessageData(jObj);
                    entity.Type = DDPType.Changed;
                    break;
                case DDPClient.DdpMessageTypeNosub:
                    HandleError(jObj[DDPClient.DdpPropsError]);
                    break;
                case DDPClient.DdpMessageTypeReady:
                    entity.RequestsIds = ((JArray) jObj[DDPClient.DdpPropsSubs]).Select(id => id.Value<int>()).ToArray();
                    entity.Type = DDPType.Ready;
                    break;
                case DDPClient.DdpMessageTypeRemoved:
                    entity = GetMessageData(jObj);
                    entity.Type = DDPType.Removed;
                    break;
            }

            _subscriber.DataReceived(entity);
        }

        private dynamic GetMessageData(JObject json)
        {
            var tmp = (JObject)json[DDPClient.DdpPropsFields];
            dynamic entity = GetMessageDataRecursive(tmp);
            entity.Id = json[DDPClient.DdpPropsId].ToString();
            entity.Collection = json[DDPClient.DdpPropsCollection].ToString();
            
            return entity;
        }

        private dynamic GetMessageDataRecursive(JObject json)
        {
            dynamic entity = new ExpandoObject();
            var entityAsCollection = (IDictionary<string, object>) entity;

            if (json == null) return entityAsCollection;

            foreach (var item in json)
            {
                string propertyName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(item.Key);
                if (item.Value is JObject) // Property is an object
                    entityAsCollection.Add(propertyName, GetMessageDataRecursive((JObject) item.Value));
                else if (item.Value is JArray) // Property is an array...
                {
                    JArray collection = (JArray) item.Value;
                    if (collection.Count == 0)
                        continue;
                    if (collection[0] is JObject) // ... of objects
                    {
                        var entityCollection =
                            (from JObject colObj in collection select GetMessageDataRecursive(colObj)).ToList();

                        entityAsCollection.Add(propertyName, entityCollection);
                    }
                    else // ... of strings
                    {
                        var strColl = collection.Select(colToken => colToken.ToString()).ToList();

                        entityAsCollection.Add(propertyName, strColl);
                    }
                }
                else // Property is a string
                    entityAsCollection.Add(propertyName, item.Value.ToString());
            }

            return entityAsCollection;
        }
    }
}
