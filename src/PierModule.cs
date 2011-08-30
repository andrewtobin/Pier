using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Transactions;
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
                                             var action = string.Empty;
                                             if (Request.Query.Action != null)
                                                 action = Request.Query.Action;
                                             else if (Request.Query.action != null)
                                                 action = Request.Query.action;

                                             if (!string.IsNullOrEmpty(action) && action.ToLower() == "shorturl")
                                             {
                                                 var shortend = Shorten(Uri.UnescapeDataString(Request.Query.url));
                                                 return shortend.ToXml();
                                             }

                                             return View["index"];
                                         };

            Post["/*"] = x => View["Shortened",Shorten(Request.Form.longurl)];

            Get["/*"] = x => View["index"];

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

        public Shortened Shorten(string longurl)
        {
            var existing = Db.ShortUrls.Where(u => u.LongUrl == longurl).FirstOrDefault();
            if (existing != null)
            {
                return existing;
            }
            var newShort = new Shortened()
            {
                Hits = 0,
                LongUrl = longurl,
                Date = DateTime.Now
            };

            using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted, Timeout = TransactionManager.DefaultTimeout }))
            {
                Db.ShortUrls.Add(newShort);
                Db.SaveChanges();

                newShort.ShortUrl = PierHelpers.Encode(newShort.Id);
                Db.SaveChanges();
                scope.Complete();
            }
            return newShort;
        }
    }
}