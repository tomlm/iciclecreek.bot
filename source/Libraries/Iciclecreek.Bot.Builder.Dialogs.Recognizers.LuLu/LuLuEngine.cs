using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Flexible.Standard;
using Lucene.Net.Search;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Directory = Lucene.Net.Store.Directory;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.LuLu
{
    /// <summary>
    /// This class implements NLU recognizer using Lucene
    /// </summary>
    public class LuLuEngine
    {
        private Directory _directory;
        private IndexSearcher _searcher;
        private StandardQueryParser _parser;

        /// <summary>
        /// Create instance of engine pointing to Directory that has a catalog created with static method QLuceneEngine.CreateCatalog
        /// </summary>
        /// <param name="directory">A Lucene Directory object with the catalog you want to search over.</param>
        public LuLuEngine(Directory directory)
        {
            _directory = directory;
            _searcher = new IndexSearcher(DirectoryReader.Open(_directory));
            _parser = new StandardQueryParser(new StandardAnalyzer(Lucene.Net.Util.LuceneVersion.LUCENE_48));
        }

        public RecognizerResult RecognizeAsync(string text)
        {

            return null;
        }

        /// <summary>
        /// Creates a search catalog for the given LU json object in the given Lucene Directory
        /// </summary>
        /// <param name="luisJson">luis JSON (output of converting .lu file to .json using bf luis:convert </param>
        /// <param name="directory">Lucene directory to store the catalog in</param>
        /// <remarks>
        ///  Lucene Directory objects are abstract location for storage of catalogs.
        ///  There are 3 super useful versions:
        ///     * new RAMDirectory() - store catalog in memroy
        ///     * FSDirectory.Create(folderPath) - Will create a FSDirectory to store the index in a folder
        ///     * AzureDirector(blobConnectionString) - Will set it up to store the catalog in blob storage.
        ///  When you create a catalog in a directory that is persistent you only need to create the catalog once.
        ///  If you use RAMDirectory you need to create the catalog once per process.
        /// </remarks>
        public static void CreateCatalog(string luisJson, Directory directory)
        {
            var indexWriterConfig = new IndexWriterConfig(Lucene.Net.Util.LuceneVersion.LUCENE_48, new StandardAnalyzer(Lucene.Net.Util.LuceneVersion.LUCENE_48));

            LuisApp app = JsonConvert.DeserializeObject<LuisApp>(luisJson);

            EntityRecognizerSet erSet = new EntityRecognizerSet();
            using (var indexWriter = new IndexWriter(directory, indexWriterConfig))
            {
                List<dynamic> requestContexts = new List<dynamic>();
                foreach (var prebuilt in app.PrebuiltEntities)
                {
                    switch (prebuilt.Name)
                    {
                        case "age": erSet.Add(new AgeEntityRecognizer()); break;
                        case "confirmation": erSet.Add(new ConfirmationEntityRecognizer()); break;
                        case "currency": erSet.Add(new CurrencyEntityRecognizer()); break;
                        case "datetime": erSet.Add(new DateTimeEntityRecognizer()); break;
                        case "datetimev2": erSet.Add(new DateTimeEntityRecognizer()); break;
                        case "dimension": erSet.Add(new DimensionEntityRecognizer()); break;
                        case "email": erSet.Add(new EmailEntityRecognizer()); break;
                        case "guid": erSet.Add(new GuidEntityRecognizer()); break;
                        case "hashtag": erSet.Add(new HashtagEntityRecognizer()); break;
                        case "ip": erSet.Add(new IpEntityRecognizer()); break;
                        case "mention": erSet.Add(new MentionEntityRecognizer()); break;
                        case "number": erSet.Add(new NumberEntityRecognizer()); break;
                        case "numberrange": erSet.Add(new NumberRangeEntityRecognizer()); break;
                        case "ordinal": erSet.Add(new OrdinalEntityRecognizer()); break;
                        case "ordinalv2": erSet.Add(new OrdinalEntityRecognizer()); break;
                        case "percentage": erSet.Add(new PercentageEntityRecognizer()); break;
                        case "phone": erSet.Add(new PhoneNumberEntityRecognizer()); break;
                        case "temperature": erSet.Add(new TemperatureEntityRecognizer()); break;
                        case "url": erSet.Add(new UrlEntityRecognizer()); break;
                    }
                }

                foreach (dynamic qna in app.ClosedLists)
                {
                    // var questions = String.Join("\n\n", ((JArray)qna.questions).Select(s => (string)s));
                    var doc = new Document
                    {
                        new StringField("id", (string)qna.id, Field.Store.YES),
                        new StringField("source", (string)qna.source, Field.Store.YES),
                        new TextField("answer", (string)qna.answer, Field.Store.YES),
                    };

                    var sb = new StringBuilder();
                    sb.AppendLine((string)qna.answer);

                    foreach (var question in (JArray)qna.questions)
                    {
                        doc.Add(new TextField("questions", (string)question.ToString(), Field.Store.YES));
                        sb.AppendLine((string)question);
                    }

                    foreach (dynamic md in qna.metadata)
                    {
                        doc.Add(new TextField((String)md.name, (string)md.value, Field.Store.YES));
                    }

                    foreach (var requestContext in requestContexts.Where(rc => rc.answerQnAId == qna.id.ToString()))
                    {
                        doc.Add(new TextField("previousQnAId", requestContext.previousQnAId.ToString(), Field.Store.NO));
                        doc.Add(new TextField("prompt", requestContext.prompt.ToString(), Field.Store.NO));
                    }

                    doc.Add(new StringField("context", ((JToken)qna.context ?? new JObject()).ToString(), Field.Store.YES));
                    doc.Add(new StringField("prompts", ((JToken)qna.prompts ?? new JArray()).ToString(), Field.Store.YES));
                    doc.Add(new TextField("questionsAndAnswer", sb.ToString(), Field.Store.NO));
                    indexWriter.AddDocument(doc);
                }
            }
        }
    }
}
