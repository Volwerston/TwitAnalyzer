using System;
using System.IO;
using System.Threading.Tasks;
using TwitAnalyzer.Domain;

namespace nBayes
{
    public class BayesAnalyzer : ITwitAnalyzer
    {
        private float I = 0;
        private float invI = 0;

        private readonly Index _positive;
        private readonly Index _negative;

        internal BayesAnalyzer(Index positive, Index negative)
        {
            _positive = positive;
            _negative = negative;
        }

        public static BayesAnalyzer GetTrained(string dataSet)
        {
            var positive = Index.CreateMemoryIndex();
            var negative = Index.CreateMemoryIndex();


            var ms = new MemoryStream();

            using (var fs = GenerateStreamFromString(dataSet))
            {
                using (var sr = new StreamReader(fs))
                {
                    sr.ReadLine();

                    string line = null;

                    while ((line =sr.ReadLine()) != null)
                    {
                        var data = line.Split(new[] {','});

                        switch (data[0].Trim())
                        {
                            case "Pos":
                                positive.Add(Entry.FromString(data[1].Trim()));
                                break;
                            case "Neg":
                                negative.Add(Entry.FromString(data[1].Trim()));
                                break;
                        }
                    }
                }
            }

            return new BayesAnalyzer(positive, negative);
        }

        public Task<TwitAnalyzerResult> Analyze(Twit twit)
        {
            var probability = GetPrediction(Entry.FromString(twit.Text));

            return Task.FromResult(
                new TwitAnalyzerResult
                {
                    Algorithm = "bayes",
                    TwitAnalysisResult = new TwitAnalysisResult
                    {
                        CategorizationResult = TwitAnalysisResult.Categorize(probability),
                        PositiveProbability = probability,
                        Text = twit.Text,
                        TimestampUtc = DateTime.UtcNow
                    }
                });
        }

        public float GetPrediction(Entry item)
        {
            foreach (var token in item)
            {
                int positiveCount = _positive.GetTokenCount(token);
                int negativeCount = _negative.GetTokenCount(token);

                float probability = CalcProbability(positiveCount, _positive.EntryCount, negativeCount, _negative.EntryCount);
            }

            float prediction = CombineProbability();
            return prediction;
        }

        #region Private Methods

        /// <summary>
        /// Calculates a probablility value based on statistics from two categories
        /// </summary>
        private float CalcProbability(float cat1count, float cat1total, float cat2count, float cat2total)
        {
            float bw = cat1count / cat1total;
            float gw = cat2count / cat2total;
            float pw = ((bw) / ((bw) + (gw)));
            float
                s = 1f,
                x = .5f,
                n = cat1count + cat2count;
            float fw = ((s * x) + (n * pw)) / (s + n);

            LogProbability(fw);

            return fw;
        }

        private void LogProbability(float prob)
        {
            if (!float.IsNaN(prob))
            {
                I = I == 0 ? prob : I * prob;
                invI = invI == 0 ? (1 - prob) : invI * (1 - prob);
            }
        }

        private float CombineProbability()
        {
            return I / (I + invI);
        }

        #endregion

        private static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
