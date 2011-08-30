using System;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Transactions;
using System.Xml.Linq;
using Devtalk.EF.CodeFirst;
using Nancy;

namespace Pier
{
    public class PierModule : NancyModule
    {
        public PierContext Db;

        public PierModule()
        {

            Database.SetInitializer(new DontDropDbJustCreateTablesIfModelChanged<PierContext>());
            
            Db = new PierContext();

            Get["/yourls-api.php"] = x =>
                                         {
                                             try
                                             {
                                                 return Shorten(x);
                                             }
                                             catch (Exception ex)
                                             {
                                                 return ex.Message;
                                             }
                                         };

            Get["/*"] = x => "<img src=\"pier.png\" /><br />Welcome to Pier";

            Get["/{shortUrl}"] = x =>
                                     {
                                         //Expressions may not contain dynamics
                                         var shortUrl = string.Empty;
                                         shortUrl = x.shortUrl;

                                         var url = Db.ShortUrls.Where(u => u.ShortUrl == shortUrl).FirstOrDefault();
                                         if (url != null)
                                         {
                                             url.Hits++;
                                             Db.SaveChanges();

                                             return Response.AsRedirect(url.LongUrl);
                                         }

                                         return string.Concat("Hello ", x.shortUrl);
                                     };

        }

        private string _shortUrlCharacterSet;
        public string ShortUrlCharacterSet
        {
            get
            {
                if (_shortUrlCharacterSet == null)
                {
                    switch (ConfigurationManager.AppSettings["baseHash"])
                    {
                        case "36":
                            _shortUrlCharacterSet = "0123456789abcdefghijklmnopqrstuvwxyz";
                            break;
                        case "62":
                        case "64":
                            _shortUrlCharacterSet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
                            break;

                    }
                }

                return _shortUrlCharacterSet;
            }
        }

        public string Encode(int id)
        {
            string result = string.Empty;
            var length = ShortUrlCharacterSet.Length;

            while (id >= length)
            {
                var mod = id % length;
                id = id / length;
                result = ShortUrlCharacterSet[mod] + result;
            }

            result = ShortUrlCharacterSet[id] + result;
            return result;
        }

        public Response Shorten(dynamic req)
        {
            var action = string.Empty;
            if (Request.Query.Action != null)
                action = Request.Query.Action;
            else if (Request.Query.action != null)
                action = Request.Query.action;

            if (!string.IsNullOrEmpty(action))
            {
                switch (action.ToLower())
                {
                    case "shorturl":

                        //Expressions may not contain dynamics
                        var url = string.Empty;
                        url = Uri.UnescapeDataString(Request.Query.url);

                        var existing = Db.ShortUrls.Where(u => u.LongUrl == url).FirstOrDefault();
                        if (existing != null)
                        {
                            return new Response
                                    {

                                        ContentType = "application/xml",
                                        Contents = Result(url, existing.ShortUrl, existing.Id, existing.Date)
                                    };
                        }
                        var newShort = new Shortened()
                                               {
                                                   Hits = 0,
                                                   LongUrl = url,
                                                   Date = DateTime.Now
                                               };

                        using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted, Timeout = TransactionManager.DefaultTimeout }))
                        {
                            Db.ShortUrls.Add(newShort);
                            Db.SaveChanges();

                            newShort.ShortUrl = Encode(newShort.Id);
                            Db.SaveChanges();
                            scope.Complete();
                        }
                        var r = new Response
                                    {
                                        ContentType = "application/xml",
                                        Contents = Result(url, newShort.ShortUrl, newShort.Id, newShort.Date, Request.UserHostAddress)
                                    };
                        return r;
                        break;

                    case "expand":
                        break;
                }
            }

            return "";
        }

        public Action<Stream> Result(string url, string shorturl, int id, DateTime time, string ip = null)
        {
            return stream =>
                       {
                           var x = new XElement("result",
                                                new XElement("url",
                                                             new XElement("id", id),
                                                             new XElement("keyword", shorturl),
                                                             new XElement("url", url),
                                                             new XElement("date", time),
                                                             new XElement("ip", ip)
                                                    ),
                                                new XElement("status", "success"),
                                                new XElement("message", string.Format("{0} added to database", url)),
                                                new XElement("shorturl", /*should add in base url*/ shorturl)
                               );
                           x.Save(stream);
                       };
        }
    }

}