﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using Harmony;
using SimpleJSON;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Extensions;

namespace XUnity.AutoTranslator.Plugin.Core.Web
{
   public class GoogleTranslateLegitimateEndpoint : KnownHttpEndpoint
   {
      private static readonly string HttpsServicePointTemplateUrl = "https://translation.googleapis.com/language/translate/v2?key={0}";

      private string _key;

      public GoogleTranslateLegitimateEndpoint( string key )
      {
         if( string.IsNullOrEmpty( key ) ) throw new ArgumentException( "The GoogleTranslateLegitimate endpoint requires an API key which has not been provided.", nameof( key ) );

         _key = key;

         // Configure service points / service point manager
         ServicePointManager.ServerCertificateValidationCallback += Security.AlwaysAllowByHosts( "translation.googleapis.com" );
         SetupServicePoints( "https://translation.googleapis.com" );
      }

      public override bool SupportsLineSplitting => true;

      public override void ApplyHeaders( WebHeaderCollection headers )
      {
      }

      public override bool TryExtractTranslated( string result, out string translated )
      {
         try
         {
            var obj = JSON.Parse( result );
            var lineBuilder = new StringBuilder( result.Length );

            foreach( JSONNode entry in obj.AsObject[ "data" ].AsObject[ "translations" ].AsArray )
            {
               var token = entry.AsObject[ "translatedText" ].ToString();
               token = token.Substring( 1, token.Length - 2 ).UnescapeJson();

               if( !lineBuilder.EndsWithWhitespaceOrNewline() ) lineBuilder.Append( "\n" );

               lineBuilder.Append( token );
            }

            translated = lineBuilder.ToString();

            var success = !string.IsNullOrEmpty( translated );
            return success;
         }
         catch
         {
            translated = null;
            return false;
         }
      }

      public override string GetServiceUrl( string untranslatedText, string from, string to )
      {
         return string.Format( HttpsServicePointTemplateUrl, WWW.EscapeURL( _key ) );
      }

      public override string GetRequestObject( string untranslatedText, string from, string to )
      {
         var b = new StringBuilder();
         b.Append( "{" );
         b.Append( "\"q\":\"" ).Append( untranslatedText.EscapeJson() ).Append( "\"," );
         b.Append( "\"target\":\"" ).Append( to ).Append( "\"," );
         b.Append( "\"source\":\"" ).Append( from ).Append( "\"," );
         b.Append( "\"format\":\"text\"" );
         b.Append( "}" );
         return b.ToString();
      }

      public override bool ShouldGetSecondChanceAfterFailure()
      {
         return false;
      }
   }
}
