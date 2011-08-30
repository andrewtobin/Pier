using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nancy;
using Nancy.Responses;

namespace Pier
{
    //https://github.com/ToJans/NerdBeers/blob/master/src/Org.NerdBeers/Org.NerdBeers.Web/Bootstrapper.cs
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void InitialiseInternal(TinyIoC.TinyIoCContainer container)
        {
            base.InitialiseInternal(container);

            BeforeRequest += ctx =>
                                 {
                                     var rootPathProvider = container.Resolve<IRootPathProvider>();

                                     var staticFileExtensions =
                                         new Dictionary<string, string>
                                             {
                                                 {"jpg", "image/jpeg"},
                                                 {"png", "image/png"},
                                                 {"gif", "image/gif"},
                                                 {"css", "text/css"},
                                                 {"js", "text/javascript"}
                                             };

                                     var requestedExtension = Path.GetExtension(ctx.Request.Url.Path);
                                     if (!string.IsNullOrEmpty(requestedExtension))
                                     {
                                         var extensionWithoutDot = requestedExtension.Substring(1);

                                         if (staticFileExtensions.Keys.Any(x => x.Equals(extensionWithoutDot, StringComparison.InvariantCultureIgnoreCase)))
                                         {
                                             var fileName = Path.GetFileName(ctx.Request.Url.Path);

                                             if (fileName == null)
                                                 return null;

                                             var filePath = Path.Combine(rootPathProvider.GetRootPath(), fileName);


                                             return !File.Exists(filePath) ? null : new GenericFileResponse(filePath, staticFileExtensions[extensionWithoutDot]);
                                         }
                                     }

                                     return null;
                                 };
        }
    }
}