﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Script.Serialization;
using RestSharp;
using System.Collections.Specialized;
using Newtonsoft.Json;

namespace Maestrano.Api
{
    public static class MnoClient
    {
        public static string ApiBase { get; set; }
        public static string ApiHost { get; set; }
        public static string ApiId { get; set; }
        public static string ApiKey { get; private set; }

        public static RestClient _client;

        static MnoClient()
        {
            ApiHost = Maestrano.Api.Host;
            ApiBase = Maestrano.Api.Base;
            ApiKey = Maestrano.Api.Key;
            ApiId = Maestrano.Api.Id;

            // silverlight friendly way to get current version
            var assembly = Assembly.GetExecutingAssembly();
            AssemblyName assemblyName = new AssemblyName(assembly.FullName);
            var version = assemblyName.Version;

            _client = new RestClient();
            _client.UserAgent = "maestrano-dotnet/" + version;
            _client.Authenticator = new HttpBasicAuthenticator(ApiId, ApiKey);
            _client.BaseUrl = String.Format("{0}{1}", ApiHost, ApiBase);
        }

        /// <summary>
        /// Execute a manual REST request
        /// </summary>
        /// <typeparam name="T">The type of object to create and populate with the returned data.</typeparam>
        /// <param name="request">The RestRequest to execute (will use client credentials)</param>
        public static T ProjectSingleObject<T>(RestRequest request)
        {
            request.OnBeforeDeserialization = (resp) =>
            {
                // for individual resources when there's an error to make
                // sure that RestException props are populated
                if (((int)resp.StatusCode) >= 400)
                    request.RootElement = "";
            };

            var response = _client.Execute(request);
            var respObj = JsonConvert.DeserializeObject<MnoObject<T>>(response.Content);

            return respObj.Data;
        }

        /// <summary>
        /// Execute a manual REST request
        /// </summary>
        /// <typeparam name="T">The type of object to create and populate with the returned data.</typeparam>
        /// <param name="request">The RestRequest to execute (will use client credentials)</param>
        public static List<T> ProcessList<T>(RestRequest request)
        {
            request.OnBeforeDeserialization = (resp) =>
            {
                // for individual resources when there's an error to make
                // sure that RestException props are populated
                //if (((int)resp.StatusCode) >= 400)
                //    request.RootElement = "";
            };

            var response = _client.Execute(request);
            Console.WriteLine(response.Content);
            var respObj = JsonConvert.DeserializeObject<MnoObject<List<T>>>(response.Content);

            return respObj.Data;
        }

        public static List<T> All<T>(string path, NameValueCollection filters = null)
        {
            var request = new RestRequest();
            request.Resource = path;
            request.Method = Method.GET;

            // Add query parameters
            if (filters != null)
                foreach (String k in filters.AllKeys)
                    request.AddParameter(k, filters[k]);

            return MnoClient.ProcessList<T>(request);
        }

        public static T Retrieve<T>(string path, string resourceId)
        {
            var request = new RestRequest();
            request.Resource = path;
            request.Method = Method.GET;
            request.AddUrlSegment("id", resourceId);
            Console.WriteLine(resourceId);

            return MnoClient.ProjectSingleObject<T>(request);
        }


    }
}
