using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using System.Net.Http;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Net;

namespace JishoSharp
{

    /// <summary>
    ///  Wraps returns from Jisho with some helper functions, etc.
    /// </summary>
    public class Jisho
    {
        /// <summary>
        /// HTML client used to get JSON query.
        /// </summary>
        [JsonIgnore()]
        private static readonly HttpClient client = new HttpClient();
        /// <summary>
        /// Represents a range of queried and valid pages between First and Last.
        /// </summary>
        public (uint First, uint Last) PageRange { get; private set; }
        /// <summary>
        /// A list of non-empty queries
        /// </summary>
        public List<JishoQuery> Pages { get; private set; }
        public int DatumLength { get; private set; }

        public Jisho()
        {
            DatumLength = 0;
            Pages = new List<JishoQuery>();
            PageRange = (1, 1);
            client.BaseAddress = new Uri("https://jisho.org/api/v1/search/words?keyword=");
        }


        /// <summary>
        /// Used to directly index into the data of a query
        /// </summary>
        /// <param name="key">The index </param>
        /// <returns></returns>
        private Datum GetDatum(int key)
        {
            var datumIDX = key - 1 % 20;
            try
            {
                Console.WriteLine("Key=" + ((key - 1) - (20 * datumIDX)));
                return Pages[datumIDX].Data[(key - 1) - (20 * datumIDX)];
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw e;
            }
        }
        // TODO: Create test for this
        public Datum this[int key]
        {
            get => GetDatum(key);
        }


        /// <summary>
        /// Queries a single page worth (20 entries) of results
        /// </summary>
        /// <param name="searchParam">Search string</param>
        /// <param name="queryType">Determines whether or not searchParam is a tag or plaintext</param>
        /// <param name="page">Which page to retrieve</param>
        /// <returns>Task<JishoQuery></returns>
        public static async Task<JishoQuery> Query(string searchParam, QueryType queryType, uint page = 1)
        {
            try
            {
                string URITag = "";
                if (queryType == QueryType.Tagged)
                {
                    // poundsign 
                    URITag += "%23";
                }
                Console.WriteLine("Trying query at page = " + page);
                var response = await client.GetAsync(client.BaseAddress + URITag + searchParam + "&page=" + page.ToString());
                response.EnsureSuccessStatusCode();
                var responseText = await response.Content.ReadAsStringAsync();

                var test = JsonConvert.DeserializeObject<JishoQuery>(responseText, Converter.Settings);

                if (test.Meta.Status == 200)
                {
                    return test;
                }
                else
                {
                    throw new HttpRequestException(test.Meta.Status.ToString());
                }

            }
            catch (HttpRequestException e)
            {
                Console.WriteLine(e);
            }

            return new JishoQuery();
        }

        /// <summary>
        /// Queries multiple pages and stores them internally 
        /// </summary>
        /// <param name="searchParam">Search string</param>
        /// <param name="queryType">Determines whether or not searchParam is a tag or plaintext</param>
        /// <param name="startPage">Which page to start pagination from</param>
        /// <param name="endPage">Which page to end pagination at</param>
        public async Task QueryPages(string searchParam, QueryType queryType, uint startPage, uint endPage)
        {
            for (var i = startPage - 1; i <= endPage - 1; i++)
            {
                var page = await Query(searchParam, queryType, i + 1);
                // If page contains data, then add it
                if (page.Data.Length > 0)
                {
                    Pages.Add(page);
                }
                else
                {
                    // Return early if we reached the end of query results
                    PageRange = (startPage, i + 1);
                    return;
                }
            }
            PageRange = (startPage, endPage);
        }

        // TODO: Document this better
        /// <summary>
        /// Overload for querypages to allow for a query for range (1, unknown)
        /// </summary>
        /// <param name="searchParam"></param>
        /// <param name="queryType"></param>
        /// <param name="startPage"></param>
        /// <returns></returns>
        public async Task QueryPages(string searchParam, QueryType queryType, uint startPage)
        {
            for (var i = startPage - 1; ; i++)
            {
                var page = await Query(searchParam, queryType, i + 1);
                // If page contains data, then add it
                if (page.Data.Length > 0)
                {
                    Pages.Add(page);
                }
                else
                {
                    // Return early if we reached the end of query results
                    PageRange = (startPage, i + 1);
                    return;
                }
            }
        }

        /// <summary>
        /// Get page as in index from internal cached Data, with page indexing starts at 1.
        /// </summary>
        /// <param name="page">Page number to access</param>
        /// <returns>JishoQuery</returns>
        public JishoQuery GetPage(uint page)
        {
            if (page < PageRange.First || page > PageRange.Last)
            {
                throw new IndexOutOfRangeException("Page = " + page + " outside of page range");
            }
            else
            {

                var offset = (PageRange.Last) - (PageRange.First) - 1;
                if (offset < 0)
                {
                    throw new Exception("Unexpected: offset less than zero in JishoQuery.Get");
                }

                try
                {
                    return Pages[(int)page - (int)PageRange.First];
                }
                catch
                {
                    throw new Exception("Error in GetPage: index=" + ((int)page - (int)offset));
                }
            }
        }

        // TOOD: Webscrape jisho for detailed information on a given word
        public void GetDetailedInfo(Datum word)
        {

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://jisho.org/search/%E5%B7%9D%20%23kanji");
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

            if (resp.StatusCode == HttpStatusCode.OK)
            {
                // scrape
            }

        }
    }

    /// <summary>
    /// Represents types of queries, such as using a tag, etc.
    /// </summary>
    public enum QueryType { Plain, Tagged};
    
    /// <summary>
    /// Abstracted representation of a JSON query.
    /// </summary>
    public partial class JishoQuery
    {
        
        [JsonProperty("meta")]
        public Meta Meta { get; set; }

