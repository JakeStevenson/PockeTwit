using System;
using System.Collections.Generic;
using System.Text;

namespace PockeTwit.OtherServices.TextShrinkers
{
    interface ITextShrinker
    {
        string GetShortenedText(string originalText);
        string ServiceName();
        string ServiceDescription();
    }
}
