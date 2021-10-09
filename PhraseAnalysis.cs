using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PhraseAnalysis
{
    //This method counts how many items does a given string contain from a given array of strings
    //ToUpper() is used to avoid case sensitivity
    public static int GetMatches(string str, string[] kw)
    {
        int counter = 0;
        for (int i = 0; i < kw.Length; i++)
        {
            if (str.ToUpper().Contains(kw[i].ToUpper()))
            {
                counter++;
            }
        }
        return counter;
    }

    //This method applies GetMatches to every item from the Keywords and returns an ID, or -1 if no matches, or -2 if empty or invalid string
    public static int GetPhraseAnalysis(string phrase)
    {
        if (phrase != string.Empty && phrase.Length > 2)
        {
            List<int> counters = new List<int>();
            for (int i = 0; i < Keywords.KeywordsDictionary.Count; i++)
            {
                counters.Add(GetMatches(phrase, Keywords.KeywordsDictionary[i]));
                Debug.Log($"Number of matches for the {i} element is: " + counters[i]);
            }
            int tempMax = counters.Max();
            int temp = counters.IndexOf(tempMax);
            if (tempMax == 0)
            {
                return -1; //zero keywords matches found
            }
            else
            {
                return temp; //maximum matches has been found at this index (connected to the department's id)
            }
        }
        else
        {
            return -2; //empty string or string's length is 2 chars or less
        }
    }
}