        [JsonProperty("data")]
        public Datum[] Data { get; set; }

        /// <summary>
        /// Checks for value equality between two JishoQueries.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        // TODO: use 'is-typecasting'
        // if (obj is JishoQuery otherObject)
        public override bool Equals(object obj)
        {
            try
            {
                var other = (JishoQuery)obj;
                if (other.Data.Length == this.Data.Length && other.Meta.Status == this.Meta.Status)
                {
                    for (int i = 0; i < Data.Length; i++)
                    {
                        var slug = Data[i].Slug == other.Data[i].Slug;
                        // NOTE: If for some reason two words share the same writing, kanji and all
                        //       then add more bools here to check
                        if (slug)
                            return true;
                    }
                }
            }
            catch (Exception e)
            {
                    Console.WriteLine(e);
                    throw e;
            }
            return false;
        }
    }


        

    // TODO: implement Equals override for JishoQuery Equals
    public partial class Datum
    {
        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("is_common")]
        public bool IsCommon { get; set; }

        [JsonProperty("tags")]
        public string[] Tags { get; set; }

        [JsonProperty("jlpt")]
        public JLPT[] Jlpt { get; set; }

        [JsonProperty("japanese")]
        public Japanese[] Japanese { get; set; }

        [JsonProperty("senses")]
        public Sense[] Senses { get; set; }

        [JsonProperty("attribution")]
        public Attribution Attribution { get; set; }

            public void AsWord()
            { 

            }
    }

    public partial class Attribution
    {
        [JsonProperty("jmdict")]
        public bool Jmdict { get; set; }

        [JsonProperty("jmnedict")]
        public bool Jmnedict { get; set; }

        [JsonProperty("dbpedia")]
        public Dbpedia Dbpedia { get; set; }
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
        public string[] PartsOfSpeech { get; set; }

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

    // TODO: change namespace names to just N5, N4 etc
    public enum JLPT { N1 = 1, N2, N3, N4, N5 };
    public enum Tag { Archaism, HonorificOrRespectfulSonkeigo, LinguisticsTerminology, UsuallyWrittenUsingKanaAlone, KansaiDialect };
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
                DbpediaConverter.Singleton,
                TagConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class JlptConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(JLPT) || t == typeof(JLPT?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "jlpt-n1":
                    return JLPT.N1;
                case "jlpt-n2":
                    return JLPT.N2;
                case "jlpt-n3":
                    return JLPT.N3;
                case "jlpt-n4":
                    return JLPT.N4;
                case "jlpt-n5":
                    return JLPT.N5;
            }
            throw new Exception("Cannot unmarshal type JLPT");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (JLPT)untypedValue;
            switch (value)
            {
                case JLPT.N1:
                    serializer.Serialize(writer, "jlpt-n1");
                    return;
                case JLPT.N2:
                    serializer.Serialize(writer, "jlpt-n2");
                    return;
                case JLPT.N3:
                    serializer.Serialize(writer, "jlpt-n3");
                    return;
                case JLPT.N4:
                    serializer.Serialize(writer, "jlpt-n4");
                    return;
                case JLPT.N5:
                    serializer.Serialize(writer, "jlpt-n5");
                    return;
            }
            throw new Exception("Cannot marshal type JLPT");
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


    internal class TagConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Tag) || t == typeof(Tag?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "Archaism":
                    return Tag.Archaism;
                case "Honorific or respectful (sonkeigo)":
                    return Tag.HonorificOrRespectfulSonkeigo;
                case "Usually written using kana alone":
                    return Tag.UsuallyWrittenUsingKanaAlone;
                case "linguistics terminology":
                    return Tag.LinguisticsTerminology;
            }
            throw new Exception("Cannot unmarshal type Tag");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Tag)untypedValue;
            switch (value)
            {
                case Tag.Archaism:
                    serializer.Serialize(writer, "Archaism");
                    return;
                case Tag.HonorificOrRespectfulSonkeigo:
                    serializer.Serialize(writer, "Honorific or respectful (sonkeigo)");
                    return;
                case Tag.UsuallyWrittenUsingKanaAlone:
                    serializer.Serialize(writer, "Usually written using kana alone");
                    return;
                case Tag.LinguisticsTerminology:
                    serializer.Serialize(writer, "linguistics terminology");
                    return;
            }
            throw new Exception("Cannot marshal type Tag");
        }

        public static readonly TagConverter Singleton = new TagConverter();
    }


    public partial struct Dbpedia
    {
        public bool? Bool;
        public Uri PurpleUri;

        public static implicit operator Dbpedia(bool Bool) => new Dbpedia { Bool = Bool };
        public static implicit operator Dbpedia(Uri PurpleUri) => new Dbpedia { PurpleUri = PurpleUri };
    }

    internal class DbpediaConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Dbpedia) || t == typeof(Dbpedia?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Boolean:
                    var boolValue = serializer.Deserialize<bool>(reader);
                    return new Dbpedia { Bool = boolValue };
                case JsonToken.String:
                case JsonToken.Date:
                    var stringValue = serializer.Deserialize<string>(reader);
                    try
                    {
                        var uri = new Uri(stringValue);
                        return new Dbpedia { PurpleUri = uri };
                    }
                    catch (UriFormatException) { }
                    break;
            }
            throw new Exception("Cannot unmarshal type Dbpedia");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            var value = (Dbpedia)untypedValue;
            if (value.Bool != null)
            {
                serializer.Serialize(writer, value.Bool.Value);
                return;
            }
            if (value.PurpleUri != null)
            {
                serializer.Serialize(writer, value.PurpleUri.ToString());
                return;
            }
            throw new Exception("Cannot marshal type Dbpedia");
        }

        public static readonly DbpediaConverter Singleton = new DbpediaConverter();
    }

}




