﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;

namespace Medidata.ZipkinTracer.Core
{
    public class ZipkinConfig : IZipkinConfig
    {
        public Predicate<IOwinRequest> Bypass { get; set; } = r => false;
        public Uri ZipkinBaseUri { get; set; }
        public Func<IOwinRequest, Uri> Domain { get; set; }
        public uint SpanProcessorBatchSize { get; set; }
        public IList<string> ExcludedPathList { get; set; } = new List<string>();
        public double SampleRate { get; set; }
        public IList<string> NotToBeDisplayedDomainList { get; set; } = new List<string>();

        public void Validate()
        {
            if (ZipkinBaseUri == null)
            {
                throw new ArgumentNullException("ZipkinBaseUri");
            }

            if (Domain == null)
            {
                Domain = request => new Uri(request.Uri.Host);
            }

            if (ExcludedPathList == null)
            {
                throw new ArgumentNullException("ExcludedPathList");
            }

            if (ExcludedPathList.Any(item => !item.StartsWith("/")))
            {
                throw new ArgumentException("Item of ExcludedPathList must start with '/'. e.g.) '/check_uri'");
            }

            if (SampleRate < 0 || SampleRate > 1)
            {
                throw new ArgumentException("SampleRate must range from 0 to 1.");
            }

            if (NotToBeDisplayedDomainList == null)
            {
                throw new ArgumentNullException("NotToBeDisplayedDomainList");
            }
        }

        public bool ShouldBeSampled(IOwinContext context, string sampled)
        {
            if (context == null)
            {
                return false;
            }

            bool result;
            if (!string.IsNullOrWhiteSpace(sampled) && Boolean.TryParse(sampled, out result))
            {
                return result;
            }

            if (!IsInDontSampleList(context.Request.Path.ToString()))
            {
                var random = new Random();
                if (random.NextDouble() <= SampleRate)
                {
                    return true;
                }
            }
            return false;
        }

        internal bool IsInDontSampleList(string path)
        {
            if (path != null)
            {
                if (ExcludedPathList.Any(uri => path.StartsWith(uri, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return true;
                }
            }
            return false;
        }
    }
}