/* Yet Another Forum.net
 * Copyright (C) 2003-2005 Bj�rnar Henden
 * Copyright (C) 2006-2007 Jaben Cargman
 * http://www.yetanotherforum.net/
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 */

using System;
using System.Data;
using System.Web;
using System.Text.RegularExpressions;
using YAF.Classes.Utils;
using YAF.Classes.Data;

namespace YAF.Classes.UI
{
	/// <summary>
	/// Summary description for BBCode.
	/// </summary>
	public class BBCode
	{
		/* Ederon : 6/16/2007 - conventions */

		private BBCode() { }

		static private RegexOptions _options = RegexOptions.IgnoreCase | RegexOptions.Singleline;
		static private string _rgxCode2 = @"\[code=(?<language>[^\]]*)\](?<inner>(.*?))\[/code\]";
		static private string _rgxCode1 = @"\[code\](?<inner>(.*?))\[/code\]";
		static private string _rgxSize = @"\[size=(?<size>([1-9]))\](?<inner>(.*?))\[/size\]";
		static private string _rgxBold = @"\[B\](?<inner>(.*?))\[/B\]";
		static private string _rgxStrike = @"\[S\](?<inner>(.*?))\[/S\]";
		static private string _rgxItalic = @"\[I\](?<inner>(.*?))\[/I\]";
		static private string _rgxUnderline = @"\[U\](?<inner>(.*?))\[/U\]";
		static private string _rgxEmail2 = @"\[email=(?<email>[^\]]*)\](?<inner>(.*?))\[/email\]";
		static private string _rgxEmail1 = @"\[email[^\]]*\](?<inner>(.*?))\[/email\]";
		static private string _rgxUrl1 = @"\[url\](?<http>(skype:)|(http://)|(https://)| (ftp://)|(ftps://))?(?<inner>(.*?))\[/url\]";
		static private string _rgxUrl2 = @"\[url\=(?<http>(skype:)|(http://)|(https://)|(ftp://)|(ftps://))?(?<url>([^\]]*?))\](?<inner>(.*?))\[/url\]";
		static private string _rgxFont = @"\[font=(?<font>([-a-z0-9, ]*))\](?<inner>(.*?))\[/font\]";
		static private string _rgxColor = @"\[color=(?<color>(\#?[-a-z0-9]*))\](?<inner>(.*?))\[/color\]";
		static private string _rgxBullet = @"\[\*\]";
		static private string _rgxList4 = @"\[list=i\](?<inner>(.*?))\[/list\]";
		static private string _rgxList3 = @"\[list=a\](?<inner>(.*?))\[/list\]";
		static private string _rgxList2 = @"\[list=1\](?<inner>(.*?))\[/list\]";
		static private string _rgxList1 = @"\[list\](?<inner>(.*?))\[/list\]";
		static private string _rgxCenter = @"\[center\](?<inner>(.*?))\[/center\]";
		static private string _rgxLeft = @"\[left\](?<inner>(.*?))\[/left\]";
		static private string _rgxRight = @"\[right\](?<inner>(.*?))\[/right\]";
		static private string _rgxQuote2 = @"\[quote=(?<quote>[^\]]*)\](?<inner>(.*?))\[/quote\]";
		static private string _rgxQuote1 = @"\[quote\](?<inner>(.*?))\[/quote\]";
		static private string _rgxHr = "^[-][-][-][-][-]*[\r]?[\n]";
		static private string _rgxBr = "[\r]?\n";
		static private string _rgxPost = @"\[post=(?<post>[^\]]*)\](?<inner>(.*?))\[/post\]";
		static private string _rgxTopic = @"\[topic=(?<topic>[^\]]*)\](?<inner>(.*?))\[/topic\]";
		static private string _rgxImg = @"\[img\](?<http>(http://)|(https://)|(ftp://)|(ftps://))?(?<inner>(.*?))\[/img\]";
		static private string _rgxYoutube = @"\[youtube\](?<inner>http://(www\.)?youtube.com/watch\?v=(?<id>[0-9A-Za-z-_]{11})[^[]*)\[/youtube\]";

		/// <summary>
		/// Helper function for older code
		/// </summary>
		/// <param name="bbcode"></param>
		/// <param name="doFormatting"></param>
		/// <param name="targetBlankOverride"></param>
		/// <returns></returns>
		static public string MakeHtml( string inputString, bool doFormatting, bool targetBlankOverride )
		{
			ReplaceRules ruleEngine = new ReplaceRules();
			MakeHtml( ref ruleEngine, ref inputString, doFormatting, targetBlankOverride );
			ruleEngine.Process( ref inputString );
			return inputString;
		}

