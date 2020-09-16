using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace QUDeveloper
{
    public class WordFinder
    {
        public const int MATRIX_LIMIT = 5;

        private IEnumerable<string> _matrix;
        public WordFinder(IEnumerable<string> matrix)
        {
            if (matrix.Count() != WordFinder.MATRIX_LIMIT || matrix.Any(row => row.Length != WordFinder.MATRIX_LIMIT))
                throw new Exception(String.Format("The Matrix needs to have {0} rows with {0} characters each.", WordFinder.MATRIX_LIMIT));

            _matrix = matrix;
        }

        public IEnumerable<string> Find(IEnumerable<string> wordstream)
        {
            int i = 0, j = 0;
            List<MatchCandidate> candidates = new List<MatchCandidate>();
            foreach (string row in _matrix)
            {
                j = 0;
                foreach (var c in row)
                {
                    // This lines removes any candidates that are not feasible after the new character.
                    candidates.RemoveAll(cand => !cand.Evaluate(c, i, j));

                    // Here I create candidates in case the character matches the first position of a word.
                    foreach (var w in wordstream.Where(w => w.StartsWith(c)))
                        candidates.Add(new MatchCandidate(c, i, j, w));

                    j++;
                }
                i++;
            }

            // The instructions asked to return the top 10 most repeated words. However, a few words after
            // it also says that if any word is found more than once in the matrix the search results should
            // count it only once.
            // I decided to count the number of hits each word had and create the Top10 anyway. In case the requirement
            // is about taking just any 10 words then there's no need to group like I did.
            var top10Analysis = candidates.GroupBy(c => c.Word)
                                          .Select(x => new { Key = x.Key, Value = x.Sum(y => y.Hits) })
                                          .OrderByDescending(x => x.Value)
                                          .Take(10);

            return top10Analysis.Select(x => x.Key);
        }
    }

    public class MatchCandidate
    {
        private List<MatchCriteria> _matchCriterias;
        public string Word;
        public int Hits { get { return _matchCriterias.Count(m => m.MatchStatus.Equals(MatchStatus.Matched)); } }

        /// <summary>
        ///  In this constructor I create instances for each type of MatchCriteria I need. 
        ///  If any more criterias are created in the future, adding it to _matchCriteria list will be enough for the class to execute them.
        /// </summary>
        public MatchCandidate(char c, int i, int j, string word)
        {
            this.Word = word;
            this._matchCriterias = new List<MatchCriteria>
            {
                new HorizontalCriteria(c, i, j, word),
                new VerticalCriteria(c, i, j, word)
            };
        }

        /// <summary>
        /// This method simply removes any criterias that stop being possible solutions.
        /// </summary>
        public bool Evaluate(char c, int i, int j)
        {
            _matchCriterias.RemoveAll(m => m.MatchStatus != MatchStatus.Matched && !m.Execute(c, i, j));
            return _matchCriterias.Count != 0;
        }
    }

    /// <summary>
    /// MatchCriteria class is the abstract class I use to encapsulate and reuse methods needed to determine if a word can be found.
    /// </summary>
    public abstract class MatchCriteria
    {
        public MatchStatus MatchStatus;

        protected string _finalWord;
        protected int _position;
        protected int _nexti, _nextj;

        public MatchCriteria(char character, int i, int j, string finalWord)
        {
            this._position = 0;
            this._finalWord = finalWord;
            this._nexti = i;
            this._nextj = j;
            this.Execute(character, i, j);
        }

        /// <summary>
        /// For each iteration, this method will verify if the next expected character is found. 
        /// In addition, it will remove any candidate that is not feasible due to the matrix's limits.
        /// </summary>
        public virtual bool Execute(char c, int i, int j)
        {
            if (_finalWord[_position++].Equals(c) && NextPositionIsPossible())
                MatchStatus = _position == _finalWord.Length ? MatchStatus.Matched : MatchStatus.Possible;
            else
                MatchStatus = MatchStatus.Failed;

            return MatchStatus != MatchStatus.Failed;
        }

        /// <summary>
        /// The method will determine an impossibility if any of the new expected positions to be out of limits
        /// unless the word has already been formed.
        /// </summary>
        protected virtual bool NextPositionIsPossible()
        {
            return (_nextj < WordFinder.MATRIX_LIMIT && _nexti < WordFinder.MATRIX_LIMIT) || _position.Equals(_finalWord.Length);
        }
    }

    public class VerticalCriteria : MatchCriteria
    {
        public VerticalCriteria(char character, int i, int j, string finalWord) : base(character, i, j, finalWord)
        {
        }

        /// <summary>
        /// The Execute method is only invoked if the character is located in the expected position
        /// Before executing, I make sure the next vertical position is set for the next iteration.
        /// </summary>
        public override bool Execute(char c, int i, int j)
        {
            if (i == _nexti && j == _nextj)
                return base.Execute(c, _nexti++, j);
            else
                return this.MatchStatus != MatchStatus.Failed;
        }
    }

    public class HorizontalCriteria : MatchCriteria
    {
        public HorizontalCriteria(char character, int i, int j, string finalWord) : base(character, i, j, finalWord)
        {
        }

        /// <summary>
        /// The Execute method is only invoked if the character is located in the expected position
        /// Before executing, I make sure the next horizontal position is set for the next iteration.
        /// </summary>
        public override bool Execute(char c, int i, int j)
        {
            if (i == _nexti && j == _nextj)
                return base.Execute(c, i, _nextj++);
            else
                return this.MatchStatus != MatchStatus.Failed;
        }
    }

    public enum MatchStatus
    {
        Possible = 1,
        Failed = 2,
        Matched = 3
    }
}