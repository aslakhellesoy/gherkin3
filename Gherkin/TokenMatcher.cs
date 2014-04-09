﻿using System;
using System.Linq;

namespace Gherkin
{
	public class TokenMatcher : ITokenMatcher
    {
        private readonly GherkinDialectProvider dialectProvider;
        private GherkinDialect currentDialect;

        public TokenMatcher(GherkinDialectProvider dialectProvider = null)
        {
            this.dialectProvider = dialectProvider ?? new GherkinDialectProvider();

            currentDialect = this.dialectProvider.DefaultDialect;
        }

		public bool Match_EOF(Token token)
		{
			if (token.IsEOF)
			{
				token.MatchedType = TokenType.EOF;
				return true;
			}
			return false;
		}

		public bool Match_Other(Token token)
		{
			token.MatchedType = TokenType.Other;
			token.Text = token.Line.GetLineText(0); //take the entire line
			return true;
		}

		public bool Match_Empty(Token token)
		{
			if (!token.IsEOF && token.Line.IsEmpty())
			{
				token.MatchedType = TokenType.Empty;
				return true;
			}
			return false;
		}

		public bool Match_Comment(Token token)
        {
			if (!token.IsEOF && token.Line.StartsWith(GherkinLanguageConstants.COMMENT_PREFIX))
            {
                token.MatchedType = TokenType.Comment;
                token.Text = token.Line.GetRestTrimmed(0).Substring(1);
                return true;
            }
            return false;
        }

		public bool Match_Language(Token token)
		{
			if (!token.IsEOF && token.Line.StartsWith(GherkinLanguageConstants.LANGUAGE_PREFIX))
			{
				var language = token.Line.GetRestTrimmed(GherkinLanguageConstants.LANGUAGE_PREFIX.Length);
				currentDialect = dialectProvider.GetDialect(language);

				token.MatchedType = TokenType.Language;
				token.Text = language;
				return true;
			}
			return false;
		}

		public bool Match_TagLine(Token token)
		{
			if (!token.IsEOF && token.Line.StartsWith(GherkinLanguageConstants.TAG_PREFIX))
			{
				token.MatchedType = TokenType.TagLine;
				token.Items = token.Line.GetTags().ToArray();
				token.Indent = token.Line.Indent;

				return true;
			}
			return false;
		}

		public bool Match_FeatureLine(Token token)
		{
			return MatchTitleLine(token, TokenType.FeatureLine, currentDialect.FeatureKeywords);
		}

		public bool Match_BackgroundLine(Token token)
        {
			return MatchTitleLine(token, TokenType.BackgroundLine, currentDialect.BackgroundKeywords);
        }

		public bool Match_ScenarioLine(Token token)
		{
			return MatchTitleLine(token, TokenType.ScenarioLine, currentDialect.ScenarioKeywords);
		}

		public bool Match_ScenarioOutlineLine(Token token)
		{
			return MatchTitleLine(token, TokenType.ScenarioOutlineLine, currentDialect.ScenarioOutlineKeywords);
		}

		public bool Match_ExamplesLine(Token token)
		{
			return MatchTitleLine(token, TokenType.ExamplesLine, currentDialect.ExamplesKeywords);
		}

        private bool MatchTitleLine(Token token, TokenType tokenType, string[] keywords)
        {
            if (token.IsEOF)
                return false;

            foreach (var keyword in keywords)
            {
                if (token.Line.StartsWithTitleKeyword(keyword))
                {
                    token.MatchedType = tokenType;
                    token.MatchedKeyword = keyword;
                    token.Text = token.Line.GetRestTrimmed(keyword.Length + GherkinLanguageConstants.TITLE_KEYWORD_SEPARATOR.Length);
	                token.Indent = token.Line.Indent;
                    return true;
                }
            }
            return false;
        }

        public bool Match_DocStringSeparator(Token token)
        {
            return Match_DocStringSeparatorInternal(token, GherkinLanguageConstants.DOCSTRING_SEPARATOR, TokenType.DocStringSeparator);
        }

        public bool Match_DocStringAlternativeSeparator(Token token)
        {
			return Match_DocStringSeparatorInternal(token, GherkinLanguageConstants.DOCSTRING_ALTERNATIVE_SEPARATOR, TokenType.DocStringAlternativeSeparator);
        }

        private bool Match_DocStringSeparatorInternal(Token token, string separator, TokenType tokenType)
        {
            if (token.Line.StartsWith(separator))
            {
                token.MatchedType = tokenType;
                token.Indent = token.Line.Indent;
                token.Text = token.Line.GetRestTrimmed(separator.Length); // content type
                return true;
            }
            return false;
        }

        public bool Match_StepLine(Token token)
        {
            var keywords = currentDialect.StepKeywords;
            foreach (var keyword in keywords)
            {
                if (token.Line.StartsWith(keyword))
                {
                    token.MatchedType = TokenType.StepLine;
                    token.MatchedKeyword = keyword;
                    token.Text = token.Line.GetRestTrimmed(keyword.Length);
	                token.Indent = token.Line.Indent;
                    return true;
                }
            }
            return false;
        }

        public bool Match_TableRow(Token token)
        {
            if (token.Line.StartsWith(GherkinLanguageConstants.TABLE_CELL_SEPARATOR))
            {
                token.MatchedType = TokenType.TableRow;
                token.Items = token.Line.GetTableCells().ToArray();
				token.Indent = token.Line.Indent;
                return true;
            }
            return false;
        }
    }
}