		/// <summary>
		/// Converts BBCode to HTML.
		/// Needs to be refactored!
		/// </summary>
		/// <param name="bbcode"></param>
		/// <param name="doFormatting"></param>
		/// <param name="targetBlankOverride"></param>
		/// <returns></returns>
		static public void MakeHtml( ref ReplaceRules ruleEngine, ref string bbcode, bool doFormatting, bool targetBlankOverride )
		{
			string target = ( YafContext.Current.BoardSettings.BlankLinks || targetBlankOverride ) ? "target=\"_blank\"" : "";

			// pull localized strings
			string localQuoteStr = YafContext.Current.Localization.GetText( "COMMON", "BBCODE_QUOTE" );
			string localQuoteWroteStr = YafContext.Current.Localization.GetText( "COMMON", "BBCODE_QUOTEWROTE" );
			string localCodeStr = YafContext.Current.Localization.GetText( "COMMON", "BBCODE_CODE" );

			// add rule for code block type with syntax highlighting			
			ruleEngine.AddRule( new SyntaxHighlightedCodeRegexReplaceRule( _rgxCode2, @"<div class=""code""><b>{0}</b><div class=""innercode"">${inner}</div></div>".Replace("{0}",localCodeStr), _options ) );

			// add rule for code block type with no syntax highlighting
			ruleEngine.AddRule( new CodeRegexReplaceRule( _rgxCode1, @"<div class=""code""><b>{0}</b><div class=""innercode"">${inner}</div></div>".Replace( "{0}", localCodeStr ), _options ) );

			// handle font sizes -- this rule class internally handles the "size" variable
			ruleEngine.AddRule( new FontSizeRegexReplaceRule( _rgxSize, @"<span style=""font-size:${size}"">${inner}</span>", _options ) );

			if ( doFormatting )
			{
				ruleEngine.AddRule( new SimpleRegexReplaceRule( _rgxBold, "<b>${inner}</b>", _options ) );
				ruleEngine.AddRule( new SimpleRegexReplaceRule( _rgxStrike, "<s>${inner}</s>", _options ) );
				ruleEngine.AddRule( new SimpleRegexReplaceRule( _rgxItalic, "<i>${inner}</i>", _options ) );
				ruleEngine.AddRule( new SimpleRegexReplaceRule( _rgxUnderline, "<u>${inner}</u>", _options ) );

				// e-mails
				ruleEngine.AddRule( new VariableRegexReplaceRule( _rgxEmail2, "<a href=\"mailto:${email}\">${inner}</a>", _options, new string [] { "email" } ) );
				ruleEngine.AddRule( new SimpleRegexReplaceRule( _rgxEmail1, "<a href=\"mailto:${inner}\">${inner}</a>", _options ) );

				// urls
				ruleEngine.AddRule(
					new VariableRegexReplaceRule(
						_rgxUrl2,
						"<a {0} rel=\"nofollow\" href=\"${http}${url}\" title=\"${http}${url}\">${inner}</a>".Replace( "{0}", target ),
						_options,
						new string [] { "url", "http" },
						new string [] { "", "http://" }
						)
				);
				ruleEngine.AddRule(
					new VariableRegexReplaceRule(
						_rgxUrl1,
						"<a {0} rel=\"nofollow\" href=\"${http}${innertrunc}\" title=\"${http}${inner}\">${http}${inner}</a>".Replace( "{0}", target ),
						_options,
						new string [] { "http" },
						new string [] { "", "http://" },
						50
						)
				);

				// font
				ruleEngine.AddRule(
					new VariableRegexReplaceRule(
						_rgxFont,
						"<span style=\"font-family:${font}\">${inner}</span>",
						_options,
						new string [] { "font" }
						)
				);

				// color
				ruleEngine.AddRule(
					new VariableRegexReplaceRule(
						_rgxColor,
						"<span style=\"color:${color}\">${inner}</span>",
						_options,
						new string [] { "color" }
						)
				);

				// bullets
				ruleEngine.AddRule( new SingleRegexReplaceRule( _rgxBullet, "<li>", _options ) );
				ruleEngine.AddRule( new SimpleRegexReplaceRule( _rgxList4, "<ol type=\"i\">${inner}</ol>", _options ) );
				ruleEngine.AddRule( new SimpleRegexReplaceRule( _rgxList3, "<ol type=\"a\">${inner}</ol>", _options ) );
				ruleEngine.AddRule( new SimpleRegexReplaceRule( _rgxList2, "<ol>${inner}</ol>", _options ) );
				ruleEngine.AddRule( new SimpleRegexReplaceRule( _rgxList1, "<ul>${inner}</ul>", _options ) );

				// alignment
				ruleEngine.AddRule( new SimpleRegexReplaceRule( _rgxCenter, "<div align=\"center\">${inner}</div>", _options ) );
				ruleEngine.AddRule( new SimpleRegexReplaceRule( _rgxLeft, "<div align=\"left\">${inner}</div>", _options ) );
				ruleEngine.AddRule( new SimpleRegexReplaceRule( _rgxRight, "<div align=\"right\">${inner}</div>", _options ) );

				// image
				ruleEngine.AddRule(
					new VariableRegexReplaceRule(
						_rgxImg,
						"<img src=\"${http}${inner}\" alt=\"\"/>",
						_options,
						new string [] { "http" },
						new string [] { "http://" }
						)
				);

				// youtube
				ruleEngine.AddRule(
					new VariableRegexReplaceRule(
						_rgxYoutube,
						@"<!-- BEGIN youtube --><object width=""425"" height=""350""><param name=""movie"" value=""http://www.youtube.com/v/${id}""></param><embed src=""http://www.youtube.com/v/${id}"" type=""application/x-shockwave-flash"" width=""425"" height=""350""></embed></object><br /><a href=""http://youtube.com/watch?v=${id}"" target=""_blank"">${inner}</a><br /><!-- END youtube -->",
						_options,
						new string [] { "id" }
						)
				);

				// handle custom BBCode
				AddCustomBBCodeRules( ref ruleEngine );

				// basic hr and br rules
				ruleEngine.AddRule( new SingleRegexReplaceRule( _rgxHr, "<hr/>", _options ) );
				ruleEngine.AddRule( new SingleRegexReplaceRule( _rgxBr, "<br/>", _options ) );
			}

			// add smilies
			FormatMsg.AddSmiles( ref ruleEngine );

			// "quote" handling...
			string tmpReplaceStr;

			tmpReplaceStr = string.Format( @"<div class=""quote""><b>{0}</b><div class=""innerquote"">{1}</div></div>", localQuoteWroteStr.Replace( "{0}", "${quote}" ), "${inner}" );
			ruleEngine.AddRule( new VariableRegexReplaceRule( _rgxQuote2, tmpReplaceStr, _options, new string [] { "quote" } ) );

			tmpReplaceStr = string.Format( @"<div class=""quote""><b>{0}</b><div class=""innerquote"">{1}</div></div>", localQuoteStr, "${inner}" );
			ruleEngine.AddRule( new SimpleRegexReplaceRule( _rgxQuote1, tmpReplaceStr, _options ) );

			// post and topic rules...
			ruleEngine.AddRule(
				new PostTopicRegexReplaceRule(
					_rgxPost,
					@"<a {0} href=""${post}"">${inner}</a>".Replace( "{0}", target ),
					_options
					)
			);
			ruleEngine.AddRule(
				new PostTopicRegexReplaceRule(
					_rgxTopic,
					@"<a {0} href=""${topic}"">${inner}</a>".Replace( "{0}", target ),
					_options
					)
			);
		}

