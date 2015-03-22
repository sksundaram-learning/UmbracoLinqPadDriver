﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Umbraco.Core;
using Umbraco.Core.Models;
using UmbracoLinqPad.Models;

namespace UmbracoLinqPad.Gateway.Models
{
    /// <summary>
    /// The enumerable collection that Linq pad will display results for
    /// </summary>
    public class ContentCollection<T> : IEnumerable<T>
        where T : IGeneratedContentBase
    {
        private readonly Assembly _generatedAssembly;
        private readonly string _contentTypeAlias;
        private readonly IContentType _contentType;

        public ContentCollection(UmbracoDataContextBase dataContext, string contentTypeAlias)
        {
            //NOTE: This is strange i know but linqpad subclasses our data context in it's own assembly, we need
            // a ref to our own generated assembly to get th types from that
            _generatedAssembly = dataContext.GetType().BaseType.Assembly;

            _contentTypeAlias = contentTypeAlias;
            _contentType = ApplicationContext.Current.Services.ContentTypeService.GetContentType(contentTypeAlias);
            if (_contentType == null) throw new ArgumentException("No content type found with alias " + contentTypeAlias);
        }

        public IEnumerator<T> GetEnumerator()
        {
            var content = ApplicationContext.Current.Services.ContentService.GetContentOfContentType(_contentType.Id);

            //convert to the generated type (needs to be from the generated assembly)
            var genType = _generatedAssembly.GetType("Umbraco.Generated." + _contentTypeAlias);
            if (genType == null)
                throw new InvalidOperationException("No generated type found: " + "Umbraco.Generated." + _contentTypeAlias +
                                                    " data context assembly: " + _generatedAssembly);
            return content.Select(x =>
            {
                var instance = (T) Activator.CreateInstance(genType);
                instance.ContentTypeAlias = x.ContentType.Alias;
                instance.Name = x.Name;
                return instance;
            }).GetEnumerator();
        }

      
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}