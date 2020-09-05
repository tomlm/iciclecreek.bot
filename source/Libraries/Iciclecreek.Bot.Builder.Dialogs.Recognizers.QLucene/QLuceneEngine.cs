using System;
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Flexible.Standard;
using Lucene.Net.Search;
using Microsoft.Bot.Builder.AI.QnA;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Directory = Lucene.Net.Store.Directory;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.QLucene
{
    /// <summary>
    /// This class implements QnAMaker smeantics using Lucene
    /// </summary>
    public class QLuceneEngine
    {
        private Directory _directory;
        private IndexSearcher _searcher;
        private StandardQueryParser _parser;

        /// <summary>
        /// Create instance of engine pointing to Directory that has a catalog created with static method QLuceneEngine.CreateCatalog
        /// </summary>
        /// <param name="directory">A Lucene Directory object with the catalog you want to search over.</param>
        public QLuceneEngine(Directory directory)
        {
            _directory = directory;
            _searcher = new IndexSearcher(DirectoryReader.Open(_directory));
            _parser = new StandardQueryParser(new StandardAnalyzer(Lucene.Net.Util.LuceneVersion.LUCENE_48));
        }

        /// <summary>
        /// Get Answers from QnA
        /// </summary>
        /// <param name="text">text to match</param>
        /// <param name="strictFilters">Optional Filters against metadata which must match</param>
        /// <param name="context">Optional context for multi-turn</param>
        /// <param name="threshold">Threshold which needs to be matched to return results.</param>
        /// <returns>The QueryResult</returns>
        public QueryResult GetAnswers(string text, Metadata[] strictFilters = null, QnARequestContext context = null, float threshold = 0.3F)
        {
            text = text.Trim('?', '*', ' ', '\t');
            var query = new BooleanQuery();

            if (context != null)
            {
                if (context.PreviousQnAId != 0)
                {
                    query.Add(_parser.Parse(context.PreviousQnAId.ToString(), "previousQnAId"), Occur.MUST);
                }

                query.Add(_parser.Parse(text, "prompt"), Occur.SHOULD);
            }
            else
            {
                query.Add(_parser.Parse(text, "questions"), Occur.SHOULD);
            }

            if (strictFilters != null)
            {
                foreach (var metadata in strictFilters)
                {
                    query.Add(new TermQuery(new Term(metadata.Name, metadata.Value)), Occur.MUST);
                }
            }

            var topDocs = _searcher.Search(query, 10);
            if (topDocs.ScoreDocs.Length > 0 && topDocs.ScoreDocs[0].Score >= threshold)
            {
                var doc = _searcher.Doc(topDocs.ScoreDocs[0].Doc);

                // System.Diagnostics.Debug.WriteLine(_searcher.Explain(query, topDocs.ScoreDocs[0].Doc));

                var queryResult = new QueryResult()
                {
                    Score = topDocs.ScoreDocs[0].Score
                };

                Dictionary<string, string> filters = new Dictionary<string, string>();

                foreach (var field in doc.Fields)
                {
                    switch (field.Name)
                    {
                        case "id":
                            queryResult.Id = doc.GetValues("id").Select(s => int.Parse(s)).FirstOrDefault();
                            break;
                        case "source":
                            queryResult.Source = doc.GetValues("source").FirstOrDefault();
                            break;
                        case "context":
                            queryResult.Context = JsonConvert.DeserializeObject<QnAResponseContext>(doc.GetValues("context").FirstOrDefault());
                            break;
                        case "questions":
                            queryResult.Questions = doc.GetValues("questions");
                            break;
                        case "answer":
                            queryResult.Answer = doc.GetValues("answer").FirstOrDefault();
                            break;
                        default:
                            filters.Add(field.Name, field.GetStringValue());
                            break;
                    }
                }

                queryResult.Metadata = filters.Select(kv => new Metadata() { Name = kv.Key, Value = kv.Value }).ToArray();
                return queryResult;
            }

            return null;
        }

        /// <summary>
        /// Creates a search catalog for the given QnAMaker json object in the given Lucene Directory
        /// </summary>
        /// <param name="qnaJson">qnaJson (output of converting .qna file to .json using bf qnamaker:convert </param>
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
        public static void CreateCatalog(string qnaJson, Directory directory)
        {
            var indexWriterConfig = new IndexWriterConfig(Lucene.Net.Util.LuceneVersion.LUCENE_48, new StandardAnalyzer(Lucene.Net.Util.LuceneVersion.LUCENE_48));

            dynamic qnaKB = JsonConvert.DeserializeObject(qnaJson);
            if (qnaKB.kb != null)
            {
                qnaKB = qnaKB.kb;
            }

            using (var indexWriter = new IndexWriter(directory, indexWriterConfig))
            {
                List<dynamic> requestContexts = new List<dynamic>();
                foreach (dynamic qna in (JArray)qnaKB.qnaList)
                {
                    if (qna.context?.prompts != null)
                    {
                        foreach (dynamic prompt in qna.context.prompts)
                        {
                            requestContexts.Add(new
                            {
                                answerQnAId = prompt.qnaId,
                                previousQnAId = qna.id,
                                prompt = prompt.displayText
                            });
                        }
                    }
                }

                foreach (dynamic qna in (JArray)qnaKB.qnaList)
                {
                    // var questions = String.Join("\n\n", ((JArray)qna.questions).Select(s => (string)s));
                    var doc = new Document
                    {
                        new StringField("id", (string)qna.id, Field.Store.YES),
                        new StringField("source", (string)qna.source, Field.Store.YES),
                        new TextField("answer", (string)qna.answer, Field.Store.YES),
                    };

                    foreach (var question in (JArray)qna.questions)
                    {
                        doc.Add(new TextField("questions", (string)question.ToString(), Field.Store.YES));
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
                    indexWriter.AddDocument(doc);
                }

            }
        }
    }
}