		/// <summary>
		/// Applies Custom BBCode Rules from the BBCode table
		/// </summary>
		/// <param name="refText">Text to transform</param>
		static protected void AddCustomBBCodeRules( ref ReplaceRules rulesEngine )
		{
			DataTable bbcodeTable = GetCustomBBCode();

			// handle custom bbcodes row by row...
			foreach ( DataRow codeRow in bbcodeTable.Rows )
			{
				if ( codeRow ["SearchRegEx"] != DBNull.Value && codeRow ["ReplaceRegEx"] != DBNull.Value )
				{
					string searchRegEx = codeRow ["SearchRegEx"].ToString();
					string replaceRegEx = codeRow ["ReplaceRegEx"].ToString();
					string rawVariables = codeRow ["Variables"].ToString();

					if ( !String.IsNullOrEmpty( rawVariables ) )
					{
						// handle variables...
						string [] variables = rawVariables.Split( new char [] { ';' } );

						VariableRegexReplaceRule rule = new VariableRegexReplaceRule( searchRegEx, replaceRegEx, _options, variables );
						rule.RuleRank = 50;
						rulesEngine.AddRule( rule );
					}
					else
					{
						// just standard replace...
						SimpleRegexReplaceRule rule = new SimpleRegexReplaceRule( searchRegEx, replaceRegEx, _options );
						rule.RuleRank = 50;
						rulesEngine.AddRule( rule );
					}
				}
			}
		}

