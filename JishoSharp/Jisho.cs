using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using System.Net.Http;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;



namespace JishoSharp
{

    public enum QueryType { Plain, Tagged};

    public partial class Jisho
    {
        [JsonIgnore()]
        private static HttpClient client = new HttpClient();

        [JsonProperty("meta")]
        public Meta Meta { get; set; }

        [JsonProperty("data")]
        public Datum[] Data { get; set; }

        public static async Task<Jisho> Query(string searchParam, QueryType queryType, uint page = 1)
        {
            try
            {
                string baseURI = "https://www.jisho.org/api/v1/search/words/?keyword=";
                if (queryType == QueryType.Tagged)
                {
                    baseURI += "%23";
                }

                var response = await client.GetAsync("https://www.jisho.org/api/v1/search/words?keyword=%23"+searchParam+"&page="+page.ToString());
                response.EnsureSuccessStatusCode();
                var responseText = await response.Content.ReadAsStringAsync();

                var test = JsonConvert.DeserializeObject<Jisho>(responseText, Converter.Settings);

                if (test.Meta.Status != 200)
                    throw new HttpRequestException(test.Meta.Status.ToString());
                else
                    return test;
           
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine(e);
            }
            
            return new Jisho();
        }

    }

    public partial class Datum
    {
        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("is_common")]
        public bool IsCommon { get; set; }

        [JsonProperty("tags")]
        public string[] Tags { get; set; }

        [JsonProperty("jlpt")]
        public Jlpt[] Jlpt { get; set; }

        [JsonProperty("japanese")]
        public Japanese[] Japanese { get; set; }

        [JsonProperty("senses")]
        public Sense[] Senses { get; set; }

        [JsonProperty("attribution")]
        public Attribution Attribution { get; set; }
    }

    public partial class Attribution
    {
        [JsonProperty("jmdict")]
        public bool Jmdict { get; set; }

        [JsonProperty("jmnedict")]
        public bool Jmnedict { get; set; }

        [JsonProperty("dbpedia")]
        public Uri Dbpedia { get; set; }
    }

    public partial class Japanese
    {
        [JsonProperty("word", NullValueHandling = NullValueHandling.Ignore)]
        public string Word { get; set; }

        [JsonProperty("reading")]
        public string Reading { get; set; }
    }

    public partial class Sense
    {
        [JsonProperty("english_definitions")]
        public string[] EnglishDefinitions { get; set; }

        [JsonProperty("parts_of_speech")]
        public PartsOfSpeech[] PartsOfSpeech { get; set; }

        [JsonProperty("links")]
        public Link[] Links { get; set; }

        [JsonProperty("tags")]
        public string[] Tags { get; set; }

        [JsonProperty("restrictions")]
        public string[] Restrictions { get; set; }

        [JsonProperty("see_also")]
        public string[] SeeAlso { get; set; }

        [JsonProperty("antonyms")]
        public object[] Antonyms { get; set; }

        [JsonProperty("source")]
        public Source[] Source { get; set; }

        [JsonProperty("info")]
        public string[] Info { get; set; }

        [JsonProperty("sentences", NullValueHandling = NullValueHandling.Ignore)]
        public object[] Sentences { get; set; }
    }

    public partial class Link
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }
    }

    public partial class Source
    {
        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("word")]
        public string Word { get; set; }
    }

    public partial class Meta
    {
        [JsonProperty("status")]
        public long Status { get; set; }
    }

    public enum Jlpt { JlptN1, JlptN2, JlptN3, JlptN4, JlptN5 };

    public enum PartsOfSpeech { Counter, NoAdjective, Noun, NounUsedAsASuffix, Place, Suffix, SuruVerb, WikipediaDefinition };

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                JlptConverter.Singleton,
                PartsOfSpeechConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class JlptConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Jlpt) || t == typeof(Jlpt?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "jlpt-n1":
                    return Jlpt.JlptN1;
                case "jlpt-n2":
                    return Jlpt.JlptN2;
                case "jlpt-n3":
                    return Jlpt.JlptN3;
                case "jlpt-n4":
                    return Jlpt.JlptN4;
                case "jlpt-n5":
                    return Jlpt.JlptN5;
            }
            throw new Exception("Cannot unmarshal type Jlpt");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Jlpt)untypedValue;
            switch (value)
            {
                case Jlpt.JlptN1:
                    serializer.Serialize(writer, "jlpt-n1");
                    return;
                case Jlpt.JlptN2:
                    serializer.Serialize(writer, "jlpt-n2");
                    return;
                case Jlpt.JlptN3:
                    serializer.Serialize(writer, "jlpt-n3");
                    return;
                case Jlpt.JlptN4:
                    serializer.Serialize(writer, "jlpt-n4");
                    return;
                case Jlpt.JlptN5:
                    serializer.Serialize(writer, "jlpt-n5");
                    return;
            }
            throw new Exception("Cannot marshal type Jlpt");
        }

        public static readonly JlptConverter Singleton = new JlptConverter();
    }

    internal class PartsOfSpeechConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(PartsOfSpeech) || t == typeof(PartsOfSpeech?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "Counter":
                    return PartsOfSpeech.Counter;
                case "No-adjective":
                    return PartsOfSpeech.NoAdjective;
                case "Noun":
                    return PartsOfSpeech.Noun;
                case "Noun - used as a suffix":
                    return PartsOfSpeech.NounUsedAsASuffix;
                case "Place":
                    return PartsOfSpeech.Place;
                case "Suffix":
                    return PartsOfSpeech.Suffix;
                case "Suru verb":
                    return PartsOfSpeech.SuruVerb;
                case "Wikipedia definition":
                    return PartsOfSpeech.WikipediaDefinition;
            }
            throw new Exception("Cannot unmarshal type PartsOfSpeech");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (PartsOfSpeech)untypedValue;
            switch (value)
            {
                case PartsOfSpeech.Counter:
                    serializer.Serialize(writer, "Counter");
                    return;
                case PartsOfSpeech.NoAdjective:
                    serializer.Serialize(writer, "No-adjective");
                    return;
                case PartsOfSpeech.Noun:
                    serializer.Serialize(writer, "Noun");
                    return;
                case PartsOfSpeech.NounUsedAsASuffix:
                    serializer.Serialize(writer, "Noun - used as a suffix");
                    return;
                case PartsOfSpeech.Place:
                    serializer.Serialize(writer, "Place");
                    return;
                case PartsOfSpeech.Suffix:
                    serializer.Serialize(writer, "Suffix");
                    return;
                case PartsOfSpeech.SuruVerb:
                    serializer.Serialize(writer, "Suru verb");
                    return;
                case PartsOfSpeech.WikipediaDefinition:
                    serializer.Serialize(writer, "Wikipedia definition");
                    return;
            }
            throw new Exception("Cannot marshal type PartsOfSpeech");
        }

        public static readonly PartsOfSpeechConverter Singleton = new PartsOfSpeechConverter();
    }
}




