public class PhraseAnalysis
{
    // Counts how many items does a given string contain from a given array of strings
    public static int GetMatches(string str, string[] kw)
    {
        str = str.ToUpper(); 
        return kw.Count(keyword => str.Contains(keyword.ToUpper()));
    }


    // Applies GetMatches to every item from the Keywords and returns an ID, or -1 if no matches, or -2 if empty or invalid string
    public static int GetPhraseAnalysis(string phrase)
    {
        if (string.IsNullOrEmpty(phrase) || phrase.Length <= 2)
        {
            return -2; // Invalid or empty phrase
        }

        phrase = phrase.ToUpper();
        List<int> counters = Keywords.KeywordsDictionary.Select(kwArray => GetMatches(phrase, kwArray.Value)).ToList();
        int maxMatches = counters.Max();

        if (maxMatches == 0)
        {
            return -1; // No keyword matches found
        }
    
        return counters.IndexOf(maxMatches); // Return index of maximum matches
    }
}