		static public System.Data.DataTable GetCustomBBCode()
		{
			string cacheKey = YafCache.GetBoardCacheKey( Constants.Cache.CustomBBCode );
			System.Data.DataTable bbCodeTable = null;

			// check if there is value cached
			if ( YafCache.Current [cacheKey] == null )
			{
				// get the bbcode table from the db...
				bbCodeTable = YAF.Classes.Data.DB.bbcode_list( YafContext.Current.PageBoardID, null );
				// cache it indefinately (or until it gets updated)
				YafCache.Current [cacheKey] = bbCodeTable;
			}
			else
			{
				// retrieve bbcode Table from the cache
				bbCodeTable = ( System.Data.DataTable )YafCache.Current [cacheKey];
			}

			return bbCodeTable;
		}

		/// <summary>
		/// Helper function that dandles registering "custom bbcode" javascript (if there is any)
		/// for all the custom BBCode.
		/// </summary>
		static public void RegisterCustomBBCodePageElements( System.Web.UI.Page currentPage, Type currentType )
		{
			RegisterCustomBBCodePageElements( currentPage, currentType, null );
		}

		/// <summary>
		/// Helper function that dandles registering "custom bbcode" javascript (if there is any)
		/// for all the custom BBCode. Defining editorID make the system also show "editor js" (if any).
		/// </summary>
		static public void RegisterCustomBBCodePageElements( System.Web.UI.Page currentPage, Type currentType, string editorID )
		{
			DataTable bbCodeTable = BBCode.GetCustomBBCode();
			string scriptID = "custombbcode";
			System.Text.StringBuilder jsScriptBuilder = new System.Text.StringBuilder();
			System.Text.StringBuilder cssBuilder = new System.Text.StringBuilder();

			jsScriptBuilder.Append( "\r\n" );
			cssBuilder.Append( "\r\n" );

			foreach ( DataRow row in bbCodeTable.Rows )
			{
				string displayScript = null;
				string editScript = null;

				if ( row ["DisplayJS"] != DBNull.Value )
				{
					displayScript = row ["DisplayJS"].ToString().Trim();
				}

				if ( !String.IsNullOrEmpty( editorID ) && row ["EditJS"] != DBNull.Value )
				{
					editScript = row ["EditJS"].ToString().Trim();
					// replace any instances of editor ID in the javascript in case the ID is needed
					editScript = editScript.Replace( "{editorid}", editorID );
				}

				if ( !String.IsNullOrEmpty( displayScript ) || !String.IsNullOrEmpty( editScript ) )
				{
					jsScriptBuilder.AppendLine( displayScript + "\r\n" + editScript );
				}

				// see if there is any CSS associated with this BBCode
				if ( row ["DisplayCSS"] != DBNull.Value && !String.IsNullOrEmpty( row ["DisplayCSS"].ToString().Trim() ) )
				{
					// yes, add it into the builder
					cssBuilder.AppendLine( row ["DisplayCSS"].ToString().Trim() );
				}
			}

			if ( jsScriptBuilder.ToString().Trim().Length > 0 )
			{
				// register the javascript from all the custom bbcode...
				if ( !currentPage.ClientScript.IsClientScriptBlockRegistered( scriptID + "_script" ) )
				{
					currentPage.ClientScript.RegisterClientScriptBlock( currentType, scriptID + "_script", string.Format( @"<script language=""javascript"" type=""text/javascript"">{0}</script>", jsScriptBuilder.ToString() ) );
				}
			}

			if ( cssBuilder.ToString().Trim().Length > 0 )
			{
				// register the CSS from all custom bbcode...
				if ( !currentPage.ClientScript.IsClientScriptBlockRegistered( scriptID + "_css" ) )
				{
					currentPage.ClientScript.RegisterClientScriptBlock( currentType, scriptID + "_css", string.Format( @"<style type=""text/css"">{0}</style>", cssBuilder.ToString() ) );
				}
			}
		}

		/// <summary>
		/// Encodes HTML
		/// </summary>
		/// <param name="html"></param>
		/// <returns></returns>
		static public string EncodeHTML( string html )
		{
			return System.Web.HttpContext.Current.Server.HtmlEncode( html );
		}

		/// <summary>
		/// Decodes HTML
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		static public string DecodeHTML( string text )
		{
			return System.Web.HttpContext.Current.Server.HtmlDecode( text );
		}
	}
}
